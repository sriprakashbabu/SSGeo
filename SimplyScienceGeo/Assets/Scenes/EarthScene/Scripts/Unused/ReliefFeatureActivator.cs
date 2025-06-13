// ReliefFeatureActivator.cs  v4.2  (globe now SCALES down)
//
// – Globe now scales to zero instead of moving off-screen.
// – The 'Off-screen Offset' property has been removed.
// – Conflict-handling for Animator/Rotator scripts is retained.
//
// Replace your old script with this one. No Inspector changes are needed
// if you already assigned the Animator and GlobeRotator from the last version.

using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Collider))]
public class ReliefFeatureActivator : MonoBehaviour
{
    /* ─── inspector ─── */

    [Header("Scene References")]
    [SerializeField] private Transform globeRoot;
    [SerializeField] private Transform detailObject;
    [SerializeField] private Button backButton;

    [Header("Components to Disable on Detail")]
    [Tooltip("Assign the globe's Animator component here.")]
    [SerializeField] private Animator globeAnimator;
    [Tooltip("Assign the globe's GlobeRotator script here.")]
    [SerializeField] private GlobeRotator globeRotator;

    [Header("Tween Settings")]
    [SerializeField] private float tweenTime = 0.4f;
    [SerializeField] private LeanTweenType easeType = LeanTweenType.easeOutCubic;

    [Header("Detail Scale")]
    [SerializeField] private Vector3 targetScale = Vector3.zero;

    [Header("UI Elements to Hide")]
    [Tooltip("Do NOT drag the Back-button here – it gets handled automatically.")]
    [SerializeField] private GameObject[] uiElementsToHideOnDetail;

    [Header("Skybox")]
    [SerializeField] private Material detailSkybox;

    /* ─── internals ─── */

    private static ReliefFeatureActivator _current;

    private Vector3 _globeStartScale; // CHANGED: We now store start scale instead of position
    private Vector3 _detailTargetScale;

    private int globeTweenId = -1;
    private int detailTweenId = -1;
    private bool isBusy = false;

    private static Material _originalSceneSkybox;
    private static bool _originalSkyboxCaptured = false;

    /* ─── life-cycle ─── */

    private void Awake()
    {
        if (!globeRoot || !detailObject || !backButton)
        { Debug.LogError($"{name}: Missing critical references (Globe Root, Detail Object, or Back Button)"); enabled = false; return; }

        // CHANGED: We capture the globe's starting scale. Position is no longer needed.
        _globeStartScale = globeRoot.localScale;
        _detailTargetScale = targetScale == Vector3.zero ? detailObject.localScale : targetScale;

        detailObject.localScale = Vector3.zero;
        detailObject.gameObject.SetActive(false);
        backButton.gameObject.SetActive(false);

        backButton.onClick.AddListener(HandleBackClicked);
    }

    /* ─── public entry ─── */

    public void ActivateDetail()
    {
        if (isBusy) return;

        if (_current && _current != this) _current.CloseImmediate();
        if (_current == this) return;

        _current = this;
        isBusy = true;

        if (globeAnimator) globeAnimator.enabled = false;
        if (globeRotator) globeRotator.enabled = false;

        ToggleUI(false);
        backButton.gameObject.SetActive(true);
        UpdateSkybox();

        // --- ANIMATION LOGIC CHANGED ---
        /* GLOBE scale down */
        globeRoot.gameObject.SetActive(true);

        globeTweenId = LeanTween.scale(globeRoot.gameObject, Vector3.zero, tweenTime)
            .setEase(easeType)
            .setOnComplete(() =>
            {
                globeRoot.gameObject.SetActive(false); // Still disable to save render cost
                globeTweenId = -1;
                if (detailTweenId == -1) isBusy = false;
            }).id;

        /* DETAIL scale up */
        detailObject.gameObject.SetActive(true);
        detailObject.localScale = Vector3.zero;

        detailTweenId = LeanTween.scale(detailObject.gameObject, _detailTargetScale, tweenTime)
            .setEase(easeType)
            .setOnComplete(() =>
            {
                detailTweenId = -1;
                if (globeTweenId == -1) isBusy = false;
            }).id;
    }

    /* ─── back button ─── */

    private void HandleBackClicked()
    {
        if (_current != this || isBusy) return;
        StartCloseAnimation();
    }

    /* ─── close helpers ─── */

    private void StartCloseAnimation()
    {
        isBusy = true;

        /* 1️⃣  IMMEDIATE SKYBOX REVERT */
        _current = null;          // we’re leaving “detail” mode right now
        UpdateSkybox();           // swap the skybox right away

        /* 2️⃣  DETAIL scales down (unchanged) */
        detailTweenId = LeanTween.scale(detailObject.gameObject, Vector3.zero, tweenTime)
            .setEase(easeType)
            .setOnComplete(() =>
            {
                detailObject.gameObject.SetActive(false);
                detailTweenId = -1;
                ToggleUI(true);                // UI comes back when detail is gone
                if (globeTweenId == -1) isBusy = false;
            }).id;

        /* 3️⃣  GLOBE scales back up (unchanged) */
        globeRoot.gameObject.SetActive(true);

        globeTweenId = LeanTween.scale(globeRoot.gameObject, _globeStartScale, tweenTime)
            .setEase(easeType)
            .setOnComplete(() =>
            {
                globeTweenId = -1;
                backButton.gameObject.SetActive(false);
                if (globeAnimator) globeAnimator.enabled = true;
                if (globeRotator) globeRotator.enabled = true;
                if (detailTweenId == -1) isBusy = false;
            }).id;
    }


    private void CloseImmediate()
    {
        LeanTween.cancel(globeTweenId); globeTweenId = -1;
        LeanTween.cancel(detailTweenId); detailTweenId = -1;

        detailObject.gameObject.SetActive(false);
        detailObject.localScale = Vector3.zero;

        // CHANGED: Set globe to its hidden (zero-scale) state immediately
        globeRoot.localScale = Vector3.zero;
        globeRoot.gameObject.SetActive(false);

        isBusy = false;
        _current = null;
        ToggleUI(true);
        UpdateSkybox();
    }

    /* ─── util ─── */

    private void ToggleUI(bool show)
    {
        if (uiElementsToHideOnDetail == null) return;

        foreach (var go in uiElementsToHideOnDetail)
            if (go && go != backButton.gameObject) go.SetActive(show);
    }

    private void UpdateSkybox()
    {
        if (!_originalSkyboxCaptured)
        {
            _originalSceneSkybox = RenderSettings.skybox;
            _originalSkyboxCaptured = true;
        }

        RenderSettings.skybox =
            (_current && _current.detailSkybox) ? _current.detailSkybox
                                                : _originalSceneSkybox;
    }

    private void OnDestroy()
    {
        LeanTween.cancel(globeTweenId);
        LeanTween.cancel(detailTweenId);

        if (_current == this) { _current = null; UpdateSkybox(); }

        if (backButton) backButton.onClick.RemoveListener(HandleBackClicked);
    }
}