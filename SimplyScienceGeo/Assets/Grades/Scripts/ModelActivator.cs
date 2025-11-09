using UnityEngine;
using UnityEngine.UI;
using DentedPixel;
using System.Collections.Generic;

public class ModelActivator : MonoBehaviour
{
    private enum ModelState { Inactive, Activating, Active, Deactivating }
    private static ModelState _state = ModelState.Inactive;

    public static bool IsIdle => _state == ModelState.Inactive;
    public static bool IsFullyActive => _state == ModelState.Active;

    [Header("Core References")]
    [SerializeField] private GameObject detailedModel;
    [SerializeField] private GameObject rootModelToScaleDown;
    [SerializeField] private Button backButton;

    [Header("Animation Settings")]
    [SerializeField] private float transitionDuration = 0.5f;
    [SerializeField] private LeanTweenType easeType = LeanTweenType.easeOutExpo;
    [SerializeField] private Vector3 customTargetScale = Vector3.zero;

    [Header("Camera Reset")]
    [Tooltip("The default rotation the camera should reset to.")]
    [SerializeField] private Vector3 cameraDefaultRotation = Vector3.zero;
    [Tooltip("The default zoom (distance) the camera should reset to.")]
    [SerializeField] private float cameraDefaultZoom = 25f;

    [Header("Manager References")]
    [SerializeField] private GlobalInputManager globalInputManager;

    [Header("Components to Disable on Detail")]
    [SerializeField] private GlobeRotator globeRotator;

    [Header("UI & Environment")]
    [SerializeField] private GameObject[] uiElementsToHide;
    [SerializeField] private GameObject[] objectsToEnableOnActive; // <-- New field
    [SerializeField] private Material detailSkybox;

    [Header("Image Display")]
    [Tooltip("Image component on UI canvas to show model-specific sprite.")]
    [SerializeField] private Image canvasImageTarget;

    [Tooltip("Sprite to display when this model is activated.")]
    [SerializeField] private Sprite displaySprite;

    private static ModelActivator _currentActiveModel;
    private static List<ModelActivator> _allActivators = new List<ModelActivator>();

    private Vector3 _rootOriginalScale;
    private Vector3 _finalTargetScale;
    private Collider _collider;

    private static Material _originalSceneSkybox;
    private static bool _originalSkyboxCaptured = false;

    void Awake()
    {
        _collider = GetComponent<Collider>();
        if (detailedModel == null || rootModelToScaleDown == null || backButton == null)
        {
            enabled = false;
            return;
        }

        if (globalInputManager == null)
            globalInputManager = FindObjectOfType<GlobalInputManager>();

        _rootOriginalScale = rootModelToScaleDown.transform.localScale;
        _finalTargetScale = (customTargetScale != Vector3.zero) ? customTargetScale : detailedModel.transform.localScale;

        detailedModel.transform.localScale = Vector3.zero;
        detailedModel.SetActive(false);
        backButton.gameObject.SetActive(false);
    }

    void OnEnable() => _allActivators.Add(this);
    void OnDisable() => _allActivators.Remove(this);

    public void Activate()
    {
        if (_state != ModelState.Inactive) return;

        _state = ModelState.Activating;
        _currentActiveModel = this;

        if (globalInputManager != null) globalInputManager.enabled = false;
        ToggleAllActivatorColliders(false);

        // Tell the camera to move back to its default position
        if (globeRotator != null)
        {
            globeRotator.MoveToTarget(
                cameraDefaultRotation,
                cameraDefaultZoom,
                transitionDuration,
                easeType
            );
        }

        ToggleOtherComponents(false);
        ToggleUI(false);

        // Activate new objects
        foreach (var obj in objectsToEnableOnActive)
        {
            if (obj != null) obj.SetActive(true);
        }

        UpdateSkybox(true);
        backButton.gameObject.SetActive(true);
        backButton.onClick.AddListener(Deactivate);

        // Show associated image
        if (canvasImageTarget != null && displaySprite != null)
        {
            canvasImageTarget.sprite = displaySprite;
            canvasImageTarget.gameObject.SetActive(true);
        }

        LeanTween.scale(rootModelToScaleDown, Vector3.zero, transitionDuration).setEase(easeType);
        detailedModel.SetActive(true);
        LeanTween.scale(detailedModel, _finalTargetScale, transitionDuration)
            .setEase(easeType)
            .setOnComplete(() => { _state = ModelState.Active; });
    }

    public void Deactivate()
    {
        if (_state != ModelState.Active || _currentActiveModel != this) return;

        _state = ModelState.Deactivating;
        backButton.interactable = false;
        UpdateSkybox(false);

        // Deactivate new objects
        foreach (var obj in objectsToEnableOnActive)
        {
            if (obj != null) obj.SetActive(false);
        }

        if (canvasImageTarget != null)
            canvasImageTarget.gameObject.SetActive(false);

        LeanTween.scale(detailedModel, Vector3.zero, transitionDuration)
            .setEase(easeType)
            .setOnComplete(() => detailedModel.SetActive(false));

        LeanTween.scale(rootModelToScaleDown, _rootOriginalScale, transitionDuration)
            .setEase(easeType)
            .setOnComplete(OnDeactivationComplete);
    }

    private void OnDeactivationComplete()
    {
        ToggleOtherComponents(true);
        ToggleUI(true);
        backButton.gameObject.SetActive(false);
        backButton.interactable = true;

        backButton.onClick.RemoveListener(Deactivate);

        if (globalInputManager != null) globalInputManager.enabled = true;
        ToggleAllActivatorColliders(true);

        _currentActiveModel = null;
        _state = ModelState.Inactive;
    }

    private static void ToggleAllActivatorColliders(bool enable)
    {
        foreach (var activator in _allActivators)
        {
            if (activator != _currentActiveModel && activator._collider != null)
                activator._collider.enabled = enable;
        }
    }

    private void ToggleUI(bool show)
    {
        foreach (var element in uiElementsToHide)
        {
            if (element != null) element.SetActive(show);
        }
    }

    private void ToggleOtherComponents(bool enable)
    {
        if (globeRotator != null) globeRotator.enabled = enable;
    }

    private void UpdateSkybox(bool isDetailView)
    {
        if (!_originalSkyboxCaptured)
        {
            _originalSceneSkybox = RenderSettings.skybox;
            _originalSkyboxCaptured = true;
        }

        RenderSettings.skybox = (isDetailView && detailSkybox != null) ? detailSkybox : _originalSceneSkybox;
    }

    void OnDestroy()
    {
        LeanTween.cancel(gameObject, true);
        if (_currentActiveModel == this)
        {
            if (backButton != null) backButton.onClick.RemoveListener(Deactivate);
            _currentActiveModel = null;
            _state = ModelState.Inactive;
        }
    }
}