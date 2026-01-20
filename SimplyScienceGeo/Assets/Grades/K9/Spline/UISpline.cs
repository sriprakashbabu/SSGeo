using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class UISplineData
{
    public List<Vector2> positions = new();
    public List<Vector2> inTangents = new();
    public List<Vector2> outTangents = new();
}

public class UISpline : MonoBehaviour
{
    public RectTransform splineRoot;
    public List<RectTransform> followers = new();

    [Range(0f, 1f)]
    public float progress;

    public float speed = 0.2f;
    public bool loop = true;

    List<UISplinePoint> points = new();

    void Awake() => CachePoints();

    void CachePoints()
    {
        points.Clear();
        foreach (Transform t in splineRoot)
            if (t.TryGetComponent(out UISplinePoint p))
                points.Add(p);
    }

    void Update()
    {
        if (points.Count < 2) return;

        progress += Time.deltaTime * speed;
        if (loop) progress %= 1f;
        else progress = Mathf.Clamp01(progress);

        foreach (var follower in followers)
            follower.anchoredPosition = Evaluate(progress);
    }

    Vector2 Evaluate(float t)
    {
        int segCount = points.Count - 1;
        float segT = t * segCount;
        int i = Mathf.Min(Mathf.FloorToInt(segT), segCount - 1);
        float localT = segT - i;

        RectTransform p0 = points[i].transform as RectTransform;
        RectTransform p1 = points[i + 1].transform as RectTransform;

        Vector2 a = p0.anchoredPosition;
        Vector2 b = a + points[i].outTangent;
        Vector2 c = p1.anchoredPosition + points[i + 1].inTangent;
        Vector2 d = p1.anchoredPosition;

        return Bezier(localT, a, b, c, d);
    }

    Vector2 Bezier(float t, Vector2 a, Vector2 b, Vector2 c, Vector2 d)
    {
        float u = 1 - t;
        return u * u * u * a +
               3 * u * u * t * b +
               3 * u * t * t * c +
               t * t * t * d;
    }

    // -------- Serialization --------

    public UISplineData Save()
    {
        CachePoints();
        UISplineData data = new();
        foreach (var p in points)
        {
            RectTransform rt = p.transform as RectTransform;
            data.positions.Add(rt.anchoredPosition);
            data.inTangents.Add(p.inTangent);
            data.outTangents.Add(p.outTangent);
        }
        return data;
    }

    public void Load(UISplineData data)
    {
        for (int i = splineRoot.childCount - 1; i >= 0; i--)
            DestroyImmediate(splineRoot.GetChild(i).gameObject);

        for (int i = 0; i < data.positions.Count; i++)
        {
            GameObject g = new GameObject($"P{i}", typeof(RectTransform), typeof(UISplinePoint));
            g.transform.SetParent(splineRoot);
            RectTransform rt = g.GetComponent<RectTransform>();
            rt.anchoredPosition = data.positions[i];

            UISplinePoint p = g.GetComponent<UISplinePoint>();
            p.inTangent = data.inTangents[i];
            p.outTangent = data.outTangents[i];
        }

        CachePoints();
    }
}
