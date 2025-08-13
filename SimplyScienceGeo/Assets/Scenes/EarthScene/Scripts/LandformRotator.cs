using UnityEngine;
using UnityEngine.InputSystem;

[System.Serializable]
public class AxisLimit
{
    [Tooltip("Enable rotation on this axis")]
    public bool enable = false;
    [Tooltip("Minimum local-rotation angle (degrees)")]
    public float minAngle = -30f;
    [Tooltip("Maximum local-rotation angle (degrees)")]
    public float maxAngle = 30f;
    [HideInInspector] public float current;
}

public class LandformRotator : MonoBehaviour
{
    [Header("General")]
    [SerializeField] private float rotationSpeed = 20f;
    [SerializeField] private Camera mainCamera;

    [Header("Per-axis limits (local)")]
    [SerializeField] private AxisLimit xAxis = new AxisLimit { enable = false };
    [SerializeField] private AxisLimit yAxis = new AxisLimit { enable = true };
    [SerializeField] private AxisLimit zAxis = new AxisLimit { enable = false };

    [Header("Reset button")]
    [SerializeField] private float resetDuration = 0.5f;
    [SerializeField] private LeanTweenType resetEase = LeanTweenType.easeOutExpo;

    private bool isRotating;
    private Vector2 prevPointerPos;
    private Vector3 initialEuler;

    private void EnsureCamera()
    {
        if (!mainCamera)
        {
            mainCamera = Camera.main;
            if (!mainCamera)
            {
                Debug.LogError($"{name}/LandformRotator: No camera assigned and Camera.main not found.");
                enabled = false;
            }
        }
    }

    private static float Normalize(float angle)
    {
        angle %= 360f;
        if (angle > 180f) angle -= 360f;
        if (angle < -180f) angle += 360f;
        return angle;
    }

    private static float Clamp(float angle, float min, float max)
    {
        angle = Normalize(angle);
        return Mathf.Clamp(angle, min, max);
    }

    private void Start()
    {
        EnsureCamera();
        initialEuler = transform.localEulerAngles;
        xAxis.current = Normalize(initialEuler.x);
        yAxis.current = Normalize(initialEuler.y);
        zAxis.current = Normalize(initialEuler.z);
    }

    private void Update()
    {
        if (!ModelActivator.IsFullyActive)
        {
            isRotating = false;
            return;
        }

        var mouse = Mouse.current;
        var touch = Touchscreen.current;

        // FIXED: Added .press to access wasPressedThisFrame
        bool isPressed = (mouse != null && mouse.leftButton.wasPressedThisFrame) || (touch != null && touch.primaryTouch.press.wasPressedThisFrame);
        // FIXED: Added .press to access wasReleasedThisFrame
        bool isReleased = (mouse != null && mouse.leftButton.wasReleasedThisFrame) || (touch != null && touch.primaryTouch.press.wasReleasedThisFrame);

        if (mainCamera == null) return;

        if (isPressed)
        {
            LeanTween.cancel(gameObject);
            isRotating = true;
            prevPointerPos = touch != null && touch.primaryTouch.press.isPressed ? touch.primaryTouch.position.ReadValue() : mouse.position.ReadValue();
        }

        if (isReleased)
        {
            isRotating = false;
        }

        if (!isRotating) return;

        Vector2 now = touch != null && touch.primaryTouch.press.isPressed ? touch.primaryTouch.position.ReadValue() : mouse.position.ReadValue();
        Vector2 delta = (now - prevPointerPos) * rotationSpeed * Time.deltaTime;
        prevPointerPos = now;

        if (yAxis.enable)
            yAxis.current = Clamp(yAxis.current - delta.x, yAxis.minAngle, yAxis.maxAngle);
        if (xAxis.enable)
            xAxis.current = Clamp(xAxis.current + delta.y, xAxis.minAngle, xAxis.maxAngle);
        if (zAxis.enable && Keyboard.current != null && Keyboard.current.leftAltKey.isPressed)
            zAxis.current = Clamp(zAxis.current + delta.y, zAxis.minAngle, zAxis.maxAngle);

        transform.localEulerAngles = new Vector3(xAxis.current, yAxis.current, zAxis.current);
    }

    public void ResetRotation()
    {
        if (!ModelActivator.IsFullyActive) return;
        isRotating = false;
        LeanTween.rotateLocal(gameObject, initialEuler, resetDuration)
                 .setEase(resetEase)
                 .setOnUpdate((Vector3 v) =>
                 {
                     xAxis.current = Normalize(v.x);
                     yAxis.current = Normalize(v.y);
                     zAxis.current = Normalize(v.z);
                 });
    }
}