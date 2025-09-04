using System;
using System.Collections;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Solana.Unity.Dex;
using Solana.Unity.Dex.Jupiter;
using Solana.Unity.Dex.Models;
using Solana.Unity.Dex.Quotes;
using Solana.Unity.SDK;
using Solana.Unity.Rpc.Models;
using Solana.Unity.Rpc.Core.Http;
using Solana.Unity.Extensions.Models.TokenMint;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Solana.Unity.SDK.Example;
using Solana.Unity.Extensions;
using UnityEngine.Networking;

public class JupiterSDKManager : MonoBehaviour
{
    [Header("UI References")]
    public TMP_InputField amountInput;
    public TMP_Text resultText;
    public TMP_Text usdcEstimateText;
    public TMP_Text balanceText;
    public TMP_Text routeText;
    public GameObject loadingScreen;
    public TMP_Dropdown tokenADropdown;
    public TMP_Dropdown tokenBDropdown;

    [Header("Network Settings")]
    public int maxRetries = 3;
    public float retryDelay = 2f;

    private IDexAggregator _dex;
    private TokenData _tokenA;
    private TokenData _tokenB;
    private SwapQuoteAg _currentQuote;
    private float _currentBalance = 0f; // Store current balance for max amount functionality

    // Common token symbols for dropdown
    private readonly string[] _commonTokens = { "SOL", "USDC", "USDT", "BONK", "JUP", "RAY", "SRM" };
    
    // Store user tokens with their information
    private List<UserTokenInfo> _userTokens = new List<UserTokenInfo>();
    private TokenMintResolver _tokenMintResolver;

    // Class to store user token information
    [System.Serializable]
    public class UserTokenInfo
    {
        public string MintAddress;
        public string Symbol;
        public string Name;
        public int Decimals;
        public float Balance;
        public string LogoUrl;
    }

    private void Start()
    {
        InitializeUI();
        UpdateBalance();
        amountInput.onValueChanged.AddListener(_ => OnAmountChanged());
        StartCoroutine(InitializeDexCoroutine());
    }

    private IEnumerator InitializeDexCoroutine()
    {
        yield return StartCoroutine(InitializeDex());

        // After initialization is complete, update the balance for the default token
        UpdateBalance();

        // Optionally, get a quote if there's an amount
        if (!string.IsNullOrEmpty(amountInput.text))
        {
            StartCoroutine(GetQuoteCoroutine());
        }
    }

    private IEnumerator InitializeDex()
    {
        if (Web3.Account == null)
        {
            Debug.LogError("❌ Web3.Account is null. Make sure wallet is connected.");
            yield break;
        }

        _dex = new JupiterDexAg(Web3.Account);
        
        // Initialize token mint resolver
        yield return StartCoroutine(InitializeTokenMintResolver());
        
        // Get user tokens
        yield return StartCoroutine(LoadUserTokens());
        
        // Get default tokens (SOL and USDC) with retry logic
        yield return StartCoroutine(GetTokenWithRetry("SOL", (token) => _tokenA = token));
        if (_tokenA == null) yield break;
        
        yield return StartCoroutine(GetTokenWithRetry("USDC", (token) => _tokenB = token));
        if (_tokenB == null) yield break;
        
        Debug.Log($"✅ Jupiter SDK initialized. Token A: {_tokenA.Symbol}, Token B: {_tokenB.Symbol}");
    }

    private IEnumerator InitializeTokenMintResolver()
    {
        var resolverTask = WalletScreen.GetTokenMintResolver();
        
        while (!resolverTask.Status.IsCompleted())
        {
            yield return null;
        }
        
        if (resolverTask.Status == UniTaskStatus.Succeeded)
        {
            _tokenMintResolver = resolverTask.GetAwaiter().GetResult();
            Debug.Log("✅ Token mint resolver initialized");
        }
        else
        {
            Debug.LogWarning("⚠️ Failed to initialize token mint resolver");
        }
    }

    private IEnumerator LoadUserTokens()
    {
        if (Web3.Account == null) yield break;

        var getTokensTask = Web3.Wallet.GetTokenAccounts(Solana.Unity.Rpc.Types.Commitment.Processed);
        
        while (!getTokensTask.IsCompleted)
        {
            yield return null;
        }

        if (getTokensTask.IsCompletedSuccessfully && getTokensTask.Result != null)
        {
            _userTokens.Clear();
            
            foreach (var tokenAccount in getTokensTask.Result)
            {
                var info = tokenAccount.Account.Data.Parsed.Info;
                
                // Debug: Show complete JSON data for this token
                Debug.Log($"🔍 COMPLETE TOKEN DATA (JSON):");
                try
                {
                    // Convert the entire token account to JSON
                    string tokenAccountJson = JsonUtility.ToJson(tokenAccount, true);
                    Debug.Log($"Token Account JSON:\n{tokenAccountJson}");
                    
                    // Also show just the parsed info as JSON
                    string parsedInfoJson = JsonUtility.ToJson(info, true);
                    Debug.Log($"Parsed Info JSON:\n{parsedInfoJson}");
                    
                    // Show the raw RPC response if available
                    if (tokenAccount.Account.Data.Parsed != null)
                    {
                        string parsedDataJson = JsonUtility.ToJson(tokenAccount.Account.Data.Parsed, true);
                        Debug.Log($"Parsed Data JSON:\n{parsedDataJson}");
                    }
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"⚠️ Failed to serialize token data to JSON: {e.Message}");
                    
                    // Fallback to manual logging
                    Debug.Log($"🔍 RAW TOKEN DATA (fallback):");
                    Debug.Log($"  Mint Address: {info.Mint}");
                    Debug.Log($"  Token Amount (raw): {info.TokenAmount.Amount}");
                    Debug.Log($"  Token Amount (ulong): {info.TokenAmount.AmountUlong}");
                    Debug.Log($"  Token Amount (decimal): {info.TokenAmount.AmountDecimal}");
                    Debug.Log($"  Token Amount (ui amount): {info.TokenAmount.UiAmount}");
                    Debug.Log($"  Token Amount (ui amount string): {info.TokenAmount.UiAmountString}");
                    Debug.Log($"  Decimals: {info.TokenAmount.Decimals}");
                    Debug.Log($"  Owner: {info.Owner}");
                    Debug.Log($"  State: {info.State}");
                    Debug.Log($"  Token Account: {tokenAccount.PublicKey}");
                    Debug.Log($"  Account Lamports: {tokenAccount.Account.Lamports}");
                    Debug.Log($"  Account Owner: {tokenAccount.Account.Owner}");
                    Debug.Log($"  Account Executable: {tokenAccount.Account.Executable}");
                    Debug.Log($"  Account Rent Epoch: {tokenAccount.Account.RentEpoch}");
                }
                Debug.Log($"  ────────────────────────────────────────────────");
                
                // Skip tokens with zero balance
                if (info.TokenAmount.AmountUlong == 0) 
                {
                    Debug.Log($"⚠️ Skipping token {info.Mint} - zero balance");
                    continue;
                }
                
                // Skip NFTs (tokens with 0 decimals and amount = 1)
                if (info.TokenAmount.Decimals == 0 && info.TokenAmount.AmountUlong == 1) 
                {
                    Debug.Log($"⚠️ Skipping token {info.Mint} - appears to be NFT");
                    continue;
                }
                
                var userToken = new UserTokenInfo
                {
                    MintAddress = info.Mint,
                    Decimals = info.TokenAmount.Decimals,
                    Balance = (float)(info.TokenAmount.AmountUlong / Math.Pow(10, info.TokenAmount.Decimals))
                };
                
                Debug.Log($"✅ Processing token: {userToken.MintAddress} with balance: {userToken.Balance}");
                
                // Try to get token information from Jupiter API first
                Debug.Log($"🔍 Starting Jupiter lookup for mint: {info.Mint}");
                yield return StartCoroutine(FetchTokenInfoFromJupiter(info.Mint, userToken));
            }
            
            Debug.Log($"✅ Loaded {_userTokens.Count} user tokens");
            
            // Debug: Print all loaded tokens
            foreach (var token in _userTokens)
            {
                Debug.Log($"🔍 Loaded token: {token.Symbol} ({token.Name}) - Balance: {token.Balance} - Mint: {token.MintAddress}");
            }
            
            // Update the dropdown with user tokens
            UpdateTokenADropdown();
        }
        else
        {
            Debug.LogWarning("⚠️ Failed to load user tokens");
        }
    }

    private void UpdateTokenADropdown()
    {
        if (tokenADropdown == null) return;

        tokenADropdown.ClearOptions();
        List<string> dropdownOptions = new List<string>();
        
        // Get common token balances
        Dictionary<string, float> commonTokenBalances = GetCommonTokenBalances();
        
        // Add common tokens with balances
        foreach (var token in _commonTokens)
        {
            float balance = commonTokenBalances.ContainsKey(token) ? commonTokenBalances[token] : 0f;
            string optionText = $"{token}: {balance:F4}";
            dropdownOptions.Add(optionText);
        }
        
        // Filter out user tokens that are already in common tokens
        var uniqueUserTokens = _userTokens.Where(userToken => 
            !_commonTokens.Any(commonToken => 
                string.Equals(userToken.Symbol, commonToken, StringComparison.OrdinalIgnoreCase)
            )
        ).ToList();
        
        // Add separator if we have unique user tokens
        if (uniqueUserTokens.Count > 0)
        {
            dropdownOptions.Add("──────────");
        }
        
        // Add unique user tokens with balances
        foreach (var userToken in uniqueUserTokens.OrderByDescending(t => t.Balance))
        {
            string optionText = $"{userToken.Symbol}: {userToken.Balance:F4}";
            dropdownOptions.Add(optionText);
        }
        
        tokenADropdown.AddOptions(dropdownOptions);
        tokenADropdown.onValueChanged.RemoveAllListeners();
        tokenADropdown.onValueChanged.AddListener(OnTokenAChanged);
        tokenADropdown.value = 0; // Ensure SOL is selected by default
    }

    private Dictionary<string, float> GetCommonTokenBalances()
    {
        var balances = new Dictionary<string, float>();
        
        // Get SOL balance
        if (WalletManager.instance != null)
        {
            balances["SOL"] = WalletManager.instance.Balance;
        }
        
        // Get SPL token balances for common tokens
        if (_userTokens != null)
        {
            Debug.Log($"🔍 Looking for common tokens in {_userTokens.Count} user tokens");
            foreach (var commonToken in _commonTokens)
            {
                if (commonToken == "SOL") continue; // SOL is handled above
                
                Debug.Log($"🔍 Looking for common token: {commonToken}");
                
                // Find matching user token by symbol
                var matchingUserToken = _userTokens.FirstOrDefault(t => 
                    string.Equals(t.Symbol, commonToken, StringComparison.OrdinalIgnoreCase)
                );
                
                // If not found by symbol, try to find by known mint addresses
                if (matchingUserToken == null)
                {
                    string knownMintAddress = GetKnownMintAddress(commonToken);
                    if (!string.IsNullOrEmpty(knownMintAddress))
                    {
                        matchingUserToken = _userTokens.FirstOrDefault(t => 
                            string.Equals(t.MintAddress, knownMintAddress, StringComparison.OrdinalIgnoreCase)
                        );
                        if (matchingUserToken != null)
                        {
                            Debug.Log($"✅ Found {commonToken} by mint address: {knownMintAddress}");
                        }
                    }
                }
                
                if (matchingUserToken != null)
                {
                    balances[commonToken] = matchingUserToken.Balance;
                    Debug.Log($"✅ Found {commonToken} with balance: {matchingUserToken.Balance}");
                }
                else
                {
                    balances[commonToken] = 0f;
                    Debug.Log($"❌ Not found: {commonToken}");
                }
            }
        }
        
        return balances;
    }

    private IEnumerator GetTokenWithRetry(string symbol, Action<TokenData> onSuccess)
    {
        for (int attempt = 1; attempt <= maxRetries; attempt++)
        {
            bool shouldRetry = false;
            Exception lastException = null;
            var tokenTask = _dex.GetTokenBySymbol(symbol);
            
            // Wait for task completion outside try block
            while (!tokenTask.IsCompleted)
            {
                yield return null;
            }
            
            try
            {
                var token = tokenTask.Result;
                onSuccess?.Invoke(token);
                Debug.Log($"✅ Successfully got token {symbol} on attempt {attempt}");
                yield break;
            }
            catch (Exception e)
            {
                lastException = e;
                Debug.LogWarning($"⚠️ Attempt {attempt}/{maxRetries} failed for {symbol}: {e.Message}");
                
                if (IsNetworkError(e) && attempt < maxRetries)
                {
                    shouldRetry = true;
                }
            }
            
            if (shouldRetry)
            {
                Debug.Log($"🔄 Retrying in {retryDelay} seconds...");
                yield return new WaitForSeconds(retryDelay);
            }
            else
            {
                Debug.LogError($"❌ Failed to get token {symbol} after {maxRetries} attempts: {lastException?.Message}");
                onSuccess?.Invoke(null);
                yield break;
            }
        }
    }

    private bool IsNetworkError(Exception e)
    {
        string errorMessage = e.Message.ToLower();
        return errorMessage.Contains("curl error 52") || 
               errorMessage.Contains("empty reply") ||
               errorMessage.Contains("connection") ||
               errorMessage.Contains("timeout") ||
               errorMessage.Contains("network") ||
               errorMessage.Contains("unable to connect");
    }

    private void InitializeUI()
    {
        // Initialize token B dropdown with common tokens
        if (tokenBDropdown != null)
        {
            tokenBDropdown.ClearOptions();
            tokenBDropdown.AddOptions(_commonTokens.ToList());
            tokenBDropdown.onValueChanged.AddListener(OnTokenBChanged);
            tokenBDropdown.value = 1; // USDC as default for B
        }
        
        // Token A dropdown will be initialized after loading user tokens
        
        // Make balance text clickable
        if (balanceText != null)
        {
            SetupBalanceTextClickable();
        }
    }
    
    private void SetupBalanceTextClickable()
    {
        // Check if the balanceText is inside a Button component
        Button balanceButton = balanceText.GetComponentInParent<Button>();
        if (balanceButton == null)
        {
            // If no parent button, add button component to the text itself
            balanceButton = balanceText.GetComponent<Button>();
            if (balanceButton == null)
            {
                balanceButton = balanceText.gameObject.AddComponent<Button>();
            }
        }
        
        // Clear existing onClick events and add our handler
        balanceButton.onClick.RemoveAllListeners();
        balanceButton.onClick.AddListener(OnBalanceTextClick);
        
        // Make sure the text is raycastable
        balanceText.raycastTarget = true;
        
        Debug.Log("✅ Balance text is now clickable");
    }

    private void OnTokenAChanged(int index)
    {
        if (_dex == null) return;
        StartCoroutine(OnTokenAChangedCoroutine(index));
    }

    private IEnumerator OnTokenAChangedCoroutine(int index)
    {
        // Get the current dropdown options to determine what was selected
        if (tokenADropdown == null || index >= tokenADropdown.options.Count) yield break;
        
        string selectedOption = tokenADropdown.options[index].text;
        
        // Check if it's a separator line
        if (selectedOption == "──────────")
        {
            yield break;
        }
        
        // Extract token symbol from the option text (format: "TOKEN: BALANCE")
        string tokenSymbol = selectedOption.Split(':')[0].Trim();
        
        // Check if it's a common token
        if (_commonTokens.Contains(tokenSymbol))
        {
            yield return StartCoroutine(GetTokenWithRetry(tokenSymbol, (token) => _tokenA = token));
            
            if (_tokenA != null)
            {
                Debug.Log($"✅ Token A changed to: {_tokenA.Symbol}");
                StartCoroutine(GetQuoteCoroutine());
                UpdateBalance();
            }
        }
        else
        {
            // It's a user token
            // Find the user token by symbol
            var userToken = _userTokens.FirstOrDefault(t => 
                string.Equals(t.Symbol, tokenSymbol, StringComparison.OrdinalIgnoreCase)
            );
            
            if (userToken != null)
            {
                // Try to get token data by mint address
                yield return StartCoroutine(GetTokenByMintAddress(userToken.MintAddress, (token) => _tokenA = token));
                
                if (_tokenA != null)
                {
                    Debug.Log($"✅ Token A changed to user token: {_tokenA.Symbol}");
                    StartCoroutine(GetQuoteCoroutine());
                    UpdateBalance();
                }
                else
                {
                    Debug.LogWarning($"⚠️ Failed to get token data for mint: {userToken.MintAddress}");
                }
            }
            else
            {
                Debug.LogWarning($"⚠️ User token not found for symbol: {tokenSymbol}");
            }
        }
        // Ignore separator lines
    }

    private IEnumerator GetTokenByMintAddress(string mintAddress, Action<TokenData> onSuccess)
    {
        for (int attempt = 1; attempt <= maxRetries; attempt++)
        {
            bool shouldRetry = false;
            Exception lastException = null;
            var tokensTask = _dex.GetTokens();
            
            // Wait for task completion outside try block
            while (!tokensTask.IsCompleted)
            {
                yield return null;
            }
            
            try
            {
                var tokens = tokensTask.Result;
                var token = tokens.FirstOrDefault(t => t.MintAddress == mintAddress);
                
                if (token != null)
                {
                    onSuccess?.Invoke(token);
                    Debug.Log($"✅ Successfully found token by mint {mintAddress} on attempt {attempt}");
                    yield break;
                }
                else
                {
                    // If token not found in Jupiter's list, create a basic TokenData from user token info
                    var userToken = _userTokens.FirstOrDefault(t => t.MintAddress == mintAddress);
                    if (userToken != null)
                    {
                        var basicToken = new TokenData
                        {
                            Symbol = userToken.Symbol,
                            Mint = userToken.MintAddress,
                            Decimals = userToken.Decimals,
                            LogoURI = userToken.LogoUrl
                        };
                        onSuccess?.Invoke(basicToken);
                        Debug.Log($"✅ Created basic token data for mint {mintAddress}");
                        yield break;
                    }
                    else
                    {
                        throw new Exception($"Token not found for mint: {mintAddress}");
                    }
                }
            }
            catch (Exception e)
            {
                lastException = e;
                Debug.LogWarning($"⚠️ Attempt {attempt}/{maxRetries} failed for mint {mintAddress}: {e.Message}");
                
                if (IsNetworkError(e) && attempt < maxRetries)
                {
                    shouldRetry = true;
                }
            }
            
            if (shouldRetry)
            {
                Debug.Log($"🔄 Retrying in {retryDelay} seconds...");
                yield return new WaitForSeconds(retryDelay);
            }
            else
            {
                Debug.LogError($"❌ Failed to get token by mint {mintAddress} after {maxRetries} attempts: {lastException?.Message}");
                onSuccess?.Invoke(null);
                yield break;
            }
        }
    }

    private void OnTokenBChanged(int index)
    {
        if (_dex == null) return;
        StartCoroutine(OnTokenBChangedCoroutine(index));
    }

    private IEnumerator OnTokenBChangedCoroutine(int index)
    {
        yield return StartCoroutine(GetTokenWithRetry(_commonTokens[index], (token) => _tokenB = token));
        
        if (_tokenB != null)
        {
            Debug.Log($"✅ Token B changed to: {_tokenB.Symbol}");
            StartCoroutine(GetQuoteCoroutine());
        }
    }

    public void OnSwapButtonClick()
    {
        if (_dex == null)
        {
            resultText.text = "❌ Jupiter SDK not initialized";
            return;
        }

        if (_currentQuote == null)
        {
            resultText.text = "❌ No quote available. Please enter an amount.";
            return;
        }

        StartCoroutine(ExecuteSwap());
    }

    public void OnAmountChanged()
    {
        StartCoroutine(GetQuoteCoroutine());
    }

    private IEnumerator GetQuoteCoroutine()
    {
        if (_dex == null || _tokenA == null || _tokenB == null) yield break;

        if (string.IsNullOrEmpty(amountInput.text))
        {
            ClearQuote();
            yield break;
        }

        if (!float.TryParse(amountInput.text, out float amount) || amount <= 0f)
        {
            ClearQuote();
            yield break;
        }

        ToggleLoading(true);
        resultText.text = "Getting quote...";

        // Convert amount to proper decimals using simple math
        ulong inputAmount = ConvertToUlong(amount, _tokenA.Decimals);
        
        // Get swap quote with retry logic
        yield return StartCoroutine(GetQuoteWithRetry(inputAmount));
        
        ToggleLoading(false);
    }

    private IEnumerator GetQuoteWithRetry(ulong inputAmount)
    {
        for (int attempt = 1; attempt <= maxRetries; attempt++)
        {
            bool shouldRetry = false;
            Exception lastException = null;
            var quoteTask = _dex.GetSwapQuote(
                _tokenA.MintAddress,
                _tokenB.MintAddress,
                inputAmount
            );
            
            // Wait for task completion outside try block
            while (!quoteTask.IsCompleted)
            {
                yield return null;
            }
            
            try
            {
                _currentQuote = quoteTask.Result;

                // Display quote information
                var outputAmount = ConvertFromBigInteger((ulong)_currentQuote.OutputAmount, _tokenB.Decimals);
                usdcEstimateText.text = $"{outputAmount:F6} {_tokenB.Symbol}";

                resultText.text = $"Quote ready! Click Swap to execute.";
                Debug.Log($"📊 Quote: {ConvertFromBigInteger(inputAmount, _tokenA.Decimals)} {_tokenA.Symbol}->{outputAmount:F6} {_tokenB.Symbol}");
                yield break;
            }
            catch (Exception e)
            {
                lastException = e;
                Debug.LogWarning($"⚠️ Quote attempt {attempt}/{maxRetries} failed: {e.Message}");
                
                if (IsNetworkError(e) && attempt < maxRetries)
                {
                    shouldRetry = true;
                }
            }
            
            if (shouldRetry)
            {
                resultText.text = $"Network issue, retrying... ({attempt}/{maxRetries})";
                yield return new WaitForSeconds(retryDelay);
            }
            else
            {
                Debug.LogError($"❌ Failed to get quote after {maxRetries} attempts: {lastException?.Message}");
                resultText.text = $"❌ Quote failed: {lastException?.Message}";
                ClearQuote();
                yield break;
            }
        }
    }

    private IEnumerator ExecuteSwap()
    {
        if (_currentQuote == null)
        {
            resultText.text = "❌ No quote available";
            yield break;
        }

        ToggleLoading(true);
        resultText.text = "Creating swap transaction...";

        // Create swap transaction with retry logic
        yield return StartCoroutine(ExecuteSwapWithRetry());
        
        ToggleLoading(false);
    }

    private IEnumerator ExecuteSwapWithRetry()
    {
        for (int attempt = 1; attempt <= maxRetries; attempt++)
        {
            bool shouldRetry = false;
            bool success = false;
            Exception lastException = null;
            
            // Create swap transaction outside try block
            var swapTask = _dex.Swap(_currentQuote);
            while (!swapTask.IsCompleted)
            {
                yield return null;
            }
            
            // Create sign task outside try block
            var signTask = (Task<RequestResult<string>>)null;
            var tx = (Transaction)null;
            
            try
            {
                if (swapTask.Exception != null)
                {
                    throw swapTask.Exception;
                }

                tx = swapTask.Result;
                resultText.text = "Signing and sending transaction...";
            }
            catch (Exception e)
            {
                lastException = e;
                Debug.LogWarning($"⚠️ Swap creation attempt {attempt}/{maxRetries} failed: {e.Message}");
                
                if (IsNetworkError(e) && attempt < maxRetries)
                {
                    shouldRetry = true;
                }
            }
            
            if (shouldRetry)
            {
                resultText.text = $"Network issue, retrying swap... ({attempt}/{maxRetries})";
                yield return new WaitForSeconds(retryDelay);
                continue;
            }
            else if (lastException != null)
            {
                Debug.LogError($"❌ Swap creation failed after {maxRetries} attempts: {lastException?.Message}");
                resultText.text = $"❌ Swap failed: {lastException?.Message}";
                yield break;
            }
            
            // Sign and send transaction outside try block
            signTask = Web3.Wallet.SignAndSendTransaction(tx);
            while (!signTask.IsCompleted)
            {
                yield return null;
            }
            
            try
            {
                if (signTask.Exception != null)
                {
                    throw signTask.Exception;
                }

                var result = signTask.Result;

                if (result?.Result != null)
                {
                    Debug.Log($"✅ Swap successful! TxID: {result.Result}");
                    resultText.text = $"Swap successful!\nTx: <size=60%>{result.Result}</size>";
                    
                    // Clear the quote and update balance
                    ClearQuote();
                    UpdateBalance();
                    success = true;
                }
                else
                {
                    throw new Exception(result?.Reason ?? "Unknown error");
                }
            }
            catch (Exception e)
            {
                lastException = e;
                Debug.LogWarning($"⚠️ Transaction signing attempt {attempt}/{maxRetries} failed: {e.Message}");
                
                if (IsNetworkError(e) && attempt < maxRetries)
                {
                    shouldRetry = true;
                }
            }
            
            if (success)
            {
                yield break;
            }
            else if (shouldRetry)
            {
                resultText.text = $"Network issue, retrying swap... ({attempt}/{maxRetries})";
                yield return new WaitForSeconds(retryDelay);
            }
            else
            {
                Debug.LogError($"❌ Swap failed after {maxRetries} attempts: {lastException?.Message}");
                resultText.text = $"❌ Swap failed: {lastException?.Message}";
                yield break;
            }
        }
    }

    private void ClearQuote()
    {
        _currentQuote = null;
        usdcEstimateText.text = "";
    }

    private void ToggleLoading(bool show)
    {
        if (loadingScreen != null)
            loadingScreen.SetActive(show);
    }

    private void UpdateBalance()
    {
        if (_tokenA == null) return;

        if (_tokenA.Symbol == "SOL")
        {
            if (WalletManager.instance != null)
            {
                _currentBalance = WalletManager.instance.Balance;
                balanceText.text = $"Your Balance: {_currentBalance:F4} {_tokenA.Symbol}";
            }
        }
        else
        {
            StartCoroutine(GetSplTokenBalanceCoroutine(_tokenA.MintAddress, _tokenA.Decimals, (balance) => {
                _currentBalance = balance;
                balanceText.text = $"Your Balance: {_currentBalance:F4} {_tokenA.Symbol}";
            }));
        }
    }
    
    // Public method to handle balance text clicks
    public void OnBalanceTextClick()
    {
        if (_currentBalance > 0f && amountInput != null)
        {
            // Set the input field to the maximum balance
            amountInput.text = _currentBalance.ToString("F6");
            Debug.Log($"✅ Set amount to maximum balance: {_currentBalance:F6} {_tokenA?.Symbol}");
            
            // Trigger quote update
            OnAmountChanged();
        }
        else
        {
            Debug.LogWarning("⚠️ No balance available or amount input not assigned");
        }
    }

    // Public method to refresh balance (can be called from other scripts)
    public void RefreshBalance()
    {
        UpdateBalance();
        // Also refresh dropdown to show updated balances
        UpdateTokenADropdown();
    }

    // Public method to refresh user tokens (can be called from other scripts)
    public void RefreshUserTokens()
    {
        StartCoroutine(LoadUserTokens());
    }

    // Public method to refresh the dropdown (can be called from other scripts)
    public void RefreshDropdown()
    {
        UpdateTokenADropdown();
    }

    // Public method to get current user tokens (for debugging)
    public List<UserTokenInfo> GetUserTokens()
    {
        return _userTokens;
    }

    // Public method to set Token B from search functionality
    public void SetTokenBFromSearch(string mintAddress, string symbol, string name, int decimals, string logoUrl = "")
    {
        if (_dex == null)
        {
            Debug.LogError("❌ Jupiter DEX not initialized");
            return;
        }

        SetTokenBFromSearchInternal(mintAddress, symbol, name, decimals, logoUrl);
    }

    private void SetTokenBFromSearchInternal(string mintAddress, string symbol, string name, int decimals, string logoUrl)
    {
        // Create a basic TokenData from the search result
        var tokenData = new TokenData
        {
            Symbol = symbol,
            Name = name,
            Mint = mintAddress,
            Decimals = decimals,
            LogoURI = logoUrl
        };

        _tokenB = tokenData;
        
        Debug.Log($"✅ Token B set from search: {symbol} ({mintAddress})");
        
        // Update the dropdown to reflect the change
        if (tokenBDropdown != null)
        {
            // Add the searched token to the dropdown if it's not already there
            bool found = false;
            for (int i = 0; i < tokenBDropdown.options.Count; i++)
            {
                if (tokenBDropdown.options[i].text.Contains(symbol))
                {
                    tokenBDropdown.value = i;
                    found = true;
                    break;
                }
            }
            
            if (!found)
            {
                // Add the new token to the dropdown
                tokenBDropdown.AddOptions(new List<string> { symbol });
                tokenBDropdown.value = tokenBDropdown.options.Count - 1;
            }
        }
        
        // Trigger a quote update if there's an amount
        if (!string.IsNullOrEmpty(amountInput.text))
        {
            StartCoroutine(GetQuoteCoroutine());
        }
    }

    // Simple conversion methods to replace DecimalUtil
    private ulong ConvertToUlong(float amount, int decimals)
    {
        return (ulong)(amount * Math.Pow(10, decimals));
    }

    private float ConvertFromBigInteger(ulong amount, int decimals)
    {
        return (float)(amount / Math.Pow(10, decimals));
    }

    private IEnumerator FetchTokenInfoFromJupiter(string mintAddress, UserTokenInfo userToken)
    {
        // Try to get token info from Jupiter DEX aggregator first
        if (_dex != null)
        {
            bool dexSuccess = false;
            Exception dexException = null;
            var tokensTask = _dex.GetTokens();
            
            // Wait for task completion outside try block
            while (!tokensTask.IsCompleted)
            {
                yield return null;
            }
            
            try
            {
                if (tokensTask.IsCompletedSuccessfully)
                {
                    var tokens = tokensTask.Result;
                    var tokenInfo = tokens.FirstOrDefault(t => t.MintAddress == mintAddress);
                    
                    if (tokenInfo != null)
                    {
                        userToken.Symbol = tokenInfo.Symbol;
                        userToken.Name = tokenInfo.Name ?? tokenInfo.Symbol;
                        userToken.LogoUrl = tokenInfo.LogoURI;
                        _userTokens.Add(userToken);
                        
                        Debug.Log($"✅ Added token from Jupiter DEX: {userToken.Symbol} ({userToken.Name})");
                        dexSuccess = true;
                    }
                    else
                    {
                        Debug.LogWarning($"⚠️ Token not found in Jupiter DEX for {mintAddress}");
                    }
                }
                else
                {
                    Debug.LogWarning($"⚠️ Failed to get tokens from Jupiter DEX");
                }
            }
            catch (Exception e)
            {
                dexException = e;
                Debug.LogWarning($"⚠️ Error getting tokens from Jupiter DEX: {e.Message}");
            }
            
            if (dexSuccess)
            {
                yield break;
            }
        }

        // If Jupiter DEX fails, try the Jupiter API
        Debug.Log($"⚠️ Jupiter DEX failed for {mintAddress}, trying Jupiter API...");
        yield return StartCoroutine(FetchTokenInfoFromJupiterAPI(mintAddress, userToken));
    }

    private IEnumerator FetchTokenInfoFromJupiterAPI(string mintAddress, UserTokenInfo userToken)
    {
        // Use Jupiter Token API V2 search endpoint to query by mint address
        string jupiterApiUrl = $"https://lite-api.jup.ag/tokens/v2/search?query={mintAddress}";
        
        using (UnityWebRequest request = UnityWebRequest.Get(jupiterApiUrl))
        {
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                string responseText = request.downloadHandler.text;
                Debug.Log($"🔍 Jupiter API search response for {mintAddress}: {responseText}");
                
                bool apiSuccess = false;
                try
                {
                    // Parse the JSON response - it's an array of tokens directly
                    // Need to wrap in an object for JsonUtility
                    string wrappedJson = $"{{\"tokens\":{responseText}}}";
                    var tokens = JsonUtility.FromJson<JupiterTokenArray>(wrappedJson);
                    
                    if (tokens != null && tokens.tokens != null && tokens.tokens.Length > 0)
                    {
                        // Find the exact match by mint address (id field)
                        var tokenInfo = tokens.tokens.FirstOrDefault(t => t.id == mintAddress);
                        
                        if (tokenInfo != null)
                        {
                            userToken.Symbol = tokenInfo.symbol;
                            userToken.Name = tokenInfo.name;
                            userToken.LogoUrl = tokenInfo.icon;
                            _userTokens.Add(userToken);
                            
                            Debug.Log($"✅ Added token from Jupiter API: {userToken.Symbol} ({userToken.Name})");
                            apiSuccess = true;
                        }
                        else
                        {
                            Debug.LogWarning($"⚠️ No exact match found in Jupiter API for {mintAddress}");
                        }
                    }
                    else
                    {
                        Debug.LogWarning($"⚠️ No tokens found in Jupiter API response for {mintAddress}");
                    }
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"⚠️ Failed to parse Jupiter API response for {mintAddress}: {e.Message}");
                }
                
                if (apiSuccess)
                {
                    yield break;
                }
            }
            else
            {
                Debug.LogWarning($"⚠️ Failed to fetch from Jupiter API for {mintAddress}: {request.error}");
            }
        }

        // If Jupiter API fails, fall back to blockchain metadata
        Debug.Log($"⚠️ Jupiter API failed for {mintAddress}, trying blockchain metadata...");
        yield return StartCoroutine(FetchTokenMetadata(mintAddress, userToken));
    }

    // Classes to parse Jupiter API response
    [System.Serializable]
    public class JupiterTokenArray
    {
        public JupiterToken[] tokens;
    }

    [System.Serializable]
    public class JupiterToken
    {
        public string id;           // mint address
        public string name;
        public string symbol;
        public string icon;         // logo URL
        public int decimals;
        public string tokenProgram;
        public bool isVerified;
        public string[] tags;
        public float organicScore;
        public string organicScoreLabel;
        public float usdPrice;
        public float mcap;
        public float fdv;
        public float liquidity;
        public int holderCount;
        public string updatedAt;
    }

    private IEnumerator FetchTokenMetadata(string mintAddress, UserTokenInfo userToken)
    {
        // Try to get token metadata using the getAsset RPC method
        string rpcUrl = "https://api.helius.xyz/v0/token-metadata";
        string jsonRpcRequest = $@"{{
            ""jsonrpc"": ""2.0"",
            ""id"": 1,
            ""method"": ""getAsset"",
            ""params"": [""{mintAddress}""]
        }}";

        using (UnityWebRequest request = UnityWebRequest.PostWwwForm(rpcUrl, ""))
        {
            request.method = "POST";
            request.SetRequestHeader("Content-Type", "application/json");
            request.uploadHandler = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(jsonRpcRequest));
            request.downloadHandler = new DownloadHandlerBuffer();

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                string responseText = request.downloadHandler.text;
                Debug.Log($"🔍 Raw metadata response for {mintAddress}: {responseText}");
                
                bool success = false;
                string jsonUri = null;
                try
                {
                    // Simple parsing to extract symbol and name
                    if (responseText.Contains("\"error\""))
                    {
                        Debug.LogWarning($"⚠️ RPC Error for {mintAddress}");
                    }
                    else if (responseText.Contains("\"result\""))
                    {
                        // Try to extract basic token info
                        string symbol = ExtractJsonValue(responseText, "symbol");
                        string name = ExtractJsonValue(responseText, "name");
                        
                        if (!string.IsNullOrEmpty(symbol))
                        {
                            userToken.Symbol = symbol;
                            userToken.Name = !string.IsNullOrEmpty(name) ? name : "Unknown Token";
                            userToken.LogoUrl = "";
                            _userTokens.Add(userToken);
                            Debug.Log($"✅ Added token from metadata: {userToken.Symbol} ({userToken.Name})");
                            success = true;
                        }
                        else
                        {
                            // Try to get metadata URI and fetch from there
                            jsonUri = ExtractJsonValue(responseText, "json_uri");
                            if (!string.IsNullOrEmpty(jsonUri))
                            {
                                success = true; // Mark as success so we can yield outside try-catch
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"⚠️ Failed to parse metadata for {mintAddress}: {e.Message}");
                }
                
                if (success)
                {
                    if (!string.IsNullOrEmpty(jsonUri))
                    {
                        yield return StartCoroutine(FetchMetadataFromUri(jsonUri, userToken));
                    }
                    else
                    {
                        yield break;
                    }
                }
            }
            else
            {
                Debug.LogWarning($"⚠️ Failed to fetch metadata for {mintAddress}: {request.error}");
            }
        }

        // If all else fails, use fallback
        SetFallbackTokenInfo(userToken, mintAddress);
    }

    private IEnumerator FetchMetadataFromUri(string metadataUri, UserTokenInfo userToken)
    {
        using (UnityWebRequest request = UnityWebRequest.Get(metadataUri))
        {
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                string responseText = request.downloadHandler.text;
                Debug.Log($"🔍 URI metadata response: {responseText}");
                
                bool success = false;
                try
                {
                    // Extract token information from the metadata
                    string symbol = ExtractJsonValue(responseText, "symbol");
                    string name = ExtractJsonValue(responseText, "name");
                    string image = ExtractJsonValue(responseText, "image");
                    
                    userToken.Symbol = !string.IsNullOrEmpty(symbol) ? symbol : $"TOKEN_{userToken.MintAddress.Substring(0, 4)}";
                    userToken.Name = !string.IsNullOrEmpty(name) ? name : "Unknown Token";
                    userToken.LogoUrl = !string.IsNullOrEmpty(image) ? image : "";
                    
                    _userTokens.Add(userToken);
                    Debug.Log($"✅ Added token from URI metadata: {userToken.Symbol} ({userToken.Name})");
                    success = true;
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"⚠️ Failed to parse URI metadata: {e.Message}");
                }
                
                if (success)
                {
                    yield break;
                }
            }
            else
            {
                Debug.LogWarning($"⚠️ Failed to fetch URI metadata: {request.error}");
            }
            
            // If we get here, something failed, so use fallback
            SetFallbackTokenInfo(userToken, userToken.MintAddress);
        }
    }

    private string ExtractJsonValue(string jsonText, string key)
    {
        try
        {
            string pattern = $"\"{key}\"\\s*:\\s*\"([^\"]*)\"";
            var match = System.Text.RegularExpressions.Regex.Match(jsonText, pattern);
            if (match.Success)
            {
                return match.Groups[1].Value;
            }
        }
        catch (Exception e)
        {
            Debug.LogWarning($"⚠️ Failed to extract JSON value for key '{key}': {e.Message}");
        }
        return null;
    }

    private string GetKnownMintAddress(string tokenSymbol)
    {
        // Known mint addresses for common tokens
        switch (tokenSymbol.ToUpper())
        {
            case "USDC":
                return "EPjFWdd5AufqSSqeM2qN1xzybapC8G4wEGGkZwyTDt1v";
            case "USDT":
                return "Es9vMFrzaCERmJfrF4H2FYD4KCoNkY11McCe8BenwNYB";
            case "BONK":
                return "DezXAZ8z7PnrnRJjz3wXBoRgixCa6xjnB7YaB1pPB263";
            case "JUP":
                return "JUPyiwrYJFskUPiHa7hkeR8VUtAeFoSYbKedZNsDvCN";
            case "RAY":
                return "4k3Dyjzvzp8eMZWUXbBCjEvwSkkk59S5iCNLY3QrkX6R";
            case "SRM":
                return "SRMuApVNdxXokk5GT7XD5cUUgXMBCoAz2LHeuAoKWRt";
            default:
                return null;
        }
    }

    private void SetFallbackTokenInfo(UserTokenInfo userToken, string mintAddress)
    {
        // Try to identify by known mint address first
        string knownSymbol = GetKnownSymbolByMint(mintAddress);
        if (!string.IsNullOrEmpty(knownSymbol))
        {
            userToken.Symbol = knownSymbol;
            userToken.Name = knownSymbol;
            userToken.LogoUrl = "";
            _userTokens.Add(userToken);
            Debug.Log($"✅ Added known token by mint address: {knownSymbol}");
        }
        else
        {
            userToken.Symbol = $"TOKEN_{mintAddress.Substring(0, 4)}";
            userToken.Name = "Unknown Token";
            userToken.LogoUrl = "";
            _userTokens.Add(userToken);
            Debug.Log($"⚠️ Added token with fallback symbol: {userToken.Symbol}");
        }
    }

    private string GetKnownSymbolByMint(string mintAddress)
    {
        // Reverse lookup: get symbol from mint address
        switch (mintAddress)
        {
            case "EPjFWdd5AufqSSqeM2qN1xzybapC8G4wEGGkZwyTDt1v":
                return "USDC";
            case "Es9vMFrzaCERmJfrF4H2FYD4KCoNkY11McCe8BenwNYB":
                return "USDT";
            case "DezXAZ8z7PnrnRJjz3wXBoRgixCa6xjnB7YaB1pPB263":
                return "BONK";
            case "JUPyiwrYJFskUPiHa7hkeR8VUtAeFoSYbKedZNsDvCN":
                return "JUP";
            case "4k3Dyjzvzp8eMZWUXbBCjEvwSkkk59S5iCNLY3QrkX6R":
                return "RAY";
            case "SRMuApVNdxXokk5GT7XD5cUUgXMBCoAz2LHeuAoKWRt":
                return "SRM";
            default:
                return null;
        }
    }

    private IEnumerator GetSplTokenBalanceCoroutine(string mintAddress, int decimals, Action<float> onResult)
    {
        for (int attempt = 1; attempt <= maxRetries; attempt++)
        {
            bool shouldRetry = false;
            bool success = false;
            Exception lastException = null;
            var getTokensTask = Web3.Wallet.GetTokenAccounts(Solana.Unity.Rpc.Types.Commitment.Processed);
            
            // Wait for task completion outside try block
            while (!getTokensTask.IsCompleted)
                yield return null;
            
            try
            {
                float balance = 0f;
                if (getTokensTask.Result != null)
                {
                    ulong total = 0;
                    foreach (var tokenAccount in getTokensTask.Result)
                    {
                        var info = tokenAccount.Account.Data.Parsed.Info;
                        if (info.Mint == mintAddress)
                        {
                            ulong amount = ulong.Parse(info.TokenAmount.Amount);
                            total += amount;
                        }
                    }
                    balance = (float)(total / Math.Pow(10, decimals));
                }
                onResult?.Invoke(balance);
                success = true;
            }
            catch (Exception e)
            {
                lastException = e;
                Debug.LogWarning($"⚠️ Balance fetch attempt {attempt}/{maxRetries} failed: {e.Message}");
                
                if (IsNetworkError(e) && attempt < maxRetries)
                {
                    shouldRetry = true;
                }
            }
            
            if (success)
            {
                yield break;
            }
            else if (shouldRetry)
            {
                yield return new WaitForSeconds(retryDelay);
            }
            else
            {
                Debug.LogError($"❌ Failed to get balance after {maxRetries} attempts: {lastException?.Message}");
                onResult?.Invoke(0f);
                yield break;
            }
        }
    }
} 