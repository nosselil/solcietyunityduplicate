using TMPro;
using UnityEngine;
using Solana.Unity.Rpc;
using System.Collections;
using System.Collections.Generic;
using Solana.Unity.Rpc.Models;
using Solana.Unity.Wallet;
using System;
using Solana.Unity.SDK;
using Solana.Unity.Extensions.Models.TokenMint;
using Solana.Unity.SDK.Utility;
using System.IO;
using Solana.Unity.SDK.Example;
using Cysharp.Threading.Tasks;

public class Inventory : MonoBehaviour
{
    public TextMeshProUGUI LatestTransction;
    public TMP_Dropdown TokenDropdown;
    public TextMeshProUGUI WalletAddress;
    
    // Token icons will be fetched automatically
    private Dictionary<string, Sprite> tokenIcons = new Dictionary<string, Sprite>();
    //public GameObject wallet;
    private IRpcClient rpcClient; // RpcClient interface
    public string walletAddress;
    
    // Token mint addresses
    private Dictionary<string, string> tokenMints = new Dictionary<string, string>
    {
        {"SOL", ""}, // SOL doesn't have a mint address
        {"USDC", "EPjFWdd5AufqSSqeM2qN1xzybapC8G4wEGGkZwyTDt1v"},
        {"USDT", "Es9vMFrzaCERmJfrF4H2FYD4KCoNkY11McCe8BenwNYB"},
        {"BONK", "DezXAZ8z7PnrnRJjz3wXBoRgixCa6xjnB7YaB1pPB263"},
        {"JUP", "JUPyiwrYJFskUPiHa7hkeR8VUtAeFoSYbKedZNsDvCN"}
    };
    
    private Dictionary<string, double> tokenBalances = new Dictionary<string, double>();
    
    void Start()
    {
   //     WalletManager.instance.SeeTran();
       LatestTransction.text = WalletManager.instance.latestTransaction;
        WalletAddress.text = WalletManager.instance.walletAddress;
        
        // Setup dropdown
        SetupTokenDropdown();
        
        // Initialize balances
        StartCoroutine(InitializeTokenBalances());
    }

    private void OnEnable()
    {
        WalletManager.instance.FetchLatestTransactions();
        WalletManager.instance.CheckBalance();
        StartCoroutine(UpdateTransactionUI());
        StartCoroutine(UpdateTokenBalances());
    }
    
    void SetupTokenDropdown()
    {
        TokenDropdown.ClearOptions();
        TokenDropdown.onValueChanged.AddListener(OnTokenSelected);
        
        // Make dropdown interactive so users can see the options
        TokenDropdown.interactable = true;
    }
    
    void OnTokenSelected(int index)
    {
        // Allow selection changes but keep track of what was selected
        Debug.Log($"Selected token: {TokenDropdown.options[index].text}");
    }
    
    IEnumerator InitializeTokenBalances()
    {
        yield return new WaitForSeconds(1f); // Wait for wallet to be ready
        
        // Get SOL balance
        tokenBalances["SOL"] = WalletManager.instance.Balance;
        
        // Fetch token icons
        yield return StartCoroutine(FetchTokenIcons());
        
        // Get SPL token balances
        yield return StartCoroutine(FetchSPLTokenBalances());
        
        // Update dropdown with all balances
        UpdateDropdownWithBalances();
    }
    
    IEnumerator UpdateTokenBalances()
    {
        yield return new WaitForSeconds(2f);
        
        // Update SOL balance
        tokenBalances["SOL"] = WalletManager.instance.Balance;
        
        // Update SPL token balances
        yield return StartCoroutine(FetchSPLTokenBalances());
        
        // Update dropdown with all balances
        UpdateDropdownWithBalances();
    }
    
    IEnumerator FetchSPLTokenBalances()
    {
        if (string.IsNullOrEmpty(WalletManager.instance.walletAddress))
        {
            Debug.LogWarning("Wallet address is null or empty");
            yield break;
        }
        
        // Reset all SPL token balances to 0
        foreach (var token in tokenMints.Keys)
        {
            if (token != "SOL")
            {
                tokenBalances[token] = 0;
            }
        }
        
        // Use Web3.Wallet.GetTokenAccounts to get all token accounts
        var getTokensTask = Web3.Wallet.GetTokenAccounts(Solana.Unity.Rpc.Types.Commitment.Processed);
        
        // Wait for the task to complete
        while (!getTokensTask.IsCompleted)
        {
            yield return null;
        }
        
        if (getTokensTask.IsCompletedSuccessfully && getTokensTask.Result != null)
        {
            var tokens = getTokensTask.Result;
            
            foreach (var token in tokens)
            {
                if (token.Account.Data.Parsed != null)
                {
                    var info = token.Account.Data.Parsed.Info;
                    string mintAddress = info.Mint;
                    
                    // Find which token this mint corresponds to
                    foreach (var tokenPair in tokenMints)
                    {
                        if (tokenPair.Value == mintAddress)
                        {
                            double balance = (double)info.TokenAmount.AmountUlong / Math.Pow(10, info.TokenAmount.Decimals);
                            tokenBalances[tokenPair.Key] = balance;
                            Debug.Log($"Found {tokenPair.Key} balance: {balance}");
                            break;
                        }
                    }
                }
            }
        }
        else
        {
            Debug.LogWarning("Failed to fetch token accounts or no accounts found");
        }
    }
    
    void UpdateDropdownWithBalances()
    {
        TokenDropdown.ClearOptions();
        List<string> dropdownOptions = new List<string>();
        
        // Create options with Unicode symbols as visual indicators
        foreach (var token in tokenMints.Keys)
        {
            double balance = tokenBalances.ContainsKey(token) ? tokenBalances[token] : 0;
            string iconSymbol = GetTokenIconSymbol(token);
            string optionText = $"{iconSymbol} {token}: {balance:F4}";
            dropdownOptions.Add(optionText);
        }
        
        TokenDropdown.AddOptions(dropdownOptions);
        
        // Set default selection to SOL (index 0)
        TokenDropdown.value = 0;
    }
    
    string GetTokenIconSymbol(string tokenName)
    {
        // No symbols at all
        return "";
    }
    
    Sprite GetTokenIcon(string tokenName)
    {
        if (tokenIcons.ContainsKey(tokenName))
        {
            return tokenIcons[tokenName];
        }
        return null;
    }
    
    IEnumerator FetchTokenIcons()
    {
        // Get token mint resolver
        var tokenMintResolverTask = WalletScreen.GetTokenMintResolver();
        
        // Wait for the task to complete
        while (!tokenMintResolverTask.Status.IsCompleted())
        {
            yield return null;
        }
        
        if (tokenMintResolverTask.Status == UniTaskStatus.Succeeded)
        {
            var tokenMintResolver = tokenMintResolverTask.GetAwaiter().GetResult();
            
            // Fetch icons for each token
            foreach (var tokenPair in tokenMints)
            {
                if (tokenPair.Key == "SOL") continue; // SOL doesn't have a mint address
                
                var tokenDef = tokenMintResolver.Resolve(tokenPair.Value);
                if (!string.IsNullOrEmpty(tokenDef.TokenLogoUrl))
                {
                    yield return StartCoroutine(LoadAndCacheTokenIcon(tokenDef.TokenLogoUrl, tokenPair.Key, tokenPair.Value));
                }
            }
        }
        else
        {
            Debug.LogWarning("Failed to get token mint resolver");
        }
    }
    
    IEnumerator LoadAndCacheTokenIcon(string logoUrl, string tokenName, string tokenMint)
    {
        if (string.IsNullOrEmpty(logoUrl) || string.IsNullOrEmpty(tokenMint)) yield break;
        
        // Load the texture
        var loadFileTask = FileLoader.LoadFile<Texture2D>(logoUrl);
        
        // Wait for the task to complete
        while (!loadFileTask.IsCompleted)
        {
            yield return null;
        }
        
        if (loadFileTask.IsCompletedSuccessfully)
        {
            var texture = loadFileTask.Result;
            if (texture != null)
            {
                // Resize texture
                var resizedTexture = FileLoader.Resize(texture, 75, 75);
                
                // Save to persistent data path
                FileLoader.SaveToPersistentDataPath(Path.Combine(Application.persistentDataPath, $"{tokenMint}.png"), resizedTexture);
                
                // Convert to Sprite and store
                var sprite = Sprite.Create(resizedTexture, new Rect(0, 0, resizedTexture.width, resizedTexture.height), new Vector2(0.5f, 0.5f));
                tokenIcons[tokenName] = sprite;
                
                Debug.Log($"Loaded icon for {tokenName}: {logoUrl}");
            }
        }
        else
        {
            Debug.LogWarning($"Failed to load icon for {tokenName}: {logoUrl}");
        }
    }
    
    IEnumerator UpdateTransactionUI()
    {
        yield return new WaitForSeconds(2f); // Allow time for async transaction fetch

        List<string> transactions = WalletManager.instance.latestTransactions;

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
