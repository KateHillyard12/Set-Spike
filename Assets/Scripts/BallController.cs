using UnityEngine;

[RequireComponent(typeof(Rigidbody), typeof(SphereCollider))]
public class BallController : MonoBehaviour
{
    [HideInInspector] public VolleyballGameManager manager;
    [HideInInspector] public Transform net;
    [HideInInspector] public LayerMask groundLayer;
    [HideInInspector] public float laneZ = 0f;

    [HideInInspector] public float leftBoundaryX;
    [HideInInspector] public float rightBoundaryX;

    [Header("Bounce")]
    public float headBounceImpulse = 4f; //default 6
    public float carryXFromPlayer = 0.2f; //default 0.5
    public float maxSpeed = 10f; //default 18

    [Header("FX")]
    [Tooltip("Sand particle prefab (SandBurst). Will be spawned on player contact.")]
    public GameObject sandFXPrefab;

    Rigidbody rb;
    bool scoredAlready = false;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = true;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        rb.interpolation = RigidbodyInterpolation.Interpolate;

        // Keep in 2.5D lane
        var c = rb.constraints;
        rb.constraints = c | RigidbodyConstraints.FreezePositionZ;
    }

    public void Launch(Vector3 initialVelocity)
    {
        rb.linearVelocity = initialVelocity;
    }

    void FixedUpdate()
    {
        if (scoredAlready) return;

        // clamp crazy speed
        if (rb.linearVelocity.sqrMagnitude > maxSpeed * maxSpeed)
            rb.linearVelocity = rb.linearVelocity.normalized * maxSpeed;

        // keep z lane exact
        var p = rb.position;
        if (Mathf.Abs(p.z - laneZ) > 0.0001f)
            rb.position = new Vector3(p.x, p.y, laneZ);

        // OUT OF BOUNDS CHECK
        if (p.x < leftBoundaryX)
        {
            scoredAlready = true;
            manager.PointScored(CourtSide.Left);
            return;
        }

        if (p.x > rightBoundaryX)
        {
            scoredAlready = true;
            manager.PointScored(CourtSide.Right);
            return;
        }
    }

    void OnCollisionEnter(Collision c)
    {
        if (scoredAlready) return;

        // 1) Ground contact ends rally
        if (((1 << c.gameObject.layer) & groundLayer) != 0)
        {
            float hitX = c.GetContact(0).point.x;
            bool leftSide = hitX < net.position.x;

            GameAudio.Instance?.PlayGroundHit();

            scoredAlready = true;
            manager.PointScored(leftSide ? CourtSide.Left : CourtSide.Right);
            return;
        }

        // 2) Player contact: bounce AND sand poof
        var mover = c.collider.GetComponentInParent<PlayerMovement2D>();
        if (mover != null)
        {
            GameAudio.Instance?.PlayVolleyballHit();

            // spawn sand at first contact point
            if (sandFXPrefab != null)
            {
                ContactPoint cp = c.GetContact(0);
                Vector3 fxPos = cp.point;

                // lock Z so it stays in lane visually
                fxPos.z = laneZ;

                Instantiate(sandFXPrefab, fxPos, Quaternion.identity);
            }

            // clear downward velocity so hits feel crisp
            Vector3 v = rb.linearVelocity;
            v.y = Mathf.Max(0f, v.y);
            rb.linearVelocity = v;

            // add upward impulse
            rb.AddForce(Vector3.up * headBounceImpulse, ForceMode.VelocityChange);

            // carry some horizontal from player
            var prb = mover.GetComponent<Rigidbody>();
            if (prb != null)
                rb.AddForce(new Vector3(prb.linearVelocity.x * carryXFromPlayer, 0f, 0f), ForceMode.VelocityChange);
        }
    }
}
