using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class GlobeRotator : MonoBehaviour
{
    [Header("Rotation")]
    [SerializeField] private float rotationSpeed = 20f;
    [Tooltip("Minimum pointer movement (px) before locking an axis.")]
    [SerializeField] private float axisLockThreshold = 5f;

    [Header("Zoom")]
    [SerializeField] private float zoomSpeed = 5f;
    [SerializeField] private float minZoomDistance = 5f;
    [SerializeField] private float maxZoomDistance = 50f;

    [Header("Camera & Reset")]
    [SerializeField] private Camera mainCamera;
    [SerializeField] private float resetDuration = 0.5f;
    [SerializeField] private LeanTweenType resetEaseType = LeanTweenType.easeOutExpo;

    // --- NEW: Vertical clamp settings
    [Header("Vertical Limits")]
    [SerializeField] private float minVerticalAngle = -85f; // Antarctica limit
    [SerializeField] private float maxVerticalAngle = 85f; // Arctic limit

    private SSGeo input;
    private bool isDragging;
    private Vector2 prevPointerPos;
    private Quaternion initialRotation;
    private Vector3 initialCamPos;
    private bool inited;

    private enum AxisLock { None, Horizontal, Vertical }
    private AxisLock lockedAxis = AxisLock.None;

    // --- NEW: Track current vertical tilt (around world X)
    private float currentVerticalAngle = 0f;

    void OnEnable()
    {
        input = new SSGeo();
        input.Gameplay.Enable();
        isDragging = false;
        lockedAxis = AxisLock.None;
    }

    void OnDisable()
    {
        input?.Dispose();
    }

    void Start()
    {
        if (mainCamera == null) mainCamera = Camera.main;
        if (mainCamera == null)
        {
            Debug.LogError("GlobeRotator: No camera. Assign one or tag a camera MainCamera.");
            enabled = false; return;
        }
        StartCoroutine(InitAfterFrame());
    }

    IEnumerator InitAfterFrame()
    {
        yield return new WaitForEndOfFrame();
        initialRotation = transform.rotation;
        initialCamPos = mainCamera.transform.position;
        // --- NEW: Initialize vertical angle from current rotation
        currentVerticalAngle = NormalizeAngle(transform.rotation.eulerAngles.x);
        inited = true;
    }

    void Update()
    {
        if (!inited) return;

        // ---- Press / Release (works for mouse + touch via Press [Pointer])
        if (input.Gameplay.Click.triggered)
        {
            // toggle begin-drag on press; end-drag happens when press is released (below)
            isDragging = true;
            lockedAxis = AxisLock.None;
            prevPointerPos = input.Gameplay.Point.ReadValue<Vector2>();
            LeanTween.cancel(gameObject);
            LeanTween.cancel(mainCamera.gameObject);
        }

        // If the button is up, stop dragging. (ReadValue = 0 when not pressed)
        if (input.Gameplay.Click.ReadValue<float>() <= 0.0f && isDragging)
        {
            isDragging = false;
            lockedAxis = AxisLock.None;
        }

        // ---- Drag rotate
        if (isDragging)
        {
            var cur = input.Gameplay.Point.ReadValue<Vector2>();
            var delta = cur - prevPointerPos;

            if (lockedAxis == AxisLock.None && delta.magnitude >= axisLockThreshold)
                lockedAxis = Mathf.Abs(delta.x) > Mathf.Abs(delta.y) ? AxisLock.Horizontal : AxisLock.Vertical;

            if (lockedAxis == AxisLock.Horizontal)
            {
                transform.Rotate(Vector3.up, -delta.x * rotationSpeed * Time.deltaTime, Space.World);
            }
            else if (lockedAxis == AxisLock.Vertical)
            {
                // --- NEW: Clamp vertical rotation
                float deltaAngle = delta.y * rotationSpeed * Time.deltaTime;
                float newAngle = Mathf.Clamp(currentVerticalAngle + deltaAngle, minVerticalAngle, maxVerticalAngle);
                float appliedDelta = newAngle - currentVerticalAngle;

                transform.Rotate(Vector3.right, appliedDelta, Space.World);
                currentVerticalAngle = newAngle;
            }

            prevPointerPos = cur;
        }

        // ---- Mouse wheel zoom (desktop)
        float scrollY = input.Gameplay.Scroll.ReadValue<Vector2>().y;
        if (Mathf.Abs(scrollY) > Mathf.Epsilon)
            ZoomBy(-scrollY * zoomSpeed * Time.deltaTime);

        // ---- Pinch zoom (mobile/touch)
        var ts = Touchscreen.current;
        if (ts != null && ts.touches.Count >= 2)
        {
            var t0 = ts.touches[0];
            var t1 = ts.touches[1];
            if (t0.isInProgress && t1.isInProgress)
            {
                Vector2 p0 = t0.position.ReadValue();
                Vector2 p1 = t1.position.ReadValue();
                float curDist = Vector2.Distance(p0, p1);

                // store previous distance in prevPointerPos.x (lightweight state)
                if (!isDragging) { prevPointerPos.x = curDist; isDragging = true; } // reuse flag to init once
                else
                {
                    float delta = curDist - prevPointerPos.x;
                    ZoomBy(-delta * (zoomSpeed / 200f)); // scale pinch sensitivity
                    prevPointerPos.x = curDist;
                }
            }
        }
        else if (isDragging && input.Gameplay.Click.ReadValue<float>() <= 0f)
        {
            // clear the pinch init helper if neither drag nor two touches are active
            isDragging = false;
        }
    }

    void ZoomBy(float amount)
    {
        Vector3 toGlobe = transform.position - mainCamera.transform.position;
        float dist = toGlobe.magnitude;
        float target = Mathf.Clamp(dist + amount, minZoomDistance, maxZoomDistance);
        mainCamera.transform.position = transform.position - toGlobe.normalized * target;
    }

    public void ResetRotation()
    {
        if (!inited) return;
        isDragging = false;
        lockedAxis = AxisLock.None;
        // --- UPDATED: Keep currentVerticalAngle in sync during tween
        LeanTween.rotate(gameObject, initialRotation.eulerAngles, resetDuration)
            .setEase(resetEaseType)
            .setOnUpdate((Vector3 val) =>
            {
                // transform rotation is already handled by LT; we just mirror angle
                currentVerticalAngle = NormalizeAngle(val.x);
            });
        LeanTween.move(mainCamera.gameObject, initialCamPos, resetDuration).setEase(resetEaseType);
    }

    public void MoveToTarget(Vector3 eulerRotation, float distance, float duration, LeanTweenType ease)
    {
        if (!inited) return;
        isDragging = false;
        LeanTween.cancel(gameObject);
        LeanTween.cancel(mainCamera.gameObject);

        // --- UPDATED: also update currentVerticalAngle while tweening
        LeanTween.value(gameObject, transform.rotation.eulerAngles, eulerRotation, duration)
                 .setEase(ease)
                 .setOnUpdate((Vector3 val) =>
                 {
                     transform.rotation = Quaternion.Euler(val);
                     currentVerticalAngle = NormalizeAngle(val.x);
                 });

        Vector3 dir = (mainCamera.transform.position - transform.position).normalized;
        Vector3 targetPos = transform.position + dir * distance;
        LeanTween.move(mainCamera.gameObject, targetPos, duration).setEase(ease);
    }

    // --- NEW: angle normalizer to [-180, 180]
    private float NormalizeAngle(float degrees)
    {
        degrees %= 360f;
        if (degrees > 180f) degrees -= 360f;
        if (degrees < -180f) degrees += 360f;
        return degrees;
    }
}
