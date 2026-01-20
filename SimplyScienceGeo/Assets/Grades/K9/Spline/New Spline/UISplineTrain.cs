using System.Collections.Generic;
using UnityEngine;

public class UISplineTrain : MonoBehaviour
{
    public enum EasingType
    {
        Linear,
        EaseIn,     // Starts slow, speeds up
        EaseOut,    // Starts fast, slows down
        SmoothStep  // Slow start, fast middle, slow end
    }

    [Header("Settings")]
    public UISplineRoot spline;
    public GameObject uiPrefab;
    public Transform container;

    [Header("Train Config")]
    public int amount = 3;
    [Range(0f, 1f)]
    public float spacing = 0.05f;
    public float speed = 0.2f;
    public bool loop = true;

    [Header("Movement Dynamics")]
    public EasingType movementType = EasingType.Linear;

    private List<RectTransform> _items = new List<RectTransform>();
    private float _progress = 0f;

    private void Start()
    {
        if (spline == null || uiPrefab == null)
        {
            Debug.LogError("UISplineTrain: Missing Spline or Prefab assignment.");
            return;
        }

        SpawnItems();
    }

    private void SpawnItems()
    {
        foreach (var item in _items)
        {
            if (item != null) Destroy(item.gameObject);
        }
        _items.Clear();

        Transform parent = container != null ? container : transform;

        for (int i = 0; i < amount; i++)
        {
            GameObject go = Instantiate(uiPrefab, parent);
            go.name = $"{uiPrefab.name}_{i}";

            RectTransform rt = go.GetComponent<RectTransform>();
            if (rt == null) rt = go.AddComponent<RectTransform>();

            rt.localScale = Vector3.one;
            rt.localRotation = Quaternion.identity;

            _items.Add(rt);
        }
    }

    private void Update()
    {
        if (_items.Count == 0 || spline == null) return;

        // 1. Advance the raw progress (linear time)
        _progress += Time.deltaTime * speed;

        if (loop)
        {
            if (_progress > 1f) _progress -= 1f;
        }
        else
        {
            if (_progress > 1f) _progress = 1f;
        }

        // 2. Move items
        for (int i = 0; i < _items.Count; i++)
        {
            // Calculate raw 't' for this item based on spacing
            float rawT = _progress - (i * spacing);

            // Handle looping logic
            if (loop)
            {
                if (rawT < 0f) rawT += 1f;
            }
            else
            {
                if (rawT < 0f) rawT = 0f;
            }

            // 3. Apply Easing to the time value
            float easedT = ApplyEasing(rawT);

            Vector3 worldPos = spline.GetPoint(easedT);
            _items[i].position = worldPos;
        }
    }

    private float ApplyEasing(float t)
    {
        switch (movementType)
        {
            case EasingType.EaseIn:
                // Quadratic Ease In (t^2)
                return t * t;

            case EasingType.EaseOut:
                // Quadratic Ease Out (flip, square, flip back)
                return t * (2f - t);

            case EasingType.SmoothStep:
                // SmoothStep (t^2 * (3 - 2t)) - Classic 'S' curve
                return t * t * (3f - 2f * t);

            case EasingType.Linear:
            default:
                return t;
        }
    }
}