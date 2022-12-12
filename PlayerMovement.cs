using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]

    private float moveSpeed;
    public float walkSpeed;
    public float sprintSpeed;
    public float slideSpeed;
    public float wallRunSpeed;

    public float groundDrag;

    private float desiredMoveSpeed;
    private float lastDesiredMoveSpeed;

    [Header("Jumping")]

    public float jumpForce;
    public float jumpCooldown;
    public float airMultiplier;
    private bool readyToJump;
    public int maxJumps;
    public int numJumps;
    public float jumpMultiplier;


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
    private bool grounded;


    [Header("Slope Handling")]

    public float maxSlopeAngle;
    private RaycastHit slopeHit;
    private bool onSlope;
    private bool exitingSlope;

    
    [Header("Wallrunning Handling")]

    public float playerWidth;
    public LayerMask whatIsWall;

    private RaycastHit wallHit;


    [Header("Orientation")]

    public Transform orientation;


    [Header("Sliding")]

    public KeyCode slideKey = KeyCode.Tab;
    public float speedIncreaseMultiplier;
    public float slopeIncreaseMultiplier;


    [Header("Jump Pad")]

    public LayerMask whatIsJumpPad;
    public bool jumpPad;
    public float jumpPadMultiplier;

    [Header("AA")]
    
    public LayerMask whatIsWin;


    // other private values used by the movement script
    float horizontalInput;
    float verticalInput;

    Vector3 moveDirection;
    Rigidbody rb;


    public MovementState state;
    public enum MovementState
    {
        walking,
        sprinting,
        crouching,
        wallrunning,
        sliding,
        air
    }

    public bool sliding;
    public bool wallRunning;


    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        readyToJump = true;
        startYScale = transform.localScale.y;
    }

    private void Update()
    {
        GroundCheck();
        onSlope = OnSlope();
        MyInput();
        SpeedControl();
        StateHandler();
        HandleDrag();
    }

    private void FixedUpdate()
    {
        MovePlayer();
        if (state == MovementState.air) {
            rb.AddForce(Physics.gravity, ForceMode.Acceleration);
        }

        if (Physics.Raycast(transform.position, Vector3.down, playerHeight + 0.3f, whatIsWin)) SceneManager.LoadScene(sceneName: "SettingsScene");
    }

    private void MyInput()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        if (Input.GetKey(jumpKey) && readyToJump && (grounded || numJumps < maxJumps) || jumpPad) {
            Jump();
            Invoke(nameof(ResetJump), jumpCooldown);
        }

        if (Input.GetKeyDown(crouchKey))
        {
            transform.localScale = new Vector3(transform.localScale.x, crouchYScale, transform.localScale.z);
            rb.AddForce(Vector3.down * 2f, ForceMode.Impulse);
        }
        
        if (Input.GetKeyUp(crouchKey))
        {
            transform.localScale = new Vector3(transform.localScale.x, startYScale, transform.localScale.z);
        }
    }

    private void StateHandler()
    {
        // Mode - WallRunning
        if (wallRunning) {
            state = MovementState.wallrunning;
            desiredMoveSpeed = wallRunSpeed;
        }

        // Mode - Sliding
        else if (sliding)
        {
            state = MovementState.sliding;
            if (OnSlope() && rb.velocity.y < 0.1f)
                desiredMoveSpeed = slideSpeed;

            else
                desiredMoveSpeed = sprintSpeed;
        } 

        // Mode - Crouching
        else if (Input.GetKey(crouchKey))
        {
            state = MovementState.crouching;
            desiredMoveSpeed = crouchSpeed;
        }

        // Mode - Sprinting
        else if (grounded && Input.GetKey(sprintKey))
        {
            state = MovementState.sprinting;
            desiredMoveSpeed = sprintSpeed;
        } 

        // Mode - Walking
        else if (grounded)
        {
            state = MovementState.walking;
            desiredMoveSpeed = walkSpeed;
        }

        // Mode - Air
        else
        {
            state = MovementState.air;
        }

        // check if desiredMoveSpeed has changed drastically
        if(Mathf.Abs(desiredMoveSpeed - lastDesiredMoveSpeed) > 4f && moveSpeed != 0)
        {
            StopAllCoroutines();
            StartCoroutine(SmoothlyLerpMoveSpeed());
        }
        else
        {
            moveSpeed = desiredMoveSpeed;
        }

        lastDesiredMoveSpeed = desiredMoveSpeed;
    }

    private IEnumerator SmoothlyLerpMoveSpeed()
    {
        // smoothly lerp movementSpeed to desired value
        float time = 0;
        float difference = Mathf.Abs(desiredMoveSpeed - moveSpeed);
        float startValue = moveSpeed;

        while (time < difference)
        {
            moveSpeed = Mathf.Lerp(startValue, desiredMoveSpeed, time / difference);

            if (OnSlope())
            {
                float slopeAngle = Vector3.Angle(Vector3.up, slopeHit.normal);
                float slopeAngleIncrease = 1 + (slopeAngle / 90f);

                time += Time.deltaTime * speedIncreaseMultiplier * slopeIncreaseMultiplier * slopeAngleIncrease;
            }
            else
                time += Time.deltaTime * speedIncreaseMultiplier;

            yield return null;
        }

        moveSpeed = desiredMoveSpeed;
    }

    private void MovePlayer()
    {
        float multiplyer;

        // calculates movement direction so that you always move in the direction you are looking
        moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;

        if (grounded) {
            multiplyer = 1f;
        } else {
            multiplyer = airMultiplier;
        }

        if (OnSlope() && !exitingSlope)
        {
            rb.AddForce(GetSlopeMoveDirection(moveDirection) * moveSpeed * 20f, ForceMode.Force);

            if (rb.velocity.y > 0)
                rb.AddForce(Vector3.down * 80f, ForceMode.Force);
        }
        
        else 
        {
            rb.AddForce(moveDirection.normalized * moveSpeed * 10f * multiplyer, ForceMode.Force);
        }

        if (onSlope) {
            rb.useGravity = false;
        } else {
            rb.useGravity = true;
        }
    }

    public Vector3 GetSlopeMoveDirection(Vector3 direction) {
        return Vector3.ProjectOnPlane(direction, slopeHit.normal).normalized;
    }

    private void HandleDrag() 
    {
        if (!grounded) {
            rb.drag = 1;
            return;
        }
        rb.drag = groundDrag;
    }

    private void SpeedControl()
    {
        // Limiting speed on slope
        if (onSlope && !exitingSlope)
        {
            if (rb.velocity.magnitude > moveSpeed)
                rb.velocity = rb.velocity.normalized * moveSpeed;

            return;
        }

        Vector3 flatVel = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

        if (flatVel.magnitude > moveSpeed) 
        {
            Vector3 limitedVel = flatVel.normalized * moveSpeed;
            rb.velocity = new Vector3(limitedVel.x, rb.velocity.y, limitedVel.z);
        }
    }
       
    private void Jump()
    {
        exitingSlope = true;
        readyToJump = false;
        numJumps += 1;

        if (state == MovementState.wallrunning)
        {
            numJumps -= 1;
        }

        // reset y velocity to 0, making jumps go equally high each time
        rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
        rb.AddForce(transform.up * jumpForce * jumpMultiplier, ForceMode.Impulse);
    }

    private void ResetJump()
    {
        readyToJump = true;
        exitingSlope = false;
    }

    private void GroundCheck()
    {

        if (Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.2f, whatIsJumpPad)) 
        {
            numJumps = 0;
            grounded = true;
            jumpPad = true;
            jumpMultiplier = jumpPadMultiplier;
        }
        
        else if (Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.2f, whatIsGround)) 
        {
            numJumps = 0;
            grounded = true;
            jumpMultiplier = 1f;
            jumpPad = false;

        } 
        
        
        
        else {
            grounded = false;
            jumpMultiplier = 1f;
            jumpPad = false;
        }
    }

    public bool OnSlope()
    {
        if (Physics.Raycast(transform.position, Vector3.down, out slopeHit, playerHeight * 0.5f + 0.3f))
        {
            float angle = Vector3.Angle(Vector3.up, slopeHit.normal);
            return angle < maxSlopeAngle && angle != 0;
        }

        return false;
    }
}
