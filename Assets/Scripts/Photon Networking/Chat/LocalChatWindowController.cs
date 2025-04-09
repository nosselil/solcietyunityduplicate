using NUnit.Framework;
using PixelCrushers.DialogueSystem.OpenAIAddon;
using Solana.Unity.Metaplex.MplNftPacks.Program;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LocalChatWindowController : MonoBehaviour
{
    // TODO: This class could include more logic from Multiplayer.cs as well. Refactor later?

    public int activeChatWindowIndex = 0; // 0 is the public chat, other chats are private chats with ids > 0 

    [SerializeField] GameObject chatWindowTabParent;
    [SerializeField] GameObject privateChatWindowTabPrefab;

    [SerializeField] GameObject privateChatRecipientSelectionPopUp;

    [SerializeField] TextMeshProUGUI fullChatText;

    public List<string> chatWindowPlayerIds = new();

    public static LocalChatWindowController Instance { get; private set; }

    public bool IsChatWindowActive => transform.GetChild(0).gameObject.activeSelf;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Instance = this;

        chatWindowPlayerIds.Add("public"); // First tab is always the public chat
        activeChatWindowIndex = 0;

        OpenPrivateChatRecipientSelectionPopUp(false); // Close this at first
        ActivateCurrentChatWindow();
    }

    // Bring up a pop-up that lists the players who you want to chat with    
    public void OpenPrivateChatRecipientSelectionPopUp(bool active)
    {        
        privateChatRecipientSelectionPopUp.SetActive(active);

        if (active)
            privateChatRecipientSelectionPopUp.GetComponent<PrivateChatRecipientSelectionPopUp>().GenerateRecipientButtons();
        
    }

    public void SendChatMessage()
    {        
        MultiplayerChat.Instance.PrepareMessageForSending();
    }

    public void ActivateChatWindow(int chatWindowIndex)
    {
        activeChatWindowIndex = chatWindowIndex;
        ActivateCurrentChatWindow();
    }

    public void OnEnabled()
    {
        ActivateCurrentChatWindow(); // Fetch the texts for the current window when this is re-opened
    }

    public void StartNewPrivateChat(string recipientName)
    {
        AddNewPrivateChatWindow(recipientName);
        //SetActiveChatWindow
        //Set chatWindowText, if not done in setActiveChatWindow
        OpenPrivateChatRecipientSelectionPopUp(false); // Close the selection pop-up
    }

    public string GetActivePrivateChatWindowPlayerId()
    {
        return chatWindowPlayerIds[activeChatWindowIndex];
    }

    public void AddNewPrivateChatWindow(string newPlayerId, bool setAsActiveWindow = true)
    {
        chatWindowPlayerIds.Add(newPlayerId);

        if (setAsActiveWindow)
            activeChatWindowIndex = chatWindowPlayerIds.Count - 1; // Make the last chat window active

        GameObject newPrivateChatWindowTab = Instantiate(privateChatWindowTabPrefab);
        newPrivateChatWindowTab.transform.SetParent(chatWindowTabParent.transform, false);

        // Calculate the desired sibling index. We want to insert the new tab as the second-to-last child.
        int siblingIndex = chatWindowTabParent.transform.childCount - 2;
        newPrivateChatWindowTab.transform.SetSiblingIndex(siblingIndex);

        if (!MultiplayerChat.Instance.chatMessages.ContainsKey(newPlayerId))
            MultiplayerChat.Instance.chatMessages[newPlayerId] = new List<string>();

        // Find the TextMeshProUGUI component on the new tab (assumes it's in the children) and set its text.
        TextMeshProUGUI tabText = newPrivateChatWindowTab.GetComponentInChildren<TextMeshProUGUI>();        
        tabText.text = newPlayerId;

        // Bind the button's onClick event so it calls ActivateChatWindow with the sibling index.
        Button button = newPrivateChatWindowTab.GetComponent<Button>();
        button.onClick.AddListener(() =>
        {
            Debug.Log("Recipient tab clicked, sibling index: " + siblingIndex);
            ActivateChatWindow(siblingIndex);
        });
        
        if (setAsActiveWindow)
            ActivateCurrentChatWindow();
    }


    public string GetActiveChatWindowRecipientId()
    {
        return chatWindowPlayerIds[activeChatWindowIndex];
    }

    public void SetCurrentChatWindowText()
    {
        string currentWindowRecipientId = chatWindowPlayerIds[activeChatWindowIndex];
        Debug.Log("CHAT: Current window recipient id is " + currentWindowRecipientId);

        if (MultiplayerChat.Instance.chatMessages.TryGetValue(currentWindowRecipientId, out List<string> chatMessages))
            fullChatText.text = string.Join("\n", chatMessages.ToArray());
        else
        {
            Debug.Log("CHAT: No chat history for recipient " + currentWindowRecipientId);
            fullChatText.text = "";
        }
        
        Debug.Log("CHAT: Full chat text is now: " + fullChatText.text);
    }



    void ActivateCurrentChatWindow()
    {
        SetCurrentChatWindowText();

        // Activate all other tab buttons except the button that opens the currently active window
        for (int i = 0; i < chatWindowTabParent.transform.childCount; i++)        // TODO: Cache the buttons
            chatWindowTabParent.transform.GetChild(i).gameObject.GetComponent<Button>().interactable = (i != activeChatWindowIndex);
        
    }

}
