using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

/// Put this on the parent that contains cloud sprites/images (and the rain system).
public class CloudWiggle : MonoBehaviour
{
    [Header("Motion")]
    [Tooltip("Max local offset (x,y) around the start position.")]
    public Vector2 moveAmplitude = new Vector2(0.15f, 0.08f);
    [Tooltip("How fast the Perlin noise drifts.")]
    public float driftSpeed = 0.15f;
    [Tooltip("Optional gentle bobbing frequency added on Y.")]
    public float bobHz = 0.08f;
    [Tooltip("Small tilt back and forth (degrees).")]
    public float rotationAmplitude = 1.5f;

    [Header("Color (subtle variation)")]
    public Color baseColor = Color.white;
    [Range(0f, 0.25f)] public float hueJitter = 0.02f;
    [Range(0f, 0.4f)] public float satJitter = 0.08f;
    [Range(0f, 0.4f)] public float valJitter = 0.08f;
    [Range(0.2f, 1f)] public float minAlpha = 0.6f;
    [Range(0.2f, 1f)] public float maxAlpha = 0.95f;

    [Header("Find/Filter")]
    [Tooltip("Only affect children with these components. Leave true unless you have a custom setup.")]
    public bool includeSpriteRenderers = true;
    public bool includeUIImages = true;
    [Tooltip("Skip children whose name contains this (e.g., 'Rain'). Leave empty to skip nothing.")]
    public string nameContainsToSkip = "Rain";

    class Cloud
    {
        public Transform tr;
        public bool isUI;
        public Vector3 startLocalPos;      // for SpriteRenderer
        public Vector2 startAnchoredPos;   // for UI Image
        public RectTransform rect;         // if UI
        public float seedX, seedY, seedR;
        public SpriteRenderer sr;
        public Image img;
    }

    readonly List<Cloud> _clouds = new List<Cloud>();
    float _t0;

    void Awake()
    {
        _t0 = Random.value * 100f; // desync instances if multiple parents exist
        GatherClouds();
        ApplyInitialColorJitter();
    }

    void GatherClouds()
    {
        _clouds.Clear();

        if (includeSpriteRenderers)
        {
            foreach (var sr in GetComponentsInChildren<SpriteRenderer>(true))
            {
                if (ShouldSkip(sr.gameObject)) continue;
                var c = new Cloud
                {
                    tr = sr.transform,
                    isUI = false,
                    startLocalPos = sr.transform.localPosition,
                    sr = sr,
                    seedX = Random.Range(0f, 1000f),
                    seedY = Random.Range(0f, 1000f),
                    seedR = Random.Range(0f, 1000f)
                };
                _clouds.Add(c);
            }
        }

        if (includeUIImages)
        {
            foreach (var im in GetComponentsInChildren<Image>(true))
            {
                if (ShouldSkip(im.gameObject)) continue;
                var rt = im.rectTransform;
                var c = new Cloud
                {
                    tr = rt,
                    isUI = true,
                    rect = rt,
                    startAnchoredPos = rt.anchoredPosition,
                    img = im,
                    seedX = Random.Range(0f, 1000f),
                    seedY = Random.Range(0f, 1000f),
                    seedR = Random.Range(0f, 1000f)
                };
                _clouds.Add(c);
            }
        }
    }

    bool ShouldSkip(GameObject go)
    {
        // skip ParticleSystems (your rain) and optional name matches
        if (go.GetComponentInParent<ParticleSystem>() && go.GetComponent<Renderer>() == null && go.GetComponent<Image>() == null)
            return true;
        if (!string.IsNullOrEmpty(nameContainsToSkip) && go.name.Contains(nameContainsToSkip))
            return true;
        return false;
    }

    void ApplyInitialColorJitter()
    {
        // convert base to HSV once
        Color.RGBToHSV(baseColor, out float h0, out float s0, out float v0);

        foreach (var c in _clouds)
        {
            float h = Mathf.Repeat(h0 + Random.Range(-hueJitter, hueJitter), 1f);
            float s = Mathf.Clamp01(s0 + Random.Range(-satJitter, satJitter));
            float v = Mathf.Clamp01(v0 + Random.Range(-valJitter, valJitter));
            float a = Random.Range(minAlpha, maxAlpha);

            var tinted = Color.HSVToRGB(h, s, v);
            tinted.a = a;

            if (c.sr) c.sr.color = tinted;
            if (c.img) c.img.color = tinted;
        }
    }

    void Update()
    {
        float t = Time.time + _t0;

        foreach (var c in _clouds)
        {
            float nx = Mathf.PerlinNoise(c.seedX, t * driftSpeed) * 2f - 1f;
            float ny = Mathf.PerlinNoise(c.seedY, t * driftSpeed) * 2f - 1f;
            float bob = Mathf.Sin((t + c.seedY) * (Mathf.PI * 2f) * bobHz);

            Vector2 offset = new Vector2(nx * moveAmplitude.x, ny * moveAmplitude.y + bob * moveAmplitude.y * 0.5f);
            float rot = rotationAmplitude * Mathf.Sin((t + c.seedR) * 0.5f);

            if (c.isUI && c.rect != null)
            {
                c.rect.anchoredPosition = c.startAnchoredPos + offset;
                c.rect.localRotation = Quaternion.Euler(0f, 0f, rot);
            }
            else
            {
                c.tr.localPosition = c.startLocalPos + (Vector3)offset;
                c.tr.localRotation = Quaternion.Euler(0f, 0f, rot);
            }
        }
    }

    // If you add/remove clouds at runtime:
    public void RefreshCloudList()
    {
        GatherClouds();
        ApplyInitialColorJitter();
    }
}
