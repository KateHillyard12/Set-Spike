using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using UnityEngine.EventSystems;


public class StartMenuController : MonoBehaviour
{
    private Button playButton;
    private Button rulesButton;
    private Button settingsButton;
    private Button quitButton;

    private Button closeRulesButton;
    private Button closeSettingsButton;

    private VisualElement rulesPanel;
    private VisualElement settingsPanel;
    private VisualElement topPanel;
    private VisualElement mainSection;

    void OnEnable()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;

        playButton = root.Q<Button>("play-button");
        rulesButton = root.Q<Button>("rules-button");
        settingsButton = root.Q<Button>("settings-button");
        quitButton = root.Q<Button>("quit-button");

        rulesPanel = root.Q<VisualElement>("rules-panel");
        settingsPanel = root.Q<VisualElement>("settings-panel");

        closeRulesButton = root.Q<Button>("close-rules");
        closeSettingsButton = root.Q<Button>("close-settings");

        topPanel = root.Q<VisualElement>("TopPannel");
        mainSection = root.Q<VisualElement>("Main");

        // Subscribe and log missing elements to help debugging
        if (playButton != null) playButton.clicked += OnPlayClicked; else Debug.LogWarning("MainMenuController: 'play-button' not found in UIDocument.");
        if (rulesButton != null) rulesButton.clicked += ShowRulesPanel; else Debug.LogWarning("MainMenuController: 'rules-button' not found in UIDocument.");
        if (settingsButton != null) settingsButton.clicked += ShowSettingsPanel; else Debug.LogWarning("MainMenuController: 'settings-button' not found in UIDocument.");
        if (quitButton != null) quitButton.clicked += OnQuitClicked; else Debug.LogWarning("MainMenuController: 'quit-button' not found in UIDocument.");

        if (closeRulesButton != null) closeRulesButton.clicked += HideRulesPanel; else Debug.LogWarning("MainMenuController: 'close-rules' not found in UIDocument.");
        if (closeSettingsButton != null) closeSettingsButton.clicked += HideSettingsPanel; else Debug.LogWarning("MainMenuController: 'close-settings' not found in UIDocument.");
    }

    void OnDisable()
    {
        // Unsubscribe to avoid duplicate handlers when component is re-enabled
        if (playButton != null) playButton.clicked -= OnPlayClicked; 

        if (rulesButton != null) rulesButton.clicked -= ShowRulesPanel;
        if (settingsButton != null) settingsButton.clicked -= ShowSettingsPanel;
        if (quitButton != null) quitButton.clicked -= OnQuitClicked;

        if (closeRulesButton != null) closeRulesButton.clicked -= HideRulesPanel;
        if (closeSettingsButton != null) closeSettingsButton.clicked -= HideSettingsPanel;
    }

    private void OnPlayClicked()
    {
        Debug.Log("Play pressed");
        SceneManager.LoadScene("Main");
    }

    private void ShowRulesPanel()
    {
        if (rulesPanel != null)
        {
            // show rules hide start sections
            rulesPanel.style.display = DisplayStyle.Flex;
            if (topPanel != null) topPanel.style.display = DisplayStyle.None;
            if (mainSection != null) mainSection.style.display = DisplayStyle.None;
            Debug.Log("Rules pressed");
        }
        else Debug.LogWarning("MainMenuController: Attempted to show rules panel but 'rules-panel' VisualElement is missing.");
    }

    private void HideRulesPanel()
    {
        if (rulesPanel != null)
        {
            rulesPanel.style.display = DisplayStyle.None;
            if (topPanel != null) topPanel.style.display = DisplayStyle.Flex;
            if (mainSection != null) mainSection.style.display = DisplayStyle.Flex;
        }
        else Debug.LogWarning("MainMenuController: Attempted to hide rules panel but 'rules-panel' VisualElement is missing.");
    }

    private void ShowSettingsPanel()
    {
        if (settingsPanel != null)
        {
            settingsPanel.style.display = DisplayStyle.Flex;
            if (topPanel != null) topPanel.style.display = DisplayStyle.None;
            if (mainSection != null) mainSection.style.display = DisplayStyle.None;
        }
        else Debug.LogWarning("MainMenuController: Attempted to show settings panel but 'settings-panel' VisualElement is missing.");
    }

    private void HideSettingsPanel()
    {
        if (settingsPanel != null)
        {
            settingsPanel.style.display = DisplayStyle.None;
            if (topPanel != null) topPanel.style.display = DisplayStyle.Flex;
            if (mainSection != null) mainSection.style.display = DisplayStyle.Flex;
        }
        else Debug.LogWarning("MainMenuController: Attempted to hide settings panel but 'settings-panel' VisualElement is missing.");
    }

    private void OnQuitClicked()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
