using UnityEngine;

public class MapToModelActivator : MonoBehaviour
{
    [Header("Core Objects")]
    public GameObject mapRoot;                 // 2D map parent
    public GameObject backButton;              // UI Back button (disable by default)

    [Header("Model Source (pick ONE)")]
    [Tooltip("If provided, this in-scene object (disabled by default) will be enabled and tweened. If left empty, the prefab path is used.")]
    public GameObject sceneModel;              // Use a DISABLED object already placed in the scene
    [Tooltip("If no sceneModel is set, this prefab will be instantiated once, then reused.")]
    public GameObject modelPrefab;             // Optional: prefab to instantiate

    [Header("Extra UI to Hide/Show")]
    public GameObject[] hideWhenModelShown;
    public GameObject[] showWhenModelShown;

    [Header("LeanTween (scale)")]
    public Vector3 targetScale = Vector3.one;  // Final size you want (set in Inspector)
    public float tweenInDuration = 0.35f;
    public float tweenOutDuration = 0.25f;     // (kept but unused in simple back)
    public LeanTweenType easeIn = LeanTweenType.easeOutBack;
    public LeanTweenType easeOut = LeanTweenType.easeInQuad; // (kept but unused in simple back)
    public bool animateOutOnBack = true;       // (kept but unused in simple back)

    private GameObject _activeModel;           // sceneModel or spawned prefab instance

    public void ShowModel()
    {
        if (mapRoot) mapRoot.SetActive(false);
        ToggleArray(hideWhenModelShown, false);
        ToggleArray(showWhenModelShown, true);
        if (backButton) backButton.SetActive(true);

        // Choose model source
        if (sceneModel != null)
        {
            _activeModel = sceneModel;
            LeanTween.cancel(_activeModel);
            _activeModel.SetActive(true);
            _activeModel.transform.localScale = Vector3.zero;
            LeanTween.scale(_activeModel, targetScale, tweenInDuration).setEase(easeIn);
        }
        else if (_activeModel == null) // instantiate prefab once and reuse
        {
            if (modelPrefab == null)
            {
                Debug.LogWarning("[MapToModelActivator] No sceneModel or modelPrefab assigned.");
                return;
            }
            _activeModel = Instantiate(modelPrefab);
            _activeModel.transform.localScale = Vector3.zero;
            LeanTween.scale(_activeModel, targetScale, tweenInDuration).setEase(easeIn);
        }
        else
        {
            // Re-show existing instance
            LeanTween.cancel(_activeModel);
            _activeModel.SetActive(true);
            _activeModel.transform.localScale = Vector3.zero;
            LeanTween.scale(_activeModel, targetScale, tweenInDuration).setEase(easeIn);
        }
    }

    public void ShowMap()
    {
        if (_activeModel != null)
        {
            // Stop any running tweens on the model (e.g., scale-in) and DISABLE immediately.
            LeanTween.cancel(_activeModel);
            _activeModel.SetActive(false);
        }

        ToggleArray(hideWhenModelShown, true);
        ToggleArray(showWhenModelShown, false);

        if (mapRoot) mapRoot.SetActive(true);
        if (backButton) backButton.SetActive(false);
    }

    private void ToggleArray(GameObject[] targets, bool state)
    {
        if (targets == null) return;
        foreach (var go in targets)
            if (go) go.SetActive(state);
    }
}
