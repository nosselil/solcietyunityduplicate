using UnityEngine;

public class UIButtonSoundPlayer : MonoBehaviour
{
    public AudioSource audioSource;
    public AudioClip soundClip;

    public void PlaySound()
    {
        if (audioSource != null && soundClip != null)
        {
            audioSource.clip = soundClip;

            // If already playing, restart
            if (audioSource.isPlaying)
            {
                audioSource.Stop();
            }

            audioSource.Play();
        }
    }

    public void TogglePausePlay()
    {
        if (audioSource == null || audioSource.clip == null) return;

        if (audioSource.isPlaying)
        {
            audioSource.Pause();
        }
        else
        {
            audioSource.UnPause();
        }
    }
}
