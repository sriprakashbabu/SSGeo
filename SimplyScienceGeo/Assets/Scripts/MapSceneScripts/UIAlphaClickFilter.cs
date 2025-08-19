using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Forces UI Image raycasts to only register on pixels above a certain alpha threshold.
/// This prevents clicks on the transparent parts of the image.
/// </summary>
[RequireComponent(typeof(Image))]
public class UIAlphaClickFilter : MonoBehaviour
{
    [Range(0f, 1f)]
    public float alphaThreshold = 0.1f; // Pixels with alpha below this are ignored for clicks

    void Awake()
    {
        var img = GetComponent<Image>();

        // IMPORTANT: The sprite texture must be Read/Write enabled in the importer
        // and use Sprite (2D and UI) mode for this to work.
        img.alphaHitTestMinimumThreshold = alphaThreshold;
    }
}
