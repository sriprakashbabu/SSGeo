using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class GlobalInputManager : MonoBehaviour
{
    [Header("Manager Reference")]
    public InteractionManager interactionManager;

    private SSGeo _input; // 🆕 Reference to your new Input Action Asset
    private Camera mainCamera;
    private float lastClickTime;
    private const float DOUBLE_CLICK_THRESHOLD = 0.3f;

    void Awake()
    {
        mainCamera = Camera.main;
        _input = new SSGeo(); // 🆕 Instantiate the new input class
    }

    void OnEnable()
    {
        _input.Enable();
        SceneManager.sceneLoaded += OnSceneLoaded;
        // 🆕 Subscribe to the 'Click' action's 'performed' event
        _input.Gameplay.Click.performed += OnClickPerformed;
    }

    void OnDisable()
    {
        // 🆕 Unsubscribe from the event to prevent memory leaks
        _input.Gameplay.Click.performed -= OnClickPerformed;
        SceneManager.sceneLoaded -= OnSceneLoaded;
        _input.Disable();
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        mainCamera = Camera.main;
    }

    // 🆕 This method is now called automatically by the Input System when 'Click' is performed
    private void OnClickPerformed(InputAction.CallbackContext context)
    {
        // Guard against multi-touch gestures interfering
        if (Touchscreen.current != null && Touchscreen.current.touches.Count > 1) return;

        if (Time.time - lastClickTime < DOUBLE_CLICK_THRESHOLD)
        {
            if (!ModelActivator.IsIdle) return;

            // 🆕 Get pointer position directly from the 'Point' action
            Vector2 pointerPosition = _input.Gameplay.Point.ReadValue<Vector2>();
            HandleInteraction(pointerPosition);
            lastClickTime = 0; // Reset timer after a successful double-click
        }
        else
        {
            lastClickTime = Time.time;
        }
    }

    // This version now takes the pointer position as a parameter
    void HandleInteraction(Vector2 pointerPosition)
    {
        if (TryUIInteraction(pointerPosition)) return;
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
                return true;
            }
        }
        return false;
    }

    void TryWorldInteraction(Vector2 pointerPosition)
    {
        if (mainCamera == null)
        {
            Debug.LogWarning("GlobalInputManager: No main camera found.");
            return;
        }

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