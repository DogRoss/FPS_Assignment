using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class Movement : MonoBehaviour
{
    CharacterController controller;
    protected Camera cam;

    [Header("General Movement Values")]
    [Tooltip("Rate at which Player gains speed.")]
    public float acceleration = 5f;
    [Tooltip("acceleration rate of DownForce the player will experience when in the air.")]
    public float gravity = 0.5f;
    [Tooltip("force used to send player into the air.")]
    public float jumpForce = 10f;

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


    //Raycast/Ground Check
    private bool grounded;
    private bool touchingWall;


    //Input
    private Vector3 direction = Vector3.zero;
    private Vector3 mouseVector = Vector3.zero;
    //Applied Input
    private Vector3 moveVec = Vector3.zero;
    private Vector3 playerInputDirec = Vector3.zero;

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

        if (controller.collisionFlags != CollisionFlags.Sides)
            touchingWall = false;

        //apply forces to controller
        if (grounded)
        {
            GroundMovement();
            GroundFriction();
            moveVec.y = currentVerticalSpeed;
            controller.Move(moveVec * Time.deltaTime);
        }
        else if(!touchingWall)
        {
            //account for gravity
            currentVerticalSpeed -= gravity;

            AirMovement();
            AirDrag();
            moveVec.y = currentVerticalSpeed;
            controller.Move(moveVec * Time.deltaTime);
        }
        else
        {
            //is touching wall, so account for wall slide
            WallSlide();
            WallFriction();
            moveVec.y = currentVerticalSpeed;
            controller.Move(moveVec * Time.deltaTime);
        }
    }
    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        //print(hit.collider.name + " // " + LayerMask.LayerToName(hit.gameObject.layer));

        if (hit.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
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
        if(value.Get<float>() > 0)
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

    private float CurrentSpeed
    {
        get
        {
            return controller.velocity.magnitude;
        }
    }
    private float CurrentHorizontalSpeed
    {
        get
        {
            Vector2 vec = new Vector2(controller.velocity.x, controller.velocity.z);
            return vec.magnitude;
        }
    }

    //--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
    // Player Functions
    //--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

    /* 
     * Handles camera movement for looking around */
    private void HandleCamera()
    {
        Vector3 axis = mouseVector;
        axis.y = 0;
        cam.transform.Rotate(axis, 1);

        axis.y = mouseVector.y;
        axis.x = 0;
        transform.Rotate(axis, 1);
    }

    /*
     * Handles what direction the player will move in based on being in the air and input direction */
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

    /*
     * Handles what direction the player will move in based on being grounded and input direction */
    private void GroundMovement()
    {
        /* grab current velocity, this will be the value that forces act against;
         * take directional input and account for transform.forward;
         * with direction, use that and ground acceleration to act against the stored velocity;
         * return velocity; */

        //make sure we are grounded
        if (!grounded)
            return;
        else
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


    /*
     * -v- Handles forces that oppose Player's movement on the ground */
    private void GroundFriction()
    {
        Vector3 counterForce = Vector3.zero;

        if (controller.velocity.x != 0)
            counterForce.x = controller.velocity.x * groundFrictionCoefficient;
        if (controller.velocity.z != 0)
            counterForce.z = controller.velocity.z * groundFrictionCoefficient;

        moveVec -= counterForce;
    }
    /*
     * Handles Forces in air that oppose Player's movement through the air */
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


    /*
     * Handles Jump Forces on player based on if the player is on a wall or on ground */
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
    /*
     * Handles how the Player moves against the wall in a sliding motion */
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
}
