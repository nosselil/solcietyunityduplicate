using Fusion;
using UnityEngine;
using UnityEngine.SceneManagement;

[DefaultExecutionOrder(-100)]
public class RoomNameSetter : MonoBehaviour
{
    void Awake()
    {
        // 1) Grab the FusionBootstrap on this GameObject
        FusionBootstrap fusionBootstrap = GetComponent<FusionBootstrap>();

        if (fusionBootstrap == null)
            return;

        // 2) Try to find your NetworkingDataContainer
        //    (either on the same GO, or anywhere in the scene)
        var container = NetworkingDataContainer.Instance;
                
        // 3) Build a room name based on worldReplicaId
        if (container != null)
        {
            fusionBootstrap.DefaultRoomName = $"{SceneManager.GetActiveScene().name}_{container.worldReplicaId}";            
        }        
    }
}
