using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

/// Drives Continents ⇆ Oceans selection without a ToggleGroup.
/// Put this on a “Manager” object and wire the fields in the Inspector.
public class ContinentOceanToggleController : MonoBehaviour
{
    [Header("UI Toggles (no ToggleGroup needed)")]
    [SerializeField] private Toggle continentsToggle;   // Continents check-box
    [SerializeField] private Toggle oceansToggle;       // Oceans check-box

    [Header("Scene Roots")]
    [SerializeField] private Transform continentsRoot;  // parent of all continent meshes + colliders
    [SerializeField] private Transform oceansRoot;      // parent of all ocean meshes + colliders

    [Header("Animation")]
    [SerializeField] private float inactiveScale = 0.9f;
    [SerializeField] private float activeScale = 1f;
    [SerializeField] private float tweenTime = 0.25f;
    [SerializeField] private LeanTweenType ease = LeanTweenType.easeOutCubic;

    /* internal caches ------------------------------------------------------ */
    private readonly List<Collider> continentCols = new();
    private readonly List<Collider> oceanCols = new();

    private void Awake()
    {
        // hook up listeners
        continentsToggle.onValueChanged.AddListener(OnContinentsChanged);
        oceansToggle.onValueChanged.AddListener(OnOceansChanged);

        // cache colliders once
        continentCols.AddRange(continentsRoot.GetComponentsInChildren<Collider>(true));
        oceanCols.AddRange(oceansRoot.GetComponentsInChildren<Collider>(true));

        // start both roots small
        continentsRoot.localScale = oceansRoot.localScale = Vector3.one * inactiveScale;

        // force an initial state (if designer forgot to tick exactly one)
        if (!continentsToggle.isOn && !oceansToggle.isOn) continentsToggle.isOn = true;
        ApplyState();          // run once to set colliders / scale
    }

    /* -------- toggle callbacks ------------------------------------------- */
    private void OnContinentsChanged(bool isOn)
    {
        if (isOn)
        {
            // turn the other OFF without triggering its callback loop
            oceansToggle.SetIsOnWithoutNotify(false);
            ApplyState();
        }
    }

    private void OnOceansChanged(bool isOn)
    {
        if (isOn)
        {
            continentsToggle.SetIsOnWithoutNotify(false);
            ApplyState();
        }
    }

    /* -------- core behaviour --------------------------------------------- */
    private void ApplyState()
    {
        bool continentsActive = continentsToggle.isOn;

        // animate roots
        TweenRoot(continentsRoot, continentsActive ? activeScale : inactiveScale);
        TweenRoot(oceansRoot, continentsActive ? inactiveScale : activeScale);

        // colliders
        SetColliders(continentCols, continentsActive);
        SetColliders(oceanCols, !continentsActive);
    }

    private void TweenRoot(Transform t, float targetScale)
    {
        LeanTween.scale(t.gameObject, Vector3.one * targetScale, tweenTime)
                 .setEase(ease);
    }

    private static void SetColliders(IEnumerable<Collider> list, bool enabled)
    {
        foreach (var c in list)
            if (c) c.enabled = enabled;
    }
}
