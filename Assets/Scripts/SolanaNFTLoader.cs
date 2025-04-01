using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Solana.Unity.Rpc;
using Solana.Unity.Rpc.Types;
using Solana.Unity.Rpc.Models;
using TMPro;
using Newtonsoft.Json.Linq;
using UnityEngine.Networking;
using System.Threading.Tasks;
using Solana.Unity.Programs;
using Solana.Unity.Metaplex.NFT.Library;
using Solana.Unity.Metaplex;
using Solana.Unity.Metaplex.NFT;

public class SolanaNFTLoader : MonoBehaviour
{
    public string walletAddress; // Set the user's Solana wallet address
    public GameObject nftPrefab; // UI prefab to display NFT
    public Transform nftContainer; // UI Parent Container

    private IRpcClient rpcClient; // Solana RPC Client

    private void Start()
    {
        // Initialize the RPC client
        rpcClient = ClientFactory.GetClient(Cluster.DevNet); // Change to DevNet if needed

        // Fetch and display NFTs
        StartCoroutine(FetchAndDisplayNFTs());
    }

    private IEnumerator FetchAndDisplayNFTs()
    {
        if (string.IsNullOrEmpty(walletAddress))
        {
            Debug.LogError("Wallet address is empty!");
            yield break;
        }

        var task = rpcClient.GetTokenAccountsByOwnerAsync(walletAddress, TokenProgram.ProgramIdKey);
        yield return new WaitUntil(() => task.IsCompleted);

        if (task.Result != null)
        {
            Debug.Log("RPC Response: " + task.Result.RawRpcResponse); // Log full response
        }

        if (task.Result == null || !task.Result.WasSuccessful || task.Result.Result == null)
        {
            Debug.LogError("Failed to fetch NFT data. Error: " + task.Result?.HttpStatusCode);
            yield break;
        }

        foreach (var token in task.Result.Result.Value)
        {
            string mintAddress = token.Account.Data.Parsed.Info.Mint;
            Debug.Log($"Token Mint Found: {mintAddress}");

            if (!string.IsNullOrEmpty(mintAddress) && mintAddress.Length == 44)
            {
                StartCoroutine(DownloadAndDisplayNFT(mintAddress));
            }
            else
            {
                Debug.LogError($"Invalid mint address detected: {mintAddress}");
            }
        }
    }

    private IEnumerator DownloadAndDisplayNFT(string mintAddress)
    {
        Task<string> metadataTask = GetNFTMetadata(mintAddress);
        yield return new WaitUntil(() => metadataTask.IsCompleted);

        string metadataJson = metadataTask.Result;
        if (string.IsNullOrEmpty(metadataJson))
        {
            Debug.LogError($"Failed to fetch NFT metadata for {mintAddress}.");
            yield break;
        }

        JObject metadata = JObject.Parse(metadataJson);
        string imageUrl = metadata["image"]?.ToString();
        string name = metadata["name"]?.ToString();

        if (!string.IsNullOrEmpty(imageUrl))
        {
            StartCoroutine(DownloadImage(imageUrl, name));
        }
    }

    private IEnumerator DownloadImage(string imageUrl, string nftName)
    {
        using (UnityWebRequest request = UnityWebRequestTexture.GetTexture(imageUrl))
        {
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                Texture2D texture = DownloadHandlerTexture.GetContent(request);
                DisplayNFT(texture, nftName);
            }
            else
            {
                Debug.LogError($"Image download failed: {request.error}");
            }
        }
    }

    private void DisplayNFT(Texture2D texture, string nftName)
    {
        GameObject nftUI = Instantiate(nftPrefab, nftContainer);
        nftUI.transform.Find("NFTImage").GetComponent<RawImage>().texture = texture;
        nftUI.transform.Find("NFTName").GetComponent<TMP_Text>().text = nftName;
    }

    private async Task<string> GetNFTMetadata(string mintAddress)
    {
        string url = $"https://api.helius.xyz/v0/token-metadata?api-key=YOUR_API_KEY&mint={mintAddress}";
        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            await request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                return request.downloadHandler.text;
            }
            else
            {
                Debug.LogError($"Failed to fetch NFT metadata: {request.error}");
                return null;
            }
        }
    }
}
