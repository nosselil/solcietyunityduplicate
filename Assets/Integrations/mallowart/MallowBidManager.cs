using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using Solana.Unity.SDK;
using Solana.Unity.Wallet;
using Solana.Unity.Rpc.Models;

public class MallowBidManager : MonoBehaviour
{
    private const string apiUrl = "https://api.mallow.art/v1/getBidOrBuyTx";
    private const string apiKey = "6HBE7PH9D2HDG4Q6";

    [System.Serializable]
    private class TxResultWrapper
    {
        public string result;
    }

    [System.Serializable]
    private class MallowPayload
    {
        public string buyer;
        public string mint;
        public long price;
    }

public IEnumerator SubmitBid(string mintAddress, string auctionAddress, float bidAmountSOL, float minBidSOL)
    {
        string buyerAddress = Web3.Account?.PublicKey;

        if (string.IsNullOrEmpty(buyerAddress))
        {
            Debug.LogError("❌ Wallet not connected or public key is null/empty.");
            yield break;
        }
        else
        {
            Debug.Log($"🟢 Wallet connected. PublicKey: {buyerAddress}");
        }

        if (string.IsNullOrEmpty(auctionAddress))
        {
            Debug.LogWarning($"⚠️ auctionHouse address is null or empty! Value: '{auctionAddress}'");
        }

        if (bidAmountSOL < minBidSOL)
        {
            Debug.LogError($"❌ Bid amount ({bidAmountSOL} SOL) is less than the minimum required ({minBidSOL} SOL). Bid not submitted.");
            yield break;
        }

        var payload = new MallowPayload
        {
            buyer = buyerAddress,
            mint = mintAddress,
            price = (long)(bidAmountSOL * 1_000_000_000L)
        };

        string jsonBody = JsonUtility.ToJson(payload);
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonBody);

        using UnityWebRequest request = new UnityWebRequest(apiUrl, "POST")
        {
            uploadHandler = new UploadHandlerRaw(bodyRaw),
            downloadHandler = new DownloadHandlerBuffer()
        };
        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("X-Api-Key", apiKey);

        Debug.Log("📤 Submitting Bid:");
        Debug.Log($"Buyer: {payload.buyer}");
        Debug.Log($"Mint: {payload.mint}");
        Debug.Log($"Price (lamports): {payload.price}");
        Debug.Log($"Raw JSON: {jsonBody}");

        yield return request.SendWebRequest();
        try
        {
            Debug.Log($"[BID] Status Code: {request.responseCode}");
            Debug.Log($"[BID] Response: {request.downloadHandler?.text}");
        }
        catch (Exception ex)
        {
            Debug.LogError($"[BID] Exception after request: {ex}");
        }

        Debug.Log($"[BID] UnityWebRequest.Result: {request.result}");

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError($"❌ Bid failed: {request.error} | Response: {request.downloadHandler?.text}");
            yield break;
        }

        string base64Tx = ExtractBase64Tx(request.downloadHandler.text);

        if (string.IsNullOrEmpty(base64Tx))
        {
            Debug.LogError($"❌ No transaction received or parsing failed. Raw response: {request.downloadHandler.text}");
            yield break;
        }

        byte[] txBytes;
        Transaction transaction;

        try
        {
            txBytes = Convert.FromBase64String(base64Tx);
            transaction = Transaction.Deserialize(txBytes);
            Debug.Log("🟢 Transaction deserialized successfully.");
        }
        catch (Exception e)
        {
            Debug.LogError($"❌ Transaction processing failed: {e.Message}\nBase64: {base64Tx}");
            yield break;
        }

        var task = Web3.Wallet.SignAndSendTransaction(transaction);
        yield return new WaitUntil(() => task.IsCompleted); // ✅ Now valid!

        if (task.Exception != null)
        {
            Debug.LogError($"❌ TX failed: {task.Exception.Message}");
            yield break;
        }

        if (task.Result == null)
        {
            Debug.LogError("❌ TX result is null after signing and sending.");
            yield break;
        }

        Debug.Log($"✅ Bid submitted! TxID: {task.Result}");
    }

    private string ExtractBase64Tx(string json)
    {
        try
        {
            TxResultWrapper wrapper = JsonUtility.FromJson<TxResultWrapper>(json);
            if (wrapper == null || string.IsNullOrEmpty(wrapper.result))
            {
                Debug.LogError("❌ Could not extract transaction from Mallow response.");
                return null;
            }
            return wrapper.result;
        }
        catch (Exception e)
        {
            Debug.LogError("❌ Failed to parse Mallow response: " + e.Message);
            return null;
        }
    }
}
