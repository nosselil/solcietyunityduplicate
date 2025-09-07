using System.Reflection;
using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;
using System;
using System.Globalization;
using System.Text; // <-- add
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
    private const string LendUrl = "https://solcietyserver.vercel.app/api/kamino-lend";    
    private const string BorrowUrl = "https://solcietyserver.vercel.app/api/kamino-borrow";
    private const string RepayUrl = "https://solcietyserver.vercel.app/api/kamino-repay";
    private const string WithdrawUrl = "https://solcietyserver.vercel.app/api/kamino-withdraw";
    private const string CapacityUrl = "https://solcietyserver.vercel.app/api/kamino-user-borrowing-capacity?wallet={0}";
    private const string ActivityUrl = "https://solcietyserver.vercel.app/api/kamino-user-activity?wallet={0}";

    public KaminoLoanOfferUI offerPrefab;
    public Transform offersContainer;
    public KaminoLendModal lendModal;
    public KaminoBorrowModal borrowModal;
    public KaminoUserActivityModal activityModal;
    public LoadingSpinner loadingSpinner;

    // Known token decimals (extend as needed; helps pre-validate tiny amounts)
    private static readonly Dictionary<string, int> KnownTokenDecimals = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
    {
        { "USDC", 6 }, { "USDT", 6 }, { "FDUSD", 6 },
        { "SOL", 9 }, { "WSOL", 9 }, { "JITOSOL", 9 }
    };

    private static decimal Pow10Dec(int exp)
    {
        decimal v = 1m;
        for (int i = 0; i < exp; i++) v *= 10m;
        return v;
    }

    private static bool TryGetMinUiAmount(string symbol, out decimal minUi)
    {
        if (KnownTokenDecimals.TryGetValue(symbol ?? "", out int dec))
        {
            minUi = decimal.One / Pow10Dec(dec);
            return true;
        }
        minUi = 0m;
        return false;
    }

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

    // Changed: allow tiny amounts (up to 18 fractional digits), avoid rounding to "0"
    private static string ToApiAmount(float amount)
    {
        if (float.IsNaN(amount) || float.IsInfinity(amount) || amount <= 0f) return "0";
        decimal d = (decimal)amount;
        if (d < 0m) d = 0m;
        // Up to 18 decimals, trim trailing zeros
        return d.ToString("0.##################", CultureInfo.InvariantCulture);
    }

    private static string ToIntegerString(string s)
    {
        if (string.IsNullOrEmpty(s)) return "0";
        int dot = s.IndexOf('.');
        var core = dot >= 0 ? s.Substring(0, dot) : s;
        core = core.Trim();
        if (core.Length == 0) return "0";
        // Remove leading '+' and leading zeros, but keep single zero if all zeros
        if (core[0] == '+') core = core.Substring(1);
        core = core.TrimStart('0');
        return core.Length == 0 ? "0" : core;
    }

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

    // --- JSON-RPC helpers to send with minContextSlot/preflight ---
    [Serializable]
    private class RpcError { public int code; public string message; public string data; }
    [Serializable]
    private class RpcRespString
    {
        public string jsonrpc;
        public string id;
        public string result;
        public RpcError error;
    }

    // Sends sendTransaction with { skipPreflight, preflightCommitment:"confirmed", encoding:"base64", minContextSlot? }
    private IEnumerator SendTransactionJsonRpc(IRpcClient rpc, string signedB64, string rpcUrl, string minContextSlot,
        Commitment preflightCommitment, bool skipPreflight,
        System.Action<bool, string, string> callback)
    {
        string endpoint = !string.IsNullOrEmpty(rpcUrl)
            ? rpcUrl
            : (rpc?.NodeAddress != null ? rpc.NodeAddress.ToString() : null);

        if (string.IsNullOrEmpty(endpoint))
        {
            callback?.Invoke(false, null, "No RPC endpoint available.");
            yield break;
        }

        // Initialize to satisfy definite assignment analysis
        ulong mcs = 0UL;
        bool haveMcs = !string.IsNullOrEmpty(minContextSlot) &&
                   ulong.TryParse(minContextSlot, NumberStyles.Integer, CultureInfo.InvariantCulture, out mcs);

        string cfg = haveMcs
            ? $"{{\"skipPreflight\":{skipPreflight.ToString().ToLowerInvariant()},\"preflightCommitment\":\"{preflightCommitment.ToString().ToLowerInvariant()}\",\"encoding\":\"base64\",\"minContextSlot\":{mcs}}}"
            : $"{{\"skipPreflight\":{skipPreflight.ToString().ToLowerInvariant()},\"preflightCommitment\":\"{preflightCommitment.ToString().ToLowerInvariant()}\",\"encoding\":\"base64\"}}";

        string payload = $"{{\"jsonrpc\":\"2.0\",\"id\":1,\"method\":\"sendTransaction\",\"params\":[\"{signedB64}\",{cfg}]}}";

        using (var uwr = new UnityWebRequest(endpoint, "POST"))
        {
            byte[] body = Encoding.UTF8.GetBytes(payload);
            uwr.uploadHandler = new UploadHandlerRaw(body);
            uwr.downloadHandler = new DownloadHandlerBuffer();
            uwr.SetRequestHeader("Content-Type", "application/json");
            yield return uwr.SendWebRequest();

            string raw = uwr.downloadHandler != null ? uwr.downloadHandler.text : null;

            if (uwr.result != UnityWebRequest.Result.Success)
            {
                callback?.Invoke(false, null, $"HTTP {uwr.responseCode} {uwr.error}. Raw: {Truncate(raw, 800)}");
                yield break;
            }

            RpcRespString resp = null;
            try { resp = JsonUtility.FromJson<RpcRespString>(raw); } catch { }

            if (resp != null && !string.IsNullOrEmpty(resp.result))
            {
                callback?.Invoke(true, resp.result, raw);
            }
            else
            {
                string msg = resp?.error != null ? $"{resp.error.code}: {resp.error.message}" : "Unknown RPC error";
                callback?.Invoke(false, null, $"RPC error: {msg}. Raw: {Truncate(raw, 800)}");
            }
        }
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

            // Optional: also use the JSON-RPC sender here (currently Lend works as-is)
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
        var owner = WalletManager.instance != null ? WalletManager.instance.walletAddress : null;
        if (string.IsNullOrEmpty(owner))
        {
            Debug.LogWarning("[KaminoAPI] Cannot fetch user borrowing capacity: no wallet connected.");
            callback?.Invoke(null);
            yield break;
        }

        string url = string.Format(CapacityUrl, owner);
        using (UnityWebRequest request = UnityWebRequest.Get(url))
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
    public class RepayRequest { public string owner; public string symbol; public string uiAmount; }

    public void Repay(string symbol, float amount, System.Action<bool, string> callback = null)
    {
        VLog($"Repay called: symbol={symbol} amount={amount}");
        if (loadingSpinner != null) loadingSpinner.Show("Preparing repay transaction...");
        StartCoroutine(RepayBuildSignSendCoroutine(symbol, amount, callback));
    }

    private IEnumerator RepayBuildSignSendCoroutine(string symbol, float amount, System.Action<bool, string> callback)
    {
        string owner = WalletManager.instance != null ? WalletManager.instance.walletAddress : null;
        if (string.IsNullOrEmpty(owner))
        {
            if (loadingSpinner != null) loadingSpinner.Hide();
            callback?.Invoke(false, "No wallet connected.");
            yield break;
        }

        // Pre-validate tiny ui amounts against known token decimals to avoid server-side floor-to-zero
        decimal amountDec = (decimal)amount;
        if (TryGetMinUiAmount(symbol, out var minUi) && amountDec < minUi)
        {
            if (loadingSpinner != null) loadingSpinner.Hide();
            callback?.Invoke(false, $"Amount is too small for {symbol} token decimals. Minimum is {minUi.ToString("0.##################", CultureInfo.InvariantCulture)}.");
            yield break;
        }

        var payload = new RepayRequest { owner = owner, symbol = symbol, uiAmount = ToApiAmount(amount) };
        string json = JsonUtility.ToJson(payload);
        VLog($"Repay request->{RepayUrl} owner={payload.owner} symbol={payload.symbol} uiAmount={payload.uiAmount}");

        using (UnityWebRequest request = new UnityWebRequest(RepayUrl, "POST"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();
            var respText = request.downloadHandler != null ? request.downloadHandler.text : null;
            VLog($"Repay HTTP {request.responseCode} respLen={SafeLen(respText)}");

            if (request.result != UnityWebRequest.Result.Success)
            {
                if (loadingSpinner != null) loadingSpinner.Hide();
                string errorMsg = string.IsNullOrEmpty(respText) ? request.error : respText;
                Debug.LogError("[KaminoAPI] Repay request failed: " + errorMsg);
                // Friendlier tiny-amount message if backend still responds with zero
                if (!string.IsNullOrEmpty(respText) && respText.IndexOf("greater than zero", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    errorMsg = $"Amount is too small for {symbol} token decimals.";
                }
                callback?.Invoke(false, errorMsg);
                yield break;
            }

            var buildResp = JsonUtility.FromJson<BuildTxResponse>(respText);
            if (buildResp == null || !string.IsNullOrEmpty(buildResp.error) || string.IsNullOrEmpty(buildResp.transaction))
            {
                if (loadingSpinner != null) loadingSpinner.Hide();
                string e = buildResp != null && !string.IsNullOrEmpty(buildResp.error) ? buildResp.error : "Invalid repay response";
                string rawSnippet = string.IsNullOrEmpty(respText) ? "<null>" : respText.Substring(0, Math.Min(respText.Length, 512));
                Debug.LogError($"[KaminoAPI] Repay parse error: {e}. Raw response (truncated {rawSnippet.Length}/{SafeLen(respText)}): {rawSnippet}");
                callback?.Invoke(false, "Repay error: " + e);
                yield break;
            }

            VLog($"Repay OK: blockhash={buildResp.blockhash} lastValid={buildResp.lastValidBlockHeight} txB64Len={SafeLen(buildResp.transaction)}");

            // Deserialize -> sign
            Transaction tx;
            try
            {
                byte[] txBytes = Convert.FromBase64String(buildResp.transaction);
                VLog($"Repay tx base64 decoded, bytes={txBytes.Length}");
                tx = Transaction.Deserialize(txBytes);
                VLog("Repay tx deserialized");
            }
            catch (Exception ex)
            {
                if (loadingSpinner != null) loadingSpinner.Hide();
                Debug.LogError("[KaminoAPI] Repay invalid transaction data: " + ex.Message);
                callback?.Invoke(false, "Invalid transaction data.");
                yield break;
            }

            var chosenRpc = !string.IsNullOrEmpty(buildResp.rpcUrl)
                ? Solana.Unity.Rpc.ClientFactory.GetClient(buildResp.rpcUrl)
                : Web3.Rpc;
            VLog($"RPC endpoint (chosen): {(chosenRpc?.NodeAddress != null ? chosenRpc.NodeAddress.ToString() : "<null>")}");

            var signTask = Web3.Wallet.SignTransaction(tx);
            while (!signTask.IsCompleted) yield return null;

            if (signTask.IsFaulted || signTask.Result == null)
            {
                if (loadingSpinner != null) loadingSpinner.Hide();
                Debug.LogError($"[KaminoAPI] Repay sign task faulted. Endpoint={(chosenRpc?.NodeAddress != null ? chosenRpc.NodeAddress.ToString() : "<null>")} Exception={FlattenException(signTask.Exception)}");
                callback?.Invoke(false, "Failed to sign transaction.");
                yield break;
            }

            string signedB64;
            try
            {
                signedB64 = Convert.ToBase64String(signTask.Result.Serialize());
            }
            catch (Exception ex)
            {
                if (loadingSpinner != null) loadingSpinner.Hide();
                Debug.LogError("[KaminoAPI] Repay serialize error: " + ex.Message);
                callback?.Invoke(false, "Failed to serialize transaction.");
                yield break;
            }

            // Send via JSON-RPC with minContextSlot & preflight=confirmed
            VLog($"Sending via RPC: {(chosenRpc?.NodeAddress != null ? chosenRpc.NodeAddress.ToString() : "<null>")} payloadLen={signedB64.Length}");

            bool sendOk = false;
            string signature = null;
            string rawRpcResponse = null;

            yield return StartCoroutine(SendTransactionJsonRpc(
                chosenRpc,
                signedB64,
                buildResp.rpcUrl,
                buildResp.minContextSlot,
                Commitment.Confirmed,
                false,
                (ok, sig, raw) => { sendOk = ok; signature = sig; rawRpcResponse = raw; }
            ));

            if (!sendOk || string.IsNullOrEmpty(signature))
            {
                if (loadingSpinner != null) loadingSpinner.Hide();
                string endpoint = chosenRpc?.NodeAddress != null ? chosenRpc.NodeAddress.ToString() : "<null>";
                Debug.LogError($"[KaminoAPI] Repay send failed. Endpoint={endpoint}\nRaw RPC: {Truncate(rawRpcResponse, 1200)}");
                callback?.Invoke(false, "Failed to send transaction: see console for RPC error.");
                yield break;
            }

            VLog($"Repay sent, signature={signature}");

            if (loadingSpinner != null) loadingSpinner.Show("Confirming transaction...");
            yield return StartCoroutine(WaitForConfirmation(chosenRpc, signature, 45f));
            VLog("Repay confirmation finished (either confirmed or timed out)");
            if (loadingSpinner != null) loadingSpinner.Hide();

            // Refresh capacity after repay
            FetchUserBorrowingCapacity();

            callback?.Invoke(true, signature);
        }
    }

    // Add alongside other request DTOs
    [System.Serializable]
    public class WithdrawBuildRequest { public string owner; public string symbol; public string cTokenAmount; }

    // Replace Withdraw flow with client-signed version
    public void Withdraw(string symbol, string cTokenAmountRaw, System.Action<bool, string> callback = null)
    {
        cTokenAmountRaw = ToIntegerString(cTokenAmountRaw);

        // Early guard: integer parsing may produce "0" -> will fail as dust
        if (cTokenAmountRaw == "0")
        {
            VLog($"Withdraw blocked: zero cToken amount after normalization. symbol={symbol}");
            callback?.Invoke(false, "Amount is too small (cTokens are integers).");
            return;
        }

        VLog($"Withdraw called: symbol={symbol} cTokenAmountRaw={Truncate(cTokenAmountRaw, 64)}");
        if (loadingSpinner != null) loadingSpinner.Show("Preparing withdraw transaction...");
        StartCoroutine(WithdrawBuildSignSendCoroutine(symbol, cTokenAmountRaw, callback));
    }

    // Backward-compat wrapper; prefer the string overload to avoid precision loss
    public void Withdraw(string symbol, float cTokenAmount, System.Action<bool, string> callback = null)
    {
        // Warn: float may lose precision for big cToken amounts; convert as integer string when possible
        string raw = ((double)cTokenAmount).ToString("0", CultureInfo.InvariantCulture);
        Withdraw(symbol, raw, callback);
    }

    private IEnumerator WithdrawBuildSignSendCoroutine(string symbol, string cTokenAmountRaw, System.Action<bool, string> callback)
    {
        string owner = WalletManager.instance != null ? WalletManager.instance.walletAddress : null;
        if (string.IsNullOrEmpty(owner))
        {
            if (loadingSpinner != null) loadingSpinner.Hide();
            callback?.Invoke(false, "No wallet connected.");
            yield break;
        }

        // Build request
        var payload = new WithdrawBuildRequest { owner = owner, symbol = symbol, cTokenAmount = cTokenAmountRaw ?? "0" };
        string json = JsonUtility.ToJson(payload);
        VLog($"Withdraw build request->{WithdrawUrl} owner={payload.owner} symbol={payload.symbol} cTokenAmount(len)={SafeLen(payload.cTokenAmount)}");

        using (UnityWebRequest request = new UnityWebRequest(WithdrawUrl, "POST"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();
            var respText = request.downloadHandler != null ? request.downloadHandler.text : null;
            VLog($"Withdraw HTTP {request.responseCode} respLen={SafeLen(respText)}");

            if (request.result != UnityWebRequest.Result.Success)
            {
                if (loadingSpinner != null) loadingSpinner.Hide();
                string errorMsg = string.IsNullOrEmpty(respText) ? request.error : respText;
                Debug.LogError("[KaminoAPI] Withdraw build failed: " + errorMsg);
                callback?.Invoke(false, "Withdraw request failed: " + errorMsg);
                yield break;
            }

            var buildResp = JsonUtility.FromJson<BuildTxResponse>(respText);
            if (buildResp == null || !string.IsNullOrEmpty(buildResp.error) || string.IsNullOrEmpty(buildResp.transaction))
            {
                if (loadingSpinner != null) loadingSpinner.Hide();
                string e = buildResp != null && !string.IsNullOrEmpty(buildResp.error) ? buildResp.error : "Invalid withdraw response";
                string rawSnippet = string.IsNullOrEmpty(respText) ? "<null>" : respText.Substring(0, Math.Min(respText.Length, 512));
                Debug.LogError($"[KaminoAPI] Withdraw parse error: {e}. Raw response (truncated {rawSnippet.Length}/{SafeLen(respText)}): {rawSnippet}");
                callback?.Invoke(false, "Withdraw error: " + e);
                yield break;
            }

            VLog($"Withdraw OK: blockhash={buildResp.blockhash} lastValid={buildResp.lastValidBlockHeight} txB64Len={SafeLen(buildResp.transaction)}");

            if (loadingSpinner != null) loadingSpinner.Show("Signing and sending...");

            // Deserialize -> sign
            Transaction tx;
            try
            {
                byte[] txBytes = Convert.FromBase64String(buildResp.transaction);
                VLog($"Withdraw tx base64 decoded, bytes={txBytes.Length}");
                tx = Transaction.Deserialize(txBytes);
                VLog("Withdraw tx deserialized");
            }
            catch (Exception ex)
            {
                if (loadingSpinner != null) loadingSpinner.Hide();
                Debug.LogError("[KaminoAPI] Withdraw invalid transaction data: " + ex.Message);
                callback?.Invoke(false, "Invalid transaction data.");
                yield break;
            }

            // Optional sanity check
            var txBlockhash = TryGetRecentBlockhash(tx);
            if (!string.IsNullOrEmpty(txBlockhash) && !string.IsNullOrEmpty(buildResp.blockhash) && txBlockhash != buildResp.blockhash)
            {
                Debug.LogWarning($"[KaminoAPI] Withdraw tx.RecentBlockhash mismatch. Tx={txBlockhash} Server={buildResp.blockhash}");
            }

            var chosenRpc = !string.IsNullOrEmpty(buildResp.rpcUrl)
                ? Solana.Unity.Rpc.ClientFactory.GetClient(buildResp.rpcUrl)
                : Web3.Rpc;
            VLog($"RPC endpoint (chosen): {(chosenRpc?.NodeAddress != null ? chosenRpc.NodeAddress.ToString() : "<null>")}");

            var signTask = Web3.Wallet.SignTransaction(tx);
            while (!signTask.IsCompleted) yield return null;

            if (signTask.IsFaulted || signTask.Result == null)
            {
                if (loadingSpinner != null) loadingSpinner.Hide();
                Debug.LogError($"[KaminoAPI] Withdraw sign task faulted. Endpoint={(chosenRpc?.NodeAddress != null ? chosenRpc.NodeAddress.ToString() : "<null>")} Exception={FlattenException(signTask.Exception)}");
                callback?.Invoke(false, "Failed to sign transaction.");
                yield break;
            }

            string signedB64;
            try
            {
                signedB64 = Convert.ToBase64String(signTask.Result.Serialize());
            }
            catch (Exception ex)
            {
                if (loadingSpinner != null) loadingSpinner.Hide();
                Debug.LogError("[KaminoAPI] Withdraw serialize error: " + ex.Message);
                callback?.Invoke(false, "Failed to serialize transaction.");
                yield break;
            }

            // Send via JSON-RPC with minContextSlot & preflight=confirmed
            VLog($"Sending via RPC: {(chosenRpc?.NodeAddress != null ? chosenRpc.NodeAddress.ToString() : "<null>")} payloadLen={signedB64.Length}");

            bool sendOk = false;
            string signature = null;
            string rawRpcResponse = null;

            yield return StartCoroutine(SendTransactionJsonRpc(
                chosenRpc,
                signedB64,
                buildResp.rpcUrl,
                buildResp.minContextSlot,
                Commitment.Confirmed,
                false,
                (ok, sig, raw) => { sendOk = ok; signature = sig; rawRpcResponse = raw; }
            ));

            if (!sendOk || string.IsNullOrEmpty(signature))
            {
                if (loadingSpinner != null) loadingSpinner.Hide();
                string endpoint = chosenRpc?.NodeAddress != null ? chosenRpc.NodeAddress.ToString() : "<null>";
                Debug.LogError($"[KaminoAPI] Withdraw send failed. Endpoint={endpoint}\nRaw RPC: {Truncate(rawRpcResponse, 1200)}");

                // Extract custom program error (e.g., 6011) and surface a friendlier hint
                if (TryExtractCustomError(rawRpcResponse, out var customCode, out var instrIdx))
                {
                    var friendly = customCode == 6011
                        ? "Program rejected the withdraw (code 6011). This often indicates a too-small amount (dust) or a reserve constraint. Try a larger amount."
                        : $"Program error code {customCode} at instruction {instrIdx}. See console logs for details.";
                    callback?.Invoke(false, friendly);
                }
                else
                {
                    callback?.Invoke(false, "Failed to send transaction: see console for RPC error.");
                }

                yield break;
            }

            VLog($"Withdraw sent, signature={signature}");

            if (loadingSpinner != null) loadingSpinner.Show("Confirming transaction...");
            yield return StartCoroutine(WaitForConfirmation(chosenRpc, signature, 45f));
            VLog("Withdraw confirmation finished (either confirmed or timed out)");
            if (loadingSpinner != null) loadingSpinner.Hide();

            // Refresh capacity after withdraw
            FetchUserBorrowingCapacity();

            callback?.Invoke(true, signature);
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

    private static bool TryGetStringProperty(System.Type t, object obj, char[] name, out string value)
    {
        value = null;
        try
        {
            foreach (var n in name)
            {
                var p = t?.GetProperty(n.ToString(), BindingFlags.Public | BindingFlags.Instance);
                if (p != null)
                {
                    var v = p.GetValue(obj);
                    value = v as string ?? v?.ToString();
                    return !string.IsNullOrEmpty(value);
                }
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
            if (TryGetStringProperty(t, tx, new char[] { 'R', 'e', 'c', 'e', 'n', 't', 'B', 'l', 'o', 'c', 'k', 'h', 'a', 's', 'h' }, out var s)) return s;
            if (TryGetStringProperty(t, tx, new char[] { 'R', 'e', 'c', 'e', 'n', 't', 'B', 'l', 'o', 'c', 'k', 'H', 'a', 's', 'h' }, out s)) return s;

            var msgProp = t.GetProperty("Message", BindingFlags.Public | BindingFlags.Instance);
            var msg = msgProp?.GetValue(tx);
            if (msg != null)
            {
                var mt = msg.GetType();
                if (TryGetStringProperty(mt, msg, new char[] { 'R', 'e', 'c', 'e', 'n', 't', 'B', 'l', 'o', 'c', 'k', 'h', 'a', 's', 'h' }, out s)) return s;
                if (TryGetStringProperty(mt, msg, new char[] { 'R', 'e', 'c', 'e', 'n', 't', 'B', 'l', 'o', 'c', 'k', 'H', 'a', 's', 'h' }, out s)) return s;
            }
        }
        catch { }
        return null;
    }

    public void Borrow(KaminoLoanOffer offer, float amount)
    {
        VLog($"Borrow called: symbol={offer?.symbol} amount={amount}");
        if (loadingSpinner != null) loadingSpinner.Show("Preparing borrow transaction...");
        StartCoroutine(BorrowBuildSignSendCoroutine(offer, amount));
    }

    private IEnumerator BorrowBuildSignSendCoroutine(KaminoLoanOffer offer, float amount)
    {
        string owner = WalletManager.instance != null ? WalletManager.instance.walletAddress : null;
        if (string.IsNullOrEmpty(owner))
        {
            if (loadingSpinner != null) loadingSpinner.Hide();
            if (borrowModal != null) borrowModal.ShowError("No wallet connected.");
            VLog("Borrow aborted: no wallet connected");
            yield break;
        }

        var payload = new BuildTxRequest
        {
            owner = owner,
            symbol = offer.symbol,
            uiAmount = ToApiAmount(amount)
        };
        string json = JsonUtility.ToJson(payload);
        VLog($"Borrow request->{BorrowUrl} owner={payload.owner} symbol={payload.symbol} uiAmount={payload.uiAmount}");

        using (UnityWebRequest request = new UnityWebRequest(BorrowUrl, "POST"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();
            var respText = request.downloadHandler != null ? request.downloadHandler.text : null;
            VLog($"Borrow HTTP {request.responseCode} respLen={SafeLen(respText)}");

            if (request.result != UnityWebRequest.Result.Success)
            {
                if (loadingSpinner != null) loadingSpinner.Hide();
                string errorMsg = string.IsNullOrEmpty(respText) ? request.error : respText;
                Debug.LogError("[KaminoAPI] Borrow request failed: " + errorMsg);
                if (borrowModal != null) borrowModal.ShowError("Borrow request failed: " + errorMsg);
                yield break;
            }

            var buildResp = JsonUtility.FromJson<BuildTxResponse>(respText);
            if (buildResp == null || !string.IsNullOrEmpty(buildResp.error) || string.IsNullOrEmpty(buildResp.transaction))
            {
                if (loadingSpinner != null) loadingSpinner.Hide();
                string e = buildResp != null && !string.IsNullOrEmpty(buildResp.error) ? buildResp.error : "Invalid borrow response";
                string rawSnippet = string.IsNullOrEmpty(respText) ? "<null>" : respText.Substring(0, Math.Min(respText.Length, 512));
                Debug.LogError($"[KaminoAPI] Borrow parse error: {e}. Raw response (truncated {rawSnippet.Length}/{SafeLen(respText)}): {rawSnippet}");
                if (borrowModal != null) borrowModal.ShowError("Borrow error: " + e);
                yield break;
            }

            VLog($"Borrow OK: blockhash={buildResp.blockhash} lastValid={buildResp.lastValidBlockHeight} txB64Len={SafeLen(buildResp.transaction)}");

            if (loadingSpinner != null) loadingSpinner.Show("Signing and sending...");

            Transaction tx;
            try
            {
                byte[] txBytes = Convert.FromBase64String(buildResp.transaction);
                VLog($"Borrow tx base64 decoded, bytes={txBytes.Length}");
                tx = Transaction.Deserialize(txBytes);
                VLog("Borrow tx deserialized");
            }
            catch (Exception ex)
            {
                if (loadingSpinner != null) loadingSpinner.Hide();
                Debug.LogError("[KaminoAPI] Borrow invalid transaction data: " + ex.Message);
                if (borrowModal != null) borrowModal.ShowError("Invalid transaction data.");
                yield break;
            }

            var txBlockhash = TryGetRecentBlockhash(tx);
            if (!string.IsNullOrEmpty(txBlockhash))
            {
                if (!string.IsNullOrEmpty(buildResp.blockhash) && txBlockhash != buildResp.blockhash)
                {
                    Debug.LogWarning($"[KaminoAPI] Borrow tx.RecentBlockhash mismatch. Tx={txBlockhash} Server={buildResp.blockhash}");
                }
            }
            else
            {
                VLog("Borrow tx: RecentBlockhash not readable from tx object; skipping check.");
            }

            var chosenRpc = !string.IsNullOrEmpty(buildResp.rpcUrl)
                ? Solana.Unity.Rpc.ClientFactory.GetClient(buildResp.rpcUrl)
                : Web3.Rpc;
            VLog($"RPC endpoint (chosen): {(chosenRpc?.NodeAddress != null ? chosenRpc.NodeAddress.ToString() : "<null>")}");

            var signTask = Web3.Wallet.SignTransaction(tx);
            while (!signTask.IsCompleted) yield return null;

            if (signTask.IsFaulted || signTask.Result == null)
            {
                if (loadingSpinner != null) loadingSpinner.Hide();
                Debug.LogError($"[KaminoAPI] Borrow sign task faulted. Endpoint={(chosenRpc?.NodeAddress != null ? chosenRpc.NodeAddress.ToString() : "<null>")} Exception={FlattenException(signTask.Exception)}");
                if (borrowModal != null) borrowModal.ShowError("Failed to sign transaction. See console for details.");
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
                Debug.LogError("[KaminoAPI] Borrow serialize error: " + ex.Message);
                if (borrowModal != null) borrowModal.ShowError("Failed to serialize transaction.");
                yield break;
            }

            // Send via JSON-RPC with minContextSlot & preflight=confirmed
            VLog($"Sending via RPC: {(chosenRpc?.NodeAddress != null ? chosenRpc.NodeAddress.ToString() : "<null>")} payloadLen={signedB64.Length}");

            bool sendOk = false;
            string signature = null;
            string rawRpcResponse = null;

            yield return StartCoroutine(SendTransactionJsonRpc(
                chosenRpc,
                signedB64,
                buildResp.rpcUrl,         // use server rpcUrl to avoid drift
                buildResp.minContextSlot, // pass minContextSlot if provided
                Commitment.Confirmed,
                false,
                (ok, sig, raw) => { sendOk = ok; signature = sig; rawRpcResponse = raw; }
            ));

            if (!sendOk || string.IsNullOrEmpty(signature))
            {
                if (loadingSpinner != null) loadingSpinner.Hide();
                string endpoint = chosenRpc?.NodeAddress != null ? chosenRpc.NodeAddress.ToString() : "<null>";
                Debug.LogError($"[KaminoAPI] Borrow send failed. Endpoint={endpoint}\nRaw RPC: {Truncate(rawRpcResponse, 1200)}");
                if (borrowModal != null)
                    borrowModal.ShowError("Failed to send transaction: see console for RPC error.");
                yield break;
            }

            VLog($"Borrow sent, signature={signature}");

            if (loadingSpinner != null) loadingSpinner.Show("Confirming transaction...");
            yield return StartCoroutine(WaitForConfirmation(chosenRpc, signature, 45f));
            VLog("Borrow confirmation finished (either confirmed or timed out)");

            if (loadingSpinner != null) loadingSpinner.Hide();

            if (borrowModal != null)
            {
                // Borrow modal API uses SetError(message, isError, isSuccess)
                borrowModal.SetError("Borrow successful! Your tokens have been borrowed.", false, true);
            }

            // Refresh capacity so the UI reflects the new borrowed amount
            FetchUserBorrowingCapacity();
        }
    }

    // Wraps a top-level array or an object with "positions" into a class JsonUtility can parse
    [Serializable]
    private class UserPositionsWrapper
    {
        public List<UserPosition> positions;
    }

    // Inspector-friendly: no params, resolves wallet at runtime
    public void OnViewActivityClicked()
    {
        string wallet = WalletManager.instance != null ? WalletManager.instance.walletAddress : null;
        if (string.IsNullOrEmpty(wallet))
        {
            if (activityModal != null)
            {
                activityModal.gameObject.SetActive(true);
                activityModal.SetStatus("Connect your wallet to view activity.", true);
            }
            else
            {
                Debug.LogWarning("[KaminoAPI] No wallet connected; cannot show activity.");
            }
            return;
        }

        if (loadingSpinner != null) loadingSpinner.Show("Fetching your activity...");
        FetchUserActivity(positions =>
        {
            if (loadingSpinner != null) loadingSpinner.Hide();
            if (activityModal != null) activityModal.Show(positions ?? new List<UserPosition>());
        });
    }

    // Convenience overload: auto-resolve wallet and fetch
    public void FetchUserActivity(Action<List<UserPosition>> callback)
    {
        string wallet = WalletManager.instance != null ? WalletManager.instance.walletAddress : null;
        if (string.IsNullOrEmpty(wallet))
        {
            Debug.LogWarning("[KaminoAPI] FetchUserActivity: no wallet connected.");
            callback?.Invoke(new List<UserPosition>());
            return;
        }
        StartCoroutine(FetchUserActivityCoroutine(wallet, callback));
    }

    private IEnumerator FetchUserActivityCoroutine(string wallet, Action<List<UserPosition>> callback)
    {
        if (string.IsNullOrEmpty(wallet))
        {
            Debug.LogWarning("[KaminoAPI] FetchUserActivity aborted: wallet is null/empty");
            callback?.Invoke(new List<UserPosition>());
            yield break;
        }

        string url = string.Format(ActivityUrl, wallet);
        using (var request = UnityWebRequest.Get(url))
        {
            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"[KaminoAPI] FetchUserActivity failed: code={request.responseCode} err={request.error}");
                callback?.Invoke(new List<UserPosition>());
                yield break;
            }

            var json = request.downloadHandler.text;
            VLog($"Activity response len={SafeLen(json)}");

            // Try parse as object with positions or as top-level array
            List<UserPosition> positions = null;
            try
            {
                if (!string.IsNullOrEmpty(json))
                {
                    if (json.TrimStart().StartsWith("["))
                    {
                        var wrapped = $"{{\"positions\":{json}}}";
                        var w = JsonUtility.FromJson<UserPositionsWrapper>(wrapped);
                        positions = w?.positions;
                    }
                    else
                    {
                        var w = JsonUtility.FromJson<UserPositionsWrapper>(json);
                        positions = w?.positions;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError("[KaminoAPI] Failed to parse user activity: " + ex.Message);
            }

            callback?.Invoke(positions ?? new List<UserPosition>());
        }
    }

    // Extracts a Custom Anchor error code and instruction index (if present) from raw RPC JSON.
    private static bool TryExtractCustomError(string rawRpc, out int customCode, out int instructionIndex)
    {
        customCode = -1;
        instructionIndex = -1;
        if (string.IsNullOrEmpty(rawRpc)) return false;
        try
        {
            // Find "Custom":<digits>
            int cpos = rawRpc.IndexOf("\"Custom\":", StringComparison.Ordinal);
            if (cpos >= 0)
            {
                int start = cpos + 9;
                int end = start;
                while (end < rawRpc.Length && char.IsDigit(rawRpc[end])) end++;
                if (int.TryParse(rawRpc.Substring(start, end - start), NumberStyles.Integer, CultureInfo.InvariantCulture, out var code))
                    customCode = code;
            }
            // Find "InstructionError":[<idx>,
            int ipos = rawRpc.IndexOf("\"InstructionError\":[", StringComparison.Ordinal);
            if (ipos >= 0)
            {
                int s = ipos + "\"InstructionError\":[".Length;
                int e = s;
                while (e < rawRpc.Length && char.IsDigit(rawRpc[e])) e++;
                int.TryParse(rawRpc.Substring(s, e - s), NumberStyles.Integer, CultureInfo.InvariantCulture, out instructionIndex);
            }
            return customCode >= 0;
        }
        catch { return false; }
    }
}