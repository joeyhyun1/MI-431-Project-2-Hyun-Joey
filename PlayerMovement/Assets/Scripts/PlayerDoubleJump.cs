using UnityEngine;

public class PlayerDoubleJump : MonoBehaviour
{
    [Header("Movement")] 
    public float moveSpeed = 5f;

    public float jumpForce = 10f;

    [Header("Ground Check")] 
    public LayerMask groundLayer;

    public bool isGrounded;

    [Header("Jumping")] 
    private int jumpCount = 0;

    private int maxJumps = 2;

    private Rigidbody rb;
    
    public Vector3 moveDirection;
    


    
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        if (rb != null)
        {
            isGrounded = Physics.Raycast(transform.position, Vector3.down, 1.1f, groundLayer);
        }

        if (isGrounded)
        {
            jumpCount = 0;
        }

        if (Input.GetButtonDown("Jump") && jumpCount < maxJumps)
        {
            HandleJump();
        }
        
    }

    void FixedUpdate()
    {
        float moveInputX = Input.GetAxis("Horizontal");
        float moveInputZ = Input.GetAxis("Vertical");

        if (rb != null)
        {
            moveDirection = Camera.main.transform.forward * moveInputZ + Camera.main.transform.right * moveInputX;       
            rb.linearVelocity = new Vector3(moveDirection.x * moveSpeed, rb.linearVelocity.y, moveDirection.z * moveSpeed);
            
        }
    }

    private void HandleJump()
    {
        jumpCount++;
        if (rb != null)
        {
            //reset vertical velocity to ensure consistent jump height
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }
    }



}
