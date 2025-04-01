using UnityEngine;

public class ToggleMusic : MonoBehaviour
{
    public AudioSource backgroundMusic; // Reference to the AudioSource component

    // This method will be called when the button is clicked
    public void ToggleBackgroundMusic()
    {
        if (backgroundMusic != null)
        {
            // Toggle the mute state of the AudioSource
            backgroundMusic.mute = !backgroundMusic.mute;

            // Alternatively, you can enable/disable the AudioSource component:
            // backgroundMusic.enabled = !backgroundMusic.enabled;
        }
    }
}