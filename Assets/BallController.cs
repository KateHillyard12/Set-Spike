using UnityEngine;

[RequireComponent(typeof(Rigidbody), typeof(SphereCollider))]
public class BallController : MonoBehaviour
{
    [HideInInspector] public VolleyballGameManager manager;
    [HideInInspector] public Transform net;
    [HideInInspector] public LayerMask groundLayer;
    [HideInInspector] public float laneZ = 0f;

    [Header("Bounce")]
    public float headBounceImpulse = 6f;     // extra pop when hitting a player
    public float carryXFromPlayer = 0.5f;    // add a bit of player X velocity to the ball
    public float maxSpeed = 18f;             // clamp so it doesn't go wild

    Rigidbody rb;

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
        // tiny clamp for sanity
        if (rb.linearVelocity.sqrMagnitude > maxSpeed * maxSpeed)
            rb.linearVelocity = rb.linearVelocity.normalized * maxSpeed;

        // keep z lane exact (in case something nudges it)
        var p = rb.position;
        if (Mathf.Abs(p.z - laneZ) > 0.0001f)
            rb.position = new Vector3(p.x, p.y, laneZ);
    }

    void OnCollisionEnter(Collision c)
    {
        // 1) Ground = point for opposite side
        if (((1 << c.gameObject.layer) & groundLayer) != 0)
        {
            float hitX = c.GetContact(0).point.x;
            bool leftSide = hitX < net.position.x;
            manager.PointScored(leftSide ? CourtSide.Left : CourtSide.Right);
            return;
        }

        // 2) Player bounce boost
        //    If the collider belongs to a Player (has PlayerMovement2D or tagged "Player"), add upward impulse + a bit of their X velocity.
        var mover = c.collider.GetComponentInParent<PlayerMovement2D>();
        if (mover != null)
        {
            // clear downward velocity so hits feel crisp
            Vector3 v = rb.linearVelocity;
            v.y = Mathf.Max(0f, v.y);
            rb.linearVelocity = v;

            // add upward impulse
            rb.AddForce(Vector3.up * headBounceImpulse, ForceMode.VelocityChange);

            // carry some of player's horizontal to the ball
            var prb = mover.GetComponent<Rigidbody>();
            if (prb != null)
                rb.AddForce(new Vector3(prb.linearVelocity.x * carryXFromPlayer, 0f, 0f), ForceMode.VelocityChange);
        }
    }
}
