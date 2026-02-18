using UnityEngine;

public class WallSlide : MonoBehaviour
{
    [Header("Wall Sliding")]
    public LayerMask wallLayer;
    public float wallCheckDistance = 0.5f;
    public float wallSlideSpeed = 2f;
    public bool isWallSliding;

    [Header("Wall Jumping")]
    public Vector2 wallJumpForce = new Vector2(5f, 10f);

    private Rigidbody rb;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody>();

    }

    // Update is called once per frame
    void Update()
    {
        CheckForWallSlide();

        if (isWallSliding && Input.GetButtonDown("Jump"))
        {
            WallJump();
        }

    }

    private void CheckForWallSlide()
    {
        bool isTouchingWall = false;

        if (rb != null)
        {
            isTouchingWall = Physics.Raycast(transform.position, Vector3.forward, wallCheckDistance, wallLayer) ||
                                              Physics.Raycast(transform.position, Vector3.back, wallCheckDistance, wallLayer) ||
                                              Physics.Raycast(transform.position, Vector3.left, wallCheckDistance, wallLayer) ||
                                              Physics.Raycast(transform.position, Vector3.right, wallCheckDistance, wallLayer);
        }

        bool isGrounded = Physics.Raycast(transform.position, Vector3.down, 1.1f);
        
        
        if (isTouchingWall && !isGrounded)
        {
            isWallSliding = true;
        }
        else
        {
            isWallSliding = false;
        }
    }

    void FixedUpdate()
    {
        if (isWallSliding)
        {
            if (rb != null)
            {
                rb.linearVelocity = new Vector3(0, Mathf.Clamp(rb.linearVelocity.y, -wallSlideSpeed, float.MaxValue), 0);

            }
        }
    }

    private void WallJump()
    {
        isWallSliding = false;

        if (rb != null)
        {
            Vector3 forceToApply = transform.up * wallJumpForce.y - transform.forward * wallJumpForce.x;
            rb.linearVelocity = Vector3.zero; // Reset velocity before applying force
            rb.AddForce(forceToApply, ForceMode.Impulse);
        }
    }
}
