using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class GlobalInputManager : MonoBehaviour
{
    [Header("Manager Reference")]
    public InteractionManager interactionManager;

    private Camera mainCamera;
    private float lastClickTime;
    private const float DOUBLE_CLICK_THRESHOLD = 0.3f;

    void Awake()
    {
        mainCamera = Camera.main;
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        mainCamera = Camera.main;
    }

    void Update()
    {
        bool wasPressedThisFrame = (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame) ||
                                 (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasPressedThisFrame);

        if (wasPressedThisFrame)
        {
            // Prevent multi-touch gestures from interfering
            if (Touchscreen.current != null && Touchscreen.current.touches.Count > 1) return;

            if (Time.time - lastClickTime < DOUBLE_CLICK_THRESHOLD)
            {
                // Guard against interacting while another model is animating
                if (!ModelActivator.IsIdle) return;

                Vector2 pointerPosition = GetPointerPosition();
                HandleInteraction(pointerPosition);
                lastClickTime = 0; // Reset timer after a successful double-click
            }
            else
            {
                // Register the time of the first click
                lastClickTime = Time.time;
            }
        }
    }

    Vector2 GetPointerPosition()
    {
        if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.isPressed)
        {
            return Touchscreen.current.primaryTouch.position.ReadValue();
        }
        return Mouse.current.position.ReadValue();
    }

    // This version uses the original, working logic.
    void HandleInteraction(Vector2 pointerPosition)
    {
        // 1. First, try to interact with a UI element.
        if (TryUIInteraction(pointerPosition))
        {
            // If we hit a UI element, we stop here.
            return;
        }

        // 2. If no UI was hit, then try to interact with the 3D world.
        TryWorldInteraction(pointerPosition);
    }

    bool TryUIInteraction(Vector2 pointerPosition)
    {
        if (EventSystem.current == null) return false;

        PointerEventData pointerData = new PointerEventData(EventSystem.current)
        {
            position = pointerPosition
        };

        var raycastResults = new System.Collections.Generic.List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerData, raycastResults);

        foreach (var result in raycastResults)
        {
            var feature = result.gameObject.GetComponent<InteractableFeature>();
            if (feature != null && interactionManager != null)
            {
                interactionManager.SelectFeature(feature);
                return true; // Found a valid UI feature, so we are done.
            }
        }

        return false; // No valid UI feature was clicked.
    }

    void TryWorldInteraction(Vector2 pointerPosition)
    {
        if (mainCamera == null) return;

        Ray ray = mainCamera.ScreenPointToRay(pointerPosition);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            InteractableFeature feature = hit.collider.GetComponent<InteractableFeature>();
            if (feature != null && interactionManager != null)
            {
                interactionManager.SelectFeature(feature);
            }
        }
    }
}