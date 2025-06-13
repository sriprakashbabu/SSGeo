using UnityEngine;
using UnityEngine.UI;

/// Attach this to your “GlobeManager” object
public class GlobeInteractiveSwitcher : MonoBehaviour
{
    public enum InteractiveMode { LatLon, Continents, Relief, TimeZones }   // ★ NEW

    [System.Serializable]
    public class ModeBundle
    {
        [Tooltip("Meshes, canvases, or panels that belong exclusively to this mode")]
        public GameObject[] objects;

        [Tooltip("Optional: the ToggleGroup whose child Toggles are part of this mode.\n" +
                 "Leave empty for modes with no toggles (e.g. Time Zones).")]
        public ToggleGroup toggleGroup;
    }

    [Header("Assign bundles for each interactive mode")]
    [SerializeField] private ModeBundle latLonBundle;
    [SerializeField] private ModeBundle continentsBundle;
    [SerializeField] private ModeBundle reliefBundle;
    [SerializeField] private ModeBundle timeZonesBundle;            // ★ NEW

    private ModeBundle _current;

    /* ───────────────────── lifecycle ───────────────────── */

    private void Awake()
    {
        // Disable everything so we start from a clean slate
        SetBundleActive(latLonBundle, false);
        SetBundleActive(continentsBundle, false);
        SetBundleActive(reliefBundle, false);
        SetBundleActive(timeZonesBundle, false);                  // ★ NEW
    }

    private void Start()
    {
#if UNITY_EDITOR
    SetMode(preview);               // Scene view preview while you work
#else
        SetMode(InteractiveMode.Continents); // Default mode in builds
#endif
    }


    /* ───────────────────── public API ───────────────────── */

    // Called from UI buttons or native bridge; index must match enum order
    public void SetMode(int index) => SetMode((InteractiveMode)index);

    public void SetMode(InteractiveMode mode)
    {
        if (_current != null) SetBundleActive(_current, false);

        _current = mode switch
        {
            InteractiveMode.LatLon => latLonBundle,
            InteractiveMode.Continents => continentsBundle,
            InteractiveMode.Relief => reliefBundle,
            InteractiveMode.TimeZones => timeZonesBundle,         // ★ NEW
            _ => null
        };

        if (_current != null) SetBundleActive(_current, true);
    }

    /* ───────────────────── helpers ───────────────────── */

    private static void SetBundleActive(ModeBundle bundle, bool isActive)
    {
        if (bundle == null) return;

        // 1) Show / hide meshes, canvases, panels
        foreach (var obj in bundle.objects)
            if (obj) obj.SetActive(isActive);

        // 2) Handle any toggles that belong to this mode
        if (bundle.toggleGroup)
        {
            bundle.toggleGroup.gameObject.SetActive(isActive);

            foreach (var t in bundle.toggleGroup.GetComponentsInChildren<Toggle>(true))
            {
                t.interactable = isActive;
                if (!isActive) t.isOn = false;   // reset when the mode is hidden
            }
        }
    }

#if UNITY_EDITOR
    /* ───────────────── Inspector preview ───────────────── */
    [Header("Editor Preview")]
    [SerializeField] private InteractiveMode preview = InteractiveMode.LatLon;

    private void OnValidate() => SetMode(preview);
#endif
}
