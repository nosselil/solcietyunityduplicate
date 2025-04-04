using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class LocalChatWindowController : MonoBehaviour
{
    // TODO: This class could include more logic from Multiplayer.cs as well. Refactor later?

    int activeChatWindow = 0; // 0 is the public chat, other chats are private chats with ids > 0 

    [SerializeField] GameObject chatWindowTabParent;
    [SerializeField] GameObject privateChatWindowTabPrefab;

    [SerializeField] GameObject privateChatRespondentSelectionPopUp;

    [SerializeField] TextMeshProUGUI fullChatText;

    List<string> chatWindowPlayerIds = new();

    public static LocalChatWindowController Instance { get; private set; }

    // Bring up a pop-up that lists the players who you want to chat with    
    public void OpenPrivateChatRespondentSelectionPopUp(bool active)
    {        
        privateChatRespondentSelectionPopUp.SetActive(active);

        if (active)
            privateChatRespondentSelectionPopUp.GetComponent<PrivateChatRespondentSelectionPopUp>().GenerateRespondentButtons();
        
    }

    public void StartNewPrivateChat(string respondentName)
    {
        AddNewPrivateChatWindow(respondentName);
        //SetActiveChatWindow
        //Set chatWindowText, if not done in setActiveChatWindow
        OpenPrivateChatRespondentSelectionPopUp(false); // Close the selection pop-up
    }

    public void AddNewPrivateChatWindow(string newPlayerId)
    {
        chatWindowPlayerIds.Add(newPlayerId);

        GameObject newPrivateChatWindowTab = Instantiate(privateChatWindowTabPrefab);
        newPrivateChatWindowTab.transform.SetParent(chatWindowTabParent.transform, false);

        // Set as the second-to-last child.
        int childCount = chatWindowTabParent.transform.childCount;        
        newPrivateChatWindowTab.transform.SetSiblingIndex(childCount - 2);

        // Find the TextMeshProUGUI component on the new tab (assumes it's in the children).
        TextMeshProUGUI tabText = newPrivateChatWindowTab.GetComponentInChildren<TextMeshProUGUI>();
        if (tabText != null)
            tabText.text = newPlayerId;
    }


    public void SetChatText(List<string> chatMessages)
    {
        // TODO: We should be checking the active window here first before replacing the text
        fullChatText.text = string.Join("\n", chatMessages.ToArray());
        Debug.Log("CHAT: Full public chat text is now: " + fullChatText.text);
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Instance = this;

        chatWindowPlayerIds.Add("public"); // First tab is always the public chat
        activeChatWindow = 0;

        OpenPrivateChatRespondentSelectionPopUp(false); // Close this at first
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
