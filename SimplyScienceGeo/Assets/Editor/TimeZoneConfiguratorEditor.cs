#if UNITY_EDITOR
using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class TimeZoneConfiguratorEditor
{
    // How many of the first entries to bold in each zone's list
    private const int BOLD_TOP_N = 6;

    private struct ZoneSpec
    {
        public string utcLabel;
        public string[] countries;   // Put "major" ones first
        public string hexColor;
    }

    private static readonly Dictionary<string, ZoneSpec> ZONES =
        new Dictionary<string, ZoneSpec>
    {
        // ---------- UTC + ----------
        { "Globe_time_zones_00", new ZoneSpec{
            utcLabel="UTC+0 (GMT)",
            countries=new string[]{ "United Kingdom (winter)", "Portugal", "Iceland", "Ghana", "Senegal" },
            hexColor="#d32b2b" }},

        { "Globe_time_zones_01", new ZoneSpec{
            utcLabel="UTC+1",
            countries=new string[]{ "France", "Germany", "Spain", "Italy", "Nigeria", "Algeria" },
            hexColor="#d34b2b" }},

        { "Globe_time_zones_02", new ZoneSpec{
            utcLabel="UTC+2",
            countries=new string[]{ "South Africa", "Egypt", "Greece", "Finland", "Israel (std.)" },
            hexColor="#d36c2b" }},

        { "Globe_time_zones_03", new ZoneSpec{
            utcLabel="UTC+3",
            countries=new string[]{ "Saudi Arabia", "Russia (Moscow)", "Iraq", "Kenya" },
            hexColor="#d38d2b" }},

        { "Globe_time_zones_04", new ZoneSpec{
            utcLabel="UTC+4",
            countries=new string[]{ "United Arab Emirates", "Oman", "Armenia", "Azerbaijan", "Mauritius" },
            hexColor="#d3ad2b" }},

        { "Globe_time_zones_04_1_2", new ZoneSpec{
            utcLabel="UTC+4:30",
            countries=new string[]{ "Afghanistan" },
            hexColor="#d3ce2b" }},

        { "Globe_time_zones_05", new ZoneSpec{
            utcLabel="UTC+5",
            countries=new string[]{ "Pakistan", "Uzbekistan", "Tajikistan", "Maldives" },
            hexColor="#b8d32b" }},

        { "Globe_time_zones_05_1_2", new ZoneSpec{
            utcLabel="UTC+5:30 (IST)",
            countries=new string[]{ "India", "Sri Lanka" },
            hexColor="#97d32b" }},

        { "Globe_time_zones_05_3_4", new ZoneSpec{
            utcLabel="UTC+5:45",
            countries=new string[]{ "Nepal" },
            hexColor="#77d32b" }},

        { "Globe_time_zones_06", new ZoneSpec{
            utcLabel="UTC+6",
            countries=new string[]{ "Bangladesh", "Bhutan", "Kyrgyzstan", "Kazakhstan (E)" },
            hexColor="#56d32b" }},

        { "Globe_time_zones_06_1_2", new ZoneSpec{
            utcLabel="UTC+6:30",
            countries=new string[]{ "Myanmar", "Cocos (Keeling) Islands" },
            hexColor="#36d32b" }},

        { "Globe_time_zones_07", new ZoneSpec{
            utcLabel="UTC+7",
            countries=new string[]{ "Thailand", "Vietnam", "Cambodia", "Laos", "Indonesia (Jakarta)" },
            hexColor="#2bd341" }},

        { "Globe_time_zones_08", new ZoneSpec{
            utcLabel="UTC+8",
            countries=new string[]{ "China", "Singapore", "Malaysia", "Philippines", "Hong Kong", "Western Australia" },
            hexColor="#2bd361" }},

        { "Globe_time_zones_09", new ZoneSpec{
            utcLabel="UTC+9",
            countries=new string[]{ "Japan", "South Korea" },
            hexColor="#2bd382" }},

        { "Globe_time_zones_09_1_2", new ZoneSpec{
            utcLabel="UTC+9:30",
            countries=new string[]{ "Australia (South Australia, Northern Territory)" },
            hexColor="#2bd3a2" }},

        { "Globe_time_zones_10", new ZoneSpec{
            utcLabel="UTC+10",
            countries=new string[]{ "Australia (Queensland)", "Papua New Guinea", "Russia (Far East)" },
            hexColor="#2bd3c3" }},

        { "Globe_time_zones_11", new ZoneSpec{
            utcLabel="UTC+11",
            countries=new string[]{ "Solomon Islands", "New Caledonia", "Russia (Sakhalin/Primorye)" },
            hexColor="#2bc3d3" }},

        { "Globe_time_zones_12", new ZoneSpec{
            utcLabel="UTC+12",
            countries=new string[]{ "New Zealand (std.)", "Fiji", "Tuvalu" },
            hexColor="#2ba2d3" }},

        // ---------- UTC − (brighter, varied palette) ----------
        { "Globe_time_zones_M01", new ZoneSpec{
            utcLabel="UTC−1",
            countries=new string[]{ "Azores (Portugal)", "Cape Verde" },
            hexColor="#42A5F5" }},  // bright blue

        { "Globe_time_zones_M02", new ZoneSpec{
            utcLabel="UTC−2",
            countries=new string[]{ "South Georgia & South Sandwich", "Greenland (parts)" },
            hexColor="#26C6DA" }},  // teal

        { "Globe_time_zones_M03", new ZoneSpec{
            utcLabel="UTC−3",
            countries=new string[]{ "Brazil (São Paulo)", "Argentina", "Uruguay" },
            hexColor="#7E57C2" }},  // purple

        { "Globe_time_zones_M03_1_2", new ZoneSpec{
            utcLabel="UTC−3:30",
            countries=new string[]{ "Canada (Newfoundland)" },
            hexColor="#EC407A" }},  // pink

        { "Globe_time_zones_M04", new ZoneSpec{
            utcLabel="UTC−4",
            countries=new string[]{ "Atlantic Canada", "Dominican Republic", "Venezuela", "Bolivia" },
            hexColor="#FF8A65" }},  // salmon

        { "Globe_time_zones_M05", new ZoneSpec{
            utcLabel="UTC−5",
            countries=new string[]{ "USA/Canada (Eastern)", "Colombia", "Peru", "Ecuador" },
            hexColor="#FFA726" }},  // bright orange

        { "Globe_time_zones_M06", new ZoneSpec{
            utcLabel="UTC−6",
            countries=new string[]{ "USA/Canada (Central)", "Mexico City", "Guatemala", "Costa Rica" },
            hexColor="#FFD54F" }},  // amber

        { "Globe_time_zones_M07", new ZoneSpec{
            utcLabel="UTC−7",
            countries=new string[]{ "USA/Canada (Mountain)", "Phoenix (no DST)", "Alberta" },
            hexColor="#9CCC65" }},  // light green

        { "Globe_time_zones_M08", new ZoneSpec{
            utcLabel="UTC−8",
            countries=new string[]{ "USA/Canada (Pacific)", "Baja California" },
            hexColor="#80DEEA" }},  // cyan

        { "Globe_time_zones_M09", new ZoneSpec{
            utcLabel="UTC−9",
            countries=new string[]{ "Alaska (Anchorage)", "Gambier Islands" },
            hexColor="#B39DDB" }},  // lavender

        { "Globe_time_zones_M10", new ZoneSpec{
            utcLabel="UTC−10",
            countries=new string[]{ "Hawaiʻi", "Cook Islands", "French Polynesia" },
            hexColor="#81C784" }},  // green

        { "Globe_time_zones_M11", new ZoneSpec{
            utcLabel="UTC−11",
            countries=new string[]{ "American Samoa", "Niue" },
            hexColor="#FFB74D" }},  // orange-peach

        { "Globe_time_zones_M12", new ZoneSpec{
            utcLabel="UTC−12",
            countries=new string[]{ "Baker Island", "Howland Island" },
            hexColor="#90A4AE" }},  // blue-grey
    };

    [MenuItem("Tools/Time Zones/Apply to Selected Parent")]
    private static void ApplyToSelectedParent()
    {
        Transform t = Selection.activeTransform;
        if (t == null)
        {
            if (!EditorUtility.DisplayDialog("Time Zones",
                "No parent selected. Apply to the whole scene?", "Yes (Whole Scene)", "Cancel"))
                return;
            ApplyToWholeScene();
            return;
        }
        ApplyUnder(t);
    }

    [MenuItem("Tools/Time Zones/Apply to Whole Scene")]
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
            ZoneSpec spec;
            if (!ZONES.TryGetValue(f.gameObject.name, out spec)) continue;

            Undo.RecordObject(f, "Apply Time Zone Data");

            // Header
            f.featureName = spec.utcLabel;

            // Body: comma-separated; first BOLD_TOP_N entries wrapped in <b>...</b>
            var sb = new StringBuilder();
            for (int i = 0; i < spec.countries.Length; i++)
            {
                string item = spec.countries[i];
                if (i < BOLD_TOP_N) item = "<b>" + item + "</b>";
                sb.Append(item);
                if (i < spec.countries.Length - 1) sb.Append(", ");
            }
            f.informationText = sb.ToString();

            // Colors
            Color baseColor;
            if (ColorUtility.TryParseHtmlString(spec.hexColor, out baseColor))
            {
                f.overrideInitialColor = true;
                f.initialColor = baseColor;

                float h, s, v;
                Color.RGBToHSV(baseColor, out h, out s, out v);
                f.highlightColor = Color.HSVToRGB(h, s, Mathf.Clamp01(v * 1.2f));
            }

            EditorUtility.SetDirty(f);
            changed++;
        }

        EditorUtility.DisplayDialog("Time Zones", "Updated " + changed + " InteractableFeature(s).", "OK");
    }
}
#endif
