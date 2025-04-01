using TMPro;
using UnityEngine;
using Solana.Unity.Rpc;
using System.Collections;
using System.Collections.Generic;

public class Inventory : MonoBehaviour
{
    public TextMeshProUGUI LatestTransction;
    public TextMeshProUGUI CurrentBalance;
    public TextMeshProUGUI WalletAddress;
    //public GameObject wallet;
    private IRpcClient rpcClient; // RpcClient interface
    public string walletAddress;
    void Start()
    {
   //     WalletManager.instance.SeeTran();
       LatestTransction.text = WalletManager.instance.latestTransaction;
        CurrentBalance.text = WalletManager.instance.Balance.ToString("F2");

      
        WalletAddress.text = WalletManager.instance.walletAddress;

    }

    private void OnEnable()
    {
        WalletManager.instance.FetchLatestTransactions();
        WalletManager.instance.CheckBalance();
        StartCoroutine(UpdateTransactionUI());
    }
    IEnumerator UpdateTransactionUI()
    {
        yield return new WaitForSeconds(2f); // Allow time for async transaction fetch

        List<string> transactions = WalletManager.instance.latestTransactions;
        CurrentBalance.text = WalletManager.instance.Balance.ToString("F2");

        // Ensure at least 3 transactions are displayed
        int count = transactions.Count < 3 ? transactions.Count : 3;

        LatestTransction.text = ""; // Clear old text
        for (int i = 0; i < count; i++)
        {
            LatestTransction.text += (i + 1) + ". " + transactions[i] + "\n"; // Append each transaction
        }

        Debug.Log("Updated Transactions UI");
    }



}
