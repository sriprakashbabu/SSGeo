using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class AirMoleculeColor : MonoBehaviour
{
    [Header("Color Settings")]
    public Color baseColor = Color.white; // starting color
    [Range(0f, 1f)] public float minAlpha = 0.1f; // lowest transparency
    [Range(0f, 1f)] public float maxAlpha = 0.4f; // highest transparency
    public float colorVariation = 0.05f; // small hue shift to randomize color

    private Material matInstance;

    void Start()
    {
        // Get a unique instance of the material so we don't affect all spheres
        matInstance = GetComponent<Renderer>().material;

        // Slightly randomize color to make them feel more organic
        Color variedColor = baseColor;

        // Randomize hue slightly for variety
        float h, s, v;
        Color.RGBToHSV(baseColor, out h, out s, out v);
        h += Random.Range(-colorVariation, colorVariation);
        variedColor = Color.HSVToRGB(Mathf.Repeat(h, 1f), s, v);

        // Randomize transparency
        float alpha = Random.Range(minAlpha, maxAlpha);
        variedColor.a = alpha;

        matInstance.color = variedColor;
    }
}
