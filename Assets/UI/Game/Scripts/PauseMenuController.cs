using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

/// <summary>
/// Handles the in-game pause menu using UI Toolkit.
/// Mirrors the Start menu flow: main button list plus Rules and Settings subpanels.
/// Toggles via Esc / Start / B (controller).
/// </summary>
public class PauseMenuController : MonoBehaviour
{
    [Header("UI Document")]
    [SerializeField] private UIDocument uiDocument;

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

    void Awake()
    {
        if (uiDocument == null)
            uiDocument = GetComponent<UIDocument>();

        // Ensure normal time/audio on load
        Time.timeScale = 1f;
        AudioListener.pause = false;
    }

    void OnEnable()
    {
        root = uiDocument != null ? uiDocument.rootVisualElement : null;
        if (root == null)
        {
            Debug.LogWarning("PauseMenuController: UIDocument root not found.");
            return;
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
            AudioListener.pause = false;
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
        AudioListener.pause = true;

        ShowMainPanel();
    }

    public void Resume()
    {
        if (!isPaused)
            return;

        isPaused = false;
        Time.timeScale = 1f;
        AudioListener.pause = false;

        HideAllPanels();
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
        SetVisible(mainPanel, true);
        SetVisible(rulesPanel, false);
        SetVisible(settingsPanel, false);

        FocusElement(resumeButton);
    }

    private void ShowRulesPanel()
    {
        if (pauseMenuRoot == null)
            return;

        SetVisible(pauseMenuRoot, true);
        SetVisible(mainPanel, false);
        SetVisible(rulesPanel, true);
        SetVisible(settingsPanel, false);

        FocusElement(backFromRulesButton);
    }

    private void ShowSettingsPanel()
    {
        if (pauseMenuRoot == null)
            return;

        SetVisible(pauseMenuRoot, true);
        SetVisible(mainPanel, false);
        SetVisible(rulesPanel, false);
        SetVisible(settingsPanel, true);

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
        if (resumeButton != null) resumeButton.clicked += Resume;
        else Debug.LogWarning("PauseMenuController: 'resume-button' not found.");

        if (settingsButton != null) settingsButton.clicked += ShowSettingsPanel;
        else Debug.LogWarning("PauseMenuController: 'pause-settings-button' not found.");

        if (rulesButton != null) rulesButton.clicked += ShowRulesPanel;
        else Debug.LogWarning("PauseMenuController: 'pause-rules-button' not found.");

        if (quitButton != null) quitButton.clicked += QuitGame;
        else Debug.LogWarning("PauseMenuController: 'pause-quit-button' not found.");

        if (backFromRulesButton != null) backFromRulesButton.clicked += ShowMainPanel;
        else Debug.LogWarning("PauseMenuController: 'pause-back-from-rules' not found.");

        if (backFromSettingsButton != null) backFromSettingsButton.clicked += ShowMainPanel;
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

    private void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
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
        }
        else
        {
            ve.AddToClassList("hidden");
            ve.style.display = DisplayStyle.None;
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
