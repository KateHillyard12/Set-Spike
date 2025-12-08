using UnityEngine;
using UnityEngine.UIElements;

public class UIToolkitNavigator : MonoBehaviour
{
    private VisualElement root;
    private Button[] buttons;
    private int index = 0;

    void OnEnable()
    {
        var uiDoc = GetComponent<UIDocument>();
        if (uiDoc == null)
        {
            Debug.LogWarning("UIToolkitNavigator: UIDocument not found.");
            return;
        }

        root = uiDoc.rootVisualElement;
        if (root == null)
        {
            Debug.LogWarning("UIToolkitNavigator: rootVisualElement is null.");
            return;
        }

        // Grab all buttons in this UI (your pause menu UI)
        buttons = root.Query<Button>().ToList().ToArray();

        if (buttons.Length == 0)
        {
            Debug.LogWarning("UIToolkitNavigator: No buttons found in UIDocument.");
            return;
        }

        index = 0;
        HighlightButton(index);
    }

    public void NavigateDown()
    {
        if (buttons == null || buttons.Length == 0)
            return;

        index = (index + 1) % buttons.Length;
        HighlightButton(index);
    }

    public void Submit()
    {
        if (buttons == null || buttons.Length == 0)
        {
            Debug.Log("UIToolkitNavigator: No buttons found to submit");
            return;
        }

        var button = buttons[index];
        Debug.Log("UIToolkitNavigator: Submitting " + button.name);

        // Instead of faking click events, directly call PauseMenuController
        var pause = FindAnyObjectByType<PauseMenuController>();
        if (pause == null)
        {
            Debug.LogWarning("UIToolkitNavigator: PauseMenuController not found.");
            return;
        }

        // Route by button name (matches your UXML names)
        switch (button.name)
        {
            case "resume-button":
                pause.ResumeFromArduino();
                break;

            case "pause-rules-button":
                pause.ShowRulesFromArduino();
                break;

            case "pause-settings-button":
                pause.ShowSettingsFromArduino();
                break;

            case "pause-quit-button":
                pause.QuitFromArduino();
                break;

            case "pause-back-from-rules":
            case "pause-back-from-settings":
                pause.ShowMainFromArduino();
                break;

            default:
                Debug.LogWarning("UIToolkitNavigator: No Arduino handler for button " + button.name);
                break;
        }
    }

    private void HighlightButton(int i)
    {
        if (buttons == null || buttons.Length == 0)
            return;

        for (int k = 0; k < buttons.Length; k++)
            buttons[k].RemoveFromClassList("highlighted");

        buttons[i].AddToClassList("highlighted");
        buttons[i].Focus();
    }
}
