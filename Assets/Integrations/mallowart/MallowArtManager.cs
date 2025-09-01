using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using TMPro;
using System.Text;

public class MallowArtManager : MonoBehaviour
{
    [Header("Assign the plane objects (1 per artwork)")]
    public List<GameObject> planeObjects;

    [Header("Assign TextMeshPro text fields")]
    public List<TextMeshPro> nameTexts;
    public List<TextMeshPro> timerTexts;
    public List<TextMeshPro> bidTexts;

    [Header("Settings")]
    public int numberOfArtworksToShow = 3;

    private List<DateTime> auctionEndTimes = new List<DateTime>();
    private float timerUpdateInterval = 1f;
    private float timeSinceLastUpdate = 0f;

    private const string DefaultArtistName = "Unknown Artist";

    private const string ApiUrl = "https://api.mallow.art/v1/artworks/listedBySeller";
    private const string ApiKey = "6HBE7PH9D2HDG4Q6";
    private const string SellerAddress = "iwuAU7w9VokK1NZbNFjQu78zrLdJ1VkG2Hbc62iUksW";

    // IPFS Gateways - ordered by reliability (Mallow gateway first since it's specific to their content)
    private readonly string[] ipfsGateways = {
        "https://ipfs.mallow.art/ipfs/",          // Mallow's own IPFS gateway (most reliable for their content)
        "https://gateway.pinata.cloud/ipfs/",     // Fallback 1 (may rate limit)
        "https://ipfs.io/ipfs/",                  // Fallback 2
        "https://dweb.link/ipfs/",                // Fallback 3
        "https://gateway.ipfs.io/ipfs/"           // Fallback 4
    };

    [Serializable]
    public class AuctionMetadata
    {
        public long currentBidAmount;
        public int minBidIncrementBps;
        public string endsAt;
        public string[] bidders;
    }

    [Serializable]
    public class ArtworkItem
    {
        public string imageUrl;
        public string name;
        public string description;
        public AuctionMetadata auctionMetadata;
        public string mintAccount;   // Mallow returns this
        public string auctionHouse;  // Mallow returns this
    }

    [Serializable]
    public class Wrapper
    {
        public ArtworkItem[] result;
    }

    [Serializable]
    public class Filter
    {
        public string seller;
        public string[] listingTypes;
    }

    [Serializable]
    public class RequestPayload
    {
        public int page;
        public string sort;
        public Filter filter;
    }

    void Start()
    {
        Debug.Log($"MALLOWART: 🚀 MallowArtManager starting...");
        Debug.Log($"MALLOWART: 🔗 API URL: {ApiUrl}");
        Debug.Log($"MALLOWART: 🔑 API Key: {ApiKey.Substring(0, 4)}...{ApiKey.Substring(ApiKey.Length - 4)}");
        Debug.Log($"MALLOWART: 👤 Seller Address: {SellerAddress}");
        
        StartCoroutine(FetchAndDisplayArtworks());
    }

    void Update()
    {
        timeSinceLastUpdate += Time.deltaTime;
        if (timeSinceLastUpdate >= timerUpdateInterval)
        {
            timeSinceLastUpdate = 0f;
            UpdateCountdownTimers();
        }
    }

    // Add a public method to manually retry fetching
    public void RetryFetchArtworks()
    {
        Debug.Log("MALLOWART: 🔄 Manually retrying to fetch artworks...");
        StartCoroutine(FetchAndDisplayArtworks());
    }

    // Add a method to test a specific IPFS hash
    public void TestIPFSHash(string ipfsHash)
    {
        Debug.Log($"MALLOWART: 🧪 Testing IPFS hash: {ipfsHash}");
        StartCoroutine(TestIPFSHashCoroutine(ipfsHash));
    }

    IEnumerator TestIPFSHashCoroutine(string ipfsHash)
    {
        Debug.Log($"MALLOWART: 🧪 Testing Pinata gateway for hash: {ipfsHash}");
        
        string gatewayUrl = ipfsGateways[0] + ipfsHash;
        Debug.Log($"MALLOWART: 🧪 Testing gateway: {gatewayUrl}");
        
        using UnityWebRequest uwr = UnityWebRequestTexture.GetTexture(gatewayUrl);
        uwr.timeout = 10;
        uwr.SetRequestHeader("User-Agent", "Unity/2022.3.0f1");
        
        yield return uwr.SendWebRequest();
        
        if (uwr.result == UnityWebRequest.Result.Success)
        {
            Debug.Log($"MALLOWART: ✅ Gateway SUCCESS for hash: {ipfsHash}");
            Debug.Log($"MALLOWART: 📊 Response size: {uwr.downloadedBytes} bytes");
            Debug.Log($"MALLOWART: 📊 Content-Type: {uwr.GetResponseHeader("content-type")}");
        }
        else
        {
            Debug.LogWarning($"MALLOWART: ❌ Gateway FAILED: {uwr.error} (Code: {uwr.responseCode})");
            if (uwr.responseCode == 500)
            {
                Debug.LogError($"MALLOWART: ❌ IPFS Hash {ipfsHash} appears to be corrupted or not available (HTTP 500)");
            }
        }
    }

    IEnumerator FetchAndDisplayArtworks()
    {
        Debug.Log($"MALLOWART: 🚀 FetchAndDisplayArtworks started");
        Debug.Log($"MALLOWART: 🔗 API URL: {ApiUrl}");
        Debug.Log($"MALLOWART: 🔑 API Key: {ApiKey.Substring(0, 4)}...{ApiKey.Substring(ApiKey.Length - 4)}");
        Debug.Log($"MALLOWART: 👤 Seller Address: {SellerAddress}");
        
        RequestPayload payload = new RequestPayload
        {
            page = 0,
            sort = "recently-listed",
            filter = new Filter
            {
                seller = SellerAddress,
                listingTypes = new[] { "auction" }
            }
        };

        string jsonData = JsonUtility.ToJson(payload);
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);

        Debug.Log($"MALLOWART: 🌐 Fetching artworks from Mallow API...");
        Debug.Log($"MALLOWART: 📤 Request payload: {jsonData}");

        using UnityWebRequest request = new UnityWebRequest(ApiUrl, "POST");
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("X-Api-Key", ApiKey);

        Debug.Log($"MALLOWART: 📡 Sending API request...");
        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError($"MALLOWART: ❌ Mallow API Error ({request.responseCode}): {request.error}");
            Debug.LogError($"MALLOWART: ❌ Response: {request.downloadHandler.text}");
            yield break;
        }

        Debug.Log($"MALLOWART: ✅ Mallow API Response received");
        Debug.Log($"MALLOWART: 📊 Response size: {request.downloadedBytes} bytes");
        Debug.Log($"MALLOWART: 📊 Response headers: {request.GetResponseHeader("content-type")}");
        
        Wrapper wrapped = JsonUtility.FromJson<Wrapper>(request.downloadHandler.text);
        
        if (wrapped?.result == null)
        {
            Debug.LogError($"MALLOWART: ❌ No results in API response");
            yield break;
        }

        Debug.Log($"MALLOWART: 📊 Found {wrapped.result.Length} artworks");
        Debug.Log($"MALLOWART: 🎯 Will display {Mathf.Min(numberOfArtworksToShow, wrapped.result.Length, planeObjects.Count)} artworks");
        auctionEndTimes.Clear();

        for (int i = 0; i < Mathf.Min(numberOfArtworksToShow, wrapped.result.Length, planeObjects.Count); i++)
        {
            ArtworkItem artwork = wrapped.result[i];
            Debug.Log($"MALLOWART: 🎨 Artwork {i}: {artwork.name}");
            Debug.Log($"MALLOWART: 🖼️ Image URL: {artwork.imageUrl}");
            Debug.Log($"MALLOWART: 💰 Current Bid: {artwork.auctionMetadata?.currentBidAmount}");
            Debug.Log($"MALLOWART: ⏰ Ends At: {artwork.auctionMetadata?.endsAt}");
            Debug.Log($"MALLOWART: 🔗 Mint Account: {artwork.mintAccount}");
            Debug.Log($"MALLOWART: 🏠 Auction House: {artwork.auctionHouse}");
            
            StartCoroutine(LoadArtworkToPlane(i, artwork));
        }
        
        Debug.Log($"MALLOWART: ✅ FetchAndDisplayArtworks completed");
    }

    IEnumerator LoadArtworkToPlane(int index, ArtworkItem artwork)
    {
        Debug.Log($"MALLOWART: 🚀 LoadArtworkToPlane called for index {index}");
        
        // Check if image URL is valid
        if (string.IsNullOrEmpty(artwork.imageUrl))
        {
            Debug.LogWarning($"MALLOWART: ⚠️ Artwork {index} has no image URL: {artwork.name}");
            yield break;
        }

        Debug.Log($"MALLOWART: 🖼️ Loading image for artwork {index}: {artwork.name} from URL: {artwork.imageUrl}");

        // Check if this is an IPFS URL and try multiple gateways
        if (artwork.imageUrl.Contains("ipfs.io/ipfs/"))
        {
            Debug.Log($"MALLOWART: 🔗 Detected IPFS URL, using fallback system");
            yield return StartCoroutine(LoadIPFSImageWithFallbacks(index, artwork));
        }
        else
        {
            Debug.Log($"MALLOWART: 🌐 Using regular image loading");
            yield return StartCoroutine(LoadRegularImage(index, artwork));
        }
        
        Debug.Log($"MALLOWART: ✅ LoadArtworkToPlane completed for index {index}");
    }

    IEnumerator LoadIPFSImageWithFallbacks(int index, ArtworkItem artwork)
    {
        // Extract the IPFS hash from the URL
        string ipfsHash = artwork.imageUrl.Replace("https://ipfs.io/ipfs/", "");
        Debug.Log($"MALLOWART: 🔗 IPFS Hash: {ipfsHash}");
        Debug.Log($"MALLOWART: 🎨 Artwork Name: {artwork.name}");
        Debug.Log($"MALLOWART: 🔗 Mint Account: {artwork.mintAccount}");

        // Try each gateway
        for (int gatewayIndex = 0; gatewayIndex < ipfsGateways.Length; gatewayIndex++)
        {
            string gatewayUrl = ipfsGateways[gatewayIndex] + ipfsHash;
            Debug.Log($"MALLOWART: 🌐 Trying gateway {gatewayIndex + 1}/{ipfsGateways.Length}: {gatewayUrl}");

            // Try with retry logic
            int maxRetries = 1; // Only 1 retry per gateway to avoid rate limiting
            for (int attempt = 1; attempt <= maxRetries; attempt++)
            {
                using UnityWebRequest uwr = UnityWebRequestTexture.GetTexture(gatewayUrl);
                
                // Add timeout and headers
                uwr.timeout = 8; // Shorter timeout
                uwr.SetRequestHeader("User-Agent", "Unity/2022.3.0f1");
                
                yield return uwr.SendWebRequest();

                if (uwr.result == UnityWebRequest.Result.Success)
                {
                    Debug.Log($"MALLOWART: ✅ Image {index} loaded successfully from gateway {gatewayIndex + 1} on attempt {attempt}");
                    Debug.Log($"MALLOWART: 📊 Downloaded bytes: {uwr.downloadedBytes}");
                    Debug.Log($"MALLOWART: 📊 Content-Type: {uwr.GetResponseHeader("content-type")}");
                    Texture2D texture = DownloadHandlerTexture.GetContent(uwr);
                    
                    if (texture != null)
                    {
                        Debug.Log($"MALLOWART: 🖼️ Texture created: {texture.width}x{texture.height}");
                        ApplyArtworkToPlane(index, artwork, texture);
                        yield break; // Success, exit
                    }
                    else
                    {
                        Debug.LogError($"MALLOWART: ❌ Image {index} texture is null after successful download");
                    }
                }
                else
                {
                    Debug.LogWarning($"MALLOWART: ⚠️ Gateway {gatewayIndex + 1} attempt {attempt}/{maxRetries} failed: {uwr.error} (Response Code: {uwr.responseCode})");
                    Debug.LogWarning($"MALLOWART: 📊 Response size: {uwr.downloadedBytes} bytes");
                    Debug.LogWarning($"MALLOWART: 📊 Response headers: {uwr.GetResponseHeader("content-type")}");
                    
                    if (uwr.responseCode == 429)
                    {
                        Debug.LogWarning($"MALLOWART: ⚠️ Gateway {gatewayIndex + 1} is rate limited (HTTP 429), trying next gateway");
                        Debug.LogWarning($"MALLOWART: ⏰ Retry-After header: {uwr.GetResponseHeader("retry-after")}");
                        break; // Try next gateway immediately
                    }
                    else if (uwr.responseCode == 500)
                    {
                        Debug.LogError($"MALLOWART: ❌ IPFS Hash {ipfsHash} appears to be corrupted or not available (HTTP 500)");
                        Debug.LogError($"MALLOWART: 🎨 Artwork: {artwork.name}");
                        Debug.LogError($"MALLOWART: 🔗 Mint: {artwork.mintAccount}");
                        break; // Don't retry on 500 errors
                    }
                    else if (uwr.responseCode == 404)
                    {
                        Debug.LogError($"MALLOWART: ❌ IPFS Hash {ipfsHash} not found (HTTP 404)");
                        Debug.LogError($"MALLOWART: 🎨 Artwork: {artwork.name}");
                        Debug.LogError($"MALLOWART: 🔗 Mint: {artwork.mintAccount}");
                        break; // Don't retry on 404 errors
                    }
                    
                    if (attempt < maxRetries)
                    {
                        Debug.Log($"MALLOWART: 🔄 Retrying gateway {gatewayIndex + 1} in 1 second...");
                        yield return new WaitForSeconds(1f);
                    }
                }
            }
            
            // Wait a bit before trying the next gateway
            if (gatewayIndex < ipfsGateways.Length - 1)
            {
                Debug.Log($"MALLOWART: 🔄 Trying next gateway in 2 seconds...");
                yield return new WaitForSeconds(2f);
            }
        }

        Debug.LogError($"MALLOWART: ❌ Image {index} failed on all IPFS gateways");
        Debug.LogError($"MALLOWART: ❌ IPFS Hash: {ipfsHash}");
        Debug.LogError($"MALLOWART: 🎨 Artwork Name: {artwork.name}");
        Debug.LogError($"MALLOWART: 🔗 Mint Account: {artwork.mintAccount}");
        Debug.LogError($"MALLOWART: 🖼️ Original Image URL: {artwork.imageUrl}");
        
        // Create a fallback placeholder texture
        Debug.Log($"MALLOWART: 🎨 Creating fallback placeholder for artwork {index}: {artwork.name}");
        Texture2D placeholderTexture = CreatePlaceholderTexture(artwork.name, ipfsHash);
        ApplyArtworkToPlane(index, artwork, placeholderTexture);
    }

    private Texture2D CreatePlaceholderTexture(string artworkName, string ipfsHash)
    {
        // Create a 512x512 placeholder texture
        int width = 512;
        int height = 512;
        Texture2D texture = new Texture2D(width, height);
        
        // Create a gradient background
        Color[] pixels = new Color[width * height];
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                float normalizedX = (float)x / width;
                float normalizedY = (float)y / height;
                
                // Create a subtle gradient
                Color color = Color.Lerp(
                    new Color(0.1f, 0.1f, 0.2f), // Dark blue
                    new Color(0.2f, 0.2f, 0.3f), // Lighter blue
                    normalizedX + normalizedY
                );
                
                pixels[y * width + x] = color;
            }
        }
        
        texture.SetPixels(pixels);
        texture.Apply();
        
        Debug.Log($"MALLOWART: 🎨 Created placeholder texture for: {artworkName}");
        Debug.Log($"MALLOWART: 🔗 Failed IPFS Hash: {ipfsHash}");
        
        return texture;
    }

    IEnumerator LoadRegularImage(int index, ArtworkItem artwork)
    {
        // Add retry logic for image loading
        int maxRetries = 3;
        for (int attempt = 1; attempt <= maxRetries; attempt++)
        {
            using UnityWebRequest uwr = UnityWebRequestTexture.GetTexture(artwork.imageUrl);
            
            // Add timeout and headers
            uwr.timeout = 30; // 30 second timeout
            uwr.SetRequestHeader("User-Agent", "Unity/2022.3.0f1");
            
            yield return uwr.SendWebRequest();

            if (uwr.result == UnityWebRequest.Result.Success)
            {
                Debug.Log($"MALLOWART: ✅ Image {index} loaded successfully on attempt {attempt}");
                Texture2D texture = DownloadHandlerTexture.GetContent(uwr);
                
                if (texture != null)
                {
                    ApplyArtworkToPlane(index, artwork, texture);
                }
                else
                {
                    Debug.LogError($"MALLOWART: ❌ Image {index} texture is null after successful download");
                }
                yield break; // Success, exit retry loop
            }
            else
            {
                Debug.LogWarning($"MALLOWART: ⚠️ Image {index} attempt {attempt}/{maxRetries} failed: {uwr.error} (Response Code: {uwr.responseCode})");
                
                if (attempt < maxRetries)
                {
                    Debug.Log($"MALLOWART: 🔄 Retrying image {index} in 2 seconds...");
                    yield return new WaitForSeconds(2f);
                }
                else
                {
                    Debug.LogError($"MALLOWART: ❌ Image {index} failed after {maxRetries} attempts: {uwr.error}");
                    Debug.LogError($"MALLOWART: ❌ URL: {artwork.imageUrl}");
                    Debug.LogError($"MALLOWART: ❌ Response Code: {uwr.responseCode}");
                    Debug.LogError($"MALLOWART: ❌ Response Headers: {uwr.GetResponseHeader("content-type")}");
                }
            }
        }
    }

    private void ApplyArtworkToPlane(int index, ArtworkItem artwork, Texture2D texture)
    {
        Debug.Log($"MALLOWART: 🎨 ApplyArtworkToPlane called for index {index}");
        
        if (index >= planeObjects.Count)
        {
            Debug.LogError($"MALLOWART: ❌ Index {index} is out of range for planeObjects (count: {planeObjects.Count})");
            return;
        }
        
        if (planeObjects[index] == null)
        {
            Debug.LogError($"MALLOWART: ❌ Plane object at index {index} is null");
            return;
        }
        
        if (texture == null)
        {
            Debug.LogError($"MALLOWART: ❌ Texture is null for index {index}");
            return;
        }
        
        Debug.Log($"MALLOWART: 🖼️ Applying texture to plane {index}");
        planeObjects[index].GetComponent<Renderer>().material.mainTexture = texture;
        
        if (nameTexts.Count > index && nameTexts[index] != null)
        {
            nameTexts[index].text = artwork.name ?? "Unnamed";
            Debug.Log($"MALLOWART: 📝 Set name text for index {index}: {artwork.name}");
        }

        if (DateTime.TryParse(artwork.auctionMetadata?.endsAt, out DateTime endTime))
            auctionEndTimes.Add(endTime.ToUniversalTime());
        else
            auctionEndTimes.Add(DateTime.UtcNow);

        if (bidTexts.Count > index && bidTexts[index] != null && artwork.auctionMetadata != null)
        {
            float minBidSOL = CalculateMinBid(artwork);
            bidTexts[index].text = $" {minBidSOL:F2} SOL";
            Debug.Log($"MALLOWART: 💰 Set bid text for index {index}: {minBidSOL:F2} SOL");
        }

        // ✅ Inject data into ArtworkInteractable if needed
        ArtworkInteractable interactable = planeObjects[index].GetComponent<ArtworkInteractable>();
        if (interactable != null)
        {
            interactable.Title = artwork.name;
            interactable.Description = artwork.description ?? "No description";
            interactable.TimeRemaining = GetTimeRemaining(artwork.auctionMetadata?.endsAt);
            interactable.MinBidSOL = CalculateMinBid(artwork);
            interactable.PreviousBids = artwork.auctionMetadata?.bidders;
            interactable.ArtistName = DefaultArtistName;
            interactable.ArtworkTexture = texture;
            interactable.MintAddress = artwork.mintAccount;
            interactable.AuctionAddress = artwork.auctionHouse;
            Debug.Log($"MALLOWART: 🔗 Updated ArtworkInteractable for index {index}");
        }
        
        Debug.Log($"MALLOWART: ✅ ApplyArtworkToPlane completed for index {index}");
    }

    private float CalculateMinBid(ArtworkItem artwork)
    {
        if (artwork.auctionMetadata == null)
            return 0f;

        long current = artwork.auctionMetadata.currentBidAmount;
        int bps = artwork.auctionMetadata.minBidIncrementBps;
        long minBid = current + (current * bps / 10_000);
        return minBid / 1_000_000_000f;
    }

    private string GetTimeRemaining(string endsAt)
    {
        if (DateTime.TryParse(endsAt, out DateTime endTime))
        {
            TimeSpan remaining = endTime.ToUniversalTime() - DateTime.UtcNow;
            if (remaining.TotalSeconds <= 0)
                return "Ended";
            return $"{remaining.Days}d {remaining.Hours:D2}h:{remaining.Minutes:D2}m:{remaining.Seconds:D2}s";
        }
        return "Unknown";
    }

    void UpdateCountdownTimers()
    {
        for (int i = 0; i < auctionEndTimes.Count && i < timerTexts.Count; i++)
        {
            if (timerTexts[i] == null) continue;

            TimeSpan remaining = auctionEndTimes[i] - DateTime.UtcNow;
            if (remaining.TotalSeconds <= 0)
            {
                timerTexts[i].text = "Ended";
            }
            else
            {
                timerTexts[i].text = $"Ends in: {remaining.Days}d {remaining.Hours:D2}h:{remaining.Minutes:D2}m:{remaining.Seconds:D2}s";
            }
        }
    }
}
