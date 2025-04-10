using Fusion;
using Starter;
using System.Collections;
using UnityEngine;

public class PlayerSpawner : SimulationBehaviour, IPlayerJoined // NOTE: Originally SimulationBehaviour
{
    public GameObject PlayerPrefab;
    [SerializeField] Vector3 spawnPosition;

    public void PlayerJoined(PlayerRef player)
    {        
        if (player == Runner.LocalPlayer)
        {
            NetworkController.Instance.localPlayerSpawned = true; // True for this scene

            //Vector3 localSpawnPosition = new Vector3(-38, -10, 70);
           // Debug.Log("The set local spawn position is " + localSpawnPosition);

            NetworkObject spawnedObject = Runner.Spawn(PlayerPrefab, spawnPosition, Quaternion.Euler(new Vector3(0, 180, 0)), Runner.LocalPlayer);
            Runner.SetPlayerObject(player, spawnedObject.gameObject.GetComponent<NetworkObject>());
            Debug.Log("Spawn player at position " + spawnPosition);

            GameObject go = spawnedObject.gameObject;
            PlayerSetup setup = go.GetComponent<PlayerSetup>();

            Debug.Log("SETUP: Calling setupPlayer for player " + player.PlayerId);
            setup.SetupPlayer();

            // Generate and assign a mock wallet address for the local player.
            string localWalletAddress;

            Debug.Log("WALLET ADDRESS: in wallet manager instance is " + WalletManager.instance.walletAddress);

            if (WalletManager.instance && !string.IsNullOrEmpty(WalletManager.instance.walletAddress))
                localWalletAddress = WalletManager.instance.walletAddress;
            else
                localWalletAddress = MultiplayerChat.Instance.AssignMockWalletAddress();
            
            // Set the local wallet address on the player's attributes.
            PlayerAttributes attributes = go.GetComponent<PlayerAttributes>();
            if (attributes != null)
            {
                attributes.LocalWalletAddress = localWalletAddress;
                //attributes.Nickname = WalletUtilities.ShortenWalletAddress(localWalletAddress); // NOTE: We're currently using separate variables for these, since we may later on have a nickname through Solana Name Service, which
                // is separate from the walletAddress                
                attributes.SetCapColorIndex(); //player.PlayerId - 1);
                //Debug.Log("CAP: Set nickname and cap color index (" + (player.PlayerId - 1) + ")");                
            }
            else
            {
                Debug.LogWarning("PlayerAttributes component not found on player object.");
            }
        }
        else
        {
            // Disable the ProximitySelection of the non-local players. Since the object may not be ready yet, we use a coroutine to poll the availability of the object

            Debug.Log("PROXIMITY: ProximitySelector disabled. Starting coroutine to monitor it.");
            StartCoroutine(WaitForAndDisableProximitySelector(player));            
        }

        Debug.Log("ROOM: current room is " +  Runner.SessionInfo?.Name);

        Debug.Log("ON PLAYER JOINED triggered");

        Invoke("UpdateWalletIdCollection", 1.0f);

        // NOTE: This won't have effect unless we're the state authority, but this is called here since SimulationBehaviour can't be used to separate who is the authority and who is not
        //Runner.GetPlayerObject(Runner.LocalPlayer).GetComponent<PlayerAttributes>().UpdateNetworkedWalletAddressDictionary();
    }

    private IEnumerator WaitForAndDisableProximitySelector(PlayerRef player)
    {
        NetworkObject netObj = null;
        // Wait until the player's network object becomes available.
        while (netObj == null)
        {
            netObj = Runner.GetPlayerObject(player);
            if (netObj == null)
            {
                Debug.Log("Waiting for non-local player's network object...");
                yield return new WaitForSeconds(0.5f);
            }
        }

        // Once available, get the ProximitySelector component.
        var proxSelector = netObj.gameObject.GetComponentInChildren<PixelCrushers.DialogueSystem.ProximitySelector>();
        if (proxSelector != null)
        {
            proxSelector.enabled = false;
            Debug.Log($"PROXIMITY: Non-local player's ProximitySelector disabled for player {player.PlayerId}");
        }

        yield break;
    }



    void UpdateWalletIdCollection()
    {
        Debug.Log("ON PLAYER JOINED: Invoked collection update");

        MultiplayerChat.Instance.UpdateWalletAddressCollection(); // Make sure each client has an updated copy of the wallet addresses
    }

}