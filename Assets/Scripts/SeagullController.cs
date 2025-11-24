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
    public float destroyDelayAfterHit = 0.1f;

    [Header("Facing (edit these in Inspector)")]
    [Tooltip("Rotation when bird is flying to the RIGHT (+X).")]
    public Vector3 rightFacingEuler = new Vector3(0f, 0f, 0f);

    [Tooltip("Rotation when bird is flying to the LEFT (-X).")]
    public Vector3 leftFacingEuler = new Vector3(0f, 180f, 0f);

    [Header("Debug")]
    [SerializeField] private bool debugDrawTarget = false;

    float speed;
    Vector3 moveDir;
    float targetX;
    float bobOffset;
    Rigidbody rb;
    SeagullSpawner owner;

    bool initialized;
    bool hasBeenHit;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
        rb.isKinematic = true;

        speed = Random.Range(minSpeed, maxSpeed);
        bobOffset = Random.Range(0f, 10f);
    }

    public void Init(Vector3 direction, float targetXWorld, SeagullSpawner spawner)
    {
        moveDir = direction.normalized;
        targetX = targetXWorld;
        owner = spawner;
        initialized = true;

        OrientToDirection(moveDir);
    }

    void OrientToDirection(Vector3 dir)
    {
        // If moving right (+X), use rightFacingEuler, else leftFacingEuler
        bool goingRight = dir.x >= 0f;
        Vector3 euler = goingRight ? rightFacingEuler : leftFacingEuler;
        transform.rotation = Quaternion.Euler(euler);
    }

    void Update()
    {
        if (!initialized || hasBeenHit) return;

        float dt = Time.deltaTime;

        // Move forward
        transform.position += moveDir * (speed * dt);

        // Bobbing
        Vector3 pos = transform.position;
        float bob = Mathf.Sin((Time.time + bobOffset) * bobFrequency) * bobAmplitude;
        pos.y += bob * dt;
        transform.position = pos;

        // Despawn when past target
        if (moveDir.x > 0f && transform.position.x >= targetX)
            Kill();
        else if (moveDir.x < 0f && transform.position.x <= targetX)
            Kill();
    }

    void OnCollisionEnter(Collision collision)
    {
        if (hasBeenHit) return;

        // Check for volleyball
        BallController ball = collision.collider.GetComponentInParent<BallController>();
        if (ball == null) return;

        hasBeenHit = true;

        // Spawn VFX
        if (hitVFXPrefab != null)
        {
            Vector3 spawnPos = transform.position;
            if (collision.contactCount > 0)
                spawnPos = collision.GetContact(0).point;

            ParticleSystem vfx = Instantiate(hitVFXPrefab, spawnPos, Quaternion.identity);
            vfx.Play();

            var main = vfx.main;
            float maxLifetime = main.startLifetime.constantMax;
            Destroy(vfx.gameObject, main.duration + maxLifetime);
        }

        DisableVisuals();

        if (destroyOnBallHit)
        {
            if (destroyDelayAfterHit > 0f)
                Invoke(nameof(Kill), destroyDelayAfterHit);
            else
                Kill();
        }
    }

    void DisableVisuals()
    {
        Collider col = GetComponent<Collider>();
        if (col) col.enabled = false;

        foreach (var rend in GetComponentsInChildren<Renderer>())
            rend.enabled = false;
    }

    void Kill()
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
