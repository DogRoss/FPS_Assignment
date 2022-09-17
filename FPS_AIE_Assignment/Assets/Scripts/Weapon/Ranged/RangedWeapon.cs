using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Rigidbody))]
public class RangedWeapon : MonoBehaviour
{
    public Transform muzzle;
    public Collider[] colliders;
    public Rigidbody rb;

    [Header("Gun Data")]
    public RangedWeaponData weaponData;

    [Header("Line Renderer")]
    private bool canShoot = true;
    private bool cActive = false; //coroutineActive
    private bool auto = true;
    private bool lmbHeld = false;
    private float storedClickTime = 0f;
    RaycastHit hit;

    //Grab points and hand pose info here
    public Transform rhtHandPos;
    public Transform lftHandPos;

    public delegate void RecoilEvent(float force);
    public RecoilEvent recoilEvent;

    private void Start()
    {
        if (weaponData.canAuto)
            auto = true;
        else
            auto = false;

        rb = GetComponent<Rigidbody>();
        colliders = GetComponentsInChildren<Collider>();
        
        //TODO:Replace below functions with interface equip function
        rb.isKinematic = true;
        foreach(Collider collider in colliders)
        {
            collider.enabled = false;
        }

        //playerRecoilEvent.RemoveAllListeners();
    }
    private void FixedUpdate()
    {
        if (!canShoot)
        {
            if (storedClickTime > weaponData.TimeBetweenBullets)
            {
                canShoot = true;
                storedClickTime = 0f;
            }
            storedClickTime += Time.fixedDeltaTime;
        }

        //Ignore collision stuff here

    }
    public void ToggleAuto()
    {
        if (weaponData.canAuto)
            auto = !auto;
        else
            auto = false;

        print("auto toggle: " + auto);
    }
    public void ToggleShot(bool firing)
    {
        if (firing && canShoot)
        {
            lmbHeld = true;
            if (!auto)
                Shoot();
            else if(!cActive)
                StartCoroutine(AutoShoot());
        }
        else
            lmbHeld = false;
    }

    private void Shoot()
    {

        bool hitSuccess = Physics.Raycast(muzzle.position, muzzle.forward, out hit, weaponData.bulletTravelUPS);

        LineRenderer rend = Instantiate(weaponData.trail);
        rend.SetPosition(0, muzzle.position);
        if (hitSuccess)
        {
            rend.SetPosition(1, hit.point);
        }
        else
        {
            rend.SetPosition(1, muzzle.position + (muzzle.forward * weaponData.bulletTravelUPS));
        }

        if(!auto)
            canShoot = false;

        recoilEvent.Invoke(weaponData.bulletRecoilForce);
        StartCoroutine(FadeTrail(rend));
    }

    /// <summary>
    /// sets parent to null and enables physics
    /// makes equipped local space object world space object
    /// </summary>
    [ContextMenu("Free Gun")]
    public void Free()
    {
        transform.parent = null;
        rb.isKinematic = false;
        foreach(Collider col in colliders)
        {
            col.enabled = true;
        }
    }

    //Enumerators
    private IEnumerator AutoShoot()
    {
        cActive = true;

        while (lmbHeld && auto)
        {
            Shoot();
            yield return new WaitForSeconds(weaponData.TimeBetweenBullets);
        }

        canShoot = false;
        cActive = false;
    }

    private IEnumerator FadeTrail(LineRenderer rend)
    {
        float currentTime = weaponData.fadeTime;
        float width = .1f;
        float percent = 0;
        while(currentTime > 0)
        {
            currentTime -= Time.deltaTime;
            percent = currentTime / weaponData.fadeTime;
            rend.startWidth = width * percent; rend.endWidth = width * percent;

            yield return null;
        }

        Destroy(rend.gameObject);
    }
}
