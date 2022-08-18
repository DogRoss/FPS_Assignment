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
    [Tooltip("speed at which the player falls while sliding on the wall")]
    public float wallFallMultiplier = 0.5f;
    public float wallSpeed = 20f;

    private float finalGroundSpeed;

    [Header("Air Movement Values")]
    public float dragCoefficient = .25f;
    public float gravity = 0.5f;
    public float gravityCoefficient = 1.5f;

    [Header("Jump Values")]
    public float jumpForce = 10f;
    public float jumpGravityCoefficient = 1f;

    private float jumpPower = 0f;
    private bool jumpHeld;

    [Header("Ground Values")]
    public LayerMask groundLayers;
    [Tooltip("How far down ground check raycast goes from set position.")]
    public float groundCheckDistance = 1f;
    [Tooltip("How far from original position ground check raycast check is.")]
    public float groundCheckYOffset = 0f;

    //Raycast/Ground Check
    private bool grounded;
    private bool touchingWall;
    private float currentGroundFriction;
    private RaycastHit hit;
    private Ray ray;

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
        //handle movement and ground detection
        ray.origin = transform.position + (transform.up * groundCheckYOffset);
        ray.direction = -transform.up;
        if (Physics.Raycast(ray, out hit, groundCheckDistance, groundLayers))
        {
            grounded = true;
            if (!jumpHeld)
                finalDir.y = 0;

            if (direction.magnitude > 0)
            {
                xAxis = direction.x * finalGroundSpeed;
                zAxis = direction.z * finalGroundSpeed;

                Vector3 preDir = transform.right * xAxis + transform.forward * zAxis;
                finalDir.x = preDir.x; finalDir.z = preDir.z;
            }
            else
            {
                finalDir.x = 0;
                finalDir.z = 0;
            }
        }
        else if (!jumpHeld)
        {
            grounded = false;
            currentGroundFriction = 0.01f;
            AirPhysics();
            //finalDir = AirDirection();
        }

        controller.Move(finalDir);

        /*
        //print(finalDir);

        //handle friction
        if (grounded)
        {
            if (rb.velocity.magnitude > 0.1f)
            {
                rb.AddForce(-(rb.velocity.normalized * currentGroundFriction), ForceMode2D.Force);
            }
            else
                rb.velocity = Vector3.zero;
        }
        else
        {
            AirPhysics();
        }
        */
    }
    private void OnCollisionEnter(Collision collision)
    {
        //if (collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
        //{
        //    //cache friction/bounce here for equating
        //    Ground ground = collision.transform.GetComponent<Ground>();
        //    currentGroundFriction = ground.friction;
        //}

        if (collision.gameObject.layer == LayerMask.NameToLayer("Wall"))
        {
            touchingWall = true;
            Debug.Log("Hit wall");
        }
    }
    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Wall"))
        {
            Debug.Log("shouldnt this work?");
            touchingWall = false;
        }
    }
    private void OnDrawGizmos()
    {
        //draw groundcheck raycast
        Debug.DrawLine(transform.position + (transform.up * groundCheckYOffset), (transform.position + (transform.up * groundCheckYOffset)) - (transform.up * groundCheckDistance), Color.red);
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
            StartCoroutine(Jump());
            Debug.Log("held");
        }
        else if(value.Get<float>() == 0)
        {
            StopCoroutine(Jump());
            jumpHeld = false;
            Debug.Log("release");
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

    private Vector3 ApplyFriction()
    {
        return Vector3.zero;
    }
    private void HandleCamera()
    {
        Vector3 axis = mouseVector;
        axis.y = 0;
        cam.transform.Rotate(axis, 1);

        axis.y = mouseVector.y;
        axis.x = 0;
        transform.Rotate(axis, 1);
    }
    private void AirPhysics()
    {
        if(finalDir.y > -gravity)
        {
            print("AirPhysics - if greater than gravity");
            finalDir.y -= gravity * gravityCoefficient;
            finalDir.y = -gravity;
        }
        else
        {
            print("AirPhysics - else");
            finalDir.y = -gravity;
        }
    }
    private Vector3 AirDirection()
    {
        Vector3 airDir = Vector3.zero;
        Vector3 currentDir = finalDir;

        if (direction.magnitude > 0)
        {
            airDir = currentDir * dragCoefficient;
            airDir.y = finalDir.y;
        }
        else
        {
            airDir = currentDir * dragCoefficient;
            airDir.y = finalDir.y;
        }

        return airDir;
    }
    private IEnumerator Jump()
    {
        print("start jump routine");
        jumpHeld = true;
        jumpPower = jumpForce;
        while(jumpPower > 0 && jumpHeld)
        {
            Debug.Log("call");
            jumpPower -= gravity * jumpGravityCoefficient; 
            finalDir.y = jumpPower;
            yield return null;
        }
        yield return null;
    }
}
