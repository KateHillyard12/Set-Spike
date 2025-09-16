using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Cinemachine;

public class CameraTargetGroupBinder : MonoBehaviour
{
    public CinemachineTargetGroup group;

    [Header("Weights / Radii")]
    public float playerWeight = 1f;
    public float playerRadius = 0.8f;
    public float ballWeight   = 2.5f;
    public float ballRadius   = 0.5f;

    void Awake()
    {
        if (!group) group = Object.FindFirstObjectByType<CinemachineTargetGroup>();
    }

    void OnEnable()
    {
        RefreshMembers();
        var pim = Object.FindFirstObjectByType<PlayerInputManager>();
        if (pim) pim.onPlayerJoined += OnPlayerJoined;
    }

    void OnDisable()
    {
        var pim = Object.FindFirstObjectByType<PlayerInputManager>();
        if (pim) pim.onPlayerJoined -= OnPlayerJoined;
    }

    void OnPlayerJoined(PlayerInput _) => RefreshMembers();

    public void RefreshMembers()
    {
        if (!group) return;

        // Clear and rebuild (robust & simple)
        group.Targets.Clear();

        // Players
        var players = Object.FindObjectsByType<PlayerMovement2D>(FindObjectsSortMode.None);
        foreach (var p in players)
            group.AddMember(p.transform, playerWeight, playerRadius);

        // Ball (single)
        var ball = Object.FindFirstObjectByType<BallController>();
        if (ball)
            group.AddMember(ball.transform, ballWeight, ballRadius);
    }
}
