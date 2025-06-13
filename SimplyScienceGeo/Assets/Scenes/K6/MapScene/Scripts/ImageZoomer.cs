using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

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
    // Define what "reset" means for scale and position
    public Vector3 defaultViewScale = Vector3.one;
    public Vector2 defaultViewPosition = Vector2.zero;


    [Header("Input Actions")]
    public InputActionReference scrollActionReference;
    public InputActionReference zoomInActionReference;
    public InputActionReference zoomOutActionReference;

    [Header("Panning Settings")]
    public bool enablePanning = true;

    private Vector2 lastPointerPosition;
    // No longer relying on initialScale/initialPannedPosition from Awake for ResetView

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
            // Allow script to run, but constraints might be an issue.
        }
    }

    void OnEnable()
    {
        scrollActionReference?.action.Enable();
        zoomInActionReference?.action.Enable();
        zoomOutActionReference?.action.Enable();

        // When the map becomes active, ensure its constraints are applied to its current state.
        // If MapSelector calls ResetView immediately after this, ResetView will re-apply them.
        ApplyConstraints();
    }

    void OnDisable()
    {
        scrollActionReference?.action.Disable();
        zoomInActionReference?.action.Disable();
        zoomOutActionReference?.action.Disable();
    }

    void Update()
    {
        if (imageRectTransform == null) return;

        // --- Keep existing Desktop/Keyboard zoom logic ---
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


        // --- ADDED: Mobile Pinch-to-Zoom Logic ---
        if (Input.touchCount == 2)
        {
            // Get the two finger touches
            Touch touchZero = Input.GetTouch(0);
            Touch touchOne = Input.GetTouch(1);

            // Find the position of each touch in the previous frame
            Vector2 touchZeroPrevPos = touchZero.position - touchZero.deltaPosition;
            Vector2 touchOnePrevPos = touchOne.position - touchOne.deltaPosition;

            // Calculate the distance between the fingers in the current and previous frame
            float prevMagnitude = (touchZeroPrevPos - touchOnePrevPos).magnitude;
            float currentMagnitude = (touchZero.position - touchOne.position).magnitude;

            // Get the difference in distance and apply it to the zoom
            float difference = currentMagnitude - prevMagnitude;
            currentZoom += difference * zoomSpeed * 0.05f; // Added a sensitivity multiplier for touch
        }

        // --- Clamp and Apply Zoom ---
        currentZoom = Mathf.Clamp(currentZoom, minZoom, maxZoom);

        if (!Mathf.Approximately(currentZoom, previousZoom))
        {
            imageRectTransform.localScale = new Vector3(currentZoom, currentZoom, imageRectTransform.localScale.z);
            ApplyConstraints();
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (!enablePanning || imageRectTransform == null || viewportRectTransform == null) return;
        if (!CanPan()) return; // Only allow drag if pannable

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            viewportRectTransform, // Drag relative to viewport's space
            eventData.position,
            GetCanvasCamera(eventData),
            out lastPointerPosition);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!enablePanning || imageRectTransform == null || viewportRectTransform == null) return;
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

    public void OnEndDrag(PointerEventData eventData) { /* Nothing specific needed */ }

    void ApplyConstraints()
    {
        if (imageRectTransform == null || viewportRectTransform == null) return;

        imageRectTransform.anchoredPosition = GetConstrainedPosition(imageRectTransform.anchoredPosition);
    }

    Vector2 GetConstrainedPosition(Vector2 targetPosition)
    {
        if (imageRectTransform == null || viewportRectTransform == null) return targetPosition;

        float currentScale = imageRectTransform.localScale.x;
        Vector2 contentScaledSize = new Vector2(imageRectTransform.rect.width * currentScale, imageRectTransform.rect.height * currentScale);
        Vector2 viewportSize = viewportRectTransform.rect.size;

        // Assuming center pivots (0.5, 0.5) for both content and viewport
        float maxPanX = Mathf.Max(0, (contentScaledSize.x - viewportSize.x) / 2f);
        float maxPanY = Mathf.Max(0, (contentScaledSize.y - viewportSize.y) / 2f);

        return new Vector2(
            Mathf.Clamp(targetPosition.x, -maxPanX, maxPanX),
            Mathf.Clamp(targetPosition.y, -maxPanY, maxPanY)
        );
    }

    // Helper to check if panning should be allowed (content is larger than viewport)
    bool CanPan()
    {
        if (imageRectTransform == null || viewportRectTransform == null) return false;
        float currentScale = imageRectTransform.localScale.x;
        return (imageRectTransform.rect.width * currentScale > viewportRectTransform.rect.width + 0.01f) ||
               (imageRectTransform.rect.height * currentScale > viewportRectTransform.rect.height + 0.01f);
    }


    public void ResetZoom() // Renamed internally for clarity if used separately
    {
        if (imageRectTransform != null)
        {
            imageRectTransform.localScale = defaultViewScale;
            // ApplyConstraints will be called by ResetView or when zoom changes
        }
    }

    public void ResetPan() // Renamed internally
    {
        if (imageRectTransform != null)
        {
            imageRectTransform.anchoredPosition = defaultViewPosition;
            // ApplyConstraints will be called by ResetView
        }
    }

    public void ResetView()
    {
        if (imageRectTransform != null)
        {
            imageRectTransform.localScale = defaultViewScale;
            imageRectTransform.anchoredPosition = defaultViewPosition;
            ApplyConstraints(); // Crucial: Apply constraints AFTER setting to default state
        }
    }

    private Camera GetCanvasCamera(PointerEventData eventData = null)
    {
        if (eventData != null && eventData.pressEventCamera != null) return eventData.pressEventCamera;

        Canvas canvas = GetComponentInParent<Canvas>();
        if (canvas == null) return null;
        return canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;
    }
}