using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;

/// <summary>
/// Diagnostic tool to check if UI input system is properly configured.
/// Attach this to any GameObject in the Game scene and run in Play mode.
/// Check the Console output to see what's happening with UI input.
/// </summary>
public class InputDiagnostics : MonoBehaviour
{
    private int frameCount = 0;

    void Update()
    {
        frameCount++;

        // Print diagnostics every 60 frames (roughly 1 second at 60 FPS)
        if (frameCount % 60 == 0)
        {
            PrintDiagnostics();
        }
    }

    private void PrintDiagnostics()
    {
        Debug.Log("=== INPUT DIAGNOSTICS ===");

        // Check EventSystem
        EventSystem eventSystem = EventSystem.current;
        Debug.Log($"EventSystem present: {eventSystem != null}");
        if (eventSystem != null)
        {
            Debug.Log($"  EventSystem name: {eventSystem.gameObject.name}");
            Debug.Log($"  EventSystem active: {eventSystem.gameObject.activeInHierarchy}");

            // Check InputModule
            BaseInputModule inputModule = eventSystem.GetComponent<BaseInputModule>();
            Debug.Log($"  InputModule type: {(inputModule != null ? inputModule.GetType().Name : "MISSING")}");

            if (inputModule != null)
            {
                Debug.Log($"  InputModule active: {inputModule.isActiveAndEnabled}");
            }
        }

        // Check UIDocument
        UIDocument uiDoc = FindObjectOfType<UIDocument>();
        Debug.Log($"UIDocument present: {uiDoc != null}");
        if (uiDoc != null)
        {
            Debug.Log($"  UIDocument name: {uiDoc.gameObject.name}");
            Debug.Log($"  UIDocument active: {uiDoc.isActiveAndEnabled}");

            var root = uiDoc.rootVisualElement;
            if (root != null)
            {
                var pauseMenu = root.Q<VisualElement>("PauseMenu");
                Debug.Log($"  PauseMenu element found: {pauseMenu != null}");
                
                if (pauseMenu != null)
                {
                    Debug.Log($"    PauseMenu display: {pauseMenu.style.display}");
                    Debug.Log($"    PauseMenu pickingMode: {pauseMenu.pickingMode}");

                    var resumeButton = root.Q<Button>("resume-button");
                    Debug.Log($"    Resume button found: {resumeButton != null}");
                    if (resumeButton != null)
                    {
                        Debug.Log($"      Button pickingMode: {resumeButton.pickingMode}");
                        Debug.Log($"      Button display: {resumeButton.style.display}");
                        Debug.Log($"      Button enabled: {resumeButton.enabledInHierarchy}");
                    }
                }
            }
        }

        // Check mouse over UI
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject(-1))
        {
            Debug.Log("🖱️ Mouse IS over UI element");
        }
        else if (EventSystem.current != null)
        {
            Debug.Log("❌ Mouse is NOT over any UI element");
        }

        Debug.Log("========================");
    }
}
