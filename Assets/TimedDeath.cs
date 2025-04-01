using UnityEngine;
using InsaneSystems.RTSStarterKit; // Assuming your Damageable script is in this namespace

public class TimedDeath : MonoBehaviour
{
    [SerializeField] private float timeToDeath = 5f; // Time in seconds before the unit dies

    private Damageable damageable; // Reference to the Damageable component
    private float deathTimer = 0f; // Timer to track how long the unit has been alive

    private void Start()
    {
        // Get the Damageable component on this GameObject
        damageable = GetComponent<Damageable>();

        // Check if the Damageable component exists. If not, log a warning
        if (damageable == null)
        {
            Debug.LogWarning("TimedDeath script requires a Damageable component on the same GameObject.");
            enabled = false; // Disable this script to prevent further errors
            return;
        }
    }

    private void Update()
    {
        if (damageable == null) return; // Safety check

        // Increment the timer
        deathTimer += Time.deltaTime;

        // Check if the timer has reached the required time to trigger death
        if (deathTimer >= timeToDeath)
        {
            damageable.Die(); // Call the Die function on the Damageable component
            enabled = false; // Disable this script to prevent calling Die() repeatedly
        }
    }
}