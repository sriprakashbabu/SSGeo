#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(UISpline))]
public class UISplineEditor : Editor
{
    UISpline spline;

    void OnEnable() => spline = (UISpline)target;

    void OnSceneGUI()
    {
        if (!spline.splineRoot) return;

        Event e = Event.current;

        // Click to add
        if (e.type == EventType.MouseDown && e.button == 0 && e.shift)
        {
            Vector2 pos = HandleUtility.GUIPointToWorldRay(e.mousePosition).origin;
            GameObject g = new GameObject("Point", typeof(RectTransform), typeof(UISplinePoint));
            g.transform.SetParent(spline.splineRoot);
            RectTransform rt = g.GetComponent<RectTransform>();
            rt.position = pos;
            Selection.activeGameObject = g;
            e.Use();
        }

        // Draw points + tangents
        foreach (Transform t in spline.splineRoot)
        {
            var p = t.GetComponent<UISplinePoint>();
            RectTransform rt = t as RectTransform;

            Vector3 world = rt.TransformPoint(rt.rect.center);
            EditorGUI.BeginChangeCheck();
            Vector3 newPos = Handles.PositionHandle(world, Quaternion.identity);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(rt, "Move Point");
                rt.position = newPos;
            }

            DrawTangent(rt, ref p.inTangent, Color.red);
            DrawTangent(rt, ref p.outTangent, Color.green);
        }
    }

    void DrawTangent(RectTransform rt, ref Vector2 tangent, Color color)
    {
        Vector3 start = rt.TransformPoint(rt.rect.center);
        Vector3 end = start + (Vector3)tangent;
        Handles.color = color;
        EditorGUI.BeginChangeCheck();
        var fmh_56_54_639045097373217837 = Quaternion.identity; Vector3 newEnd = Handles.FreeMoveHandle(end, 8, Vector3.zero, Handles.CircleHandleCap);
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(rt, "Move Tangent");
            tangent = (Vector2)(newEnd - start);
        }
        Handles.DrawLine(start, end);
    }
}
#endif
