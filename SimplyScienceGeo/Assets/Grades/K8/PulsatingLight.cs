using UnityEngine;

// This ensures that the script requires a Light component to be attached.
[RequireComponent(typeof(Light))]
public class PulsatingLight : MonoBehaviour
{
    // This will be our reference to the Light component
    private Light lightComponent;

    [Header("Pulsation Settings")]
    [Tooltip("The lowest intensity the light will dim to.")]
    public float minIntensity = 0.5f;

    [Tooltip("The highest intensity the light will brighten to.")]
    public float maxIntensity = 2.0f;

    [Tooltip("How fast the light pulsates (higher value = faster).")]
    public float pulsationSpeed = 1.0f;

    void Start()
    {
        // Get the Light component attached to this same GameObject
        lightComponent = GetComponent<Light>();
    }

    void Update()
    {
        // Calculate the pulsation
        // 1. Mathf.Sin() creates a smooth wave that goes from -1 to 1.
        //    We use Time.time * pulsationSpeed to make it move over time.
        float rawSinWave = Mathf.Sin(Time.time * pulsationSpeed);

        // 2. Remap the -1 to 1 range to a 0 to 1 range.
        //    (-1 + 1) / 2 = 0
        //    ( 1 + 1) / 2 = 1
        float normalizedPulse = (rawSinWave + 1.0f) / 2.0f;

        // 3. Use Mathf.Lerp() to interpolate between your min and max intensity
        //    based on the normalized (0-1) pulse value.
        float targetIntensity = Mathf.Lerp(minIntensity, maxIntensity, normalizedPulse);

        // 4. Apply the final intensity to the light
        lightComponent.intensity = targetIntensity;
    }
}