using System;
using System.IO.Ports;
using System.Linq;
using UnityEngine;

public class ArduinoReceiver : MonoBehaviour
{
    [Header("Serial")]
    [SerializeField] string portName = "COM4";
    [SerializeField] int baudRate = 9600;
    [SerializeField] int readTimeoutMs = 25;

    SerialPort serialPort;

    void Start()
    {
        try
        {
            var ports = SerialPort.GetPortNames();
            bool portAvailable = ports.Any(p => string.Equals(p, portName, StringComparison.OrdinalIgnoreCase));
            if (!portAvailable)
            {
                Debug.LogWarning($"Serial: '{portName}' not found. Disabling ArduinoReceiver.");
                enabled = false;
                return;
            }

            serialPort = new SerialPort(portName, baudRate)
            {
                ReadTimeout = readTimeoutMs
            };
            serialPort.Open();
        }
        catch (Exception e)
        {
            Debug.LogWarning($"Serial open failed: {e.Message}. Disabling ArduinoReceiver.");
            enabled = false;
        }
    }

    void OnDisable()
    {
        if (serialPort != null)
        {
            try { if (serialPort.IsOpen) serialPort.Close(); } catch { /* ignore */ }
            serialPort.Dispose();
            serialPort = null;
        }
    }

    void Update()
    {
        if (serialPort == null || !serialPort.IsOpen) return;

        try
        {
            // Example protocol: "JUMP" or "MOVE:-1..1"
            string line = serialPort.ReadLine().Trim();

            if (string.Equals(line, "JUMP", StringComparison.OrdinalIgnoreCase))
            {
                foreach (var p in UnityEngine.Object.FindObjectsByType<PlayerMovement2D>(FindObjectsSortMode.None))
                    p.ExternalJump();
            }
            else if (line.StartsWith("MOVE:", StringComparison.OrdinalIgnoreCase))
            {
                var parts = line.Split(':');
                if (parts.Length == 2 && float.TryParse(parts[1], out float x))
                {
                    x = Mathf.Clamp(x, -1f, 1f);
                    foreach (var p in UnityEngine.Object.FindObjectsByType<PlayerMovement2D>(FindObjectsSortMode.None))
                        p.ExternalMove(x);
                }
            }
        }
        catch (TimeoutException) { /* no data this frame */ }
        catch (Exception e)
        {
            Debug.LogWarning($"Serial read error: {e.Message}");
        }
    }
}
