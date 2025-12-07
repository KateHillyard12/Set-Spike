using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using TMPro;
using System.Collections;
using Unity.Cinemachine;


public enum CourtSide { Left, Right }

public class VolleyballGameManager : MonoBehaviour
{
    [Header("Scene Refs")]
    public Transform net;
    public GameObject ballPrefab;
    public LayerMask groundLayer;
    public float laneZ = 0f;
    public GameUIController uiController; // Drag the GameUIController here in inspector

    [Header("Player Spawns / Court")]
    public Transform leftSpawn;    // Player 1 home side
    public Transform rightSpawn;   // Player 2 home side
    [Tooltip("How high above the server we spawn the ball.")]
    public float serveOffsetY = 2.0f;

    [Tooltip("World X where LEFT is considered out-of-bounds behind P1. (should be a little past left player's court edge)")]
    public float leftBoundaryX = -15f;

    [Tooltip("World X where RIGHT is considered out-of-bounds behind P2.")]
    public float rightBoundaryX = 15f;

    [Header("Serve Physics")]
    public float serveSpeedX = 8f;
    public float serveSpeedY = 2f;
    public float respawnDelay = 1.25f;

    [Header("Scoring / Win")]
    public int winScore = 15;
    public float bannerSeconds = 3f;
    public bool reloadSceneOnWin = false;

    [Header("Start Gate & Countdown")]
    public int requiredPlayers = 2;
    public float preServeCountdown = 3f;

    int p1Score, p2Score;
    BallController currentBall;
    bool matchOver;
    bool readyToServe;
    Coroutine countdownRoutine;

    //hank here
    // Global freeze used by PlayerMovement2D to lock input/motion during win sequences.
    public static bool freezePlayers = false;

    // If Left = left player will serve next. If Right = right player will serve next.
    CourtSide nextServeSide = CourtSide.Left;

    void Start()
    {
        UpdateUI();
        TryStartCountdownWhenReady();
    }

    void Update()
    {
        if (!matchOver && !readyToServe && countdownRoutine == null)
            TryStartCountdownWhenReady();
    }

    void TryStartCountdownWhenReady()
    {
        if (GetJoinedCount() >= requiredPlayers)
            countdownRoutine = StartCoroutine(CountdownThenServe());
        else
        {
            // TODO: Add "Waiting for players" UI element in GameUI.uxml
            if (uiController) uiController.ShowWaitingForPlayers();
        }
    }

    int GetJoinedCount() =>
        Object.FindObjectsByType<PlayerInput>(FindObjectsSortMode.None).Length;

    IEnumerator CountdownThenServe()
    {
        // snap both players to their home spawns for fairness
        ResetPlayersToSpawns();

        float t = preServeCountdown;
        while (t > 0f)
        {
            if (GetJoinedCount() < requiredPlayers)
            {
                // someone dipped, cancel
                if (uiController) uiController.ShowWaitingForPlayers();
                countdownRoutine = null;
                yield break;
            }

            if (uiController)
                uiController.ShowCountdown(Mathf.CeilToInt(t));

            yield return new WaitForSeconds(1f);
            t -= 1f;
        }

        if (uiController)
        {
            uiController.ShowCountdown(0); // Show "Go!"
            yield return new WaitForSeconds(0.5f);
            uiController.HideCountdown();
        }

        readyToServe = true;
        countdownRoutine = null;
        StartServe();
    }

    public void StartServe()
    {
        matchOver = false;

        // figure out where to spawn based on which side is serving
        Transform serveFrom = (nextServeSide == CourtSide.Left) ? leftSpawn : rightSpawn;
        Vector3 spawnPos = serveFrom.position + new Vector3(0f, serveOffsetY, 0f);
        spawnPos.z = laneZ; // lock lane

        var go = Instantiate(ballPrefab, spawnPos, Quaternion.identity);

        currentBall = go.GetComponent<BallController>();
        currentBall.manager     = this;
        currentBall.net         = net;
        currentBall.groundLayer = groundLayer;
        currentBall.laneZ       = laneZ;

        // give it boundaries so it can call out-of-bounds
        currentBall.leftBoundaryX  = leftBoundaryX;
        currentBall.rightBoundaryX = rightBoundaryX;

        // launch toward the receiving player
        currentBall.Launch(Vector3.zero);
    }


    public void PointScored(CourtSide groundSide)
    {
        if (matchOver) return;

        

        if (groundSide == CourtSide.Left) 
        {
            // ball hit left ground => RIGHT player scored
            p2Score++;
            nextServeSide = CourtSide.Right;
        }
        else 
        {
            // ball hit right ground => LEFT player scored
            p1Score++;
            nextServeSide = CourtSide.Left;
        }

        UpdateUI();

        // check win
        if (p1Score >= winScore || p2Score >= winScore)
        {
            matchOver = true;
            int winningPlayer = p1Score > p2Score ? 0 : 1;

            if (uiController)
                uiController.ShowVictoryBanner(winningPlayer);
            
            GameAudio.Instance?.PlayVictory();

            if (currentBall) Destroy(currentBall.gameObject);
            StartCoroutine(RestartRoutine());
            return;
        }

        // rally over -> kill old ball, serve from the scoring side after a delay
        if (currentBall) Destroy(currentBall.gameObject);
        StartCoroutine(ServeLater());
    }

    IEnumerator ServeLater()
    {
        yield return new WaitForSeconds(respawnDelay);
        StartServe(); // no new countdown mid-match
    }

    IEnumerator RestartRoutine()
    {
        yield return new WaitForSeconds(bannerSeconds);

        if (reloadSceneOnWin)
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            yield break;
        }

        // soft reset for rematch
        p1Score = 0;
        p2Score = 0;
        UpdateUI();
        if (uiController) uiController.HideVictoryBanner();

        ResetPlayersToSpawns();

        // start over needing countdown again
        readyToServe = false;
        nextServeSide = CourtSide.Left; // default first serve again
        yield return new WaitForSeconds(0.25f);

        if (countdownRoutine != null) StopCoroutine(countdownRoutine);
        countdownRoutine = StartCoroutine(CountdownThenServe());
    }

    void ResetPlayersToSpawns()
    {
        var players = Object.FindObjectsByType<PlayerMovement2D>(FindObjectsSortMode.None);
        foreach (var m in players)
        {
            var pi = m.GetComponent<PlayerInput>();
            if (!pi) continue;

            bool isP1 = pi.playerIndex == 0;
            Transform spawn = isP1 ? leftSpawn : rightSpawn;
            if (!spawn) continue;

            var rb = m.GetComponent<Rigidbody>();
            if (rb)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
                rb.position = spawn.position;
                rb.rotation = spawn.rotation;
            }
            else
            {
                m.transform.SetPositionAndRotation(spawn.position, spawn.rotation);
            }

            m.laneZ = spawn.position.z;
        }
    }

    void UpdateUI()
    {
        if (uiController) uiController.UpdateScores(p1Score, p2Score);
        GameAudio.Instance?.PlayScore();
    }
}
