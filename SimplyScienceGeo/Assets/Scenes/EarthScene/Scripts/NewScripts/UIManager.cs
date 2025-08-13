using UnityEngine;
using TMPro;

/// <summary>
/// The "Voice". Manages all UI elements, such as showing and hiding
/// the information panel for a selected feature.
/// </summary>
public class UIManager : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("The parent GameObject for the entire info panel.")]
    public GameObject infoPanel;

    [Tooltip("The TextMeshPro text element for the feature's name/title.")]
    public TextMeshProUGUI titleText;

    [Tooltip("The TextMeshPro text element for the feature's detailed information.")]
    public TextMeshProUGUI bodyText;

    [Header("Scene Default Text")]
    public string defaultTitle = "Welcome!";
    [TextArea(3, 10)]
    public string defaultBody = "Click on a feature to learn more.";

    [Header("Text Tween Settings")]
    [Tooltip("The duration of the fade-out/fade-in animation for text.")]
    public float textFadeDuration = 0.2f;
    [Tooltip("The scale factor the text will reach during the animation (e.g., 1.05 for a slight pop).")]
    public float textPopScale = 1.05f;
    [Tooltip("The duration for the text pop/scale animation.")]
    public float textPopDuration = 0.1f;

    // References to CanvasGroup components for tweening
    private CanvasGroup _titleCanvasGroup;
    private CanvasGroup _bodyCanvasGroup;

    // LeanTween handles for current tweens to allow cancellation if new changes occur quickly.
    private LTDescr _currentTitleAlphaTween;
    private LTDescr _currentBodyAlphaTween;
    private LTDescr _currentTitleScaleTween;
    private LTDescr _currentBodyScaleTween;

    void Awake()
    {
        // Get the CanvasGroup components from the TextMeshProUGUI GameObjects
        if (titleText != null)
        {
            _titleCanvasGroup = titleText.gameObject.GetComponent<CanvasGroup>();
            if (_titleCanvasGroup == null)
            {
                _titleCanvasGroup = titleText.gameObject.AddComponent<CanvasGroup>();
            }
        }
        if (bodyText != null)
        {
            _bodyCanvasGroup = bodyText.gameObject.GetComponent<CanvasGroup>();
            if (_bodyCanvasGroup == null)
            {
                _bodyCanvasGroup = bodyText.gameObject.AddComponent<CanvasGroup>();
            }
        }
    }

    void Start()
    {
        DisplayDefaultText(defaultTitle, defaultBody);
        // Ensure initial alpha is 1 and scale is 1, in case previous scene or editor state was different.
        if (titleText != null)
        {
            titleText.alpha = 1f;
            if (_titleCanvasGroup != null) _titleCanvasGroup.alpha = 1f;
            titleText.rectTransform.localScale = Vector3.one;
        }
        if (bodyText != null)
        {
            bodyText.alpha = 1f;
            if (_bodyCanvasGroup != null) _bodyCanvasGroup.alpha = 1f;
            bodyText.rectTransform.localScale = Vector3.one;
        }
    }

    public void DisplayInformation(string info, string title)
    {
        if (infoPanel == null || titleText == null || bodyText == null || _titleCanvasGroup == null || _bodyCanvasGroup == null) return;

        // Cancel any ongoing tweens to prevent conflicts or visual glitches
        CancelAllTextTweens();

        // --- Title Text Tween ---
        // Fade out current title text using CanvasGroup
        _currentTitleAlphaTween = LeanTween.alphaCanvas(_titleCanvasGroup, 0f, textFadeDuration)
            .setOnComplete(() =>
            {
                titleText.text = title; // Change text content after fade out
                // Fade in new title text using CanvasGroup
                LeanTween.alphaCanvas(_titleCanvasGroup, 1f, textFadeDuration);

                // Pop scale animation for title text
                _currentTitleScaleTween = LeanTween.scale(titleText.rectTransform, Vector3.one * textPopScale, textPopDuration)
                    .setEase(LeanTweenType.easeOutQuad) // Corrected: lowercase 'e'
                    .setLoopPingPong(1)
                    .setOnComplete(() => { titleText.rectTransform.localScale = Vector3.one; });
            });

        // --- Body Text Tween ---
        // Fade out current body text using CanvasGroup
        _currentBodyAlphaTween = LeanTween.alphaCanvas(_bodyCanvasGroup, 0f, textFadeDuration)
            .setOnComplete(() =>
            {
                bodyText.text = info; // Change text content after fade out
                // Fade in new body text using CanvasGroup
                LeanTween.alphaCanvas(_bodyCanvasGroup, 1f, textFadeDuration);

                // Pop scale animation for body text
                _currentBodyScaleTween = LeanTween.scale(bodyText.rectTransform, Vector3.one * textPopScale, textPopDuration)
                    .setEase(LeanTweenType.easeOutQuad) // Corrected: lowercase 'e'
                    .setLoopPingPong(1)
                    .setOnComplete(() => { bodyText.rectTransform.localScale = Vector3.one; });
            });

        infoPanel.SetActive(true); // Ensure panel is visible
    }

    /// <summary>
    /// Displays a default message in the info panel when no feature is selected.
    /// </summary>
    public void DisplayDefaultText(string title, string body)
    {
        if (infoPanel == null || titleText == null || bodyText == null || _titleCanvasGroup == null || _bodyCanvasGroup == null) return;

        // Cancel any ongoing tweens
        CancelAllTextTweens();

        // --- Title Text Tween ---
        // Fade out current title text using CanvasGroup
        _currentTitleAlphaTween = LeanTween.alphaCanvas(_titleCanvasGroup, 0f, textFadeDuration)
            .setOnComplete(() =>
            {
                titleText.text = title; // Change text content after fade out
                // Fade in new title text using CanvasGroup
                LeanTween.alphaCanvas(_titleCanvasGroup, 1f, textFadeDuration);

                // Pop scale animation for title text
                _currentTitleScaleTween = LeanTween.scale(titleText.rectTransform, Vector3.one * textPopScale, textPopDuration)
                    .setEase(LeanTweenType.easeOutQuad) // Corrected: lowercase 'e'
                    .setLoopPingPong(1)
                    .setOnComplete(() => { titleText.rectTransform.localScale = Vector3.one; });
            });

        // --- Body Text Tween ---
        // Fade out current body text using CanvasGroup
        _currentBodyAlphaTween = LeanTween.alphaCanvas(_bodyCanvasGroup, 0f, textFadeDuration)
            .setOnComplete(() =>
            {
                bodyText.text = body; // Change text content after fade out
                // Fade in new body text using CanvasGroup
                LeanTween.alphaCanvas(_bodyCanvasGroup, 1f, textFadeDuration);

                // Pop scale animation for body text
                _currentBodyScaleTween = LeanTween.scale(bodyText.rectTransform, Vector3.one * textPopScale, textPopDuration)
                    .setEase(LeanTweenType.easeOutQuad) // Corrected: lowercase 'e'
                    .setLoopPingPong(1)
                    .setOnComplete(() => { bodyText.rectTransform.localScale = Vector3.one; });
            });

        infoPanel.SetActive(true); // Ensure panel is visible
    }

    public void HideInformation()
    {
        if (infoPanel == null) return;

        // Cancel any pending text tweens when hiding the panel
        CancelAllTextTweens();

        // Instantly reset alpha and scale of text elements when hiding the panel,
        // to ensure they are ready for the next display.
        if (titleText != null)
        {
            titleText.alpha = 1f;
            if (_titleCanvasGroup != null) _titleCanvasGroup.alpha = 1f;
            titleText.rectTransform.localScale = Vector3.one;
        }
        if (bodyText != null)
        {
            bodyText.alpha = 1f;
            if (_bodyCanvasGroup != null) _bodyCanvasGroup.alpha = 1f;
            bodyText.rectTransform.localScale = Vector3.one;
        }

        infoPanel.SetActive(false);
    }

    /// <summary>
    /// Helper method to cancel all active LeanTween text animations.
    /// </summary>
    private void CancelAllTextTweens()
    {
        if (_currentTitleAlphaTween != null) LeanTween.cancel(_currentTitleAlphaTween.id);
        if (_currentBodyAlphaTween != null) LeanTween.cancel(_currentBodyAlphaTween.id);
        if (_currentTitleScaleTween != null) LeanTween.cancel(_currentTitleScaleTween.id);
        if (_currentBodyScaleTween != null) LeanTween.cancel(_currentBodyScaleTween.id);
    }
}