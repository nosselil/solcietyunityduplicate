using Fusion;
using UnityEngine;

public class PlayerSpawner : SimulationBehaviour, IPlayerJoined
{
    public GameObject PlayerPrefab;
    [SerializeField] Vector3 spawnPosition;

    public void PlayerJoined(PlayerRef player)
    {
        if (player == Runner.LocalPlayer)
        {
            Vector3 localSpawnPosition = new Vector3(-38, -10, 70);
            Debug.Log("The set local spawn position is " + localSpawnPosition);

            NetworkObject spawnedObject = Runner.Spawn(PlayerPrefab, localSpawnPosition, Quaternion.identity);
            Debug.Log("Spawn player at position " + localSpawnPosition);

            GameObject go = spawnedObject.gameObject;            
            PlayerSetup setup = go.GetComponent<PlayerSetup>();

            Debug.Log("SETUP: Calling setupPlayer for player " + player.PlayerId);

            string localWalletAddress = MultiplayerChat.Instance.AssignMockWalletAddress();
            MultiplayerChat.Instance.RegisterWalletAddressRpc(Runner.LocalPlayer.PlayerId, localWalletAddress);

            setup.SetupPlayer();
        }
    }

}