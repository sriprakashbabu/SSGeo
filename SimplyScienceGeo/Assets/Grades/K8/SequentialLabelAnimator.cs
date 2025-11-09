using UnityEngine;
// The 'using LeanTween;' directive has been removed as LeanTween is a static class, not a namespace.
// Its methods are accessed directly via 'LeanTween.MethodName'.

/// <summary>
/// Handles the sequential animated appearance of two child GameObjects 
/// (a Line/Cube and a Text element) when the parent object is enabled.
/// </summary>
public class SequentialLabelAnimator : MonoBehaviour
{
    [Header("Animation Settings")]
    [Tooltip("Delay before the entire sequence begins.")]
    public float activationDelay = 0.5f;

    [Tooltip("Duration for the Line/Cube (second child) to scale up.")]
    public float lineAnimationDuration = 0.3f;

    [Tooltip("Duration for the Text (first child) to scale up.")]
    public float textAnimationDuration = 0.2f;

    [Tooltip("Ease type for the scaling animations (e.g., easeOutBack is great for popping).")]
    public LeanTweenType easeType = LeanTweenType.easeOutBack;

    [Header("Floating Effect (Text Only)")]
    [Tooltip("Enable a gentle up-and-down floating effect on the text after it appears.")]
    public bool enableFloatingEffect = true;

    [Tooltip("The vertical distance the text will move up from its base position.")]
    public float floatAmplitude = 0.01f;

    [Tooltip("The time (in seconds) it takes for the text to complete one full up-and-down cycle.")]
    public float floatCycleDuration = 2.0f;


    // Internal references
    private Transform textTransform;
    private Transform lineTransform;
    private Vector3 targetScaleText;
    private Vector3 targetScaleLine;
    private Vector3 targetLocalPosText; // To store the original local position for floating

    // --- Setup and Initialization ---

    void Awake()
    {
        // Check for the required child objects as per the setup described:
        // Child 0: Text
        // Child 1: Cube/Line

        if (transform.childCount < 2)
        {
            Debug.LogError("SequentialLabelAnimator requires at least two children (Text and Line). Please check the hierarchy.");
            return;
        }

        // 1. Get references to the child Transforms
        textTransform = transform.GetChild(0);
        lineTransform = transform.GetChild(1);

        // 2. Store the desired final (target) scales from the Inspector
        targetScaleText = textTransform.localScale;
        targetScaleLine = lineTransform.localScale;

        // 3. Store the final local position of the text
        targetLocalPosText = textTransform.localPosition;

        // 4. Initialize both children to scale zero (hidden)
        textTransform.localScale = Vector3.zero;
        lineTransform.localScale = Vector3.zero;

        // Initialize LeanTween. Calling this multiple times is safe.
        // The problematic 'if' check has been removed.
        LeanTween.init(800); // Initialize with a generous capacity
    }

    // --- Activation Trigger ---

    void OnEnable()
    {
        // This method is called whenever the parent GameObject (and thus this script) is activated.
        ShowLabel();
    }

    /// <summary>
    /// Starts the sequential animation of the line and then the text.
    /// </summary>
    public void ShowLabel()
    {
        // Always stop existing tweens to prevent conflicts if OnEnable is called multiple times
        // Added null checks for safety
        if (textTransform != null)
        {
            LeanTween.cancel(textTransform.gameObject);
        }
        if (lineTransform != null)
        {
            LeanTween.cancel(lineTransform.gameObject);
        }

        // Ensure transforms are still valid before proceeding
        if (textTransform == null || lineTransform == null)
        {
            Debug.LogError("Child transforms are missing. Was setup in Awake() successful?");
            return;
        }

        // Reset text position before animating, in case it was hidden mid-float
        textTransform.localPosition = targetLocalPosText;

        // 1. Initial delay for the whole sequence
        LeanTween.delayedCall(gameObject, activationDelay, () => {

            // 2. Animate the Line (Cube) scale
            LeanTween.scale(lineTransform.gameObject, targetScaleLine, lineAnimationDuration)
                .setEase(easeType)
                // 3. On completion of the line animation, start the text animation
                .setOnComplete(() => {

                    // 4. Animate the Text scale
                    LeanTween.scale(textTransform.gameObject, targetScaleText, textAnimationDuration)
                        .setEase(easeType)
                        // 5. On completion of the text scaling, start the float
                        .setOnComplete(StartFloatingAnimation); // Call the new float method
                });
        });
    }

    /// <summary>
    /// Starts the floating animation on the text element if enabled.
    /// </summary>
    void StartFloatingAnimation()
    {
        if (!enableFloatingEffect || textTransform == null)
        {
            return;
        }

        // Calculate the target "up" position
        float targetY = targetLocalPosText.y + floatAmplitude;

        // Use moveLocalY with setLoopPingPong to create a smooth up-and-down motion.
        // The time parameter (floatCycleDuration / 2.0f) is for one direction (half the cycle).
        LeanTween.moveLocalY(textTransform.gameObject, targetY, floatCycleDuration / 2.0f)
            .setEase(LeanTweenType.easeInOutSine) // A smooth ease for floating
            .setLoopPingPong(); // This makes it go back and forth
    }


    /// <summary>
    /// Instantly hides the label elements by resetting their scale and position.
    /// </summary>
    public void HideLabelInstant()
    {
        // Stop all ongoing tweens
        if (textTransform != null)
        {
            LeanTween.cancel(textTransform.gameObject);
        }
        if (lineTransform != null)
        {
            LeanTween.cancel(lineTransform.gameObject);
        }

        // Reset scale and position instantly
        if (textTransform != null && lineTransform != null)
        {
            textTransform.localScale = Vector3.zero;
            textTransform.localPosition = targetLocalPosText; // Reset position
            lineTransform.localScale = Vector3.zero;
        }
    }
}