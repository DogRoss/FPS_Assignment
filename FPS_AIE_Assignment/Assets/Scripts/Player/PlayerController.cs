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
    public Transform hipfirePos;
    public Transform aimingPos;

    //public float gunControlForce = 1f; //amount of force used to control the gun
    //public float aimCoefficient = 2f;
    public float aimingPositionalForce = 2f;
    public float aimingPointingForce = 2f;
    public float gunPositionalForce = 5f;
    public float gunPointingForce = 1f;
    public float gunTargetMaxDist = 1f;
    public float horizontalMaxDist = 2f;
    public float verticalMaxDist = 2f;
    

    [Header("Gun & Wall Collision")]
    public LayerMask gunCollisionMask;
    public float gunWallCheckDist = 2f;

    private bool aimDownSights = false;

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

        if (!aimDownSights)
            AimGunToHip();
        else
            AimGunToADS();
    }
    public override void FixedUpdate()
    {
        base.FixedUpdate();
        hit = false;
        ray.origin = cam.transform.position; ray.direction = cam.transform.forward;
        hit = Physics.Raycast(ray, out hitData, gunWallCheckDist, gunCollisionMask.value);

        if (!aimDownSights)
            MoveGunToHip();
        else
            MoveGunToADS();
        
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
    private void OnRMouseDown(InputValue value)
    {
        if(value.Get<float>() > 0)
            aimDownSights = true;
        else
            aimDownSights = false;

        print("call RMB");
    }
    private void OnSwitchFireMode()
    {
        if (gun)
            gun.ToggleAuto();
    }
    private void MoveGunToADS()
    {
        //CALLED IN FIXED

        //store distance
        float distance = Vector3.Distance(aimingPos.position, gun.transform.position + gun.weaponData.aimPositionalOffset);
        //store direction
        Vector3 dir = (aimingPos.position - (gun.transform.position + gun.weaponData.aimPositionalOffset)).normalized;
        //find current aiming power ()
        float curPower = distance / gunTargetMaxDist;
        //apply to gun position
        gun.transform.position += dir * (aimingPositionalForce * curPower) * Time.fixedDeltaTime;

        //clamp gun
        Vector3 pos = gun.transform.position;
        pos.y = Mathf.Clamp(pos.y, aimingPos.transform.position.y - verticalMaxDist, aimingPos.transform.position.y + verticalMaxDist);
        pos.x = Mathf.Clamp(pos.x, aimingPos.transform.position.x - horizontalMaxDist, aimingPos.transform.position.x + horizontalMaxDist);
        pos.z = Mathf.Clamp(pos.z, aimingPos.transform.position.z - horizontalMaxDist, aimingPos.transform.position.z + horizontalMaxDist);
        gun.transform.position = pos;
    }
    private void MoveGunToHip()
    {
        //CALLED IN FIXED

        //store distance
        float distance = Vector3.Distance(hipfirePos.position, gun.transform.position + gun.weaponData.hipPositionalOffset);
        //store direction
        Vector3 dir = (hipfirePos.position - (gun.transform.position + gun.weaponData.hipPositionalOffset)).normalized;
        //find current aiming power ()
        float curPower = distance / gunTargetMaxDist;
        //apply forces to gun
        gun.transform.position += dir * (gunPositionalForce * curPower) * Time.fixedDeltaTime;

        //clamp gun within certain bounds
        Vector3 pos = Vector3.zero;
        pos = gun.transform.position;
        pos.y = Mathf.Clamp(pos.y, hipfirePos.transform.position.y - verticalMaxDist, hipfirePos.transform.position.y + verticalMaxDist);
        pos.x = Mathf.Clamp(pos.x, hipfirePos.transform.position.x - horizontalMaxDist, hipfirePos.transform.position.x + horizontalMaxDist);
        pos.z = Mathf.Clamp(pos.z, hipfirePos.transform.position.z - horizontalMaxDist, hipfirePos.transform.position.z + horizontalMaxDist);
        gun.transform.position = pos;
    }
    private void AimGunToADS()
    {
        //CALLED IN NORMAL

        /* find dot product to see how close to direction gun is facing
         * add to dot with rotational force * deltaTime
         */
        float dot = Vector3.Dot(gun.transform.forward, ray.direction);
        float afterProd = aimingPointingForce * Time.deltaTime / dot;

        gun.transform.forward = Vector3.RotateTowards(gun.transform.forward, ray.direction, .1f * Time.deltaTime, Mathf.Infinity);
    }
    private void AimGunToHip()
    {
        //CALLED IN NORMAL

        /*
         * find dot product to see how close to direction gun is facing
         * add to dot with rotational force * deltaTime
         */
        float dot = Vector3.Dot(gun.transform.forward, ray.direction);
        float afterProd = gunPointingForce * Time.deltaTime / dot;
        //dot = Mathf.Clamp01(dot);
        //afterProd -= dot;
        gun.transform.forward = Vector3.RotateTowards(gun.transform.forward, ray.direction, .1f * Time.deltaTime, Mathf.Infinity);
    }
    private void EquipWeapon(int slot)
    {
        //TODO: create functionality, later planned
    }
    private void ApplyPlayerRecoil(float recoilX, float recoilY, float recoilZ)
    {
        //apply recoil here
    }
}
