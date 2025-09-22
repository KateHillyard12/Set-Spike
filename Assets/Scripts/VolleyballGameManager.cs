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

    [Header("Spawns for Reset")]
    public Transform leftSpawn;   // P1
    public Transform rightSpawn;  // P2

    [Header("Serve")]
    public float serveHeight = 3.5f;
    public float serveSpeedX = 8f;
    public float serveSpeedY = 2f;
    public float respawnDelay = 1.25f;

    [Header("Scoring / Win")]
    public int winScore = 15;
    public TMP_Text p1Text;                 // optional
    public TMP_Text p2Text;                 // optional
    public TMP_Text bannerText;             // "Player X WINS!"
    public float bannerSeconds = 3f;
    public bool reloadSceneOnWin = false;

    [Header("Start Gate & Countdown")]
    public int requiredPlayers = 2;
    public TMP_Text countdownText;          // big centered "3,2,1, Go!"
    public float preServeCountdown = 3f;    // seconds

    int p1Score, p2Score;
    BallController currentBall;
    bool matchOver;
    bool readyToServe;                      // true once initial countdown has happened
    Coroutine countdownRoutine;

    void Start()
    {
        UpdateUI();
        // Do NOT serve yet – wait for 2 players.
        TryStartCountdownWhenReady();
    }

    void Update()
    {
        // Poll so this works no matter how PlayerInputManager is configured
        if (!matchOver && !readyToServe && countdownRoutine == null)
            TryStartCountdownWhenReady();
    }

    void TryStartCountdownWhenReady()
    {
        if (GetJoinedCount() >= requiredPlayers)
            countdownRoutine = StartCoroutine(CountdownThenServe());
        else if (countdownText)
        {
            countdownText.gameObject.SetActive(true);
            countdownText.text = "Waiting for players…";
        }
    }

    int GetJoinedCount() => Object.FindObjectsByType<PlayerInput>(FindObjectsSortMode.None).Length;

    IEnumerator CountdownThenServe()
    {
        // Make sure both players are at their spawns
        ResetPlayersToSpawns();

        float t = preServeCountdown;
        while (t > 0f)
        {
            if (GetJoinedCount() < requiredPlayers)
            {
                // Someone left – abort countdown
                if (countdownText) countdownText.text = "Waiting for players…";
                countdownRoutine = null;
                yield break;
            }

            if (countdownText)
            {
                countdownText.gameObject.SetActive(true);
                countdownText.text = Mathf.CeilToInt(t).ToString();
            }
            yield return new WaitForSeconds(1f);
            t -= 1f;
        }

        if (countdownText)
        {
            countdownText.text = "Go!";
            yield return new WaitForSeconds(0.5f);
            countdownText.gameObject.SetActive(false);
        }

        readyToServe = true;
        countdownRoutine = null;
        StartServe();
    }

    public void StartServe()
    {
        matchOver = false;

        Vector3 pos = new Vector3(net.position.x, net.position.y + serveHeight, laneZ);
        var go = Instantiate(ballPrefab, pos, Quaternion.identity);


        currentBall = go.GetComponent<BallController>();
        currentBall.manager     = this;
        currentBall.net         = net;
        currentBall.groundLayer = groundLayer;
        currentBall.laneZ       = laneZ;

        float dir = Random.value < 0.5f ? -1f : 1f;
        currentBall.Launch(new Vector3(dir * serveSpeedX, serveSpeedY, 0f));



    }

    public void PointScored(CourtSide groundSide)
    {
        if (matchOver) return;

        if (groundSide == CourtSide.Left) p2Score++; else p1Score++;
        UpdateUI();

        if (p1Score >= winScore || p2Score >= winScore)
        {
            matchOver = true;
            string winner = p1Score > p2Score ? "Player 1" : "Player 2";

            if (bannerText)
            {
                bannerText.gameObject.SetActive(true);
                bannerText.text = $"{winner} WINS!";
            }

            if (currentBall) Destroy(currentBall.gameObject);
            StartCoroutine(RestartRoutine());
            return;
        }

        // Normal rally end -> serve again after a short delay
        if (currentBall) Destroy(currentBall.gameObject);
        StartCoroutine(ServeLater());
    }

    IEnumerator ServeLater()
    {
        yield return new WaitForSeconds(respawnDelay);
        StartServe(); // countdown only happens at match start / after game reset
    }

    IEnumerator RestartRoutine()
    {
        yield return new WaitForSeconds(bannerSeconds);

        if (reloadSceneOnWin)
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            yield break;
        }

        // Soft reset: scores → 0, players back to spawns, new countdown
        p1Score = 0; p2Score = 0;
        UpdateUI();
        if (bannerText) bannerText.gameObject.SetActive(false);

        ResetPlayersToSpawns();
        readyToServe = false;            // require countdown again
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
        if (p1Text) p1Text.text = p1Score.ToString();
        if (p2Text) p2Text.text = p2Score.ToString();
    }
}
