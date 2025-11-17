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
        // Make sure group allows zero selected
        if (toggleGroup != null)
            toggleGroup.allowSwitchOff = true;
    }

    void OnEnable()
    {
        // Every time this object is enabled, clear selections
        ClearSelection();
    }

    public void ClearSelection()
    {
        // Ensure toggle group permits switching everything off
        if (toggleGroup != null)
            toggleGroup.allowSwitchOff = true;

        // TRUE deselection – triggers OnValueChanged(false)
        foreach (var t in toggles)
        {
            if (t != null && t.isOn)
                t.isOn = false; // triggers deselect event
        }

        // Remove UI highlight from last selected toggle
        EventSystem.current?.SetSelectedGameObject(null);
    }
}
