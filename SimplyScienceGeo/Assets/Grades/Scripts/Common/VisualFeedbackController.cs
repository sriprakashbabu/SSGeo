using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The "Appearance". Manages all visual feedback for selected objects,
/// such as starting and stopping a pulsing highlight effect.
/// </summary>
public class VisualFeedbackController : MonoBehaviour
{
    [Header("Pulse Settings")]
    public float pulseCyclesPerSecond = 0.5f;

    private Dictionary<InteractableFeature, Coroutine> _runningHighlights = new Dictionary<InteractableFeature, Coroutine>();
    private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");

    public void StartHighlight(InteractableFeature feature)
    {
        if (feature == null || feature.FeatureRenderer == null) return;

        // If this feature is already being highlighted, don't start another coroutine.
        if (_runningHighlights.ContainsKey(feature)) return;

        Coroutine pulseCoroutine = StartCoroutine(PulseRoutine(feature));
        _runningHighlights[feature] = pulseCoroutine;
    }

    public void StopHighlight(InteractableFeature feature)
    {
        if (feature == null || !_runningHighlights.ContainsKey(feature)) return;

        // Stop the coroutine and remove it from the dictionary.
        StopCoroutine(_runningHighlights[feature]);
        _runningHighlights.Remove(feature);

        // IMPORTANT: Reset the object to its original color.
        feature.PropertyBlock.SetColor(BaseColorId, feature.OriginalColor);
        feature.FeatureRenderer.SetPropertyBlock(feature.PropertyBlock);
    }

    private IEnumerator PulseRoutine(InteractableFeature feature)
    {
        Renderer featureRenderer = feature.FeatureRenderer;
        MaterialPropertyBlock propertyBlock = feature.PropertyBlock;
        Color originalColor = feature.OriginalColor;
        Color highlightColor = feature.highlightColor;

        float speed = pulseCyclesPerSecond * 2f * Mathf.PI;

        while (true)
        {
            // Calculate a sine wave to smoothly transition between colors.
            float lerpFactor = (Mathf.Sin(Time.time * speed) + 1f) * 0.5f;
            propertyBlock.SetColor(BaseColorId, Color.Lerp(originalColor, highlightColor, lerpFactor));
            featureRenderer.SetPropertyBlock(propertyBlock);

            yield return null; // Wait for the next frame
        }
    }
}