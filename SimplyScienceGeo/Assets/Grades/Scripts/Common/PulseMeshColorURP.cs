using UnityEngine;

public class PulseMeshColorURP : MonoBehaviour
{
    [Header("Pulse Settings")]
    public bool pulseAlpha = true;
    public bool pulseColor = false;

    [Header("Alpha Pulse Range")]
    [Range(0f, 1f)] public float minAlpha = 0.2f;
    [Range(0f, 1f)] public float maxAlpha = 1f;

    [Header("Color Pulse Range")]
    public Color startColor = Color.white;
    public Color endColor = Color.red;

    [Header("Timing")]
    public float pulseSpeed = 2f;

    private Renderer _renderer;
    private MaterialPropertyBlock _block;

    private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");

    void Awake()
    {
        _renderer = GetComponent<Renderer>();
        if (_renderer == null)
        {
            Debug.LogError("PulseMeshColorURP: No Renderer found.");
            enabled = false;
            return;
        }

        _block = new MaterialPropertyBlock();
    }

    void Update()
    {
        Pulse();
    }

    private void Pulse()
    {
        float t = (Mathf.Sin(Time.time * pulseSpeed) + 1f) * 0.5f; // 0 → 1

        _renderer.GetPropertyBlock(_block);

        Color current = _block.GetColor(BaseColorId);

        // If color pulse is enabled
        if (pulseColor)
        {
            current = Color.Lerp(startColor, endColor, t);
        }

        // If alpha pulse is enabled
        if (pulseAlpha)
        {
            float a = Mathf.Lerp(minAlpha, maxAlpha, t);
            current.a = a;
        }

        _block.SetColor(BaseColorId, current);
        _renderer.SetPropertyBlock(_block);
    }

    // External override
    public void SetColor(Color newColor)
    {
        _renderer.GetPropertyBlock(_block);
        _block.SetColor(BaseColorId, newColor);
        _renderer.SetPropertyBlock(_block);
    }
}
