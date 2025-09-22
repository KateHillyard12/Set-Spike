using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PauseMenu : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject pauseMenuRoot;   // whole pause menu panel
    [SerializeField] private GameObject controlsPanel;   // child panel
    [SerializeField] private GameObject rulesPanel;      // child panel

    [Header("Optional: Use an existing InputAction called \"Pause\" in your maps")]
    [Tooltip("If left empty, a fallback action (Esc/Start) is created automatically.")]
    [SerializeField] private InputActionReference pauseActionRef;

    private bool isPaused = false;

    // Track per-player subscriptions so both players can pause
    private readonly List<InputAction> _subscribedPlayerActions = new();

    // Global fallback (Esc / Start) in case you didn’t create a Pause action
    private InputAction _fallbackPause;

    // Instance ref to PlayerInputManager for join events (non-static)
    private PlayerInputManager _pim;

    void Awake()
    {
        if (pauseMenuRoot) pauseMenuRoot.SetActive(false);
        if (controlsPanel) controlsPanel.SetActive(false);
        if (rulesPanel)    rulesPanel.SetActive(false);

        Time.timeScale = 1f;
        AudioListener.pause = false;
    }

    void OnEnable()
    {
        // Subscribe to existing players (use the non-obsolete API)
        var players = Object.FindObjectsByType<PlayerInput>(FindObjectsSortMode.None);
        foreach (var pi in players)
            SubscribePlayer(pi);

        // Subscribe to future joins via the INSTANCE of PlayerInputManager
        _pim = Object.FindAnyObjectByType<PlayerInputManager>();
        if (_pim != null)
        {
            // was: _pim.playerJoinedEvent += SubscribePlayer;
            _pim.playerJoinedEvent.AddListener(SubscribePlayer);
        }


        // Global fallback (Esc/Start) if no action reference provided
        if (pauseActionRef == null)
        {
            _fallbackPause = new InputAction(type: InputActionType.Button);
            _fallbackPause.AddBinding("<Keyboard>/escape");
            _fallbackPause.AddBinding("<Gamepad>/start");
            _fallbackPause.performed += OnPausePerformed;
            _fallbackPause.Enable();
        }
        else
        {
            pauseActionRef.action.performed += OnPausePerformed;
            pauseActionRef.action.Enable();
        }
    }

    void OnDisable()
    {
        if (_pim != null)
        {
            // was: _pim.playerJoinedEvent -= SubscribePlayer;
            _pim.playerJoinedEvent.RemoveListener(SubscribePlayer);
            _pim = null;
        }


        // Unhook per-player actions
        foreach (var act in _subscribedPlayerActions)
            act.performed -= OnPausePerformed;
        _subscribedPlayerActions.Clear();

        // Tear down fallback
        if (_fallbackPause != null)
        {
            _fallbackPause.performed -= OnPausePerformed;
            _fallbackPause.Disable();
            _fallbackPause.Dispose();
            _fallbackPause = null;
        }

        if (pauseActionRef != null)
            pauseActionRef.action.performed -= OnPausePerformed;
    }

    private void SubscribePlayer(PlayerInput pi)
    {
        if (pi != null && pi.actions != null)
        {
            var pause = pi.actions.FindAction("Pause", throwIfNotFound: false);
            if (pause != null)
            {
                pause.performed += OnPausePerformed;
                if (!pause.enabled) pause.Enable();
                _subscribedPlayerActions.Add(pause);
            }
        }
    }

    private void OnPausePerformed(InputAction.CallbackContext ctx)
    {
        if (!isPaused) Pause();
        else Resume(); // toggle
    }

    public void Pause()
    {
        isPaused = true;
        if (pauseMenuRoot) pauseMenuRoot.SetActive(true);
        if (controlsPanel) controlsPanel.SetActive(false);
        if (rulesPanel)    rulesPanel.SetActive(false);

        Time.timeScale = 0f;         // freeze game
        AudioListener.pause = true;  // pause audio
    }

    public void Resume()
    {
        isPaused = false;
        if (pauseMenuRoot) pauseMenuRoot.SetActive(false);
        if (controlsPanel) controlsPanel.SetActive(false);
        if (rulesPanel)    rulesPanel.SetActive(false);

        Time.timeScale = 1f;         // unfreeze
        AudioListener.pause = false;
    }

    public void ShowControls()
    {
        if (controlsPanel) controlsPanel.SetActive(true);
        if (rulesPanel)    rulesPanel.SetActive(false);
    }

    public void ShowRules()
    {
        if (rulesPanel)    rulesPanel.SetActive(true);
        if (controlsPanel) controlsPanel.SetActive(false);
    }

    public void BackFromSubpanel()
    {
        if (controlsPanel) controlsPanel.SetActive(false);
        if (rulesPanel)    rulesPanel.SetActive(false);
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
