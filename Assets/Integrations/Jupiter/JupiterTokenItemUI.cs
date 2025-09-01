using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Networking;
using UnityEngine.EventSystems;

public class JupiterTokenItemUI : MonoBehaviour, IPointerClickHandler
{
    public Image iconImage;
    public TMP_Text nameText;
    public TMP_Text priceText;
    public TMP_Text mcapText;
    
    [Header("Optional - Price Change Display")]
    public TMP_Text priceChangeText; // Assign in inspector to show percentage
    public bool showPriceChange = true; // Toggle to show/hide percentage
    
    [Header("Jupiter Search Integration")]
    public JupiterTokenSearch jupiterTokenSearch;
    public GameObject jupiterCanvas;
    
    private JupiterTrendingTokens.JupiterToken _currentToken;
    
    private void Start()
    {
        // Find references at runtime if not assigned
        if (jupiterTokenSearch == null)
        {
            jupiterTokenSearch = JupiterTokenSearch.Instance;
        }
        
        if (jupiterCanvas == null)
        {
            // Try to find Jupiter canvas by name (including inactive objects)
            jupiterCanvas = FindJupiterCanvasIncludingInactive();
        }
    }
    
    private GameObject FindJupiterCanvasIncludingInactive()
    {
        // Search all GameObjects in the scene, including inactive ones
        GameObject[] allObjects = Resources.FindObjectsOfTypeAll<GameObject>();
        
        foreach (GameObject obj in allObjects)
        {
            // Check if it's the Jupiter canvas by name
            if (obj.name == "JupiterCanvas" || obj.name == "Jupiter Canvas")
            {
                Debug.Log($"✅ Found Jupiter canvas (including inactive): {obj.name}");
                return obj;
            }
        }
        
        Debug.LogWarning("⚠️ Could not find JupiterCanvas in scene (including inactive objects).");
        return null;
    }

    public void SetToken(JupiterTrendingTokens.JupiterToken token)
    {
        _currentToken = token;
        nameText.text = $"{token.symbol}";
        priceText.text = token.usdPrice > 0 ? $"${token.usdPrice:F4}" : "";
        mcapText.text = token.mcap > 0 ? $"MCap: ${token.mcap:N0}" : "MCap: N/A";
        
        // Display price change percentage if enabled and available
        if (showPriceChange && priceChangeText != null && token.showPriceChange)
        {
            DisplayPriceChange(token.priceChangePercent);
        }
        else if (priceChangeText != null)
        {
            // Hide the percentage text if not showing
            priceChangeText.gameObject.SetActive(false);
        }
        
        // Don't load icon here - it will be loaded by the parent with fallback
    }
    
    private void DisplayPriceChange(float percentChange)
    {
        if (priceChangeText == null) return;
        
        // Show the percentage text
        priceChangeText.gameObject.SetActive(true);
        
        // Format the percentage with + or - sign
        string sign = percentChange >= 0 ? "+" : "";
        string formattedPercent = $"{sign}{percentChange:F2}%";
        
        // Set color based on gain/loss
        Color textColor;
        if (percentChange > 0)
        {
            textColor = Color.green; // Green for gains
        }
        else if (percentChange < 0)
        {
            textColor = Color.red; // Red for losses
        }
        else
        {
            textColor = Color.white; // White for no change
        }
        
        // Apply the text and color
        priceChangeText.text = formattedPercent;
        priceChangeText.color = textColor;
        
        Debug.Log($"📊 Price change: {formattedPercent} ({textColor})");
    }
    
    public void SetTokenIcon(Sprite sprite)
    {
        if (iconImage != null && sprite != null)
        {
            iconImage.sprite = sprite;
        }
    }
    
    public bool HasValidIcon()
    {
        return iconImage != null && iconImage.sprite != null;
    }
    
    // Public method to update price change percentage
    public void UpdatePriceChange(float newPercentChange)
    {
        if (showPriceChange && priceChangeText != null)
        {
            DisplayPriceChange(newPercentChange);
        }
    }
    
    // Public method to toggle price change display
    public void SetPriceChangeVisibility(bool visible)
    {
        showPriceChange = visible;
        if (priceChangeText != null)
        {
            priceChangeText.gameObject.SetActive(visible && _currentToken != null && _currentToken.showPriceChange);
        }
    }

    IEnumerator LoadIcon(string url)
    {
        if (string.IsNullOrEmpty(url)) 
        {
            SetDefaultIcon();
            yield break;
        }
        
        using (UnityWebRequest req = UnityWebRequestTexture.GetTexture(url))
        {
            req.timeout = 5; // 5 second timeout
            yield return req.SendWebRequest();
            
            if (req.result == UnityWebRequest.Result.Success)
            {
                Texture2D tex = DownloadHandlerTexture.GetContent(req);
                if (tex != null && tex.width > 0 && tex.height > 0)
                {
                    iconImage.sprite = Sprite.Create(
                        tex,
                        new Rect(0, 0, tex.width, tex.height),
                        new Vector2(0.5f, 0.5f),
                        100
                    );
                    Debug.Log($"✅ Loaded token icon from: {url}");
                }
                else
                {
                    Debug.LogWarning($"⚠️ Invalid texture from: {url}");
                    SetDefaultIcon();
                }
            }
            else
            {
                Debug.LogWarning($"⚠️ Failed to load token icon from: {url} - {req.error}");
                SetDefaultIcon();
            }
        }
    }
    
    private void SetDefaultIcon()
    {
        // You can assign a default token icon sprite in the inspector
        // For now, we'll just clear the icon
        if (iconImage != null)
        {
            iconImage.sprite = null;
        }
    }
    
    public void OnPointerClick(PointerEventData eventData)
    {
        if (_currentToken != null)
        {
            Debug.Log($"🔄 Trending token clicked: {_currentToken.symbol} ({_currentToken.id})");
            OpenJupiterSearchWithToken(_currentToken);
        }
    }
    
    private void OpenJupiterSearchWithToken(JupiterTrendingTokens.JupiterToken token)
    {
        // Try to find JupiterTokenSearch if not assigned
        if (jupiterTokenSearch == null)
        {
            jupiterTokenSearch = JupiterTokenSearch.Instance;
            
            // If Instance is null (first click), find it directly
            if (jupiterTokenSearch == null)
            {
                jupiterTokenSearch = FindObjectOfType<JupiterTokenSearch>(true); // true = include inactive
                Debug.Log($"🔍 JupiterTokenSearch found via FindObjectOfType: {(jupiterTokenSearch != null ? "YES" : "NO")}");
            }
            else
            {
                Debug.Log($"🔍 JupiterTokenSearch found via Instance: YES");
            }
        }
        
        // Try to find Jupiter canvas if not assigned
        if (jupiterCanvas == null)
        {
            jupiterCanvas = FindJupiterCanvasIncludingInactive();
        }
        
        // Activate Jupiter canvas if it's not already active
        if (jupiterCanvas != null && !jupiterCanvas.activeInHierarchy)
        {
            jupiterCanvas.SetActive(true);
            Debug.Log($"✅ Activated Jupiter canvas: {jupiterCanvas.name}");
        }
        
        // CRITICAL: Ensure JupiterCanvas and all its essential children are active
        if (jupiterCanvas != null)
        {
            // Activate JupiterCanvas itself
            if (!jupiterCanvas.activeInHierarchy)
            {
                jupiterCanvas.SetActive(true);
                Debug.Log($"✅ Activated JupiterCanvas: {jupiterCanvas.name}");
            }
            
            // Activate essential child objects in JupiterCanvas
            ActivateEssentialChildren(jupiterCanvas.transform);
        }
        else
        {
            Debug.LogWarning("⚠️ JupiterCanvas reference is null!");
        }
        
        // Activate JupiterTokenSearch GameObject if it's disabled
        if (jupiterTokenSearch != null && !jupiterTokenSearch.gameObject.activeInHierarchy)
        {
            jupiterTokenSearch.gameObject.SetActive(true);
            Debug.Log($"✅ Activated JupiterTokenSearch GameObject");
        }
        
        // CRITICAL: Ensure the search panel stays active by activating its parent hierarchy
        if (jupiterTokenSearch != null && jupiterTokenSearch.searchPanel != null)
        {
            // Activate the search panel's parent hierarchy to ensure it stays active
            Transform parent = jupiterTokenSearch.searchPanel.transform.parent;
            while (parent != null)
            {
                if (!parent.gameObject.activeInHierarchy)
                {
                    parent.gameObject.SetActive(true);
                    Debug.Log($"✅ Activated parent: {parent.name}");
                }
                parent = parent.parent;
            }
            
            // Now activate the search panel itself
            if (!jupiterTokenSearch.searchPanel.activeInHierarchy)
            {
                jupiterTokenSearch.searchPanel.SetActive(true);
                Debug.Log($"✅ Activated search panel");
            }
            else
            {
                Debug.Log($"✅ Search panel already active");
            }
        }
        else
        {
            Debug.LogWarning("⚠️ Search panel reference is null!");
        }
        
        // Ensure the contract address input is active and visible
        if (jupiterTokenSearch != null && jupiterTokenSearch.contractAddressInput != null)
        {
            // Activate the input's parent hierarchy too
            Transform inputParent = jupiterTokenSearch.contractAddressInput.transform.parent;
            while (inputParent != null)
            {
                if (!inputParent.gameObject.activeInHierarchy)
                {
                    inputParent.gameObject.SetActive(true);
                    Debug.Log($"✅ Activated input parent: {inputParent.name}");
                }
                inputParent = inputParent.parent;
            }
            
            if (!jupiterTokenSearch.contractAddressInput.gameObject.activeInHierarchy)
            {
                jupiterTokenSearch.contractAddressInput.gameObject.SetActive(true);
                Debug.Log($"✅ Activated contract address input");
            }
            else
            {
                Debug.Log($"✅ Contract address input already active");
            }
        }
        else
        {
            Debug.LogWarning("⚠️ Contract address input reference is null!");
        }
        
        // Set up the search with the token's mint address
        if (jupiterTokenSearch != null)
        {
            Debug.Log($"🔍 Setting up search with JupiterTokenSearch: {jupiterTokenSearch.name}");
            
            // Set the contract address input with the token's mint address
            if (jupiterTokenSearch.contractAddressInput != null)
            {
                jupiterTokenSearch.contractAddressInput.text = token.id;
                Debug.Log($"✅ Set search input to: {token.id}");
            }
            else
            {
                Debug.LogWarning("⚠️ contractAddressInput is null!");
            }
            
            // Open the search panel
            if (jupiterTokenSearch.searchPanel != null)
            {
                jupiterTokenSearch.searchPanel.SetActive(true);
                Debug.Log($"✅ Opened Jupiter search panel");
            }
            else
            {
                Debug.LogWarning("⚠️ searchPanel is null!");
            }
            
            // Wait a frame to ensure everything is activated, then trigger the search
            StartCoroutine(TriggerSearchAfterActivation(token.id));
        }
        else
        {
            Debug.LogWarning("⚠️ JupiterTokenSearch not found! Make sure JupiterTokenSearch is in the scene.");
        }
    }
    
    private IEnumerator TriggerSearchAfterActivation(string tokenId)
    {
        // Wait a frame to ensure all GameObjects are properly activated
        yield return null;
        
        // CRITICAL: Force activation check and wait for everything to be ready
        yield return StartCoroutine(EnsureAllObjectsAreActive());
        
        // Debug: Check if everything is properly activated
        if (jupiterTokenSearch != null)
        {
            Debug.Log($"🔍 JupiterTokenSearch active: {jupiterTokenSearch.gameObject.activeInHierarchy}");
            if (jupiterTokenSearch.searchPanel != null)
            {
                Debug.Log($"🔍 Search panel active: {jupiterTokenSearch.searchPanel.activeInHierarchy}");
            }
            if (jupiterTokenSearch.contractAddressInput != null)
            {
                Debug.Log($"🔍 Contract input active: {jupiterTokenSearch.contractAddressInput.gameObject.activeInHierarchy}");
            }
        }
        
        // CRITICAL: Wait longer for the first activation to ensure everything is fully ready
        yield return new WaitForSeconds(0.2f);
        
        // Double-check that everything is still active before proceeding
        if (jupiterTokenSearch != null && 
            jupiterTokenSearch.gameObject.activeInHierarchy &&
            jupiterTokenSearch.searchPanel != null && 
            jupiterTokenSearch.searchPanel.activeInHierarchy)
        {
            Debug.Log($"✅ All UI elements are active, triggering search for: {tokenId}");
            
            // Now trigger the search
            jupiterTokenSearch.SearchTokenByAddress(tokenId);
            
            // Also ensure the token info panel is visible
            if (jupiterTokenSearch.tokenInfoPanel != null && !jupiterTokenSearch.tokenInfoPanel.activeInHierarchy)
            {
                jupiterTokenSearch.tokenInfoPanel.SetActive(true);
                Debug.Log($"✅ Activated token info panel");
            }
        }
        else
        {
            Debug.LogError($"❌ Failed to activate UI elements for search. TokenSearch active: {jupiterTokenSearch?.gameObject.activeInHierarchy}, SearchPanel active: {jupiterTokenSearch?.searchPanel?.activeInHierarchy}");
            
            // Try one more time with additional delay
            yield return new WaitForSeconds(0.3f);
            
            if (jupiterTokenSearch != null && 
                jupiterTokenSearch.gameObject.activeInHierarchy &&
                jupiterTokenSearch.searchPanel != null && 
                jupiterTokenSearch.searchPanel.activeInHierarchy)
            {
                Debug.Log($"✅ UI elements activated on retry, triggering search for: {tokenId}");
                jupiterTokenSearch.SearchTokenByAddress(tokenId);
            }
            else
            {
                Debug.LogError($"❌ Failed to activate UI elements even after retry. Please check the hierarchy.");
            }
        }
    }
    
    private IEnumerator EnsureAllObjectsAreActive()
    {
        Debug.Log("🔄 Ensuring all objects are properly activated...");
        
        // Force activation of JupiterTokenSearch if needed
        if (jupiterTokenSearch != null && !jupiterTokenSearch.gameObject.activeInHierarchy)
        {
            jupiterTokenSearch.gameObject.SetActive(true);
            Debug.Log("✅ Forced activation of JupiterTokenSearch");
        }
        
        // Force activation of search panel if needed
        if (jupiterTokenSearch?.searchPanel != null && !jupiterTokenSearch.searchPanel.activeInHierarchy)
        {
            jupiterTokenSearch.searchPanel.SetActive(true);
            Debug.Log("✅ Forced activation of search panel");
        }
        
        // Force activation of contract input if needed
        if (jupiterTokenSearch?.contractAddressInput != null && !jupiterTokenSearch.contractAddressInput.gameObject.activeInHierarchy)
        {
            jupiterTokenSearch.contractAddressInput.gameObject.SetActive(true);
            Debug.Log("✅ Forced activation of contract input");
        }
        
        // Wait a bit for the activation to take effect
        yield return new WaitForSeconds(0.1f);
        
        Debug.Log("✅ Object activation check complete");
    }

    private void ActivateEssentialChildren(Transform parent)
    {
        // Activate all children of the parent transform
        foreach (Transform child in parent)
        {
            if (!child.gameObject.activeInHierarchy)
            {
                child.gameObject.SetActive(true);
                Debug.Log($"✅ Activated essential child: {child.name}");
            }
            
            // Activate specific important objects by name
            if (child.name.Contains("Search") || child.name.Contains("Panel") || 
                child.name.Contains("Canvas") || child.name.Contains("Jupiter"))
            {
                if (!child.gameObject.activeInHierarchy)
                {
                    child.gameObject.SetActive(true);
                    Debug.Log($"✅ Activated important object: {child.name}");
                }
            }
            
            // Recursively activate children of children
            ActivateEssentialChildren(child);
        }
    }
}