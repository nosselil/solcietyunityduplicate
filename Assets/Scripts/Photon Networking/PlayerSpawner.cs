using Fusion;
using Starter;
using System.Collections;
using System.Linq;
using UnityEngine;

public class PlayerSpawner : SimulationBehaviour, IPlayerJoined // NOTE: Originally SimulationBehaviour
{
    public GameObject PlayerPrefab;
    [SerializeField] Vector3 spawnPosition;
    [SerializeField] Vector3 spawnRotation; // (0, 180, 0) for most scenes
    [SerializeField] float initialCameraYRotation;

    private bool runnerInfoKeyPressed = false;
    
    public void PlayerJoined(PlayerRef player)
    {        
        if (player == Runner.LocalPlayer)
        {
            NetworkController.Instance.localPlayerSpawned = true; // True for this scene

            //Vector3 localSpawnPosition = new Vector3(-38, -10, 70);
           // Debug.Log("The set local spawn position is " + localSpawnPosition);

            NetworkObject spawnedObject = Runner.Spawn(PlayerPrefab, spawnPosition, Quaternion.Euler(spawnRotation), Runner.LocalPlayer);
            Runner.SetPlayerObject(player, spawnedObject.gameObject.GetComponent<NetworkObject>());
            Debug.Log("Spawn player at position " + spawnPosition);

            GameObject go = spawnedObject.gameObject;
            PlayerSetup setup = go.GetComponent<PlayerSetup>();
            
            setup.SetupPlayer(initialCameraYRotation);

            // Generate and assign a mock wallet address for the local player.
            string localWalletAddress;

            Debug.Log("PREPARE TO SEND: in wallet manager instance is " + WalletManager.instance.walletAddress);

            if (WalletManager.instance != null && WalletManager.instance.worldReplicaId == -1)
            {
                // How many players are in the session right now?
                int totalPlayers = Runner.ActivePlayers.Count();
                // Bucket them: players 1–64 → 1, 65–128 → 2, etc. (assuming 64 players is the max per replica)
                int replicaId = ((totalPlayers - 1) / NetworkController.Instance.maxPlayersPerReplica) + 1;
                WalletManager.instance.worldReplicaId = replicaId;
                Debug.Log($"WORLD REPLICA: Assigned worldReplicaId = {replicaId} (player #{totalPlayers})");
            }

            if (WalletManager.instance && !string.IsNullOrEmpty(WalletManager.instance.walletAddress))
            {
                localWalletAddress = WalletManager.instance.walletAddress;
                MultiplayerChat.Instance.localWalletAddress = localWalletAddress;
            }
            else
                localWalletAddress = MultiplayerChat.Instance.AssignMockWalletAddress();
                        
            // Set the local wallet address on the player's attributes.
            PlayerAttributes attributes = go.GetComponent<PlayerAttributes>();
            if (attributes != null)
            {                
                attributes.LocalWalletAddress = localWalletAddress;

                Debug.Log("PREPARE TO SEND: Player attributes local address changed to " + attributes.LocalWalletAddress);

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

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.U))
        {
            runnerInfoKeyPressed = true;
        }
    }

    public override void FixedUpdateNetwork()
    {
        if (runnerInfoKeyPressed)
        {
            if (Runner != null)
                Debug.Log("ROOM: Current room: " + Runner.SessionInfo.Name + ", current game mode: " + Runner.GameMode);
            else
                Debug.Log("ROOM: Runner is null, can't get room or game mode info");

            runnerInfoKeyPressed = false;
        }
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
            //Debug.Log($"PROXIMITY: Non-local player's ProximitySelector disabled for player {player.PlayerId}");
        }

        yield break;
    }



    void UpdateWalletIdCollection()
    {
        //Debug.Log("ON PLAYER JOINED: Invoked collection update");

        MultiplayerChat.Instance.UpdateWalletAddressCollection(); // Make sure each client has an updated copy of the wallet addresses
    }

}