using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class PauseMenu : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject pauseMenuRoot;   // whole pause UI root (stays active)
    [SerializeField] private GameObject mainButtonsPanel; // Resume / Controls / Rules / Quit
    [SerializeField] private GameObject controlsPanel;
    [SerializeField] private GameObject rulesPanel;

    [Header("Default Selection")]
    [SerializeField] private GameObject firstMainButton;     // e.g. Resume


    private bool isPaused;
    private InputAction pauseAction;

    void Awake()
    {
        // root is visible only when paused
        if (pauseMenuRoot) pauseMenuRoot.SetActive(false);
        if (mainButtonsPanel) mainButtonsPanel.SetActive(true);
        if (controlsPanel) controlsPanel.SetActive(false);
        if (rulesPanel) rulesPanel.SetActive(false);

        Time.timeScale = 1f;
        AudioListener.pause = false;
    }



    void OnEnable()
    {
        if (pauseAction == null)
        {
            pauseAction = new InputAction(type: InputActionType.Button);

            // Keyboard
            pauseAction.AddBinding("<Keyboard>/escape");
            pauseAction.AddBinding("<Mouse>/rightButton");

            // Gamepad
            pauseAction.AddBinding("<Gamepad>/start");       // menu/options
            pauseAction.AddBinding("<Gamepad>/buttonEast");  // B button
            pauseAction.AddBinding("<Gamepad>/buttonSouth");  // a button
        }

        pauseAction.performed += OnPausePerformed;
        pauseAction.Enable();
    }

    void OnDisable()
    {
        if (pauseAction != null)
        {
            pauseAction.performed -= OnPausePerformed;
            pauseAction.Disable();
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
        // Not paused yet? → open menu
        if (!isPaused)
        {
            Pause();
            return;
        }

        // Already paused:
        bool inSubpanel =
            (controlsPanel && controlsPanel.activeSelf) ||
            (rulesPanel && rulesPanel.activeSelf);

        if (inSubpanel)
        {
            // B / Esc while viewing Controls/Rules → go back to main pause screen
            BackFromSubpanel();
        }
        else
        {
            // B / Esc / Start on main pause screen → resume game
            Resume();
        }
    }

    public void Pause()
    {
        isPaused = true;

        if (pauseMenuRoot) pauseMenuRoot.SetActive(true);
        if (mainButtonsPanel) mainButtonsPanel.SetActive(true);
        if (controlsPanel) controlsPanel.SetActive(false);
        if (rulesPanel) rulesPanel.SetActive(false);

        Time.timeScale = 0f;
        AudioListener.pause = true;

        SetSelected(firstMainButton);
        
    }

    public void Resume()
    {
        isPaused = false;

        if (pauseMenuRoot) pauseMenuRoot.SetActive(false);
        if (mainButtonsPanel) mainButtonsPanel.SetActive(true);
        if (controlsPanel) controlsPanel.SetActive(false);
        if (rulesPanel) rulesPanel.SetActive(false);

        Time.timeScale = 1f;
        AudioListener.pause = false;

        // Clear selection so gameplay UI / world input can take over
        SetSelected(null);
    }

    public void ShowControls()
    {
        if (mainButtonsPanel) mainButtonsPanel.SetActive(false);
        if (controlsPanel) controlsPanel.SetActive(true);
        if (rulesPanel) rulesPanel.SetActive(false);

    }

    public void ShowRules()
    {
        if (mainButtonsPanel) mainButtonsPanel.SetActive(false);
        if (rulesPanel) rulesPanel.SetActive(true);
        if (controlsPanel) controlsPanel.SetActive(false);

    }

    public void BackFromSubpanel()
    {
        if (controlsPanel) controlsPanel.SetActive(false);
        if (rulesPanel) rulesPanel.SetActive(false);
        if (mainButtonsPanel) mainButtonsPanel.SetActive(true);

        SetSelected(firstMainButton);
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
        Debug.Log("Quit Game");
#endif
    }

    public bool IsPaused => isPaused;

    public void TogglePauseFromExternal()
    {
        if (!isPaused) Pause();
        else           Resume();
    }



    private void SetSelected(GameObject go)
    {
        if (EventSystem.current == null) return;

        EventSystem.current.SetSelectedGameObject(null);
        if (go != null)
            EventSystem.current.SetSelectedGameObject(go);
    }
}
