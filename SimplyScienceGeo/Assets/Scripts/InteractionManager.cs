using UnityEngine;

/// <summary>
/// Central coordinator for selection. Ensures only one feature is active at a time,
/// drives visual highlight + UI, and toggles per-object extras via FeatureExtras.
/// </summary>
public class InteractionManager : MonoBehaviour
{
    [Header("Manager References")]
    [Tooltip("Assign the VisualFeedbackController from the scene.")]
    public VisualFeedbackController visualController;

    [Tooltip("Assign the UIManager from the scene.")]
    public UIManager uiManager;

    // --- Selection state ---
    private InteractableFeature _currentlySelectedFeature;
    public InteractableFeature CurrentlySelectedFeature => _currentlySelectedFeature;

    // Cache the extras of the currently selected feature so we can disable quickly on clear
    private FeatureExtras _currentExtras;

    /// <summary>
    /// Selects a new feature. Clears the previous one first so only one remains active.
    /// </summary>
    public void SelectFeature(InteractableFeature newFeature)
    {
        // Clicking the same feature again? Do nothing.
        if (_currentlySelectedFeature == newFeature) return;

        // Turn off highlight, UI bindings (if desired), and extras for the previous selection.
        ClearCurrentSelection();

        // Set the new feature as the current selection.
        _currentlySelectedFeature = newFeature;

        if (_currentlySelectedFeature != null)
        {
            // Start highlight visuals for this feature.
            visualController?.StartHighlight(_currentlySelectedFeature);

            // Update the info panel with this feature's data.
            uiManager?.DisplayInformation(
                _currentlySelectedFeature.informationText,
                _currentlySelectedFeature.featureName
            );

            // Enable extras only for this selected feature.
            _currentExtras = _currentlySelectedFeature.GetComponent<FeatureExtras>();
            _currentExtras?.EnableExtras();
        }
    }

    /// <summary>
    /// Clears the current selection and resets visuals and extras.
    /// </summary>
    public void ClearCurrentSelection()
    {
        if (_currentlySelectedFeature == null) return;

        // Disable extras for the object we are clearing.
        if (_currentExtras != null)
        {
            _currentExtras.DisableExtras();
            _currentExtras = null;
        }

        // Stop highlight effect for the previously selected feature.
        visualController?.StopHighlight(_currentlySelectedFeature);

        // Optionally hide or reset UI. Your current flow keeps panel visible with default text.
        // If you prefer to hide it completely, uncomment the next line and adjust your UIManager.
        // uiManager?.HideInformation();

        // Forget the selection.
        _currentlySelectedFeature = null;
    }

    /// <summary>
    /// Clears selection and shows default UI text.
    /// </summary>
    public void ResetToDefaultUI()
    {
        // Properly clear and clean up any currently selected feature.
        ClearCurrentSelection();

        // Then show your default title/body on the panel.
        if (uiManager != null)
        {
            uiManager.DisplayDefaultText(uiManager.defaultTitle, uiManager.defaultBody);
        }
    }
}
