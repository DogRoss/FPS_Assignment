using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RangedWeapon : MonoBehaviour
{
    public Transform muzzle;

    [Header("Gun Data")]
    public RangedWeaponData weaponData;

    [Header("Line Renderer")]
    public bool canShoot = true;
    private bool cActive = false; //coroutineActive
    private bool auto = true;
    private bool lmbHeld = false;
    public float storedClickTime = 0f;
    RaycastHit hit;

    private void Start()
    {
        if (weaponData.canAuto)
            auto = true;
        else
            auto = false;
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
            storedClickTime += Time.deltaTime;
        }

    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawSphere(muzzle.position, .05f);
        Gizmos.DrawSphere(hit.point, .05f);
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
            print("made it");
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

        print("SHOT DA BULLE");
        bool hitSuccess = Physics.Raycast(muzzle.position, muzzle.forward, out hit, weaponData.bulletTravelUPS);

        LineRenderer rend = Instantiate(weaponData.trail);
        rend.SetPosition(0, transform.position);
        if (hitSuccess)
        {
            rend.SetPosition(1, hit.point);
        }
        else
        {
            rend.SetPosition(1, muzzle.position + (muzzle.forward * weaponData.bulletTravelUPS));
        }

        canShoot = false;

        StartCoroutine(FadeTrail(rend));
    }

    private IEnumerator AutoShoot()
    {
        cActive = true;

        while (lmbHeld && auto)
        {
            bool hitSuccess = Physics.Raycast(muzzle.position, muzzle.forward, out hit, weaponData.bulletTravelUPS);

            LineRenderer rend = Instantiate(weaponData.trail);
            rend.SetPosition(0, transform.position);
            if (hitSuccess)
            {
                rend.SetPosition(1, hit.point);
            }
            else
            {
                rend.SetPosition(1, muzzle.position + (muzzle.forward * weaponData.bulletTravelUPS));
            }

            StartCoroutine(FadeTrail(rend));

            yield return new WaitForSeconds(weaponData.TimeBetweenBullets);
        }

        canShoot = false;
        cActive = false;
    }

    private IEnumerator FadeTrail(LineRenderer rend)
    {
        float currentTime = weaponData.fadeTime;
        float percent = 0;
        while(currentTime > 0)
        {
            currentTime -= Time.deltaTime;
            percent = currentTime / weaponData.fadeTime;
            rend.startWidth = percent; rend.endWidth = percent;

            yield return null;
        }

        Destroy(rend.gameObject);
    }
}
