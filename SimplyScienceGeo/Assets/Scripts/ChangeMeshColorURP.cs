using UnityEngine;

public class ChangeMeshColorURP : MonoBehaviour
{
    private Renderer _renderer;
    private MaterialPropertyBlock _propertyBlock;

    [Tooltip("The color (including alpha for transparency) to apply to the mesh.")]
    public Color targetColor = new Color(1f, 0f, 0f, 1f); // Default to opaque red

    private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");

    void Awake()
    {
        _renderer = GetComponent<Renderer>();
        if (_renderer == null)
        {
            Debug.LogError("ChangeMeshColorURP: No Renderer found on this GameObject.", this);
            enabled = false;
            return;
        }
        _propertyBlock = new MaterialPropertyBlock();
    }

    void Start()
    {
        ApplyColor();
    }

    public void ApplyColor()
    {
        if (_renderer == null || _propertyBlock == null) return;

        _renderer.GetPropertyBlock(_propertyBlock);
        _propertyBlock.SetColor(BaseColorId, targetColor); // targetColor includes alpha
        _renderer.SetPropertyBlock(_propertyBlock);
    }

    public void SetColor(Color newColor)
    {
        targetColor = newColor; // newColor can have its own alpha value
        ApplyColor();
    }
}