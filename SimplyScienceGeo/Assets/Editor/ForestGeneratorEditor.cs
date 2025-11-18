using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ForestGenerator))]
public class ForestGeneratorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // Required for editable arrays
        serializedObject.Update();

        DrawDefaultInspector();

        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Forest Tools", EditorStyles.boldLabel);

        ForestGenerator gen = (ForestGenerator)target;

        if (GUILayout.Button("Generate Forest"))
        {
            Undo.RegisterCompleteObjectUndo(gen.gameObject, "Generate Forest");
            gen.GenerateForest();
        }

        if (GUILayout.Button("Clear Forest"))
        {
            Undo.RegisterCompleteObjectUndo(gen.gameObject, "Clear Forest");
            gen.ClearForest();
        }

        // REQUIRED for saving array changes
        serializedObject.ApplyModifiedProperties();
    }
}
