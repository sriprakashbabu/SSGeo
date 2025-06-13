using UnityEngine;

/// Detects double-clicks on this mesh and pushes info to GlobeInfoUIManager.
[RequireComponent(typeof(Collider))]
public class ClickInfoHandler : MonoBehaviour
{
    [TextArea]
    [Tooltip("Text to show when this object is double-clicked.")]
    public string infoText;

    // Changed from static to instance variable for per-object double-click detection
    private float _instanceLastClickTime;
    private const float DBL_CLICK_THRESHOLD = 0.3f; // seconds (standard naming convention for constants)

    private GlobeInfoUIManager _uiManager;

    private void Start()
    {
        _uiManager = FindObjectOfType<GlobeInfoUIManager>();
        if (!_uiManager)
        {
            Debug.LogWarning($"ClickInfoHandler on '{gameObject.name}': No GlobeInfoUIManager found in scene. Info display will not work.", this);
        }
        // Initialize to a value that ensures the first click isn't a double click
        _instanceLastClickTime = -DBL_CLICK_THRESHOLD; // Ensures first click is not a double click
    }

    private void OnMouseDown() // Works for touch & mouse
    {
        Debug.Log($"OnMouseDown fired on: {gameObject.name} at Time: {Time.time:F2}");

        if (Time.time - _instanceLastClickTime < DBL_CLICK_THRESHOLD)
        {
            // === DOUBLE-CLICK DETECTED ===
            Debug.Log($"DOUBLE-CLICK detected on {gameObject.name}! Info Text: '{infoText}'");
            if (_uiManager != null)
            {
                if (!string.IsNullOrEmpty(infoText))
                {
                    _uiManager.DisplayInformation(infoText);
                }
                else
                {
                    Debug.LogWarning($"Double-click on {gameObject.name}, but 'Info Text' is empty in the Inspector.", this);
                    // Optionally display a default message for empty info text:
                    // _uiManager.DisplayInformation($"{gameObject.name}: No specific information available.");
                }
            }
            else
            {
                Debug.LogWarning($"Double-click on {gameObject.name}, but _uiManager reference is null (was not found in Start).", this);
            }
            // Reset last click time after a double click to require two new clicks for the next double click
            _instanceLastClickTime = -DBL_CLICK_THRESHOLD;
        }
        else
        {
            // --- SINGLE-CLICK (or first click of a potential double-click) ---
            Debug.Log($"Single click (or first part of double-click) on {gameObject.name}. Storing click time.");
        }
        // Update last click time to the current time for this instance
        _instanceLastClickTime = Time.time;
    }
}