using UnityEngine;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

// 1. This attribute makes the script run in the Editor
[ExecuteAlways]
public class UISplineFollower : MonoBehaviour
{
    public RectTransform image;
    public RectTransform pointsParent;
    public float speed = 0.3f;
    public bool loop = true;

    // 2. Add a slider to scrub the animation manually
    [Range(0, 1)]
    public float previewProgress = 0f;

    List<RectTransform> points = new();
    float t;

    void OnEnable()
    {
        CachePoints();
    }

    void CachePoints()
    {
        points.Clear();
        if (!pointsParent) return;

        foreach (Transform child in pointsParent)
            if (child is RectTransform rt)
                points.Add(rt);
    }

    void Update()
    {
        // In the editor, we re-cache often to handle you moving points around
        if (!Application.isPlaying) CachePoints();

        if (points.Count < 4 || !image) return;

        float maxT = points.Count - 3;

        // 3. Logic: If playing, use Time. If in Editor, use the Slider.
        if (Application.isPlaying)
        {
            t += Time.deltaTime * speed;

            float splineT = loop ? Mathf.Repeat(t, maxT) : Mathf.Clamp(t, 0, maxT);
            MoveImage(splineT);
        }
        else
        {
            // Map the 0-1 slider to the spline's length
            float splineT = previewProgress * maxT;
            MoveImage(splineT);
        }
    }

    void MoveImage(float splineT)
    {
        int i = Mathf.FloorToInt(splineT);

        // Safety clamp for the very end of the spline
        if (i >= points.Count - 3) i = points.Count - 4;

        float localT = splineT - i;

        Vector2 pos = CatmullRom(
            localT,
            points[i].anchoredPosition,
            points[i + 1].anchoredPosition,
            points[i + 2].anchoredPosition,
            points[i + 3].anchoredPosition
        );

        image.anchoredPosition = pos;
    }

    Vector2 CatmullRom(float t, Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3)
    {
        return 0.5f * (
            (2f * p1) +
            (-p0 + p2) * t +
            (2f * p0 - 5f * p1 + 4f * p2 - p3) * t * t +
            (-p0 + 3f * p1 - 3f * p2 + p3) * t * t * t
        );
    }

#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        // Ensure points are ready for drawing
        if (points.Count == 0) CachePoints();
        if (points.Count < 4) return;

        // --- Draw Curve ---
        Handles.color = Color.magenta;
        Vector3 prev = ToWorld(points[1].anchoredPosition); 

        int steps = 20;
        for (int i = 0; i < points.Count - 3; i++)
        {
            for (int j = 1; j <= steps; j++)
            {
                float t = j / (float)steps;
                Vector2 p = CatmullRom(
                    t,
                    points[i].anchoredPosition,
                    points[i + 1].anchoredPosition,
                    points[i + 2].anchoredPosition,
                    points[i + 3].anchoredPosition
                );

                Vector3 world = ToWorld(p);
                Handles.DrawLine(prev, world);
                prev = world;
            }
        }

        // --- Draw Points ---
        for (int i = 0; i < points.Count; i++)
        {
            Vector3 world = points[i].TransformPoint(points[i].rect.center);
            string label = $"P{i}";
            float size = 10f;

            if (i == 1) { Handles.color = Color.green; label += " (Start)"; size = 15f; }
            else if (i == points.Count - 2) { Handles.color = Color.red; label += " (End)"; size = 15f; }
            else if (i == 0 || i == points.Count - 1) { Handles.color = Color.yellow; label += " (Control)"; size = 8f; }
            else { Handles.color = Color.cyan; }

            Handles.SphereHandleCap(0, world, Quaternion.identity, size, EventType.Repaint);
            
            GUIStyle style = new GUIStyle();
            style.normal.textColor = Handles.color;
            Handles.Label(world + Vector3.up * 15f, label, style);
        }
    }

    Vector3 ToWorld(Vector2 anchoredPos)
    {
        if (pointsParent == null) return Vector3.zero;
        return pointsParent.TransformPoint(anchoredPos);
    }
#endif
}