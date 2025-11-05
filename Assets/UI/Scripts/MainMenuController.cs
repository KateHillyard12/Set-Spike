using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class MainMenuController : MonoBehaviour
{
    private Button playButton;
    private Button rulesButton;
    private Button settingsButton;
    private Button quitButton;

    private Button closeRulesButton;
    private Button closeSettingsButton;

    private VisualElement rulesPanel;
    private VisualElement settingsPanel;

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

        playButton.clicked += OnPlayClicked;
        rulesButton.clicked += ShowRulesPanel;
        settingsButton.clicked += ShowSettingsPanel;
        quitButton.clicked += OnQuitClicked;

        closeRulesButton.clicked += HideRulesPanel;
        closeSettingsButton.clicked += HideSettingsPanel;
    }

    private void OnPlayClicked()
    {
        Debug.Log("Play pressed");
        SceneManager.LoadScene("GameScene");
    }

    private void ShowRulesPanel()
    {
        rulesPanel.RemoveFromClassList("hidden");
    }

    private void HideRulesPanel()
    {
        rulesPanel.AddToClassList("hidden");
    }

    private void ShowSettingsPanel()
    {
        settingsPanel.RemoveFromClassList("hidden");
    }

    private void HideSettingsPanel()
    {
        settingsPanel.AddToClassList("hidden");
    }

    private void OnQuitClicked()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
