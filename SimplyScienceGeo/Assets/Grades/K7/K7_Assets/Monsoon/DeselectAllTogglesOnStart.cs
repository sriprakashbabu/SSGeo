using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class DeselectAllTogglesOnStart : MonoBehaviour
{
    [SerializeField] List<Toggle> toggles = new List<Toggle>();
    [SerializeField] ToggleGroup toggleGroup; // optional

    void Awake()
    {
        // 1) Allow none selected if using ToggleGroup
        if (toggleGroup != null) toggleGroup.allowSwitchOff = true;

        // 2) Turn every toggle OFF without sending events (prevents highlight/callbacks)
        foreach (var t in toggles)
            if (t != null) t.SetIsOnWithoutNotify(false);

        // 3) Clear any UI “selected” state so nothing looks highlighted
        EventSystem.current?.SetSelectedGameObject(null);
    }

    // Optional: call this anytime you want to force "none selected"
    public void ClearSelection()
    {
        if (toggleGroup != null) toggleGroup.allowSwitchOff = true;
        foreach (var t in toggles)
            if (t != null) t.SetIsOnWithoutNotify(false);
        EventSystem.current?.SetSelectedGameObject(null);
    }
}
