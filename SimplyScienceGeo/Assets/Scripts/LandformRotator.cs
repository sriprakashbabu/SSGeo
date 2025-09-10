using UnityEngine;
using UnityEngine.InputSystem;

[System.Serializable]
public class AxisConfig
{
    [Tooltip("If ON, this axis will rotate.")]
    public bool enable = true;

    [Tooltip("If ON, clamp rotation between minAngle and maxAngle.")]
    public bool useLimits = false;

    public float minAngle = -45f;
    public float maxAngle = 45f;

    [Tooltip("Degrees per (pixel/sec) of pointer movement.")]
    public float speed = 80f;

    [HideInInspector] public float current;
}

public class LandformRotator : MonoBehaviour
{
    [Header("Axes")]
    [SerializeField] private AxisConfig xAxis = new AxisConfig { enable = true, useLimits = false, minAngle = -45f, maxAngle = 45f, speed = 80f };
    [SerializeField] private AxisConfig yAxis = new AxisConfig { enable = true, useLimits = false, minAngle = -90f, maxAngle = 90f, speed = 80f };
    [SerializeField] private AxisConfig zAxis = new AxisConfig { enable = false, useLimits = false, minAngle = -45f, maxAngle = 45f, speed = 80f };

    [Tooltip("If ON, Z (roll) uses horizontal drag; if OFF, it uses vertical drag.")]
    [SerializeField] private bool zUseHorizontalDrag = true;

    [Header("Interaction")]
    [Tooltip("Hold click/touch to rotate (from your Input Actions).")]
    [SerializeField] private bool holdToRotate = true;

    [Header("Reset Button")]
    [SerializeField] private float resetDuration = 0.5f;
    [SerializeField] private LeanTweenType resetEase = LeanTweenType.easeOutExpo;

    [Header("Advanced")]
    [Tooltip("If ON, rotation only when ModelActivator.IsFullyActive is true (keep OFF for the new map flow).")]
    [SerializeField] private bool requireFullyActive = false;

    // --- Input (uses your Input System actions asset class) ---
    private SSGeo _input;               // your generated input class
    private bool isRotating;
    private Vector2 prevPointerPos;

    // --- State ---
    private Vector3 initialEuler;
    private int _resetTweenId = -1;     // only cancel our own reset tween

    private void Awake()
    {
        _input = new SSGeo();
        _input.Gameplay.Click.started += OnPointerDown;
        _input.Gameplay.Click.canceled += OnPointerUp;
    }

    private void OnEnable() { _input.Enable(); }
    private void OnDisable() { _input.Disable(); }

    private void Start()
    {
        initialEuler = transform.localEulerAngles;
        xAxis.current = Normalize(initialEuler.x);
        yAxis.current = Normalize(initialEuler.y);
        zAxis.current = Normalize(initialEuler.z);
    }

    private void OnPointerDown(InputAction.CallbackContext _)
    {
        if (!holdToRotate) return;
        if (requireFullyActive && !ModelActivator.IsFullyActive) return;

        // Cancel ONLY our reset tween so we don't nuke scale tweens
        if (_resetTweenId != -1) { LeanTween.cancel(_resetTweenId); _resetTweenId = -1; }

        isRotating = true;
        prevPointerPos = _input.Gameplay.Point.ReadValue<Vector2>();
    }

    private void OnPointerUp(InputAction.CallbackContext _) => isRotating = false;

    private void Update()
    {
        if (requireFullyActive && !ModelActivator.IsFullyActive) return;

        // If not using hold-to-rotate, we still need a 'button down' once to init prevPointerPos
        if (!holdToRotate && Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
            prevPointerPos = _input.Gameplay.Point.ReadValue<Vector2>();

        if (holdToRotate && !isRotating) return;

        Vector2 now = _input.Gameplay.Point.ReadValue<Vector2>();
        Vector2 delta = (now - prevPointerPos) * Time.deltaTime;  // pixels/sec-ish
        prevPointerPos = now;

        // Yaw (Y) ← horizontal drag
        if (yAxis.enable)
        {
            float d = -delta.x * yAxis.speed;
            yAxis.current = yAxis.useLimits
                ? Clamp(yAxis.current + d, yAxis.minAngle, yAxis.maxAngle)
                : Normalize(yAxis.current + d);
        }

        // Pitch (X) ← vertical drag
        if (xAxis.enable)
        {
            float d = delta.y * xAxis.speed;
            xAxis.current = xAxis.useLimits
                ? Clamp(xAxis.current + d, xAxis.minAngle, xAxis.maxAngle)
                : Normalize(xAxis.current + d);
        }

        // Roll (Z) ← choose horizontal or vertical drag
        if (zAxis.enable)
        {
            float drag = zUseHorizontalDrag ? delta.x : delta.y;
            float d = drag * zAxis.speed;
            zAxis.current = zAxis.useLimits
                ? Clamp(zAxis.current + d, zAxis.minAngle, zAxis.maxAngle)
                : Normalize(zAxis.current + d);
        }

        transform.localEulerAngles = new Vector3(xAxis.current, yAxis.current, zAxis.current);
    }

    public void ResetRotation()
    {
        isRotating = false;
        _resetTweenId = LeanTween.rotateLocal(gameObject, initialEuler, resetDuration)
                                 .setEase(resetEase)
                                 .setOnUpdate((Vector3 v) =>
                                 {
                                     xAxis.current = Normalize(v.x);
                                     yAxis.current = Normalize(v.y);
                                     zAxis.current = Normalize(v.z);
                                 }).id;
    }

    // --- Helpers ---
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
}
