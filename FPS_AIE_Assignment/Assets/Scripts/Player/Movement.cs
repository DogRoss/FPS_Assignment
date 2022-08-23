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
    [Tooltip("Wall Gravity Coefficient: Speed at which the Player falls while sliding on the wall.")]
    public float WGCoefficient = 0.1f;
    [Tooltip("Max Wall Gravity Coefficient: max speed the player can travel down on the Y Axis")]
    public float MWGCoefficient = .5f;
    [Tooltip("Wall Acceleration Coefficient: put desc here.")]
    public float WACoefficient = 1f;
    [Tooltip("Max Wall Speed Coefficient: put desc here.")]
    public float MWSCoefficient = 1f;
    public float wallJumpForceX = 2f;
    public float wallJumpForceY = 5f;
    [Tooltip("Coefficient of forces that act against the Player when in motion against a wall.")]
    [Range(0, 1)]
    public float wallFrictionCoefficient = 0.1f;

    private ControllerColliderHit wallPointHit;


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
    //Applied
    private Vector3 moveVec = Vector3.zero;
    private Vector3 playerInputDirec = Vector3.zero;

    Vector3 vec;
    Vector3 wallThing = Vector3.zero;

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
            moveVec = ((GroundMovement() - GroundFriction()) + (Vector3.up * currentVerticalSpeed));
            controller.Move(moveVec * Time.deltaTime);
        }
        else if(!touchingWall)
        {
            //account for gravity
            currentVerticalSpeed -= gravity;

            moveVec = (AirMovement() - AirDrag()) + (Vector3.up * currentVerticalSpeed);
            controller.Move(moveVec * Time.deltaTime);
        }
        else
        {
            //is touching wall, so account for wall slide
            WallSlide();
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
        }
        else if (!grounded && hit.gameObject.layer == LayerMask.NameToLayer("Wall"))
        {
            touchingWall = true;
            grounded = false;
            wallPointHit = hit;
        }
    }

    private void OnDrawGizmos()
    {
        //draw movement vector
        if(controller && controller.velocity != null)
            Debug.DrawLine(transform.position, transform.position + controller.velocity.normalized * 3);

        Debug.DrawLine(transform.position, transform.position + vec, Color.blue);
        Debug.DrawLine(transform.position, transform.position + playerInputDirec, Color.green);
        if(wallPointHit != null)
            Debug.DrawLine(wallPointHit.point, wallPointHit.point + wallThing, Color.magenta);
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

        /*
         * first, set gravity based on wall values
         * find direction towards wall
         * check which way the player is facing in relation to wall using Dot Product
         * using Dot Product, find which way the player will slide along the wall
         * after finding direction, keep magnitude and apply it to slide direction
         * apply wall gravity
         */

        //calculate gravity on wall
        if (currentVerticalSpeed > -gravity * MWGCoefficient)
            currentVerticalSpeed = controller.velocity.y - (gravity * WGCoefficient);
        currentVerticalSpeed = Mathf.Clamp(currentVerticalSpeed, -gravity * MWGCoefficient, Mathf.Infinity);

        //get the direction towards the wall that was hit
        Vector3 wallForward = Vector3.Cross(wallPointHit.normal, Vector3.up); //use this to apply forces
        wallThing = wallForward;
        //store input in relation to where the player is facing
        playerInputDirec = transform.forward * direction.z + transform.right * direction.x;
        playerInputDirec.y = 0;

        //see which way in relation to wall the player inputs towards
        //the closer the dot is to 0 means more perpendicular
        //the closer the dot is to -1/1 means either facing same way or opposite way
        float inputDot = Vector3.Dot(wallForward, playerInputDirec);

        Debug.Log(inputDot + " Input doooooot");
        //take input dot and smoothly accelerate towards that direction

        if(inputDot > 0)
        {
            //move towards one side
            moveVec = wallForward.normalized * (moveVec.x + moveVec.z);
            if(CurrentHorizontalSpeed < maxSpeed * MWSCoefficient)
                moveVec += moveVec.normalized * WACoefficient;
            else
                moveVec = moveVec.normalized * (maxSpeed * MWSCoefficient);
        }
        if(inputDot < 0)
        {
            //move towards the other
            moveVec = wallForward.normalized * (moveVec.x + moveVec.z);
            if (CurrentHorizontalSpeed < maxSpeed * MWSCoefficient)
                moveVec += moveVec.normalized * WACoefficient;
            else
                moveVec = moveVec.normalized * (maxSpeed * MWSCoefficient);
        }
    }
    //private Vector3 WallSlide()
    //{

    //    /*
    //     * first, set gravity based on wall values
    //     * find direction towards wall
    //     * check which way the player is facing in relation to wall using Dot Product
    //     * using Dot Product, find which way the player will slide along the wall
    //     * after finding direction, keep magnitude and apply it to slide direction
    //     * apply wall gravity
    //     */

    //    //calculate gravity on wall
    //    if (currentVerticalSpeed > -gravity * MWGCoefficient)
    //        currentVerticalSpeed = controller.velocity.y - (gravity * WGCoefficient);
    //    currentVerticalSpeed = Mathf.Clamp(currentVerticalSpeed, -gravity * MWGCoefficient, Mathf.Infinity);

    //    //get the direction towards the wall that was hit
    //    Vector3 wallDir = (wallPoint - transform.position).normalized;
    //    //store input in relation to where the player is facing
    //    playerInputDirec = transform.forward * direction.z + transform.right * direction.x;

    //    //test to see how close to which side one is
    //    //the closer the dot is to 0 means more perpendicular
    //    //the closer the dot is to -1/1 means either facing same way or opposite way
    //    float sideWaysDot = Vector3.Dot(transform.right, wallDir);
    //    float inputDot = Vector3.Dot(playerInputDirec, wallDir);

    //    Debug.Log(inputDot + " Input doooooot");

    //    //if facing left of wall
    //    if (sideWaysDot > 0)
    //    {
    //        wallDir = Vector3.Cross(wallDir, Vector3.up).normalized;

    //    }
    //    //if facing right of wall
    //    if (sideWaysDot < 0)
    //    {
    //        wallDir = -Vector3.Cross(wallDir, Vector3.up).normalized;

    //    }
    //    moveVec = wallDir.normalized * moveVec.magnitude;
    //    wallDir *= WACoefficient * sideWaysDot;
    //    vec = wallDir;

    //    return wallDir;
    //}
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
