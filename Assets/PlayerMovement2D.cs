using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody), typeof(PlayerInput))]
public class PlayerMovement2D : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 6f;
    public float airControlMultiplier = 0.8f;

    [Header("Jump")]
    public float jumpForce = 7.5f;

    [Header("Ground Check (3D)")]
    public Transform groundCheck;         // place at feet
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayer;

    [Header("2.5D Lane Lock")]
    public float laneZ = 0f;              // keep player on this Z

    // --- internals ---
    Rigidbody rb;
    Vector2 moveInput;
    bool jumpQueued;
    bool isGrounded;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        // Freeze Z / Rot XZ in the Rigidbody constraints from the Inspector.
    }

    void Update()
    {
        // Ground check
        if (groundCheck)
        {
            isGrounded = Physics.CheckSphere(
                groundCheck.position,
                groundCheckRadius,
                groundLayer,
                QueryTriggerInteraction.Ignore
            );
        }

        // Jump
        if (jumpQueued && isGrounded)
        {
            var v = rb.linearVelocity;
            v.y = 0f;                 // consistent jump height
            rb.linearVelocity = v;
            rb.AddForce(Vector3.up * jumpForce, ForceMode.VelocityChange);
        }
        jumpQueued = false;

        // Lock to lane Z
        var p = rb.position;
        if (Mathf.Abs(p.z - laneZ) > 0.0001f)
            rb.position = new Vector3(p.x, p.y, laneZ);
    }

    void FixedUpdate()
    {
        // Horizontal move (X only)
        float control = isGrounded ? 1f : airControlMultiplier;
        var vel = rb.linearVelocity;
        vel.x = moveInput.x * moveSpeed * control;
        rb.linearVelocity = vel;

        // Simple facing flip
        if (Mathf.Abs(moveInput.x) > 0.01f)
        {
            float dir = Mathf.Sign(moveInput.x);
            var s = transform.localScale;
            transform.localScale = new Vector3(Mathf.Abs(s.x) * dir, s.y, s.z);
        }
    }

    // -------- Input System (supports both modes) --------
    // Send Messages signatures:
    public void OnMove(InputValue value) { moveInput = value.Get<Vector2>(); }
    public void OnJump(InputValue value) { if (value.isPressed) jumpQueued = true; }

    // Unity Events signatures:
    public void OnMove(InputAction.CallbackContext ctx) { moveInput = ctx.ReadValue<Vector2>(); }
    public void OnJump(InputAction.CallbackContext ctx) { if (ctx.performed) jumpQueued = true; }

    void OnDrawGizmosSelected()
    {
        if (!groundCheck) return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
    }

    // --- External (e.g., Arduino) helpers ---
    public void ExternalJump() => jumpQueued = true;
    public void ExternalMove(float x) => moveInput = new Vector2(Mathf.Clamp(x, -1f, 1f), 0f);
}
