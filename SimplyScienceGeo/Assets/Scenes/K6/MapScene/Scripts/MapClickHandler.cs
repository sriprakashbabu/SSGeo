using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Handles clicks on UI-based map areas and forwards them to the InteractionManager.
/// Attach this to a UI Image (can be fully transparent) that has an InteractableFeature.
/// </summary>
public class MapClickHandler : MonoBehaviour, IPointerClickHandler
{
    [Tooltip("Reference to the InteractionManager in the scene.")]
    public InteractionManager interactionManager;

    [Tooltip("The InteractableFeature associated with this clickable area.")]
    public InteractableFeature feature;

    /// <summary>
    /// Called automatically by Unity when this UI element is clicked.
    /// </summary>
    public void OnPointerClick(PointerEventData eventData)
    {
        if (interactionManager != null && feature != null)
        {
            interactionManager.SelectFeature(feature);
        }
        else
        {
            Debug.LogWarning($"MapClickHandler on {gameObject.name} is missing references.", this);
        }
    }
}
