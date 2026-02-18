using System.Collections;
using UnityEngine;

public class PlayerDash : MonoBehaviour
{
    [Header("Dashing")] 
    public float dashSpeed = 20f;
    public float dashDuration = 0.2f;
    public bool canDash = true;
    //public float dashCooldown = 1f;

    
    private PlayerDoubleJump doubleJumpScript;

    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        doubleJumpScript = GetComponent<PlayerDoubleJump>();
    }

    void Update()
    {
        if (doubleJumpScript.isGrounded == true && canDash == false)
        {
            canDash = true;
        }
        
        if (Input.GetKeyDown(KeyCode.LeftShift) && canDash)
        {
            StartCoroutine(Dash());
        }
        

    }



    private IEnumerator Dash()
    {
        canDash = false;

        float originalSpeed = doubleJumpScript.moveSpeed;
        doubleJumpScript.moveSpeed = dashSpeed;

        bool originalGravity = rb.useGravity;
        rb.useGravity = false;

        rb.linearVelocity = transform.forward * dashSpeed;

        yield return new WaitForSeconds(dashDuration);

        rb.useGravity = originalGravity;
        doubleJumpScript.moveSpeed = originalSpeed;
        rb.linearVelocity = Vector3.zero;

        


    }
}