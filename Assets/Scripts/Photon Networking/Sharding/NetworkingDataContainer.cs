using UnityEngine;

public class NetworkingDataContainer : MonoBehaviour
{
    // Static singleton instance
    public static NetworkingDataContainer Instance { get; private set; }

    // Your payload
    [HideInInspector]
    public int worldReplicaId = -1;

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
