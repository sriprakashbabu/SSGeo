using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(ForestGenerator))]
public class ForestGeneratorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // 1. Get reference to the script
        ForestGenerator generator = (ForestGenerator)target;

        // 2. Draw the Default "Script" field and other basic settings
        // We manually draw specific fields to keep it clean
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Main Settings", EditorStyles.boldLabel);

        // Draw the Enum dropdown for Forest Type
        generator.forestType = (ForestGenerator.ForestType)EditorGUILayout.EnumPopup("Forest Type", generator.forestType);

        // Draw Area Size
        generator.areaSize = EditorGUILayout.Vector2Field("Area Size", generator.areaSize);

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Object Counts", EditorStyles.boldLabel);
        generator.treeCount = EditorGUILayout.IntField("Tree Count", generator.treeCount);
        generator.stoneCount = EditorGUILayout.IntField("Stone Count", generator.stoneCount);
        generator.bushCount = EditorGUILayout.IntField("Bush Count", generator.bushCount);

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Fine Tuning", EditorStyles.boldLabel);
        generator.treeSpacing = EditorGUILayout.FloatField("Tree Spacing", generator.treeSpacing);
        generator.stoneSpacing = EditorGUILayout.FloatField("Stone Spacing", generator.stoneSpacing);
        generator.bushSpacing = EditorGUILayout.FloatField("Bush Spacing", generator.bushSpacing);

        // Draw Randomization settings
        SerializedProperty minScale = serializedObject.FindProperty("minScale");
        SerializedProperty maxScale = serializedObject.FindProperty("maxScale");
        SerializedProperty maxTilt = serializedObject.FindProperty("maxTiltAngle");
        EditorGUILayout.PropertyField(minScale);
        EditorGUILayout.PropertyField(maxScale);
        EditorGUILayout.PropertyField(maxTilt);

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Active Preset Configuration", EditorStyles.boldLabel);

        // 3. ONLY draw the preset that is currently selected
        SerializedProperty activeList = null;

        switch (generator.forestType)
        {
            case ForestGenerator.ForestType.Mangrove:
                activeList = serializedObject.FindProperty("mangrovePreset");
                break;
            case ForestGenerator.ForestType.Montane:
                activeList = serializedObject.FindProperty("montanePreset");
                break;
            case ForestGenerator.ForestType.TropicalThorn:
                activeList = serializedObject.FindProperty("thornPreset");
                break;
            case ForestGenerator.ForestType.TropicalDeciduous:
                activeList = serializedObject.FindProperty("deciduousPreset");
                break;
            case ForestGenerator.ForestType.TropicalEvergreen:
                activeList = serializedObject.FindProperty("evergreenPreset");
                break;
        }

        // Show the relevant list (e.g., Mangrove Preset)
        if (activeList != null)
        {
            EditorGUILayout.PropertyField(activeList, true);
        }

        EditorGUILayout.Space(20);

        // 4. The Big Button
        GUI.backgroundColor = Color.green;
        if (GUILayout.Button("Generate Forest", GUILayout.Height(40)))
        {
            generator.GenerateForest();
        }

        GUI.backgroundColor = Color.red;
        if (GUILayout.Button("Clear Forest"))
        {
            generator.ClearForest();
        }
        GUI.backgroundColor = Color.white;

        // Apply changes to the serialized object
        serializedObject.ApplyModifiedProperties();
    }
}