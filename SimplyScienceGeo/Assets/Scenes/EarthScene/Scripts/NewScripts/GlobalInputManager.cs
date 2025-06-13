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
        if (EventSystem.current.IsPointerOverGameObject()) return;

        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            if (Time.time - lastClickTime < DOUBLE_CLICK_THRESHOLD)
            {
                // This gatekeeper check is the key to the whole system's stability.
                if (!ModelActivator.IsIdle) return;

                HandleInteraction();
                lastClickTime = 0;
            }
            else
            {
                lastClickTime = Time.time;
            }
        }
    }

    void HandleInteraction()
    {
        if (mainCamera == null)
        {
            Debug.LogWarning("GlobalInputManager: No main camera found.");
            return;
        }

        Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
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