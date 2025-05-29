using PixelCrushers.DialogueSystem;
using PixelCrushers.DialogueSystem.OpenAIAddon;
using UnityEngine;

public class NetworkingDataContainer : MonoBehaviour
{
    // TODO: NetworkingDataContainer is a bit of a misnomer. This class was originally intended to only hold network-specific persistent variables, but now it's a container of pretty much all kinds of
    // persistent variables. Perhaps "PersistentDataContainer" would be a more suitable name.

    // Static singleton instance
    public static NetworkingDataContainer Instance { get; private set; }

    // Your payload
    [HideInInspector]
    public int worldReplicaId = -1;

    [HideInInspector]
    public bool allowPlayerControlling = true; // Lock player controls when a dialogue box is active. NOTE: Not really a networking variable, so refactor once we've got more variables like this

    [HideInInspector]
    public bool allowPlayerCameraControlling = true;

    [HideInInspector]
    public bool usingMobileJoystick = false; // Lock mobile camera rotation when mobile joystick is used

    void Awake()
    {
        // If an instance already exists and it's not this, destroy duplicate
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        // Claim singleton and make persistent
        Instance = this;
        DontDestroyOnLoad(gameObject);

        DialogueSystemTrigger.OnConversationStarted += HandleConversationStarted;
        DialogueSystemTrigger.OnConversationEnded += HandleConversationEnded;

        RuntimeAIConversation.OnConversationStarted += HandleConversationStarted;
        RuntimeAIConversation.OnConversationEnded += HandleConversationEnded;

        Debug.Log("DIALOGUE: Subscribed to conversation started, allow player controlling: " + allowPlayerControlling);

    }

    void HandleConversationStarted()
    {
        allowPlayerControlling = false; // This will prevent any further movement
    }

    void HandleConversationEnded()
    {
        allowPlayerControlling = true;
    }

    // TODO: Event subscription logic in OnSceneLoaded / Unloaded?

    
}
