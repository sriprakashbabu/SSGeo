using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class PulsatingImage : MonoBehaviour
{
    [Header("Pulse Settings")]
    public float minAlpha = 0.3f; // Minimum opacity
    public float maxAlpha = 1f;   // Maximum opacity
    public float pulseSpeed = 2f; // How fast it pulses

    private Image image;
    private Color originalColor;
    private float t;

    void Awake()
    {
        image = GetComponent<Image>();
        originalColor = image.color;
        t = 0f;
    }

    void Update()
    {
        // Use PingPong to smoothly oscillate alpha
        t += Time.unscaledDeltaTime * pulseSpeed;
        float alpha = Mathf.Lerp(minAlpha, maxAlpha, Mathf.PingPong(t, 1f));

        Color c = originalColor;
        c.a = alpha;
        image.color = c;
    }
}
