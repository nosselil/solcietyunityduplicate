using Fusion;
using Fusion.Photon.Realtime;
using Language.Lua;
using NUnit.Framework;
using Solana.Unity.Soar.Types;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MultiplayerChat : NetworkBehaviour
{
    // Dictionary where the key is a string:
    // "" or "public" for public chat messages,
    // Other keys are associated with players' wallet ids
    private Dictionary<string, List<string>> chatMessages = new ();

    //    Dictionary<int, string> playerWallets = new();

    //[Networked] // TODO: Adjust capacity    
    //private Dictionary<int, string> PlayerWallets => default;
    private HashSet<string> PlayerWallets = new();

    public string localWalletAddress; // This will be set to player object's LocalWalletAddress, which is a networked variable
    
    [SerializeField] TMP_InputField chatMessageInputText;

    [SerializeField] GameObject chatParent;

    public static MultiplayerChat Instance;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Instance = this;
       
        chatParent.SetActive(false); // Hide chat at first
        chatMessages["public"] = new List<string>();
    }

    public string[] GetActivePlayerList() // Used with private chats
    {
        return PlayerWallets.ToArray();
    }



    void SetChatActive(bool active)
    {
        chatParent.SetActive(active);
    }

    // Update is called once per frame
    void Update()
    {        
        if (Input.GetKeyDown(KeyCode.LeftControl) || Input.GetKeyDown(KeyCode.RightControl)) /*Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))*/
        {
            Debug.Log("CHAT: Toggle chat parent active");
            chatParent.SetActive(!chatParent.activeInHierarchy); // Toggle chat parent activity
        }

        if (Input.GetKeyDown(KeyCode.D))
        {
            Debug.Log("Printing networked hashset contents:");
            foreach (string wallet in PlayerWallets)
            {
                Debug.Log($"WalletAddress: {wallet}");
            }
        }

        if (Input.GetKeyDown(KeyCode.U))
            UpdateWalletAddressCollection(); // Make sure each client has an updated copy of the wallet addresses
    }

    public void OnInputFinished()
    {
        Debug.Log("CHAT: On Input finished");

        // Check for Enter key press (may be frame-dependent)
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            // Create a shortened version of the wallet address.
            string shortenedAddress = ShortenWalletAddress(localWalletAddress);
            // Include the local player id (for debug purposes).
            int playerId = Runner.LocalPlayer.PlayerId;
            string newMessage = shortenedAddress + ": " + chatMessageInputText.text;
            chatMessageInputText.text = ""; // Reset the input field
            Debug.Log("CHAT: ENTER PRESSED, sending message: " + newMessage);

            // For a public message, use the reserved key "public".
            SendChatMessageRpc(newMessage, "public");

            // To send a private message, call with the target wallet address instead:
            // SendChatMessageRpc(newMessage, targetWalletAddress);
        }
        
    }

    // RPC: Called on all clients to update messages for a given conversation.
    // targetWalletAddress: "public" for public messages, or a specific wallet address for a private conversation.
    [Rpc(RpcSources.All, RpcTargets.All)]
    void SendChatMessageRpc(string newMessage, string targetWalletAddress, RpcInfo info = default)
    {
        Debug.Log("CHAT: SendChatMessageRpc called with message: " + newMessage +
                  " for target wallet: " + targetWalletAddress +
                  ". Source: " + info.Source + " | IsInvokeLocal: " + info.IsInvokeLocal);

        // If it's a public message, update the public conversation.
        if (targetWalletAddress == "public" || string.IsNullOrEmpty(targetWalletAddress))
        {
            chatMessages["public"].Add(newMessage);
            LocalChatWindowController.Instance.SetChatText(chatMessages["public"]);
                //fullChatText.text = string.Join("\n", chatMessages["public"].ToArray());            
        }
        else
        {
            // For private messages, only add if the local wallet address matches the target.
            if (localWalletAddress == targetWalletAddress)
            {
                if (!chatMessages.ContainsKey(targetWalletAddress))
                {
                    chatMessages[targetWalletAddress] = new List<string>();
                }
                chatMessages[targetWalletAddress].Add(newMessage);
                Debug.Log("CHAT: Private message received for wallet " + targetWalletAddress);
            }
        }
    }

    public void UpdateWalletAddressCollection()
    {
        GameObject[] playerObjects = GameObject.FindGameObjectsWithTag("NetworkedPlayer");
        foreach (GameObject playerObject in playerObjects)
        {
            // Assume every spawned player object has a PlayerAttributes component.
            PlayerAttributes attr = playerObject.GetComponent<PlayerAttributes>();
            if (attr != null && !string.IsNullOrEmpty(attr.LocalWalletAddress))
            {
                // Add the wallet address to the HashSet.
                PlayerWallets.Add(attr.LocalWalletAddress);
            }
        }
    }


    /*[Rpc(RpcSources.All, RpcTargets.All)]
    public void RegisterWalletAddressRpc(int playerId, string walletAddress, RpcInfo info = default) // Consider making private and refactoring this
    {
        Debug.Log($"WALLET: Registering wallet address '{walletAddress}' for player ID {playerId}");

        // Find and remove any duplicate wallet addresses associated with a different playerId.
        List<int> keysToRemove = new List<int>();
        foreach (var kvp in PlayerWallets)
        {
            if (kvp.Value == walletAddress && kvp.Key != playerId)
            {
                keysToRemove.Add(kvp.Key);
            }
        }
        foreach (int key in keysToRemove)
        {
            PlayerWallets.Remove(key);
            Debug.Log($"Removed duplicate wallet address for player ID {key}");
        }

        // Add or update the wallet address for the specified playerId.
        //PlayerWallets.Set(playerId, walletAddress); // For networked dictionary
        PlayerWallets[playerId] = walletAddress;
        Debug.Log($"Updated wallet dictionary: {string.Join(", ", PlayerWallets)}");
    }*/

    #region helpers

    // Helper method to shorten a wallet address.
    private string ShortenWalletAddress(string walletAddress)
    {
        if (string.IsNullOrEmpty(walletAddress) || walletAddress.Length <= 10)
            return walletAddress;

        // Get the first 5 characters and the last 3 characters.
        string firstPart = walletAddress.Substring(0, 5);
        string lastPart = walletAddress.Substring(walletAddress.Length - 3);
        return firstPart + ".." + lastPart;
    }

    public string GenerateLocalWalletAddress(int mockWalletKey = 0) // TODO: Remove when done with testing
    {
        string walletKey = mockWalletKey == 0 ? "mockWalletAddress" : "mockWalletAddress2";
        // Try to retrieve an existing wallet address from PlayerPrefs.
        string walletAddress = PlayerPrefs.GetString(walletKey, string.Empty);

        // If it doesn't exist, generate a new one.
        if (string.IsNullOrEmpty(walletAddress))
        {
            walletAddress = System.Guid.NewGuid().ToString();
            PlayerPrefs.SetString(walletKey, walletAddress);
            PlayerPrefs.Save();
            Debug.Log("Generated new mock wallet address: " + walletAddress);
        }
        else
        {
            Debug.Log("Found existing mock wallet address: " + walletAddress);
        }

        return walletAddress;
    }

    public string AssignMockWalletAddress()
    {
        int cloneIndex = GetCloneIndex();
        //Debug.Log("Clone index " + cloneIndex);

        localWalletAddress = GenerateLocalWalletAddress(cloneIndex); // Read stored address or generate a new one, one address per ParrelSync clone
        Debug.Log("Mock address for client is " + localWalletAddress);

        return localWalletAddress;
    }

    public int GetCloneIndex() // 0 if not clone, 1 if clone
    {
        string[] args = System.Environment.GetCommandLineArgs();
        //Debug.Log("Command line args:");
        foreach (string arg in args)
        {
            //Debug.Log("Command line: " + arg);
            if (arg.ToLower().Contains("clone"))
            {
                return 1;
            }
        }
        return 0;
    }

    #endregion
}
