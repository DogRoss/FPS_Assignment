using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

/*
 * inherits movement for movement variables
 * handles player interactions like weapons and worldspace interactions
 */

/// <summary>
/// Handles player and player interactions, like guns and grenades.
/// Inherits Movement class and IDamageable interface.
/// </summary>
public class PlayerController : Movement, IDamageable
{
    public static Movement player;

    [Header("Animation")]
    public RagdollController rc;
    public float health = 100f;

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
    public Image reticle;

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

    private float currentTime = 0f;
    //TODO: make a wallcheck distance thing
    ////camera rays
    //Ray ray;
    //RaycastHit hitData;
    //bool hit = false;
     
    public override void Start()
    {
        base.Start();

        player = this;

        gun.recoilEvent += AddRecoil;

        rc = GetComponentInChildren<RagdollController>();
        if (!rc)
            Debug.LogError("ERROR: couldnt grab RagdollController!");
    }
    public override void Update()
    {
        base.Update();

        CalculateWeaponPosition();
        ApplyRecoil();
        HandleAnimation();

        currentTime += Time.deltaTime;
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
        if (!gun)
            return;

        if (value.Get<float>() > 0)
            gun.ToggleShot(true);
        if (value.Get<float>() <= 0)
            gun.ToggleShot(false);
    }
    private void OnRMouseDown(InputValue value)
    {
        if (value.Get<float>() > 0)
        {
            ads = true;
            reticle.enabled = false;
        }
        else
        {
            ads = false;
            reticle.enabled = true;
        }
    }
    private void OnSwitchFireMode()
    {
        if (gun)
            gun.ToggleAuto();
    }
    private void OnThrow()
    {
        if(currentTime > cooldownTime)
        {
            GrenadeWeapon grenadeObj = Instantiate(grenadePrefab);
            grenadeObj.transform.position = cam.transform.position + (cam.transform.forward * 2);
            grenadeObj.GetComponent<Rigidbody>().AddForce(cam.transform.forward * throwForce, ForceMode.Impulse);
            StartCoroutine(grenadeObj.GrenadeTimer());

            currentTime = 0;
        }
        
    }

    //--------------------------------------------------------------------------------------------------------------------------
    // User Functions
    //--------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Applies recoil values to gun and drops off recoil energy over time
    /// </summary>
    private void ApplyRecoil()
    {
        currentRecoilPosition += recoilVelocity * Time.fixedDeltaTime;
        recoilVelocity = Vector3.Lerp(recoilVelocity, Vector3.zero, aimingPositionForce * Time.fixedDeltaTime);
    }
    /// <summary>
    /// Adds recoil force to the Player/Gun recoil values
    /// </summary>
    /// <param name="force"></param>
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
    /// <summary>
    /// takes in relevant position/rotation values and applies to gun
    /// </summary>
    private void CalculateWeaponPosition()
    {
        if (!gun)
            return;

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
    /// <summary>
    /// Handles Ragdoll Rig controller
    /// </summary>
    private void HandleAnimation()
    {
        if(CurrentSpeed > 0)
        {
            rc.Anim.SetBool("Idle", false);
        }
        else
        {
            rc.Anim.SetBool("Idle", true);
        }

        //handle rig here
        if (gun)
        {
            //rc.RigController.SetAimDirection(gun.muzzle.forward);
            RaycastHit hit;
            Ray ray = new Ray();
            ray.origin = gun.muzzle.position;
            ray.direction = gun.muzzle.forward;
            Physics.Raycast(gun.muzzle.position, gun.muzzle.forward, out hit);
            if (hit.collider)
                rc.RigController.SetAimTargetPos(hit.point);
            else
                rc.RigController.SetAimTargetPos(transform.position + ray.direction * 200f);


        }
    }
    private void EquipWeapon(int slot)
    {
        //TODO: create functionality, later planned
    }

    //--------------------------------------------------------------------------------------------------------------------------
    // Inherited Functions
    //--------------------------------------------------------------------------------------------------------------------------

    public void TakeDamage(float damage)
    {
        print("took: " + damage + " damage");

        health -= damage;

        if (health <= 0)
            OnDeath();
    }

    [ContextMenu("Die")]
    public void OnDeath()
    {
        Vector3 force = controller.velocity;

        controller.enabled = false;
        rc.RagdollEnabled = true;
        rc.transform.parent = null;
        movementEnabled = false;

        foreach(Rigidbody rb in rc.RigidBodies)
        {
            rb.velocity = force;
        }

        Camera.main.cullingMask = -1;

        gun.Free();
        gun = null;
        //TODO: toggle off gun interaction and noticeable character movement
        //TODO: take away input functionality
    }
}
