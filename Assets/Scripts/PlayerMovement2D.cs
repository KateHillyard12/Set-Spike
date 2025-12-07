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
    public Transform groundCheck;        
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayer;

    [Header("2.5D Lane Lock")]
    public float laneZ = 0f;              // keep player on this Z

    [Header("FX")]
    [Tooltip("Sand burst particle prefab (the same SandBurst prefab you made).")]
    public GameObject sandFXPrefab;
    [Tooltip("Vertical offset from groundCheck to place the puff so it isn't clipping.")]
    public float jumpFXYOffset = 0.05f;

    
    // --- internals ---
    Rigidbody rb;
    Animator anim;
    Vector2 moveInput;
    bool isGrounded;

    float jumpBufferTimer = 0f;
    public float jumpBufferTime = 0.12f;

    // hank here Footstep audio sync
    private float footstepInterval = 0.4f; // time between footsteps in seconds
    private float footstepTimer = 0f;
    private bool wasMovingLastFrame = false;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;

        // Was: anim = GetComponent<Animator>();
        anim = GetComponentInChildren<Animator>();
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
        // Reset jump anim on ground
        if (isGrounded)
        {
            if (anim != null)
                anim.ResetTrigger("Jump");
        }

        // --------------------------------------
        // Jump Buffer Countdown
        // --------------------------------------
        if (jumpBufferTimer > 0f)
            jumpBufferTimer -= Time.deltaTime;

        // --------------------------------------
        // Jump Triggering Logic (Buffered)
        // --------------------------------------
        if (jumpBufferTimer > 0f && isGrounded)
        {
            jumpBufferTimer = 0f;

            // Sand puff
            if (sandFXPrefab != null && groundCheck != null)
            {
                Vector3 fxPos = groundCheck.position;
                fxPos.y += jumpFXYOffset;
                fxPos.z = laneZ;

                Instantiate(sandFXPrefab, fxPos, Quaternion.identity);
            }

            // Trigger animation
            if (anim != null)
                anim.SetTrigger("Jump");

            // Zero downward momentum
            var v = rb.linearVelocity;
            v.y = 0f;
            rb.linearVelocity = v;

            // Jump force
            rb.AddForce(Vector3.up * jumpForce, ForceMode.VelocityChange);
            //hank here
            GameAudio.Instance?.PlaySfx(GameAudio.Instance.jumpClip);
        }

        // Lock to lane Z
        var p = rb.position;
        if (Mathf.Abs(p.z - laneZ) > 0.0001f)
            rb.position = new Vector3(p.x, p.y, laneZ);

        // Animator movement parameter
        if (anim != null)
        anim.SetFloat("MoveX", moveInput.x);

        // Hank here: Animation synced footstep audio
        bool isMoving = Mathf.Abs(moveInput.x) > 0.01f && isGrounded;
        if (isMoving)
        {
            footstepTimer -= Time.deltaTime;
            if (footstepTimer <= 0f)
            {
                
                GameAudio.Instance?.PlayFootstep();
                footstepTimer = footstepInterval;
            }
        }
        else
        {
            footstepTimer = 0f; // reset timer when not moving
        }
        wasMovingLastFrame = isMoving;

        if (VolleyballGameManager.freezePlayers)
        {
            moveInput = Vector2.zero;
            return;
        }


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

        if (VolleyballGameManager.freezePlayers)
        {
            rb.linearVelocity = Vector3.zero;
            return;
        }

    }

    // -------- Input System --------
    // Send Messages signatures:
    public void OnMove(InputValue value)
     { 
        moveInput = value.Get<Vector2>();
        if (VolleyballGameManager.freezePlayers) return;
     }
    public void OnJump(InputValue value) 
    { 
        if (value.isPressed) jumpBufferTimer = jumpBufferTime; // store jump for buffer window
        if (VolleyballGameManager.freezePlayers) return;

    }

    public void OnMove(UnityEngine.InputSystem.InputAction.CallbackContext ctx)
    {
        moveInput = ctx.ReadValue<Vector2>();
    }

    public void OnJump(UnityEngine.InputSystem.InputAction.CallbackContext ctx)
    {
        if (ctx.performed)
            jumpBufferTimer = jumpBufferTime;
    }

    void OnDrawGizmosSelected()
    {
        if (!groundCheck) return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
    }

    // --- External (e.g., Arduino) helpers ---
    public void ExternalJump() => jumpBufferTimer = jumpBufferTime;
    public void ExternalMove(float x) => moveInput = new Vector2(Mathf.Clamp(x, -1f, 1f), 0f);
}
