using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Toast : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI txt;

    public void ShowToast(string text, int duration)
    {
        StartCoroutine(ShowToastCor(text, duration));
    }

    private IEnumerator ShowToastCor(string text, int duration)
    {
        // Store the original color
        var originalColor = txt.color;

        // Set the text and enable it
        txt.text = text;
        txt.enabled = true;

        // Fade in
        yield return FadeInAndOut(txt, true, 0.5f);

        // Wait for the duration
        float counter = 0;
        while (counter < duration)
        {
            counter += Time.deltaTime;
            yield return null;
        }

        // Fade out
        yield return FadeInAndOut(txt, false, 0.5f);

        // Disable the text and reset its color
        txt.enabled = false;
        txt.color = originalColor;
    }

    private static IEnumerator FadeInAndOut(Graphic targetText, bool fadeIn, float duration)
    {
        // Store the original color
        var originalColor = targetText.color;

        // Set the start and end alpha values
        float startAlpha = fadeIn ? 0f : originalColor.a;
        float endAlpha = fadeIn ? originalColor.a : 0f;

        float counter = 0f;

        while (counter < duration)
        {
            counter += Time.deltaTime;
            float alpha = Mathf.Lerp(startAlpha, endAlpha, counter / duration);
            targetText.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
            yield return null;
        }
    }
}