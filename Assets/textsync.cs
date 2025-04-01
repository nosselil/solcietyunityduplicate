using UnityEngine;
using UnityEngine.UI;

public class TextSync : MonoBehaviour
{
    // Reference to the Text component that will be updated
    public Text targetText;

    // Reference to the Text component that will trigger the change
    public Text sourceText;

    // Optional: You can use this to initialize the target text with the source text's value
    private void Start()
    {
        if (sourceText != null && targetText != null)
        {
            targetText.text = sourceText.text;
        }
    }

    // Update is called once per frame
    private void Update()
    {
        // Check if the source text has changed
        if (sourceText != null && targetText != null && targetText.text != sourceText.text)
        {
            // Update the target text to match the source text
            targetText.text = sourceText.text;
        }
    }
}