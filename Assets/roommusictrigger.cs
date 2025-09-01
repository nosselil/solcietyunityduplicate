using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

public class RoomMusicTrigger : MonoBehaviour
{
    [Header("Audio Settings")]
    public AudioSource musicSource;
    public float fadeSpeed = 1.0f;
    public float targetVolume = 0.5f; // Control max volume of music in this room

    [Header("Post Process Settings")]
    [SerializeField] private PostProcessVolume roomPostProcess;

    private bool playerInside = false;

    void Awake()
    {
        // Automatically grab PostProcessVolume if not assigned
        if (roomPostProcess == null)
            roomPostProcess = GetComponent<PostProcessVolume>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInside = true;

            if (!musicSource.isPlaying)
                musicSource.Play();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInside = false;
        }
    }

    void Update()
    {
        // --- MUSIC FADE ---
        if (playerInside && musicSource.volume < targetVolume)
        {
            musicSource.volume += fadeSpeed * Time.deltaTime;
            if (musicSource.volume > targetVolume)
                musicSource.volume = targetVolume;
        }
        else if (!playerInside && musicSource.volume > 0f)
        {
            musicSource.volume -= fadeSpeed * Time.deltaTime;

            if (musicSource.volume <= 0f)
            {
                musicSource.volume = 0f;
                musicSource.Stop();
            }
        }

        // --- POST PROCESS FADE ---
        if (playerInside && roomPostProcess.weight < 1f)
        {
            roomPostProcess.weight += fadeSpeed * Time.deltaTime;
            if (roomPostProcess.weight > 1f)
                roomPostProcess.weight = 1f;
        }
        else if (!playerInside && roomPostProcess.weight > 0f)
        {
            roomPostProcess.weight -= fadeSpeed * Time.deltaTime;
            if (roomPostProcess.weight < 0f)
                roomPostProcess.weight = 0f;
        }
    }
}
