using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class GlobalInputManager : MonoBehaviour
{
    [Header("Manager Reference")]
    public InteractionManager interactionManager;

    [Header("Click Settings")]
    // Fields (add/replace these)
    [SerializeField] private bool requireDoubleClick = true;   // turn it on
    [SerializeField] private float doubleClickThreshold = 0.30f;
    [SerializeField] private float doubleClickMaxMovePx = 20f;

    private float lastClickTime = -999f;   // <-- sentinel, not 0
    private Vector2 lastClickPos = Vector2.zero;


    private SSGeo input;              // ← uses your Input Actions
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
        if (input.Gameplay.Click.triggered)
        {
            var curPos = input.Gameplay.Point.ReadValue<Vector2>();

            // UI stays single-click; return if a UI element handled it
            if (TryUIInteraction(curPos)) return;

            if (!requireDoubleClick)
            {
                // Single-click path (if you ever want it)
                TryWorldInteraction(curPos);
                return;
            }

            float t = Time.time;
            bool withinTime = (t - lastClickTime) <= doubleClickThreshold;
            bool withinMove = (lastClickPos == Vector2.zero) ||
                              (Vector2.Distance(curPos, lastClickPos) <= doubleClickMaxMovePx);

            if (withinTime && withinMove)
            {
                // Double-click/tap detected → select feature
                TryWorldInteraction(curPos);
                lastClickTime = -999f;
                lastClickPos = Vector2.zero;
            }
            else
            {
                // First click: record and wait for a second click
                lastClickTime = t;
                lastClickPos = curPos;
            }
        }
    }

    void HandleInteraction(Vector2 screenPos)
    {
        // 1) Try UI first
        if (TryUIInteraction(screenPos)) return;

        // 2) Then try world
        TryWorldInteraction(screenPos);
    }

    bool TryUIInteraction(Vector2 screenPos)
    {
        if (EventSystem.current == null) return false;

        var pointerData = new PointerEventData(EventSystem.current) { position = screenPos };
        var results = new System.Collections.Generic.List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerData, results);

        foreach (var r in results)
        {
            var feature = r.gameObject.GetComponent<InteractableFeature>();
            if (feature != null && interactionManager != null)
            {
                interactionManager.SelectFeature(feature);
                return true;
            }
        }
        return false;
    }

    void TryWorldInteraction(Vector2 screenPos)
    {
        if (mainCamera == null) return;

        Ray ray = mainCamera.ScreenPointToRay(screenPos);
        if (Physics.Raycast(ray, out var hit))
        {
            var feature = hit.collider.GetComponent<InteractableFeature>();
            if (feature != null && interactionManager != null)
            {
                interactionManager.SelectFeature(feature);
            }
        }
    }
}
