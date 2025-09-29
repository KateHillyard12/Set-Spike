using UnityEngine;

public class CameraScript : MonoBehaviour
{
    [Header("Field of View Settings")]
    public float minFOV = 50f;
    public float maxFOV = 80f;
    public float fovChangeSpeed = 1f;

    [Header("Rotation Settings")]
    public float rotationSpeed = 10f;      
    public float maxRotationAngle = 15f;  

    private Camera cam;
    private float targetFOV;
    private Quaternion targetRotation;

    void Start()
    {
        cam = GetComponent<Camera>();
        PickNewTargets();
    }

    void Update()
    {
        cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, targetFOV, Time.deltaTime * fovChangeSpeed);
        transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);

        if (Mathf.Abs(cam.fieldOfView - targetFOV) < 0.1f &&
            Quaternion.Angle(transform.rotation, targetRotation) < 0.1f)
        {
            PickNewTargets();
        }
    }

    void PickNewTargets()
    {
        targetFOV = Random.Range(minFOV, maxFOV);

        Vector3 randomEuler = new Vector3(
            Random.Range(-maxRotationAngle, maxRotationAngle),
            Random.Range(-maxRotationAngle, maxRotationAngle),
            0f
        );

        targetRotation = Quaternion.Euler(randomEuler);
    }
}

