using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class ToggleGroupActivator : MonoBehaviour
{
    [System.Serializable]
    public class ToggleSet
    {
        public Toggle toggle;                // The UI toggle to listen to
        public List<GameObject> objectsToEnable;  // Objects to activate when this toggle is selected
    }

    [Header("Toggle Groups")]
    public List<ToggleSet> toggleSets;       // Add toggle + objects in Inspector

    void Start()
    {
        // Ensure all toggles start off and all objects are deactivated
        foreach (var set in toggleSets)
        {
            if (set.toggle != null)
            {
                set.toggle.isOn = false;
                set.toggle.onValueChanged.AddListener((isOn) => HandleToggleChanged(set, isOn));
            }

            foreach (var obj in set.objectsToEnable)
                if (obj != null) obj.SetActive(false);
        }
    }

    private void HandleToggleChanged(ToggleSet changedSet, bool isOn)
    {
        if (isOn)
        {
            // Disable all other groups first
            foreach (var set in toggleSets)
            {
                foreach (var obj in set.objectsToEnable)
                    if (obj != null) obj.SetActive(false);
            }

            // Enable the objects for the selected toggle
            foreach (var obj in changedSet.objectsToEnable)
                if (obj != null) obj.SetActive(true);
        }
        else
        {
            // If a toggle is turned off manually, also deactivate its objects
            foreach (var obj in changedSet.objectsToEnable)
                if (obj != null) obj.SetActive(false);
        }
    }
}
