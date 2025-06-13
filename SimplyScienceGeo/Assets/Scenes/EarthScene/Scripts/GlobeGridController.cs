using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// A simplified controller that completely enables or disables the line group GameObjects.
/// It works with UI Toggles to show/hide entire sets of lines.
/// </summary>
public class GlobeGridController : MonoBehaviour
{
    [Header("Manager References")]
    [Tooltip("Assign the InteractionManager from your persistent scene.")]
    public InteractionManager interactionManager;

    [Header("Line Group Parents")]
    [Tooltip("The parent GameObject holding all the latitude line objects.")]
    public GameObject latitudeLinesParent;
    [Tooltip("The parent GameObject holding all the longitude line objects.")]
    public GameObject longitudeLinesParent;

    [Header("UI Toggles")]
    [Tooltip("The UI Toggle for showing/hiding the latitude lines.")]
    public Toggle latitudeToggle;
    [Tooltip("The UI Toggle for showing/hiding the longitude lines.")]
    public Toggle longitudeToggle;

    [Header("Default Mode Text")]
    [Tooltip("The default info to show when the Latitude view is active and nothing is selected.")]
    [TextArea(2, 5)]
    public string latitudeDefaultInfo = "Latitude lines measure distance north or south of the Earth's equator.";
    [Tooltip("The default info to show when the Longitude view is active and nothing is selected.")]
    [TextArea(2, 5)]
    public string longitudeDefaultInfo = "Longitude lines run from the North Pole to the South Pole and measure distance east or west.";

    void Start()
    {
        // --- Setup and Validation ---
        if (interactionManager == null)
            interactionManager = FindObjectOfType<InteractionManager>();

        if (latitudeLinesParent == null || longitudeLinesParent == null || latitudeToggle == null || longitudeToggle == null || interactionManager == null)
        {
            Debug.LogError("GlobeGridController: A required component or manager has not been assigned!", this);
            enabled = false;
            return;
        }

        // --- Add listeners to the toggles ---
        latitudeToggle.onValueChanged.AddListener(SetLatitudeGroupActive);
        longitudeToggle.onValueChanged.AddListener(SetLongitudeGroupActive);

        // --- Set initial state based on which toggle is on by default ---
        SetLatitudeGroupActive(latitudeToggle.isOn);
        SetLongitudeGroupActive(longitudeToggle.isOn);
    }

    /// <summary>
    /// Called when the latitude toggle's value changes.
    /// </summary>
    public void SetLatitudeGroupActive(bool isActive)
    {
        if (latitudeLinesParent != null)
        {
            latitudeLinesParent.SetActive(isActive);
        }

        // If this view was just activated, reset the selection and show the default text.
        if (isActive)
        {
            interactionManager.ClearCurrentSelection();
            interactionManager.uiManager.DisplayDefaultText("Latitude View", latitudeDefaultInfo);
        }
    }

    /// <summary>
    /// Called when the longitude toggle's value changes.
    /// </summary>
    public void SetLongitudeGroupActive(bool isActive)
    {
        if (longitudeLinesParent != null)
        {
            longitudeLinesParent.SetActive(isActive);
        }

        // If this view was just activated, reset the selection and show the default text.
        if (isActive)
        {
            interactionManager.ClearCurrentSelection();
            interactionManager.uiManager.DisplayDefaultText("Longitude View", longitudeDefaultInfo);
        }
    }

    void OnDestroy()
    {
        // Clean up listeners when the object is destroyed.
        if (latitudeToggle != null) latitudeToggle.onValueChanged.RemoveAllListeners();
        if (longitudeToggle != null) longitudeToggle.onValueChanged.RemoveAllListeners();
    }
}