using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class GlobeRotator : MonoBehaviour
{
    [Header("Rotation Settings")]
    [SerializeField] private float rotationSpeed = 20f;

    [Header("Zoom Settings")]
    [SerializeField] private float zoomSpeed = 5f;
    [SerializeField] private float minZoomDistance = 5f;
    [SerializeField] private float maxZoomDistance = 50f;

    [Header("Camera & Reset")]
    [SerializeField] private Camera mainCamera;
    [SerializeField] private float resetDuration = 0.5f;
    [SerializeField] private LeanTweenType resetEaseType = LeanTweenType.easeOutExpo;

    [Tooltip("Minimum mouse movement before locking rotation axis.")]
    [SerializeField] private float axisLockThreshold = 5f;

    private Vector2 _previousPointerPosition;
    private bool _isRotating = false;

    private Quaternion _initialRotation;
    private Vector3 _initialCameraPosition;
    private bool _isInitialized = false;

    private enum AxisLock { None, Horizontal, Vertical }
    private AxisLock lockedAxis = AxisLock.None;

    private void OnEnable() => _isRotating = false;

    private void EnsureCamera()
    {
        if (mainCamera != null) return;
        mainCamera = Camera.main;
        if (mainCamera == null)
        {
            Debug.LogError("GlobeRotator: Main camera not found.");
            enabled = false;
        }
    }

    private void Start()
    {
        EnsureCamera();
        StartCoroutine(InitializePositionsAfterFrameEnd());
    }

    private IEnumerator InitializePositionsAfterFrameEnd()
    {
        yield return new WaitForEndOfFrame();
        _initialRotation = transform.rotation;
        if (mainCamera != null)
        {
            _initialCameraPosition = mainCamera.transform.position;
        }
        _isInitialized = true;
    }

    private void Update()
    {
        if (!_isInitialized || mainCamera == null) return;

        var mouse = Mouse.current;
        var touch = Touchscreen.current;
        if (mouse == null && touch == null) return;

        HandleRotation(mouse, touch);
        HandleZoom(mouse, touch);
    }

    private void HandleRotation(Mouse mouse, Touchscreen touch)
    {
        // FIXED: Added .press to access wasPressedThisFrame
        bool isPressed = (mouse != null && mouse.leftButton.wasPressedThisFrame) || (touch != null && touch.primaryTouch.press.wasPressedThisFrame);
        // FIXED: Added .press to access wasReleasedThisFrame
        bool isReleased = (mouse != null && mouse.leftButton.wasReleasedThisFrame) || (touch != null && touch.primaryTouch.press.wasReleasedThisFrame);

        if (isPressed)
        {
            if (touch != null && touch.touches.Count > 1) return;

            LeanTween.cancel(gameObject);
            if (mainCamera != null) LeanTween.cancel(mainCamera.gameObject);
            _isRotating = true;
            _previousPointerPosition = touch != null && touch.primaryTouch.press.isPressed ? touch.primaryTouch.position.ReadValue() : mouse.position.ReadValue();
            lockedAxis = AxisLock.None;
        }

        if (isReleased)
        {
            _isRotating = false;
            lockedAxis = AxisLock.None;
        }

        if (_isRotating && (touch == null || touch.touches.Count <= 1))
        {
            Vector2 currentPointerPosition = touch != null && touch.primaryTouch.press.isPressed ? touch.primaryTouch.position.ReadValue() : mouse.position.ReadValue();
            Vector2 deltaPointer = currentPointerPosition - _previousPointerPosition;

            if (lockedAxis == AxisLock.None && deltaPointer.magnitude >= axisLockThreshold)
            {
                lockedAxis = Mathf.Abs(deltaPointer.x) > Mathf.Abs(deltaPointer.y) ? AxisLock.Horizontal : AxisLock.Vertical;
            }

            if (lockedAxis == AxisLock.Horizontal)
            {
                transform.Rotate(Vector3.up, -deltaPointer.x * rotationSpeed * Time.deltaTime, Space.World);
            }
            else if (lockedAxis == AxisLock.Vertical)
            {
                transform.Rotate(Vector3.right, deltaPointer.y * rotationSpeed * Time.deltaTime, Space.World);
            }
            _previousPointerPosition = currentPointerPosition;
        }
    }

    private void HandleZoom(Mouse mouse, Touchscreen touch)
    {
        if (touch != null && touch.touches.Count == 2)
        {
            _isRotating = false;
            var touch0 = touch.touches[0];
            var touch1 = touch.touches[1];

            Vector2 touch0PrevPos = touch0.position.ReadValue() - touch0.delta.ReadValue();
            Vector2 touch1PrevPos = touch1.position.ReadValue() - touch1.delta.ReadValue();

            float prevMagnitude = (touch0PrevPos - touch1PrevPos).magnitude;
            float currentMagnitude = (touch0.position.ReadValue() - touch1.position.ReadValue()).magnitude;

            float difference = currentMagnitude - prevMagnitude;
            ApplyZoom(difference * 0.1f);
        }
        else if (mouse != null)
        {
            float scrollValue = mouse.scroll.ReadValue().y;
            if (Mathf.Abs(scrollValue) > Mathf.Epsilon)
            {
                ApplyZoom(scrollValue * zoomSpeed * Time.deltaTime);
            }
        }
    }

    void ApplyZoom(float amount)
    {
        Vector3 toGlobe = transform.position - mainCamera.transform.position;
        float currentDistance = toGlobe.magnitude;
        float targetDistance = Mathf.Clamp(currentDistance - amount, minZoomDistance, maxZoomDistance);
        mainCamera.transform.position = transform.position - toGlobe.normalized * targetDistance;
    }

    public void ResetRotation()
    {
        if (!_isInitialized) return;
        _isRotating = false;
        lockedAxis = AxisLock.None;
        LeanTween.rotate(gameObject, _initialRotation.eulerAngles, resetDuration).setEase(resetEaseType);
        LeanTween.move(mainCamera.gameObject, _initialCameraPosition, resetDuration).setEase(resetEaseType);
    }

    public void MoveToTarget(Vector3 eulerRotation, float distance, float duration, LeanTweenType ease)
    {
        if (!_isInitialized) return;

        _isRotating = false;

        LeanTween.cancel(gameObject);
        if (mainCamera != null) LeanTween.cancel(mainCamera.gameObject);

        LeanTween.rotate(gameObject, eulerRotation, duration).setEase(ease);

        if (mainCamera != null)
        {
            Vector3 direction = (mainCamera.transform.position - transform.position).normalized;
            Vector3 targetPos = transform.position + direction * distance;
            LeanTween.move(mainCamera.gameObject, targetPos, duration).setEase(ease);
        }
    }
}