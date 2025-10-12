using UnityEngine;

public class ScaleOnActive : MonoBehaviour
{
    [Header("Scale Settings")]
    public float scaleUpSize = 1f;
    public float scaleDownSize = 0f;
    public float tweenDuration = 0.3f;
    public LeanTweenType tweenType = LeanTweenType.easeOutBack;

    private Vector3 originalScale;

    private void Awake()
    {
        originalScale = transform.localScale;
        transform.localScale = Vector3.one * scaleDownSize; // start small/invisible
    }

    private void OnEnable()
    {
        // Scale up smoothly when activated
        LeanTween.cancel(gameObject);
        transform.localScale = Vector3.one * scaleDownSize;
        LeanTween.scale(gameObject, originalScale * scaleUpSize, tweenDuration)
                 .setEase(tweenType);
    }

    public void DeactivateWithScaleDown()
    {
        // Scale down, then disable
        LeanTween.cancel(gameObject);
        LeanTween.scale(gameObject, Vector3.one * scaleDownSize, tweenDuration)
                 .setEase(LeanTweenType.easeInBack)
                 .setOnComplete(() => gameObject.SetActive(false));
    }
    private void OnDisable()
    {
        transform.localScale = Vector3.one * scaleDownSize;
    }

}
