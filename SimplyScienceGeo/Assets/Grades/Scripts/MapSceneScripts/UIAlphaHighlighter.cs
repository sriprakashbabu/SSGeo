using UnityEngine;
using UnityEngine.UI;

public class UIAlphaHighlighter : MonoBehaviour
{
    [Range(0f, 1f)] public float normalAlpha = 0.2f;       // Idle alpha
    [Range(0f, 1f)] public float pulseMinAlpha = 0.3f;      // Min during pulse
    [Range(0f, 1f)] public float pulseMaxAlpha = 0.6f;      // Max during pulse
    public float pulseSpeed = 2f;                           // Pulses per second
    public float fadeSpeed = 5f;                            // How fast to fade when deselected

    private Image _image;
    private InteractableFeature _feature;
    private InteractionManager _interactionManager;
    private bool _isSelected;

    void Awake()
    {
        _image = GetComponent<Image>();
        _feature = GetComponent<InteractableFeature>();

        if (_image == null)
            Debug.LogError("UIAlphaPulseHighlighter requires an Image component.", this);
        if (_feature == null)
            Debug.LogError("UIAlphaPulseHighlighter requires an InteractableFeature component.", this);

        _interactionManager = FindObjectOfType<InteractionManager>();
        if (_interactionManager == null)
            Debug.LogError("No InteractionManager found in scene for UIAlphaPulseHighlighter.", this);

        SetAlpha(normalAlpha);
    }

    void Update()
    {
        if (_interactionManager != null && _feature != null)
            _isSelected = (_interactionManager.CurrentlySelectedFeature == _feature);

        if (_isSelected)
        {
            // Pulse alpha between min and max
            float t = (Mathf.Sin(Time.time * pulseSpeed * Mathf.PI * 2) + 1f) / 2f; // 0→1
            float alpha = Mathf.Lerp(pulseMinAlpha, pulseMaxAlpha, t);
            SetAlpha(alpha);
        }
        else
        {
            // Fade back to idle alpha
            float current = _image.color.a;
            float newAlpha = Mathf.Lerp(current, normalAlpha, Time.deltaTime * fadeSpeed);
            SetAlpha(newAlpha);
        }
    }

    private void SetAlpha(float alpha)
    {
        Color c = _image.color;
        c.a = alpha;
        _image.color = c;
    }
}