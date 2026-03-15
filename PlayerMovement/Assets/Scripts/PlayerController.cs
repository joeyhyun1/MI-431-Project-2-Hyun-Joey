using System.Collections;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public States state;
    
    [Header("Movement")] 
    public float moveSpeed;
    public float jumpForce;
    public int jumpCount = 0;
    public int maxJumps = 1;
    private bool readyToJump;
    public float groundDrag;
    public float airMultiplier;
    
    [Header("Ground Check")] 
    public float playerHeight;
    public LayerMask groundLayer;
    public bool isGrounded;

    private Rigidbody rb;
    public Vector3 moveDirection;
    public Transform orientation;
    
    float horizontalInput;
    float verticalInput;

    [Header("Dash")]
    public float dashSpeed = 20f;
    public float dashDuration = 0.2f;
    public bool canDash = true;
    private bool isDashing = false;

    [Header("Wall Running")]
    public float wallRunForce = 200f;
    public float wallCheckDistance = 0.7f;
    public float minJumpHeight = 1.5f;
    public float wallJumpCooldown = 0.5f;
    public LayerMask whatIsWall;
    private bool wallLeft;
    private bool wallRight;
    private RaycastHit leftWallHit;
    private RaycastHit rightWallHit;
    public bool isWallRunning;
    private bool canWallRun = true;


    [Header("Slope Handling")]
    public float maxSlopeAngle = 40f;
    private RaycastHit slopeHit;

    public enum States
    {
        Running,
        Jumping,
        Dashing,
        WallRiding,
    }

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
    }

    private void FixedUpdate()
    {
        if (state == States.WallRiding)
            WallRunMovement();
        else
            MovePlayer();
    }

    private void Update()
    {
        CheckForWall();
        StateHandler();
        isGrounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.2f, groundLayer);
        
        MyInput();
        SpeedControl();

        if (isGrounded)
        {
            rb.linearDamping = groundDrag;
            jumpCount = 0;
            canDash = true;
        }
        else
        {
            rb.linearDamping = 0;
        }

        if (Input.GetButtonDown("Jump") && jumpCount < maxJumps)
        {
            readyToJump = false;
            Jump();
        }
    }

    private void StateHandler()
    {

        if ((wallLeft || wallRight) && verticalInput > 0 && AboveGround() && canWallRun)
        {
            if (!isWallRunning)
                StartWallRun();
            state = States.WallRiding;
        }
        else if (isGrounded)
        {
            state = States.Running;
            StopWallRun();
        }
        else
        {
            state = States.Jumping;
            StopWallRun();
        }

        if (Input.GetKeyDown(KeyCode.LeftShift) && canDash)
        {
            state = States.Dashing;
            StartCoroutine(Dash());
        }
    }

    private void MyInput()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");
    }

    private void MovePlayer()
    {
        if (isDashing) return;

        moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;
        
    if (OnSlope())
        {
            Vector3 slopeMoveDir = GetSlopeMoveDirection();
            float slopeMultiplier = slopeMoveDir.y < 0 ? 2.5f : 1.5f;
            rb.AddForce(slopeMoveDir * moveSpeed * 15f * slopeMultiplier, ForceMode.Force);
        }
        else if (isGrounded)
            rb.AddForce(moveDirection.normalized * moveSpeed * 10f, ForceMode.Force);
        else if (!isGrounded)
            rb.AddForce(moveDirection.normalized * moveSpeed * 10f * airMultiplier, ForceMode.Force);

    }

    private void SpeedControl()
    {
        if (isWallRunning) return;

        if (OnSlope())
        {
            float slopeSpeedLimit = GetSlopeMoveDirection().y < 0 ? moveSpeed * 2f : moveSpeed * 0.6f;
            if (rb.linearVelocity.magnitude > slopeSpeedLimit)
                rb.linearVelocity = rb.linearVelocity.normalized * slopeSpeedLimit;
        }
        else
        {
            Vector3 flatVel = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
            if (flatVel.magnitude > moveSpeed)
            {
                Vector3 limitedVel = flatVel.normalized * moveSpeed;
                rb.linearVelocity = new Vector3(limitedVel.x, rb.linearVelocity.y, limitedVel.z);
            }
        }
    }

    private void Jump()
    {
        if (isWallRunning)
        {
            Vector3 wallNormal = wallRight ? rightWallHit.normal : leftWallHit.normal;
            StopWallRun();
            StartCoroutine(WallJumpCooldown());
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
            rb.AddForce(wallNormal * 5f + Vector3.up * jumpForce, ForceMode.Impulse);
            return;
        }

        jumpCount++;
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
        rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
    }

    private IEnumerator WallJumpCooldown()
    {
        canWallRun = false;
        yield return new WaitForSeconds(wallJumpCooldown);
        canWallRun = true;
    }

    private IEnumerator Dash()
    {
        isDashing = true;
        canDash = false;

        float originalSpeed = moveSpeed;
        moveSpeed = dashSpeed;

        bool originalGravity = rb.useGravity;
        rb.useGravity = false;

        rb.linearVelocity = moveDirection.normalized * dashSpeed;

        yield return new WaitForSeconds(dashDuration);

        rb.useGravity = originalGravity;
        moveSpeed = originalSpeed;
        rb.linearVelocity = Vector3.zero;
        isDashing = false;
    }

    private void CheckForWall()
    {
        wallRight = Physics.Raycast(transform.position, orientation.right, out rightWallHit, wallCheckDistance, whatIsWall);
        wallLeft = Physics.Raycast(transform.position, -orientation.right, out leftWallHit, wallCheckDistance, whatIsWall);
    }

    private bool AboveGround()
    {
        return !Physics.Raycast(transform.position, Vector3.down, minJumpHeight, groundLayer);
    }

    private bool OnSlope()
    {
        if (Physics.Raycast(transform.position, Vector3.down, out slopeHit, playerHeight * 0.5f + 0.3f))
        {
            float angle = Vector3.Angle(Vector3.up, slopeHit.normal);
            return angle < maxSlopeAngle && angle != 0;
        }
        return false;
    }

    private Vector3 GetSlopeMoveDirection()
    {
        return Vector3.ProjectOnPlane(moveDirection, slopeHit.normal).normalized;
    }

    private void StartWallRun()
    {
        isWallRunning = true;
    }

    private void WallRunMovement()
    {
        jumpCount = 0;
        rb.useGravity = false;
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);

        Vector3 wallNormal = wallRight ? rightWallHit.normal : leftWallHit.normal;
        Vector3 wallForward = Vector3.Cross(wallNormal, transform.up);

        if ((orientation.forward - wallForward).magnitude > (orientation.forward - -wallForward).magnitude)
            wallForward = -wallForward;

        rb.AddForce(wallForward * wallRunForce, ForceMode.Force);
        rb.AddForce(-wallNormal * 1, ForceMode.Force);
    }

    private void StopWallRun()
    {
        rb.useGravity = true;
        isWallRunning = false;
    }
}