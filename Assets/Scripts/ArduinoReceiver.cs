using System;
using System.IO.Ports;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class ArduinoReceiver : MonoBehaviour
{
    [Header("Serial")]
    [SerializeField] private string portName = "COM4";   // match Arduino IDE
    [SerializeField] private int baudRate = 9600;
    [SerializeField] private int readTimeoutMs = 25;

    [Header("Player Join")]
    [Tooltip("If true, first Arduino input will auto-join a new player via PlayerInputManager.")]
    [SerializeField] private bool autoJoinArduinoPlayer = true;

    [Tooltip("Leave -1 to join as 'next free player'. Set 0/1 if you want to force P1/P2.")]
    [SerializeField] private int desiredPlayerIndex = -1;

    private SerialPort serialPort;

    private bool arduinoPlayerJoined = false;
    private PlayerMovement2D arduinoPlayerMovement;

    void Start()
    {
        try
        {
            var ports = SerialPort.GetPortNames();
            bool portAvailable = ports.Any(p => string.Equals(p, portName, StringComparison.OrdinalIgnoreCase));
            if (!portAvailable)
            {
                Debug.LogWarning($"ArduinoReceiver: Port '{portName}' not found. Available: {string.Join(", ", ports)}");
                enabled = false;
                return;
            }

            serialPort = new SerialPort(portName, baudRate)
            {
                ReadTimeout = readTimeoutMs
            };
            serialPort.Open();
            Debug.Log($"ArduinoReceiver: Opened {portName} @ {baudRate}");
        }
        catch (Exception e)
        {
            Debug.LogWarning($"ArduinoReceiver: Failed to open port {portName} → {e.Message}");
            enabled = false;
        }
    }

    void OnDisable()
    {
        if (serialPort != null)
        {
            try
            {
                if (serialPort.IsOpen) serialPort.Close();
            }
            catch { /* ignore */ }

            serialPort.Dispose();
            serialPort = null;
        }
    }

    // --- Ensure we have a player for the Arduino to control ---
    private void EnsureArduinoPlayer()
    {
        if (arduinoPlayerMovement != null) return;

        // If we should auto-join, ask the PlayerInputManager to spawn one
        if (autoJoinArduinoPlayer && !arduinoPlayerJoined)
        {
            var pim = PlayerInputManager.instance;
            if (pim != null)
            {
                PlayerInput pi;
                if (desiredPlayerIndex >= 0)
                    pi = pim.JoinPlayer(desiredPlayerIndex);
                else
                    pi = pim.JoinPlayer(); // next slot

                arduinoPlayerJoined = true;
                arduinoPlayerMovement = pi.GetComponent<PlayerMovement2D>();

                if (arduinoPlayerMovement == null)
                    Debug.LogWarning("ArduinoReceiver: Joined player but couldn't find PlayerMovement2D on it.");

                Debug.Log($"ArduinoReceiver: Arduino joined as player {pi.playerIndex}");
                return;
            }
        }

        // Fallback: just grab any PlayerMovement2D if there is no PlayerInputManager
        if (arduinoPlayerMovement == null)
        {
            arduinoPlayerMovement = FindAnyObjectByType<PlayerMovement2D>();
            if (arduinoPlayerMovement != null)
                Debug.Log("ArduinoReceiver: Using first PlayerMovement2D found in scene as Arduino player.");
        }
    }

    void Update()
    {
        if (serialPort == null || !serialPort.IsOpen) return;

        try
        {
            string raw = serialPort.ReadLine();
            if (string.IsNullOrWhiteSpace(raw)) return;

            string line = raw.Trim();

            // ---- Jump button ----
            if (string.Equals(line, "JUMP", StringComparison.OrdinalIgnoreCase))
            {
                EnsureArduinoPlayer();
                arduinoPlayerMovement?.ExternalJump();
                return;
            }

            // ---- Menu button (pause/submit) ----
            if (string.Equals(line, "MENU", StringComparison.OrdinalIgnoreCase))
            {
                // First, see if there is a PauseMenu in this scene
                var pause = FindAnyObjectByType<PauseMenu>();
                var navs  = FindObjectsByType<AccessibleMenuNavigator>(FindObjectsSortMode.None);

                if (pause != null)
                {
                    if (!pause.IsPaused)
                    {
                        // We are in gameplay → open pause menu
                        pause.TogglePauseFromExternal();
                    }
                    else
                    {
                        // Pause menu already open → treat as "submit" on the active menu
                        if (navs.Length > 0)
                        {
                            foreach (var n in navs)
                                n.ExternalSubmit();
                        }
                        else
                        {
                            // No navigator for some reason, just unpause
                            pause.TogglePauseFromExternal();
                        }
                    }
                }
                else
                {
                    // No PauseMenu in this scene (e.g. main menu) → MENU is just "submit"
                    if (navs.Length > 0)
                    {
                        foreach (var n in navs)
                            n.ExternalSubmit();
                    }
                }

                return;
            }


            // ---- Gameplay movement axis ----
            if (line.StartsWith("MOVE:", StringComparison.OrdinalIgnoreCase))
            {
                var parts = line.Split(':');
                if (parts.Length == 2 && float.TryParse(parts[1], out float x))
                {
                    EnsureArduinoPlayer();
                    arduinoPlayerMovement?.ExternalMove(Mathf.Clamp(x, -1f, 1f));
                }
                return;
            }

            // ---- Menu navigation axis ----
            if (line.StartsWith("NAV:", StringComparison.OrdinalIgnoreCase))
            {
                var parts = line.Split(':');
                if (parts.Length == 2 && int.TryParse(parts[1], out int dir))
                {
                    dir = Mathf.Clamp(dir, -1, 1);
                    var navs = FindObjectsByType<AccessibleMenuNavigator>(FindObjectsSortMode.None);
                    foreach (var n in navs)
                        n.ExternalNavigate(dir);
                }
                return;
            }
        }
        catch (TimeoutException)
        {
            // no data this frame, totally fine
        }
        catch (Exception e)
        {
            Debug.LogWarning($"ArduinoReceiver: Serial read error → {e.Message}");
        }
    }
}
