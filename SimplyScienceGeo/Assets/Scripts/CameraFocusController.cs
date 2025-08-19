using UnityEngine;

/// <summary>
/// This component can be attached to any GameObject. It holds a predefined
/// rotation and zoom level. Its public TriggerFocus() method can be called
/// from any UnityEvent (like a Button's OnClick, a Toggle's OnValueChanged,
/// or a Dropdown's OnValueChanged) to command the GlobeRotator.
/// </summary>
public class CameraFocusController : MonoBehaviour
{
    [Header("Target References")]
    [Tooltip("The GlobeRotator script in the scene that controls the main globe. If left empty, it will try to find it automatically.")]
    public GlobeRotator globeRotator;

    [Header("Focus Point Settings")]
    [Tooltip("The target rotation (in Euler angles) for the globe to focus on.")]
    public Vector3 targetRotation;

    [Tooltip("The target distance for the camera from the globe's center.")]
    public float targetZoom;

    [Header("Animation Settings")]
    [Tooltip("How long the camera transition should take in seconds.")]
    public float transitionDuration = 1.5f;
    [Tooltip("The easing function for the animation, which controls the acceleration and deceleration.")]
    public LeanTweenType easeType = LeanTweenType.easeInOutSine;

    void Start()
    {
        // If the GlobeRotator wasn't assigned in the Inspector, try to find it in the scene.
        if (globeRotator == null)
        {
            globeRotator = FindObjectOfType<GlobeRotator>();
            if (globeRotator == null)
            {
                Debug.LogError("CameraFocusController: A GlobeRotator script was not found in the scene. Please assign it in the Inspector.", this);
                // Disable this component if the rotator is missing.
                enabled = false;
            }
        }
    }
    [ContextMenu("Print Rotation & Zoom (Copy-Paste Ready)")]
    public void PrintRotationAndZoom()
    {
        if (globeRotator == null)
        {
            Debug.LogWarning("GlobeRotator reference is missing.");
            return;
        }

        Vector3 rotation = globeRotator.transform.rotation.eulerAngles;

        // Convert to -180 to 180 format for Inspector-matching values
        rotation.x = NormalizeAngle(rotation.x);
        rotation.y = NormalizeAngle(rotation.y);
        rotation.z = NormalizeAngle(rotation.z);

        float zoom = Vector3.Distance(Camera.main.transform.position, globeRotator.transform.position);

        string objectName = gameObject.name;
        string output = $"[{objectName}] Camera Focus Values:\n" +
                        $"Target Rotation: new Vector3({rotation.x:F1}f, {rotation.y:F1}f, {rotation.z:F1}f)\n" +
                        $"Target Zoom: {zoom:F2}f";

        Debug.Log(output);
    }

    private float NormalizeAngle(float angle)
    {
        return (angle > 180f) ? angle - 360f : angle;
    }



    /// <summary>
    /// This public method can be called from any UnityEvent to start the camera transition.
    /// </summary>
    public void TriggerFocus()
    {
        // If our GlobeRotator is valid, tell it to move to our specified target.
        if (globeRotator != null)
        {
            globeRotator.MoveToTarget(targetRotation, targetZoom, transitionDuration, easeType);
        }
    }
}
