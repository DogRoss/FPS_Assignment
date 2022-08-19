using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class Movement : MonoBehaviour
{
    CharacterController controller;
    Camera cam;

    [Header("General Movement Values")]
    public float maxSpeed = 5f; //used universally in the movement script to decide what the speed is in certain conditions
    public float acceleration = 5f;
    [Tooltip("acceleration rate of DownForce the player will experience when in the air.")]
    public float gravity = 0.5f;

    [Header("Ground Movement Values")]
    [Tooltip("Ground Acceleration Coefficient: put desc here.")]
    public float GACoefficient = 1f;
    [Tooltip("Max Ground Speed Coefficient: multiplied by 'Max Speed' to find what the Max Ground Speed will be")]
    public float MGSCoefficient = 1f;
    [Tooltip("How fast the Player can change it's velocity.")]
    public float groundControlCoefficient = 0.7f;
    [Tooltip("Coefficient of forces that act against the Player when in motion.")]
    [Range(0, 1)]
    public float groundFrictionCoefficient = 0.1f;


    [Header("Wall Movement Values")]
    [Tooltip("Speed at which the Player falls while sliding on the wall.")]
    public float wallFallCoefficient = 0.1f;
    [Tooltip("Max Wall Speed Coefficient: put desc here.")]
    public float MWSCoefficient = 1f;
    [Tooltip("Wall Acceleration Coefficient: put desc here.")]
    public float WACoefficient = 1f;
    public float wallJumpForceX = 2f;
    public float wallJumpForceY = 5f;

    private Vector3 wallPoint;


    [Header("Air Movement Values")]
    [Tooltip("Air Acceleration Coefficient: put desc here.")]
    public float AACoefficient = 5f;
    [Tooltip("Max Air Speed Coefficient: put desc here.")]
    public float MASCoefficient = 1f;
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
    


    [Header("Jump Values")]
    [Tooltip("force used to send player into the air.")]
    public float jumpForce = 10f;

    //Raycast/Ground Check
    public bool grounded;
    public bool touchingWall;

    //Input
    private Vector3 direction = Vector3.zero;
    private float xAxis;
    private float zAxis;
    public Vector3 horizontalDir = Vector3.zero;
    public Vector3 verticalDir = Vector3.zero;
    public Vector3 magV;
    public float mag;
    private Vector3 mouseVector = Vector3.zero;

    Vector3 testVect = Vector3.zero;
    Vector3 direc = Vector3.zero;

    //--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
    // Built In Engine Functions
    //--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
    void Start()
    {
        controller = GetComponent<CharacterController>();
        cam = Camera.main;
    }
    private void Update()
    {
        magV = horizontalDir.normalized;
        mag = CurrentSpeed;

        HandleCamera();
    }
    private void FixedUpdate()
    {

        if (controller.collisionFlags != CollisionFlags.Sides)
            touchingWall = false;

        //air detection detection
        if (!touchingWall && !grounded)
        {
            GravityPhysics();
            AirMovement();
        }
        //apply forces to controller, divide by 50 to account to for 1:50 ratio (Unit:Velocity)

        //if (grounded)
        //{
        //    horizontalDir *= acceleration * GACoefficient;
        //    controller.Move((horizontalDir + verticalDir) / 50f);

        //}
        controller.Move((horizontalDir + verticalDir) / 50f);

    }
    private void OnControllerColliderHit(ControllerColliderHit hit)
    {

        //print(hit.collider.name + " // " + LayerMask.LayerToName(hit.gameObject.layer));

        if (hit.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            touchingWall = false;
            grounded = true;
            GroundMovement();
        }
        else if (hit.gameObject.layer == LayerMask.NameToLayer("Wall") && !grounded)
        {
            touchingWall = true;
            grounded = false;
            wallPoint = hit.point;
            WallSlide();
        }
    }

    private void OnDrawGizmos()
    {
        //draw movement vector
        Debug.DrawLine(transform.position, transform.position  + horizontalDir * 3);
        //draw input vector
        Debug.DrawLine(transform.position, transform.position + (direction.normalized + transform.forward), Color.blue);
        

        Debug.DrawLine(testVect, testVect + direc);
        Gizmos.DrawSphere(testVect, .1f);
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
            if (grounded)
                Jump(false);
            else if (touchingWall)
                Jump(true);
        }
    }
    private void OnMouseChange(InputValue value)
    {
        mouseVector.x = -value.Get<Vector2>().y;
        mouseVector.y = value.Get<Vector2>().x;
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
    private void GravityPhysics()
    {
        print("call gravity");
        if (!grounded)
            verticalDir.y -= gravity;

        //horizontalDir.y = Mathf.Clamp(horizontalDir.y, -gravity, Mathf.Infinity);
    }
    

    /*
     * Handles what direction the player will move in based on being in the air and input direction */
    private void AirMovement()
    {
        Debug.Log("call air movement");

        //make sure we are NOT grounded
        if (grounded)
            return;


        //get input from correct axis
        xAxis = direction.normalized.x * (acceleration * AACoefficient);
        zAxis = direction.normalized.z * (acceleration * AACoefficient);

        //account for player face
        if(horizontalDir.x + horizontalDir.z < (maxSpeed * MASCoefficient))
            horizontalDir += ((transform.right * xAxis) * airControlCoefficientX) + ((transform.forward * zAxis) * airControlCoefficientZ);

        AirDrag();
    }
    /*
     * Handles what direction the player will move in based on being grounded and input direction */
    private void GroundMovement()
    {
        //make sure we are grounded
        if (!grounded)
        {
            print("return");
            return;
        }
        else
            verticalDir.y = -.1f;

        //get input from correct axis and account for intended acceleration
        xAxis = direction.x * (acceleration * GACoefficient);
        zAxis = direction.z * (acceleration * GACoefficient);

        Vector3 vec = horizontalDir;
        //account for player face                    
        horizontalDir += (transform.right * xAxis) + (transform.forward * zAxis);

        //clamp to max speed to ensure we do not get too fast
        horizontalDir.x = Mathf.Clamp(horizontalDir.x, -(maxSpeed * MGSCoefficient), (maxSpeed * MGSCoefficient));
        horizontalDir.z = Mathf.Clamp(horizontalDir.z, -(maxSpeed * MGSCoefficient), (maxSpeed * MGSCoefficient));

        //horizontalDir = vec;

        GroundFriction();
    }


    /*
     * -v- Handles forces that oppose Player's movement on the ground */
    private void GroundFriction()
    {
        if (horizontalDir.x != 0)
            horizontalDir.x -= horizontalDir.x * groundFrictionCoefficient;
        if (horizontalDir.z != 0)
            horizontalDir.z -= horizontalDir.z * groundFrictionCoefficient;
    }
    /*
     * Handles Forces in air that oppose Player's movement through the air */
    private void AirDrag()
    {
        if (horizontalDir.magnitude > 0)
        {
            horizontalDir.x -= horizontalDir.x * horizontalDragCoefficient;
            verticalDir.y -= verticalDir.y * verticalDragCoefficient;
            horizontalDir.z -= horizontalDir.z * horizontalDragCoefficient;
        }
    }


    /*
     * Handles Jump Forces on player based on if the player is on a wall or on ground */
    private void Jump(bool wallJump)
    {
        if (wallJump)
            WallJump();
        else
            verticalDir.y = jumpForce;

        grounded = false;
    }
    /*
     * Handles how the Player moves against the wall in a sliding motion */
    private void WallSlide()
    {
        //gravity
        if (verticalDir.y > -gravity * wallFallCoefficient)
            verticalDir.y -= gravity * wallFallCoefficient;
        verticalDir.y = Mathf.Clamp(verticalDir.y, -gravity * wallFallCoefficient, gravity);

        //get the direction towards the wall that was hit
        direc = (wallPoint - transform.position).normalized;
        float wallPull = 0.1f;
        testVect = wallPoint;

        //test to see how close to which side one is
        //the closer the dot is to 0 means more perpendicular
        //the closer the dot is to -1/1 means either facing same way or opposite way
        float forwardDot = Vector3.Dot(direc, transform.forward);
        float sideWaysDot = Vector3.Dot(direc, transform.right);
        //Debug.Log(forwardDot + " forward // sideWays: " + sideWaysDot);

        float wallSpeedCoefficient = Mathf.Abs(sideWaysDot);
        //account for player forward
        horizontalDir.x += transform.forward.x; horizontalDir.z += transform.forward.z;

        horizontalDir.x *= wallSpeedCoefficient;
        horizontalDir.z *= wallSpeedCoefficient;

        //account for wall direction
        horizontalDir.x += direc.x * wallPull; horizontalDir.z = direc.z * wallPull;

        Vector2 vec2FDir = new Vector2(horizontalDir.x, horizontalDir.z);
        if(vec2FDir.magnitude > (maxSpeed * MWSCoefficient))
        {
            vec2FDir = vec2FDir.normalized * (maxSpeed * MWSCoefficient);
            horizontalDir.x = vec2FDir.x; horizontalDir.z = vec2FDir.y;
        }
    }
    /*
     * Handles Jumping off wall */
    private void WallJump()
    {
        //get the direction towards the wall that was hit
        direc = (wallPoint - transform.position).normalized;

        //add direction away from wall
        horizontalDir.x = direc.x * -wallJumpForceX; horizontalDir.z = direc.z * -wallJumpForceX;
        verticalDir.y = wallJumpForceY;

    }

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
}
