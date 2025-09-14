using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;
using Solana.Unity.SDK;
using Solana.Unity.Rpc;
using Solana.Unity.Rpc.Models;
using Solana.Unity.Rpc.Types;

public class CitrusClientBorrowManager : MonoBehaviour
{
    [Header("Events")]
    public UnityEvent OnBorrowStarted;
    public UnityEvent<string> OnBorrowSignature;
    public UnityEvent<string> OnBorrowFailed;

    [Header("Settings")]
    public float confirmationTimeoutSeconds = 45f;

    [SerializeField] private bool verboseLogging = false;

    // Correlate one borrow attempt end-to-end
    private string currentBorrowId;
    private float borrowStartTime;
    private float tAfterBuild;
    private float tAfterSigning;
    private string lastEndpoint;
    private string lastRawRpcResponse;

    private void V(string msg)
    {
        if (verboseLogging)
        {
            var id = string.IsNullOrEmpty(currentBorrowId) ? "-" : currentBorrowId;
            Debug.Log($"[CitrusClientBorrow][{id}] {msg}");
        }
    }

#if !UNITY_WEBGL || UNITY_EDITOR
    [Header("Editor Fallback")]
    [Tooltip("If enabled in Editor/Standalone, builds the tx via HTTP to your Svelte dev route instead of JSLib.")]
    [SerializeField] private bool useEditorHttpFallback = true;

    [Tooltip("POST endpoint that returns { transaction, blockhash?, lastValidBlockHeight?, rpcUrl?, minContextSlot? }")]
    [SerializeField] private string editorBuildBorrowUrl = "http://localhost:5173/api/citrus/build-borrow";
#endif

#if UNITY_EDITOR
    [Header("Editor Mock")]
    [Tooltip("If enabled in Editor, bypasses HTTP/JSLib and feeds a mock build response JSON.")]
    [SerializeField] private bool useEditorMockBuild = false;

    [Tooltip("Mock JSON for OnBorrowBuildSuccess. Leave empty to auto-generate a minimal payload.")]
    [TextArea(3, 8)] [SerializeField] private string editorMockBuildJson = "";

    [Tooltip("If enabled with mock build, skip signing/sending and simulate success signature immediately.")]
    [SerializeField] private bool editorMockSimulateSignature = false;
#endif

#if UNITY_WEBGL && !UNITY_EDITOR
    [DllImport("__Internal")] private static extern void Citrus_BuildBorrowTx(string loanAccount, string nftMint, string borrower);
#else
    private static void Citrus_BuildBorrowTx(string loanAccount, string nftMint, string borrower)
    {
        Debug.LogWarning("[CitrusClientBorrow] Citrus_BuildBorrowTx is only available in WebGL builds.");
    }
#endif

    [Serializable]
    private class BuildTxResponse
    {
        public string transaction;
        public string blockhash;
        public string lastValidBlockHeight;
        public string error;
        public string rpcUrl;
        public string minContextSlot;
    }

#if !UNITY_WEBGL || UNITY_EDITOR
    [Serializable]
    private class BuildBorrowReq
    {
        public string loanAccount;
        public string nftMint;
        public string borrower;
    }
#endif

    [Serializable] private class RpcError { public int code; public string message; public string data; }
    [Serializable] private class RpcRespString { public string jsonrpc; public string id; public string result; public RpcError error; }
    [Serializable] private class RpcErrorData { public string[] logs; public object err; }
    [Serializable] private class RpcErrWithData { public int code; public string message; public RpcErrorData data; }
    [Serializable] private class RpcRespWithLogs { public string jsonrpc; public string id; public RpcErrWithData error; }

    private static string Truncate(string s, int max) => string.IsNullOrEmpty(s) ? "<null>" : (s.Length <= max ? s : s.Substring(0, max));
    // Optionally bump a wider default for RPC logs:
    private const int RpcLogMax = 8000;

    public void BeginBorrow(string loanAccount, string nftMint)
    {
        var borrower = WalletManager.instance != null ? WalletManager.instance.walletAddress : null;
        if (string.IsNullOrEmpty(borrower))
        {
            var msg = "[CitrusClientBorrow] No wallet connected.";
            Debug.LogError(msg);
            OnBorrowFailed?.Invoke(msg);
            return;
        }

        Debug.Log($"[CitrusClientBorrow] BeginBorrow loanAccount={loanAccount} nftMint={nftMint} borrower={borrower}");
        OnBorrowStarted?.Invoke();

#if UNITY_WEBGL && !UNITY_EDITOR
        Citrus_BuildBorrowTx(loanAccount, nftMint, borrower);
#else
#if UNITY_EDITOR
        if (useEditorMockBuild)
        {
            // If not provided, auto-generate a minimal payload. The base64 decodes, but won't be a valid tx.
            var json = string.IsNullOrEmpty(editorMockBuildJson)
                ? "{\"transaction\":\"bm90X2Jhc2U2NA==\",\"blockhash\":\"MOCK\",\"lastValidBlockHeight\":\"0\",\"rpcUrl\":\"https://api.mainnet-beta.solana.com\",\"minContextSlot\":\"0\"}"
                : editorMockBuildJson;
            Debug.Log("[CitrusClientBorrow] Editor mock: feeding mocked build JSON");
            OnBorrowBuildSuccess(json);
            return;
        }
#endif
        if (useEditorHttpFallback && !string.IsNullOrEmpty(editorBuildBorrowUrl))
        {
            StartCoroutine(EditorBuildBorrowTxCoroutine(loanAccount, nftMint, borrower));
        }
        else
        {
            Debug.LogWarning("[CitrusClientBorrow] Editor fallback disabled; build requires WebGL bridge.");
        }
#endif
    }

#if !UNITY_WEBGL || UNITY_EDITOR
    private IEnumerator EditorBuildBorrowTxCoroutine(string loanAccount, string nftMint, string borrower)
    {
        var payload = new BuildBorrowReq { loanAccount = loanAccount, nftMint = nftMint, borrower = borrower };
        var json = JsonUtility.ToJson(payload);

        using (var uwr = new UnityWebRequest(editorBuildBorrowUrl, "POST"))
        {
            uwr.uploadHandler = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(json));
            uwr.downloadHandler = new DownloadHandlerBuffer();
            uwr.SetRequestHeader("Content-Type", "application/json");
            yield return uwr.SendWebRequest();

            if (uwr.result == UnityWebRequest.Result.Success)
            {
                OnBorrowBuildSuccess(uwr.downloadHandler.text);
            }
            else
            {
                var raw = uwr.downloadHandler != null ? uwr.downloadHandler.text : null;
                OnBorrowBuildError($"HTTP {uwr.responseCode} {uwr.error}. Raw: {Truncate(raw, 800)}");
            }
        }
    }
#endif

    // Called from JS or Editor fallback
    public void OnBorrowBuildSuccess(string json)
    {
        tAfterBuild = Time.realtimeSinceStartup;
        V($"Build response received after {(tAfterBuild - borrowStartTime):F2}s. jsonLen={ (json != null ? json.Length : 0) }");
        Debug.Log($"[CitrusClientBorrow] OnBorrowBuildSuccess len={ (json != null ? json.Length : 0) }");
        BuildTxResponse build = null;
        try { build = JsonUtility.FromJson<BuildTxResponse>(json); }
        catch (Exception e) { OnBorrowFailed?.Invoke("Build parse error: " + e.Message); return; }

        if (build == null || !string.IsNullOrEmpty(build.error) || string.IsNullOrEmpty(build.transaction))
        {
            var err = build != null && !string.IsNullOrEmpty(build.error) ? build.error : "Invalid build response";
            OnBorrowFailed?.Invoke(err);
            return;
        }

        V($"BuildTx meta: txB64Len={build.transaction?.Length ?? 0}, rpcUrl={build.rpcUrl ?? "<null>"}," +
          $" blockhash={build.blockhash ?? "<null>"} lastValidBlockHeight={build.lastValidBlockHeight ?? "<null>"} minContextSlot={build.minContextSlot ?? "<null>"}");

#if UNITY_EDITOR
        if (useEditorMockBuild && editorMockSimulateSignature)
        {
            Debug.Log("[CitrusClientBorrow] Editor mock: simulating signature without signing/sending.");
            OnBorrowSignature?.Invoke("MOCK_SIGNATURE_EDITOR");
            return;
        }
#endif
        StartCoroutine(SignSendConfirm(build));
    }

    // Called from JS or Editor fallback
    public void OnBorrowBuildError(string errorMessage)
    {
        if (string.IsNullOrEmpty(errorMessage)) errorMessage = "Unknown build error";
        Debug.LogError("[CitrusClientBorrow] OnBorrowBuildError: " + errorMessage);
        OnBorrowFailed?.Invoke(errorMessage);
    }

    private IEnumerator SignSendConfirm(BuildTxResponse build)
    {
        // Deserialize unsigned tx (legacy or versioned)
        byte[] txBytes = null;
        try
        {
            txBytes = Convert.FromBase64String(build.transaction);
            if (txBytes == null || txBytes.Length == 0) throw new Exception("Empty transaction payload");
        }
        catch (Exception ex) { OnBorrowFailed?.Invoke("Invalid base64 transaction: " + ex.Message); yield break; }

        bool isVersioned = (txBytes[0] & 0x80) != 0;
        V($"Signing… txBytes={txBytes.Length} isVersioned={isVersioned}");

        string signedB64 = null;

        if (isVersioned)
        {
            VersionedTransaction vtx = null;
            try { vtx = VersionedTransaction.Deserialize(txBytes); }
            catch (Exception ex) { OnBorrowFailed?.Invoke("Failed to parse versioned tx: " + ex.Message); yield break; }

            var signTaskV = Web3.Wallet.SignTransaction(vtx);
            while (!signTaskV.IsCompleted) yield return null;
            if (signTaskV.IsFaulted || signTaskV.Result == null) { OnBorrowFailed?.Invoke("Wallet signing failed (versioned)"); yield break; }
            try { signedB64 = Convert.ToBase64String(signTaskV.Result.Serialize()); }
            catch (Exception ex) { OnBorrowFailed?.Invoke("Serialize signed versioned tx failed: " + ex.Message); yield break; }
        }
        else
        {
            Transaction ltx = null;
            try { ltx = Transaction.Deserialize(txBytes); }
            catch (Exception ex) { OnBorrowFailed?.Invoke("Failed to parse legacy tx: " + ex.Message); yield break; }

            var signTaskL = Web3.Wallet.SignTransaction(ltx);
            while (!signTaskL.IsCompleted) yield return null;
            if (signTaskL.IsFaulted || signTaskL.Result == null) { OnBorrowFailed?.Invoke("Wallet signing failed (legacy)"); yield break; }
            try { signedB64 = Convert.ToBase64String(signTaskL.Result.Serialize()); }
            catch (Exception ex) { OnBorrowFailed?.Invoke("Serialize signed legacy tx failed: " + ex.Message); yield break; }
        }

        tAfterSigning = Time.realtimeSinceStartup;
        V($"Signing done in {(tAfterSigning - tAfterBuild):F2}s. signedLen={signedB64?.Length ?? 0}");

        var rpc = !string.IsNullOrEmpty(build.rpcUrl) ? ClientFactory.GetClient(build.rpcUrl) : Web3.Rpc;
        lastEndpoint = !string.IsNullOrEmpty(build.rpcUrl)
            ? build.rpcUrl
            : (rpc?.NodeAddress != null ? rpc.NodeAddress.ToString() : null);

        bool sendOk = false;
        string signature = null;
        string rawRpcResponse = null;

        V($"Sending… endpoint={lastEndpoint ?? "<null>"} commitment=confirmed skipPreflight=false minContextSlot={build.minContextSlot ?? "<null>"}");

        yield return StartCoroutine(SendTransactionJsonRpc(
            rpc,
            signedB64,
            build.rpcUrl,
            build.minContextSlot,
            Commitment.Confirmed,
            false,
            (ok, sig, raw) => { sendOk = ok; signature = sig; rawRpcResponse = raw; }
        ));

        lastRawRpcResponse = rawRpcResponse;

        if (!sendOk || string.IsNullOrEmpty(signature))
        {
            var endpoint = rpc?.NodeAddress != null ? rpc.NodeAddress.ToString() : "<null>";
            var msg = $"Send failed. Endpoint={endpoint}. Raw RPC: {Truncate(rawRpcResponse, 1200)}";
            Debug.LogError("[CitrusClientBorrow] " + msg);
            OnBorrowFailed?.Invoke(msg);
            yield break;
        }

        Debug.Log("[CitrusClientBorrow] Sent signature=" + signature);
        V($"Send done in {(Time.realtimeSinceStartup - tAfterSigning):F2}s");
        OnBorrowSignature?.Invoke(signature);

        V("Waiting for confirmation…");
        yield return StartCoroutine(WaitForConfirmation(rpc, signature, confirmationTimeoutSeconds));
        V($"Confirmation finished after {(Time.realtimeSinceStartup - borrowStartTime):F2}s total");
        Debug.Log("[CitrusClientBorrow] Confirmation finished (confirmed or timeout)");
    }

    private IEnumerator SendTransactionJsonRpc(IRpcClient rpc, string signedB64, string rpcUrl, string minContextSlot,
        Commitment preflightCommitment, bool skipPreflight,
        Action<bool, string, string> callback)
    {
        string endpoint = !string.IsNullOrEmpty(rpcUrl)
            ? rpcUrl
            : (rpc?.NodeAddress != null ? rpc.NodeAddress.ToString() : null);

        if (string.IsNullOrEmpty(endpoint))
        {
            callback?.Invoke(false, null, "No RPC endpoint available.");
            yield break;
        }

        ulong mcs = 0UL;
        bool haveMcs = !string.IsNullOrEmpty(minContextSlot) && ulong.TryParse(minContextSlot, out mcs);

        string cfg = haveMcs
            ? $"{{\"skipPreflight\":{skipPreflight.ToString().ToLowerInvariant()},\"preflightCommitment\":\"{preflightCommitment.ToString().ToLowerInvariant()}\",\"encoding\":\"base64\",\"minContextSlot\":{mcs}}}"
            : $"{{\"skipPreflight\":{skipPreflight.ToString().ToLowerInvariant()},\"preflightCommitment\":\"{preflightCommitment.ToString().ToLowerInvariant()}\",\"encoding\":\"base64\"}}";

        string payload = $"{{\"jsonrpc\":\"2.0\",\"id\":1,\"method\":\"sendTransaction\",\"params\":[\"{signedB64}\",{cfg}]}}";

        using (var uwr = new UnityWebRequest(endpoint, "POST"))
        {
            byte[] body = System.Text.Encoding.UTF8.GetBytes(payload);
            uwr.uploadHandler = new UploadHandlerRaw(body);
            uwr.downloadHandler = new DownloadHandlerBuffer();
            uwr.SetRequestHeader("Content-Type", "application/json");

            V($"POST {endpoint} bodyLen={body.Length}");
            yield return uwr.SendWebRequest();

            string raw = uwr.downloadHandler != null ? uwr.downloadHandler.text : null;
            lastEndpoint = endpoint;
            lastRawRpcResponse = raw;

            if (uwr.result != UnityWebRequest.Result.Success)
            {
                callback?.Invoke(false, null, $"HTTP {uwr.responseCode} {uwr.error}. Raw: {Truncate(raw, RpcLogMax)}");
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
                // Try to parse Anchor logs from error.data
                string msg = resp?.error != null ? $"{resp.error.code}: {resp.error.message}" : "Unknown RPC error";

                string logsJoined = null;
                try
                {
                    var respWithLogs = JsonUtility.FromJson<RpcRespWithLogs>(raw);
                    if (respWithLogs?.error?.data?.logs != null && respWithLogs.error.data.logs.Length > 0)
                    {
                        var joined = string.Join("\n", respWithLogs.error.data.logs);
                        logsJoined = Truncate(joined, RpcLogMax);
                    }
                }
                catch { /* ignore parse errors */ }

                string final = logsJoined != null
                    ? $"RPC error: {msg}. Logs:\n{logsJoined}"
                    : $"RPC error: {msg}. Raw: {Truncate(raw, RpcLogMax)}";

                callback?.Invoke(false, null, final);
            }
        }
    }

    private IEnumerator WaitForConfirmation(IRpcClient rpc, string signature, float timeoutSeconds)
    {
        float elapsed = 0f;
        const float step = 0.8f;
        string lastSeenStatus = null;
        V($"Polling signature={signature} for up to {timeoutSeconds:F1}s");

        while (elapsed < timeoutSeconds)
        {
            var statusTask = rpc.GetSignatureStatusesAsync(new List<string> { signature }, false);
            while (!statusTask.IsCompleted) yield return null;

            var rr = statusTask.Result;
            if (rr != null && rr.WasSuccessful && rr.Result != null && rr.Result.Value != null && rr.Result.Value.Count > 0)
            {
                var info = rr.Result.Value[0];
                var status = info != null ? info.ConfirmationStatus : null;

                if (status != lastSeenStatus && verboseLogging)
                {
                    V($"Status change: {lastSeenStatus ?? "<null>"} -> {status ?? "<null>"} (t={elapsed:F1}s)");
                    lastSeenStatus = status;
                }

                if (info != null && (status == "confirmed" || status == "finalized"))
                {
                    Debug.Log($"[CitrusClientBorrow] Confirmed signature={signature} status={info.ConfirmationStatus}");
                    yield break;
                }
            }

            yield return new WaitForSeconds(step);
            elapsed += step;
        }

        Debug.LogWarning("[CitrusClientBorrow] Confirmation timeout for " + signature);
    }

#if UNITY_EDITOR
    [ContextMenu("Citrus/DEBUG: Mock Build Success")]
    private void DebugContext_MockBuildSuccess()
    {
        var json = string.IsNullOrEmpty(editorMockBuildJson)
            ? "{\"transaction\":\"bm90X2Jhc2U2NA==\",\"blockhash\":\"MOCK\",\"lastValidBlockHeight\":\"0\",\"rpcUrl\":\"https://api.mainnet-beta.solana.com\",\"minContextSlot\":\"0\"}"
            : editorMockBuildJson;
        OnBorrowBuildSuccess(json);
    }

    [ContextMenu("Citrus/DEBUG: Mock Build Error")]
    private void DebugContext_MockBuildError()
    {
        OnBorrowBuildError("Mock build error (editor)");
    }

    [ContextMenu("Citrus/DEBUG: Toggle Verbose Logging")]
    private void Debug_ToggleVerboseLogging()
    {
        verboseLogging = !verboseLogging;
        Debug.Log("[CitrusClientBorrow] VerboseLogging=" + verboseLogging);
    }

    [ContextMenu("Citrus/DEBUG: Copy Last Raw RPC Response")]
    private void Debug_CopyLastRpc()
    {
        GUIUtility.systemCopyBuffer = string.IsNullOrEmpty(lastRawRpcResponse) ? "" : lastRawRpcResponse;
        Debug.Log("[CitrusClientBorrow] Copied lastRawRpcResponse to clipboard (" + (lastRawRpcResponse?.Length ?? 0) + " chars)");
    }
#endif
}