using UnityEngine;

public class NPCLookAtPlayer : MonoBehaviour
{
    private Transform player; // Reference to the player's transform
    private bool isPlayerInRange = false; // Track if the player is inside the collider

    private void OnTriggerEnter(Collider other)
    {
        // Check if the collider is the player
        if (other.CompareTag("Player"))
        {
            player = other.transform; // Cache the player's transform
            isPlayerInRange = true; // Set the flag to true
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // Check if the collider is the player
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = false; // Set the flag to false
        }
    }

    private void Update()
    {
        // If the player is in range, make the NPC look at the player
        if (isPlayerInRange && player != null)
        {
            // Make the NPC look at the player
            transform.LookAt(player);

            // Optional: Lock rotation to only the Y axis (if needed)
            Vector3 eulerAngles = transform.eulerAngles;
            eulerAngles.x = 0; // Lock X rotation
            eulerAngles.z = 0; // Lock Z rotation
            transform.eulerAngles = eulerAngles;
        }
    }
}