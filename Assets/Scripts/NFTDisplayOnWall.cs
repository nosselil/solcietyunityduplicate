using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;
using System.Linq;
using System.Text;

public class NFTDisplayOnWall : MonoBehaviour
{
    public Renderer[] nftRenderers; // Array of GameObjects' renderers to apply textures
    public string npcWalletAddress = "GvekSRZc6XCX1TxF96QHhngfKFE67zwobo23K9Npo5ke"; // NPC Wallet Address
    private string rpcUrl = "https://mainnet.helius-rpc.com/?api-key=0a682a0d-9417-48e9-b5e2-dca209af89eb"; // Solana RPC URL

    private Dictionary<string, Texture2D> imageCache = new Dictionary<string, Texture2D>(); // Image cache
    private HashSet<string> displayedMintAddresses = new HashSet<string>(); // Track displayed mint addresses
    private int currentNFTIndex = 0; // Keep track of which NFT is currently displayed

    private async void Start()
    {
        await FetchNPCNFTs();
    }

    private async Task FetchNPCNFTs()
    {

        Debug.LogError("Calling FetchNPCNFTs from: NFTDisplayOnWall" + gameObject.name);
        string jsonRpcRequest = $@"{{
            ""jsonrpc"": ""2.0"",
            ""id"": 1,
            ""method"": ""getTokenAccountsByOwner"",
            ""params"": [
                ""{npcWalletAddress}"",
                {{""programId"": ""TokenkegQfeZyiNwAJbNbGKPFXCWuBvf9Ss623VQ5DA""}},
                {{""encoding"": ""jsonParsed""}}
            ]
        }}";

        string responseText = await SendWebRequestAsync(rpcUrl, jsonRpcRequest);
        if (responseText != null)
        {
            Debug.Log($"NFT Data: {responseText}");
            ProcessNFTResponse(responseText);
        }
    }

    private void ProcessNFTResponse(string jsonResponse)
    {
        JObject parsedResponse = JObject.Parse(jsonResponse);
        JArray tokenAccounts = (JArray)parsedResponse["result"]["value"];

        // Start the process of displaying NFTs on GameObjects
        StartCoroutine(DisplayNFTsSequentially(tokenAccounts));
    }

    private IEnumerator DisplayNFTsSequentially(JArray tokenAccounts)
    {
        foreach (var tokenAccount in tokenAccounts)
        {
            if (currentNFTIndex >= nftRenderers.Length) yield break; // Stop if out of GameObject slots

            var tokenInfo = tokenAccount["account"]["data"]["parsed"]["info"];
            string mintAddress = tokenInfo["mint"].ToString();
            int decimals = int.Parse(tokenInfo["tokenAmount"]["decimals"].ToString());
            int supply = int.Parse(tokenInfo["tokenAmount"]["amount"].ToString());

            if (decimals == 0 && supply == 1 && !displayedMintAddresses.Contains(mintAddress)) // Only NFTs
            {
                Debug.Log($"Valid NFT Mint Address: {mintAddress}");
                displayedMintAddresses.Add(mintAddress); // Mark this mint address as displayed
                yield return FetchAndDisplayNFT(mintAddress, currentNFTIndex);
                currentNFTIndex++; // Increment to load the next NFT after this one is done
            }
        }
    }

    private async Task FetchAndDisplayNFT(string mintAddress, int index)
    {
        string jsonRpcRequest = $@"{{
            ""jsonrpc"": ""2.0"",
            ""id"": 1,
            ""method"": ""getAsset"",
            ""params"": [
                ""{mintAddress}""
            ]
        }}";

        string responseText = await SendWebRequestAsync(rpcUrl, jsonRpcRequest);
        if (responseText != null)
        {
            Debug.Log($"Metadata Response: {responseText}");
            ProcessNFTMetadata(responseText, index);
        }
    }

    private async void ProcessNFTMetadata(string metadataJson, int index)
    {
        JObject metadata = JObject.Parse(metadataJson);
        string metadataUri = metadata["result"]?["content"]?["json_uri"]?.ToString();

        if (string.IsNullOrEmpty(metadataUri))
        {
            Debug.LogError($"Metadata URI not found in the NFT data: {metadataJson}");
            return;
        }

        Debug.Log($"Fetching external metadata from: {metadataUri}");
        await FetchExternalNFTMetadata(metadataUri, index);
    }

    private async Task FetchExternalNFTMetadata(string metadataUri, int index)
    {
        string responseText = await SendWebRequestAsync(metadataUri, null);
        if (responseText != null)
        {
            JObject externalMetadata = JObject.Parse(responseText);

            string imageUrl = externalMetadata["image"]?.ToString();

            if (!string.IsNullOrEmpty(imageUrl))
            {
                await DownloadAndDisplayNFT(imageUrl, index);
            }
            else
            {
                Debug.LogError($"Image URL missing in external metadata: {responseText}");
            }
        }
    }

    private async Task DownloadAndDisplayNFT(string imageUrl, int index)
    {
        if (index >= nftRenderers.Length) return; // Ensure index is within bounds

        if (imageCache.ContainsKey(imageUrl))
        {
            ApplyTextureToGameObject(nftRenderers[index], imageCache[imageUrl]);
            return;
        }

        Texture2D texture = await DownloadTextureAsync(imageUrl);
        if (texture != null)
        {
            imageCache[imageUrl] = texture; // Cache the image
            ApplyTextureToGameObject(nftRenderers[index], texture);
        }
    }

    // Apply the downloaded texture to the GameObject's material
    private void ApplyTextureToGameObject(Renderer renderer, Texture2D texture)
    {
        if (renderer != null)
        {
            renderer.material.mainTexture = texture;
        }
    }

    // Helper function to send async web requests
    private async Task<string> SendWebRequestAsync(string url, string jsonRpcRequest)
    {
        using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
        {
            if (jsonRpcRequest != null)
            {
                byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonRpcRequest);
                request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            }
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            var asyncOp = request.SendWebRequest();
            while (!asyncOp.isDone)
            {
                await Task.Yield();
            }

            if (request.result == UnityWebRequest.Result.Success)
            {
                return request.downloadHandler.text;
            }
            else
            {
                Debug.LogError($"Request failed: {request.error}");
                return null;
            }
        }
    }

    // Helper function to download textures faster
    private async Task<Texture2D> DownloadTextureAsync(string imageUrl)
    {
        using (UnityWebRequest request = UnityWebRequestTexture.GetTexture(imageUrl))
        {
            var asyncOp = request.SendWebRequest();
            while (!asyncOp.isDone)
            {
                await Task.Yield();
            }

            if (request.result == UnityWebRequest.Result.Success)
            {
                return DownloadHandlerTexture.GetContent(request);
            }
            else
            {
                Debug.LogError($"Failed to download image: {request.error}");
                return null;
            }
        }
    }
}