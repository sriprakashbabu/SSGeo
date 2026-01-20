using System.Collections.Generic;
using UnityEngine;


[ExecuteAlways]
public class UISplineRoot : MonoBehaviour
{
    [System.Serializable]
    public class SplinePoint
    {
        public Vector2 position;
        public Vector2 inTangent;
        public Vector2 outTangent;
    }

    public List<SplinePoint> points = new List<SplinePoint>();

    [Range(8, 64)]
    public int resolution = 32;

    [Header("Editor Visualization")]
    public float pointHandleSize = 6f;
    public float tangentHandleSize = 4f;
    public float lineThickness = 2f;

    public Color startColor = Color.green;
    public Color endColor = Color.red;
    public Color midColor = Color.yellow;
    public Color tangentColor = Color.cyan;
    public Color lineColor = Color.white;

    public Vector3 GetPoint(float t)
    {
        if (points.Count < 2)
            return transform.position;

        int segmentCount = points.Count - 1;
        float scaledT = t * segmentCount;

        int segIndex = Mathf.Clamp(Mathf.FloorToInt(scaledT), 0, segmentCount - 1);
        float segT = scaledT - segIndex;

        return CubicBezier(
            transform.TransformPoint(points[segIndex].position),
            transform.TransformPoint(points[segIndex].position + points[segIndex].outTangent),
            transform.TransformPoint(points[segIndex + 1].position + points[segIndex + 1].inTangent),
            transform.TransformPoint(points[segIndex + 1].position),
            segT
        );
    }

    private Vector3 CubicBezier(Vector3 a, Vector3 b, Vector3 c, Vector3 d, float t)
    {
        float u = 1f - t;
        return
            u * u * u * a +
            3f * u * u * t * b +
            3f * u * t * t * c +
            t * t * t * d;
    }
}
