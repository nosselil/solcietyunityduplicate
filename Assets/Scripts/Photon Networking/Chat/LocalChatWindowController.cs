using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class LocalChatWindowController : MonoBehaviour
{
    // TODO: This class could include more logic from Multiplayer.cs as well. Refactor later?

    int activeChatWindow = 0; // 0 is the public chat, other chats are private chats with ids > 0 

    [SerializeField] GameObject chatWindowTabParent;
    [SerializeField] GameObject privateChatWindowTabPrefab;

    [SerializeField] GameObject privateChatRespondentSelectionPopUp;

    List<string> chatWindowPlayerIds = new();

    public void AddNewPrivateChatWindow(string newPlayerId)
    {
        chatWindowPlayerIds.Add(newPlayerId); // This will link the index of the current tab to a specific user / walletAddress
        GameObject newPrivateChatWindowTab = Instantiate(privateChatWindowTabPrefab);
        newPrivateChatWindowTab.transform.SetParent(chatWindowTabParent.transform);
    }

    public void OnNewPrivateChatButtonClicked()
    {
        // Bring up a pop-up that lists the players who you want to chat with
        OpenPrivateChatRespondentSelectionPopUp();
    }

    void OpenPrivateChatRespondentSelectionPopUp()
    {
        privateChatRespondentSelectionPopUp.SetActive(true);
        string[] activePlayerList = MultiplayerChat.Instance.GetActivePlayerList();
        privateChatRespondentSelectionPopUp.GetComponent<PrivateChatRespondentSelectionPopUp>().
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        chatWindowPlayerIds.Add("public"); // First tab is always the public chat
        activeChatWindow = 0;
    }

    void SetActiveChatWindow(int activeChatWindowIndex)
    {
        // Hide the 

        // Activate all other tab buttons except the button that opens the currently active window
        for (int i = 0; i < chatWindowTabParent.transform.childCount; i++)        
            chatWindowTabParent.transform.GetChild(i).gameObject.SetActive(i != activeChatWindow);
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
