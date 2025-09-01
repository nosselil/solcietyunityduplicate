using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;
using System.Linq;
using System.Text;

public class NFTDisplayNPC : MonoBehaviour
{
    public RawImage[] nftImageSlots; // UI image slots
    public Renderer[] wallRenderers; // Mesh renderers

    public string npcWalletAddress = "6tyYVbCJ2Ru7gkeVF6qefvk5szvL6HdwxBpLquEzRqPg"; // NPC Wallet Address
    private string rpcUrl = "https://mainnet.helius-rpc.com/?api-key=0a682a0d-9417-48e9-b5e2-dca209af89eb"; // Updated to devnet Solana RPC

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
        // Apply the texture to the RawImage
        nftImageSlots[index].texture = texture;

        // Rename the RawImage game object based on the NFT name
        try
        {
            JObject parsedMetadata = JObject.Parse(nftMetadata);
            string nftName = parsedMetadata["name"]?.ToString();

            if (!string.IsNullOrEmpty(nftName))
            {
                nftImageSlots[index].gameObject.name = nftName; // Rename RawImage GameObject
                Debug.Log($"Image renamed to NFT name: {nftName}");
            }
            else
            {
                Debug.LogWarning("NFT name missing in metadata for image.");
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Error parsing NFT metadata for image renaming: {ex.Message}");
        }

        // If there are mesh renderers, apply the texture to them too
        if (index < wallRenderers.Length && wallRenderers[index] != null)
        {
            wallRenderers[index].material.mainTexture = texture;

            // Assign tag dynamically
            string tagName = $"NFT_{index + 1}";
            wallRenderers[index].gameObject.tag = tagName;

            // Store metadata on the GameObject
            NFTMetadataHolder metadataHolder = wallRenderers[index].gameObject.GetComponent<NFTMetadataHolder>();
            if (metadataHolder == null)
            {
                metadataHolder = wallRenderers[index].gameObject.AddComponent<NFTMetadataHolder>();
            }

         //   metadataHolder.metadata = nftMetadata; // Store the full metadata
            metadataHolder.mintAddress = mintAddress; // Store the mint address

            // Extract NFT name from metadata and set it as the mesh name
            try
            {
                JObject parsedMetadata = JObject.Parse(nftMetadata);
                string nftName = parsedMetadata["name"]?.ToString();

                if (!string.IsNullOrEmpty(nftName))
                {
                    wallRenderers[index].gameObject.name = nftName; // Rename GameObject
                    Debug.Log($"Mesh renamed to NFT name: {nftName}");
                }
                else
                {
                    Debug.LogWarning("NFT name missing in metadata.");
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Error parsing NFT metadata: {ex.Message}");
            }
        }
        else
        {
            // If no mesh renderers exist, we will apply the texture to the RawImage only
            if (nftImageSlots[index] != null)
            {
                nftImageSlots[index].texture = texture;
            }
        }
    }



    public void DebugNFTMetadata(GameObject triggeredObject)
    {
        string objectTag = triggeredObject.tag;

        if (meshMetadata.ContainsKey(objectTag))
        {
            Debug.Log($"NFT Metadata for {objectTag}: {meshMetadata[objectTag]}");
        }
        else
        {
            Debug.Log("No metadata found for this object.");
        }
    }

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

    private async Task<Texture2D> DownloadTextureAsync(string imageUrl)
    {
        // Check if the texture is already in the cache
        if (imageCache.ContainsKey(imageUrl))
        {
            return imageCache[imageUrl];
        }

        using (UnityWebRequest request = UnityWebRequestTexture.GetTexture(imageUrl))
        {
            var asyncOp = request.SendWebRequest();
            while (!asyncOp.isDone)
            {
                await Task.Yield();
            }

            if (request.result == UnityWebRequest.Result.Success)
            {
                Texture2D texture = DownloadHandlerTexture.GetContent(request);
                // Cache the texture after downloading it
                imageCache[imageUrl] = texture;
                return texture;
            }
            else
            {
                Debug.LogError($"Failed to download image: {request.error}");
                return null;
            }
        }
    }

}