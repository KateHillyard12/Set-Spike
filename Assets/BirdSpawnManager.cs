using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BirdSpawner : MonoBehaviour
{
    [Header("Prefab & Limit")]
    public BirdAI birdPrefab;
    public int maxAlive = 10;

    [Header("Random Spawn Timing (s)")]
    public Vector2 spawnIntervalRange = new Vector2(1.5f, 4f);

    [Header("Bird Appearance Delay (s)")]
    public Vector2 spawnDelayRange = new Vector2(0.5f, 2.5f); 

    [Header("Final Target Area (Z locked to 0)")]
    public Vector2 xRange = new Vector2(-10f, 10f);
    public Vector2 yRange = new Vector2(7f, 12f);
    public float zFixed = 0f;

    [Header("Approach Spawn Offset")]
    public float approachHeight = 20f;
    public float approachDistance = 25f;
    public float approachSpeed = 12f;

    [Header("Parenting")]
    public bool parentUnderSpawner = true;
    private readonly List<BirdAI> _alive = new List<BirdAI>();
    private int _pendingSpawns = 0; 

    private void OnValidate()  //does caculations off of set varibles
    {
        spawnIntervalRange.x = Mathf.Max(0.05f, spawnIntervalRange.x);
        spawnIntervalRange.y = Mathf.Max(spawnIntervalRange.x, spawnIntervalRange.y);
        spawnDelayRange.x    = Mathf.Max(0f, spawnDelayRange.x);
        spawnDelayRange.y    = Mathf.Max(spawnDelayRange.x, spawnDelayRange.y);
    }

    private void Start() //stats spawn loop
    {
        StartCoroutine(SpawnLoop()); 
    }

    private IEnumerator SpawnLoop() //attempts to spawns in birds over time
    {
    
        while (true)
        {
            _alive.RemoveAll(b => b == null); 
            if (_alive.Count + _pendingSpawns < maxAlive)
                StartCoroutine(SpawnWithDelay());
            yield return new WaitForSeconds(Random.Range(spawnIntervalRange.x, spawnIntervalRange.y));
        }
    }

    private IEnumerator SpawnWithDelay() //delays spawn by set amount
    {
        _pendingSpawns++;
        float delay = Random.Range(spawnDelayRange.x, spawnDelayRange.y);
        yield return new WaitForSeconds(delay);

        _alive.RemoveAll(b => b == null); 
        if (_alive.Count < maxAlive)
            SpawnOne();

        _pendingSpawns--;
    }

    private void SpawnOne() //spawns in bird
    {
        if (!birdPrefab) return;

        Vector3 target = new Vector3(
            Random.Range(xRange.x, xRange.y),
            Random.Range(yRange.x, yRange.y),
            zFixed
        );

        float side = (Random.value < 0.5f) ? -1f : 1f;
        Vector3 spawnPos = new Vector3(
            target.x + side * approachDistance,
            target.y + approachHeight,
            zFixed
        );

        Transform parent = parentUnderSpawner ? transform : null;
        BirdAI bird = Instantiate(birdPrefab, spawnPos, Quaternion.identity, parent);

        Vector3 toTarget = target - spawnPos; toTarget.z = 0f;
        if (toTarget.sqrMagnitude > 0.001f)
            bird.transform.rotation = Quaternion.LookRotation(Vector3.forward, toTarget.normalized);

        bird.PrepareForApproach(target, approachSpeed);

        var hook = bird.gameObject.AddComponent<BirdDespawnHook>();
        hook.owner = this;
        hook.bird  = bird;

        _alive.Add(bird);
    }
    internal void NotifyBirdDestroyed(BirdAI bird)
    {
        _alive.Remove(bird);
    }
}

sealed class BirdDespawnHook : MonoBehaviour  //lets this script know if bird was destroyed
{
    internal BirdSpawner owner;
    internal BirdAI bird;

    private void OnDestroy()
    {
        if (owner != null) owner.NotifyBirdDestroyed(bird);
    }
}
