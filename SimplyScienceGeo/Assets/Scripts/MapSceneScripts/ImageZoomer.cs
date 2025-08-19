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
    public Vector3 defaultViewScale = Vector3.one;
    public Vector2 defaultViewPosition = Vector2.zero;

    // ADDED: New settings for the reset animation
    [Header("Reset Tween Settings")]
    public float resetTweenDuration = 0.4f;
    public LeanTweenType resetEaseType = LeanTweenType.easeOutCubic;


    [Header("Input Actions")]
    public InputActionReference scrollActionReference;
    public InputActionReference zoomInActionReference;
    public InputActionReference zoomOutActionReference;

    [Header("Panning Settings")]
    public bool enablePanning = true;

    private Vector2 lastPointerPosition;

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
        scrollActionReference?.action.Enable();
        zoomInActionReference?.action.Enable();
        zoomOutActionReference?.action.Enable();
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

        // --- ADDED: Prevent user input while reset tween is active ---
        if (LeanTween.isTweening(imageRectTransform.gameObject)) return;

        // ... (rest of Update method is the same)
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

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (!enablePanning || imageRectTransform == null || viewportRectTransform == null) return;
        // ADDED: Prevent drag while tweening
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
        // ADDED: Prevent drag while tweening
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

    // CHANGED: This method now tweens the position and scale.
    public void ResetView()
    {
        if (imageRectTransform == null) return;

        // Cancel any existing tweens on this object to prevent conflicts
        LeanTween.cancel(imageRectTransform.gameObject);

        // Tween the scale back to its default value
        LeanTween.scale(imageRectTransform, defaultViewScale, resetTweenDuration)
            .setEase(resetEaseType);

        // Tween the anchoredPosition back to its default
        // and apply constraints once the tween is complete.
        LeanTween.move(imageRectTransform, defaultViewPosition, resetTweenDuration)
            .setEase(resetEaseType)
            .setOnComplete(ApplyConstraints);
    }

    private Camera GetCanvasCamera(PointerEventData eventData = null)
    {
        if (eventData != null && eventData.pressEventCamera != null) return eventData.pressEventCamera;
        Canvas canvas = GetComponentInParent<Canvas>();
        if (canvas == null) return null;
        return canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;
    }
}