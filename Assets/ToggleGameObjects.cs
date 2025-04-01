using UnityEngine;
using System.Collections;

public class ToggleGameObjects : MonoBehaviour
{
    // Reference to the first GameObject with CanvasGroup
    public GameObject firstObject;

    // Reference to the second GameObject with CanvasGroup
    public GameObject secondObject;

    // Duration of the fade effect
    public float fadeDuration = 1f;

    // Reference to the AudioSource component
    public AudioSource audioSource;

    // Cooldown duration in seconds
    public float cooldownDuration = 0.5f;

    // Variable to keep track of the toggle state
    private bool isFirstObjectActive = true;

    // Cooldown flag
    private bool isOnCooldown = false;

    // Method to toggle the GameObjects with fade effect
    public void ToggleObjects()
    {
        // Check if the cooldown is active
        if (isOnCooldown)
        {
            Debug.Log("Toggle is on cooldown. Please wait.");
            return;
        }

        // Start the fade coroutine
        StartCoroutine(ToggleWithFade());

        // Start the cooldown coroutine
        StartCoroutine(StartCooldown());
    }

    // Coroutine to handle the fade effect
    private IEnumerator ToggleWithFade()
    {
        // Play the sound
        if (audioSource != null && audioSource.clip != null)
        {
            audioSource.Play();
        }

        // Get the CanvasGroup components
        CanvasGroup firstCanvasGroup = firstObject.GetComponent<CanvasGroup>();
        CanvasGroup secondCanvasGroup = secondObject.GetComponent<CanvasGroup>();

        // Ensure the CanvasGroup components exist
        if (firstCanvasGroup == null || secondCanvasGroup == null)
        {
            Debug.LogError("CanvasGroup component missing on one or both GameObjects.");
            yield break;
        }

        // Fade out the currently active object
        if (isFirstObjectActive)
        {
            yield return StartCoroutine(FadeCanvasGroup(firstCanvasGroup, 1f, 0f));
            firstObject.SetActive(false); // Disable after fading out
        }
        else
        {
            yield return StartCoroutine(FadeCanvasGroup(secondCanvasGroup, 1f, 0f));
            secondObject.SetActive(false); // Disable after fading out
        }

        // Toggle the state
        isFirstObjectActive = !isFirstObjectActive;

        // Fade in the newly active object
        if (isFirstObjectActive)
        {
            firstObject.SetActive(true); // Enable before fading in
            yield return StartCoroutine(FadeCanvasGroup(firstCanvasGroup, 0f, 1f));
        }
        else
        {
            secondObject.SetActive(true); // Enable before fading in
            yield return StartCoroutine(FadeCanvasGroup(secondCanvasGroup, 0f, 1f));
        }
    }

    // Coroutine to fade a CanvasGroup
    private IEnumerator FadeCanvasGroup(CanvasGroup canvasGroup, float startAlpha, float endAlpha)
    {
        float elapsedTime = 0f;

        // Set the initial alpha
        canvasGroup.alpha = startAlpha;

        // Lerp the alpha over time
        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(startAlpha, endAlpha, elapsedTime / fadeDuration);
            yield return null; // Wait for the next frame
        }

        // Ensure the final alpha is set
        canvasGroup.alpha = endAlpha;
    }

    // Coroutine to handle the cooldown
    private IEnumerator StartCooldown()
    {
        // Set the cooldown flag to true
        isOnCooldown = true;

        // Wait for the cooldown duration
        yield return new WaitForSeconds(cooldownDuration);

        // Reset the cooldown flag
        isOnCooldown = false;

        Debug.Log("Cooldown ended. Toggle is ready to use.");
    }
}