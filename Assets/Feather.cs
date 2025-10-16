using UnityEngine;

public class FeatherFloat : MonoBehaviour
{
    public float fallSpeed = 0.3f;
    public float driftAmplitude = 0.3f;
    public float driftFrequency = 1.5f;
    public float rotationSpeed = 25f;
    public float randomTiltSpeed = 15f;
    public float randomDriftVariation = 0.2f;
    public float randomSpinVariation = 0.5f;
    public float destroyAfter = 6f;

    float timeOffsetX;
    float timeOffsetZ;
    Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (!rb) rb = gameObject.AddComponent<Rigidbody>();

        rb.useGravity = false;
        rb.linearDamping = 2f;
        rb.angularDamping = 2f;

        timeOffsetX = Random.Range(0f, 100f);
        timeOffsetZ = Random.Range(0f, 100f);

        fallSpeed += Random.Range(-0.05f, 0.05f);
        driftAmplitude += Random.Range(-randomDriftVariation, randomDriftVariation);
        rotationSpeed += Random.Range(-randomSpinVariation * rotationSpeed, randomSpinVariation * rotationSpeed);

        Destroy(gameObject, destroyAfter);
    }

    void FixedUpdate()
    {
        float time = Time.time;
        float xDrift = Mathf.Sin(time * driftFrequency + timeOffsetX) * driftAmplitude;
        float zDrift = Mathf.Cos(time * driftFrequency + timeOffsetZ) * driftAmplitude;
        Vector3 drift = new Vector3(xDrift, -fallSpeed, zDrift);

        rb.MovePosition(transform.position + drift * Time.fixedDeltaTime);

        transform.Rotate(Vector3.up, rotationSpeed * Time.fixedDeltaTime, Space.World);
        transform.Rotate(Vector3.right, randomTiltSpeed * Mathf.Sin(time) * Time.fixedDeltaTime);
        transform.Rotate(Vector3.forward, randomTiltSpeed * Mathf.Cos(time) * Time.fixedDeltaTime);
    }
}
