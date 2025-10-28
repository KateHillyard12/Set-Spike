using UnityEngine;
using System.Collections;

public class BirdAI : MonoBehaviour
{

public enum BirdState { Approaching, Normal, Hit, Falling, Exiting } //states

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

    [Header("Approach")]   //for bird approach after spawn in
    public float approachStopDistance = 0.05f;  
    bool _hasApproach;
    Vector3 _approachTarget;
    float _approachSpeed = 10f;

    static bool _layerCollisionConfigured = false;
    Vector3 spawnPosition;
    Quaternion spawnRotation;
    Renderer[] rends;
    Collider[] cols;


    [Header("Auto Exit")]   //for Bird Exit After Timer
    public bool enableAutoExit = true;                 
    public Vector2 exitDelayRange = new Vector2(6f, 12f); 
    public float exitXDistance = 200f;                 
    public float exitYUp = 70f;                        
    public float exitSpeed = 14f;                      


    Vector3 _exitTarget;
    Coroutine _exitCo;





        void Awake()    //gets conponets before script
    {
        rb = GetComponent<Rigidbody>();
        if (!rb) rb = gameObject.AddComponent<Rigidbody>();
        rb.useGravity = false;
        rb.constraints = RigidbodyConstraints.FreezeRotation;
        int birdLayer = LayerMask.NameToLayer("Bird");
        if (birdLayer >= 0)
    {
        gameObject.layer = birdLayer;   //for disabling collisions with other birds
        if (!_layerCollisionConfigured)
    {
        Physics.IgnoreLayerCollision(birdLayer, birdLayer, true);
        _layerCollisionConfigured = true;
    }
    }
        if (featherPrefab) prefabScale = featherPrefab.transform.localScale;  //for getting feather 
        rends = GetComponentsInChildren<Renderer>(true);
        cols  = GetComponentsInChildren<Collider>(true);
        originalZ = 0f;
        targetZ   = originalZ;
    }


    void Start()   //saves physics settings in game
    {
        spawnPosition = transform.position;
        spawnRotation = transform.rotation;
        rb.useGravity = false;
        rb.constraints = RigidbodyConstraints.FreezeRotation;
        if (!_hasApproach) currentState = BirdState.Normal;
    }

    void Update()  //handles state
    {
    switch (currentState)
    {
        case BirdState.Approaching: //start with approach state
            rb.useGravity = false;
            Vector3 pos  = transform.position;
            Vector3 next = Vector3.MoveTowards(pos, _approachTarget, _approachSpeed * Time.deltaTime);
            next.z = targetZ;
            transform.position = next;

            Vector3 d = (_approachTarget - next); d.z = 0f;
            if (d.sqrMagnitude > 0.0001f) transform.up = d.normalized;

            if (Vector3.Distance(next, _approachTarget) <= approachStopDistance)
            {
                currentState = BirdState.Normal;
                transform.rotation = Quaternion.identity;
                rb.constraints = RigidbodyConstraints.FreezeRotation;
                dir = (Random.value < 0.5f) ? Vector2.left : Vector2.right;
                    rb.linearVelocity = new Vector3(dir.x * moveSpeed, 0f, 0f);
                StartExitTimer(); 
            }
            break;
        case BirdState.Normal:   //normal movemnt 
            rb.useGravity = false;
            rb.linearVelocity = new Vector3(dir.x * moveSpeed, 0f, 0f);
            break;

        case BirdState.Hit:   //on hit with ball 
            hitTimer -= Time.deltaTime;  
            rb.useGravity = false;
            rb.linearVelocity = new Vector3(dir.x * moveSpeed * hitSlowdownFactor, 0f, 0f);
            if (hitTimer <= 0f)
            {
                hitCyclesCompleted++;
                if (hitCyclesCompleted >= 3) EnterFallingState();
                else { currentState = BirdState.Normal; targetZ = originalZ; }
            }
            break;

        case BirdState.Falling: //fall state
                break;
            

        case BirdState.Exiting: //for bird exit after timer
            rb.useGravity = false;
            Vector3 p0  = transform.position;
            Vector3 p1  = Vector3.MoveTowards(p0, _exitTarget, exitSpeed * Time.deltaTime);
            p1.z = targetZ;
            transform.position = p1;

            Vector3 face = (_exitTarget - p1); face.z = 0f;
            if (face.sqrMagnitude > 0.0001f) transform.up = face.normalized;

            if (Vector3.Distance(p1, _exitTarget) <= approachStopDistance)
            DespawnImmediate();
        break;


    }

    var p = transform.position; p.z = targetZ; transform.position = p;
}


   void Bounce(Vector3 normal)   //bounce off walls
{
    if (Mathf.Abs(normal.x) > Mathf.Abs(normal.y)) dir.x = -dir.x; else dir.y = -dir.y;
    if (currentState != BirdState.Falling)
        rb.linearVelocity = new Vector3(dir.x * moveSpeed, 0f, 0f); 
    rb.position += normal * 0.05f;
}


    void OnCollisionEnter(Collision collision)  //detect collisions
    {
        if (collision.collider.CompareTag("OOB1") || collision.collider.CompareTag("OOB2"))   //detect out of bounds 
        {
            Bounce(collision.contacts[0].normal);   
        }
        else if (collision.collider.CompareTag("Ball"))      //detect Ball
        {
            if (currentState != BirdState.Falling)
            {
                SpawnFeathers(Random.Range(feathersOnHitMin, feathersOnHitMax + 1));
                StartHit();
            }
        }
        else if (collision.collider.CompareTag("Ground"))       //detect Ground
        {
            TryGroundSequence();
        }
    }

    public void EnterFallingState() //makes bird fall when hit 3 times
    {
        if (hasEnteredFalling) return;
        hasEnteredFalling = true;
        currentState = BirdState.Falling;
        rb.useGravity = true;
        rb.linearVelocity = Vector3.zero; 
        SpawnFeathers(Random.Range(feathersOnDieMin, feathersOnDieMax + 1));
    }


    void TryGroundSequence()   //detect when to despawn
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

    IEnumerator DespawnAndRespawn() //despawn and drop feather on death
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


    public void PrepareForApproach(Vector3 target, float speed)  //sets its game position to move too after spawn for approach
    {
        _hasApproach   = true;
        _approachTarget = new Vector3(target.x, target.y, 0f); 
        _approachSpeed  = Mathf.Max(0.1f, speed);
        currentState    = BirdState.Approaching;
        rb.useGravity   = false;
        rb.linearVelocity     = Vector3.zero;
    }

    void SetAliveVisuals(bool on)  //handes visual fade when spawning and despawning
    {
        if (rends != null) foreach (var r in rends) if (r) r.enabled = on;
        if (cols != null) foreach (var c in cols) if (c) c.enabled = on;
    }

    void StartHit()  //moves bird when hit with ball
    {
        currentState = BirdState.Hit;
        hitTimer = hitDuration;
        targetZ = -5f;
    }

    void StartExitTimer() //start timer for exit if enabled
    {
        if (!enableAutoExit) return;
        if (_exitCo != null) StopCoroutine(_exitCo);
        _exitCo = StartCoroutine(ExitTimerRoutine());
    }

    IEnumerator ExitTimerRoutine()  //exit timer
    {
        float wait = Random.Range(exitDelayRange.x, exitDelayRange.y);
        yield return new WaitForSeconds(wait);
        BeginExit();
    }

    void BeginExit()  //direction to move too when exiting and switch to exit state
    {
        if (currentState == BirdState.Falling) return; 

        float side = (Random.value < 0.5f) ? -1f : 1f;
        Vector3 here = transform.position;

        _exitTarget = new Vector3(
        here.x + side * Mathf.Abs(exitXDistance),
        here.y + Mathf.Abs(exitYUp),
        0f
        );

        rb.useGravity = false;
        currentState  = BirdState.Exiting;
    }

    void DespawnImmediate()  //despawns when activated
    {
        SetAliveVisuals(false);
        Destroy(gameObject);
    }


    void SpawnFeathers(int count)  //handes feather spawning
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
