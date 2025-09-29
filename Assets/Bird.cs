using UnityEngine;
using System.Collections;

public class BirdAI : MonoBehaviour
{
 
    BirdState state = BirdState.Normal;

    public float moveSpeed = 5f;
    public float hitSlowdownFactor = 0.2f;
    public float hitDuration = 1f;
    public float gravity = 9.81f;

    Vector2 dir = Vector2.right;
    float hitTimer;
    Rigidbody rb;
    float originalZ;
    float targetZ;
    public enum BirdState { Normal, Hit, Falling, Dying }
    public BirdState currentState = BirdState.Normal;

    public float dieDelay = 2f;   // adjustable in Inspector


    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        if (!rb) rb = gameObject.AddComponent<Rigidbody>();
        rb.useGravity = false;
        rb.constraints = RigidbodyConstraints.FreezeRotation;
    }
     void Start()
    {
        // spawn at (0, currentY, 0)
        transform.position = new Vector3(0f, transform.position.y, 0f);
        originalZ = 0f;
        targetZ = originalZ;

        // get Rigidbody reference
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
        }

        rb.useGravity = false;   // disable default gravity at start
        rb.constraints = RigidbodyConstraints.FreezeRotation; // no flipping
    }

    // === NEW METHOD ===
    public void EnterFallingState()
    {
        currentState = BirdState.Falling;
        rb.useGravity = true;    // turn gravity on
        rb.linearVelocity = Vector3.zero;
    }


    private IEnumerator DieAfterDelay()
    {
        // lock state as falling during wait
        currentState = BirdState.Falling;
        yield return new WaitForSeconds(dieDelay);
        EnterDieState();
    }

    private void EnterDieState()
    {
        currentState = BirdState.Dying;
        rb.useGravity = false;      // stop gravity
        rb.linearVelocity = Vector3.zero; // freeze in place
        Debug.Log("Bird has died.");
        // Optionally: Destroy(gameObject, 1f); // uncomment to remove bird
    }

    void Update()
    {
        switch (state)
        {
            case BirdState.Normal:
                rb.linearVelocity = new Vector3(dir.x * moveSpeed, dir.y * moveSpeed, 0f);
                break;
            case BirdState.Hit:
                hitTimer -= Time.deltaTime;
                rb.linearVelocity = new Vector3(dir.x * moveSpeed * hitSlowdownFactor, dir.y * moveSpeed * hitSlowdownFactor, 0f);
                if (hitTimer <= 0f)
                {
                    state = BirdState.Normal;
                    targetZ = originalZ;
                }
                break;
            case BirdState.Dying:
                rb.linearVelocity += Vector3.down * gravity * Time.deltaTime;
                break;
        }
        var p = transform.position;
        p.z = targetZ;
        transform.position = p;
    }

    void Bounce(Vector3 normal)
    {
        if (Mathf.Abs(normal.x) > Mathf.Abs(normal.y)) dir.x = -dir.x; else dir.y = -dir.y;
        rb.linearVelocity = new Vector3(dir.x * moveSpeed, dir.y * moveSpeed, 0f);
        rb.position += normal * 0.05f;
    }

private void OnCollisionEnter(Collision collision)
{
    // Handle bounce off walls
    if (collision.collider.CompareTag("OOB1") || collision.collider.CompareTag("OOB2"))
    {
        Bounce(collision.contacts[0].normal);
    }
    // Handle ball collision
    else if (collision.collider.CompareTag("Ball"))
    {
        if (currentState == BirdState.Falling)
        {
            // If already falling → die after delay
            StartCoroutine(DieAfterDelay());
        }
        else
        {
            // Otherwise just enter hit state
            StartHit();
        }
    }
}



    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("OOB1") || other.CompareTag("OOB2"))
        {
            Vector3 normal = (transform.position - other.ClosestPoint(transform.position)).normalized;
            Bounce(normal);
        }
        else if (other.CompareTag("Ball")) StartHit();
    }

    void StartHit()
    {
        state = BirdState.Hit;
        hitTimer = hitDuration;
        targetZ = -5f;
    }


}