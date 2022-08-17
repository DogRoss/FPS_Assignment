using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class Movement : MonoBehaviour
{
    CharacterController controller;

    [Header("Ground/Wall Movement Values")]
    public ForceMode groundForceType;
    [Tooltip("Acceleration of player when on ground, accelerates differently based on what 'Ground Force Type' is applied.")]
    public float groundAcceleration;
    [Tooltip("Max speed player can reach when touching the ground.")]
    public float maxSpeed;
    [Tooltip("speed at which the player falls while sliding on the wall")]
    public float wallFallMultiplier;
    public float wallSpeed;

    [Header("Air Movement Values")]
    public ForceMode2D airForceType;
    [Tooltip("Acceleration of player when in the air, accelerates differently based on what 'Air Force Type' is applied.")]
    public float airAcceleration;
    [Tooltip("Multiplied by 'Max Speed' variable to find how fast the player can move in the air.")]
    public float airMaxSpeedCoefficient;

    private Vector3 direction;

    [Header("Jump Values")]
    public ForceMode jumpForceType;
    public float jumpForce;
    public float fallMultiplier;

    private bool jumpHeld;

    [Header("Ground Values")]
    public LayerMask groundLayers;
    [Tooltip("How far down ground check raycast goes from set position.")]
    public float groundCheckDistance;
    [Tooltip("How far from original position ground check raycast check is.")]
    public float groundCheckYOffset;

    private bool grounded;
    private bool touchingWall;
    private float currentGroundFriction;
    private RaycastHit hit;
    private Ray ray;

    //--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
    // Built In Engine Functions
    //--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
    void Start()
    {
        controller = GetComponent<CharacterController>();
    }
    void Update()
    {
        Debug.Log(direction);
    }
    private void FixedUpdate()
    {
        //handle movement and ground detection
        //if (Physics.Raycast(transform.position + (transform.up * groundCheckOffset), -transform.up, groundCheckDistance, groundLayers))
        ray.origin = transform.position + (transform.up * groundCheckYOffset);
        ray.direction = -transform.up;
        if (Physics.Raycast(ray, out hit, groundCheckDistance, groundLayers))
        {
            grounded = true;

            ////movement application
            //if (rb.velocity.magnitude < maxSpeed)
            //    rb.AddForce(direction * groundAcceleration, groundForceType);
            //else
            //    rb.velocity = rb.velocity.normalized * maxSpeed;
        }
        //else
        //{
        //    grounded = false;
        //    currentGroundFriction = 0.01f;
        //    if (rb.velocity.magnitude < (maxSpeed * airMaxSpeedCoefficient))
        //        rb.AddForce(direction * (airAcceleration), airForceType);
        //    else
        //        rb.velocity = rb.velocity.normalized * (maxSpeed * airMaxSpeedCoefficient);
        //}


        ////handle friction
        //if (grounded)
        //{
        //    if (rb.velocity.magnitude > 0.1f)
        //    {
        //        rb.AddForce(-(rb.velocity.normalized * currentGroundFriction), ForceMode2D.Force);
        //    }
        //    else
        //        rb.velocity = Vector3.zero;
        //}
        //else
        //{
        //    AirPhysics();
        //}
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
        ////draw groundcheck raycast
        //Debug.DrawLine(transform.position + (transform.up * groundCheckOffset), (transform.position + (transform.up * groundCheckOffset)) - (transform.up * groundCheckDistance), Color.red);
    }
    //--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
    // Input System Functions
    //--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
    private void OnMove(InputValue value)
    {
        direction.x = value.Get<Vector2>().x;
        direction.z = value.Get<Vector2>().y;
    }
    private void OnJump(InputValue value)
    {
        //if (value.Get<float>() > 0)
        //{
        //    jumpHeld = true;
        //    downHeld = false;
        //    if (grounded)
        //    {
        //        rb.AddForce(transform.up * jumpForce, jumpForceType);
        //    }
        //}
        //else if (value.Get<float>() == 0)
        //{
        //    jumpHeld = false;
        //    downHeld = false;
        //}
        //else if (value.Get<float>() < 0 && !grounded)
        //{
        //    rb.AddForce(-transform.up * jumpForce, jumpForceType);
        //    downHeld = true;
        //}
    }

    //--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
    // Player Functions
    //--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

    private void AirPhysics()
    {
        //if (rb.velocity.y < 0 && !touchingWall)
        //{
        //    rb.velocity += Vector2.up * Physics2D.gravity.y * fallMultiplier * Time.deltaTime;
        //}
        //else if (rb.velocity.y > 0 && !jumpHeld && !touchingWall)
        //{
        //    rb.velocity += Vector2.up * Physics2D.gravity.y * fallMultiplier * Time.deltaTime;
        //}
        //else if (rb.velocity.y < 0 && touchingWall)
        //{
        //    rb.velocity += Vector2.up * Physics2D.gravity.y * wallSlideMultiplier * Time.deltaTime;
        //}
    }


}
