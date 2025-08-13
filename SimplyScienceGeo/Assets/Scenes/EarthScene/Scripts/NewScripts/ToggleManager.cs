using UnityEngine;
using UnityEngine.UI; // Required for the Toggle and ToggleGroup components
using TMPro; // Required for TextMeshProUGUI

/// <summary>
/// Manages the toggling between displaying country-based timezones
/// and longitude-based timezones, and updates the UI accordingly.
/// It also handles resetting the interaction state, specifically for a ToggleGroup setup.
/// </summary>
public class ToggleManager : MonoBehaviour
{
    [Header("Manager References")]
    [Tooltip("Assign the InteractionManager from the scene.")]
    public InteractionManager interactionManager;

    [Tooltip("Assign the UIManager from the scene.")]
    public UIManager uiManager;

    [Header("Toggle Group Setup")]
    [Tooltip("The ToggleGroup component that manages the exclusive selection of toggles.")]
    public ToggleGroup displayModeToggleGroup;

    [Tooltip("The Toggle for displaying timezones based on countries. Should be part of the ToggleGroup.")]
    public Toggle countryTimezonesToggle;

    [Tooltip("The Toggle for displaying timezones based on longitudes. Should be part of the ToggleGroup.")]
    public Toggle longitudeTimezonesToggle;

    [Header("Display Objects")]
    [Tooltip("The GameObject representing timezones based on countries.")]
    public GameObject countryTimezonesObject;

    [Tooltip("The GameObject representing timezones based on longitudes.")]
    public GameObject longitudeTimezonesObject;

    [Header("Display Text")]
    [Tooltip("The title to display when country timezones are active.")]
    public string countryTimezoneTitle = "Countries & Timezones";

    [TextArea(3, 10)]
    [Tooltip("The body text to display when country timezones are active.")]
    public string countryTimezoneBody = "Click on a country to see its timezone information.";

    [Tooltip("The title to display when longitude timezones are active.")]
    public string longitudeTimezoneTitle = "Longitudes & Timezones";

    [TextArea(3, 10)]
    [Tooltip("The body text to display when longitude timezones are active.")]
    public string longitudeTimezoneBody = "Click on a longitude line to see its associated timezone.";

    void Start()
    {
        // Ensure all required references are set
        if (interactionManager == null) { Debug.LogError("ToggleManager: InteractionManager not assigned!"); return; }
        if (uiManager == null) { Debug.LogError("ToggleManager: UIManager not assigned!"); return; }
        if (displayModeToggleGroup == null) { Debug.LogError("ToggleManager: Display Mode Toggle Group not assigned!"); return; }
        if (countryTimezonesToggle == null) { Debug.LogError("ToggleManager: Country Timezones Toggle not assigned!"); return; }
        if (longitudeTimezonesToggle == null) { Debug.LogError("ToggleManager: Longitude Timezones Toggle not assigned!"); return; }
        if (countryTimezonesObject == null) { Debug.LogError("ToggleManager: Country Timezones Object not assigned!"); return; }
        if (longitudeTimezonesObject == null) { Debug.LogError("ToggleManager: Longitude Timezones Object not assigned!"); return; }

        // Add listeners to individual toggles within the group
        countryTimezonesToggle.onValueChanged.AddListener(OnCountryToggleChanged);
        longitudeTimezonesToggle.onValueChanged.AddListener(OnLongitudeToggleChanged);

        // Set initial state based on which toggle is active in the group
        // This makes sure the correct objects and text are shown on start.
        if (countryTimezonesToggle.isOn)
        {
            OnCountryToggleChanged(true);
        }
        else if (longitudeTimezonesToggle.isOn)
        {
            OnLongitudeToggleChanged(true);
        }
        else // Fallback if neither is initially on, or if you want a default
        {
            countryTimezonesToggle.isOn = true; // Force country toggle on if nothing is selected
        }
    }

    /// <summary>
    /// Called when the Country Timezones Toggle's value changes.
    /// </summary>
    /// <param name="isOn">True if the toggle is on, false if off.</param>
    private void OnCountryToggleChanged(bool isOn)
    {
        // Only react if this toggle is being turned ON.
        // The ToggleGroup ensures only one can be ON at a time.
        if (isOn)
        {
            // 1. Reset any currently selected interactable feature and UI
            interactionManager.ResetToDefaultUI();

            // 2. Enable/Disable the appropriate game objects
            countryTimezonesObject.SetActive(true);
            longitudeTimezonesObject.SetActive(false);

            // 3. Change the starting message
            uiManager.DisplayDefaultText(countryTimezoneTitle, countryTimezoneBody);
        }
    }

    /// <summary>
    /// Called when the Longitude Timezones Toggle's value changes.
    /// </summary>
    /// <param name="isOn">True if the toggle is on, false if off.</param>
    private void OnLongitudeToggleChanged(bool isOn)
    {
        // Only react if this toggle is being turned ON.
        if (isOn)
        {
            // 1. Reset any currently selected interactable feature and UI
            interactionManager.ResetToDefaultUI();

            // 2. Enable/Disable the appropriate game objects
            countryTimezonesObject.SetActive(false);
            longitudeTimezonesObject.SetActive(true);

            // 3. Change the starting message
            uiManager.DisplayDefaultText(longitudeTimezoneTitle, longitudeTimezoneBody);
        }
    }
}