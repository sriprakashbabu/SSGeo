using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
// No specific 'using DentedPixel;' is usually required for LeanTween's core functionalities
// as it often uses extension methods or is globally accessible.

public class MapSelector : MonoBehaviour
{
    [System.Serializable]
    public class MapTogglePair
    {
        public Toggle toggle;
        public GameObject mapGameObject; // This is the ZoomableMapImage_X GameObject
        public bool initiallyActive = false;
    }

    public List<MapTogglePair> mapTogglePairs = new List<MapTogglePair>();
    public ToggleGroup toggleGroup;

    [Header("Tween Settings")]
    public float transitionDuration = 0.3f; // Duration for fade in/out
    public LeanTweenType easeOutType = LeanTweenType.easeOutExpo;
    public LeanTweenType easeInType = LeanTweenType.easeInExpo;

    private GameObject currentActiveMapGO = null;
    private bool isTransitioning = false;
    private int activeTweenId = -1; // To keep track of active tweens for cancellation


    void Start()
    {
        if (mapTogglePairs.Count == 0)
        {
            Debug.LogWarning("MapSelector: No map toggle pairs assigned.", this);
            return;
        }

        if (toggleGroup != null)
        {
            foreach (var pair in mapTogglePairs)
            {
                if (pair.toggle != null) pair.toggle.group = toggleGroup;
            }
        }

        for (int i = 0; i < mapTogglePairs.Count; i++)
        {
            int index = i;
            if (mapTogglePairs[index].toggle != null)
            {
                mapTogglePairs[index].toggle.onValueChanged.AddListener((isOn) =>
                {
                    // Pass the toggle itself to check if it's the one initiating the change.
                    HandleToggleValueChanged(mapTogglePairs[index], isOn, mapTogglePairs[index].toggle);
                });
            }
        }
        InitialMapSetup();
    }

    void InitialMapSetup()
    {
        foreach (var pair in mapTogglePairs)
        {
            if (pair.mapGameObject != null)
            {
                // Ensure CanvasGroup exists and set initial alpha for inactive maps
                CanvasGroup cg = pair.mapGameObject.GetComponent<CanvasGroup>();
                if (cg == null) cg = pair.mapGameObject.AddComponent<CanvasGroup>();
                cg.alpha = 0f; // Start all maps transparent
                cg.interactable = false;
                cg.blocksRaycasts = false;
                pair.mapGameObject.SetActive(false); // Initially inactive
            }
            if (pair.toggle != null) pair.toggle.SetIsOnWithoutNotify(false);
        }

        MapTogglePair pairToActivate = null;
        for (int i = 0; i < mapTogglePairs.Count; i++)
        {
            if (mapTogglePairs[i].initiallyActive && IsValidPair(mapTogglePairs[i]))
            {
                pairToActivate = mapTogglePairs[i];
                break;
            }
        }

        if (pairToActivate == null && mapTogglePairs.Count > 0)
        {
            foreach (var pair in mapTogglePairs)
            {
                if (IsValidPair(pair))
                {
                    pairToActivate = pair;
                    break;
                }
            }
        }

        if (pairToActivate != null)
        {
            currentActiveMapGO = pairToActivate.mapGameObject;
            if (currentActiveMapGO != null)
            {
                currentActiveMapGO.SetActive(true); // Activate GO before setting alpha/resetting
                ImageZoomer zoomer = currentActiveMapGO.GetComponent<ImageZoomer>();
                if (zoomer != null) zoomer.ResetView();

                CanvasGroup cg = currentActiveMapGO.GetComponent<CanvasGroup>(); // Should exist from loop above
                cg.alpha = 1f; // Make the initial map fully visible
                cg.interactable = true;
                cg.blocksRaycasts = true;
            }
            // Set toggle.isOn = true *after* setting currentActiveMapGO and its state.
            // This ensures that if OnToggleValueChanged is called, it sees the correct current state.
            pairToActivate.toggle.SetIsOnWithoutNotify(true); // Use SetIsOnWithoutNotify for initial setup
        }
    }

    bool IsValidPair(MapTogglePair pair)
    {
        return pair != null && pair.toggle != null && pair.mapGameObject != null;
    }


    // Added sourceToggle to know which toggle initiated the change
    void HandleToggleValueChanged(MapTogglePair changedPair, bool isOn, Toggle sourceToggle)
    {
        // Only process if the toggle is being turned ON
        // And if the source of this event is actually the toggle that is now 'on'
        // (prevents issues if SetIsOnWithoutNotify internally triggers something unintended)
        if (!isOn || !sourceToggle.isOn)
        {
            return;
        }

        GameObject newMapGO = changedPair.mapGameObject;

        // If the selected map is already the active one and visible, do nothing.
        if (newMapGO == currentActiveMapGO && newMapGO != null && newMapGO.activeSelf)
        {
            // Optional: you could force a ResetView here if you want a re-click to reset
            // ImageZoomer zoomer = newMapGO.GetComponent<ImageZoomer>();
            // if (zoomer != null) zoomer.ResetView();
            return;
        }

        // If a transition is already happening, cancel the old one and proceed.
        if (isTransitioning)
        {
            LeanTween.cancel(activeTweenId); // Cancel the specific tween
            // Potentially force complete the state of the map that was being animated.
            // For simplicity, we'll just start the new transition.
        }
        isTransitioning = true;

        GameObject oldMapToFadeOut = currentActiveMapGO;
        currentActiveMapGO = newMapGO; // Update current active map tracker

        // --- Fade Out Old Map ---
        System.Action onOldMapFadedOutComplete = () =>
        {
            if (oldMapToFadeOut != null)
            {
                CanvasGroup oldCG = oldMapToFadeOut.GetComponent<CanvasGroup>();
                if (oldCG != null)
                {
                    oldCG.interactable = false;
                    oldCG.blocksRaycasts = false;
                }
                oldMapToFadeOut.SetActive(false);
            }
            ActivateAndFadeInNewMap(newMapGO);
        };

        if (oldMapToFadeOut != null && oldMapToFadeOut.activeSelf)
        {
            CanvasGroup oldMapCG = oldMapToFadeOut.GetComponent<CanvasGroup>();
            if (oldMapCG == null) oldMapCG = oldMapToFadeOut.AddComponent<CanvasGroup>(); // Should exist

            activeTweenId = LeanTween.alphaCanvas(oldMapCG, 0f, transitionDuration)
                .setEase(easeOutType)
                .setOnComplete(onOldMapFadedOutComplete)
                .id;
        }
        else
        {
            // No old map to fade, or it wasn't active. Directly proceed to new map.
            onOldMapFadedOutComplete();
        }

        // Ensure other UI toggles are correctly set to off (ToggleGroup usually handles this)
        foreach (var pair in mapTogglePairs)
        {
            if (IsValidPair(pair) && pair.toggle != sourceToggle && pair.toggle.isOn)
            {
                pair.toggle.SetIsOnWithoutNotify(false);
            }
        }
    }

    void ActivateAndFadeInNewMap(GameObject newMapGO)
    {
        if (newMapGO == null)
        {
            isTransitioning = false;
            currentActiveMapGO = null; // No map is active if new one is null
            return;
        }

        newMapGO.SetActive(true); // Must be active to get components and run tweens

        ImageZoomer zoomer = newMapGO.GetComponent<ImageZoomer>();
        if (zoomer != null)
        {
            zoomer.ResetView(); // Reset before it becomes visible
        }

        CanvasGroup newMapCG = newMapGO.GetComponent<CanvasGroup>();
        if (newMapCG == null) newMapCG = newMapGO.AddComponent<CanvasGroup>();

        newMapCG.alpha = 0f; // Start fully transparent
        newMapCG.interactable = false; // Not interactable during fade-in start
        newMapCG.blocksRaycasts = false; // Not blocking raycasts initially

        activeTweenId = LeanTween.alphaCanvas(newMapCG, 1f, transitionDuration)
            .setEase(easeInType)
            .setOnComplete(() =>
            {
                if (newMapCG != null)
                { // Check if still valid
                    newMapCG.interactable = true; // Make interactable upon full visibility
                    newMapCG.blocksRaycasts = true;
                }
                isTransitioning = false;
            })
            .id;
    }

    void OnDestroy()
    {
        foreach (var pair in mapTogglePairs)
        {
            if (pair.toggle != null) pair.toggle.onValueChanged.RemoveAllListeners();
        }
        // It's good practice to cancel any active tweens if this object is destroyed
        if (isTransitioning) LeanTween.cancel(activeTweenId);
    }
}