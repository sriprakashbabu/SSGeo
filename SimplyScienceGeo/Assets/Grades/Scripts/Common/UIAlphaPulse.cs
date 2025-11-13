using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIAlphaPulse : MonoBehaviour
{
    [Range(0f, 1f)] public float minAlpha = 0.3f;
    [Range(0f, 1f)] public float maxAlpha = 1f;
    public float speed = 2f; // how fast the pulse happens

    private Graphic uiGraphic;      // for Image, RawImage, Text
    private TextMeshProUGUI tmpText; // if using TMP

    private void Awake()
    {
        uiGraphic = GetComponent<Graphic>();
        tmpText = GetComponent<TextMeshProUGUI>();
    }

    void Update()
    {
        float alpha = Mathf.Lerp(minAlpha, maxAlpha, (Mathf.Sin(Time.time * speed) + 1f) / 2f);

        if (uiGraphic != null)
        {
            Color c = uiGraphic.color;
            c.a = alpha;
            uiGraphic.color = c;
        }
        else if (tmpText != null)
        {
            Color c = tmpText.color;
            c.a = alpha;
            tmpText.color = c;
        }
    }
}
