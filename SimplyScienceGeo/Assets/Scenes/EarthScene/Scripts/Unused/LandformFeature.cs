//using UnityEngine;

//[RequireComponent(typeof(Collider))]
//[RequireComponent(typeof(ChangeMeshColorURP))] // To set its base color
//public class LandformFeature : MonoBehaviour
//{
//    [Header("Landform Properties")]
//    public string landformName = "Unnamed Landform";
//    [Tooltip("The base color for this landform. Alpha will be set to 1.")]
//    public Color baseLandformColor = Color.gray;
//    [Tooltip("Information to display when this landform is double-clicked.")]
//    [TextArea(3, 10)]
//    public string informationToShow;
//    [Tooltip("Prefab of the 3D detail model to display for this landform.")]
//    public GameObject detailModelPrefab;

//    // For double-click detection
//    private float _instanceLastClickTime;
//    private const float DOUBLE_CLICK_THRESHOLD = 0.3f; // Time in seconds for a double click

//    // References to other managers
//    private LandformDetailViewer _landformDetailViewer;
//    private GlobeInfoUIManager _globeInfoUIManager; // To display text directly via LandformDetailViewer
//    private ChangeMeshColorURP _colorController;
//    private Collider _thisCollider;

//    void Awake()
//    {
//        _thisCollider = GetComponent<Collider>();
//        _colorController = GetComponent<ChangeMeshColorURP>();
//        _instanceLastClickTime = -DOUBLE_CLICK_THRESHOLD; // Initialize for proper first double-click
//    }

//    void Start()
//    {
//        // Find necessary managers
//        _landformDetailViewer = FindObjectOfType<LandformDetailViewer>();
//        _globeInfoUIManager = FindObjectOfType<GlobeInfoUIManager>(); // Though LandformDetailViewer might primarily use it

//        // Apply the base color to this landform
//        if (_colorController != null)
//        {
//            // Ensure full opacity for the base color unless specified otherwise
//            _colorController.targetColor = new Color(baseLandformColor.r, baseLandformColor.g, baseLandformColor.b, 1f);
//            _colorController.ApplyColor();
//        }
//        else
//        {
//            Debug.LogError($"LandformFeature on '{gameObject.name}' is missing the ChangeMeshColorURP component.", this);
//        }

//        if (_landformDetailViewer == null)
//        {
//            Debug.LogError($"LandformFeature on '{gameObject.name}': LandformDetailViewer not found in the scene! Detailed view will not work.", this);
//        }
//        if (detailModelPrefab == null)
//        {
//            Debug.LogWarning($"LandformFeature on '{gameObject.name}': Detail Model Prefab is not assigned.", this);
//        }
//    }

//    /// <summary>
//    /// Called by GlobalClickHandler when this landform's collider is clicked.
//    /// Manages its own double-click detection.
//    /// </summary>
//    public void ProcessClick()
//    {
//        if (!_thisCollider.enabled || !this.enabled)
//        {
//            // Debug.Log($"LandformFeature on '{gameObject.name}': ProcessClick called, but component or collider is disabled.");
//            return;
//        }

//        Debug.Log($"LandformFeature: ProcessClick() on '{gameObject.name}' at Time: {Time.time:F2}");

//        if (Time.time - _instanceLastClickTime < DOUBLE_CLICK_THRESHOLD)
//        {
//            // --- DOUBLE-CLICK DETECTED ---
//            Debug.Log($"LandformFeature: DOUBLE-CLICK confirmed for '{gameObject.name}'!");

//            if (_landformDetailViewer != null)
//            {
//                // Tell the viewer to show this landform's details
//                _landformDetailViewer.ShowLandformDetail(this);
//            }
//            else
//            {
//                Debug.LogError($"LandformFeature on '{gameObject.name}': Cannot show detail, LandformDetailViewer not found!");
//            }

//            _instanceLastClickTime = -DOUBLE_CLICK_THRESHOLD; // Reset after a successful double click
//        }
//        else
//        {
//            Debug.Log($"LandformFeature: Single click (or first part of double-click) on '{gameObject.name}'. Storing click time.");
//        }
//        _instanceLastClickTime = Time.time; // Always update for the next click detection
//    }
//}