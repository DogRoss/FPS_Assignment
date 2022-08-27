using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

/*
 * inherits movement for movement variables
 * handles player interactions like weapons and worldspace interactions
 */

public class PlayerController : Movement
{
    [Header("Spacer")]
    public RangedWeapon gun;
    public Transform gunHolder;

    public float gunControlForce = 1f; //amount of force used to control the gun
    public float aimCoefficient = 2f;

    public float gunWallDist = 0f;
    public float maxGunDisplacement;

    public LayerMask gunCollisionMask;



    public float slideForce;
    public float length;
    public float progress;

    //camera rays
    Ray ray;
    Ray gRay;
    RaycastHit hitData;
    RaycastHit gHitData;
    bool hit = false;
    bool gHit = false;

    public override void Start()
    {
        base.Start();
    }
    public override void Update()
    {
        base.Update();

        gun.transform.forward = ray.direction;

        //if (hit)
        //{
        //    Slide();

        //    //find direction to hit point
        //    Vector3 dir = (hitData.point - gun.transform.position).normalized;
        //    gun.transform.forward += dir * Time.deltaTime * 40;
        //}
        //else

    }
    public override void FixedUpdate()
    {
        base.FixedUpdate();

        //store distance from gun muzzle
        gRay.origin = gun.muzzle.position; gRay.direction = gun.muzzle.forward;
        gHit = Physics.SphereCast(gRay, .1f, out gHitData, .1f, gunCollisionMask.value);
        if(gHit)
        {
            print("hit: " + gHitData.collider.name);
            gunWallDist = gHitData.distance;
        }
        else
        {
            gunWallDist = 999f;
        }

        Slide();

        //raycast forward from player
        hit = false;
        ray.origin = cam.transform.position; ray.direction = cam.transform.forward;
        hit = Physics.Raycast(ray, out hitData, gunCollisionMask.value);
    }

    private void OnLMouseDown(InputValue value)
    {
        print("call");

        if (value.Get<float>() > 0)
            gun.ToggleShot(true);
            print("held");
        if (value.Get<float>() <= 0)
            gun.ToggleShot(false);
            print("release");
    }
    private void OnSwitchFireMode()
    {
        if (gun)
            gun.ToggleAuto();
    }

    private void Slide()
    {
        if(gunWallDist < 0.1f)
        {
            Debug.Log("account for wall");

            progress += Time.deltaTime;
        }
        else
        {
            progress -= Time.deltaTime;
        }
        progress = Mathf.Clamp01(progress);

 

        gun.transform.position = gunHolder.position + (-transform.forward * (length * progress));
    }
}
