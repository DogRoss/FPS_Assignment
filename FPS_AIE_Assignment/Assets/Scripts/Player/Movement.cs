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
    [Tooltip("Rate at which Player gains speed.")]
    public float acceleration = 5f;
    [Tooltip("acceleration rate of DownForce the player will experience when in the air.")]
    public float gravity = 0.5f;

    private float currentVerticalSpeed = 0f;


    [Header("Ground Movement Values")]
    [Tooltip("Ground Acceleration Coefficient: multiplied by 'Acceleration' to get the acceleration of Player when touching the ground.")]
    public float GACoefficient = 1f;
    [Tooltip("Max Ground Speed Coefficient: multiplied by 'Max Speed' to find what speed the Player is able to reach.")]
    public float MGSCoefficient = 1f;
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
    private Vector3 mouseVector = Vector3.zero;

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
        HandleCamera();
    }
    private void FixedUpdate()
    {

        if (controller.collisionFlags != CollisionFlags.Sides)
            touchingWall = false;

        //apply forces to controller
        if (grounded)
        {
            Vector3 moveVec = ((GroundMovement() - GroundFriction()) + (Vector3.up * currentVerticalSpeed));
            controller.Move(moveVec * Time.deltaTime);
        }
        else
        {
            //account for gravity
            if (!touchingWall)
                currentVerticalSpeed -= gravity;

            Vector3 moveVec = (AirMovement() - AirDrag()) + (Vector3.up * currentVerticalSpeed);
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
        }
        else if (!grounded && hit.gameObject.layer == LayerMask.NameToLayer("Wall"))
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
        if(controller && controller.velocity != null)
            Debug.DrawLine(transform.position, transform.position + controller.velocity.normalized * 3);
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
    private Vector3 AirMovement()
    {
        /* grab current velocity, this will be the value that forces act against;
         * take directional input and account for transform.forward;
         * with direction, use that and air acceleration to act against the stored velocity;
         * return velocity; */

        //make sure we are grounded
        if (grounded)
        {
            print("return");
            return controller.velocity;
        }

        //store velocity
        Vector3 vel = controller.velocity;
        vel.y = 0;

        //get directional input and account for player forward
        Vector3 forces = transform.forward * direction.z + transform.right * direction.x;
        //get acceleration rate
        forces *= acceleration * AACoefficient;
        //apply forces
        vel += forces;

        //clamp to max speed
        if(vel.magnitude > maxSpeed * MASCoefficient)
            vel = vel.normalized * (maxSpeed * MASCoefficient);

        return vel;
    }

    /*
     * Handles what direction the player will move in based on being grounded and input direction */
    private Vector3 GroundMovement()
    {
        /* grab current velocity, this will be the value that forces act against;
         * take directional input and account for transform.forward;
         * with direction, use that and ground acceleration to act against the stored velocity;
         * return velocity; */

        //make sure we are grounded
        if (!grounded)
        {
            print("return");
            return controller.velocity;
        }
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

        //clamp to max speed
        if (vel.magnitude > maxSpeed * MGSCoefficient)
            vel = vel.normalized * (maxSpeed * MGSCoefficient);

        return vel;
    }


    /*
     * -v- Handles forces that oppose Player's movement on the ground */
    private Vector3 GroundFriction()
    {
        Vector3 counterForce = Vector3.zero;

        if (controller.velocity.x != 0)
            counterForce.x = controller.velocity.x * groundFrictionCoefficient;
        if (controller.velocity.z != 0)
            counterForce.z = controller.velocity.z * groundFrictionCoefficient;

        return counterForce;
    }
    /*
     * Handles Forces in air that oppose Player's movement through the air */
    private Vector3 AirDrag()
    {
        Vector3 counterForce = Vector3.zero;

        if (controller.velocity.magnitude > 0)
        {
            counterForce.x = controller.velocity.x * horizontalDragCoefficient;
            currentVerticalSpeed -= currentVerticalSpeed * verticalDragCoefficient;
            counterForce.z = controller.velocity.z * horizontalDragCoefficient;
        }

        return counterForce;
    }


    /*
     * Handles Jump Forces on player based on if the player is on a wall or on ground */
    private void Jump(bool wallJump)
    {
        if (wallJump)
            WallJump();
        else
            currentVerticalSpeed = jumpForce;

        grounded = false;
    }
    /*
     * Handles how the Player moves against the wall in a sliding motion */
    private void WallSlide()
    {
        ////gravity
        //if (currentVerticalSpeed > -gravity * wallFallCoefficient)
        //    currentVerticalSpeed -= gravity * wallFallCoefficient;
        //currentVerticalSpeed = Mathf.Clamp(currentVerticalSpeed, -gravity * wallFallCoefficient, gravity);

        ////get the direction towards the wall that was hit
        //direc = (wallPoint - transform.position).normalized;
        //float wallPull = 0.1f;
        //testVect = wallPoint;

        ////test to see how close to which side one is
        ////the closer the dot is to 0 means more perpendicular
        ////the closer the dot is to -1/1 means either facing same way or opposite way
        //float forwardDot = Vector3.Dot(direc, transform.forward);
        //float sideWaysDot = Vector3.Dot(direc, transform.right);
        ////Debug.Log(forwardDot + " forward // sideWays: " + sideWaysDot);

        //float wallSpeedCoefficient = Mathf.Abs(sideWaysDot);
        ////account for player forward
        //horizontalDir.x += transform.forward.x; horizontalDir.z += transform.forward.z;

        //horizontalDir.x *= wallSpeedCoefficient;
        //horizontalDir.z *= wallSpeedCoefficient;

        ////account for wall direction
        //horizontalDir.x += direc.x * wallPull; horizontalDir.z = direc.z * wallPull;

        //Vector2 vec2FDir = new Vector2(horizontalDir.x, horizontalDir.z);
        //if(vec2FDir.magnitude > (maxSpeed * MWSCoefficient))
        //{
        //    vec2FDir = vec2FDir.normalized * (maxSpeed * MWSCoefficient);
        //    horizontalDir.x = vec2FDir.x; horizontalDir.z = vec2FDir.y;
        //}
    }
    /*
     * Handles Jumping off wall */
    private void WallJump()
    {
        ////get the direction towards the wall that was hit
        //direc = (wallPoint - transform.position).normalized;

        ////add direction away from wall
        //horizontalDir.x = direc.x * -wallJumpForceX; horizontalDir.z = direc.z * -wallJumpForceX;
        //currentVerticalSpeed = wallJumpForceY;

    }

    
}
