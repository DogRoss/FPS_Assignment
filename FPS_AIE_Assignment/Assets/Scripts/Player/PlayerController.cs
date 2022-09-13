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
    public float tempRecoil = 10f;

    //aiming variables
    [Header("Recoil Position Variables")]
    //Position
    public float maxGunDistance = .25f;
    public float gunTolerance = 10f;
    public float recoilReturnPositionSpeed = 10f;
    public float recoilReturnPositionSnappiness = 10f;

    private Vector3 targetRecoilPosition = Vector3.zero;
    private Vector3 currentRecoilPosition = Vector3.zero;
    private Vector3 recoilVelocity = Vector3.zero;

    [Header("Recoil Rotation Variables")]
    //Rotation
    public float maxGunAngle = 10f;
    public float recoilReturnRotationSpeed = 5f;
    public float recoilReturnRotationSnappiness = 10f;

    private Vector3 currentRecoilRotation = Vector3.zero;
    private Vector3 targetRecoilRotation = Vector3.zero;

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

        gun.recoilEvent += AddRecoil;
        gun.recoilEvent.Invoke(0, 0, 0);
    }
    public override void Update()
    {
        base.Update();

        CalculateWeaponPosition();
        ApplyRecoil();
    }
    public override void FixedUpdate()
    {
        base.FixedUpdate();

        hit = false;
        ray.origin = cam.transform.position; ray.direction = cam.transform.forward;
        hit = Physics.Raycast(ray, out hitData, gunWallCheckDist, gunCollisionMask.value);
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
        if (gun)
            gun.ToggleAuto();
    }

    //--------------------------------------------------------------------------------------------------------------------------
    // User Functions
    //--------------------------------------------------------------------------------------------------------------------------
    private void ApplyRecoil()
    {
        currentRecoilPosition += recoilVelocity * Time.fixedDeltaTime;
        currentRecoilPosition = Vector3.ClampMagnitude(currentRecoilPosition, maxGunDistance);
        recoilVelocity = Vector3.Lerp(recoilVelocity, Vector3.zero, recoilReturnPositionSpeed * Time.fixedDeltaTime);
    }
    private void AddRecoil(float x, float y, float z)
    {
        //recoilVelocity.x += x;
        //recoilVelocity.y += y;
        //recoilVelocity.z += z;

        //temp recoil method
        //targetRotation += new Vector3(tempRecoil, Random.Range(-tempRecoil, tempRecoil), Random.Range(-tempRecoil, tempRecoil));
        targetRecoilRotation += new Vector3(-tempRecoil, Random.Range(-tempRecoil, tempRecoil), 0);

    }
    private void CalculateWeaponPosition()
    {
        //take current movement speed, and store opposite of velocity direction
        Vector3 velDirection = transform.InverseTransformDirection(-controller.velocity.normalized);
        //amount of lagbehind the gun will experience
        float amount = Mathf.Clamp(controller.velocity.magnitude / gunTolerance, 0, maxGunDistance);

        //recover recoil position
        //currentRecoilPosition = Vector3.Lerp(currentRecoilPosition, Vector3.zero, recoilReturnSpeed * Time.fixedDeltaTime);
        //targetRecoilPosition = Vector3.Lerp(targetRecoilPosition, Vector3.zero, recoilReturnPositionSpeed * Time.fixedDeltaTime);
        //currentRecoilPosition = Vector3.Lerp(currentRecoilPosition, targetRecoilPosition, recoilReturnPositionSnappiness * Time.deltaTime);
        //
        //gun.transform.localPosition = hipfirePos.localPosition + (velDirection * amount) + currentRecoilPosition;


        targetRecoilPosition = Vector3.Lerp(targetRecoilPosition, Vector3.zero, recoilReturnPositionSpeed * Time.fixedDeltaTime);
        currentRecoilPosition = Vector3.Lerp(currentRecoilPosition, targetRecoilPosition, recoilReturnPositionSnappiness * Time.deltaTime);

        gun.transform.localPosition = hipfirePos.localPosition + (velDirection * amount) + currentRecoilPosition;

        //--------------------------------------------------------------------------------------------------------------------------------------------------
        // Rotational Recoil
        //--------------------------------------------------------------------------------------------------------------------------------------------------
        targetRecoilRotation = Vector3.Lerp(targetRecoilRotation, Vector3.zero, recoilReturnRotationSpeed * Time.fixedDeltaTime);
        //account gun rotation for mouse movement
        targetRecoilRotation += mouseVector * gun.weaponData.turnForce * Time.fixedDeltaTime;
        currentRecoilRotation = Vector3.Slerp(currentRecoilRotation, targetRecoilRotation, recoilReturnRotationSnappiness * Time.fixedDeltaTime);
        //clamp rotation to max values
        currentRecoilRotation = Vector3.ClampMagnitude(currentRecoilRotation, maxGunAngle);
        gun.transform.localRotation = Quaternion.Euler(currentRecoilRotation);
        //--------------------------------------------------------------------------------------------------------------------------------------------------
    }
    private void EquipWeapon(int slot)
    {
        //TODO: create functionality, later planned
    }
}
