using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Cinemachine;
using System.Collections;   // <- add this

public class PlayerSpawner : MonoBehaviour
{
    [Header("Spawns (scene objects)")]
    public Transform leftSpawn;   // P1
    public Transform rightSpawn;  // P2

    [Header("Appearance (optional)")]
    public Material p1Material;
    public Material p2Material;

    public bool setLaneZFromSpawn = true;

    [Header("Camera Group Framing")]
    [Tooltip("Drag your CinemachineGroupFraming component here (on the vcam).")]
    public MonoBehaviour groupFramingComponent;  // e.g. CinemachineGroupFraming

    int joinedCount = 0;
    bool groupFramingForcedOn = false;

    // Hook this in the PlayerInputManager inspector: "Player Joined Event"
    public void HandleJoined(PlayerInput pi)
    {
        bool isP1 = pi.playerIndex == 0;
        Transform spawn = isP1 ? leftSpawn : rightSpawn;

        if (!spawn)
        {
            Debug.LogWarning("PlayerSpawner: Missing spawn transform.");
            return;
        }

        // Place/zero motion
        var rb = pi.GetComponent<Rigidbody>();
        if (rb)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.position = spawn.position;
            rb.rotation = spawn.rotation;
        }
        else
        {
            pi.transform.SetPositionAndRotation(spawn.position, spawn.rotation);
        }

        // Keep lane Z consistent (your 2.5D lock)
        if (setLaneZFromSpawn)
        {
            var mover = pi.GetComponent<PlayerMovement2D>();
            if (mover) mover.laneZ = spawn.position.z;
        }

        // Optional tint
        var look = pi.GetComponent<PlayerAppearance>();
        if (look) look.Apply(isP1 ? p1Material : p2Material);

        Debug.Log($"[Spawner] Joined P{pi.playerIndex} → {(isP1 ? "LEFT" : "RIGHT")} @ {spawn.position}");

        // --- NEW: enable Cinemachine Group Framing once we have the first player ---
        joinedCount++;

        if (!groupFramingForcedOn && joinedCount == 1 && groupFramingComponent != null)
        {
            // wait a frame so TargetGroupAutoRegister has time to add the player
            StartCoroutine(EnableGroupFramingNextFrame());
        }
    }

    IEnumerator EnableGroupFramingNextFrame()
    {
        yield return null; // let TargetGroupAutoRegister run its own "yield return null"

        if (groupFramingComponent != null)
        {
            groupFramingComponent.enabled = true;
            groupFramingForcedOn = true;
            Debug.Log("[Spawner] Enabled Cinemachine Group Framing after first player joined.");
        }
    }
}
