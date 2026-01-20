using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(UISplineRoot))]
public class UISplineRootEditor : Editor
{
    UISplineRoot spline;

    private void OnEnable()
    {
        spline = (UISplineRoot)target;
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        GUILayout.Space(10);

        if (GUILayout.Button("Add Point"))
            AddPoint();

        if (GUILayout.Button("Remove Last Point"))
            RemovePoint();
    }

    void AddPoint()
    {
        // FIX 1: Correctly reference the nested class
        UISplineRoot.SplinePoint p = new UISplineRoot.SplinePoint();

        p.position = spline.points.Count == 0
            ? Vector2.zero
            : spline.points[spline.points.Count - 1].position + Vector2.right * 100f;

        p.inTangent = Vector2.left * 30f;
        p.outTangent = Vector2.right * 30f;

        spline.points.Add(p);
        EditorUtility.SetDirty(spline);
    }

    void RemovePoint()
    {
        if (spline.points.Count == 0) return;

        spline.points.RemoveAt(spline.points.Count - 1);
        EditorUtility.SetDirty(spline);
    }

    private void OnSceneGUI()
    {
        if (spline.points.Count < 2)
            return;

        Transform tr = spline.transform;

        // ---- Draw curve ----
        Handles.color = spline.lineColor;
        Vector3 prev = spline.GetPoint(0f);

        for (int i = 1; i <= spline.resolution; i++)
        {
            float t = i / (float)spline.resolution;
            Vector3 pos = spline.GetPoint(t);
            Handles.DrawAAPolyLine(spline.lineThickness, prev, pos);
            prev = pos;
        }

        // ---- Draw points & tangents ----
        for (int i = 0; i < spline.points.Count; i++)
        {
            var p = spline.points[i];
            Vector3 wp = tr.TransformPoint(p.position);

            Color pointColor =
                (i == 0) ? spline.startColor :
                (i == spline.points.Count - 1) ? spline.endColor :
                spline.midColor;

            // Point handle
            Handles.color = pointColor;
            EditorGUI.BeginChangeCheck();

            // FIX 2: Removed decompilation artifacts ("fmh_...")
            Vector3 newWp = Handles.FreeMoveHandle(
                wp,
                HandleUtility.GetHandleSize(wp) * spline.pointHandleSize * 0.1f,
                Vector3.zero,
                Handles.SphereHandleCap
            );

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(spline, "Move Spline Point");
                p.position = tr.InverseTransformPoint(newWp);
            }

            // Tangent handles
            Handles.color = spline.tangentColor;

            Vector3 outWp = tr.TransformPoint(p.position + p.outTangent);
            Vector3 inWp = tr.TransformPoint(p.position + p.inTangent);

            Handles.DrawLine(wp, outWp);
            Handles.DrawLine(wp, inWp);

            EditorGUI.BeginChangeCheck();

            // FIX 2: Removed decompilation artifacts
            Vector3 newOut = Handles.FreeMoveHandle(
                outWp,
                HandleUtility.GetHandleSize(outWp) * spline.tangentHandleSize * 0.1f,
                Vector3.zero,
                Handles.CircleHandleCap
            );

            Vector3 newIn = Handles.FreeMoveHandle(
                inWp,
                HandleUtility.GetHandleSize(inWp) * spline.tangentHandleSize * 0.1f,
                Vector3.zero,
                Handles.CircleHandleCap
            );

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(spline, "Move Tangents");
                // Explicitly cast p.position to Vector3 for math, then cast result to Vector2
                p.outTangent = (Vector2)(tr.InverseTransformPoint(newOut) - (Vector3)p.position);
                p.inTangent = (Vector2)(tr.InverseTransformPoint(newIn) - (Vector3)p.position);
            }
        }
    }
}