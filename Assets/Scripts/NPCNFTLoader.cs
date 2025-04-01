using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using Newtonsoft.Json.Linq;
using TMPro;
using System.Threading.Tasks;
using System.Text;

public class NPCNFTLoader : MonoBehaviour
{
    [Header("NPC Meshes")]
    public MeshRenderer[] nftMeshes;

    [Header("NPC Images")]
    public RawImage[] nftImagesUi;

 //   [Header("NPC ImagesForWager")]
//    public RawImage[] nftImagesWager;

    [Header("NPC Wallet Address")]
    public string npcWalletAddress;

    [Header("Helius API Key")]
    public string heliusApiKey; // ✅ Set this in Unity Inspector

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
        rpcUrl = useMainnet
            ? $"https://mainnet.helius-rpc.com/?api-key={heliusApiKey}"
            : $"https://devnet.helius-rpc.com/?api-key={heliusApiKey}"; // ✅ Uses Helius API

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
            if (nftIndex >= nftImagesUi.Length) break; // Stop if all slots are filled

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
            Debug.Log($"✅ Helius Metadata Response: {responseText}");
            await ProcessNFTMetadata(responseText, mintAddress);
        }
        else
        {
            Debug.LogError("❌ Failed to retrieve NFT metadata from Helius.");
        }
    }

    private async Task ProcessNFTMetadata(string metadataJson, string mintAddress)
    {
        var parsedResponse = JObject.Parse(metadataJson);
        if (parsedResponse["error"] != null)
        {
            Debug.LogError($"❌ Helius Error: {parsedResponse["error"]["message"]}");
            return;
        }

        string metadataUri = parsedResponse["result"]?["content"]?["json_uri"]?.ToString();
        if (string.IsNullOrEmpty(metadataUri))
        {
            Debug.LogError($"❌ Metadata URI missing. Trying Metaplex...");
            return;
        }

        Debug.Log($"✅ Metadata URI Found: {metadataUri}");
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
        string nftName = metadata["name"]?.ToString();
        string imageUrl = metadata["image"]?.ToString();

        if (!string.IsNullOrEmpty(imageUrl))
        {
            await DownloadAndDisplayNFT(imageUrl, nftName, mintAddress);
        }
        else
        {
            Debug.LogError($"❌ Image URL missing in metadata: {metadataUrl}");
        }
    }

    private async Task DownloadAndDisplayNFT(string imageUrl, string nftName, string nftAddress)
    {
        if (nftIndex >= nftImagesUi.Length || nftIndex >= nftMeshes.Length) return;

        if (imageCache.TryGetValue(imageUrl, out Texture2D cachedTexture))
        {
            AssignNFTVisuals(nftIndex, cachedTexture, nftName, nftAddress);
            return;
        }

        Texture2D texture = await DownloadTextureAsync(imageUrl);
        if (texture != null)
        {
            imageCache[imageUrl] = texture;
            AssignNFTVisuals(nftIndex, texture, nftName, nftAddress);
        }
        else
        {
            Debug.LogError($"❌ Failed to download NFT image: {imageUrl}");
        }
    }

    private void AssignNFTVisuals(int index, Texture2D texture, string nftName, string nftAddress)
    {
        nftImagesUi[index].texture = texture;
        nftMeshes[index].material.mainTexture = texture;
        nftImagesUi[index].name = nftName;
        nftMeshes[index].name = nftName;

        SaveMetadata(nftAddress, nftName);
        nftIndex++;
    }

    private void SaveMetadata(string nftAddress, string nftName)
    {
        Debug.Log($"Saving Metadata: {nftName} for NFT Address: {nftAddress}");

        NFTMetadataHolder metadataHolder = nftMeshes[nftIndex].GetComponent<NFTMetadataHolder>();
        if (metadataHolder == null)
        {
            Debug.LogWarning($"❌ NFTMetadataHolder not found on {nftMeshes[nftIndex].name}");
            return;
        }

        metadataHolder.nftName = nftName;
        metadataHolder.mintAddress = nftAddress;
        Debug.Log($"✅ Metadata saved in {metadataHolder.gameObject.name}: {metadataHolder.nftName}");
    }

    private async Task<Texture2D> DownloadTextureAsync(string imageUrl)
    {
        using UnityWebRequest request = UnityWebRequestTexture.GetTexture(imageUrl);
        await SendUnityWebRequest(request);
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

        await SendUnityWebRequest(request);
        return request.result == UnityWebRequest.Result.Success ? request.downloadHandler.text : null;
    }

    private async Task SendUnityWebRequest(UnityWebRequest request)
    {
        var asyncOperation = request.SendWebRequest();
        while (!asyncOperation.isDone) await Task.Yield();
    }
}
