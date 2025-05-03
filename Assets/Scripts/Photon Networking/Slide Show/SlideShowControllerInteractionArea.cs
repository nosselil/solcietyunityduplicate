using Fusion;
using UnityEngine;

public class SlideShowControllerInteractionArea : MonoBehaviour
{
    [HideInInspector]
    public bool localPlayerInsideInteractionArea = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // NOTE: The below triggers seem to fire twice per player, but doesn't matter really. Maybe there are multiple colliders in the player?

    private void OnTriggerEnter(Collider other)
    {
        // Look for Fusion's NetworkObject on the thing that just entered
        var netObj = other.GetComponent<NetworkObject>();
        // If it's the local player's object, fire the event
        if (netObj != null && netObj.HasInputAuthority)
        {
            Debug.Log("SLIDE CONTROLLER: Local player entered trigger zone!");
            localPlayerInsideInteractionArea = true;
        }        
    }

    private void OnTriggerExit(Collider other)
    {
        // Look for Fusion's NetworkObject on the thing that just entered
        var netObj = other.GetComponent<NetworkObject>();
        // If it's the local player's object, fire the event
        if (netObj != null && netObj.HasInputAuthority)
        {
            Debug.Log("SLIDE CONTROLLER: Local player left the trigger zone!");
            localPlayerInsideInteractionArea = false;
        }
    }
}
