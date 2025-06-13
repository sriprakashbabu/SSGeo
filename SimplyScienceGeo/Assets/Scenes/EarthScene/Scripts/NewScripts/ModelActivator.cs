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
    // --- REVERTED: The Y-Offset is no longer needed ---
    [SerializeField] private float transitionDuration = 0.5f;
    [SerializeField] private LeanTweenType easeType = LeanTweenType.easeOutExpo;
    [SerializeField] private Vector3 customTargetScale = Vector3.zero;

    [Header("Manager References")]
    [SerializeField] private GlobalInputManager globalInputManager;

    [Header("Components to Disable on Detail")]
    [SerializeField] private GlobeRotator globeRotator;

    [Header("UI & Environment")]
    [SerializeField] private GameObject[] uiElementsToHide;
    [SerializeField] private Material detailSkybox;

    private static ModelActivator _currentActiveModel;
    private static List<ModelActivator> _allActivators = new List<ModelActivator>();

    // --- REVERTED: We need original scale, not position ---
    private Vector3 _rootOriginalScale;
    private Vector3 _finalTargetScale;
    private Collider _collider;

    private static Material _originalSceneSkybox;
    private static bool _originalSkyboxCaptured = false;

    void Awake()
    {
        _collider = GetComponent<Collider>();
        if (detailedModel == null || rootModelToScaleDown == null || backButton == null) { enabled = false; return; }
        if (globalInputManager == null) globalInputManager = FindObjectOfType<GlobalInputManager>();

        // --- REVERTED: Storing the original scale of the root model ---
        _rootOriginalScale = rootModelToScaleDown.transform.localScale;
        _finalTargetScale = (customTargetScale != Vector3.zero) ? customTargetScale : detailedModel.transform.localScale;

        detailedModel.transform.localScale = Vector3.zero;
        detailedModel.SetActive(false);
        backButton.gameObject.SetActive(false);
        backButton.onClick.AddListener(Deactivate);
    }

    void OnEnable() { if (!_allActivators.Contains(this)) _allActivators.Add(this); }
    void OnDisable() { _allActivators.Remove(this); }

    public void Activate()
    {
        if (_state != ModelState.Inactive) return;

        _state = ModelState.Activating;
        _currentActiveModel = this;

        if (globalInputManager != null) globalInputManager.enabled = false;
        ToggleAllActivatorColliders(false);
        ToggleOtherComponents(false);
        ToggleUI(false);
        UpdateSkybox(true);
        backButton.gameObject.SetActive(true);

        // --- REVERTED: Scale the globe down to zero ---
        LeanTween.scale(rootModelToScaleDown, Vector3.zero, transitionDuration).setEase(easeType);

        detailedModel.SetActive(true);
        LeanTween.scale(detailedModel, _finalTargetScale, transitionDuration)
            .setEase(easeType)
            .setOnComplete(() => {
                _state = ModelState.Active;
            });
    }

    public void Deactivate()
    {
        if (_state != ModelState.Active) return;

        _state = ModelState.Deactivating;
        backButton.interactable = false;
        UpdateSkybox(false);

        LeanTween.scale(detailedModel, Vector3.zero, transitionDuration)
            .setEase(easeType)
            .setOnComplete(() => detailedModel.SetActive(false));

        // --- REVERTED: Scale the globe back to its original size ---
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

        if (globalInputManager != null) globalInputManager.enabled = true;
        ToggleAllActivatorColliders(true);

        _currentActiveModel = null;
        _state = ModelState.Inactive;
    }

    private static void ToggleAllActivatorColliders(bool enable) { foreach (var activator in _allActivators) { if (activator != _currentActiveModel && activator._collider != null) { activator._collider.enabled = enable; } } }
    private void ToggleUI(bool show) { foreach (var element in uiElementsToHide) { if (element != null) element.SetActive(show); } }
    private void ToggleOtherComponents(bool enable) { if (globeRotator != null) globeRotator.enabled = enable; }
    private void UpdateSkybox(bool isDetailView) { if (!_originalSceneSkybox) { _originalSceneSkybox = RenderSettings.skybox; _originalSkyboxCaptured = true; } RenderSettings.skybox = (isDetailView && detailSkybox != null) ? detailSkybox : _originalSceneSkybox; }
    void OnDestroy() { LeanTween.cancel(gameObject, true); if (_currentActiveModel == this) { _currentActiveModel = null; _state = ModelState.Inactive; } }
}