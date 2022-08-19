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
    [Tooltip("Acceleration of player when on ground, accelerates differently based on what 'Ground Force Type' is applied.")]
    public float groundSpeed = 10f;
    [Tooltip("Coefficient of forces that act against the Player when in motion.")]
    [Range(0, 1)]
    public float groundFrictionCoefficient = 0.1f;
    [Tooltip("speed at which the player falls while sliding on the wall")]
    public float wallFallCoefficient = 0.1f;
    public float wallSpeed = 20f;

    private float finalGroundSpeed;
    private float finalWallSpeed;
    private Vector3 wallPoint;

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

    //Raycast/Ground Check
    public bool grounded;
    public bool touchingWall;

    //Input
    private Vector3 direction = Vector3.zero;
    private float xAxis;
    private float zAxis;
    public Vector3 finalDir = Vector3.zero;
    public float finalDirMag;
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

        finalGroundSpeed = groundSpeed / 10f;
        finalWallSpeed = wallSpeed / 10f;
    }
    private void Update()
    {
        finalDirMag = finalDir.magnitude;

        HandleCamera();
    }
    private void FixedUpdate()
    {
        if (controller.collisionFlags != CollisionFlags.Sides)
            touchingWall = false;

        //air detection detection
        if (!touchingWall)
        {
            GravityPhysics();

            if(!grounded)
                AirMovement();
        }

        //handle friction/drag/gravity and slow down player at a rate
        if (grounded)
            GroundFriction();
        else
            AirDrag();

        //apply forces to controller
        controller.Move(finalDir);
    }
    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        print(hit.collider.name + " // " + LayerMask.LayerToName(hit.gameObject.layer));

        if (hit.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            touchingWall = false;
            grounded = true;
            GroundMovement();
        }
        else if (hit.gameObject.layer == LayerMask.NameToLayer("Wall"))
        {
            //TODO: handle wall here
            touchingWall = true;
            grounded = false;
            wallPoint = hit.point;
            WallSlide();
        }
    }

    private void OnDrawGizmos()
    {
        //draw groundcheck raycast
        Debug.DrawLine(transform.position, transform.position  + finalDir * 3);
        //Debug.DrawRay(testVect, direc);
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
        print("call gravity");
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

        grounded = false;
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
        print("call ground movement");

        //make sure we are grounded
        if (!grounded)
        {
            print("return");
            return;
        }

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
        print("call friction");

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
        //gravity
        if (finalDir.y > -gravity * wallFallCoefficient)
            finalDir.y -= gravityAcceleration * wallFallCoefficient;
        finalDir.y = Mathf.Clamp(finalDir.y, -gravity * wallFallCoefficient, gravity);

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
        finalDir.x += transform.forward.x; finalDir.z += transform.forward.z;

        finalDir.x *= wallSpeedCoefficient;
        finalDir.z *= wallSpeedCoefficient;

        //account for wall direction
        finalDir.x += direc.x * wallPull; finalDir.z = direc.z * wallPull;

        Vector2 vec2FDir = new Vector2(finalDir.x, finalDir.z);
        if(vec2FDir.magnitude > finalWallSpeed)
        {
            vec2FDir = vec2FDir.normalized * finalWallSpeed;
            finalDir.x = vec2FDir.x; finalDir.z = vec2FDir.y;
        }
    }
    /*
     * Gets the normalized direction towards the wall thats currently being touched */
    private Vector3 GetDirToWall()
    {
        if (!touchingWall)
            return Vector3.zero;
        Vector3 returnVec = (wallPoint - transform.position).normalized;
        return returnVec;
    }
    //private Vector3 PlaceHolder()
    //{
    //    float forwardDot = Vector3.Dot(direc, transform.forward);
    //    float sideWaysDot = Vector3.Dot(direc, transform.right);
    //}
}
