using TMPro;
using UnityEngine;

public class UserBalance : MonoBehaviour
{
    public TextMeshProUGUI CurrentBalance;

    void Start()
    {
                 WalletManager.instance.CheckBalance();

        if (CurrentBalance == null)
        {
            Debug.LogError("CurrentBalance is not assigned!");
            return;
        }
        UpdateBalance();
    }

    public void UpdateTheBalance()
    {
        Debug.Log("UpdateTheBalance called!");
         WalletManager.instance.CheckBalance();

        UpdateBalance();
    }

    private void UpdateBalance()
    {
        if (WalletManager.instance == null)
        {
            Debug.LogError("bbbbWalletManager instance is null!");


            CurrentBalance.text = "0.00"; // Fallback value
            return;
        }

        try
        {
            WalletManager.instance.CheckBalance();

            Debug.Log($"bbbbUpdating balance to: {WalletManager.instance.Balance}");
            
            CurrentBalance.text = WalletManager.instance.Balance.ToString("F2");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"bbbbError updating balance: {ex.Message}");
            CurrentBalance.text = "0.00"; // Fallback value
        }
    }
}