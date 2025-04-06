using Fusion;
using UnityEngine;

public class PlayerSpawner : SimulationBehaviour, IPlayerJoined // NOTE: Originally SimulationBehaviour
{
    public GameObject PlayerPrefab;
    [SerializeField] Vector3 spawnPosition;

    public void PlayerJoined(PlayerRef player)
    {        
        if (player == Runner.LocalPlayer)
        {
            Vector3 localSpawnPosition = new Vector3(-38, -10, 70);
            Debug.Log("The set local spawn position is " + localSpawnPosition);

            NetworkObject spawnedObject = Runner.Spawn(PlayerPrefab, localSpawnPosition, Quaternion.identity, Runner.LocalPlayer);
            Runner.SetPlayerObject(player, spawnedObject.gameObject.GetComponent<NetworkObject>());
            Debug.Log("Spawn player at position " + localSpawnPosition);

            GameObject go = spawnedObject.gameObject;
            PlayerSetup setup = go.GetComponent<PlayerSetup>();

            Debug.Log("SETUP: Calling setupPlayer for player " + player.PlayerId);
            setup.SetupPlayer();

            // Generate and assign a mock wallet address for the local player.
            string localWalletAddress = MultiplayerChat.Instance.AssignMockWalletAddress();
            Debug.Log("WALLET: Generated local wallet address: " + localWalletAddress);

            // Set the local wallet address on the player's attributes.
            PlayerAttributes attributes = go.GetComponent<PlayerAttributes>();
            if (attributes != null)
            {
                attributes.LocalWalletAddress = localWalletAddress;
            }
            else
            {
                Debug.LogWarning("PlayerAttributes component not found on player object.");
            }
        }

        Debug.Log("ON PLAYER JOINED triggered");

        Invoke("UpdateWalletIdCollection", 1.0f);

        // NOTE: This won't have effect unless we're the state authority, but this is called here since SimulationBehaviour can't be used to separate who is the authority and who is not
        //Runner.GetPlayerObject(Runner.LocalPlayer).GetComponent<PlayerAttributes>().UpdateNetworkedWalletAddressDictionary();
    }

    void UpdateWalletIdCollection()
    {
        Debug.Log("ON PLAYER JOINED: Invoked collection update");

        MultiplayerChat.Instance.UpdateWalletAddressCollection(); // Make sure each client has an updated copy of the wallet addresses
    }

}