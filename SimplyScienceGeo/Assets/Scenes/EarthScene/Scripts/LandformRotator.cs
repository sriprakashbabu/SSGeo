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

    private SSGeo _input; // 🆕 Reference to your new Input Action Asset
    private bool isRotating;
    private Vector2 prevPointerPos;
    private Vector3 initialEuler;

    // 🆕 Use Awake() to create and subscribe to the input events just once.
    void Awake()
    {
        _input = new SSGeo();
        _input.Gameplay.Click.started += OnRotationStarted;
        _input.Gameplay.Click.canceled += OnRotationEnded;
    }

    // 🆕 OnEnable() simply enables the input action map.
    private void OnEnable()
    {
        _input.Enable();
    }

    // 🆕 OnDisable() simply disables the input action map.
    private void OnDisable()
    {
        _input.Disable();
    }

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

    // 🆕 New event handler for when rotation starts
    private void OnRotationStarted(InputAction.CallbackContext context)
    {
        if (!ModelActivator.IsFullyActive) return;
        LeanTween.cancel(gameObject);
        isRotating = true;
        prevPointerPos = _input.Gameplay.Point.ReadValue<Vector2>();
    }

    // 🆕 New event handler for when rotation ends
    private void OnRotationEnded(InputAction.CallbackContext context)
    {
        isRotating = false;
    }

    // Update is now only for continuous rotation logic
    private void Update()
    {
        if (!isRotating || !ModelActivator.IsFullyActive) return;

        Vector2 now = _input.Gameplay.Point.ReadValue<Vector2>();
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