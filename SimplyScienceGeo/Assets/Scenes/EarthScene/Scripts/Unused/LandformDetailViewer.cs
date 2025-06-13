//using UnityEngine;
//using UnityEngine.UI; // For Button
//using System.Collections;

//public class LandformDetailViewer : MonoBehaviour
//{
//    [Header("Scene References")]
//    [Tooltip("Assign an empty GameObject where detailed models will be instantiated and positioned.")]
//    public Transform modelSpawnPoint;
//    [Tooltip("Assign your UI Back Button. It will be activated when a model is shown.")]
//    public Button backButton;
//    [Tooltip("Assign your main Globe GameObject that needs to be scaled.")]
//    public GameObject globeGameObject;
//    [Tooltip("Assign your GlobeInfoUIManager to display text.")]
//    public GlobeInfoUIManager globeInfoUIManager;

//    [Header("Animation Settings")]
//    [Tooltip("Scale factor for the globe when a detail model is active (e.g., 0.7 for 70%).")]
//    public float globeScaleWhenModelActive = 0.7f;
//    [Tooltip("Duration for globe and model scale animations.")]
//    public float transitionDuration = 0.5f;
//    public LeanTweenType scaleEase = LeanTweenType.easeOutExpo; // Animation curve

//    private GameObject _currentDetailModelInstance;
//    private LandformFeature _currentDisplayedFeature;
//    private Vector3 _globeOriginalScale;
//    private bool _isDetailViewActive = false;

//    void Start()
//    {
//        if (globeGameObject != null)
//        {
//            _globeOriginalScale = globeGameObject.transform.localScale;
//        }
//        else { Debug.LogError("LandformDetailViewer: Globe GameObject not assigned!", this); }

//        if (backButton != null)
//        {
//            backButton.onClick.AddListener(HideLandformDetail);
//            backButton.gameObject.SetActive(false); // Start with back button hidden
//        }
//        else { Debug.LogError("LandformDetailViewer: Back Button not assigned!", this); }

//        if (modelSpawnPoint == null) Debug.LogError("LandformDetailViewer: Model Spawn Point not assigned!", this);

//        if (globeInfoUIManager == null)
//        {
//            globeInfoUIManager = FindObjectOfType<GlobeInfoUIManager>(); // Attempt to find if not assigned
//            if (globeInfoUIManager == null) Debug.LogError("LandformDetailViewer: GlobeInfoUIManager not found or assigned! Info text won't display.", this);
//        }
//    }

//    public void ShowLandformDetail(LandformFeature featureToShow)
//    {
//        if (_isDetailViewActive && _currentDisplayedFeature == featureToShow)
//        {
//            // Already showing this feature, do nothing or maybe re-focus animation
//            Debug.Log($"LandformDetailViewer: '{featureToShow.landformName}' is already being displayed.");
//            return;
//        }

//        if (featureToShow == null || featureToShow.detailModelPrefab == null || modelSpawnPoint == null)
//        {
//            Debug.LogError("LandformDetailViewer: Feature data, its prefab, or spawn point is null. Cannot show detail.", this);
//            return;
//        }

//        // If another model is already showing, hide it first smoothly or instantly
//        if (_currentDetailModelInstance != null)
//        {
//            // For a smoother transition, you might want to animate out the old one first
//            // and then animate in the new one. For simplicity here, we'll do an immediate hide.
//            HideLandformDetailImmediate();
//        }

//        _isDetailViewActive = true;
//        _currentDisplayedFeature = featureToShow;
//        Debug.Log($"LandformDetailViewer: Showing detail for '{featureToShow.landformName}'");

//        // Scale down globe
//        if (globeGameObject != null)
//        {
//            LeanTween.scale(globeGameObject, _globeOriginalScale * globeScaleWhenModelActive, transitionDuration).setEase(scaleEase);
//        }

//        // Instantiate and scale up new model
//        _currentDetailModelInstance = Instantiate(featureToShow.detailModelPrefab, modelSpawnPoint.position, modelSpawnPoint.rotation, modelSpawnPoint);
//        _currentDetailModelInstance.transform.localScale = Vector3.zero; // Start from zero scale
//        LeanTween.scale(_currentDetailModelInstance, Vector3.one, transitionDuration).setEase(scaleEase).setDelay(0.1f); // Slight delay for effect

//        // Display information text
//        if (globeInfoUIManager != null)
//        {
//            globeInfoUIManager.DisplayInformation(featureToShow.informationToShow);
//        }

//        if (backButton != null) backButton.gameObject.SetActive(true);
//    }

//    public void HideLandformDetail() // Called by the Back Button
//    {
//        if (!_isDetailViewActive) return;

//        Debug.Log("LandformDetailViewer: Hiding landform detail.");
//        if (_currentDetailModelInstance != null)
//        {
//            LeanTween.scale(_currentDetailModelInstance, Vector3.zero, transitionDuration).setEase(scaleEase).setOnComplete(() =>
//            {
//                Destroy(_currentDetailModelInstance);
//                _currentDetailModelInstance = null;
//            });
//        }

//        // Scale up globe
//        if (globeGameObject != null)
//        {
//            LeanTween.scale(globeGameObject, _globeOriginalScale, transitionDuration).setEase(scaleEase).setDelay(0.1f);
//        }

//        // Clear info text or set to default
//        if (globeInfoUIManager != null)
//        {
//            globeInfoUIManager.DisplayInformation("Click on a geographic feature for information."); // Or your preferred default
//        }

//        if (backButton != null) backButton.gameObject.SetActive(false);
//        _currentDisplayedFeature = null;
//        _isDetailViewActive = false;
//    }

//    private void HideLandformDetailImmediate() // Helper if switching directly between details
//    {
//        if (_currentDetailModelInstance != null)
//        {
//            Destroy(_currentDetailModelInstance);
//            _currentDetailModelInstance = null;
//        }
//        // Don't reset globe scale here if another detail is about to be shown immediately
//        // Also, don't hide back button or clear text yet.
//    }

//    // Ensure LeanTween is cleaned up if this object is destroyed.
//    void OnDestroy()
//    {
//        if (_currentDetailModelInstance != null)
//        {
//            LeanTween.cancel(_currentDetailModelInstance); // Cancel tweens on the model
//        }
//        if (globeGameObject != null)
//        {
//            LeanTween.cancel(globeGameObject); // Cancel tweens on the globe
//        }
//    }
//}