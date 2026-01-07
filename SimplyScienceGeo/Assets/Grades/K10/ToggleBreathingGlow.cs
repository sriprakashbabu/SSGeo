using UnityEngine;
using UnityEngine.UI;

public class ToggleScalePulsing : MonoBehaviour
{
    [Header("References")]
    public Toggle linkedToggle;
    [Tooltip("Assign the specific object you want to scale (e.g., the Icon image). If empty, it scales this object.")]
    public Transform targetToScale;

    [Header("Animation Settings")]
    public float pulseSpeed = 5.0f;
    [Tooltip("Normal size (usually 1)")]
    public float minScale = 1.0f;
    [Tooltip("How big it gets (e.g., 1.2 is 20% bigger)")]
    public float maxScale = 1.2f;

    private Vector3 baseScale;

    void Start()
    {
        if (linkedToggle == null) linkedToggle = GetComponent<Toggle>();
        if (targetToScale == null) targetToScale = transform;

        // Remember the size it started at so we don't distort it later
        baseScale = targetToScale.localScale;

        linkedToggle.onValueChanged.AddListener(OnToggleChanged);
    }

    private void OnToggleChanged(bool isOn)
    {
        if (!isOn && targetToScale != null)
        {
            // Snap back to normal immediately when deselected
            targetToScale.localScale = baseScale;
        }
    }

    void Update()
    {
        // Only animate if the toggle is ON
        if (linkedToggle != null && linkedToggle.isOn && targetToScale != null)
        {
            // Calculate a smooth sine wave between 0 and 1
            float wave = (Mathf.Sin(Time.time * pulseSpeed) + 1.0f) / 2.0f;

            // Interpolate between min and max scale
            float scale = Mathf.Lerp(minScale, maxScale, wave);

            targetToScale.localScale = baseScale * scale;
        }
    }
}