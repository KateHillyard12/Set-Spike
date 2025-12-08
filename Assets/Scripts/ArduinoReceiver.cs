using System;
using System.IO.Ports;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class ArduinoReceiver : MonoBehaviour
{
    [Header("Serial Settings")]
    [SerializeField] private string portName = "COM4";
    [SerializeField] private int baudRate = 9600;
    [SerializeField] private int readTimeoutMs = 25;

    [Header("Player Join")]
    public bool autoJoinArduinoPlayer = true;

    [Tooltip("Leave -1 for automatic assignment. Use 0 for P1 or 1 for P2.")]
    public int desiredPlayerIndex = -1;

    private SerialPort serialPort;
    private PlayerMovement2D arduinoPlayerMovement;
    private PlayerInput arduinoPlayerInput;
    private bool arduinoPlayerJoined = false;

    [Header("Debounce Settings")]
    public float navigationCooldown = 0.25f;
    private float lastNavTime = 0f;

    void Start()
    {
        try
        {
            var ports = SerialPort.GetPortNames();
            bool portAvailable = ports.Any(p => string.Equals(p, portName, StringComparison.OrdinalIgnoreCase));

            if (!portAvailable)
            {
                Debug.LogWarning($"ArduinoReceiver: Port '{portName}' not found.");
                enabled = false;
                return;
            }

            serialPort = new SerialPort(portName, baudRate)
            {
                ReadTimeout = readTimeoutMs
            };

            serialPort.Open();
            Debug.Log($"ArduinoReceiver: Opened {portName}");
        }
        catch (Exception e)
        {
            Debug.LogWarning($"ArduinoReceiver: Could not open port: {e.Message}");
            enabled = false;
        }
    }

    private void EnsureArduinoPlayer()
    {
        if (arduinoPlayerMovement != null)
            return;

        if (autoJoinArduinoPlayer && !arduinoPlayerJoined)
        {
            var pim = PlayerInputManager.instance;

            if (pim != null)
            {
                PlayerInput pi = desiredPlayerIndex >= 0
                    ? pim.JoinPlayer(desiredPlayerIndex)
                    : pim.JoinPlayer();

                arduinoPlayerJoined = true;
                arduinoPlayerInput = pi;
                arduinoPlayerMovement = pi.GetComponent<PlayerMovement2D>();

                Debug.Log($"ArduinoReceiver: Arduino joined as Player {pi.playerIndex}");
                return;
            }
        }

        arduinoPlayerMovement = FindAnyObjectByType<PlayerMovement2D>();
    }

    void Update()
    {
        if (serialPort == null || !serialPort.IsOpen) return;

        try
        {
            string line = serialPort.ReadLine().Trim();
            if (string.IsNullOrWhiteSpace(line)) return;

            if (line == "MENU")
            {
                HandleMenuButton();
                return;
            }

            if (line == "JUMP")
            {
                HandleJumpButton();
                return;
            }

            if (line.StartsWith("MOVE:"))
            {
                string[] parts = line.Split(':');
                if (float.TryParse(parts[1], out float x))
                {
                    EnsureArduinoPlayer();
                    arduinoPlayerMovement?.ExternalMove(Mathf.Clamp(x, -1f, 1f));
                }
                return;
            }
        }
        catch (TimeoutException) { }
        catch (Exception e)
        {
            Debug.LogWarning($"ArduinoReceiver Error: {e.Message}");
        }
    }

    // ---------------------------------------------------------
    // MENU BUTTON (OPEN PAUSE or SUBMIT)
    // ---------------------------------------------------------
    void HandleMenuButton()
    {
        var pause = FindAnyObjectByType<PauseMenuController>();

        Debug.Log($"MENU pressed. PauseVisible? {pause?.IsMenuVisible()}");

        // Gameplay → open pause
        if (pause != null && !pause.IsMenuVisible())
        {
            pause.TogglePauseFromExternal();
            return;
        }

        // Pause menu open → submit currently highlighted pause button
        if (pause != null && pause.IsMenuVisible())
        {
            Debug.Log("MENU pressed -> Submit UI Toolkit Button");

            var nav = FindAnyObjectByType<UIToolkitNavigator>();
            if (nav != null)
            {
                nav.Submit();
            }
            else
            {
                Debug.LogWarning("UIToolkitNavigator not found in scene.");
            }

            return;
        }

        // (Optional) Canvas main-menu fallback if you use AccessibleMenuNavigator
        var menuNavs = FindObjectsByType<AccessibleMenuNavigator>(FindObjectsSortMode.None);
        foreach (var n in menuNavs)
            n.ExternalSubmit();
    }


    // ---------------------------------------------------------
    // JUMP BUTTON (SCROLL or JUMP)
    // ---------------------------------------------------------
    void HandleJumpButton()
    {
        var pause = FindAnyObjectByType<PauseMenuController>();
        bool menuOpen = pause != null && pause.IsMenuVisible();
        Debug.Log($"MenuOpen = {menuOpen}");

        // If pause menu is open → use it for navigation
        if (menuOpen)
        {
            if (Time.unscaledTime - lastNavTime < navigationCooldown)
                return;

            lastNavTime = Time.unscaledTime;

            var nav = FindAnyObjectByType<UIToolkitNavigator>();
            if (nav != null)
            {
                nav.NavigateDown();
            }
            return;
        }

        // If pause menu is NOT open → ALWAYS treat JUMP as gameplay jump
        EnsureArduinoPlayer();
        arduinoPlayerMovement?.ExternalJump();
    }

}
