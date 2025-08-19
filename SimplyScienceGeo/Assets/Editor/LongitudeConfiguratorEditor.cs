#if UNITY_EDITOR
using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class LongitudeConfiguratorEditor
{
    private const int BOLD_TOP_N = 2; // bold first N cities for readability

    private struct BandInfo
    {
        public string region;     // simple description for 6th grade
        public string[] cities;   // most famous first
    }

    // Map by starting degree in the name: longitudes_00, _15, _30, … _345
    private static readonly Dictionary<string, BandInfo> INFO =
        new Dictionary<string, BandInfo>
    {
        // 0°E–180°E
        { "longitudes_00",  new BandInfo{ region="Western Europe and North Africa",          cities=new string[]{ "London", "Paris", "Madrid", "Casablanca" } } },
        { "longitudes_15",  new BandInfo{ region="Central & Eastern Europe",                 cities=new string[]{ "Berlin", "Rome", "Vienna", "Prague" } } },
        { "longitudes_30",  new BandInfo{ region="Northeast Africa and the Middle East",     cities=new string[]{ "Cairo", "Addis Ababa", "Khartoum" } } },
        { "longitudes_45",  new BandInfo{ region="The Middle East and the Caucasus",         cities=new string[]{ "Riyadh", "Baghdad", "Tehran", "Baku" } } },
        { "longitudes_60",  new BandInfo{ region="Central Asia (west)",                      cities=new string[]{ "Karachi", "Tashkent", "Ashgabat" } } },
        { "longitudes_75",  new BandInfo{ region="Northern India and Himalaya",              cities=new string[]{ "New Delhi", "Jaipur", "Kathmandu" } } },
        { "longitudes_90",  new BandInfo{ region="Bangladesh, Myanmar, and west Thailand",   cities=new string[]{ "Dhaka", "Yangon", "Naypyidaw" } } },
        { "longitudes_105", new BandInfo{ region="Thailand, Vietnam, and south China",       cities=new string[]{ "Bangkok", "Hanoi", "Ho Chi Minh City" } } },
        { "longitudes_120", new BandInfo{ region="Eastern China, Taiwan, Philippines",       cities=new string[]{ "Shanghai", "Taipei", "Manila" } } },
        { "longitudes_135", new BandInfo{ region="Japan and far east China",                 cities=new string[]{ "Tokyo", "Osaka", "Sapporo" } } },
        { "longitudes_150", new BandInfo{ region="Pacific Russia and islands",               cities=new string[]{ "Vladivostok", "Honiara" } } },
        { "longitudes_165", new BandInfo{ region="Fiji and nearby Pacific",                  cities=new string[]{ "Suva", "Tarawa" } } },
        { "longitudes_180", new BandInfo{ region="Around the 180° line (Pacific Ocean)",     cities=new string[]{ "International Date Line", "Outer Pacific islands" } } },

        // 180°E–360° (= 180°W–0°W)
        { "longitudes_195", new BandInfo{ region="Samoa and nearby Pacific islands",         cities=new string[]{ "Apia", "Pago Pago", "Niue" } } },
        { "longitudes_210", new BandInfo{ region="Aleutian Islands and open Pacific",        cities=new string[]{ "Adak", "Aleutian Islands" } } },
        { "longitudes_225", new BandInfo{ region="Alaska and Yukon",                         cities=new string[]{ "Anchorage", "Fairbanks", "Whitehorse" } } },
        { "longitudes_240", new BandInfo{ region="Pacific coast of USA and Canada",          cities=new string[]{ "Vancouver", "Seattle", "San Francisco" } } },
        { "longitudes_255", new BandInfo{ region="Western USA and Canadian Rockies",         cities=new string[]{ "Denver", "Calgary", "Salt Lake City" } } },
        { "longitudes_270", new BandInfo{ region="Central North America and Mexico",         cities=new string[]{ "Mexico City", "Winnipeg", "Guatemala City" } } },
        { "longitudes_285", new BandInfo{ region="Eastern North America and Cuba",           cities=new string[]{ "New York", "Toronto", "Havana" } } },
        { "longitudes_300", new BandInfo{ region="Caribbean and northern South America",     cities=new string[]{ "San Juan", "Caracas", "Santo Domingo" } } },
        { "longitudes_315", new BandInfo{ region="North Atlantic and Greenland coast",       cities=new string[]{ "Nuuk", "Tasiilaq" } } },
        { "longitudes_330", new BandInfo{ region="Mid-Atlantic islands",                     cities=new string[]{ "Ponta Delgada (Azores)", "Praia (Cape Verde)" } } },
        { "longitudes_345", new BandInfo{ region="UK, Ireland and Portugal area",            cities=new string[]{ "Dublin", "Belfast", "Lisbon" } } },
    };

    [MenuItem("Tools/Longitudes/Apply to Selected Parent")]
    private static void ApplyToSelectedParent()
    {
        Transform t = Selection.activeTransform;
        if (t == null)
        {
            if (!EditorUtility.DisplayDialog("Longitudes",
                "No parent selected. Apply to the whole scene?", "Yes (Whole Scene)", "Cancel"))
                return;
            ApplyToWholeScene();
            return;
        }
        ApplyUnder(t);
    }

    [MenuItem("Tools/Longitudes/Apply to Whole Scene")]
    private static void ApplyToWholeScene()
    {
        var scene = SceneManager.GetActiveScene();
        foreach (var root in scene.GetRootGameObjects())
            ApplyUnder(root.transform);
    }

    private static void ApplyUnder(Transform parent)
    {
        var features = parent.GetComponentsInChildren<InteractableFeature>(true);
        int changed = 0;

        foreach (var f in features)
        {
            string key = f.gameObject.name;
            BandInfo info;
            if (!INFO.TryGetValue(key, out info)) continue;

            int startDeg;
            if (!TryParseStartDegree(key, out startDeg)) continue;

            string header = BuildHeaderFromStartDegree(startDeg);

            Undo.RecordObject(f, "Apply Longitude Info");

            // Header (e.g., "165°W – 150°W" or "30°E – 45°E")
            f.featureName = header;

            // Body (simple sentence + cities, first two bold)
            var sb = new StringBuilder();
            sb.Append("Covers ").Append(info.region).Append(". ");
            if (info.cities != null && info.cities.Length > 0)
            {
                sb.Append("Examples: ");
                for (int i = 0; i < info.cities.Length; i++)
                {
                    string name = info.cities[i];
                    if (i < BOLD_TOP_N) name = "<b>" + name + "</b>";
                    sb.Append(name);
                    if (i < info.cities.Length - 1) sb.Append(", ");
                }
                sb.Append(".");
            }
            f.informationText = sb.ToString();

            EditorUtility.SetDirty(f);
            changed++;
        }

        EditorUtility.DisplayDialog("Longitudes", "Updated " + changed + " InteractableFeature(s).", "OK");
    }

    // Parse "longitudes_XXX" → XXX as int (0..345, step 15)
    private static bool TryParseStartDegree(string name, out int startDeg)
    {
        startDeg = 0;
        const string prefix = "longitudes_";
        if (!name.StartsWith(prefix)) return false;
        string tail = name.Substring(prefix.Length);
        int deg;
        if (!int.TryParse(tail, out deg)) return false;
        // clamp to 0..345 in 15° steps
        deg = Mathf.Clamp(deg, 0, 345);
        if (deg % 15 != 0) deg -= deg % 15;
        startDeg = deg;
        return true;
    }

    // Convert startDeg (0..345) to a human header like:
    // 0..165 → "X°E – Y°E"; 180..345 → "A°W – B°W"
    private static string BuildHeaderFromStartDegree(int startDeg)
    {
        int endDeg = (startDeg + 15);
        if (endDeg > 360) endDeg -= 360;

        if (startDeg < 180)
        {
            // East longitudes 0..179
            return startDeg + "°E – " + endDeg + "°E";
        }
        else
        {
            // Convert to West notation: e.g., 195 → 165°W (because 360-195)
            int startW = 360 - startDeg;  // e.g., 195 → 165
            int endW   = 360 - endDeg;    // e.g., 210 → 150
            return startW + "°W – " + endW + "°W";
        }
    }
}
#endif
