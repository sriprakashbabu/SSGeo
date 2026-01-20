using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ImageZoomer : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("Target Image")]
    public RectTransform imageRectTransform;

    [Header("Viewport")]
    public RectTransform viewportRectTransform;

    [Header("Zoom Settings")]
    public float zoomSpeed = 0.1f;
    public float keyboardZoomSensitivity = 1.0f;
    public float minZoom = 0.5f;
    public float maxZoom = 3.0f;
    public Vector3 defaultViewScale = Vector3.one;
    public Vector2 defaultViewPosition = Vector2.zero;

    [Header("Reset Tween Settings")]
    public float resetTweenDuration = 0.4f;
    public LeanTweenType resetEaseType = LeanTweenType.easeOutCubic;

    [Header("Input Actions")]
    public InputActionReference scrollActionReference;
    public InputActionReference zoomInActionReference;
    public InputActionReference zoomOutActionReference;

    [Header("Panning Settings")]
    public bool enablePanning = true;

    // ---------- PRESETS ----------
    [System.Serializable]
    public struct ImageViewPreset
    {
        [Tooltip("Index of this preset in the array (auto-filled).")]
        public int index;

        [Tooltip("Friendly name so you remember what this view is for.")]
        public string name;

        [Tooltip("Target zoom level.")]
        public float zoom;

        [Tooltip("Target pan X.")]
        public float panX;

        [Tooltip("Target pan Y.")]
        public float panY;
    }

    [Header("Presets")]
    public ImageViewPreset[] presets;

    [Header("Preset UI")]
    [Tooltip("Optional: ToggleGroup that these preset toggles belong to (set Allow Switch Off on it).")]
    public ToggleGroup presetToggleGroup;

    [Tooltip("Toggles that correspond to each preset (index must match).")]
    public Toggle[] presetToggles;

    [Tooltip("If true, when no toggle is selected it will reset to default view.")]
    public bool resetWhenNoPresetSelected = true;
    // -----------------------------

    private Vector2 lastPointerPosition;
    private int currentPresetIndex = -1;   // -1 means no preset active

    void Awake()
    {
        if (imageRectTransform == null)
            imageRectTransform = GetComponent<RectTransform>();

        if (imageRectTransform == null)
        {
            Debug.LogError("ImageZoomer: No RectTransform for imageRectTransform.", this);
            enabled = false;
            return;
        }

        if (viewportRectTransform == null && transform.parent != null)
            viewportRectTransform = transform.parent.GetComponent<RectTransform>();

        if (viewportRectTransform == null)
        {
            Debug.LogError("ImageZoomer: No RectTransform for viewportRectTransform. Constraints may fail.", this);
        }
    }

    void OnEnable()
    {
        // 1. Enable inputs
        scrollActionReference?.action.Enable();
        zoomInActionReference?.action.Enable();
        zoomOutActionReference?.action.Enable();

        // 2. STOP any running tweens to unlock Dragging immediately
        if (imageRectTransform != null)
            LeanTween.cancel(imageRectTransform.gameObject);

        ApplyConstraints();
    }

    void OnDisable()
    {
        // --- CRITICAL FIX START ---

        // DO NOT disable the actions here. 
        // If you disable them, you kill the input for the OTHER ImageZoomer too!
        // Since this script's Update() loop stops running when disabled, 
        // polling stops automatically.

        // scrollActionReference?.action.Disable();  <-- REMOVE OR COMMENT OUT
        // zoomInActionReference?.action.Disable();  <-- REMOVE OR COMMENT OUT
        // zoomOutActionReference?.action.Disable(); <-- REMOVE OR COMMENT OUT

        // Stop tweens so the "isTweening" flag doesn't get stuck true
        if (imageRectTransform != null)
            LeanTween.cancel(imageRectTransform.gameObject);

        // --- CRITICAL FIX END ---
    }

    // Auto-sync preset indices in inspector
    void OnValidate()
    {
        if (presets != null)
        {
            for (int i = 0; i < presets.Length; i++)
            {
                presets[i].index = i;
            }
        }
    }

    void Update()
    {
        if (imageRectTransform == null) return;

        // Handle zoom input (mouse, keyboard, pinch)
        if (!LeanTween.isTweening(imageRectTransform.gameObject))
        {
            float previousZoom = imageRectTransform.localScale.x;
            float currentZoom = previousZoom;

            if (scrollActionReference != null && scrollActionReference.action.enabled)
            {
                float scrollInput = scrollActionReference.action.ReadValue<Vector2>().y;
                if (scrollInput != 0)
                {
                    if (Mathf.Abs(scrollInput) > 1.0f) scrollInput = Mathf.Sign(scrollInput);
                    currentZoom += scrollInput * zoomSpeed;
                }
            }

            if (zoomInActionReference != null && zoomInActionReference.action.enabled && zoomInActionReference.action.IsPressed())
            {
                currentZoom += keyboardZoomSensitivity * zoomSpeed * Time.deltaTime;
            }

            if (zoomOutActionReference != null && zoomOutActionReference.action.enabled && zoomOutActionReference.action.IsPressed())
            {
                currentZoom -= keyboardZoomSensitivity * zoomSpeed * Time.deltaTime;
            }

            if (Input.touchCount == 2)
            {
                Touch touchZero = Input.GetTouch(0);
                Touch touchOne = Input.GetTouch(1);
                Vector2 touchZeroPrevPos = touchZero.position - touchZero.deltaPosition;
                Vector2 touchOnePrevPos = touchOne.position - touchOne.deltaPosition;
                float prevMagnitude = (touchZeroPrevPos - touchOnePrevPos).magnitude;
                float currentMagnitude = (touchZero.position - touchOne.position).magnitude;
                float difference = currentMagnitude - prevMagnitude;
                currentZoom += difference * zoomSpeed * 0.05f;
            }

            currentZoom = Mathf.Clamp(currentZoom, minZoom, maxZoom);

            if (!Mathf.Approximately(currentZoom, previousZoom))
            {
                imageRectTransform.localScale = new Vector3(currentZoom, currentZoom, imageRectTransform.localScale.z);
                ApplyConstraints();
            }
        }

        // 🔍 Handle preset selection via toggles
        UpdatePresetSelectionFromToggles();
    }

    // Check toggles and apply / reset as needed
    private void UpdatePresetSelectionFromToggles()
    {
        if (presetToggles == null || presetToggles.Length == 0)
            return;

        int newIndex = -1;

        // Find first toggle that is on
        for (int i = 0; i < presetToggles.Length; i++)
        {
            if (presetToggles[i] != null && presetToggles[i].isOn)
            {
                newIndex = i;
                break;
            }
        }

        // If nothing changed, do nothing
        if (newIndex == currentPresetIndex)
            return;

        // Selection changed
        currentPresetIndex = newIndex;

        if (currentPresetIndex >= 0)
        {
            // A preset is now selected → go to that view
            ApplyPreset(currentPresetIndex);
        }
        else
        {
            // No preset selected → reset view (if enabled)
            if (resetWhenNoPresetSelected)
            {
                ResetView();
            }
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (!enablePanning || imageRectTransform == null || viewportRectTransform == null) return;
        if (LeanTween.isTweening(imageRectTransform.gameObject)) return;
        if (!CanPan()) return;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            viewportRectTransform,
            eventData.position,
            GetCanvasCamera(eventData),
            out lastPointerPosition);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!enablePanning || imageRectTransform == null || viewportRectTransform == null) return;
        if (LeanTween.isTweening(imageRectTransform.gameObject)) return;
        if (!CanPan()) return;

        Vector2 currentPointerPosition;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            viewportRectTransform,
            eventData.position,
            GetCanvasCamera(eventData),
            out currentPointerPosition))
        {
            Vector2 delta = currentPointerPosition - lastPointerPosition;
            imageRectTransform.anchoredPosition += delta;
            lastPointerPosition = currentPointerPosition;
            ApplyConstraints();
        }
    }

    public void OnEndDrag(PointerEventData eventData) { }

    // Apply current-scale panning constraints
    void ApplyConstraints()
    {
        if (imageRectTransform == null || viewportRectTransform == null) return;
        imageRectTransform.anchoredPosition = GetPanningConstrainedPosition(imageRectTransform.anchoredPosition);
    }

    Vector2 GetPanningConstrainedPosition(Vector2 targetPosition)
    {
        if (imageRectTransform == null || viewportRectTransform == null) return targetPosition;

        float currentScale = imageRectTransform.localScale.x;
        return GetPanningConstrainedPositionForZoom(targetPosition, currentScale);
    }

    // Constraints for an arbitrary zoom (used by presets)
    Vector2 GetPanningConstrainedPositionForZoom(Vector2 targetPosition, float zoomScale)
    {
        if (imageRectTransform == null || viewportRectTransform == null) return targetPosition;

        Vector2 contentScaledSize = new Vector2(
            imageRectTransform.rect.width * zoomScale,
            imageRectTransform.rect.height * zoomScale
        );
        Vector2 viewportSize = viewportRectTransform.rect.size;

        float maxPanX, maxPanY;

        if (contentScaledSize.x < viewportSize.x)
            maxPanX = 0;
        else
            maxPanX = contentScaledSize.x / 2f;

        if (contentScaledSize.y < viewportSize.y)
            maxPanY = 0;
        else
            maxPanY = contentScaledSize.y / 2f;

        return new Vector2(
            Mathf.Clamp(targetPosition.x, -maxPanX, maxPanX),
            Mathf.Clamp(targetPosition.y, -maxPanY, maxPanY)
        );
    }

    // Reset constraints (using default scale)
    Vector2 GetResetConstrainedPosition(Vector2 targetPosition)
    {
        if (imageRectTransform == null || viewportRectTransform == null) return targetPosition;

        float currentScale = defaultViewScale.x;
        Vector2 contentScaledSize = new Vector2(
            imageRectTransform.rect.width * currentScale,
            imageRectTransform.rect.height * currentScale
        );
        Vector2 viewportSize = viewportRectTransform.rect.size;

        float maxPanX = Mathf.Max(0, (contentScaledSize.x - viewportSize.x) / 2f);
        float maxPanY = Mathf.Max(0, (contentScaledSize.y - viewportSize.y) / 2f);

        return new Vector2(
            Mathf.Clamp(targetPosition.x, -maxPanX, maxPanX),
            Mathf.Clamp(targetPosition.y, -maxPanY, maxPanY)
        );
    }

    bool CanPan()
    {
        if (imageRectTransform == null || viewportRectTransform == null) return false;
        float currentScale = imageRectTransform.localScale.x;
        return (imageRectTransform.rect.width * currentScale > viewportRectTransform.rect.width + 0.01f) ||
               (imageRectTransform.rect.height * currentScale > viewportRectTransform.rect.height + 0.01f);
    }

    public void ResetZoom()
    {
        if (imageRectTransform != null)
        {
            imageRectTransform.localScale = defaultViewScale;
        }
    }

    public void ResetPan()
    {
        if (imageRectTransform != null)
        {
            imageRectTransform.anchoredPosition = defaultViewPosition;
        }
    }

    public void ResetView()
    {
        if (imageRectTransform == null) return;

        LeanTween.cancel(imageRectTransform.gameObject);

        LeanTween.scale(imageRectTransform, defaultViewScale, resetTweenDuration)
            .setEase(resetEaseType);

        LeanTween.move(imageRectTransform, defaultViewPosition, resetTweenDuration)
            .setEase(resetEaseType)
            .setOnComplete(OnResetViewComplete);
    }

    void OnResetViewComplete()
    {
        imageRectTransform.anchoredPosition = GetResetConstrainedPosition(defaultViewPosition);
    }

    // Smoothly go to a preset (by index)
    public void ApplyPreset(int index)
    {
        if (imageRectTransform == null) return;
        if (presets == null || presets.Length == 0)
        {
            Debug.LogWarning("ImageZoomer: No presets defined.");
            return;
        }
        if (index < 0 || index >= presets.Length)
        {
            Debug.LogWarning($"ImageZoomer: Preset index {index} is out of range.");
            return;
        }

        ImageViewPreset p = presets[index];

        float targetZoom = Mathf.Clamp(p.zoom, minZoom, maxZoom);
        Vector3 endScale = new Vector3(targetZoom, targetZoom, imageRectTransform.localScale.z);

        Vector2 rawTargetPos = new Vector2(p.panX, p.panY);
        Vector2 endPos = GetPanningConstrainedPositionForZoom(rawTargetPos, targetZoom);

        LeanTween.cancel(imageRectTransform.gameObject);

        LeanTween.scale(imageRectTransform, endScale, resetTweenDuration)
            .setEase(resetEaseType);

        LeanTween.move(imageRectTransform, endPos, resetTweenDuration)
            .setEase(resetEaseType);
    }

    private Camera GetCanvasCamera(PointerEventData eventData = null)
    {
        if (eventData != null && eventData.pressEventCamera != null) return eventData.pressEventCamera;
        Canvas canvas = GetComponentInParent<Canvas>();
        if (canvas == null) return null;
        return canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;
    }
}
