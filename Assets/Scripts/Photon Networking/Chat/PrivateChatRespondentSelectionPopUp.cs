using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PrivateChatRecipientSelectionPopUp : MonoBehaviour
{
    [SerializeField] GameObject recipientButtonListParent;
    [SerializeField] GameObject noRecipientsAvailableText;

    [SerializeField] GameObject recipientButtonPrefab; // TODO: Object pooling could be used to prevent instantion but doesn't optimize that much

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void GenerateRecipientButtons()
    {
        // Clear previous buttons.
        foreach (Transform child in recipientButtonListParent.transform)
            Destroy(child.gameObject);

        string[] recipients = MultiplayerChat.Instance.GetActivePlayerList();

        // If there are no recipients (other than ourselves), show "no recipients available" text.
        // Note: recipients array includes the local wallet as well.
        if (recipients.Length <= 1)
        {
            noRecipientsAvailableText.SetActive(true);
            return;
        }

        int buttonsCreated = 0;
        noRecipientsAvailableText.SetActive(false);

        // Loop through each recipient in the list
        foreach (string recipient in recipients)
        {
            // Skip creating a button for the local player
            if (recipient == MultiplayerChat.Instance.localWalletAddress || MultiplayerChat.Instance.chatMessages.ContainsKey(recipient))
                continue;
            
            // Instantiate a new recipient button.
            GameObject newRecipientButtonGO = Instantiate(recipientButtonPrefab);
            newRecipientButtonGO.transform.SetParent(recipientButtonListParent.transform, false);

            // Set the button's text to the recipient's wallet address.
            TextMeshProUGUI buttonText = newRecipientButtonGO.GetComponentInChildren<TextMeshProUGUI>();          
            buttonText.text = recipient;
            

            // Dynamically bind OnRecipientButtonClicked() to the button's OnClick event
            Button button = newRecipientButtonGO.GetComponent<Button>();
            button.onClick.AddListener(() =>
            {
                OnRecipientButtonClicked();
            });

            buttonsCreated++;
        }

        // If no buttons were created, then show "no recipients available" text
        if (buttonsCreated == 0)        
            noRecipientsAvailableText.SetActive(true);        
    }


    public void OnRecipientButtonClicked()
    {
        Debug.Log("OnRecipientButtonClicked triggered.");
        // Get the text component from this button's children.
        TextMeshProUGUI buttonText = GetComponentInChildren<TextMeshProUGUI>();
        if (buttonText != null)
        {
            Debug.Log("Text component found on button: " + buttonText.text);
            string recipientName = buttonText.text;
            Debug.Log("Recipient name extracted: " + recipientName);
            LocalChatWindowController.Instance.StartNewPrivateChat(recipientName);
        }
        else
        {
            Debug.LogWarning("OnRecipientButtonClicked: No TextMeshProUGUI component found on button's children.");
        }
    }

}
