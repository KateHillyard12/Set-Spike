using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private GameObject controlsPanel;
    [SerializeField] private GameObject rulesPanel;
    [SerializeField] private string gameSceneName = "Game"; // set your game scene name

    void Awake()
    {
        Time.timeScale = 1f;
        AudioListener.pause = false;
        if (controlsPanel) controlsPanel.SetActive(false);
        if (rulesPanel) rulesPanel.SetActive(false);
    }

    public void PlayGame() => SceneManager.LoadScene(gameSceneName);

    public void ShowControls() { if (controlsPanel) controlsPanel.SetActive(true); if (rulesPanel) rulesPanel.SetActive(false); }
    public void ShowRules()    { if (rulesPanel) rulesPanel.SetActive(true);     if (controlsPanel) controlsPanel.SetActive(false); }
    public void BackToMenu()   { if (controlsPanel) controlsPanel.SetActive(false); if (rulesPanel) rulesPanel.SetActive(false); }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
