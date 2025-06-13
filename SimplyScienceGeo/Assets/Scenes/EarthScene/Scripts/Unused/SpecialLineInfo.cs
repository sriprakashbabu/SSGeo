//using UnityEngine;

//// Make sure ChangeMeshColorURP.cs is in your project
//// [RequireComponent(typeof(ChangeMeshColorURP))] // Optional: Enforce ChangeMeshColorURP is present
//public class SpecialLineInfo : MonoBehaviour
//{
//    [Header("Line Properties")]
//    [Tooltip("The specific color for this line.")]
//    public Color lineBaseColor = Color.yellow; // Set this uniquely for each special line

//    [Tooltip("Information to display when this line is clicked.")]
//    [TextArea(3, 10)] // Makes the string field larger in the Inspector
//    public string informationToShow;

//    // Event to notify the UI manager
//    // The 'string' payload will be the informationToShow
//    public static event System.Action<string> OnSpecialLineClicked;

//    private ChangeMeshColorURP _colorController;

//    void Awake()
//    {
//        // Attempt to get the ChangeMeshColorURP component
//        _colorController = GetComponent<ChangeMeshColorURP>();

//        if (_colorController == null)
//        {
//            // If you prefer to add it automatically if missing:
//            // _colorController = gameObject.AddComponent<ChangeMeshColorURP>();
//            // Debug.LogWarning($"SpecialLineInfo on {gameObject.name} added ChangeMeshColorURP component dynamically.", this);

//            // If it's a strict requirement to be added manually:
//            Debug.LogError($"SpecialLineInfo on {gameObject.name} requires a ChangeMeshColorURP component to be attached manually. Please add it.", this);
//            enabled = false; // Disable this script if setup is incorrect
//            return;
//        }
//    }

//    void Start()
//    {
//        // Apply the specific color to this line
//        if (_colorController != null)
//        {
//            _colorController.targetColor = lineBaseColor; // Set the color
//            _colorController.ApplyColor();                // Apply it using MaterialPropertyBlock
//        }
//    }

//    // This method is called by Unity when the Collider on this GameObject is clicked
//    // (Requires a Collider component on this GameObject, and no UI element completely obscuring it)
//    void OnMouseDown()
//    {
//        if (string.IsNullOrEmpty(informationToShow))
//        {
//            Debug.LogWarning($"{gameObject.name} clicked, but has no informationToShow assigned.", this);
//            OnSpecialLineClicked?.Invoke($"{gameObject.name}: No specific information available.");
//        }
//        else
//        {
//            Debug.Log($"{gameObject.name} clicked. Info: {informationToShow}");
//            // Broadcast the event with the information
//            OnSpecialLineClicked?.Invoke(informationToShow);
//        }
//    }
//}