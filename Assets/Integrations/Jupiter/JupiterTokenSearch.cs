using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.Networking;

public class JupiterTokenSearch : MonoBehaviour
{
    // Static reference for easy access from prefabs
    public static JupiterTokenSearch Instance { get; private set; }
    
    [Header("Search UI References")]
    public Button searchButton;
    public GameObject searchPanel;
    public TMP_InputField contractAddressInput;
    public Button searchConfirmButton;
    public Button searchCancelButton;
    
    [Header("Token Info Display")]
    public TMP_Text tokenNameText;
    public TMP_Text tokenMarketCapText;
    public TMP_Text tokenPriceText;
    public Image tokenIconImage;
    public GameObject tokenInfoPanel;
    
    [Header("Jupiter SDK Reference")]
    public JupiterSDKManager jupiterManager;
    
    [Header("Settings")]
    public bool autoSetAsTokenB = true;
    public bool showTokenInfo = true;
    
    private TokenSearchResult _currentSearchResult;
    
    [System.Serializable]
    public class TokenSearchResult
    {
        public string mintAddress;
        public string symbol;
        public string name;
        public string icon;
        public int decimals;
        public float usdPrice;
        public float mcap;
        public bool isVerified;
    }
    
    private void Start()
    {
        // Set the static instance
        Instance = this;
        
        InitializeSearchUI();
    }
    
    private void InitializeSearchUI()
    {
        // Initialize search button
        if (searchButton != null)
        {
            searchButton.onClick.AddListener(OnSearchButtonClick);
        }
        
        // Initialize search panel buttons
        if (searchConfirmButton != null)
        {
            searchConfirmButton.onClick.AddListener(OnSearchConfirmClick);
        }
        
        if (searchCancelButton != null)
        {
            searchCancelButton.onClick.AddListener(OnSearchCancelClick);
        }
        
        // Initialize input field
        if (contractAddressInput != null)
        {
            contractAddressInput.onSubmit.AddListener(OnSearchSubmit);
            contractAddressInput.onValueChanged.AddListener(OnInputValueChanged);
        }
        
        // Hide panels initially
        if (searchPanel != null) searchPanel.SetActive(false);
        if (tokenInfoPanel != null) tokenInfoPanel.SetActive(false);
    }
    
    public void OnSearchButtonClick()
    {
        if (searchPanel != null)
        {
            searchPanel.SetActive(true);
            if (contractAddressInput != null)
            {
                contractAddressInput.text = "";
                contractAddressInput.Select();
            }
        }
    }
    
    public void OnSearchCancelClick()
    {
        if (searchPanel != null) searchPanel.SetActive(false);
        if (tokenInfoPanel != null) tokenInfoPanel.SetActive(false);
        ClearSearchResult();
    }
    
    public void OnSearchSubmit(string contractAddress)
    {
        if (!string.IsNullOrEmpty(contractAddress))
        {
            StartCoroutine(SearchTokenByContractAddress(contractAddress));
        }
    }
    
    public void OnInputValueChanged(string contractAddress)
    {
        // Only search if the input is exactly 44 characters (Solana address length)
        // and contains only valid base58 characters
        if (!string.IsNullOrEmpty(contractAddress) && 
            contractAddress.Length == 44 && 
            IsValidSolanaAddress(contractAddress))
        {
            // Add a small delay to avoid searching on every keystroke
            StartCoroutine(DelayedSearch(contractAddress));
        }
        else
        {
            // Clear the search result if input is invalid
            ClearSearchResult();
        }
    }
    
    // Public method to trigger search from external scripts
    public void SearchTokenByAddress(string contractAddress)
    {
        if (!string.IsNullOrEmpty(contractAddress))
        {
            StartCoroutine(SearchTokenByContractAddress(contractAddress));
        }
    }
    
    private IEnumerator DelayedSearch(string contractAddress)
    {
        // Wait a short moment to avoid searching while user is still typing
        yield return new WaitForSeconds(0.5f);
        
        // Check if the input hasn't changed during the delay
        if (contractAddressInput != null && contractAddressInput.text == contractAddress)
        {
            StartCoroutine(SearchTokenByContractAddress(contractAddress));
        }
    }
    
    private bool IsValidSolanaAddress(string address)
    {
        if (string.IsNullOrEmpty(address))
            return false;
            
        // Check if it contains only valid base58 characters
        string validChars = "123456789ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz";
        foreach (char c in address)
        {
            if (!validChars.Contains(c.ToString()))
                return false;
        }
        
        return true;
    }
    
    public void OnSearchConfirmClick()
    {
        if (_currentSearchResult != null && jupiterManager != null)
        {
            // Set the searched token as Token B
            StartCoroutine(SetSearchedTokenAsTokenB(_currentSearchResult));
            
            // Hide panels
            if (searchPanel != null) searchPanel.SetActive(false);
            if (tokenInfoPanel != null) tokenInfoPanel.SetActive(false);
            
            ClearSearchResult();
        }
    }
    
    private IEnumerator SearchTokenByContractAddress(string contractAddress)
    {
        Debug.Log($"🔍 Searching for token with contract address: {contractAddress}");
        
        // Show loading state
        if (searchConfirmButton != null) searchConfirmButton.interactable = false;
        
        // Try Jupiter API first
        yield return StartCoroutine(SearchTokenInJupiterAPI(contractAddress));
        
        // If Jupiter API fails, try alternative sources
        if (_currentSearchResult == null)
        {
            yield return StartCoroutine(SearchTokenInAlternativeAPI(contractAddress));
        }
        

        
        // Display results
        if (_currentSearchResult != null)
        {
            DisplayTokenInfo(_currentSearchResult);
            if (searchConfirmButton != null) searchConfirmButton.interactable = true;
        }
        else
        {
            Debug.LogWarning($"⚠️ Token not found for contract address: {contractAddress}");
            // Show error message
            if (tokenNameText != null) tokenNameText.text = "Token not found";
            if (tokenInfoPanel != null) tokenInfoPanel.SetActive(true);
        }
    }
    
    private IEnumerator SearchTokenInJupiterAPI(string contractAddress)
    {
        string jupiterApiUrl = $"https://lite-api.jup.ag/tokens/v2/search?query={contractAddress}";
        
        using (UnityWebRequest request = UnityWebRequest.Get(jupiterApiUrl))
        {
            yield return request.SendWebRequest();
            
            if (request.result == UnityWebRequest.Result.Success)
            {
                string responseText = request.downloadHandler.text;
                Debug.Log($"🔍 Jupiter API search response: {responseText}");
                
                try
                {
                    // Parse the JSON response
                    string wrappedJson = $"{{\"tokens\":{responseText}}}";
                    var tokens = JsonUtility.FromJson<JupiterTokenArray>(wrappedJson);
                    
                    if (tokens != null && tokens.tokens != null && tokens.tokens.Length > 0)
                    {
                        // Find exact match by mint address
                        var tokenInfo = Array.Find(tokens.tokens, t => t.id == contractAddress);
                        
                        if (tokenInfo != null)
                        {
                            _currentSearchResult = new TokenSearchResult
                            {
                                mintAddress = tokenInfo.id,
                                symbol = tokenInfo.symbol,
                                name = tokenInfo.name,
                                icon = tokenInfo.icon,
                                decimals = tokenInfo.decimals,
                                usdPrice = tokenInfo.usdPrice,
                                mcap = tokenInfo.mcap,
                                isVerified = tokenInfo.isVerified
                            };
                            
                            Debug.Log($"✅ Found token in Jupiter API: {_currentSearchResult.symbol} ({_currentSearchResult.name})");
                        }
                    }
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"⚠️ Failed to parse Jupiter API response: {e.Message}");
                }
            }
            else
            {
                Debug.LogWarning($"⚠️ Failed to fetch from Jupiter API: {request.error}");
            }
        }
    }
    
    private IEnumerator SearchTokenInAlternativeAPI(string contractAddress)
    {
        // Try Helius API for additional token information
        string heliusApiUrl = "https://api.helius.xyz/v0/token-metadata";
        string jsonRpcRequest = $@"{{
            ""jsonrpc"": ""2.0"",
            ""id"": 1,
            ""method"": ""getAsset"",
            ""params"": [""{contractAddress}""]
        }}";
        
        using (UnityWebRequest request = UnityWebRequest.PostWwwForm(heliusApiUrl, ""))
        {
            request.method = "POST";
            request.SetRequestHeader("Content-Type", "application/json");
            request.uploadHandler = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(jsonRpcRequest));
            request.downloadHandler = new DownloadHandlerBuffer();
            
            yield return request.SendWebRequest();
            
            if (request.result == UnityWebRequest.Result.Success)
            {
                string responseText = request.downloadHandler.text;
                Debug.Log($"🔍 Helius API response: {responseText}");
                
                try
                {
                    // Extract basic token info
                    string symbol = ExtractJsonValue(responseText, "symbol");
                    string name = ExtractJsonValue(responseText, "name");
                    
                    if (!string.IsNullOrEmpty(symbol))
                    {
                        _currentSearchResult = new TokenSearchResult
                        {
                            mintAddress = contractAddress,
                            symbol = symbol,
                            name = !string.IsNullOrEmpty(name) ? name : "Unknown Token",
                            icon = "",
                            decimals = 9, // Default decimals
                            usdPrice = 0f,
                            mcap = 0f,
                            isVerified = false
                        };
                        
                        Debug.Log($"✅ Found token in Helius API: {_currentSearchResult.symbol} ({_currentSearchResult.name})");
                    }
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"⚠️ Failed to parse Helius API response: {e.Message}");
                }
            }
            else
            {
                Debug.LogWarning($"⚠️ Failed to fetch from Helius API: {request.error}");
            }
        }
    }
    
    private void DisplayTokenInfo(TokenSearchResult token)
    {
        if (tokenInfoPanel != null) tokenInfoPanel.SetActive(true);
        
        if (tokenNameText != null)
        {
            tokenNameText.text = $"<b>{token.name}</b> - <color=#888888><size=80%>{token.symbol}</size></color>";
        }
        
        if (tokenMarketCapText != null)
        {
            if (token.mcap > 0)
            {
                tokenMarketCapText.text = $"Market Cap\n<color=#888888><size=80%>${token.mcap:N0}</size></color>";
            }
            else
            {
                tokenMarketCapText.text = $"Market Cap\n<color=#888888><size=80%>N/A</size></color>";
            }
        }
        

        
        if (tokenPriceText != null)
        {
            if (token.usdPrice > 0)
            {
                tokenPriceText.text = $"Price\n<color=#888888><size=80%>${token.usdPrice:F6}</size></color>";
            }
            else
            {
                tokenPriceText.text = $"Price\n<color=#888888><size=80%>N/A</size></color>";
            }
        }
        
        // Load token icon if available
        if (!string.IsNullOrEmpty(token.icon) && tokenIconImage != null)
        {
            StartCoroutine(LoadTokenIcon(token.icon));
        }
        else if (tokenIconImage != null)
        {
            // Set default icon or clear
            tokenIconImage.sprite = null;
        }
        
        Debug.Log($"📊 Displaying token info: {token.symbol} - {token.name}");
    }
    

    
    private IEnumerator LoadTokenIcon(string iconUrl)
    {
        if (string.IsNullOrEmpty(iconUrl) || tokenIconImage == null) yield break;
        
        using (UnityWebRequest request = UnityWebRequestTexture.GetTexture(iconUrl))
        {
            request.timeout = 5; // 5 second timeout
            
            yield return request.SendWebRequest();
            
            if (request.result == UnityWebRequest.Result.Success)
            {
                Texture2D tex = DownloadHandlerTexture.GetContent(request);
                if (tex != null && tex.width > 0 && tex.height > 0)
                {
                    tokenIconImage.sprite = Sprite.Create(
                        tex,
                        new Rect(0, 0, tex.width, tex.height),
                        new Vector2(0.5f, 0.5f),
                        100
                    );
                    Debug.Log($"✅ Loaded token icon from: {iconUrl}");
                }
                else
                {
                    Debug.LogWarning($"⚠️ Invalid texture from: {iconUrl}");
                    tokenIconImage.sprite = null;
                }
            }
            else
            {
                Debug.LogWarning($"⚠️ Failed to load token icon: {request.error} from {iconUrl}");
                tokenIconImage.sprite = null;
            }
        }
    }
    
    private IEnumerator SetSearchedTokenAsTokenB(TokenSearchResult token)
    {
        if (jupiterManager == null)
        {
            Debug.LogError("❌ JupiterSDKManager reference is null");
            yield break;
        }
        
        Debug.Log($"🔄 Setting searched token as Token B: {token.symbol}");
        
        // Use the new public method to set Token B
        jupiterManager.SetTokenBFromSearch(token.mintAddress, token.symbol, token.name, token.decimals, token.icon);
        
        Debug.Log($"✅ Token B set to: {token.symbol} ({token.mintAddress})");
    }
    
    private void ClearSearchResult()
    {
        _currentSearchResult = null;
        
        if (tokenNameText != null) tokenNameText.text = "";
        if (tokenMarketCapText != null) tokenMarketCapText.text = "";
        if (tokenPriceText != null) tokenPriceText.text = "";
        if (tokenIconImage != null) tokenIconImage.sprite = null;
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
    
    // Classes to parse Jupiter API response (same as in JupiterSDKManager)
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
    

} 