using UnityEngine;
using UnityEditor; // We can safely use this namespace in an Editor script

// This tells Unity that this is a custom editor FOR the ImageZoomer class
[CustomEditor(typeof(ImageZoomer))]
public class ImageZoomerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // Draw the default inspector (so you see all your public variables)
        DrawDefaultInspector();

        // Get a reference to the script we're inspecting
        ImageZoomer zoomer = (ImageZoomer)target;

        // Add a horizontal space for neatness
        EditorGUILayout.Space(10);

        // Add our custom button
        if (GUILayout.Button("Copy Current View Parameters"))
        {
            // Call our new copy function
            CopyCurrentViewParameters(zoomer);
        }
    }

    private void CopyCurrentViewParameters(ImageZoomer zoomer)
    {
        // Check if we're in Play Mode, as values are 0 otherwise
        if (!Application.isPlaying)
        {
            Debug.LogWarning("Please enter Play Mode to copy the runtime view values.");
            return;
        }

        if (zoomer.imageRectTransform == null)
        {
            Debug.LogWarning("Cannot copy values: Image RectTransform is null.");
            return;
        }

        // Format the values
        string output = $"--- ImageZoomer Values ---\n" +
                        $"Pan X: {zoomer.imageRectTransform.anchoredPosition.x:F2}\n" +
                        $"Pan Y: {zoomer.imageRectTransform.anchoredPosition.y:F2}\n" +
                        $"Zoom:  {zoomer.imageRectTransform.localScale.x:F2}\n" +
                        $"--------------------------";

        // This is the line that caused the error, but it's safe here.
        GUIUtility.systemCopyBuffer = output;

        // Also print to the console
        Debug.Log(output + " (Copied to clipboard)");
    }
}