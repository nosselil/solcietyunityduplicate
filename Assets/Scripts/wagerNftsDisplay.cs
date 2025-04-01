using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;
using System.Linq;
using System.Text;

public class wagerNftsDisplay : MonoBehaviour
{
    public RawImage[] nftImageSlots; // UI image slots
    public Renderer[] wallRenderers; // Mesh renderers

    public string npcWalletAddress = "6tyYVbCJ2Ru7gkeVF6qefvk5szvL6HdwxBpLquEzRqPg"; // NPC Wallet Address
    private string rpcUrl = "https://api.mainnet-beta.solana.com"; // Updated to devnet Solana RPC

    private Dictionary<string, Texture2D> imageCache = new Dictionary<string, Texture2D>();
    private Dictionary<string, string> metadataCache = new Dictionary<string, string>();
    private Dictionary<string, string> meshMetadata = new Dictionary<string, string>(); // Stores metadata per mesh

    private async void Start()
    {
        await FetchAndDisplayNFTs();
    }

    private async Task FetchAndDisplayNFTs()
    {
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
            await ProcessNFTResponse(responseText);
        }
    }

    private async Task ProcessNFTResponse(string jsonResponse)
    {
        JObject parsedResponse = JObject.Parse(jsonResponse);
        JArray tokenAccounts = (JArray)parsedResponse["result"]["value"];
        List<string> mintAddresses = new List<string>();

        foreach (var tokenAccount in tokenAccounts)
        {
            var tokenInfo = tokenAccount["account"]["data"]["parsed"]["info"];
            string mintAddress = tokenInfo["mint"].ToString();
            int decimals = int.Parse(tokenInfo["tokenAmount"]["decimals"].ToString());
            int supply = int.Parse(tokenInfo["tokenAmount"]["amount"].ToString());

            if (decimals == 0 && supply == 1)
            {
                mintAddresses.Add(mintAddress);
            }
        }

        await FetchAndDisplayNFTImages(mintAddresses);
    }

    private async Task FetchAndDisplayNFTImages(List<string> mintAddresses)
    {
        List<Task> fetchTasks = new List<Task>();

        for (int i = 0; i < mintAddresses.Count && i < nftImageSlots.Length; i++)
        {
            string mintAddress = mintAddresses[i];
            fetchTasks.Add(FetchAndDisplayNFTForAddress(mintAddress, i));
        }

        await Task.WhenAll(fetchTasks);
    }

    private async Task FetchAndDisplayNFTForAddress(string mintAddress, int index)
    {
        if (!metadataCache.ContainsKey(mintAddress))
        {
            string jsonRpcRequest = $@"{{
            ""jsonrpc"": ""2.0"",
            ""id"": 1,
            ""method"": ""getAsset"",
            ""params"": [""{mintAddress}""] 
        }}";

            string responseText = await SendWebRequestAsync(rpcUrl, jsonRpcRequest);
            if (responseText != null)
            {
                JObject metadata = JObject.Parse(responseText);
                string metadataUri = metadata["result"]?["content"]?["json_uri"]?.ToString();

                if (!string.IsNullOrEmpty(metadataUri))
                {
                    metadataCache[mintAddress] = metadataUri;
                }
            }
        }

        if (metadataCache.ContainsKey(mintAddress))
        {
            string metadataUri = metadataCache[mintAddress];
            await FetchExternalNFTMetadata(metadataUri, index, mintAddress);
        }
    }

    private async Task FetchExternalNFTMetadata(string metadataUri, int index, string mintAddress)
    {
        string responseText = await SendWebRequestAsync(metadataUri, null);
        if (responseText != null)
        {
            JObject externalMetadata = JObject.Parse(responseText);
            string imageUrl = externalMetadata["image"]?.ToString();
            string nftMetadata = externalMetadata.ToString(); // Store full metadata

            if (!string.IsNullOrEmpty(imageUrl))
            {
                await DownloadAndDisplayNFT(imageUrl, index, nftMetadata, mintAddress);  // Pass mintAddress
            }
            else
            {
                Debug.LogError($"Image URL missing in external metadata: {responseText}");
            }
        }
    }

    private async Task DownloadAndDisplayNFT(string imageUrl, int index, string nftMetadata, string mintAddress)
    {
        if (index >= nftImageSlots.Length) return;

        // Check if the image is already in the cache
        if (imageCache.ContainsKey(imageUrl))
        {
            ApplyTexture(imageCache[imageUrl], index, nftMetadata, mintAddress);
            return;
        }

        // If not in cache, download and cache it
        Texture2D texture = await DownloadTextureAsync(imageUrl);
        if (texture != null)
        {
            imageCache[imageUrl] = texture;  // Cache the downloaded texture
            ApplyTexture(texture, index, nftMetadata, mintAddress);
        }
    }


    private void ApplyTexture(Texture2D texture, int index, string nftMetadata, string mintAddress)
    {
        Debug.Log($"Applying texture to slot {index} with mintAddress: {mintAddress}");

        // Ensure NFTAddress component is added first if it's missing
        NFTAddress addressHolder = nftImageSlots[index].GetComponent<NFTAddress>();
        if (addressHolder == null)
        {
            addressHolder = nftImageSlots[index].gameObject.AddComponent<NFTAddress>();
            Debug.Log($"NFTAddress component added to {nftImageSlots[index].gameObject.name}");
        }

        // Set the NFT address
        addressHolder.nftAddress = mintAddress;
        Debug.Log($"Assigned NFT Address {mintAddress} to {nftImageSlots[index].gameObject.name}");

        // Assign texture to RawImage
        nftImageSlots[index].texture = texture;
        nftImageSlots[index].gameObject.name = JObject.Parse(nftMetadata)["name"]?.ToString() ?? "UnknownNFT";

        // Optionally, update the wall renderers if available
        if (index < wallRenderers.Length && wallRenderers[index] != null)
        {
            wallRenderers[index].material.mainTexture = texture;
            wallRenderers[index].gameObject.tag = $"NFT_{index + 1}";

            var metadataHolder = wallRenderers[index].gameObject.GetComponent<NFTMetadataHolder>()
                ?? wallRenderers[index].gameObject.AddComponent<NFTMetadataHolder>();
          //  metadataHolder.metadata = nftMetadata;
            metadataHolder.mintAddress = mintAddress;
            wallRenderers[index].gameObject.name = nftImageSlots[index].gameObject.name;
        }
    }
    private async Task<string> SendWebRequestAsync(string url, string jsonRpcRequest)
    {
        using UnityWebRequest request = new(url, "POST")
        {
            uploadHandler = jsonRpcRequest != null ? new UploadHandlerRaw(Encoding.UTF8.GetBytes(jsonRpcRequest)) : null,
            downloadHandler = new DownloadHandlerBuffer()
        };
        request.SetRequestHeader("Content-Type", "application/json");

        await request.SendWebRequest();
        return request.result == UnityWebRequest.Result.Success ? request.downloadHandler.text : null;
    }

    private async Task<Texture2D> DownloadTextureAsync(string imageUrl)
    {
        if (imageCache.TryGetValue(imageUrl, out Texture2D cachedTexture)) return cachedTexture;
        using UnityWebRequest request = UnityWebRequestTexture.GetTexture(imageUrl);
        await request.SendWebRequest();
        return request.result == UnityWebRequest.Result.Success ? DownloadHandlerTexture.GetContent(request) : null;
    }
}