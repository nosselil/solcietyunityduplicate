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
        // Clear the previous button list
        foreach (Transform child in respondentButtonListParent.transform)        
            Destroy(child.gameObject);
        
        string[] respondents = MultiplayerChat.Instance.GetActivePlayerList();

        // If there are no respondents (other than ourselves), show the "no respondents available" text.
        if (respondents.Length <= 1)
        {
            noRespondentsAvailableText.SetActive(true);
        }
        else
        {
            noRespondentsAvailableText.SetActive(false);

            // Loop through each respondent in the list.
            foreach (string respondent in respondents)
            {
                if (respondent == MultiplayerChat.Instance.localWalletAddress) // Skip creating a buttons for the local player
                    continue;

                GameObject newRespondentButtonGO = Instantiate(respondentButtonPrefab);
                
                newRespondentButtonGO.transform.SetParent(respondentButtonListParent.transform);
                
                TextMeshProUGUI buttonText = newRespondentButtonGO.GetComponentInChildren<TextMeshProUGUI>();
                if (buttonText != null)                
                    buttonText.text = respondent;

                Button button = newRespondentButtonGO.GetComponent<Button>();
                if (button != null)
                {
                    Debug.Log("Button component found on respondent button prefab. Adding click listener.");
                    button.onClick.AddListener(() =>
                    {
                        Debug.Log("Respondent button clicked. Calling OnRespondentButtonClicked.");
                        OnRespondentButtonClicked();
                    });
                }
                else
                {
                    Debug.LogWarning("Button component not found on respondent button prefab.");
                }
            }
        }
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
