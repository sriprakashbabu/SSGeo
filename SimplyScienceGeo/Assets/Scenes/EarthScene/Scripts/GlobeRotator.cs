using UnityEngine;
using UnityEngine.InputSystem;

public class GlobeRotator : MonoBehaviour
{
    [Tooltip("How sensitive the rotation is to mouse movement.")]
    [SerializeField] private float rotationSpeed = 20f;

    [Tooltip("Optional: Assign a specific camera if not using Camera.main.")]
    [SerializeField] private Camera mainCamera;

    [Tooltip("How long the reset animation should take in seconds.")]
    [SerializeField] private float resetDuration = 0.5f;

    [Tooltip("The LeanTween ease type for the reset animation.")]
    [SerializeField] private LeanTweenType resetEaseType = LeanTweenType.easeOutExpo;


    private Vector2 _previousMousePosition;
    private bool _isRotating = false;
    private Quaternion _initialRotation;

    // --- THIS IS THE FIX ---
    /// <summary>
    /// This function is called every time the component is enabled.
    /// We reset the rotation state here to fix the bug.
    /// </summary>
    void OnEnable()
    {
        _isRotating = false;
    }

    private void EnsureCamera()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
            if (mainCamera == null)
            {
                Debug.LogError("GlobeRotator: Main camera not found. Please assign a camera in the Inspector or ensure a camera is tagged 'MainCamera'.");
                this.enabled = false;
            }
        }
    }

    void Start()
    {
        EnsureCamera();
        _initialRotation = transform.rotation;
    }

    void Update()
    {
        if (mainCamera == null) return;

        Mouse currentMouse = Mouse.current;
        if (currentMouse == null) return;

        if (currentMouse.leftButton.wasPressedThisFrame)
        {
            LeanTween.cancel(gameObject);
            _isRotating = true;
            _previousMousePosition = currentMouse.position.ReadValue();
        }

        if (currentMouse.leftButton.wasReleasedThisFrame)
        {
            _isRotating = false;
        }

        if (_isRotating)
        {
            Vector2 currentMousePosition = currentMouse.position.ReadValue();
            Vector2 deltaMousePosition = currentMousePosition - _previousMousePosition;

            transform.Rotate(Vector3.up, -deltaMousePosition.x * rotationSpeed * Time.deltaTime, Space.World);
            if (mainCamera != null)
            {
                transform.Rotate(mainCamera.transform.right, deltaMousePosition.y * rotationSpeed * Time.deltaTime, Space.World);
            }
            _previousMousePosition = currentMousePosition;
        }
    }

    public void ResetRotation()
    {
        if (_isRotating)
        {
            _isRotating = false;
        }

        LeanTween.rotate(gameObject, _initialRotation.eulerAngles, resetDuration)
            .setEase(resetEaseType);
    }
}