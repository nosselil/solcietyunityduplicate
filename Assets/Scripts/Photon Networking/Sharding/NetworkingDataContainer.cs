using UnityEngine;

public class NetworkingDataContainer : MonoBehaviour
{
    // Static singleton instance
    public static NetworkingDataContainer Instance { get; private set; }

    // Your payload
    [HideInInspector]
    public int worldReplicaId = -1;

    [HideInInspector]
    public bool allowPlayerControlling = true; // Lock player controls when a dialogue box is active. NOTE: Not really a networking variable, so refactor once we've got more variables like this

    void Awake()
    {
        // If an instance already exists and it’s not this, destroy duplicate
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        // Claim singleton and make persistent
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
}
