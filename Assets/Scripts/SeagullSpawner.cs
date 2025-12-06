using System.Collections;
using UnityEngine;

public class SeagullSpawner : MonoBehaviour
{
    [Header("Prefab")]
    [Tooltip("Seagull prefab with SeagullController + collider + rigidbody.")]
    public GameObject seagullPrefab;

    [Header("Court Edges")]
    [Tooltip("Roughly the left side of your court (can reuse left player spawn).")]
    public Transform leftEdge;
    [Tooltip("Roughly the right side of your court (can reuse right player spawn).")]
    public Transform rightEdge;

    [Header("Spawn / Despawn")]
    [Tooltip("Height where seagulls fly.")]
    public float flightHeight = 5f;
    [Tooltip("Spawn offset beyond the court edge so it slides in from offscreen.")]
    public float spawnOffsetX = 6f;
    [Tooltip("How far beyond the opposite edge before it despawns.")]
    public float despawnExtraX = 8f;

    [Header("Timing")]
    [Tooltip("Min seconds between possible spawns.")]
    public float minSpawnDelay = 5f;
    [Tooltip("Max seconds between possible spawns.")]
    public float maxSpawnDelay = 12f;
    [Tooltip("If true, only one active seagull at a time.")]
    public bool onlyOneAtATime = true;

    SeagullController activeSeagull;

    IEnumerator Start()
    {
        if (!seagullPrefab || !leftEdge || !rightEdge)
        {
            Debug.LogWarning("[SeagullSpawner] Missing references.");
            yield break;
        }

        while (true)
        {
            float delay = Random.Range(minSpawnDelay, maxSpawnDelay);
            yield return new WaitForSeconds(delay);

            if (onlyOneAtATime && activeSeagull != null)
                continue;

            bool fromLeft = Random.value < 0.5f;
            SpawnOne(fromLeft);
        }
    }

    void SpawnOne(bool fromLeft)
    {
        float baseZ = leftEdge.position.z; // works fine for your 2.5D lane

        float spawnX = fromLeft
            ? leftEdge.position.x - spawnOffsetX
            : rightEdge.position.x + spawnOffsetX;

        float targetX = fromLeft
            ? rightEdge.position.x + despawnExtraX
            : leftEdge.position.x - despawnExtraX;

        Vector3 spawnPos = new Vector3(spawnX, flightHeight, baseZ);

        GameObject g = Instantiate(seagullPrefab, spawnPos, Quaternion.identity);
        activeSeagull = g.GetComponent<SeagullController>();

        if (activeSeagull != null)
        {
            Vector3 dir = fromLeft ? Vector3.right : Vector3.left;
            activeSeagull.Init(dir, targetX, this);
        }
        else
        {
            Debug.LogWarning("[SeagullSpawner] Spawned seagullPrefab has no SeagullController.");
        }
    }

    public void NotifySeagullDestroyed(SeagullController controller)
    {
        if (activeSeagull == controller)
            activeSeagull = null;
    }
}
