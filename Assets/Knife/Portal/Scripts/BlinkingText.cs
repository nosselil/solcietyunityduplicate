using UnityEngine;
using TMPro;

public class BlinkingText : MonoBehaviour
{
    public float blinkInterval = 0.5f; // Time in seconds for each blink
    private TextMeshProUGUI tmpText;
    private float timer;
    private bool isVisible = true;

    void Awake()
    {
        tmpText = GetComponent<TextMeshProUGUI>();
    }

    void Update()
    {
        timer += Time.unscaledDeltaTime; // Use unscaled time for UI

        if (timer >= blinkInterval)
        {
            isVisible = !isVisible;
            SetAlpha(isVisible ? 1f : 0f);
            timer = 0f;
        }
    }

    void SetAlpha(float alpha)
    {
        if (tmpText != null)
        {
            Color c = tmpText.color;
            c.a = alpha;
            tmpText.color = c;
        }
    }
}