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
    [Header("Gun Variables")]
    public RangedWeapon gun;
    public Transform hipfirePos;
    public Transform adsPos;

    //aiming variables
    [Header("Aiming/Recoil Variables")]
    public float recoilRecoveryForce;
    public float gunTolerance = 10f;
    public float maxGunDistance = .25f;
    public float maxGunAngle = 10f;
    public float gunTurnForce = 10f;

    private Vector3 currentRecoil = Vector3.zero;
    private Vector3 currentRotation = Vector3.zero;

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

        CalculateWeaponPosition();
        RecoverRecoil();
    }

    //--------------------------------------------------------------------------------------------------------------------------
    // Input Functions
    //--------------------------------------------------------------------------------------------------------------------------
    private void OnLMouseDown(InputValue value)
    {
        if (value.Get<float>() > 0)
            gun.ToggleShot(true);
        if (value.Get<float>() <= 0)
            gun.ToggleShot(false);
    }
    private void OnRMouseDown(InputValue value)
    {
        if (value.Get<float>() > 0)
            print("call aim");
    }
    private void OnSwitchFireMode()
    {
        //if (gun)
        //    gun.ToggleAuto();

        if (gun != null)
        {
            gun.playerRecoilEvent.AddListener(AddRecoil);
            print("listener amount: " + gun.playerRecoilEvent);
        }
    }

    //--------------------------------------------------------------------------------------------------------------------------
    // User Functions
    //--------------------------------------------------------------------------------------------------------------------------
    private void AddRecoil(float x, float y, float z)
    {
        print("bro what");

        currentRecoil.x += x;
        currentRecoil.y += y;
        currentRecoil.z += z;
    }
    private void RecoverRecoil()
    {
        float xMulti = 0, yMulti = 0, zMulti = 0;

        if(Mathf.Abs(currentRecoil.x) > 0)
            xMulti = Mathf.Clamp(currentRecoil.x / recoilRecoveryForce, -1, 1);
        if (Mathf.Abs(currentRecoil.y) > 0)
            yMulti = Mathf.Clamp(currentRecoil.y / recoilRecoveryForce, -1, 1);
        if (Mathf.Abs(currentRecoil.z) > 0)
            zMulti = Mathf.Clamp(currentRecoil.z / recoilRecoveryForce, -1, 1);

        currentRecoil.x -= xMulti * recoilRecoveryForce;
        currentRecoil.y -= yMulti * recoilRecoveryForce;
        currentRecoil.z -= zMulti * recoilRecoveryForce;
    }
    private void CalculateWeaponPosition()
    {
        //take current movement speed, and store opposite of velocity direction
        Vector3 velDirection = transform.InverseTransformDirection(-controller.velocity.normalized);
        //amount of lagbehind the gun will experience
        float amount = Mathf.Clamp(controller.velocity.magnitude / gunTolerance, 0, maxGunDistance);

        gun.transform.localPosition = hipfirePos.localPosition + (velDirection * amount) + currentRecoil;

        currentRotation.x += Mathf.Clamp(mouseVector.x * Time.deltaTime * gunTurnForce, -maxGunAngle, maxGunAngle);
        currentRotation.y += Mathf.Clamp(mouseVector.y * Time.deltaTime * gunTurnForce, -maxGunAngle, maxGunAngle);

        currentRotation.x -= currentRotation.x * Time.deltaTime * gunTurnForce;
        currentRotation.y -= currentRotation.y * Time.deltaTime * gunTurnForce;

        gun.transform.localRotation = Quaternion.Euler(currentRotation);
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
