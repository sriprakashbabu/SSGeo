using UnityEngine;
using TMPro; // Make sure you have TextMesh Pro imported

/// <summary>
/// The "Voice". Manages all UI elements, such as showing and hiding
/// the information panel for a selected feature.
/// </summary>
public class UIManager : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("The parent GameObject for the entire info panel.")]
    public GameObject infoPanel;

    [Tooltip("The TextMeshPro text element for the feature's name/title.")]
    public TextMeshProUGUI titleText;

    [Tooltip("The TextMeshPro text element for the feature's detailed information.")]
    public TextMeshProUGUI bodyText;

    void Start()
    {
        // Start with the panel hidden.
        HideInformation();
    }

    public void DisplayInformation(string info, string title)
    {
        if (infoPanel == null || titleText == null || bodyText == null) return;

        titleText.text = title;
        bodyText.text = info;
        infoPanel.SetActive(true);
    }
    // --- ADD THIS NEW METHOD ---
    /// <summary>
    /// Displays a default message in the info panel when no feature is selected.
    /// </summary>
    public void DisplayDefaultText(string title, string body)
    {
        if (infoPanel == null || titleText == null || bodyText == null) return;

        titleText.text = title;
        bodyText.text = body;
        infoPanel.SetActive(true); // Ensure panel is visible
    }
    public void HideInformation()
    {
        if (infoPanel == null) return;

        infoPanel.SetActive(false);
    }
}