using UnityEngine;
using UnityEngine.InputSystem; // For the new Input System (Mouse.current)

public class GlobalClickHandler : MonoBehaviour
{
    private static ClickableInfo _currentlySelectedFeature;
    private static GlobalClickHandler _instance; // Simple singleton pattern instance

    void Awake()
    {
        // Ensure only one instance of GlobalClickHandler exists
        if (_instance != null && _instance != this)
        {
            Debug.LogWarning("GlobalClickHandler: Multiple instances found, destroying this one.", gameObject);
            Destroy(gameObject);
        }
        else
        {
            _instance = this;
            // Optional: DontDestroyOnLoad(gameObject); if this manager needs to persist across scene loads
        }
    }

    void Update()
    {
        // Check for mouse presence, left button press, and if a main camera exists
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame && Camera.main != null)
        {
            Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                // Debug.Log($"GlobalClickHandler: Raycast HIT object: '{hit.collider.gameObject.name}'"); // Optional: for detailed debugging

                ClickableInfo clickedFeature = hit.collider.gameObject.GetComponent<ClickableInfo>();

                // Check if the hit object has ClickableInfo and if it can be clicked
                // The CanBeClicked() method in ClickableInfo should check if its own collider and script are enabled.
                if (clickedFeature != null && clickedFeature.CanBeClicked())
                {
                    HandleFeatureSelection(clickedFeature);
                }
                // else
                // {
                // Optional: Log if the hit object doesn't have an active ClickableInfo component
                // Debug.Log($"GlobalClickHandler: Hit '{hit.collider.gameObject.name}', but no active ClickableInfo component found or it cannot be clicked.");
                // }
            }
            // else
            // {
            //    Debug.Log("GlobalClickHandler: Raycast did not hit any colliders."); // Optional: for debugging
            // }
        }
    }

    /// <summary>
    /// Manages the selection and deselection of clickable features.
    /// </summary>
    /// <param name="newFeature">The ClickableInfo component of the newly clicked feature.</param>
    private void HandleFeatureSelection(ClickableInfo newFeature)
    {
        if (newFeature == null) return; // Should have been caught by CanBeClicked, but as a safeguard

        // If a different feature was already selected and pulsing, deselect and stop its pulse.
        if (_currentlySelectedFeature != null && _currentlySelectedFeature != newFeature)
        {
            _currentlySelectedFeature.DeselectAndReset();
        }

        // If we clicked a new feature, or re-clicked the same one.
        // SelectAndPulse in ClickableInfo handles displaying info AND starting/managing the pulse.
        if (_currentlySelectedFeature != newFeature)
        {
            _currentlySelectedFeature = newFeature;
            _currentlySelectedFeature.SelectAndPulse();
        }
        else // Clicked the same feature again
        {
            // Re-selecting the current feature will call SelectAndPulse again.
            // The SelectAndPulse method in ClickableInfo is designed to display info
            // and ensure the pulse is running (it should ideally not start a duplicate coroutine if already pulsing).
            _currentlySelectedFeature.SelectAndPulse();
        }
    }

    /// <summary>
    /// Static method that can be called from anywhere (e.g., a global reset button)
    /// to stop the current feature from pulsing and deselect it.
    /// </summary>
    public static void ResetCurrentSelection()
    {
        if (_instance == null)
        {
            // This can happen if ResetCurrentSelection is called after GlobalClickHandler has been destroyed
            // or before it has Awoken.
            Debug.LogWarning("GlobalClickHandler: Instance not found. Cannot reset current selection.");
            return;
        }

        if (_currentlySelectedFeature != null)
        {
            Debug.Log($"GlobalClickHandler: ResetCurrentSelection called. Resetting pulse for '{_currentlySelectedFeature.gameObject.name}'");
            _currentlySelectedFeature.DeselectAndReset();
            _currentlySelectedFeature = null; // Clear the selection
        }
        else
        {
            Debug.Log("GlobalClickHandler: ResetCurrentSelection called, but no feature was currently selected.");
        }
    }

    void OnDestroy()
    {
        // Clear the static instance if this object is destroyed
        if (_instance == this)
        {
            _instance = null;

            // Ensure any active pulse is stopped if this manager is destroyed,
            // though the ClickableInfo's OnDestroy should also handle its own coroutine.
            // This is an extra safeguard.
            if (_currentlySelectedFeature != null)
            {
                _currentlySelectedFeature.DeselectAndReset();
                _currentlySelectedFeature = null;
            }
        }
    }
}
