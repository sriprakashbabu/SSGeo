using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class MapDropdownSelector : MonoBehaviour
{
    [System.Serializable]
    public class MapEntry
    {
        public GameObject mapGameObject;
        public string mapTitle = "New Map";
        [TextArea(3, 8)]
        public string mapBodyText = "Select a feature on this map to learn more.";

        public GameObject associatedToggleGroup;

        // --- ADDED: Field for the optional default toggle ---
        [Tooltip("The toggle to automatically select when this map is activated. Leave null for none.")]
        public Toggle defaultToggle;
    }

    [Header("Manager References")]
    public InteractionManager interactionManager;
    public UIManager uiManager;

    [Header("UI")]
    public TMP_Dropdown dropdown;
    public List<MapEntry> mapEntries = new List<MapEntry>();

    [Header("Tween Settings")]
    public float transitionDuration = 0.3f;
    public LeanTweenType easeOutType = LeanTweenType.easeOutExpo;
    public LeanTweenType easeInType = LeanTweenType.easeInExpo;

    GameObject currentMap;
    bool isTransitioning;
    int activeTweenId = -1;

    void Start()
    {
        if (dropdown == null || mapEntries.Count == 0)
        {
            Debug.LogWarning("MapDropdownSelector: set the dropdown and map entries!", this);
            enabled = false;
            return;
        }

        dropdown.onValueChanged.AddListener(RequestMapChange);

        foreach (var e in mapEntries)
        {
            if (e.mapGameObject != null)
            {
                var cg = e.mapGameObject.GetComponent<CanvasGroup>() ?? e.mapGameObject.AddComponent<CanvasGroup>();
                cg.alpha = 0;
                cg.interactable = false;
                cg.blocksRaycasts = false;
                e.mapGameObject.SetActive(false);
            }

            if (e.associatedToggleGroup != null)
            {
                e.associatedToggleGroup.SetActive(false);
            }
        }

        RequestMapChange(dropdown.value);
    }

    public void SelectMapByIndex(int index)
    {
        if (index < 0 || index >= mapEntries.Count) return;
        dropdown.SetValueWithoutNotify(index);
        RequestMapChange(index);
    }

    void RequestMapChange(int targetIndex)
    {
        if (!enabled) return;
        StartCoroutine(ChangeMapRoutine(targetIndex));
    }

    IEnumerator ChangeMapRoutine(int targetIndex)
    {
        if (!ModelActivator.IsIdle && interactionManager != null && interactionManager.CurrentlySelectedFeature != null)
        {
            var activeModel = interactionManager.CurrentlySelectedFeature.GetComponent<ModelActivator>();
            activeModel?.Deactivate();
        }
        while (!ModelActivator.IsIdle) yield return null;

        interactionManager?.ClearCurrentSelection();
        SwitchMapImmediate(targetIndex);
    }

    void SwitchMapImmediate(int index)
    {
        if (index < 0 || index >= mapEntries.Count) return;

        for (int i = 0; i < mapEntries.Count; i++)
        {
            var entry = mapEntries[i];
            if (entry.associatedToggleGroup != null)
            {
                bool shouldBeActive = (i == index);
                entry.associatedToggleGroup.SetActive(shouldBeActive);

                // --- ADDED: Set the default toggle state for the newly activated group ---
                if (shouldBeActive)
                {
                    SetDefaultToggleState(entry.associatedToggleGroup, entry.defaultToggle);
                }
            }
        }

        if (uiManager != null)
        {
            var mapInfo = mapEntries[index];
            uiManager.DisplayInformation(mapInfo.mapBodyText, mapInfo.mapTitle);
        }

        var newMap = mapEntries[index].mapGameObject;
        if (newMap == currentMap) return;

        if (isTransitioning) LeanTween.cancel(activeTweenId);
        isTransitioning = true;
        var oldMap = currentMap;
        currentMap = newMap;
        System.Action afterFadeOut = () =>
        {
            if (oldMap != null)
            {
                var oldCg = oldMap.GetComponent<CanvasGroup>();
                if (oldCg != null)
                {
                    oldCg.interactable = false;
                    oldCg.blocksRaycasts = false;
                }
                oldMap.SetActive(false);
            }
            FadeInNewMap(newMap);
        };
        if (oldMap != null && oldMap.activeSelf)
        {
            var oldCg = oldMap.GetComponent<CanvasGroup>();
            activeTweenId = LeanTween.alphaCanvas(oldCg, 0f, transitionDuration)
                            .setEase(easeOutType)
                            .setOnComplete(afterFadeOut)
                            .id;
        }
        else
        {
            afterFadeOut();
        }
    }

    void FadeInNewMap(GameObject mapGO)
    {
        if (mapGO == null)
        {
            isTransitioning = false;
            currentMap = null;
            return;
        }
        mapGO.SetActive(true);
        mapGO.GetComponent<ImageZoomer>()?.ResetView();
        var cg = mapGO.GetComponent<CanvasGroup>();
        cg.alpha = 0;
        cg.interactable = false;
        cg.blocksRaycasts = false;
        activeTweenId = LeanTween.alphaCanvas(cg, 1f, transitionDuration)
                        .setEase(easeInType)
                        .setOnComplete(() =>
                        {
                            cg.interactable = true;
                            cg.blocksRaycasts = true;
                            isTransitioning = false;
                        })
                        .id;
    }

    // --- ADDED: Helper methods to set the default toggle state ---
    private void SetDefaultToggleState(GameObject groupParent, Toggle defaultToggle)
    {
        if (groupParent == null) return;
        StartCoroutine(DelayedSetDefaultToggle(groupParent, defaultToggle));
    }

    private IEnumerator DelayedSetDefaultToggle(GameObject groupParent, Toggle defaultToggle)
    {
        yield return null;

        if (defaultToggle != null)
        {
            defaultToggle.isOn = true;
        }
        else
        {
            var toggleGroup = groupParent.GetComponentInChildren<ToggleGroup>();
            if (toggleGroup != null)
            {
                toggleGroup.SetAllTogglesOff();
            }
            else
            {
                Toggle[] toggles = groupParent.GetComponentsInChildren<Toggle>(true);
                foreach (var toggle in toggles)
                {
                    toggle.isOn = false;
                }
            }
        }
    }

    void OnDestroy()
    {
        if (dropdown != null) dropdown.onValueChanged.RemoveListener(RequestMapChange);
        if (isTransitioning) LeanTween.cancel(activeTweenId);
    }
}