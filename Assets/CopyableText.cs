using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class CopyableText : MonoBehaviour
{
    public TextMeshProUGUI textToCopy; // Reference to the TextMeshProUGUI component
    public TextMeshProUGUI feedbackText; // Reference to the feedback text (e.g., "Copied to clipboard")
    public float feedbackDuration = 2f; // Duration to display the feedback text

    private void Start()
    {
        // Ensure the feedback text is hidden at the start
        if (feedbackText != null)
        {
            feedbackText.gameObject.SetActive(false);
        }
    }

    public void OnClickCopyText()
    {
        if (textToCopy != null)
        {
            // Copy the text to the clipboard
            GUIUtility.systemCopyBuffer = textToCopy.text;

            // Show the feedback text
            if (feedbackText != null)
            {
                feedbackText.gameObject.SetActive(true);
                Invoke("HideFeedbackText", feedbackDuration);
            }
        }
    }

    private void HideFeedbackText()
    {
        if (feedbackText != null)
        {
            feedbackText.gameObject.SetActive(false);
        }
    }
}