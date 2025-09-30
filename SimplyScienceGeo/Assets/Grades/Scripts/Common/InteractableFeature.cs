using UnityEngine;

/// <summary>
/// A simple data container that lives on every clickable object.
/// It holds the information and visual properties for that specific feature.
/// </summary>
public class InteractableFeature : MonoBehaviour
{
    [Header("Feature Data")]
    [Tooltip("The name of the feature, e.g., 'Equator' or 'France'.")]
    public string featureName = "New Feature";

    [Tooltip("The detailed text to display in the UI when this feature is selected.")]
    [TextArea(3, 10)]
    public string informationText;

    [Header("Appearance Properties")]
    [Tooltip("The color the object will pulse or highlight to when selected.")]
    public Color highlightColor = Color.yellow;

    [Tooltip("Check this to override the material's default color with the 'Initial Color' below.")]
    public bool overrideInitialColor = false; // The new boolean flag

    [Tooltip("The unique default color for this specific object. Only used if 'Override Initial Color' is checked.")]
    public Color initialColor = Color.white; // The new color field

    // --- Caching for performance (private) ---
    private Renderer _renderer;
    private Color _originalColor; // This will now store either the material's color OR the override color
    private MaterialPropertyBlock _propertyBlock;
    private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");

    // Public getters allow other scripts to access private cached components without modifying them.
    public Renderer FeatureRenderer => _renderer;
    public Color OriginalColor => _originalColor;
    public MaterialPropertyBlock PropertyBlock => _propertyBlock;

    void Awake()
    {
        _renderer = GetComponent<Renderer>();
        if (_renderer != null)
        {
            _propertyBlock = new MaterialPropertyBlock();
            _renderer.GetPropertyBlock(_propertyBlock);

            // --- THIS IS THE NEW LOGIC ---
            if (overrideInitialColor)
            {
                // If we are overriding, this IS our original color.
                _originalColor = initialColor;
                // Apply this initial color to the mesh right away.
                _propertyBlock.SetColor(BaseColorId, _originalColor);
                _renderer.SetPropertyBlock(_propertyBlock);
            }
            else
            {
                // Otherwise, get the color from the material just like before.
                if (_renderer.sharedMaterial.HasProperty(BaseColorId))
                {
                    _originalColor = _renderer.sharedMaterial.GetColor(BaseColorId);
                }
                else
                {
                    _originalColor = Color.white;
                }
            }
        }
    }
}