using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PrivateChatRespondentSelectionPopUp : MonoBehaviour
{
    [SerializeField] GameObject respondentButtonListParent;
    [SerializeField] GameObject noRespondentsAvailableText;

    [SerializeField] GameObject respondentButtonPrefab; // TODO: Object pooling could be used to prevent instantion but doesn't optimize that much

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void GenerateRespondentButtons()
    {
        // Clear previous buttons.
        foreach (Transform child in respondentButtonListParent.transform)
            Destroy(child.gameObject);

        string[] respondents = MultiplayerChat.Instance.GetActivePlayerList();

        // If there are no respondents (other than ourselves), show "no respondents available" text.
        // Note: respondents array includes the local wallet as well.
        if (respondents.Length <= 1)
        {
            noRespondentsAvailableText.SetActive(true);
            return;
        }

        int buttonsCreated = 0;
        noRespondentsAvailableText.SetActive(false);

        // Loop through each respondent in the list
        foreach (string respondent in respondents)
        {
            // Skip creating a button for the local player
            if (respondent == MultiplayerChat.Instance.localWalletAddress || MultiplayerChat.Instance.chatMessages.ContainsKey(respondent))
                continue;
            
            // Instantiate a new respondent button.
            GameObject newRespondentButtonGO = Instantiate(respondentButtonPrefab);
            newRespondentButtonGO.transform.SetParent(respondentButtonListParent.transform, false);

            // Set the button's text to the respondent's wallet address.
            TextMeshProUGUI buttonText = newRespondentButtonGO.GetComponentInChildren<TextMeshProUGUI>();          
            buttonText.text = respondent;
            

            // Dynamically bind OnRespondentButtonClicked() to the button's OnClick event
            Button button = newRespondentButtonGO.GetComponent<Button>();
            button.onClick.AddListener(() =>
            {
                OnRespondentButtonClicked();
            });

            buttonsCreated++;
        }

        // If no buttons were created, then show "no respondents available" text
        if (buttonsCreated == 0)        
            noRespondentsAvailableText.SetActive(true);        
    }


    public void OnRespondentButtonClicked()
    {
        Debug.Log("OnRespondentButtonClicked triggered.");
        // Get the text component from this button's children.
        TextMeshProUGUI buttonText = GetComponentInChildren<TextMeshProUGUI>();
        if (buttonText != null)
        {
            Debug.Log("Text component found on button: " + buttonText.text);
            string respondentName = buttonText.text;
            Debug.Log("Respondent name extracted: " + respondentName);
            LocalChatWindowController.Instance.StartNewPrivateChat(respondentName);
        }
        else
        {
            Debug.LogWarning("OnRespondentButtonClicked: No TextMeshProUGUI component found on button's children.");
        }
    }

}
