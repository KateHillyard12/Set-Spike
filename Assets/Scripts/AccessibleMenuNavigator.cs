using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class AccessibleMenuNavigator : MonoBehaviour
{
    [Tooltip("Buttons (or other Selectables) in the order you want to cycle through.")]
    public Selectable[] items;

    [Tooltip("Where to start selection (index into items).")]
    public int startIndex = 0;

    int currentIndex = 0;

    void Awake()
    {
        if (items != null && items.Length > 0)
        {
            currentIndex = Mathf.Clamp(startIndex, 0, items.Length - 1);
            SelectCurrent();
        }
    }

    void OnEnable()
    {
        if (items != null && items.Length > 0)
            SelectCurrent();
    }

    void SelectCurrent()
    {
        if (EventSystem.current == null) return;
        if (items == null || items.Length == 0) return;

        var sel = items[currentIndex];
        if (sel != null)
            EventSystem.current.SetSelectedGameObject(sel.gameObject);
    }

    public void ExternalNavigate(int dir)
    {
        if (items == null || items.Length == 0) return;
        if (dir == 0) return;

        currentIndex += (dir > 0 ? 1 : -1);

        // wrap around
        if (currentIndex < 0) currentIndex = items.Length - 1;
        if (currentIndex >= items.Length) currentIndex = 0;

        SelectCurrent();
    }

    public void ExternalSubmit()
    {
        if (items == null || items.Length == 0) return;

        var sel = items[currentIndex];
        if (!sel) return;

        // Try Button first
        var btn = sel.GetComponent<Button>();
        if (btn != null)
        {
            btn.onClick.Invoke();
            return;
        }

        // Otherwise, send a submit event to the selectable
        var data = new BaseEventData(EventSystem.current);
        ExecuteEvents.Execute(sel.gameObject, data, ExecuteEvents.submitHandler);
    }
}
