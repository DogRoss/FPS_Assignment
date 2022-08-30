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

    //public float gunControlForce = 1f; //amount of force used to control the gun
    //public float aimCoefficient = 2f;
    public Vector3 aimingForces;
    public float aimingForce;
    public float maxGunDist = 2f;

    [Header("Gun & Wall Collision")]
    public LayerMask gunCollisionMask;
    public float gunWallCheckDist = 2f;

    //camera rays
    Ray ray;
    RaycastHit hitData;
    bool hit = false;

    public override void Start()
    {
        base.Start();
    }
    public override void Update()
    {
        base.Update();

        
    }
    public override void FixedUpdate()
    {
        base.FixedUpdate();
        hit = false;
        ray.origin = cam.transform.position; ray.direction = cam.transform.forward;
        hit = Physics.Raycast(ray, out hitData, gunWallCheckDist, gunCollisionMask.value);

        MoveGunToHold();
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
    private void MoveGunToHold()
    {
        ////check distance from wall, and compare to wallcheckdistance
        //if (!hit)
        //    gun.transform.position = gunHolder.position;
        //else
        //{
        //    float difference = gunWallCheckDist - Vector3.Distance(hitData.point, ray.origin);
        //    gun.transform.position = gunHolder.position + (-transform.forward * difference);
        //}
        //gun.transform.forward = ray.direction;

        //store distance
        float distance = Vector3.Distance(gunHolder.position, gun.transform.position);
        //store direction
        Vector3 dir = (gunHolder.position - gun.transform.position).normalized;
        aimingForces = dir * aimingForce; //apply force towards object

        //if more than max distance, set within max distance
        if(distance > maxGunDist)
        {
            gun.transform.position = gunHolder.position + (-dir * maxGunDist);
        }

        //the closer we get, the smoother the gun movement to position it should be
        //float percentage = 

        gun.transform.position += aimingForces * Time.deltaTime;

        print("end of function");

        //find direction towards hold point
    }
    private void EquipWeapon(int slot)
    {
        //TODO: create functionality, later planned
    }
    private void ApplyRecoil(float recoilX, float recoilY, float recoilZ)
    {
        //apply recoil here
    }
}
