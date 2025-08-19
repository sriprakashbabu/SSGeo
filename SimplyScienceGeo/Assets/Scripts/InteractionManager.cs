using UnityEngine;

/// <summary>
/// The central coordinator (the "Brain"). It manages the application's selection state
/// and tells the other managers (UI, Visuals, and ModelActivators) what to do.
/// </summary>
public class InteractionManager : MonoBehaviour
{
    [Header("Manager References")]
    [Tooltip("Assign the VisualFeedbackController from the scene.")]
    public VisualFeedbackController visualController;

    [Tooltip("Assign the UIManager from the scene.")]
    public UIManager uiManager;

    private InteractableFeature _currentlySelectedFeature;
    public InteractableFeature CurrentlySelectedFeature => _currentlySelectedFeature;

    /// <summary>
    /// Selects a new feature, updating the state and instructing other managers.
    /// </summary>
    public void SelectFeature(InteractableFeature newFeature)
    {
        // If we click the same feature again, do nothing.
        if (_currentlySelectedFeature == newFeature) return;

        // First, deselect the old feature. This will also trigger the deactivation
        // of any active model view before proceeding.
        ClearCurrentSelection();

        // Set the new feature as the current selection.
        _currentlySelectedFeature = newFeature;

        if (_currentlySelectedFeature != null)
        {
            // --- Standard Behavior ---
            visualController?.StartHighlight(_currentlySelectedFeature);
            uiManager?.DisplayInformation(
                _currentlySelectedFeature.informationText,
                _currentlySelectedFeature.featureName);

            // --- THIS IS THE CRUCIAL LINE ---
            // Check if this feature has a ModelActivator and, if so, tell it to activate.
            _currentlySelectedFeature.GetComponent<ModelActivator>()?.Activate();
        }
    }
    public void ResetToDefaultUI()
    {
        // First, properly clear and clean up any currently selected feature.
        // This will correctly call StopHighlight and handle other cleanup.
        ClearCurrentSelection();

        // After clearing the selection, then display the default text.
        uiManager?.DisplayDefaultText(uiManager.defaultTitle, uiManager.defaultBody);
    }
    /// <summary>
    /// Clears the current selection and resets all visuals and active models.
    /// </summary>
    public void ClearCurrentSelection()
    {
        if (_currentlySelectedFeature == null) return;

        // --- Deactivate the model view first ---
        // Find the ModelActivator on the object we are clearing and tell it to deactivate.
        // The ModelActivator script will handle the timing and state changes.
        _currentlySelectedFeature.GetComponent<ModelActivator>()?.Deactivate();

        // Stop the highlight effect.
        visualController?.StopHighlight(_currentlySelectedFeature);

        // This line is commented out to allow GlobeGridController to show default text.
        // If you want the panel to hide completely, you can uncomment it.
        // uiManager?.HideInformation();

        // Finally, forget the selection.
        _currentlySelectedFeature = null;
    }
}