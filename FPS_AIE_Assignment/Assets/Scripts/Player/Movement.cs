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

    protected CharacterController controller;
    protected Camera cam;
    protected bool movementEnabled = true;

    public Transform spawn;

    [Header("General Movement Values")]
    [Tooltip("Weight of player character (in kilograms)")]
    public float playerMass = 70f;
    [Tooltip("Rate at which Player gains speed.")]
    public float acceleration = 5f;
    [Tooltip("acceleration rate of DownForce the player will experience when in the air.")]
    public float gravity = 0.5f;
    [Tooltip("force used to send player into the air.")]
    public float jumpForce = 10f;
    public LayerMask groundMask;

    private float currentVerticalSpeed = 0f;
    private bool doubleJumped = false;

    [Header("Ground Movement Values")]
    [Tooltip("Ground Acceleration Coefficient: multiplied by 'Acceleration' to get the acceleration of Player when touching the ground.")]
    public float GACoefficient = 1f;
    [Tooltip("Coefficient of forces that act against the Player when in motion.")]
    [Range(0, 1)]
    public float groundFrictionCoefficient = 0.1f;


    [Header("Wall Movement Values")]
    [Tooltip("Wall Gravity Coefficient: Speed at which the Player falls while sliding on the wall.")]
    public float WGCoefficient = 0.1f;
    [Tooltip("Max Wall Gravity Coefficient: max speed the player can travel down on the Y Axis")]
    public float MWGCoefficient = .5f;
    [Tooltip("Wall Acceleration Coefficient: put desc here.")]
    public float WACoefficient = 1f;
    [Tooltip("Wall Jump Coefficient: multiplied by jump force to get force when jumping on wall, z is forward of player")]
    public Vector3 WJCoefficient = Vector3.zero;
    [Tooltip("Coefficient of forces that act against the Player when in motion against a wall.")]
    [Range(0, 1)]
    public float wallFrictionCoefficient = 0.1f;

    private ControllerColliderHit wallPointHit;


    [Header("Air Movement Values")]
    [Tooltip("Air Acceleration Coefficient: put desc here.")]
    public float AACoefficient = 5f;
    [Tooltip("how much air drag affects the acceleration imposed by gravity.")]
    [Range(0,1)]
    public float verticalDragCoefficient = .25f;
    [Tooltip("how much air drag affects the acceleration imposed by gravity.")]
    [Range(0, 1)]
    public float horizontalDragCoefficient = .25f;

    [Header("Camera Values")]
    public float sensitivity = 0.5f;
    public float maxLookAngle;

    //Raycast/Ground Check
    public bool grounded;
    private bool touchingWall;
    private bool crouching = false;


    //Input
    private Vector3 direction = Vector3.zero;
    protected Vector3 mouseVector = Vector3.zero;
    //Applied Input
    public Vector3 moveVec = Vector3.zero;
    public Vector3 addedVelocity = Vector3.zero;
    private Vector3 playerInputDirec = Vector3.zero;

    //Temp variables
    //TODO: redo varaibles  or delete

    //--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
    // Built In Engine Functions
    //--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
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

            if (controller.collisionFlags == CollisionFlags.None)
            {
                grounded = false;
            }

            Debug.DrawLine(transform.position, transform.position + (Vector3.down * ((controller.height / 2) + 0.3f)), Color.magenta);

            if (controller.collisionFlags != CollisionFlags.Sides)
                touchingWall = false;
            //if (controller.collisionFlags != CollisionFlags.None)
            //    grounded = false;

            //apply forces to controller
            if (grounded)
            {
                GroundMovement();
                GroundFriction();
            }
            else if (!touchingWall)
            {
                //account for gravity
                AirMovement();
                AirDrag();
            }
            else
            {
                //is touching wall, so account for wall slide
                WallSlide();
                WallFriction();
            }

            moveVec.y = currentVerticalSpeed;
            moveVec += addedVelocity;
            addedVelocity = Vector3.zero;
            controller.Move(moveVec * Time.deltaTime);
        }
    }
    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if(hit.gameObject.layer == LayerMask.NameToLayer("Death"))
        {
            controller.transform.position = spawn.position;
        }

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
            touchingWall = false;
            grounded = true;
            if (doubleJumped)
                doubleJumped = false;
        }
        else if (!grounded && hit.gameObject.layer == LayerMask.NameToLayer("Wall"))
        {
            touchingWall = true;
            grounded = false;
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
    //--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
    // Input System Functions
    //--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
    private void OnMove(InputValue value)
    {
        direction = value.Get<Vector3>().normalized;
    }
    private void OnJump(InputValue value)
    {
        if (value.Get<float>() > 0)
        {
            if (grounded)
                Jump(false);
            else if (touchingWall)
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

    //--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
    // Getters
    //--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

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

    //--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
    // Player Functions
    //--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

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

        //make sure we are grounded
        if (grounded)
        {
            return;
        }

        currentVerticalSpeed -= gravity;

        //store velocity
        Vector3 vel = moveVec;
        vel.y = 0;

        //get directional input and account for player forward
        Vector3 forces = transform.forward * direction.z + transform.right * direction.x;
        //get acceleration rate
        forces *= acceleration * AACoefficient;
        //apply forces
        vel += forces;

        moveVec = vel;
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

        //make sure we are grounded
        if (!grounded)
            return;

        currentVerticalSpeed = -.1f;

        //store velocity
        Vector3 vel = controller.velocity;
        vel.y = 0;

        //get directional input and account for player forward
        Vector3 forces = transform.forward * direction.z + transform.right * direction.x;

        //get acceleration rate
        forces *= acceleration * GACoefficient;

        //apply forces
        vel += forces;

        moveVec = vel;
    }


    /// <summary>
    /// Handles forces that oppose Player's movement on the ground
    /// </summary>
    private void GroundFriction()
    {
        Vector3 counterForce = Vector3.zero;

        if (controller.velocity.x != 0)
            counterForce.x = controller.velocity.x * groundFrictionCoefficient;
        if (controller.velocity.z != 0)
            counterForce.z = controller.velocity.z * groundFrictionCoefficient;

        moveVec -= counterForce;
    }
    /// <summary>
    /// Handles Forces in air that oppose Player's movement through the air 
    /// </summary>
    private void AirDrag()
    {
        Vector3 counterForce = Vector3.zero;

        if (controller.velocity.magnitude > 0)
        {
            counterForce.x = controller.velocity.x * horizontalDragCoefficient;
            currentVerticalSpeed -= currentVerticalSpeed * verticalDragCoefficient;
            counterForce.z = controller.velocity.z * horizontalDragCoefficient;
        }

        moveVec -= counterForce;
    }


    /// <summary>
    /// Handles Jump Forces on player based on if the player is on a wall or on ground 
    /// </summary>
    /// <param name="wallJump"></param>
    private void Jump(bool wallJump)
    {
        if (!wallJump)
        {
            currentVerticalSpeed = jumpForce;
        }
        else
        { 
            touchingWall = false;
            moveVec += wallPointHit.normal * (jumpForce * WJCoefficient.x) + (transform.forward * (jumpForce * WJCoefficient.z));
            currentVerticalSpeed = jumpForce * WJCoefficient.y;
        }

        grounded = false;
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

        if (!touchingWall)
            return;

        //store input in relation to where the player is facing
        playerInputDirec = transform.forward * direction.z + transform.right * direction.x;
        playerInputDirec.y = 0;

        //take current velocity and transfer to wall plane
        moveVec = Vector3.ProjectOnPlane(moveVec - (Vector3.up * -moveVec.y), wallPointHit.normal);

        //measure forces and apply to plane
        Vector3 forces = playerInputDirec * (acceleration * WACoefficient);
        forces = Vector3.ProjectOnPlane(forces, wallPointHit.normal);
        forces -= wallPointHit.normal;

        //add to movement vector
        moveVec += forces;
    }
    /// <summary>
    /// handles how counteracting forces affect Player's movement
    /// </summary>
    private void WallFriction()
    {
        //calculate gravity on wall
        if (currentVerticalSpeed > -gravity * MWGCoefficient)
            currentVerticalSpeed = controller.velocity.y - (gravity * WGCoefficient);
        else
            currentVerticalSpeed -= currentVerticalSpeed * wallFrictionCoefficient;
        //currentVerticalSpeed = Mathf.Clamp(currentVerticalSpeed, -gravity * MWGCoefficient, Mathf.Infinity);

        Vector3 counterForce = Vector3.zero;

        if (CurrentHorizontalSpeed > 0)
        {
            counterForce.x = controller.velocity.x * wallFrictionCoefficient;
            counterForce.z = controller.velocity.z * wallFrictionCoefficient;
        }
        
        moveVec -= counterForce;
    }

    public void AddForce(Vector3 force)
    {
        print("AddForce called");
        addedVelocity += force;
    }

    public void SetForce(Vector3 force)
    {
        addedVelocity = force;
    }
}
