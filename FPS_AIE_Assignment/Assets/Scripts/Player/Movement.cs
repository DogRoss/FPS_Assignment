using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class Movement : MonoBehaviour
{
    CharacterController controller;
    Camera cam;



    [Header("Ground/Wall Movement Values")]
    public LayerMask groundMask;
    public LayerMask wallMask;
    [Tooltip("Acceleration of player when on ground, accelerates differently based on what 'Ground Force Type' is applied.")]
    public float groundSpeed = 10f;
    [Tooltip("Coefficient of forces that act against the Player when in motion.")]
    [Range(0, 1)]
    public float groundFrictionCoefficient = 0.1f;
    //[Tooltip("speed at which the player falls while sliding on the wall")]
    //public float wallFallMultiplier = 0.5f;
    //public float wallSpeed = 20f;

    private float finalGroundSpeed;

    [Header("Air Movement Values")]
    [Range(0, 1)]
    public float airControlCoefficientX = 0.5f;
    [Range(0, 1)]
    public float airControlCoefficientZ = 0.5f;
    [Tooltip("how much air drag affects the acceleration imposed by gravity.")]
    [Range(0,1)]
    public float verticalDragCoefficient = .25f;
    [Tooltip("how much air drag affects the acceleration imposed by gravity.")]
    [Range(0, 1)]
    public float horizontalDragCoefficient = .25f;
    [Tooltip("amount of down force the player will accelerate to when in the air.")]
    public float gravity = 0.5f;
    [Tooltip("The rate at which the player accelerates from imposed gravity.")]
    public float gravityAcceleration = .1f;

    [Header("Jump Values")]
    [Tooltip("force used to send player into the air.")]
    public float jumpForce = 10f;

    private float jumpPower = 0f;
    private bool jumpHeld;

    [Header("Ground Values")]
    public LayerMask groundLayers;
    [Tooltip("How far down ground check raycast goes from set position.")]
    public float groundCheckDistance = 1f;
    [Tooltip("How far from original position ground check raycast check is.")]
    public float groundCheckYOffset = 0f;

    //Raycast/Ground Check
    public bool grounded;
    public bool touchingWall;

    //Input
    private Vector3 direction = Vector3.zero;
    private float xAxis;
    private float zAxis;
    public Vector3 finalDir = Vector3.zero;
    private Vector3 mouseVector = Vector3.zero;

    //--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
    // Built In Engine Functions
    //--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
    void Start()
    {
        controller = GetComponent<CharacterController>();
        cam = Camera.main;

        finalGroundSpeed = groundSpeed / 10f;
    }
    private void Update()
    {
        HandleCamera();
    }
    private void FixedUpdate()
    {
        //ground detection
        if (controller.collisionFlags == CollisionFlags.Below)
        {
            
        }
        else if(
        (controller.collisionFlags.HasFlag(CollisionFlags.Sides) &&
        !controller.collisionFlags.HasFlag(CollisionFlags.Below)))
        {
            
        }
        if(controller.collisionFlags != CollisionFlags.Below)
        {
            touchingWall = false;
            grounded = false;
            GravityPhysics();
            AirMovement();
        }

        //handle friction/drag/gravity and slow down player at a rate
        if (grounded)
            GroundFriction();
        else 
        {
            AirDrag();
            Debug.Log("called air");
        }

        //apply forces to controller
        controller.Move(finalDir);
    }
    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        Debug.Log("called hitThing: " + LayerMask.LayerToName(hit.gameObject.layer));

        if(hit.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            Debug.Log("called ground");
            touchingWall = false;
            grounded = true;
            GroundMovement();
        }
        else if(hit.gameObject.layer == wallMask.value)
        {
            Debug.Log(controller.collisionFlags.HasFlag(CollisionFlags.Below) + " // how many: " + (controller.collisionFlags & CollisionFlags.Above));
            Debug.Log("called wall");
            //TODO: handle wall here
            touchingWall = true;
            grounded = false;
            WallSlide();
            GravityPhysics();
            AirMovement();
        }
    }
    private void OnDrawGizmos()
    {
        //draw groundcheck raycast
        Debug.DrawLine(transform.position + (transform.up * groundCheckYOffset), (transform.position + (transform.up * groundCheckYOffset)) - (transform.up * groundCheckDistance), Color.red);

        Debug.DrawLine(transform.position, transform.position  + finalDir * 3);
    }
    //--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
    // Input System Functions
    //--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
    private void OnMove(InputValue value)
    {
        direction = value.Get<Vector3>();
    }
    private void OnJump(InputValue value)
    {
        if(value.Get<float>() > 0)
        {
            jumpHeld = true;
            if (grounded)
                Jump(false);
            else if (touchingWall)
                Jump(true);
        }
        else if(value.Get<float>() == 0)
            jumpHeld = false;
    }
    private void OnMouseChange(InputValue value)
    {
        mouseVector.x = -value.Get<Vector2>().y;
        mouseVector.y = value.Get<Vector2>().x;
    }

    //--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
    // Player Functions
    //--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

    private Vector3 ApplyFriction()
    {
        return Vector3.zero;
    }
    /* 
     * Handles camera movement for looking around */
    private void HandleCamera()
    {
        Vector3 axis = mouseVector;
        axis.y = 0;
        cam.transform.Rotate(axis, 3);

        axis.y = mouseVector.y;
        axis.x = 0;
        transform.Rotate(axis, 3);
    }
    private void GravityPhysics()
    {
        if (finalDir.y > -gravity)
            finalDir.y -= gravityAcceleration;

        finalDir.y = Mathf.Clamp(finalDir.y, -gravity, Mathf.Infinity);
    }
    /*
     * Handles Jump Forces on player based on if the player is on a wall or on ground */
    private void Jump(bool wallJump)
    {
        if (wallJump)
        {

        }
        else
            finalDir.y = jumpForce;
    }

    /*
     * Handles what direction the player will move in based on being in the air and input direction */
    private void AirMovement()
    {
        //make sure we are NOT grounded
        if (grounded)
            return;

        //get input from correct axis
        xAxis = direction.x * finalGroundSpeed;
        zAxis = direction.z * finalGroundSpeed;

        //account for player face                    
        finalDir += (transform.right * xAxis * airControlCoefficientX) + (transform.forward * zAxis * airControlCoefficientZ);

        //clamp to max speed to ensure we do not get too fast
        Vector2 clampee = new Vector2(finalDir.x, finalDir.z);
        if(clampee.magnitude > finalGroundSpeed)
        {
            clampee = clampee.normalized * finalGroundSpeed;
        }

        finalDir.x = clampee.x;
        finalDir.z = clampee.y;
    }
    /*
     * Handles what direction the player will move in based on being grounded and input direction */
    private void GroundMovement()
    {
        //make sure we are grounded
        if (!grounded)
            return;

        //make sure gravity isnt affecting us
        if (!jumpHeld)
            finalDir.y = 0;

        //check input and handle movement
        if (direction.magnitude > 0)
        {
            xAxis = direction.x * finalGroundSpeed;
            zAxis = direction.z * finalGroundSpeed;

            //account for player face                    
            Vector3 preDir = transform.right * xAxis + transform.forward * zAxis;
            finalDir.x = preDir.x; finalDir.z = preDir.z;
        }
    }

    
    /*
     * -v- Handles forces that oppose Player's movement on the ground */
    private void GroundFriction()
    {
        if (finalDir.x != 0)
            finalDir.x -= finalDir.x * groundFrictionCoefficient;
        if (finalDir.z != 0)
            finalDir.z -= finalDir.z * groundFrictionCoefficient;
    }
    /*
     * Handles Forces in air that oppose Player's movement through the air */
    private void AirDrag()
    {
        if (finalDir.magnitude > 0)
        {
            finalDir.x -= finalDir.x * horizontalDragCoefficient;
            finalDir.y -= finalDir.y * verticalDragCoefficient;
            finalDir.z -= finalDir.z * horizontalDragCoefficient;
        }
    }
    /*
     * Handles how the Player moves against the wall in a sliding motion */
    private void WallSlide()
    {

    }
}
