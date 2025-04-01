using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using Newtonsoft.Json.Linq;
using TMPro;
using System.Threading.Tasks;
using System.Text;

public class NPCNFTLoaderWager : MonoBehaviour
{
    [Header("NPC Meshes")]
    public MeshRenderer[] nftMeshes;
    [Header("NPC Images")]
    public RawImage[] nftImages;  // Array to store multiple NFT images
   
    [Header("NPC Wallet Address")]
    public string npcWalletAddress;

    [Header("Network Selection")]
    public bool useMainnet = true;

    private string rpcUrl;
    private readonly Dictionary<string, Texture2D> imageCache = new();
    private int nftIndex = 0; // Tracks the current index for assignment

    private const string TokenProgramId = "TokenkegQfeZyiNwAJbNbGKPFXCWuBvf9Ss623VQ5DA";
    private const string RpcGetTokenAccountsByOwner = "getTokenAccountsByOwner";
    private const string RpcGetAsset = "getAsset";

    private async void Start()
    {
        rpcUrl = useMainnet ? "https://api.mainnet-beta.solana.com" : "https://api.devnet.solana.com";
        Debug.Log($"Fetching NFTs from: {(useMainnet ? "Mainnet" : "Devnet")}");
        await FetchNPCNFTs();
    }

    private async Task FetchNPCNFTs()
    {
        if (string.IsNullOrEmpty(npcWalletAddress))
        {
            Debug.LogError("NPC wallet address is empty. Please set it in the Inspector.");
            return;
        }

        string jsonRpcRequest = $@"{{
            ""jsonrpc"": ""2.0"", ""id"": 1, ""method"": ""{RpcGetTokenAccountsByOwner}"",
            ""params"": [""{npcWalletAddress}"", {{""programId"": ""{TokenProgramId}""}}, {{""encoding"": ""jsonParsed""}}]
        }}";

        string responseText = await SendWebRequestAsync(rpcUrl, jsonRpcRequest);
        if (!string.IsNullOrEmpty(responseText))
        {
            Debug.Log($"NFT Data: {responseText}");
            ProcessNFTResponse(responseText);
        }
        else
        {
            Debug.LogError("Failed to retrieve token accounts.");
        }
    }

    private void ProcessNFTResponse(string jsonResponse)
    {
        var parsedResponse = JObject.Parse(jsonResponse);
        var tokenAccounts = parsedResponse?["result"]?["value"] as JArray;

        if (tokenAccounts == null || tokenAccounts.Count == 0)
        {
            Debug.LogError("No NFTs found in the NPC wallet.");
            return;
        }

        foreach (var tokenAccount in tokenAccounts)
        {
            if (nftIndex >= nftImages.Length) break; // Stop if all slots are filled

            var tokenInfo = tokenAccount["account"]["data"]["parsed"]["info"];
            string mintAddress = tokenInfo["mint"].ToString();
            int decimals = (int)tokenInfo["tokenAmount"]["decimals"];
            int supply = (int)tokenInfo["tokenAmount"]["amount"];

            if (decimals == 0 && supply == 1)
            {
                Debug.Log($"✅ Valid NFT Mint Address: {mintAddress}");
                FetchNFTMetadata(mintAddress);
            }
        }
    }

    private async Task FetchNFTMetadata(string mintAddress)
    {
        string jsonRpcRequest = $@"{{
            ""jsonrpc"": ""2.0"", ""id"": 1, ""method"": ""{RpcGetAsset}"",
            ""params"": [""{mintAddress}""]
        }}";

        string responseText = await SendWebRequestAsync(rpcUrl, jsonRpcRequest);
        if (!string.IsNullOrEmpty(responseText))
        {
            Debug.Log($"Metadata Response: {responseText}");
            await ProcessNFTMetadata(responseText, mintAddress);
        }
    }

    private async Task ProcessNFTMetadata(string metadataJson, string mintAddress)
    {
        var metadata = JObject.Parse(metadataJson);
        string metadataUri = metadata["result"]?["content"]?["json_uri"]?.ToString();

        if (string.IsNullOrEmpty(metadataUri))
        {
            Debug.LogError($"❌ Metadata URI not found: {metadataJson}");
            return;
        }

        Debug.Log($"🔍 Fetching external metadata from: {metadataUri}");
        await FetchExternalNFTMetadata(metadataUri, mintAddress);
    }


    private async Task FetchExternalNFTMetadata(string metadataUrl, string mintAddress)
    {
        using UnityWebRequest request = UnityWebRequest.Get(metadataUrl);
        request.SetRequestHeader("Content-Type", "application/json");

        await SendUnityWebRequest(request);

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError($"❌ Failed to fetch metadata: {request.error}");
            return;
        }

        var metadata = JObject.Parse(request.downloadHandler.text);
        string imageUrl = metadata["image"]?.ToString();
        string nftName = metadata["name"]?.ToString();

        if (!string.IsNullOrEmpty(imageUrl))
        {
            await DownloadAndDisplayNFT(imageUrl, nftName, mintAddress);  // Now mintAddress is available
        }
        else
        {
            Debug.LogError($"❌ Image URL missing in metadata: {metadataUrl}");
        }
    }


    private async Task DownloadAndDisplayNFT(string imageUrl, string nftName, string nftAddress)
    {
        if (nftIndex >= nftImages.Length || nftIndex >= nftMeshes.Length) return; // Avoid overflow if no more slots are available

        if (imageCache.TryGetValue(imageUrl, out Texture2D cachedTexture))
        {
            // Assign texture to both RawImage and MeshRenderer
            nftImages[nftIndex].texture = cachedTexture;
            nftMeshes[nftIndex].material.mainTexture = cachedTexture;

            // Assign the NFT address to the NFTAddress script for RawImage
            AssignNFTAddress(nftImages[nftIndex], nftAddress);

            nftIndex++; // Move to next slot
            return;
        }

        Texture2D texture = await DownloadTextureAsync(imageUrl);
        if (texture != null)
        {
            imageCache[imageUrl] = texture;

            // Assign texture to both RawImage and MeshRenderer
            nftImages[nftIndex].texture = texture;
            nftMeshes[nftIndex].material.mainTexture = texture;

            // Assign the NFT address to the NFTAddress script for RawImage
            AssignNFTAddress(nftImages[nftIndex], nftAddress);

            Debug.Log($"🎨 NFT {nftIndex + 1} Image Displayed: {nftName}");
            nftIndex++; // Move to the next available slot
        }
        else
        {
            Debug.LogError($"❌ Failed to download NFT image: {imageUrl}");
        }
    }

    private void AssignNFTAddress(RawImage rawImage, string nftAddress)
    {
        NFTAddress nftAddressComponent = rawImage.GetComponent<NFTAddress>();
        if (nftAddressComponent != null)
        {
            nftAddressComponent.nftAddress = nftAddress;
            Debug.Log($"✅ Assigned NFT Address: {nftAddress} to {rawImage.name}");
        }
        else
        {
            Debug.LogWarning($"⚠️ NFTAddress script missing on {rawImage.name}");
        }
    }


    private async Task<Texture2D> DownloadTextureAsync(string imageUrl)
    {
        using UnityWebRequest request = UnityWebRequestTexture.GetTexture(imageUrl);
        await SendUnityWebRequest(request); // ✅ FIXED

        return request.result == UnityWebRequest.Result.Success ? DownloadHandlerTexture.GetContent(request) : null;
    }

    private async Task<string> SendWebRequestAsync(string url, string jsonRpcRequest)
    {
        using UnityWebRequest request = new UnityWebRequest(url, "POST")
        {
            uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(jsonRpcRequest)),
            downloadHandler = new DownloadHandlerBuffer()
        };
        request.SetRequestHeader("Content-Type", "application/json");

        await SendUnityWebRequest(request); // ✅ FIXED

        return request.result == UnityWebRequest.Result.Success ? request.downloadHandler.text : null;
    }

    private async Task SendUnityWebRequest(UnityWebRequest request)
    {
        var asyncOperation = request.SendWebRequest();

        while (!asyncOperation.isDone)
        {
            await Task.Yield(); // ✅ FIXED: Proper async handling
        }
    }
}
