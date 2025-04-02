using Fusion;
using Fusion.Photon.Realtime;
using Language.Lua;
using NUnit.Framework;
using Solana.Unity.Soar.Types;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MultiplayerChat : NetworkBehaviour
{
    public List<string> publicMessages; // Or should this be in the dictionary as well?

    // Dictionary where the key is the target player's ID and the value is the list of messages.
    private Dictionary<int, List<string>> privateMessages = new Dictionary<int, List<string>>();


    [SerializeField] TextMeshProUGUI fullChatText;
    [SerializeField] TMP_InputField chatMessageInputText;

    [SerializeField] GameObject chatParent;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        chatParent.SetActive(false); // Hide chat at first
        publicMessages = new List<string>();
    }

    void SetChatActive(bool active)
    {
        chatParent.SetActive(active);
    }

    // Update is called once per frame
    void Update()
    {
        if (!HasStateAuthority) // Another alternative would be to move this to FixedUpdateNetwork, I guess
            return;

        if (Input.GetKeyDown(KeyCode.LeftControl) || Input.GetKeyDown(KeyCode.RightControl)) /*Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))*/
        {
            chatParent.SetActive(!chatParent.activeInHierarchy); // Toggle chat parent activity
        }
    }

    public void OnInputFinished()
    {
        // Only process input if this is the state authority
        //Debug.Log("CHAT: Input finished for object " + name + ", hasStateAuthority: " + HasStateAuthority);
        
        if (!HasStateAuthority)
            return;
        
        // Check for Enter key press
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            int playerId = Runner.LocalPlayer.PlayerId;
            string newMessage = "User_" + playerId + ": " + chatMessageInputText.text;
            chatMessageInputText.text = ""; // Reset the input field
            Debug.Log("CHAT: ENTER PRESSED, following message has been written: " + newMessage);

            // Call an RPC so that all clients update their chat messages
            SendChatMessageRpc(newMessage);
        }
    }

    // RPC: Called on the state authority, executed on all clients.
    // -1 = public message, no filtering within the recipients
    [Rpc(RpcSources.All, RpcTargets.All)]
    void SendChatMessageRpc(string newMessage, int targetPlayerId = -1, RpcInfo info = default)
    {        
        Debug.Log("CHAT: Send chat RPC called with " + newMessage + ", executed on all clients. Source: " + info.Source + " is local invoke: " + info.IsInvokeLocal);

        //if (targetPlayedId == -1) // public message, append to chatMessages
        //else
        // AddPrivateMessage(targetPlayerId, newMessage);

        publicMessages.Add(newMessage);
        fullChatText.text = string.Join("\n", publicMessages.ToArray());

        Debug.Log("CHAT: Full chat text is now " + fullChatText.text);
    }


    
    void AddPrivateMessage(int targetPlayerId, string message)
    {
        if (!privateMessages.ContainsKey(targetPlayerId))        
            privateMessages[targetPlayerId] = new List<string>();
        
        privateMessages[targetPlayerId].Add(message);
    }
}
