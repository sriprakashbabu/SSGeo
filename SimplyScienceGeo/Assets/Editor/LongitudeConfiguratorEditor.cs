#if UNITY_EDITOR
using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class LongitudeConfiguratorEditor
{
    private const int BOLD_TOP_N = 2; // how many cities to bold for kids

    private struct BandInfo
    {
        public string region;     // simple description
        public string[] cities;   // put the most famous first
    }

    // Simple, 6th-grade friendly region + city examples for each band
    private static readonly Dictionary<string, BandInfo> INFO =
        new Dictionary<string, BandInfo>
    {
        // ---- 0°E → 180°E (positive) ----
        { "Globe_longitudes_0",  new BandInfo{ region="Western Europe and North Africa", cities=new string[]{ "Paris", "Berlin", "Rome", "Algiers" } } },
        { "Globe_longitudes_1",  new BandInfo{ region="Central & Eastern Europe", cities=new string[]{ "Athens", "Budapest", "Warsaw", "Prague" } } },
        { "Globe_longitudes_2",  new BandInfo{ region="Northeast Africa and the Middle East", cities=new string[]{ "Cairo", "Nairobi", "Addis Ababa" } } },
        { "Globe_longitudes_3",  new BandInfo{ region="The Middle East and the Caucasus", cities=new string[]{ "Tehran", "Dubai", "Baku" } } },
        { "Globe_longitudes_4",  new BandInfo{ region="Central Asia (west)", cities=new string[]{ "Karachi", "Tashkent", "Yekaterinburg" } } },
        { "Globe_longitudes_5",  new BandInfo{ region="Northern India", cities=new string[]{ "New Delhi", "Jaipur", "Kolkata" } } },
        { "Globe_longitudes_6",  new BandInfo{ region="Myanmar and Thailand area", cities=new string[]{ "Yangon", "Bangkok", "Kunming" } } },
        { "Globe_longitudes_7",  new BandInfo{ region="Vietnam and southern China", cities=new string[]{ "Hanoi", "Ho Chi Minh City", "Hong Kong", "Jakarta" } } },
        { "Globe_longitudes_8",  new BandInfo{ region="Eastern China, Taiwan and the Philippines", cities=new string[]{ "Shanghai", "Taipei", "Manila" } } },
        { "Globe_longitudes_9",  new BandInfo{ region="Japan and Papua New Guinea", cities=new string[]{ "Tokyo", "Osaka", "Port Moresby" } } },
        { "Globe_longitudes_10", new BandInfo{ region="Far East Russia and Pacific islands", cities=new string[]{ "Magadan", "Honiara" } } },
        { "Globe_longitudes_11", new BandInfo{ region="New Zealand and Fiji area", cities=new string[]{ "Auckland", "Suva", "Tarawa" } } },
        { "Globe_longitudes_12", new BandInfo{ region="Around the 180° meridian (Pacific Ocean)", cities=new string[]{ "Dateline region", "Outer Pacific islands" } } },

        // ---- 0°W → 180°W (negative, 'M' = minus) ----
        { "Globe_longitudes_M1",  new BandInfo{ region="UK, Ireland and Portugal area", cities=new string[]{ "London", "Dublin", "Lisbon" } } },
        { "Globe_longitudes_M2",  new BandInfo{ region="Mid-Atlantic islands", cities=new string[]{ "Ponta Delgada (Azores)", "Praia (Cape Verde)" } } },
        { "Globe_longitudes_M3",  new BandInfo{ region="North Atlantic and Greenland coast", cities=new string[]{ "East Greenland", "North Atlantic Ocean" } } },
        { "Globe_longitudes_M4",  new BandInfo{ region="Atlantic near South America/Greenland", cities=new string[]{ "Greenland coast", "Atlantic Ocean" } } },
        { "Globe_longitudes_M5",  new BandInfo{ region="Caribbean and northern South America", cities=new string[]{ "San Juan", "Caracas", "Halifax" } } },
        { "Globe_longitudes_M6",  new BandInfo{ region="Eastern North America and Cuba", cities=new string[]{ "Washington, D.C.", "Toronto", "Havana" } } },
        { "Globe_longitudes_M7",  new BandInfo{ region="Central North America and Mexico", cities=new string[]{ "Mexico City", "Winnipeg", "Guatemala City" } } },
        { "Globe_longitudes_M8",  new BandInfo{ region="Western USA and Canada (mountain/desert)", cities=new string[]{ "Denver", "Phoenix", "Calgary" } } },
        { "Globe_longitudes_M9",  new BandInfo{ region="Pacific coast of USA and Canada", cities=new string[]{ "Vancouver", "Seattle", "San Francisco" } } },
        { "Globe_longitudes_M10", new BandInfo{ region="Alaska and Yukon", cities=new string[]{ "Anchorage", "Whitehorse" } } },
        { "Globe_longitudes_M11", new BandInfo{ region="Aleutian Islands and open Pacific", cities=new string[]{ "Adak", "Aleutian Islands" } } },
        { "Globe_longitudes_M12", new BandInfo{ region="Samoa and nearby Pacific islands", cities=new string[]{ "Apia", "Pago Pago", "Niue" } } },
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

            // Build header from the object's name
            string header = BuildHeaderFromName(key);
            if (string.IsNullOrEmpty(header)) continue;

            Undo.RecordObject(f, "Apply Longitude Info");

            // Header
            f.featureName = header;

            // Body (short sentence + bold first 2 cities, comma-separated)
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

    // Name → "XX°E – YY°E" or "XX°W – YY°W"
    private static string BuildHeaderFromName(string goName)
    {
        // Positive bands: Globe_longitudes_0 .. 12  (0 = 0°E–15°E)
        if (goName.StartsWith("Globe_longitudes_") && !goName.Contains("M"))
        {
            string tail = goName.Substring("Globe_longitudes_".Length);
            int idx;
            if (int.TryParse(tail, out idx))
            {
                idx = Mathf.Clamp(idx, 0, 12);
                int start = idx * 15;
                int end   = start + 15;
                if (end > 180) end = 180;
                return start + "°E – " + end + "°E";
            }
        }

        // Negative bands: Globe_longitudes_M1 .. M12 (1 = 0°W–15°W)
        if (goName.StartsWith("Globe_longitudes_M"))
        {
            string tail = goName.Substring("Globe_longitudes_M".Length);
            int idx;
            if (int.TryParse(tail, out idx))
            {
                idx = Mathf.Clamp(idx, 1, 12);
                int start = (idx - 1) * 15;
                int end   = idx * 15;
                return start + "°W – " + end + "°W";
            }
        }

        return null;
    }
}
#endif
