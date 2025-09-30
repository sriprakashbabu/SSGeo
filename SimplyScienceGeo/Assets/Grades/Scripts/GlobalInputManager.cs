using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class GlobalInputManager : MonoBehaviour
{
    [Header("Manager Reference")]
    public InteractionManager interactionManager;

    [Header("Click Settings")]
    [SerializeField] private bool requireDoubleClick = true;
    [SerializeField] private float doubleClickThreshold = 0.30f;
    [SerializeField] private float doubleClickMaxMovePx = 20f;

    private float lastClickTime = -999f;
    private Vector2 lastClickPos = Vector2.zero;

    private SSGeo input;     // your Input Actions
    private Camera mainCamera;

    void Awake() => mainCamera = Camera.main;

    void OnEnable()
    {
        input = new SSGeo();
        input.Gameplay.Enable();
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        input?.Dispose();
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode) => mainCamera = Camera.main;

    void Update()
    {
        if (!input.Gameplay.Click.triggered) return;

        Vector2 curPos = input.Gameplay.Point.ReadValue<Vector2>();

        // 1) UI raycast: if we hit an InteractableFeature on UI, handle it here.
        bool overAnyUI = IsPointerOverUI(curPos, out InteractableFeature uiFeature);

        if (uiFeature != null)
        {
            if (PassesDoubleClickGate(curPos))
            {
                interactionManager?.SelectFeature(uiFeature);
                ResetDoubleClickState();
            }
            // Whether or not we passed the gate, consume here so world doesn’t also fire
            return;
        }

        // If pointer is over some OTHER UI (buttons, etc), don't process world.
        if (overAnyUI) return;

        // 2) World raycast (3D/2D colliders rendered by Camera)
        if (!requireDoubleClick)
        {
            TryWorldInteraction(curPos);
            return;
        }

        if (PassesDoubleClickGate(curPos))
        {
            TryWorldInteraction(curPos);
            ResetDoubleClickState();
        }
        // else: first click armed; do nothing on world
    }

    // --- Helpers ---

    bool IsPointerOverUI(Vector2 screenPos, out InteractableFeature uiFeature)
    {
        uiFeature = null;
        if (EventSystem.current == null) return false;

        var pointerData = new PointerEventData(EventSystem.current) { position = screenPos };
        var results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerData, results);

        foreach (var r in results)
        {
            var f = r.gameObject.GetComponent<InteractableFeature>();
            if (f != null)
            {
                uiFeature = f;
                break;
            }
        }
        return results.Count > 0;
    }

    bool PassesDoubleClickGate(Vector2 curPos)
    {
        if (!requireDoubleClick) return true;

        float t = Time.time;
        bool withinTime = (t - lastClickTime) <= doubleClickThreshold;
        bool withinMove = lastClickPos == Vector2.zero ||
                          Vector2.Distance(curPos, lastClickPos) <= doubleClickMaxMovePx;

        if (withinTime && withinMove) return true;

        // First click (or too slow / moved too far) → arm
        lastClickTime = t;
        lastClickPos = curPos;
        return false;
    }

    void ResetDoubleClickState()
    {
        lastClickTime = -999f;
        lastClickPos = Vector2.zero;
    }

    void TryWorldInteraction(Vector2 screenPos)
    {
        if (mainCamera == null) return;

        Ray ray = mainCamera.ScreenPointToRay(screenPos);

        // 3D colliders
        if (Physics.Raycast(ray, out var hit3D))
        {
            var feature3D = hit3D.collider.GetComponent<InteractableFeature>();
            if (feature3D != null && interactionManager != null)
            {
                interactionManager.SelectFeature(feature3D);
                return;
            }
        }

        // 2D colliders (optional)
        RaycastHit2D hit2D = Physics2D.GetRayIntersection(ray);
        if (hit2D.collider != null)
        {
            var feature2D = hit2D.collider.GetComponent<InteractableFeature>();
            if (feature2D != null && interactionManager != null)
            {
                interactionManager.SelectFeature(feature2D);
            }
        }
    }
}
