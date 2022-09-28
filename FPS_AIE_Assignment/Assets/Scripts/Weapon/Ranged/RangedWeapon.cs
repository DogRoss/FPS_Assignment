using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Handles weapon interactions that include a projectile being fired from a object.
/// Stats are based off Scriptable Object data (RangedWeaponData).
/// </summary>
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
    /// <summary>
    /// Toggles ranged weapon firing.
    /// Values used in case automatic firing is enabled.
    /// </summary>
    /// <param name="firing"></param>
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
    /// <summary>
    /// Handles interaction that happens when ranged weapon is fired.
    /// </summary>
    private void Shoot()
    {

        bool hitSuccess = Physics.Raycast(muzzle.position, muzzle.forward, out hit, weaponData.bulletVelocity);

        LineRenderer rend = Instantiate(weaponData.trail);
        rend.SetPosition(0, muzzle.position);
        if (hitSuccess)
        {
            rend.SetPosition(1, hit.point);
            print(weaponData.BulletForce);
            hit.transform.GetComponent<IDamageable>()?.TakeDamage(weaponData.damagePerBullet);
            hit.transform.TryGetComponent<Rigidbody>(out Rigidbody rigid);
            rigid?.AddForceAtPosition(muzzle.forward * weaponData.BulletForce, hit.point, ForceMode.Impulse);
        }
        else
        {
            rend.SetPosition(1, muzzle.position + (muzzle.forward * weaponData.bulletVelocity));
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
    /// <summary>
    /// Handles firing interaction when automatic firing is enabled.
    /// </summary>
    /// <returns></returns>
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
    /// <summary>
    /// fades bullet trail at fixed rate.
    /// </summary>
    /// <param name="rend"></param>
    /// <returns></returns>
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
