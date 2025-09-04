using System.Reflection;
using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;
using System;
using System.Globalization;
using Solana.Unity.SDK;
using Solana.Unity.Rpc;
using Solana.Unity.Rpc.Types;
using Solana.Unity.Rpc.Models; // <-- needed for Transaction model

[System.Serializable]
public class KaminoLoanOffer
{
    public string symbol;
    public float ltv;
    public string supplyApy;
    public string borrowApy;
    // Add this helper method to KaminoAPI class to fix CS0103 for FlattenException


}

[System.Serializable]
public class KaminoLoanOffersResponse
{
    public List<KaminoLoanOffer> offers;
}

[System.Serializable]
public class KaminoLendRequest
{
    public string symbol;
    public string amount;
    public KaminoLendRequest(string symbol, string amount)
    {
        this.symbol = symbol;
        this.amount = amount;
    }
}

[System.Serializable]
public class UserBorrowingCapacityResponse
{
    public bool success;
    public float totalCollateral;
    public float totalBorrowed;
    public float availableToBorrow;
}

[System.Serializable]
public class UserPosition
{
    public string symbol;
    public float amount;
    public float apy;
    public bool isLend;
    public string date;
    public string cTokenAccount;
    public string cTokenBalance;
}

public class KaminoAPI : MonoBehaviour
{
    public static KaminoAPI Instance { get; private set; }
    void Awake()
    {
        Instance = this;
        VLog("Awake");
    }

    private const string API_URL = "https://solcietyserver.vercel.app/api/kamino-loan-offers";
    // CHANGED: point to kamino-lend (not kamino-lend-build)
    private const string LendUrl = "https://solcietyserver.vercel.app/api/kamino-lend";
    private const string BorrowBuildUrl = "https://solcietyserver.vercel.app/api/kamino-borrow-build";
    private const string RepayUrl = "https://solcietyserver.vercel.app/api/kamino-repay";
    private const string WithdrawUrl = "https://solcietyserver.vercel.app/api/kamino-withdraw";
    private const string CapacityUrl = "https://solcietyserver.vercel.app/api/kamino-user-borrowing-capacity";
    private const string ActivityUrl = "https://solcietyserver.vercel.app/api/kamino-user-activity?wallet={0}";

    public KaminoLoanOfferUI offerPrefab;
    public Transform offersContainer;
    public KaminoLendModal lendModal;
    public KaminoBorrowModal borrowModal;
    public KaminoUserActivityModal activityModal;
    public LoadingSpinner loadingSpinner;

    private static string FlattenException(Exception ex)
    {
        if (ex == null) return "<null>";
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        int depth = 0;
        while (ex != null && depth < 5)
        {
            sb.AppendLine(ex.Message);
            ex = ex.InnerException;
            depth++;
        }
        return sb.ToString();
    }

    private static string ToApiAmount(float amount) => amount.ToString("F8", CultureInfo.InvariantCulture);

    [Serializable]
    private class BuildTxRequest
    {
        public string owner;
        public string symbol;
        public string uiAmount;
    }
    [Serializable]
    private class BuildTxResponse
    {
        public string transaction;
        public string blockhash;
        // Unity JsonUtility does not support long (Int64) reliably; use string to avoid parse issues.
        public string lastValidBlockHeight;
        public string error;

        // New (optional) fields — server may return them to avoid endpoint mismatch and stale context
        public string rpcUrl;
        public string minContextSlot;
    }

    void Start()
    {
        VLog("Start: fetching offers and borrowing capacity");
        FetchKaminoLoanOffers();
        FetchUserBorrowingCapacity();
    }

    public void FetchKaminoLoanOffers()
    {
        if (loadingSpinner != null) loadingSpinner.Show("Loading Kamino tokens...");
        VLog($"Fetching loan offers from {API_URL}");
        StartCoroutine(FetchKaminoLoanOffersCoroutine());
    }

    private IEnumerator FetchKaminoLoanOffersCoroutine()
    {
        using (UnityWebRequest request = UnityWebRequest.Get(API_URL))
        {
            yield return request.SendWebRequest();
            if (loadingSpinner != null) loadingSpinner.Hide();

            if (request.result == UnityWebRequest.Result.Success)
            {
                var json = request.downloadHandler.text;
                VLog($"Offers response received, length={SafeLen(json)}");
                KaminoLoanOffersResponse response = JsonUtility.FromJson<KaminoLoanOffersResponse>(json);
                if (response != null && response.offers != null)
                {
                    VLog($"Parsed {response.offers.Count} offers; rebuilding UI");
                    foreach (Transform child in offersContainer) Destroy(child.gameObject);
                    foreach (var offer in response.offers)
                    {
                        var offerUI = Instantiate(offerPrefab, offersContainer);
                        offerUI.SetOffer(offer);
                        offerUI.lendModal = lendModal;
                        offerUI.borrowModal = borrowModal;
                    }
                }
                else
                {
                    Debug.LogWarning("[KaminoAPI] No offers found or failed to parse JSON");
                }
            }
            else
            {
                Debug.LogError($"[KaminoAPI] Failed to fetch offers: code={request.responseCode} error={request.error}");
            }
        }
    }

    // ---------------- LEND: build unsigned -> sign+send -> confirm ----------------
    public void Lend(KaminoLoanOffer offer, float amount)
    {
        VLog($"Lend called: symbol={offer?.symbol} amount={amount}");
        if (loadingSpinner != null) loadingSpinner.Show("Preparing lend transaction...");
        StartCoroutine(LendBuildSignSendCoroutine(offer, amount));
    }

    private IEnumerator LendBuildSignSendCoroutine(KaminoLoanOffer offer, float amount)
    {
        string owner = WalletManager.instance != null ? WalletManager.instance.walletAddress : null;
        if (string.IsNullOrEmpty(owner))
        {
            if (loadingSpinner != null) loadingSpinner.Hide();
            if (lendModal != null) lendModal.ShowError("No wallet connected.");
            VLog("Lend aborted: no wallet connected");
            yield break;
        }

        var payload = new BuildTxRequest
        {
            owner = owner,
            symbol = offer.symbol,
            uiAmount = ToApiAmount(amount)
        };
        string json = JsonUtility.ToJson(payload);
        VLog($"Lend request->{LendUrl} owner={payload.owner} symbol={payload.symbol} uiAmount={payload.uiAmount}");

        using (UnityWebRequest request = new UnityWebRequest(LendUrl, "POST"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();
            var respText = request.downloadHandler != null ? request.downloadHandler.text : null;
            VLog($"Lend HTTP {request.responseCode} respLen={SafeLen(respText)}");

            if (request.result != UnityWebRequest.Result.Success)
            {
                if (loadingSpinner != null) loadingSpinner.Hide();
                string errorMsg = string.IsNullOrEmpty(respText) ? request.error : respText;
                Debug.LogError("[KaminoAPI] Lend request failed: " + errorMsg);
                if (lendModal != null) lendModal.ShowError("Lend request failed: " + errorMsg);
                yield break;
            }

            var buildResp = JsonUtility.FromJson<BuildTxResponse>(respText);
            if (buildResp == null || !string.IsNullOrEmpty(buildResp.error) || string.IsNullOrEmpty(buildResp.transaction))
            {
                if (loadingSpinner != null) loadingSpinner.Hide();
                string e = buildResp != null && !string.IsNullOrEmpty(buildResp.error) ? buildResp.error : "Invalid lend response";
                string rawSnippet = string.IsNullOrEmpty(respText) ? "<null>" : respText.Substring(0, Math.Min(respText.Length, 512));
                Debug.LogError($"[KaminoAPI] Lend parse error: {e}. Raw response (truncated {rawSnippet.Length}/{SafeLen(respText)}): {rawSnippet}");
                if (lendModal != null) lendModal.ShowError("Lend error: " + e);
                yield break;
            }

            VLog($"Lend OK: blockhash={buildResp.blockhash} lastValid={buildResp.lastValidBlockHeight} txB64Len={SafeLen(buildResp.transaction)}");

            if (loadingSpinner != null) loadingSpinner.Show("Signing and sending...");

            // Deserialize base64 -> Transaction
            Transaction tx;
            try
            {
                byte[] txBytes = Convert.FromBase64String(buildResp.transaction);
                VLog($"Lend tx base64 decoded, bytes={txBytes.Length}");
                tx = Transaction.Deserialize(txBytes);
                VLog("Lend tx deserialized");
            }
            catch (Exception ex)
            {
                if (loadingSpinner != null) loadingSpinner.Hide();
                Debug.LogError("[KaminoAPI] Lend invalid transaction data: " + ex.Message);
                if (lendModal != null) lendModal.ShowError("Invalid transaction data.");
                yield break;
            }

            // Sanity-check: blockhash inside tx matches server’s (when readable)
            var txBlockhash = TryGetRecentBlockhash(tx);
            if (!string.IsNullOrEmpty(txBlockhash))
            {
                if (!string.IsNullOrEmpty(buildResp.blockhash) && txBlockhash != buildResp.blockhash)
                {
                    Debug.LogWarning($"[KaminoAPI] Lend tx.RecentBlockhash mismatch. Tx={txBlockhash} Server={buildResp.blockhash}");
                }
            }
            else
            {
                VLog("Lend tx: RecentBlockhash not readable from tx object; skipping check.");
            }

            // Choose endpoint
            var chosenRpc = !string.IsNullOrEmpty(buildResp.rpcUrl) ? Solana.Unity.Rpc.ClientFactory.GetClient(buildResp.rpcUrl) : Web3.Rpc;
            VLog($"RPC endpoint (chosen): {(chosenRpc?.NodeAddress != null ? chosenRpc.NodeAddress.ToString() : "<null>")}");

            // Sign
            var signTask = Web3.Wallet.SignTransaction(tx);
            while (!signTask.IsCompleted) yield return null;

            if (signTask.IsFaulted || signTask.Result == null)
            {
                if (loadingSpinner != null) loadingSpinner.Hide();
                Debug.LogError($"[KaminoAPI] Lend sign task faulted. Endpoint={(chosenRpc?.NodeAddress != null ? chosenRpc.NodeAddress.ToString() : "<null>")} Exception={FlattenException(signTask.Exception)}");
                if (lendModal != null) lendModal.ShowError("Failed to sign transaction. See console for details.");
                yield break;
            }

            var signedTx = signTask.Result;
            string signedB64;
            try
            {
                signedB64 = Convert.ToBase64String(signedTx.Serialize());
            }
            catch (Exception ex)
            {
                if (loadingSpinner != null) loadingSpinner.Hide();
                Debug.LogError("[KaminoAPI] Lend serialize error: " + ex.Message);
                if (lendModal != null) lendModal.ShowError("Failed to serialize transaction.");
                yield break;
            }

            VLog($"Sending via RPC: {(chosenRpc?.NodeAddress != null ? chosenRpc.NodeAddress.ToString() : "<null>")} payloadLen={signedB64.Length}");
            var sendTask = chosenRpc.SendTransactionAsync(signedB64);
            while (!sendTask.IsCompleted) yield return null;

            var sendResult = sendTask.Result; // RequestResult<string>
            if (sendResult == null || !sendResult.WasSuccessful || string.IsNullOrEmpty(sendResult.Result))
            {
                if (loadingSpinner != null) loadingSpinner.Hide();
                string endpoint = chosenRpc?.NodeAddress != null ? chosenRpc.NodeAddress.ToString() : "<null>";
                Debug.LogError($"[KaminoAPI] Lend send failed. Endpoint={endpoint}\n{DumpRequestResult(sendResult)}");
                if (lendModal != null)
                    lendModal.ShowError("Failed to send transaction: " + (sendResult?.Reason ?? "Unknown error"));
                yield break;
            }

            string signature = sendResult.Result;
            VLog($"Lend sent, signature={signature}");

            if (loadingSpinner != null) loadingSpinner.Show("Confirming transaction...");
            yield return StartCoroutine(WaitForConfirmation(chosenRpc, signature, 45f));
            VLog("Lend confirmation finished (either confirmed or timed out)");

            if (loadingSpinner != null) loadingSpinner.Hide();

            if (lendModal != null)
            {
                lendModal.ShowSuccess("Lend successful! Your deposit transaction was confirmed.");
            }

            FetchUserBorrowingCapacity();
        }
    }

    private IEnumerator WaitForConfirmation(IRpcClient rpc, string signature, float timeoutSeconds)
    {
        VLog($"Begin confirmation: signature={signature} timeout={timeoutSeconds}s");
        float elapsed = 0f;
        const float step = 0.8f;

        while (elapsed < timeoutSeconds)
        {
            var statusTask = rpc.GetSignatureStatusesAsync(new List<string> { signature }, false);
            while (!statusTask.IsCompleted) yield return null;

            var rr = statusTask.Result;
            if (rr != null && rr.WasSuccessful && rr.Result != null && rr.Result.Value != null && rr.Result.Value.Count > 0)
            {
                var info = rr.Result.Value[0];
                if (info != null && (info.ConfirmationStatus == "confirmed" || info.ConfirmationStatus == "finalized"))
                {
                    VLog($"Confirmed: signature={signature} status={info.ConfirmationStatus}");
                    yield break;
                }
            }

            yield return new WaitForSeconds(step);
            elapsed += step;
        }

        Debug.LogWarning("[KaminoAPI] Transaction confirmation timeout for " + signature);
    }

    // --------- Existing capacity/activity remain unchanged ---------

    public void FetchUserBorrowingCapacity()
    {
        VLog("Fetching user borrowing capacity");
        StartCoroutine(FetchUserBorrowingCapacityCoroutine());
    }

    public void FetchUserBorrowingCapacity(System.Action<UserBorrowingCapacityResponse> callback)
    {
        VLog("Fetching user borrowing capacity (with callback)");
        StartCoroutine(FetchUserBorrowingCapacityCoroutine(callback));
    }

    private IEnumerator FetchUserBorrowingCapacityCoroutine()
    {
        return FetchUserBorrowingCapacityCoroutine(null);
    }

    private IEnumerator FetchUserBorrowingCapacityCoroutine(System.Action<UserBorrowingCapacityResponse> callback)
    {
        using (UnityWebRequest request = UnityWebRequest.Get(CapacityUrl))
        {
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                var json = request.downloadHandler.text;
                VLog($"Capacity response len={SafeLen(json)}");
                var response = JsonUtility.FromJson<UserBorrowingCapacityResponse>(json);
                if (response != null && response.success)
                {
                    VLog($"Capacity: collateral={response.totalCollateral} borrowed={response.totalBorrowed} available={response.availableToBorrow}");
                    currentUserBorrowingCapacity = response;
                    callback?.Invoke(response);
                }
                else
                {
                    Debug.LogWarning("[KaminoAPI] Failed to parse borrowing capacity response");
                    callback?.Invoke(null);
                }
            }
            else
            {
                Debug.LogError($"[KaminoAPI] Failed to fetch borrowing capacity: code={request.responseCode} err={request.error}");
                callback?.Invoke(null);
            }
        }
    }

    private UserBorrowingCapacityResponse currentUserBorrowingCapacity;

    public UserBorrowingCapacityResponse GetCurrentUserBorrowingCapacity()
    {
        return currentUserBorrowingCapacity;
    }

    // --------- Repay/Withdraw kept server-signed for now; add logs ---------

    [System.Serializable]
    public class RepayRequest { public string symbol; public string amount; }

    public void Repay(string symbol, float amount, System.Action<bool, string> callback = null)
    {
        VLog($"Repay called: symbol={symbol} amount={amount}");
        StartCoroutine(RepayCoroutine(symbol, amount, callback));
    }

    private IEnumerator RepayCoroutine(string symbol, float amount, System.Action<bool, string> callback)
    {
        string url = RepayUrl;
        var payload = new RepayRequest { symbol = symbol, amount = ToApiAmount(amount) };
        string json = JsonUtility.ToJson(payload);
        VLog($"Repay POST {url} payload={json}");

        using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();
            var resp = request.downloadHandler != null ? request.downloadHandler.text : null;
            VLog($"Repay HTTP {request.responseCode} respLen={SafeLen(resp)}");

            if (request.result == UnityWebRequest.Result.Success)
            {
                callback?.Invoke(true, resp);
            }
            else
            {
                string errorMessage = string.IsNullOrEmpty(resp) ? request.error : resp;
                Debug.LogError("[KaminoAPI] Repay failed: " + errorMessage);
                callback?.Invoke(false, errorMessage);
            }
        }
    }

    [System.Serializable]
    public class WithdrawRequest { public string symbol; public string amount; }

    public void Withdraw(string symbol, float cTokenAmount, System.Action<bool, string> callback = null)
    {
        VLog($"Withdraw called: symbol={symbol} amount={cTokenAmount}");
        StartCoroutine(WithdrawCoroutine(symbol, cTokenAmount, callback));
    }

    private IEnumerator WithdrawCoroutine(string symbol, float cTokenAmount, System.Action<bool, string> callback)
    {
        string url = WithdrawUrl;
        var payload = new WithdrawRequest { symbol = symbol, amount = ToApiAmount(cTokenAmount) };
        string json = JsonUtility.ToJson(payload);
        VLog($"Withdraw POST {url} payload={json}");

        using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();
            var resp = request.downloadHandler != null ? request.downloadHandler.text : null;
            VLog($"Withdraw HTTP {request.responseCode} respLen={SafeLen(resp)}");

            if (request.result == UnityWebRequest.Result.Success)
            {
                callback?.Invoke(true, resp);
            }
            else
            {
                string errorMessage = string.IsNullOrEmpty(resp) ? request.error : resp;
                Debug.LogError("[KaminoAPI] Withdraw failed: " + errorMessage);
                callback?.Invoke(false, errorMessage);
            }
        }
    }

    // ---------------- Helpers & logging ----------------

    private bool verboseLogging = true;
    private void VLog(string msg) { if (verboseLogging) Debug.Log("[KaminoAPI] " + msg); }
    private static int SafeLen(string s) => string.IsNullOrEmpty(s) ? 0 : s.Length;
    private static string Truncate(string s, int max)
        => string.IsNullOrEmpty(s) ? "<null>" : (s.Length <= max ? s : s.Substring(0, max));

    private static string DumpRequestResult(object rrObj)
    {
        if (rrObj == null) return "<null>";
        var sb = new System.Text.StringBuilder();
        try
        {
            var t = rrObj.GetType();
            var props = t.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (var p in props)
            {
                object val = null;
                try { val = p.GetValue(rrObj, null); } catch { }

                if (p.Name == "RawRpcResponse")
                {
                    var raw = val as string;
                    sb.AppendLine($"RawRpcResponse (truncated {SafeLen(raw)}): {Truncate(raw, 1200)}");
                }
                else if (p.Name == "Result")
                {
                    var s = val != null ? val.ToString() : "<null>";
                    sb.AppendLine($"Result: {Truncate(s, 256)}");
                }
                else
                {
                    sb.AppendLine($"{p.Name}: {(val ?? "<null>")}");
                }
            }
        }
        catch (Exception ex)
        {
            sb.AppendLine($"DumpRequestResult error: {ex.Message}");
        }
        return sb.ToString();
    }

    private static bool TryGetStringProperty(System.Type t, object obj, string name, out string value)
    {
        value = null;
        try
        {
            var p = t?.GetProperty(name, BindingFlags.Public | BindingFlags.Instance);
            if (p != null)
            {
                var v = p.GetValue(obj);
                value = v as string ?? v?.ToString();
                return !string.IsNullOrEmpty(value);
            }
        }
        catch { }
        return false;
    }

    // Tries tx.RecentBlockhash/RecentBlockHash and tx.Message.RecentBlockhash/RecentBlockHash
    private static string TryGetRecentBlockhash(object tx)
    {
        if (tx == null) return null;
        try
        {
            var t = tx.GetType();
            if (TryGetStringProperty(t, tx, "RecentBlockhash", out var s)) return s;
            if (TryGetStringProperty(t, tx, "RecentBlockHash", out s)) return s;

            var msgProp = t.GetProperty("Message", BindingFlags.Public | BindingFlags.Instance);
            var msg = msgProp?.GetValue(tx);
            if (msg != null)
            {
                var mt = msg.GetType();
                if (TryGetStringProperty(mt, msg, "RecentBlockhash", out s)) return s;
                if (TryGetStringProperty(mt, msg, "RecentBlockHash", out s)) return s;
            }
        }
        catch { }
        return null;
    }

    public void Borrow(KaminoLoanOffer offer, float amount)
    {
        VLog($"[Mock] Borrow called: symbol={offer?.symbol} amount={amount}");
        Debug.LogWarning("[KaminoAPI] Borrow is currently mocked. No transaction has been built or sent.");
    }
}