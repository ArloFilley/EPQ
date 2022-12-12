using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallRunning : MonoBehaviour
{

    [Header("Wallrunning")]

    public LayerMask whatIsWall;
    public LayerMask whatIsGround;
    public float wallRunForce;
    public float maxWallRunTime;
    public float wallJumpUpForce;
    public float wallJumpSideForce;
    public KeyCode wallJumpKey = KeyCode.Space;

    private float wallRunTimer;


    [Header("Exiting")]
    private bool exitingWall;
    public float exitWallTime; 
    public float exitWallTimer;


    [Header("Input")]
    
    private float horizontalInput;
    private float verticalInput;


    [Header("Detection")]
    
    public float wallCheckDistance;
    public float minJumpHeight;
    private RaycastHit leftWallhit;
    private RaycastHit rightWallhit;
    private bool wallLeft;
    private bool wallRight;


    [Header("References")]
    public Transform orientation;
    public PlayerCam cam;
    private PlayerMovement pm;
    private Rigidbody rb;

    // Start is called before the first frame update
    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        pm = GetComponent<PlayerMovement>();
        cam = GetComponent<PlayerCam>();
    }

    private void Update()
    {
        WallCheck();
        StateMachine();
    }

    private void FixedUpdate()
    {
        if (pm.wallRunning) WallRunningMovement();
    }

    private void WallCheck()
    {
        // These Raycasts check if there is a wall within wallCheckDistance of the player
        // And store it in the appropriate variables

        wallRight = Physics.Raycast(transform.position, orientation.right, out rightWallhit, wallCheckDistance, whatIsWall);
        wallLeft = Physics.Raycast(transform.position, -orientation.right, out leftWallhit, wallCheckDistance, whatIsWall);
    }

    private bool AboveGround()
    {
        // Checks that the player is a suitable height above the ground
        // Such that they can start wall running

        return !Physics.Raycast(transform.position, Vector3.down, minJumpHeight, whatIsGround);
    }

    private void StateMachine()
    {
        // Getting Inputs for horizontal and vertical

        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        // State 1 - Wallrunning
        if ((wallLeft || wallRight) && verticalInput > 0f && AboveGround() && !exitingWall)
        {
            if (!pm.wallRunning) StartWallRun();

            // wallrun timer
            if (wallRunTimer > 0) wallRunTimer -= Time.deltaTime;

            if (wallRunTimer <= 0 && pm.wallRunning)
            {
                exitingWall = true;
                exitWallTimer = exitWallTime;
            }

            if (Input.GetKeyDown(wallJumpKey)) WallJump();
        }

        // State 2 - WallJumping off wall
        else if (exitingWall)
        {
            if (pm.wallRunning) StopWallRun();

            if (exitWallTimer > 0) exitWallTimer -= Time.deltaTime;

            if (exitWallTimer <= 0) exitingWall = false;
        }

        // State 3 - None
        else
        {
            if (pm.wallRunning) StopWallRun();
        }

    }

    private void StartWallRun()
    {
        pm.wallRunning = true;

        wallRunTimer = maxWallRunTime;
        cam.DoFOV(90f);
    }

    private void WallRunningMovement()
    {
        // turns gravity off
        rb.useGravity = false;
        rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

        // calculates the direction of the wall so that the player moves along it correctly
        Vector3 wallNormal = wallRight ? rightWallhit.normal : leftWallhit.normal;
        Vector3 wallForward = Vector3.Cross(wallNormal, transform.up);

        // makes sure the player is wallrunning in the direction they are looking
        if ((orientation.forward - wallForward).magnitude > (orientation.forward + wallForward).magnitude)
            wallForward = -wallForward;

        // forward force to make the player move forward
        rb.AddForce(wallForward * wallRunForce, ForceMode.Force);

        // pushes the player to the wall 
        if (!(wallLeft && horizontalInput > 0) && !(wallRight && horizontalInput < 0))
            rb.AddForce(-wallNormal * 10f, ForceMode.Force);
    }

    private void StopWallRun()
    {
        pm.wallRunning = false;
        cam.DoFOV(60f);
    }

    private void WallJump()
    {
        // enter exiting wall state
        exitingWall = true;
        exitWallTimer = exitWallTime;

        Vector3 wallNormal = wallRight ? rightWallhit.normal : leftWallhit.normal;

        Vector3 forceToApply = transform.up * wallJumpUpForce + wallNormal * wallJumpSideForce;

        rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
        rb.AddForce(forceToApply, ForceMode.Impulse);
    }
}
