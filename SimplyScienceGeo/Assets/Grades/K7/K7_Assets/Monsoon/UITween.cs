using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
[RequireComponent(typeof(CanvasGroup))]
public class UITween : MonoBehaviour
{
    [Header("Tween Settings")]
    public float tweenDuration = 0.3f;
    public LeanTweenType easeIn = LeanTweenType.easeOutBack;
    public LeanTweenType easeOut = LeanTweenType.easeInBack;
    public Vector2 tweenOffset = new Vector2(200f, 0f); // slide from right

    private RectTransform rect;
    private CanvasGroup canvasGroup;
    private Vector2 startPos;
    private bool initialized = false;

    private void Awake()
    {
        rect = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        startPos = rect.anchoredPosition;
        initialized = true;
    }

    private void OnEnable()
    {
        if (!initialized) Awake();
        TweenIn();
    }

    private void OnDisable()
    {
        // Reset instantly when disabled, so it’s ready for next activation
        rect.anchoredPosition = startPos;
        canvasGroup.alpha = 0f;
    }

    /// <summary>
    /// Tweens in when enabled.
    /// </summary>
    private void TweenIn()
    {
        LeanTween.cancel(gameObject);

        rect.anchoredPosition = startPos + tweenOffset;
        canvasGroup.alpha = 0;

        LeanTween.move(rect, startPos, tweenDuration).setEase(easeIn);
        LeanTween.value(gameObject, 0, 1, tweenDuration)
            .setOnUpdate((float val) => canvasGroup.alpha = val);
    }

    /// <summary>
    /// Tweens out, then disables the object.
    /// </summary>
    public void TweenOutAndDisable()
    {
        LeanTween.cancel(gameObject);

        LeanTween.move(rect, startPos + tweenOffset, tweenDuration)
            .setEase(easeOut);
        LeanTween.value(gameObject, 1, 0, tweenDuration)
            .setOnUpdate((float val) => canvasGroup.alpha = val)
            .setOnComplete(() => gameObject.SetActive(false));
    }
}
