using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class SelectableImageToggle : MonoBehaviour
{
    [Header("Toggle Reference")]
    public Toggle toggle;

    [Header("Transition Settings")]
    [Tooltip("How long (in seconds) the color change takes.")]
    public float fadeDuration = 0.25f;

    [Header("Color Change Settings")]
    [Tooltip("Define groups of images and the color they should turn when this toggle is ON.")]
    public List<ColorGroup> colorGroups;

    [Tooltip("Check this if you want images to go back to white/original when you uncheck the toggle.")]
    public bool resetColorsOnDeselect = true;

    [Header("Objects to Disable On Select")]
    public GameObject[] objectsToDisable;

    [Header("Objects to Enable On Select")]
    public GameObject[] objectsToEnable;

    // Internal storage to remember original colors
    private Dictionary<Image, Color> originalColorCache = new Dictionary<Image, Color>();

    // ---------------------------------------------------------
    // STATIC FIX: We must track WHO (which script) started the fade
    // ---------------------------------------------------------
    struct FadeTracker
    {
        public Coroutine coroutine;
        public SelectableImageToggle ownerScript;
    }

    // Maps an Image to the specific FadeTracker that is currently animating it
    private static Dictionary<Image, FadeTracker> activeFades = new Dictionary<Image, FadeTracker>();

    private bool initialized = false;

    [System.Serializable]
    public class ColorGroup
    {
        public string groupName = "New Color Group";
        public Color targetColor = Color.white;
        public Image[] imagesToColor;
    }

    void Start()
    {
        if (toggle == null) toggle = GetComponent<Toggle>();

        InitializeOriginalColors();

        if (toggle != null)
        {
            toggle.onValueChanged.AddListener(OnToggleChanged);
            // Run once at start to set initial state (Instant, no fade on startup)
            OnToggleChanged(toggle.isOn, true);
        }
    }

    private void InitializeOriginalColors()
    {
        if (initialized) return;

        foreach (var group in colorGroups)
        {
            foreach (var img in group.imagesToColor)
            {
                if (img != null && !originalColorCache.ContainsKey(img))
                {
                    originalColorCache.Add(img, img.color);
                }
            }
        }
        initialized = true;
    }

    public void OnToggleChanged(bool isOn)
    {
        OnToggleChanged(isOn, false);
    }

    // Overload to allow instant setting during Start()
    private void OnToggleChanged(bool isOn, bool instant)
    {
        // 1. Handle Activation/Deactivation
        foreach (var obj in objectsToDisable) if (obj) obj.SetActive(!isOn);
        foreach (var obj in objectsToEnable) if (obj) obj.SetActive(isOn);

        // 2. Handle Coloring
        if (isOn)
        {
            ApplyActiveColors(instant);
        }
        else
        {
            if (resetColorsOnDeselect)
            {
                RestoreOriginalColors(instant);
            }
        }
    }

    private void ApplyActiveColors(bool instant)
    {
        foreach (var group in colorGroups)
        {
            foreach (var img in group.imagesToColor)
            {
                if (img != null) StartColorFade(img, group.targetColor, instant);
            }
        }
    }

    private void RestoreOriginalColors(bool instant)
    {
        foreach (var group in colorGroups)
        {
            foreach (var img in group.imagesToColor)
            {
                if (img != null && originalColorCache.ContainsKey(img))
                {
                    StartColorFade(img, originalColorCache[img], instant);
                }
            }
        }
    }

    // ─────────────────────────────────────────────
    // Fading Logic (Fixed for Multiple Toggles)
    // ─────────────────────────────────────────────

    private void StartColorFade(Image targetImage, Color targetColor, bool instant)
    {
        // 1. Check if ANY script is currently fading this specific image
        if (activeFades.ContainsKey(targetImage))
        {
            FadeTracker tracker = activeFades[targetImage];

            // If the script that started the previous fade is still alive, tell IT to stop the coroutine
            if (tracker.ownerScript != null && tracker.coroutine != null)
            {
                tracker.ownerScript.StopCoroutine(tracker.coroutine);
            }

            activeFades.Remove(targetImage);
        }

        // 2. If instant, just set color and exit
        if (instant)
        {
            targetImage.color = targetColor;
            return;
        }

        // 3. Start the new fade on THIS script
        // We must check if gameObject is active, otherwise StartCoroutine fails
        if (this.gameObject.activeInHierarchy)
        {
            Coroutine newRoutine = StartCoroutine(FadeRoutine(targetImage, targetColor));

            // 4. Register this fade in the static dictionary
            FadeTracker newTracker = new FadeTracker
            {
                coroutine = newRoutine,
                ownerScript = this
            };
            activeFades.Add(targetImage, newTracker);
        }
        else
        {
            // Fallback if this object is disabled but we still tried to run logic
            targetImage.color = targetColor;
        }
    }

    private System.Collections.IEnumerator FadeRoutine(Image targetImage, Color endColor)
    {
        float time = 0;
        float duration = fadeDuration;
        Color startColor = targetImage.color;

        while (time < duration)
        {
            if (targetImage == null) yield break;

            time += Time.deltaTime;
            targetImage.color = Color.Lerp(startColor, endColor, time / duration);
            yield return null;
        }

        targetImage.color = endColor;

        // Clean up dictionary if WE are still the ones controlling it
        if (activeFades.ContainsKey(targetImage) && activeFades[targetImage].ownerScript == this)
        {
            activeFades.Remove(targetImage);
        }
    }

    private void OnDisable()
    {
        // Safety: If this button is disabled while fading, ensure we don't leave broken references
        // We don't clear the color, but we stop tracking the coroutine since Unity kills it anyway
    }
}