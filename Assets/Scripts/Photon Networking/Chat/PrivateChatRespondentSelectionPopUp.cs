using UnityEngine;

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

    public void GenerateRespondentButtons(string[] respondents)
    {
        if (respondents.Length == 0)        
            noRespondentsAvailableText.SetActive(true);
        else
        {
            GameObject newRespondentButtonGO = Instantiate(respondentButtonPrefab);
            newRespondentButtonGO.transform.SetParent(respondentButtonListParent.transform);
        }

    }
}
