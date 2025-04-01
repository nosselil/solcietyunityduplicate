using UnityEngine;
using TMPro;

public class loadingPanel : MonoBehaviour
{
    public TMP_Text loadingTxt;
    public UserBalance userBalance; // Reference to the UserBalance script

    private void OnDisable()
    {
        // Reset the loading text
        loadingTxt.text = "Please wait...";

        // Call the UpdateTheBalance function from the UserBalance script
        if (userBalance != null)
        {
            userBalance.UpdateTheBalance();
        }
        else
        {
            Debug.LogError("UserBalance reference is not set in loadingPanel!");
        }
    }
}