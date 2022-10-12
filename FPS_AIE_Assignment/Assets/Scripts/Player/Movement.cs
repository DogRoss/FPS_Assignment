using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// FPS movement character controller 3D.
/// simulates rigidbody-like behaviour with adding and transferring of forces
/// there is no set max speed for anything, drag and other counteracting forces calculate a max speed in real time
/// </summary>
[RequireComponent(typeof(CharacterController))]
public class Movement : MonoBehaviour
{
    #region Protected Variables
    protected CharacterController controller;
    protected Camera cam;
    protected bool movementEnabled = true;
    #endregion

    #region Private Variables
    //TODO: switch to enum
    public MoveState moveState;
    private bool crouching = false;

    private Vector3 currentGravity;
    public bool doubleJumped = false;

    private ControllerColliderHit wallPointHit;

    //Input
    private Vector3 direction = Vector3.zero;
    protected Vector3 mouseVector = Vector3.zero;
    //Applied Input
    public Vector3 moveVelocity = Vector3.zero;
    private Vector3 playerInputDirec = Vector3.zero;
    #endregion

    #region Public Variables
    [Header("General Movement")]
    public int groundLayer;
    public int wallLayer;
    [Tooltip("Weight of player character (in kilograms)")]
    public float playerMass = 70f;
    [Tooltip("Rate at which Player gains speed.")]
    public float acceleration = 5f;
    [Tooltip("rate and direction at which the player is pulled towards")]
    public Vector3 gravity = Physics.gravity;
    [Tooltip("X = regular gravity scale, Y = wall gravity scale.")]
    public Vector2 gravityScale = new Vector2(1f, 1f);
    [Tooltip("force used to send player into the air.")]
    public float jumpForce = 10f;

    [Header("Counter Movement")]
    [Tooltip("Coefficient of forces that act against the Player when in motion (used when on ground).")]
    [Range(0, 1)]
    public float groundFriction = 0.1f;
    [Tooltip("X = horizontal, Y = vertical. Coefficient of forces that act against the Player when in motion (used when on wall).")]
    public Vector2 wallFriction = new Vector2(.1f, .1f);
    [Tooltip("Coefficient of forces that act against the Player when in motion (used when air).")]
    [Range(0, 1)]
    public float drag = 0.3f;

    [Header("Wall Movement Values")]
    [Tooltip("Wall Gravity Coefficient: Speed at which the Player falls while sliding on the wall.")]
    public float WGCoefficient = 0.1f;
    [Tooltip("Max Wall Gravity Coefficient: max speed the player can travel down on the Y Axis")]
    public float MWGCoefficient = .5f;
    [Tooltip("Wall Acceleration Coefficient: put desc here.")]
    public float WACoefficient = 1f;
    [Tooltip("Wall Jump Coefficient: multiplied by jump force to get force when jumping on wall, z is forward of player")]
    public Vector3 WJCoefficient = Vector3.zero;



    [Header("Air Movement Values")]
    [Tooltip("Air Acceleration Coefficient: put desc here.")]
    public float AACoefficient = 5f;

    [Header("Camera Values")]
    public float sensitivity = 0.5f;
    public float maxLookAngle;
    #endregion

    #region Built In Engine Functions
    public virtual void Start()
    {

        controller = GetComponent<CharacterController>();
        cam = Camera.main;
    }
    public virtual void Update()
    {
        HandleCamera();
    }
    public virtual void FixedUpdate()
    {
        if (movementEnabled)
        {

            if (moveState == 0 && controller.collisionFlags == CollisionFlags.None)
            {
                moveState = (MoveState)1;
            }
            else if (moveState == (MoveState)2 && controller.collisionFlags != CollisionFlags.Sides)
                moveState = (MoveState)1;

            //apply forces to controller
            switch (moveState)
            {
                case 0:             //Ground
                    GroundMovement();
                    ApplyFriction();
                    break;
                case (MoveState)1:  //Air
                    AirMovement();
                    break;
                case (MoveState)2:  //Wall
                    WallSlide();
                    ApplyFriction();
                    break;
            }

            ApplyDrag();
            ApplyGravity();

            controller.Move(moveVelocity * Time.deltaTime);
        }
    }
    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if(hit.transform.root.TryGetComponent<RagdollController>(out RagdollController rgdc))
        {
            rgdc.RagdollEnabled = true;
        }

        if (hit.transform.TryGetComponent<Rigidbody>(out Rigidbody rb))
        {
            rb.AddForceAtPosition(controller.velocity + (controller.velocity.normalized * (acceleration * playerMass)), hit.point, ForceMode.Force);
        }

        if (hit.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            print("entteeeer");
            moveState = 0;
            if (doubleJumped)
                doubleJumped = false;
        }
        else if (moveState != 0 && hit.gameObject.layer == LayerMask.NameToLayer("Wall"))
        {
            moveState = (MoveState)2;
            wallPointHit = hit;
            if (doubleJumped)
                doubleJumped = false;
        }
    }
    
    private void OnDrawGizmos()
    {
        //draw movement vector
        if(controller && controller.velocity != null)
            Debug.DrawLine(transform.position, transform.position + controller.velocity.normalized * 3);

        Debug.DrawLine(transform.position, transform.position + playerInputDirec, Color.green);
    }
    #endregion

    #region Input System Functions
    private void OnMove(InputValue value)
    {
        direction = value.Get<Vector3>().normalized;
    }
    private void OnJump(InputValue value)
    {
        if (value.Get<float>() > 0)
        {
            if (moveState == 0)
            {
                moveVelocity.y = 0;
                Jump(false);
            }
            else if (moveState == (MoveState)2)
                Jump(true);
            else if (!doubleJumped)
            {
                Jump(false);
                doubleJumped = true;
            }
        }
    }
    private void OnMouseChange(InputValue value)
    {
        mouseVector.x = -value.Get<Vector2>().y;
        mouseVector.y = value.Get<Vector2>().x;
    }
    #endregion

    #region Getter/Setter Functions
    public float CurrentSpeed
    {
        get
        {
            return controller.velocity.magnitude;
        }
    }
    public float CurrentHorizontalSpeed
    {
        get
        {
            Vector2 vec = new Vector2(controller.velocity.x, controller.velocity.z);
            return vec.magnitude;
        }
    }
    public Camera Cam
    {
        get
        {
            return cam;
        }
    }
    public CharacterController Controller
    {
        get
        {
            return controller;
        }
    }
    #endregion

    #region Player Functions
    /// <summary>
    /// Handles camera movement for looking around
    /// </summary>
    private void HandleCamera()
    {
        Vector3 axis = mouseVector;
        axis.y = 0;
        cam.transform.Rotate(axis, Mathf.Abs(mouseVector.x) * sensitivity);

        axis.y = mouseVector.y;
        axis.x = 0;
        transform.Rotate(axis, Mathf.Abs(mouseVector.y) * sensitivity);
    }

    /// <summary>
    /// Handles what direction the player will move in based on being in the air and input direction
    /// </summary>
    private void AirMovement()
    {
        /* grab current velocity, this will be the value that forces act against;
         * take directional input and account for transform.forward;
         * with direction, use that and air acceleration to act against the stored velocity;
         * return velocity; */

        //get directional input and account for player forward
        Vector3 forces = transform.forward * direction.z + transform.right * direction.x;
        //get acceleration rate
        forces *= acceleration * AACoefficient;

        AddForce(forces);
    }

    /// <summary>
    /// Handles what direction the player will move in based on being grounded and input direction
    /// </summary>
    private void GroundMovement()
    {
        /* grab current velocity, this will be the value that forces act against;
         * take directional input and account for transform.forward;
         * with direction, use that and ground acceleration to act against the stored velocity;
         * return velocity; */

        //TODO: set gravity lower here

        //get directional input and account for player forward
        Vector3 forces = transform.forward * direction.z + transform.right * direction.x;

        //get acceleration rate
        forces *= acceleration;


        AddForce(forces);
    }


    /// <summary>
    /// Handles Jump Forces on player based on if the player is on a wall or on ground 
    /// </summary>
    /// <param name="wallJump"></param>
    private void Jump(bool wallJump)
    {
        if (!wallJump)
        {
            AddForce(Vector2.up * jumpForce * playerMass);
        }
        else
        { 
            //TODO: change moveState off of wall
            //moveVelocity += wallPointHit.normal * (jumpForce * WJCoefficient.x) + (transform.forward * (jumpForce * WJCoefficient.z));
            //currentVerticalSpeed = jumpForce * WJCoefficient.y;
            //AddForce()
        }

        //grounded = false;
    }
    /// <summary>
    /// Handles how the Player moves against the wall in a sliding motion
    /// </summary>
    private void WallSlide()
    {

        /*
         * pain and agony
         * the description goes here but im just waaaay too tired rn
         */

        if (moveState != (MoveState)2)
            return;

        //store input in relation to where the player is facing
        playerInputDirec = transform.forward * direction.z + transform.right * direction.x;
        playerInputDirec.y = 0;

        //take current velocity and transfer to wall plane
        moveVelocity = Vector3.ProjectOnPlane(moveVelocity - (Vector3.up * -moveVelocity.y), wallPointHit.normal);

        //measure forces and apply to plane
        Vector3 forces = playerInputDirec * (acceleration * WACoefficient);
        forces = Vector3.ProjectOnPlane(forces, wallPointHit.normal);
        forces -= wallPointHit.normal;

        //add to movement vector
        moveVelocity += forces;
    }

    private void ApplyDrag()
    {
        Vector3 counterForce = Vector3.zero;

        if (controller.velocity.magnitude > 0)
        {
            counterForce.x = controller.velocity.x * drag;
            counterForce.y = controller.velocity.y * drag;
            counterForce.z = controller.velocity.z * drag;
        }

        AddForce(-counterForce);
    }
    private void ApplyFriction()
    {
        Vector3 counterForce = Vector3.zero;

        switch (moveState)
        {
            case 0:
                print("ground fric");
                if (CurrentHorizontalSpeed != 0)
                    counterForce.x = controller.velocity.x * groundFriction;
                    counterForce.z = controller.velocity.z * groundFriction;
                break;
            case (MoveState)2:
                if (CurrentHorizontalSpeed > 0)
                    counterForce.x = controller.velocity.x * wallFriction.x;
                    counterForce.y = controller.velocity.y * wallFriction.y;
                    counterForce.z = controller.velocity.z * wallFriction.x;
                break;
        }

        AddForce(-counterForce);
    }
    private void ApplyGravity()
    {
        switch (moveState)
        {
            case 0:
                currentGravity = Vector3.zero;
                break;
            case (MoveState)1:
                currentGravity = Physics.gravity * gravityScale.x;
                break;
            case (MoveState)2:
                currentGravity = Physics.gravity * gravityScale.y;
                break;
        }

        AddForce(currentGravity * playerMass, ForceMode.Force);
    }

    public void AddForce(Vector3 force, ForceMode forceType = ForceMode.Force)
    {
        switch (forceType)
        {
            case ForceMode.Force:
                moveVelocity += force * (1.0f / playerMass);
                break;
            case ForceMode.Impulse:
                moveVelocity += force * playerMass;
                break;
            case ForceMode.Acceleration:
                moveVelocity += force;
                break;
            case ForceMode.VelocityChange:
                moveVelocity = force;
                break;
        }
    }

    #endregion
}
