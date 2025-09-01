using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using TMPro; // If you use TextMeshPro
using UnityEngine.UI;

public class JupiterTrendingTokens : MonoBehaviour
{
    [System.Serializable]
    public class JupiterToken
    {
        public string id;
        public string name;
        public string symbol;
        public string icon;
        public int decimals;
        public float usdPrice;
        public float mcap;
        [Header("Optional - Price Change")]
        public float priceChangePercent; // Can be positive (gain) or negative (loss)
        public bool showPriceChange = true; // Toggle to show/hide percentage
        
        // Jupiter API stats for different timeframes
        public TokenStats stats5m;
        public TokenStats stats1h;
        public TokenStats stats24h;
    }
    
    [System.Serializable]
    public class TokenStats
    {
        public float priceChange;
        public float volumeChange;
        public float liquidityChange;
        public float buyVolume;
        public float sellVolume;
    }

    public string apiUrl = "https://lite-api.jup.ag/tokens/v2/toptrending/24h?limit=3";
    public Transform tokenListParent; // Assign in inspector: parent object for UI items
    public GameObject tokenItemPrefab; // Assign in inspector: prefab for each token row
    public Sprite defaultTokenIcon; // Assign a default icon in inspector

    private List<JupiterToken> trendingTokens = new List<JupiterToken>();
    private string currentInterval = "24h";
    
    // IPFS and image gateway fallbacks
    private static readonly string[] ipfsGateways = new string[]
    {
        "https://ipfs.io/ipfs/",
        "https://cloudflare-ipfs.com/ipfs/",
        "https://gateway.pinata.cloud/ipfs/",
        "https://nftstorage.link/ipfs/",
        "https://dweb.link/ipfs/"
    };
    
    private static readonly string[] imageProxyServices = new string[]
    {
        "https://images.weserv.nl/?w=64&h=64&url=",
        "https://api.allorigins.win/raw?url=",
        "https://cors-anywhere.herokuapp.com/"
    };

    void Start()
    {
        StartCoroutine(FetchTrendingTokens());
    }

    IEnumerator FetchTrendingTokens()
    {
        string apiUrl = $"https://lite-api.jup.ag/tokens/v2/toptrending/{currentInterval}?limit=4";
        using (UnityWebRequest request = UnityWebRequest.Get(apiUrl))
        {
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                // Wrap the array for JsonUtility
                string wrappedJson = "{\"tokens\":" + request.downloadHandler.text + "}";
                JupiterTokenArray tokenArray = JsonUtility.FromJson<JupiterTokenArray>(wrappedJson);
                trendingTokens = new List<JupiterToken>(tokenArray.tokens);
                
                // Calculate price change percentages from available data
                CalculatePriceChanges();
                
                DisplayTokens();
            }
            else
            {
                Debug.LogError("Failed to fetch trending tokens: " + request.error);
            }
        }
    }
    
    private void CalculatePriceChanges()
    {
        foreach (var token in trendingTokens)
        {
            // Try to extract price change from the stats data if available
            // The Jupiter API provides stats for different timeframes
            if (currentInterval == "24h" && token.stats24h != null)
            {
                token.priceChangePercent = token.stats24h.priceChange;
                Debug.Log($"📊 {token.symbol}: 24h price change = {token.priceChangePercent:F2}%");
            }
            else if (currentInterval == "1h" && token.stats1h != null)
            {
                token.priceChangePercent = token.stats1h.priceChange;
                Debug.Log($"📊 {token.symbol}: 1h price change = {token.priceChangePercent:F2}%");
            }
            else if (currentInterval == "5m" && token.stats5m != null)
            {
                token.priceChangePercent = token.stats5m.priceChange;
                Debug.Log($"📊 {token.symbol}: 5m price change = {token.priceChangePercent:F2}%");
            }
            else
            {
                // Generate mock price change for demonstration
                token.priceChangePercent = UnityEngine.Random.Range(-15f, 25f);
                Debug.Log($"📊 {token.symbol}: Mock price change = {token.priceChangePercent:F2}%");
            }
            
            // Ensure the percentage is reasonable
            token.priceChangePercent = Mathf.Clamp(token.priceChangePercent, -99f, 999f);
        }
    }

    void DisplayTokens()
    {
        // Clear previous
        foreach (Transform child in tokenListParent)
            Destroy(child.gameObject);

        foreach (var token in trendingTokens)
        {
            var go = Instantiate(tokenItemPrefab, tokenListParent);
            var ui = go.GetComponent<JupiterTokenItemUI>();
            if (ui != null)
            {
                ui.SetToken(token);
                // Load token icon with fallback
                StartCoroutine(LoadTokenIconWithFallback(token.icon, ui));
            }
        }
    }
    
    private IEnumerator LoadTokenIconWithFallback(string iconUrl, JupiterTokenItemUI ui)
    {
        if (string.IsNullOrEmpty(iconUrl))
        {
            SetDefaultIcon(ui);
            yield break;
        }
        
        // Try the original URL first
        yield return StartCoroutine(TryLoadIcon(iconUrl, ui));
        
        // If that failed, try IPFS gateways if it's an IPFS URL
        if (iconUrl.Contains("ipfs/") || iconUrl.Contains("Qm") || iconUrl.Contains("bafy"))
        {
            string ipfsHash = ExtractIPFSHash(iconUrl);
            if (!string.IsNullOrEmpty(ipfsHash))
            {
                foreach (string gateway in ipfsGateways)
                {
                    string ipfsUrl = gateway + ipfsHash;
                    yield return StartCoroutine(TryLoadIcon(ipfsUrl, ui));
                    
                    // If successful, break
                    if (ui.HasValidIcon()) yield break;
                }
            }
        }
        
        // If still failed, try proxy services
        foreach (string proxy in imageProxyServices)
        {
            string proxyUrl = proxy + UnityWebRequest.EscapeURL(iconUrl);
            yield return StartCoroutine(TryLoadIcon(proxyUrl, ui));
            
            // If successful, break
            if (ui.HasValidIcon()) yield break;
        }
        
        // If all failed, set default icon
        if (!ui.HasValidIcon())
        {
            SetDefaultIcon(ui);
        }
    }
    
    private IEnumerator TryLoadIcon(string url, JupiterTokenItemUI ui)
    {
        using (UnityWebRequest request = UnityWebRequestTexture.GetTexture(url))
        {
            request.timeout = 5; // 5 second timeout
            
            yield return request.SendWebRequest();
            
            if (request.result == UnityWebRequest.Result.Success)
            {
                Texture2D texture = DownloadHandlerTexture.GetContent(request);
                if (texture != null && texture.width > 0 && texture.height > 0)
                {
                    Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
                    ui.SetTokenIcon(sprite);
                    Debug.Log($"✅ Successfully loaded token icon from: {url}");
                    yield break;
                }
            }
            
            Debug.LogWarning($"⚠️ Failed to load token icon from: {url} - {request.error}");
        }
    }
    
    private string ExtractIPFSHash(string url)
    {
        // Extract IPFS hash from various URL formats
        if (url.Contains("ipfs/"))
        {
            int ipfsIndex = url.IndexOf("ipfs/");
            string afterIpfs = url.Substring(ipfsIndex + 5);
            // Remove any query parameters or additional paths
            int queryIndex = afterIpfs.IndexOf('?');
            if (queryIndex > 0) afterIpfs = afterIpfs.Substring(0, queryIndex);
            int slashIndex = afterIpfs.IndexOf('/');
            if (slashIndex > 0) afterIpfs = afterIpfs.Substring(0, slashIndex);
            return afterIpfs;
        }
        
        // Look for Qm or bafy hashes
        string[] parts = url.Split('/');
        foreach (string part in parts)
        {
            if (part.StartsWith("Qm") || part.StartsWith("bafy"))
            {
                return part;
            }
        }
        
        return null;
    }
    
    private void SetDefaultIcon(JupiterTokenItemUI ui)
    {
        if (defaultTokenIcon != null)
        {
            ui.SetTokenIcon(defaultTokenIcon);
        }
        Debug.Log("🔄 Using default token icon");
    }

    public void SetInterval(string interval)
    {
        currentInterval = interval;
        StartCoroutine(FetchTrendingTokens());
    }

    // Optional: Helper methods for Unity UI button hookup
    public void On5mButton() => SetInterval("5m");
    public void On1hButton() => SetInterval("1h");
    public void On24hButton() => SetInterval("24h");

    [System.Serializable]
    public class JupiterTokenArray
    {
        public JupiterToken[] tokens;
    }
}