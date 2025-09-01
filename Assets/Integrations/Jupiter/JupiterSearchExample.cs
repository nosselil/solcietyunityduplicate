using UnityEngine;
using TMPro;
using UnityEngine.UI;

/// <summary>
/// Example implementation of Jupiter Token Search functionality
/// This script demonstrates how to set up the search UI and handle the search flow
/// </summary>
public class JupiterSearchExample : MonoBehaviour
{
    [Header("Jupiter Components")]
    public JupiterSDKManager jupiterManager;
    public JupiterTokenSearch tokenSearch;
    
    [Header("Example UI Setup")]
    public Button searchButton;
    public GameObject searchPanel;
    public TMP_InputField contractAddressInput;
    public Button searchConfirmButton;
    public Button searchCancelButton;
    
    [Header("Token Info Display")]
    public TMP_Text tokenNameText;
    public TMP_Text tokenMarketCapText;
    public TMP_Text tokenVolumeText;
    public TMP_Text tokenPriceText;
    public Image tokenIconImage;
    public GameObject tokenInfoPanel;
    
    [Header("Example Contract Addresses")]
    public string[] exampleAddresses = {
        "DezXAZ8z7PnrnRJjz3wXBoRgixCa6xjnB7YaB1pPB263", // BONK
        "JUPyiwrYJFskUPiHa7hkeR8VUtAeFoSYbKedZNsDvCN", // JUP
        "4k3Dyjzvzp8eMZWUXbBCjEvwSkkk59S5iCNLY3QrkX6R", // RAY
        "SRMuApVNdxXokk5GT7XD5cUUgXMBCoAz2LHeuAoKWRt"  // SRM
    };
    
    private void Start()
    {
        SetupJupiterSearch();
        SetupExampleUI();
    }
    
    private void SetupJupiterSearch()
    {
        if (tokenSearch == null)
        {
            Debug.LogError("❌ JupiterTokenSearch component not assigned!");
            return;
        }
        
        // Assign the JupiterSDKManager reference
        if (jupiterManager != null)
        {
            tokenSearch.jupiterManager = jupiterManager;
        }
        else
        {
            Debug.LogWarning("⚠️ JupiterSDKManager not assigned to token search");
        }
    }
    
    private void SetupExampleUI()
    {
        // This is an example of how to set up the UI programmatically
        // In a real implementation, you would set these up in the Unity Inspector
        
        if (searchButton != null)
        {
            // Set up search button
            searchButton.onClick.AddListener(() => {
                Debug.Log("🔍 Search button clicked");
                if (searchPanel != null) searchPanel.SetActive(true);
            });
        }
        
        if (contractAddressInput != null)
        {
            // Set up input field
            contractAddressInput.placeholder.GetComponent<TMP_Text>().text = "Enter token contract address...";
            contractAddressInput.characterLimit = 44; // Solana address length
        }
        
        if (searchConfirmButton != null)
        {
            // Set up confirm button
            searchConfirmButton.onClick.AddListener(() => {
                Debug.Log("✅ Search confirmed");
                if (searchPanel != null) searchPanel.SetActive(false);
            });
        }
        
        if (searchCancelButton != null)
        {
            // Set up cancel button
            searchCancelButton.onClick.AddListener(() => {
                Debug.Log("❌ Search cancelled");
                if (searchPanel != null) searchPanel.SetActive(false);
                if (tokenInfoPanel != null) tokenInfoPanel.SetActive(false);
            });
        }
    }
    
    /// <summary>
    /// Example method to test the search functionality with a known token
    /// </summary>
    public void TestSearchWithBONK()
    {
        if (contractAddressInput != null)
        {
            contractAddressInput.text = "DezXAZ8z7PnrnRJjz3wXBoRgixCa6xjnB7YaB1pPB263";
            Debug.Log("🧪 Testing search with BONK token");
        }
    }
    
    /// <summary>
    /// Example method to test the search functionality with JUP token
    /// </summary>
    public void TestSearchWithJUP()
    {
        if (contractAddressInput != null)
        {
            contractAddressInput.text = "JUPyiwrYJFskUPiHa7hkeR8VUtAeFoSYbKedZNsDvCN";
            Debug.Log("🧪 Testing search with JUP token");
        }
    }
    
    /// <summary>
    /// Example method to test the search functionality with RAY token
    /// </summary>
    public void TestSearchWithRAY()
    {
        if (contractAddressInput != null)
        {
            contractAddressInput.text = "4k3Dyjzvzp8eMZWUXbBCjEvwSkkk59S5iCNLY3QrkX6R";
            Debug.Log("🧪 Testing search with RAY token");
        }
    }
    
    /// <summary>
    /// Example method to test the search functionality with SRM token
    /// </summary>
    public void TestSearchWithSRM()
    {
        if (contractAddressInput != null)
        {
            contractAddressInput.text = "SRMuApVNdxXokk5GT7XD5cUUgXMBCoAz2LHeuAoKWRt";
            Debug.Log("🧪 Testing search with SRM token");
        }
    }
    
    /// <summary>
    /// Example method to test with a random address from the example list
    /// </summary>
    public void TestSearchWithRandomToken()
    {
        if (contractAddressInput != null && exampleAddresses.Length > 0)
        {
            string randomAddress = exampleAddresses[Random.Range(0, exampleAddresses.Length)];
            contractAddressInput.text = randomAddress;
            Debug.Log($"🧪 Testing search with random token: {randomAddress}");
        }
    }
    
    /// <summary>
    /// Example method to clear the search input
    /// </summary>
    public void ClearSearchInput()
    {
        if (contractAddressInput != null)
        {
            contractAddressInput.text = "";
            Debug.Log("🧹 Search input cleared");
        }
    }
    
    /// <summary>
    /// Example method to show/hide the token info panel
    /// </summary>
    public void ToggleTokenInfoPanel()
    {
        if (tokenInfoPanel != null)
        {
            bool isActive = tokenInfoPanel.activeSelf;
            tokenInfoPanel.SetActive(!isActive);
            Debug.Log($"📊 Token info panel {(isActive ? "hidden" : "shown")}");
        }
    }
    
    /// <summary>
    /// Example method to simulate a successful token search
    /// </summary>
    public void SimulateTokenSearch()
    {
        if (tokenNameText != null) tokenNameText.text = "<b>Bonk</b> - <color=#888888><size=80%>BONK</size></color>";
        if (tokenMarketCapText != null) tokenMarketCapText.text = "Market Cap: $1,234,567";
        if (tokenVolumeText != null) tokenVolumeText.text = "24h Volume: $987,654";
        if (tokenPriceText != null) tokenPriceText.text = "Price: $0.000012";
        
        if (tokenInfoPanel != null) tokenInfoPanel.SetActive(true);
        
        Debug.Log("🎭 Simulated token search results displayed");
    }
    
    /// <summary>
    /// Example method to simulate a failed token search
    /// </summary>
    public void SimulateFailedSearch()
    {
        if (tokenNameText != null) tokenNameText.text = "Token not found";
        if (tokenMarketCapText != null) tokenMarketCapText.text = "";
        if (tokenVolumeText != null) tokenVolumeText.text = "";
        if (tokenPriceText != null) tokenPriceText.text = "";
        
        if (tokenInfoPanel != null) tokenInfoPanel.SetActive(true);
        
        Debug.Log("❌ Simulated failed token search");
    }
    
    /// <summary>
    /// Example method to validate a contract address format
    /// </summary>
    public bool ValidateContractAddress(string address)
    {
        if (string.IsNullOrEmpty(address))
            return false;
            
        // Basic Solana address validation (44 characters, base58)
        if (address.Length != 44)
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
    
    /// <summary>
    /// Example method to format a contract address for display
    /// </summary>
    public string FormatContractAddress(string address)
    {
        if (string.IsNullOrEmpty(address) || address.Length < 8)
            return address;
            
        // Format as: first 4 chars...last 4 chars
        return $"{address.Substring(0, 4)}...{address.Substring(address.Length - 4)}";
    }
    
    /// <summary>
    /// Example method to copy contract address to clipboard
    /// </summary>
    public void CopyToClipboard(string text)
    {
        GUIUtility.systemCopyBuffer = text;
        Debug.Log($"📋 Copied to clipboard: {text}");
    }
    
    /// <summary>
    /// Example method to paste from clipboard
    /// </summary>
    public void PasteFromClipboard()
    {
        if (contractAddressInput != null)
        {
            contractAddressInput.text = GUIUtility.systemCopyBuffer;
            Debug.Log($"📋 Pasted from clipboard: {GUIUtility.systemCopyBuffer}");
        }
    }
} 