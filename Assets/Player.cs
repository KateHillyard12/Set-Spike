using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class CapsuleController : MonoBehaviour
{
    public float moveSpeed = 5f;    
    public float jumpForce = 5f;    
    private Rigidbody rb;

    private bool isGrounded = true;  

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.FreezeRotationZ | RigidbodyConstraints.FreezeRotationX;
    }

    void Update()
    {
        HandleMovement();
        HandleJump();
    }

    void HandleMovement()
    {
        float moveX = 0f;

        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
        {
            moveX = 1f;
        }

        // One control backward (A)
        if (Input.GetKey(KeyCode.A))
        {
            moveX = -1f;
        }

        // Apply movement on X axis only
        Vector3 velocity = rb.linearVelocity;
        velocity.x = moveX * moveSpeed;
        rb.linearVelocity = velocity;
    }

    void HandleJump()
    {
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            isGrounded = false;
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.contacts.Length > 0)
        {
            isGrounded = true;
        }
    }
}
