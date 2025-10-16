using UnityEngine;
using System.Collections;

public class BirdAI : MonoBehaviour
{
    public enum BirdState { Normal, Hit, Falling }
    public BirdState currentState = BirdState.Normal;

    public float moveSpeed = 5f;
    public float hitSlowdownFactor = 0.2f;
    public float hitDuration = 1f;

    public GameObject featherPrefab;
    public int feathersOnHitMin = 2;
    public int feathersOnHitMax = 5;
    public int feathersOnDieMin = 6;
    public int feathersOnDieMax = 12;
    public int feathersOnGroundMin = 4;
    public int feathersOnGroundMax = 10;
    public int feathersOnDespawnMin = 2;
    public int feathersOnDespawnMax = 6;

    public float featherSpawnRadiusMin = 0.2f;
    public float featherSpawnRadiusMax = 3f;
    public float featherImpulseMin = 1f;
    public float featherImpulseMax = 6f;
    public float featherUpwardImpulseMin = 0.2f;
    public float featherUpwardImpulseMax = 2f;
    public float featherTorqueMax = 3f;
    public bool applyRandomFeatherScale = false;
    public Vector2 featherScaleRange = new Vector2(0.6f, 1.2f);
    public float featherLifetime = 4f;
    public bool destroyFeathersAfterLifetime = true;

    public float despawnDelayOnGround = 0.75f;
    public float respawnDelay = 2f;

    Vector2 dir = Vector2.right;
    float hitTimer;
    Rigidbody rb;
    float originalZ;
    float targetZ;
    int hitCyclesCompleted;
    bool hasEnteredFalling;
    bool hasGroundedOnce;
    bool isDespawning;
    Vector3 prefabScale = Vector3.one;

    Vector3 spawnPosition;
    Quaternion spawnRotation;
    Renderer[] rends;
    Collider[] cols;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        if (!rb) rb = gameObject.AddComponent<Rigidbody>();
        rb.useGravity = false;
        rb.constraints = RigidbodyConstraints.FreezeRotation;
        if (featherPrefab) prefabScale = featherPrefab.transform.localScale;
        rends = GetComponentsInChildren<Renderer>(true);
        cols = GetComponentsInChildren<Collider>(true);
    }

    void Start()
    {
        spawnPosition = transform.position;
        spawnRotation = transform.rotation;
        transform.position = new Vector3(0f, transform.position.y, 0f);
        originalZ = 0f;
        targetZ = originalZ;
        rb.useGravity = false;
        rb.constraints = RigidbodyConstraints.FreezeRotation;
    }

    public void EnterFallingState()
    {
        if (hasEnteredFalling) return;
        hasEnteredFalling = true;
        currentState = BirdState.Falling;
        rb.useGravity = true;
        rb.linearVelocity = Vector3.zero;
        SpawnFeathers(Random.Range(feathersOnDieMin, feathersOnDieMax + 1));
    }

    void Update()
    {
        switch (currentState)
        {
            case BirdState.Normal:
                rb.useGravity = false;
                rb.linearVelocity = new Vector3(dir.x * moveSpeed, dir.y * moveSpeed, 0f);
                break;
            case BirdState.Hit:
                hitTimer -= Time.deltaTime;
                rb.useGravity = false;
                rb.linearVelocity = new Vector3(dir.x * moveSpeed * hitSlowdownFactor, dir.y * moveSpeed * hitSlowdownFactor, 0f);
                if (hitTimer <= 0f)
                {
                    hitCyclesCompleted++;
                    if (hitCyclesCompleted >= 3) EnterFallingState();
                    else { currentState = BirdState.Normal; targetZ = originalZ; }
                }
                break;
            case BirdState.Falling:
                break;
        }
        var p = transform.position;
        p.z = targetZ;
        transform.position = p;
    }

    void Bounce(Vector3 normal)
    {
        if (Mathf.Abs(normal.x) > Mathf.Abs(normal.y)) dir.x = -dir.x; else dir.y = -dir.y;
        if (currentState != BirdState.Falling) rb.linearVelocity = new Vector3(dir.x * moveSpeed, dir.y * moveSpeed, 0f);
        rb.position += normal * 0.05f;
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.CompareTag("OOB1") || collision.collider.CompareTag("OOB2"))
        {
            Bounce(collision.contacts[0].normal);
        }
        else if (collision.collider.CompareTag("Ball"))
        {
            if (currentState != BirdState.Falling)
            {
                SpawnFeathers(Random.Range(feathersOnHitMin, feathersOnHitMax + 1));
                StartHit();
            }
        }
        else if (collision.collider.CompareTag("Ground"))
        {
            TryGroundSequence();
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("OOB1") || other.CompareTag("OOB2"))
        {
            Vector3 normal = (transform.position - other.ClosestPoint(transform.position)).normalized;
            Bounce(normal);
        }
        else if (other.CompareTag("Ball") && currentState != BirdState.Falling)
        {
            SpawnFeathers(Random.Range(feathersOnHitMin, feathersOnHitMax + 1));
            StartHit();
        }
        else if (other.CompareTag("Ground"))
        {
            TryGroundSequence();
        }
    }

    void TryGroundSequence()
    {
        if (currentState != BirdState.Falling) return;
        if (isDespawning) return;
        isDespawning = true;
        if (!hasGroundedOnce)
        {
            hasGroundedOnce = true;
            SpawnFeathers(Random.Range(feathersOnGroundMin, feathersOnGroundMax + 1));
        }
        StartCoroutine(DespawnAndRespawn());
    }

    IEnumerator DespawnAndRespawn()
    {
        yield return new WaitForSeconds(Mathf.Max(0f, despawnDelayOnGround));
        SpawnFeathers(Random.Range(feathersOnDespawnMin, feathersOnDespawnMax + 1));
        SetAliveVisuals(false);
        rb.useGravity = false;
        rb.linearVelocity = Vector3.zero;
        transform.position = spawnPosition;
        transform.rotation = spawnRotation;
        targetZ = originalZ;
        dir = Vector2.right;
        hitCyclesCompleted = 0;
        hasEnteredFalling = false;
        hasGroundedOnce = false;
        currentState = BirdState.Normal;
        yield return new WaitForSeconds(Mathf.Max(0f, respawnDelay));
        SetAliveVisuals(true);
    }

    void SetAliveVisuals(bool on)
    {
        if (rends != null) foreach (var r in rends) if (r) r.enabled = on;
        if (cols != null) foreach (var c in cols) if (c) c.enabled = on;
    }

    void StartHit()
    {
        currentState = BirdState.Hit;
        hitTimer = hitDuration;
        targetZ = -5f;
    }

    void SpawnFeathers(int count)
    {
        if (!featherPrefab || count <= 0) return;
        float rMin = Mathf.Max(0f, featherSpawnRadiusMin);
        float rMax = Mathf.Max(rMin, featherSpawnRadiusMax);
        for (int i = 0; i < count; i++)
        {
            Vector2 d2 = Random.insideUnitCircle.normalized;
            float dist = Random.Range(rMin, rMax);
            Vector3 offset = new Vector3(d2.x, 0f, d2.y) * dist;
            Vector3 spawnPos = transform.position + offset;
            Quaternion rot = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
            GameObject f = Instantiate(featherPrefab, spawnPos, rot);
            if (applyRandomFeatherScale)
            {
                float s = Random.Range(featherScaleRange.x, featherScaleRange.y);
                f.transform.localScale = prefabScale * s;
            }
            Rigidbody fr = f.GetComponent<Rigidbody>();
            if (!fr) fr = f.AddComponent<Rigidbody>();
            Vector3 planar = new Vector3(d2.x, 0f, d2.y).normalized;
            float planarMag = Random.Range(featherImpulseMin, featherImpulseMax);
            float upMag = Random.Range(featherUpwardImpulseMin, featherUpwardImpulseMax);
            Vector3 impulse = planar * planarMag + Vector3.up * upMag;
            fr.AddForce(impulse, ForceMode.Impulse);
            Vector3 torque = new Vector3(Random.Range(-featherTorqueMax, featherTorqueMax),
                                         Random.Range(-featherTorqueMax, featherTorqueMax),
                                         Random.Range(-featherTorqueMax, featherTorqueMax));
            if (featherTorqueMax > 0f) fr.AddTorque(torque, ForceMode.Impulse);
            if (destroyFeathersAfterLifetime && featherLifetime > 0f) Destroy(f, featherLifetime);
        }
    }
}
