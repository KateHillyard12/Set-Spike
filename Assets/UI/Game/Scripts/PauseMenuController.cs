using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;
using UnityEngine.EventSystems;


/// <summary>
/// Handles the in-game pause menu using UI Toolkit.
/// Mirrors the Start menu flow: main button list plus Rules and Settings subpanels.
/// Toggles via Esc / Start / B (controller).
/// </summary>
public class PauseMenuController : MonoBehaviour
{
    // NOTE: This script should be attached to the SAME GameObject as GameUIController
    // Both scripts will share the UIDocument component on that GameObject
    private VisualElement root;
    private VisualElement pauseMenuRoot;
    private VisualElement mainPanel;
    private VisualElement rulesPanel;
    private VisualElement settingsPanel;

    private Button resumeButton;
    private Button settingsButton;
    private Button rulesButton;
    private Button quitButton;
    private Button backFromRulesButton;
    private Button backFromSettingsButton;

    private bool isPaused;
    private InputAction pauseAction;
    private PauseMenuAudio pauseAudio;

    // Remember cursor state so we can restore after pause
    private bool prevCursorVisible;
    private CursorLockMode prevCursorLock;

    void Awake()
    {
        // Ensure normal time/audio on load
        Time.timeScale = 1f;
        AudioListener.pause = false;
    }

    void OnEnable()
    {
        // Get UIDocument from this GameObject (shared with GameUIController)
        UIDocument uiDocument = GetComponent<UIDocument>();
        root = uiDocument != null ? uiDocument.rootVisualElement : null;
        if (root == null)
        {
            Debug.LogWarning("PauseMenuController: UIDocument not found on this GameObject. Make sure PauseMenuController is on the same GameObject as GameUIController.");
            return;
        }

        Debug.Log("PauseMenuController.OnEnable() - Setting up pause menu UI");

        // Get audio manager
        pauseAudio = GetComponent<PauseMenuAudio>();
        if (pauseAudio == null)
        {
            Debug.LogWarning("PauseMenuController: PauseMenuAudio not found on this GameObject. Audio will be disabled.");
        }

        // Query elements
        pauseMenuRoot = root.Q<VisualElement>("PauseMenu");
        mainPanel = root.Q<VisualElement>("PauseMainPanel");
        rulesPanel = root.Q<VisualElement>("PauseRulesPanel");
        settingsPanel = root.Q<VisualElement>("PauseSettingsPanel");

        resumeButton = root.Q<Button>("resume-button");
        settingsButton = root.Q<Button>("pause-settings-button");
        rulesButton = root.Q<Button>("pause-rules-button");
        quitButton = root.Q<Button>("pause-quit-button");
        backFromRulesButton = root.Q<Button>("pause-back-from-rules");
        backFromSettingsButton = root.Q<Button>("pause-back-from-settings");

        // Ensure the pause overlay itself can receive clicks
        if (pauseMenuRoot != null)
            pauseMenuRoot.pickingMode = PickingMode.Position;

        // Ensure all buttons can receive pointer events (CRITICAL for mouse interaction)
        if (resumeButton != null) resumeButton.pickingMode = PickingMode.Position;
        if (settingsButton != null) settingsButton.pickingMode = PickingMode.Position;
        if (rulesButton != null) rulesButton.pickingMode = PickingMode.Position;
        if (quitButton != null) quitButton.pickingMode = PickingMode.Position;
        if (backFromRulesButton != null) backFromRulesButton.pickingMode = PickingMode.Position;
        if (backFromSettingsButton != null) backFromSettingsButton.pickingMode = PickingMode.Position;

        // CRITICAL: Enable focusable on buttons so they work during pause (timeScale = 0)
        if (resumeButton != null) resumeButton.focusable = true;
        if (settingsButton != null) settingsButton.focusable = true;
        if (rulesButton != null) rulesButton.focusable = true;
        if (quitButton != null) quitButton.focusable = true;
        if (backFromRulesButton != null) backFromRulesButton.focusable = true;
        if (backFromSettingsButton != null) backFromSettingsButton.focusable = true;

        // Hook up buttons with null-safety logs
        SubscribeButtons();

        // Build pause input if needed
        if (pauseAction == null)
        {
            pauseAction = new InputAction(type: InputActionType.Button);
            pauseAction.AddBinding("<Keyboard>/escape");
            pauseAction.AddBinding("<Gamepad>/start");
            pauseAction.AddBinding("<Gamepad>/buttonEast"); // B / Circle / "back"
        }

        pauseAction.performed += OnPausePerformed;
        pauseAction.Enable();

        // Hide UI at start
        SetVisible(pauseMenuRoot, false);
        SetVisible(mainPanel, false);
        SetVisible(rulesPanel, false);
        SetVisible(settingsPanel, false);
    }

    void OnDisable()
    {
        UnsubscribeButtons();

        if (pauseAction != null)
        {
            pauseAction.performed -= OnPausePerformed;
            pauseAction.Disable();
        }

        if (isPaused)
        {
            Time.timeScale = 1f;
            if (GameAudio.Instance != null)
                GameAudio.Instance.ResumeMusic();
            isPaused = false;
        }
    }

    void OnDestroy()
    {
        if (pauseAction != null)
        {
            pauseAction.Dispose();
            pauseAction = null;
        }
    }

    private void OnPausePerformed(InputAction.CallbackContext ctx)
    {
        // If UI root missing, do nothing
        if (pauseMenuRoot == null)
            return;

        if (!isPaused)
        {
            Pause();
            return;
        }

        // Already paused: if in subpanel, go back; otherwise resume
        if (IsVisible(rulesPanel) || IsVisible(settingsPanel))
            ShowMainPanel();
        else
            Resume();
    }

    public void Pause()
    {
        if (isPaused)
            return;

        isPaused = true;
        Time.timeScale = 0f;
        
        // Pause game audio but NOT AudioListener (allows PauseMenuAudio to play)
        if (GameAudio.Instance != null)
            GameAudio.Instance.PauseMusic();

        // Unlock cursor for UI interaction and remember previous state
        prevCursorVisible = UnityEngine.Cursor.visible;
        prevCursorLock = UnityEngine.Cursor.lockState;
        UnityEngine.Cursor.visible = true;
        UnityEngine.Cursor.lockState = CursorLockMode.None;

        // Force EventSystem update to ensure UI responds during pause
        if (EventSystem.current != null)
        {
            EventSystem.current.sendNavigationEvents = true;
            EventSystem.current.SetSelectedGameObject(null);
        }

        // Trigger audio for pause menu
        if (pauseAudio != null)
            pauseAudio.OnPauseMenuShown();

        ShowMainPanel();
    }

    public void Resume()
    {
        Debug.Log("🎮 Resume() called!");
        
        if (!isPaused)
        {
            Debug.LogWarning("Resume() called but game is not paused");
            return;
        }

        isPaused = false;
        Time.timeScale = 1f;
        
        // Resume game audio
        if (GameAudio.Instance != null)
            GameAudio.Instance.ResumeMusic();

        // Restore cursor state
        UnityEngine.Cursor.visible = prevCursorVisible;
        UnityEngine.Cursor.lockState = prevCursorLock;

        HideAllPanels();
        Debug.Log("✅ Game resumed successfully");

        // Trigger audio for resume
        if (pauseAudio != null)
            pauseAudio.OnGameResumed();
    }

    public void TogglePauseFromExternal()
    {
        if (!isPaused) Pause();
        else Resume();
    }

    private void ShowMainPanel()
    {
        if (pauseMenuRoot == null)
            return;

        SetVisible(pauseMenuRoot, true);
        ShowRulesPanel(false);
        ShowSettingsPanel(false);
        SetVisible(mainPanel, true);

        FocusElement(resumeButton);
    }

    private void ShowRulesPanel()
    {
        ShowRulesPanel(true);
    }

    private void ShowRulesPanel(bool show)
    {
        if (pauseMenuRoot == null)
            return;

        SetVisible(pauseMenuRoot, true);
        SetVisible(mainPanel, show ? false : true);
        SetVisible(rulesPanel, show);
        SetVisible(settingsPanel, false);

        if (show)
            FocusElement(backFromRulesButton);
    }

    private void ShowSettingsPanel()
    {
        ShowSettingsPanel(true);
    }

    private void ShowSettingsPanel(bool show)
    {
        if (pauseMenuRoot == null)
            return;

        SetVisible(pauseMenuRoot, true);
        SetVisible(mainPanel, show ? false : true);
        SetVisible(rulesPanel, false);
        SetVisible(settingsPanel, show);

        if (show)
            FocusElement(backFromSettingsButton);
    }

    private void HideAllPanels()
    {
        SetVisible(pauseMenuRoot, false);
        SetVisible(mainPanel, false);
        SetVisible(rulesPanel, false);
        SetVisible(settingsPanel, false);
    }

    private void SubscribeButtons()
    {
        if (resumeButton != null) 
        {
            resumeButton.clicked += Resume;
            Debug.Log("✅ Resume button subscribed");
        }
        else Debug.LogWarning("PauseMenuController: 'resume-button' not found.");

        if (settingsButton != null) 
        {
            settingsButton.clicked += ShowSettingsPanel;
            Debug.Log("✅ Settings button subscribed");
        }
        else Debug.LogWarning("PauseMenuController: 'pause-settings-button' not found.");

        if (rulesButton != null) 
        {
            rulesButton.clicked += ShowRulesPanel;
            Debug.Log("✅ Rules button subscribed");
        }
        else Debug.LogWarning("PauseMenuController: 'pause-rules-button' not found.");

        if (quitButton != null) 
        {
            quitButton.clicked += QuitGame;
            Debug.Log("✅ Quit button subscribed");
        }
        else Debug.LogWarning("PauseMenuController: 'pause-quit-button' not found.");

        if (backFromRulesButton != null) 
        {
            backFromRulesButton.clicked += ShowMainPanel;
            Debug.Log("✅ Back from rules button subscribed");
        }
        else Debug.LogWarning("PauseMenuController: 'pause-back-from-rules' not found.");

        if (backFromSettingsButton != null) 
        {
            backFromSettingsButton.clicked += ShowMainPanel;
            Debug.Log("✅ Back from settings button subscribed");
        }
        else Debug.LogWarning("PauseMenuController: 'pause-back-from-settings' not found.");
    }

    private void UnsubscribeButtons()
    {
        if (resumeButton != null) resumeButton.clicked -= Resume;
        if (settingsButton != null) settingsButton.clicked -= ShowSettingsPanel;
        if (rulesButton != null) rulesButton.clicked -= ShowRulesPanel;
        if (quitButton != null) quitButton.clicked -= QuitGame;
        if (backFromRulesButton != null) backFromRulesButton.clicked -= ShowMainPanel;
        if (backFromSettingsButton != null) backFromSettingsButton.clicked -= ShowMainPanel;
    }

    public bool IsMenuVisible()
    {
        if (pauseMenuRoot == null)
            return false;

        // If the pause root has "hidden" class, it is NOT visible
        return !pauseMenuRoot.ClassListContains("hidden");
    }

    // --- Arduino helper wrappers (called by UIToolkitNavigator) ---

    public void ResumeFromArduino()
    {
        Resume();
    }

    public void ShowRulesFromArduino()
    {
        ShowRulesPanel(true);
    }

    public void ShowSettingsFromArduino()
    {
        ShowSettingsPanel(true);
    }

    public void ShowMainFromArduino()
    {
        ShowMainPanel();
    }

    public void QuitFromArduino()
    {
        QuitGame();
    }




    private void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
        Debug.Log("Quit Game");
#endif

    }

    private void SetVisible(VisualElement ve, bool show)
    {
        if (ve == null)
            return;

        if (show)
        {
            ve.RemoveFromClassList("hidden");
            ve.style.display = DisplayStyle.Flex;
            Debug.Log("Show element: " + ve.name);
        }
        else
        {
            ve.AddToClassList("hidden");
            ve.style.display = DisplayStyle.None;
            Debug.Log("Hide element: " + ve.name);
        }
    }

    private bool IsVisible(VisualElement ve)
    {
        return ve != null && ve.resolvedStyle.display != DisplayStyle.None;
    }

    private void FocusElement(VisualElement ve)
    {
        if (ve == null) return;
        ve.Focus();
    }

    public bool IsPaused => isPaused;

    
}
