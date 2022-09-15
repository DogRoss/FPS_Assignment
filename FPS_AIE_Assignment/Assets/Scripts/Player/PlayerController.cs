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
    [Header("Animation")]
    public Animator gfx;

    [Header("Gun Variables")]
    public RangedWeapon gun;
    public Transform hipfirePos;
    public Transform adsPos;

    //TODO: make obsolete / delete
    public bool ads;

    //aiming variables
    [Header("Recoil Variables")]
    public float turnForce = 10f;
    public float gunDistanceTolerance = 10f;
    [Tooltip("speed at which gun recovers from recoil position offset")]
    public float aimingPositionForce = 10f;
    [Tooltip("speed at which gun recovers from recoil angle offset")]
    public float aimingRotationForce = 5f;
    [Tooltip("multiplied by aiming Position Force for aiming down sights.")]
    public float ADSPositionCoefficient = 2f;
    [Tooltip("multiplied by aiming Rotation Force for aiming down sights.")]
    public float ADSRotationCoefficient = 2f;
    [Tooltip("how fast the gun points and move towards target Vectors")]
    public float aimingSnappiness = 10f;

    //Position
    private Vector3 targetRecoilPosition = Vector3.zero;
    private Vector3 currentRecoilPosition = Vector3.zero;
    private Vector3 recoilVelocity = Vector3.zero;
    //Rotation
    private Vector3 currentRecoilRotation = Vector3.zero;
    private Vector3 targetRecoilRotation = Vector3.zero;
    //

    //TODO: change title to 'Throwable Variables'
    //TODO: expand functionality to throwable class
    [Header("Grenade Variables")]
    public GrenadeWeapon grenadePrefab;
    public float throwForce = 10f;
    public float throwAngle = 5f;
    public float cooldownTime = 5f;

    //TODO: make a wallcheck distance thing
    ////camera rays
    //Ray ray;
    //RaycastHit hitData;
    //bool hit = false;
     
    public override void Start()
    {
        base.Start();

        gun.recoilEvent += AddRecoil;
    }
    public override void Update()
    {
        base.Update();

        CalculateWeaponPosition();
        ApplyRecoil();
        HandleAnimation();
    }
    public override void FixedUpdate()
    {
        base.FixedUpdate();
        //hit = false;
        //ray.origin = cam.transform.position; ray.direction = cam.transform.forward;
        //hit = Physics.Raycast(ray, out hitData, gunWallCheckDist, gunCollisionMask.value);
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
            ads = true;
        else
            ads = false; 
    }
    private void OnSwitchFireMode()
    {
        if (gun)
            gun.ToggleAuto();
    }
    private void OnThrow()
    {
        GrenadeWeapon grenadeObj = Instantiate(grenadePrefab);
        grenadeObj.transform.position = transform.position + transform.forward * 1.5f;
        grenadeObj.GetComponent<Rigidbody>().AddForce((transform.forward + Vector3.up) * throwForce, ForceMode.Impulse);
        grenadeObj.StartGrenadeTimer();
    }

    //--------------------------------------------------------------------------------------------------------------------------
    // User Functions
    //--------------------------------------------------------------------------------------------------------------------------
    private void ApplyRecoil()
    {
        currentRecoilPosition += recoilVelocity * Time.fixedDeltaTime;
        recoilVelocity = Vector3.Lerp(recoilVelocity, Vector3.zero, aimingPositionForce * Time.fixedDeltaTime);
    }
    private void AddRecoil(float force)
    {
        Vector3 vec = Vector3.zero;
        vec.x = Random.Range(-force, 0);
        vec.y = Random.Range(-force, force);

        targetRecoilRotation += ads ? (vec / ADSRotationCoefficient) * gun.weaponData.BRFAngularCoefficient : vec * gun.weaponData.BRFAngularCoefficient;

        vec.z = -force;
        vec.y = Random.Range(0, force);
        vec.x = Random.Range(-force, force);
        recoilVelocity += ads ? vec / ADSPositionCoefficient : vec;
    }
    private void CalculateWeaponPosition()
    {

        //take current movement speed, and store opposite of velocity direction
        Vector3 velDirection = transform.InverseTransformDirection(-controller.velocity.normalized);
        float amount = 0;
        //--------------------------------------------------------------------------------------------------------------------------------------------------
        // Positional Recoil
        //--------------------------------------------------------------------------------------------------------------------------------------------------
        targetRecoilPosition = Vector3.Lerp(targetRecoilPosition, Vector3.zero, aimingPositionForce * Time.fixedDeltaTime);
        if (ads)
        {
            currentRecoilPosition = Vector3.Lerp(currentRecoilPosition, targetRecoilPosition + adsPos.localPosition, aimingSnappiness * Time.deltaTime);
            amount = (controller.velocity.magnitude / gunDistanceTolerance) / ADSPositionCoefficient;
        }
        else
        {
            currentRecoilPosition = Vector3.Lerp(currentRecoilPosition, targetRecoilPosition + hipfirePos.localPosition, aimingSnappiness * Time.deltaTime);
            amount = controller.velocity.magnitude / gunDistanceTolerance;
        }
        gun.transform.localPosition = (velDirection * amount) + currentRecoilPosition;
        //--------------------------------------------------------------------------------------------------------------------------------------------------

        //--------------------------------------------------------------------------------------------------------------------------------------------------
        // Rotational Recoil
        //--------------------------------------------------------------------------------------------------------------------------------------------------
        targetRecoilRotation = Vector3.Lerp(targetRecoilRotation, Vector3.zero, aimingRotationForce * Time.fixedDeltaTime);
        //account gun rotation for mouse movement
        targetRecoilRotation += mouseVector * turnForce * Time.fixedDeltaTime;
        currentRecoilRotation = Vector3.Slerp(currentRecoilRotation, targetRecoilRotation, aimingSnappiness * Time.fixedDeltaTime);
        //clamp rotation to max values
        gun.transform.localRotation = Quaternion.Euler(currentRecoilRotation);
        //--------------------------------------------------------------------------------------------------------------------------------------------------
    }
    private void HandleAnimation()
    {
        if(CurrentSpeed > 0)
        {
            gfx.SetBool("Idle", false);
        }
        else
        {
            gfx.SetBool("Idle", true);
        }
    }
    private void EquipWeapon(int slot)
    {
        //TODO: create functionality, later planned
    }
}
