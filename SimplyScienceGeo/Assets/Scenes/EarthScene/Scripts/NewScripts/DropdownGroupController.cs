using UnityEngine;
using TMPro;
using System.Collections.Generic;
using UnityEngine.UI;

/// <summary>
/// Manages a TMP_Dropdown and updates associated groups, UI text, and globe rotation.
/// Includes an optional feature to set a default toggle for each dropdown option.
/// </summary>
public class DropdownGroupController : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("Assign the TextMeshPro Dropdown from your scene here.")]
    public TMP_Dropdown dropdown;

    [Tooltip("Assign the UIManager from your scene.")]
    public UIManager uiManager;

    [Header("Globe Focus Controller")]
    [Tooltip("Assign the GlobeRotator if you want dropdowns to trigger camera focus.")]
    public GlobeRotator globeRotator;

    [Header("Group Configurations")]
    [Tooltip("Define the groups that correspond to each dropdown option.")]
    public List<DropdownOptionGroup> optionGroups;

    [System.Serializable]
    public class DropdownOptionGroup
    {
        [Tooltip("Descriptive name (for reference only).")]
        public string groupName;

        [Tooltip("Main toggle panel to activate/deactivate.")]
        public GameObject toggleGroupParent;

        [Tooltip("GameObjects to activate when this option is selected.")]
        public List<GameObject> additionalObjectsToActivate;

        [Tooltip("GameObjects to deactivate when this option is selected.")]
        public List<GameObject> objectsToDeactivate;

        // --- CHANGE: Added a field for the optional default toggle ---
        [Header("Default Selection")]
        [Tooltip("The toggle to be automatically selected when this group is activated. Leave null for none.")]
        public Toggle defaultToggle;

        [Header("UI Content")]
        [Tooltip("Title text for the UI panel.")]
        public string titleText;

        [Tooltip("Body content for the UI panel.")]
        [TextArea(3, 10)]
        public string bodyText;

        [Header("Camera Focus (Optional)")]
        [Tooltip("Target rotation to focus the globe on.")]
        public Vector3 targetRotation;

        [Tooltip("Camera distance from globe center.")]
        public float targetZoom = 15f;
    }

    void Start()
    {
        if (dropdown == null)
        {
            Debug.LogError("DropdownGroupController: Dropdown reference is missing!", this);
            return;
        }

        if (uiManager == null)
        {
            Debug.LogWarning("DropdownGroupController: UIManager not assigned.");
        }

        dropdown.onValueChanged.AddListener(HandleDropdownValueChanged);
        HandleDropdownValueChanged(dropdown.value); // Set initial state
    }

    void OnDestroy()
    {
        if (dropdown != null)
        {
            dropdown.onValueChanged.RemoveListener(HandleDropdownValueChanged);
        }
    }

    private void HandleDropdownValueChanged(int selectedIndex)
    {
        if (globeRotator != null)
        {
            globeRotator.enabled = true;
        }

        for (int i = 0; i < optionGroups.Count; i++)
        {
            bool isSelected = (i == selectedIndex);
            var group = optionGroups[i];

            if (group.toggleGroupParent != null)
            {
                group.toggleGroupParent.SetActive(isSelected);

                if (!isSelected)
                {
                    SelectableToggleButton[] togglesToReset = group.toggleGroupParent.GetComponentsInChildren<SelectableToggleButton>(true);
                    foreach (var toggleButton in togglesToReset)
                    {
                        toggleButton.ForceResetHighlight();
                    }
                }
            }
            if (isSelected)
            {
                // --- CHANGE: Replaced the old toggle clearing method with the new one ---
                SetDefaultToggleState(group.toggleGroupParent, group.defaultToggle);

                StartCoroutine(DelayedFallbackDisplay(group));
            }

            if (group.additionalObjectsToActivate != null)
            {
                foreach (var obj in group.additionalObjectsToActivate)
                {
                    if (obj != null) obj.SetActive(isSelected);
                }
            }

            if (group.objectsToDeactivate != null)
            {
                foreach (var obj in group.objectsToDeactivate)
                {
                    if (obj != null) obj.SetActive(!isSelected);
                }
            }
        }

        if (uiManager != null && selectedIndex >= 0 && selectedIndex < optionGroups.Count)
        {
            var selectedGroup = optionGroups[selectedIndex];
            uiManager.DisplayInformation(selectedGroup.bodyText, selectedGroup.titleText);
        }

        if (globeRotator != null && selectedIndex >= 0 && selectedIndex < optionGroups.Count)
        {
            var selectedGroup = optionGroups[selectedIndex];
            globeRotator.MoveToTarget(
                selectedGroup.targetRotation,
                selectedGroup.targetZoom,
                duration: 1.2f,
                ease: LeanTweenType.easeInOutSine
            );
        }
    }

    // --- CHANGE: These two new methods handle setting the default toggle ---
    private void SetDefaultToggleState(GameObject groupParent, Toggle defaultToggle)
    {
        if (groupParent == null) return;
        StartCoroutine(DelayedSetDefaultToggle(groupParent, defaultToggle));
    }

    private System.Collections.IEnumerator DelayedSetDefaultToggle(GameObject groupParent, Toggle defaultToggle)
    {
        yield return null; // Wait 1 frame to ensure all objects are active.

        // Scenario 1: A default toggle IS assigned.
        if (defaultToggle != null)
        {
            // Activating one toggle in a group automatically deactivates the others.
            // This is the cleanest way to set the default.
            defaultToggle.isOn = true;
        }
        // Scenario 2: NO default toggle is assigned.
        else
        {
            var toggleGroup = groupParent.GetComponentInChildren<ToggleGroup>();
            if (toggleGroup != null)
            {
                // Use the official method to clear the selection.
                // This correctly respects the "Allow Switch Off" setting in the ToggleGroup's inspector.
                toggleGroup.SetAllTogglesOff();
            }
            else
            {
                // Fallback for toggles not in a group: turn them all off manually.
                Toggle[] toggles = groupParent.GetComponentsInChildren<Toggle>(true);
                foreach (var toggle in toggles)
                {
                    toggle.isOn = false;
                }
            }
        }
    }

    private System.Collections.IEnumerator DelayedFallbackDisplay(DropdownOptionGroup group)
    {
        yield return null; // wait 1 frame for toggle reset

        bool anyToggleOn = false;
        if (group.toggleGroupParent != null)
        {
            Toggle[] toggles = group.toggleGroupParent.GetComponentsInChildren<Toggle>(true);
            foreach (var toggle in toggles)
            {
                if (toggle.isOn)
                {
                    anyToggleOn = true;
                    break;
                }
            }
        }

        if (!anyToggleOn)
        {
            uiManager?.DisplayInformation(group.bodyText, group.titleText);
            globeRotator?.MoveToTarget(group.targetRotation, group.targetZoom, 1.2f, LeanTweenType.easeInOutSine);
        }
    }
}