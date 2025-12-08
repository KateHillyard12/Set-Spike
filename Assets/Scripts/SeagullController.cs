using UnityEngine;

[RequireComponent(typeof(Collider), typeof(Rigidbody))]
public class SeagullController : MonoBehaviour
{
    [Header("Movement")]
    public float minSpeed = 4f;
    public float maxSpeed = 10f;
    public float bobAmplitude = 0.3f;
    public float bobFrequency = 2f;

    [Header("Hit Reaction")]
    [Tooltip("If true, destroy the seagull when hit by the ball.")]
    public bool destroyOnBallHit = true;

    [Tooltip("Particle system prefab to spawn when the bird is hit.")]
    public ParticleSystem hitVFXPrefab;

    [Tooltip("Optional delay before destroying bird after hit (to let VFX play).")]
    public float destroyDelayAfterHit = 1f;

    [Header("Facing (edit these in Inspector)")]
    [Tooltip("Rotation when bird is flying to the RIGHT (+X).")]
    public Vector3 rightFacingEuler = new Vector3(0f, 0f, 0f);

    [Tooltip("Rotation when bird is flying to the LEFT (-X).")]
    public Vector3 leftFacingEuler = new Vector3(0f, 180f, 0f);

    [Header("Debug")]
    [SerializeField] private bool debugDrawTarget = false;

    // Private fields
    private float speed;
    private Vector3 moveDir;
    private float targetX;
    private float bobOffset;
    private Rigidbody rb;
    private SeagullSpawner owner;

    // State machine
    private ISeagullState currentState;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
        rb.isKinematic = true;

        speed = Random.Range(minSpeed, maxSpeed);
        bobOffset = Random.Range(0f, 10f);

        // Start in flying state
        ChangeState(new SeagullFlyingState());
    }

    public void Init(Vector3 direction, float targetXWorld, SeagullSpawner spawner)
    {
        moveDir = direction.normalized;
        targetX = targetXWorld;
        owner = spawner;

        OrientToDirection(moveDir);
    }

    void OrientToDirection(Vector3 dir)
    {
        bool goingRight = dir.x >= 0f;
        Vector3 euler = goingRight ? rightFacingEuler : leftFacingEuler;
        transform.rotation = Quaternion.Euler(euler);
    }

    void Update()
    {
        // Update current state
        if (currentState != null)
            currentState.OnUpdate(this);
    }

    void OnCollisionEnter(Collision collision)
    {
        // Only respond to hits while flying
        if (currentState is not SeagullFlyingState) return;

        // Check for volleyball
        BallController ball = collision.collider.GetComponentInParent<BallController>();
        if (ball == null) return;

        if (destroyOnBallHit)
        {
            // Transition to hit state
            var hitState = new SeagullHitState(hitVFXPrefab, transform.position, collision);
            ChangeState(hitState);

            // Schedule transition to dead state after delay
            if (destroyDelayAfterHit > 0f)
                Invoke(nameof(TransitionToDead), destroyDelayAfterHit);
            else
                TransitionToDead();
        }
    }

    void TransitionToDead()
    {
        ChangeState(new SeagullDeadState());
    }

    /// <summary>
    /// Transition to a new state, calling exit on old and enter on new.
    /// </summary>
    public void ChangeState(ISeagullState newState)
    {
        if (currentState != null)
            currentState.OnExit(this);

        currentState = newState;

        if (currentState != null)
            currentState.OnEnter(this);
    }

    // Getters for state access to controller data
    public Vector3 GetMoveDirection() => moveDir;
    public float GetSpeed() => speed;
    public float GetTargetX() => targetX;
    public float GetBobOffset() => bobOffset;

    public void EnableVisuals()
    {
        Collider col = GetComponent<Collider>();
        if (col) col.enabled = true;

        foreach (var rend in GetComponentsInChildren<Renderer>())
            rend.enabled = true;
    }

    public void DisableVisuals()
    {
        Collider col = GetComponent<Collider>();
        if (col) col.enabled = false;

        foreach (var rend in GetComponentsInChildren<Renderer>())
            rend.enabled = false;
    }

    public void NotifyAndDestroy()
    {
        if (owner != null)
            owner.NotifySeagullDestroyed(this);

        Destroy(gameObject);
    }

    void OnDrawGizmosSelected()
    {
        if (!debugDrawTarget) return;
        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(transform.position,
            new Vector3(targetX, transform.position.y, transform.position.z));
    }
}
