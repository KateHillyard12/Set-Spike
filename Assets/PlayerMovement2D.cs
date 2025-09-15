using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMovement2D : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 6f;
    public float airControlMultiplier = 0.8f;   // lower = less control in air

    [Header("Jump")]
    public float jumpForce = 7.5f;              // in m/s applied as velocity change

    [Header("Ground Check (3D)")]
    public Transform groundCheck;               // place at feet
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayer;

    [Header("2.5D Lane Lock")]
    public float laneZ = 0f;                    // keep the player locked to this Z

    private Rigidbody rb;
    private PlayerControls controls;

    private Vector2 moveInput;
    private bool jumpQueued;
    private bool isGrounded;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;

        // Input System (generated class)
        controls = new PlayerControls();
        controls.Player.Move.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
        controls.Player.Move.canceled  += ctx => moveInput = Vector2.zero;
        controls.Player.Jump.performed += ctx => jumpQueued = true;
    }

    void OnEnable()  => controls.Player.Enable();
    void OnDisable() => controls.Player.Disable();

    void Update()
    {
        // Ground check using 3D physics
        if (groundCheck != null)
            isGrounded = Physics.CheckSphere(
                groundCheck.position,
                groundCheckRadius,
                groundLayer,
                QueryTriggerInteraction.Ignore
            );

        // Handle jump on Update to minimize input latency
        if (jumpQueued && isGrounded)
        {
            // zero vertical first so repeated jumps feel consistent
            Vector3 v = rb.linearVelocity;
            v.y = 0f;
            rb.linearVelocity = v;

            // apply instant upward velocity
            rb.AddForce(Vector3.up * jumpForce, ForceMode.VelocityChange);
        }
        jumpQueued = false;

        // Keep player locked to lane Z (works with continuous collisions)
        Vector3 p = rb.position;
        if (Mathf.Abs(p.z - laneZ) > 0.0001f)
            rb.position = new Vector3(p.x, p.y, laneZ);
    }

    void FixedUpdate()
    {
        // Horizontal movement on X only; preserve existing Y velocity
        float control = isGrounded ? 1f : airControlMultiplier;
        Vector3 vel = rb.linearVelocity;
        vel.x = moveInput.x * moveSpeed * control;
        rb.linearVelocity = vel;

        // Simple facing flip (assumes right-facing scale = +1)
        if (Mathf.Abs(moveInput.x) > 0.01f)
        {
            float dir = Mathf.Sign(moveInput.x);
            Vector3 s = transform.localScale;
            transform.localScale = new Vector3(Mathf.Abs(s.x) * dir, s.y, s.z);
        }
    }

    // Optional: hook these if you’re using PlayerInput (Send Messages / Unity Events)
    public void OnMove(InputAction.CallbackContext ctx)   => moveInput = ctx.ReadValue<Vector2>();
    public void OnJump(InputAction.CallbackContext ctx)   { if (ctx.performed) jumpQueued = true; }

    void OnDrawGizmosSelected()
    {
        if (groundCheck == null) return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
    }
}
