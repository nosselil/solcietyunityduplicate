using TMPro;
using UnityEngine;
using Solana.Unity.SDK;

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

    private void OnEnable()
    {
        // Subscribe to automatic balance change events
        Web3.OnBalanceChange += OnBalanceChanged;
    }

    private void OnDisable()
    {
        // Unsubscribe to prevent memory leaks
        Web3.OnBalanceChange -= OnBalanceChanged;
    }

    // This method will be called automatically whenever the wallet balance changes
    private void OnBalanceChanged(double newBalance)
    {
        Debug.Log($"💰 Balance automatically updated: {newBalance} SOL");
        
        // Update the UI on the main thread
        if (CurrentBalance != null)
        {
            CurrentBalance.text = newBalance.ToString("F2");
        }
        
        // Also update the WalletManager's balance for consistency
        if (WalletManager.instance != null)
        {
            WalletManager.instance.Balance = (float)newBalance;
        }
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