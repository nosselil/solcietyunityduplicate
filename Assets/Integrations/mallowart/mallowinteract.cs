using UnityEngine;

public class mallowinteract : MonoBehaviour
{
    private GameObject currentArtwork;

    void Update()
    {
        if (currentArtwork != null && Input.GetKeyDown(KeyCode.E))
        {
            currentArtwork.GetComponent<ArtworkInteractable>().ShowModal();
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Artwork"))
            currentArtwork = other.gameObject;
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Artwork"))
            currentArtwork = null;
    }
}
