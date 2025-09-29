using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Volleyball2DPhysics : MonoBehaviour
{
    public float baseUpwardForce = 8f;
    public float baseHorizontalForce = 8f;

    public Transform player1;
    public Transform player2;
    public Vector3 player1Start = new Vector3(-6f, 1f, 0f);
    public Vector3 player2Start = new Vector3(6f, 1f, 0f);

    public int player1Score = 0;
    public int player2Score = 0;

    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = true;
        rb.constraints = RigidbodyConstraints.FreezePositionZ | RigidbodyConstraints.FreezeRotation;
        Physics.gravity = new Vector3(0, -4.9f, 0);

        if (player1 != null) player1.position = player1Start;
        if (player2 != null) player2.position = player2Start;
    }

    void FixedUpdate()
    {
        Vector3 vel = rb.linearVelocity;
        vel.z = 0f;
        rb.linearVelocity = vel;
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            if (transform.position.x < 0)
            {
                player1Score++;
                transform.position = new Vector3(player2Start.x, player2Start.y + 6f, 0f);
            }
            else
            {
                player2Score++;
                transform.position = new Vector3(player1Start.x, player1Start.y + 6f, 0f);
            }
            ResetPlayers();
        }

        if (collision.gameObject.CompareTag("OOB1"))
        {
            player1Score++;
            transform.position = new Vector3(player2Start.x, player2Start.y + 6f, 0f);
            ResetPlayers();
        }

        if (collision.gameObject.CompareTag("OOB2"))
        {
            player2Score++;
            transform.position = new Vector3(player1Start.x, player1Start.y + 6f, 0f);
            ResetPlayers();
        }

        if (collision.gameObject.CompareTag("Player1") || collision.gameObject.CompareTag("Player2"))
        {
            float x = transform.position.x;
            float upward = baseUpwardForce + Random.Range(-2f, 2f);
            float horizontal;

            if (x < 0)
            {
                horizontal = baseHorizontalForce + Random.Range(-2f, 2f);
                upward *= (Mathf.Abs(x) >= 11f) ? 1.5f : 0.7f;
            }
            else
            {
                horizontal = -baseHorizontalForce + Random.Range(-2f, 2f);
                upward *= (Mathf.Abs(x) >= 11f) ? 1.5f : 0.7f;
            }

            rb.AddForce(new Vector3(horizontal, upward, 0f), ForceMode.Impulse);
        }
    }

    void ResetPlayers()
    {
        rb.linearVelocity = Vector3.zero;
        if (player1 != null) player1.position = player1Start;
        if (player2 != null) player2.position = player2Start;
    }
}
