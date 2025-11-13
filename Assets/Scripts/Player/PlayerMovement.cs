using JetBrains.Rider.Unity.Editor;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    private float moveSpeed;
    public float walkSpeed;
    public float sprintSpeed;

    public float groundDrag;

    [Header("Jumping")]
    public float jumpForce;
    public float jumpCooldown;
    public float airSpeedMultiplier;
    bool readyToJump;

    [Header("Crouching")]
    public float crouchSpeed;
    public float crouchYScale;
    private float startYScale;

    [Header("Keybinds")]
    public KeyCode jumpKey = KeyCode.Space;
    public KeyCode sprintKey = KeyCode.LeftShift;
    public KeyCode crouchKey = KeyCode.LeftControl;

    [Header("Ground Check")]
    public float playerHeight;
    public LayerMask whatIsGround;
    bool grounded;

    [Header("Slope handling")]
    public float maxSlopeAngle;
    private RaycastHit slopeHit;
    private bool exitingSlope;

    [Header("Floating")]
    public float RideHeight;
    public float RideSpringStrength;
    public float RideSpringDamper;
    private float groundRange = 1.4f;

    public Transform orientation;

    float horizontalInput;
    float verticalInput;

    Vector3 moveDirection;

    Rigidbody rb;
    CapsuleCollider col;

    public MovementState state;

    public enum MovementState
    {
        walking,
        sprinting,
        crouching,
        air
    }

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;

        readyToJump = true;

        startYScale = transform.localScale.y;
    }

    private void Update()
    {
        // Keeping grounded correct whether on ground or slope
        grounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.3f);

        MyInput(); // Handles my Inputs for Jumping and Crouching
        SpeedControl(); // Handles speed so the player maxs out instead of becoming the flash
        StateHandler(); // Keeps track of the state my player is in

        
        if (grounded) // If im touching ground make sure theres drag
        { rb.drag = groundDrag; }
        else 
        { rb.drag = 0.5f; }
    }

    private void FixedUpdate()
    {
        // Move the Player based on the inputs from MyInput()
        MovePlayer();

        ApplyFloating();
    }

    private void MyInput()
    {
        // getting inputs from wasd
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        // jump if on ground and cooldown ready
        if (Input.GetKey(jumpKey) && readyToJump && grounded)
        {
            readyToJump = false;

            Jump();

            Invoke(nameof(ResetJump), jumpCooldown);
        }

        // start crouching
        if (Input.GetKeyDown(crouchKey))
        {
            transform.localScale = new Vector3(transform.localScale.x, crouchYScale, transform.localScale.z);
            rb.AddForce(Vector3.down * 5f, ForceMode.Impulse);
            RideHeight = RideHeight / 2;
        }

        // stop crouching
        if (Input.GetKeyUp(crouchKey))
        {
            transform.localScale = new Vector3(transform.localScale.x, startYScale, transform.localScale.z);
            RideHeight = RideHeight * 2;
        }
    }

    private void StateHandler()
    { 
        // changes state to the respective MovementState
        if (Input.GetKey(sprintKey))
        {
            state = MovementState.sprinting;
            moveSpeed = sprintSpeed;
        }
        else if (grounded && Input.GetKey(crouchKey))
        {
            state = MovementState.crouching;
            moveSpeed = crouchSpeed;
        }
        else if (grounded)
        {
            state = MovementState.walking;
            moveSpeed = walkSpeed;
        }
        else
        {
            state = MovementState.air;
        }
    }

    private void MovePlayer()
    {
        Vector3 flatForward = orientation.forward;
        flatForward.y = 0;

        moveDirection = flatForward * verticalInput + orientation.right * horizontalInput;

        if (OnSlope() && !exitingSlope)
        {
            Vector3 slopeMoveDirection = GetSlopeMoveDirection();

            // Adjust movement force based on slope angle
            float slopeAngle = Vector3.Angle(Vector3.up, slopeHit.normal);
            float slopeModifier = Mathf.Lerp(1f, 0.5f, slopeAngle / maxSlopeAngle);

            rb.AddForce(slopeMoveDirection * moveSpeed * slopeModifier * 10f, ForceMode.Force);

            if (rb.velocity.y > 0)
            {
                rb.AddForce(Vector3.down * 20f, ForceMode.Force); // Adding a downward force to keep the player grounded
            }
        }
        else if (grounded)
        {
            rb.AddForce(moveDirection.normalized * moveSpeed * 10f, ForceMode.Force);
        }
        else if (!grounded)
        {
            rb.AddForce(moveDirection.normalized * moveSpeed * airSpeedMultiplier * 10f, ForceMode.Force);
        }

    }

    // caps off the speed
    private void SpeedControl()
    {
        if (OnSlope() && !exitingSlope)
        {
            if (rb.velocity.magnitude > moveSpeed)
            {
                rb.velocity = rb.velocity.normalized * moveSpeed;
            }
        }
        else
        {
            Vector3 flatVel = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

            // limit velocity to the max
            if (flatVel.magnitude > moveSpeed)
            {
                Vector3 limitedVel = flatVel.normalized * moveSpeed;
                rb.velocity = new Vector3(limitedVel.x, rb.velocity.y, limitedVel.z);
            }
        }
    }

    // jumps
    private void Jump() {
        exitingSlope = true;
        // reset y value so that player on the ground
        rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
        rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
    }

    private void ResetJump() {
        readyToJump = true;
        exitingSlope = false;
    }

    // check whether we are on slope
    private bool OnSlope() {
        if (Physics.Raycast(transform.position, Vector3.down, out slopeHit, playerHeight * .5f + .3f))
        {
            float angle = Vector3.Angle(Vector3.up, slopeHit.normal);
            return angle < maxSlopeAngle && angle != 0;
        } 
        return false;
    }

    private Vector3 GetSlopeMoveDirection() {
        return Vector3.ProjectOnPlane(moveDirection, slopeHit.normal).normalized;
    }

    void ApplyFloating()
    {
        RaycastHit hit;

        // Perform the raycast
        if (Physics.Raycast(transform.position, Vector3.down, out hit, groundRange))
        {
            rb.useGravity = false;
            Vector3 vel = rb.velocity;
            Vector3 rayDir = -Vector3.up;

            Vector3 otherVel = hit.rigidbody != null ? hit.rigidbody.velocity : Vector3.zero;
            Rigidbody hitBody = hit.rigidbody;
            if (hitBody != null)
            {
                otherVel = hitBody.velocity;
            }

            float rayDirVel = Vector3.Dot(rayDir, vel);
            float otherDirVel = Vector3.Dot(rayDir, otherVel);

            float relVel = rayDirVel - otherDirVel;

            float x = hit.distance - RideHeight;

            // Calculate spring force
            float springForce = (x * RideSpringStrength) - (relVel * RideSpringDamper);

           


            // Apply force to the player
            rb.AddForce(rayDir * springForce);

            // Apply an equal but opposite force to the hit rigidbody (if any)
            if (hit.rigidbody != null)
            {
                hit.rigidbody.AddForceAtPosition(rayDir * -springForce, hit.point);
            }
        }
        else
        {
            rb.useGravity = true;
        }
    }
}




