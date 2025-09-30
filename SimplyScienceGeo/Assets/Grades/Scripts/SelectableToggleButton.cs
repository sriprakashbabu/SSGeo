using UnityEngine;
using UnityEngine.UI;

public class SelectableToggleButton : MonoBehaviour
{
    [Header("Toggle Reference")]
    public Toggle toggle;

    [Header("Feature Target (Main Representative)")]
    public InteractableFeature linkedFeature;

    [Header("Extra Meshes to Highlight (Overrides Any InteractableFeature)")]
    public Renderer[] extraMeshes;
    [Tooltip("Highlight colors for each extra mesh (same length as Extra Meshes)")]
    public Color[] extraHighlightColors;

    [Header("Objects to Disable On Select")]
    public GameObject[] objectsToDisable;

    [Header("Objects to Enable On Select")]
    public GameObject[] objectsToEnable;

    [Header("Managers")]
    public InteractionManager interactionManager;
    [Header("Optional Camera Focus")]
    public CameraFocusController cameraFocusController;

    [Header("Optional Globe Rotation Control")]
    public GlobeRotator globeRotator;
    public bool disableGlobeRotationWhenOn = false;

    private Color[] originalColors;
    private bool colorsInitialized = false;

    /// <summary>
    /// Ensures the originalColors array is populated.
    /// This prevents errors if other scripts call methods before Start() has run.
    /// </summary>
    private void EnsureColorsInitialized()
    {
        if (colorsInitialized)
        {
            return;
        }

        if (extraMeshes != null)
        {
            originalColors = new Color[extraMeshes.Length];
            for (int i = 0; i < extraMeshes.Length; i++)
            {
                if (extraMeshes[i] != null && extraMeshes[i].sharedMaterial != null && extraMeshes[i].sharedMaterial.HasProperty("_BaseColor"))
                {
                    originalColors[i] = extraMeshes[i].sharedMaterial.GetColor("_BaseColor");
                }
            }
        }
        else
        {
            originalColors = new Color[0]; // Ensure array is not null even if extraMeshes is
        }

        colorsInitialized = true;
    }

    void Start()
    {
        if (toggle == null || interactionManager == null)
        {
            Debug.LogWarning($"SelectableToggleButton setup missing references on {gameObject.name}");
            return;
        }

        EnsureColorsInitialized();

        toggle.onValueChanged.AddListener(OnToggleChanged);
    }

    private void OnToggleChanged(bool isOn)
    {
        EnsureColorsInitialized();   // safety

        // ─────────────────────────────
        // 1. Globe-rotation handling
        // ─────────────────────────────
        if (globeRotator != null)
            globeRotator.enabled = !disableGlobeRotationWhenOn || !isOn;

        // ─────────────────────────────
        // 2. Objects enable / disable
        // ─────────────────────────────
        foreach (var obj in objectsToDisable)
            if (obj) obj.SetActive(!isOn);          // disable on select

        foreach (var obj in objectsToEnable)
            if (obj) obj.SetActive(isOn);           // enable on select

        // ─────────────────────────────
        // 3. Mesh highlight colours
        // ─────────────────────────────
        for (int i = 0; i < extraMeshes.Length; i++)
        {
            var mesh = extraMeshes[i];
            if (mesh && mesh.sharedMaterial && mesh.sharedMaterial.HasProperty("_BaseColor"))
            {
                var col = isOn
                    ? (i < extraHighlightColors.Length ? extraHighlightColors[i] : Color.yellow)
                    : originalColors[i];

                mesh.material.SetColor("_BaseColor", col);
            }
        }

        // ─────────────────────────────
        // 4. Feature-driven UI / camera
        // ─────────────────────────────
        if (isOn)
        {
            if (linkedFeature != null)
            {
                interactionManager.SelectFeature(linkedFeature);
                cameraFocusController?.TriggerFocus();
            }
        }
        else
        {
            if (linkedFeature != null &&
                interactionManager.CurrentlySelectedFeature == linkedFeature)
            {
                interactionManager.ResetToDefaultUI();
            }
        }
    }

    public void ForceResetHighlight()
    {
        EnsureColorsInitialized(); // This will now safely initialize colors if needed

        // Reset mesh colors
        for (int i = 0; i < extraMeshes.Length; i++)
        {
            if (extraMeshes[i] != null && extraMeshes[i].sharedMaterial.HasProperty("_BaseColor"))
            {
                extraMeshes[i].material.SetColor("_BaseColor", originalColors[i]);
            }
        }
    }
}