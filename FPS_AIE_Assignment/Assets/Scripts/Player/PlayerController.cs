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
    public float gunPositionalForce = 5f;
    public float gunPointingForce = 1f;
    public float gunTargetMaxDist = 2f;
    public float gunMaxDistY = 2f;

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
        //store distance
        float distance = Vector3.Distance(gunHolder.position, gun.transform.position);
        //store direction
        Vector3 dir = (gunHolder.position - gun.transform.position).normalized;
        //find current aiming power ()
        float curPower = distance / gunTargetMaxDist;
        //apply to gun position
        Vector3 pos = gun.transform.position + dir * (gunPositionalForce * curPower) * Time.deltaTime;
        pos.y = Mathf.Clamp(pos.y, gunHolder.transform.position.y - gunMaxDistY, gunHolder.transform.position.y + gunMaxDistY);
        gun.transform.position = pos;
        /*
         * find dot product to see how close to direction gun is facing
         * add to dot with rotational force * deltaTime
         */
        float dot = Vector3.Dot(gun.transform.forward, ray.direction);
        float afterProd = gunPointingForce * Time.deltaTime / dot;
        //dot = Mathf.Clamp01(dot);
        //afterProd -= dot;
        gun.transform.forward = Vector3.RotateTowards(gun.transform.forward, ray.direction, afterProd, Mathf.Infinity);
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
