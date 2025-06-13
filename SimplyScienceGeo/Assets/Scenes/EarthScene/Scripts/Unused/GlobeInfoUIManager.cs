using UnityEngine;
using TMPro; // Required for TextMeshPro UI elements

public class GlobeInfoUIManager : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("Assign the TextMeshProUGUI element here to display information.")]
    public TextMeshProUGUI infoDisplayBox;

    void Awake()
    {
        if (infoDisplayBox == null)
        {
            Debug.LogError("GlobeInfoUIManager: Info Display Box (TextMeshProUGUI) not assigned in the Inspector!", this);
            enabled = false; // Disable script if not set up
            return;
        }
        // Set initial text
        infoDisplayBox.text = "Click on a geographic feature for information.";
    }

    // Subscribe to the event when this component is enabled
    // This part was for the event-based system with SpecialLineInfo.cs
    // If GlobeGridController now directly calls DisplayInformation,
    // you might not need these OnEnable/OnDisable event subscriptions
    // for this particular direct call, but they don't hurt if SpecialLineInfo
    // might still exist or be reintroduced for other purposes.
    // For the consolidated approach where GlobeGridController calls this directly,
    // these event subscriptions related to SpecialLineInfo.OnSpecialLineClicked
    // are no longer the primary way this method is used by GlobeGridController.
    /*
    void OnEnable()
    {
        SpecialLineInfo.OnSpecialLineClicked += DisplayInformation;
    }

    void OnDisable()
    {
        SpecialLineInfo.OnSpecialLineClicked -= DisplayInformation;
    }
    */

    /// <summary>
    /// Updates the text box with the provided information.
    /// Now public so GlobeGridController can call it.
    /// </summary>
    /// <param name="info">The string information to display.</param>
    public void DisplayInformation(string info) // Changed to public
    {
        if (infoDisplayBox != null)
        {
            infoDisplayBox.text = info;
        }
        else
        {
            // This case should ideally be caught by the Awake check
            Debug.LogWarning("GlobeInfoUIManager: Attempted to display info, but Info Display Box is not assigned.", this);
        }
    }
}