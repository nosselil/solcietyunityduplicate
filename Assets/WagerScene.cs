using UnityEngine;
using TMPro;
using Solana.Unity.Programs;
using Solana.Unity.Rpc.Builders;
using Solana.Unity.Wallet;
using System.Threading.Tasks;
using Solana.Unity.Rpc;
using Solana.Unity.Wallet.Bip39;
using Solana.Unity.SDK;
using Solana.Unity.Rpc.Types;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine.UI;
using Solana.Unity.SDK.Nft;
using System.Collections;
using UnityEngine.SceneManagement;
using System;

public class WagerScene : MonoBehaviour
{
    public static WagerScene instance;
    public TMP_Text walletAddress;
    public TMP_Text balance;
    public TMP_Text nftNames;
    public TMP_Text wagerAmountTxt;
    public TMP_Text transactionStatusTxt;
    public RawImage nftImg;

    [Header("Solana Settings")]
    public Cluster network; // Change to DevNet if testing

    private IRpcClient rpcClient;
    private Wallet userWallet;
    private Account senderAccount;
    private Wallet npcWallet;
    private Account npcAccount;
    private readonly Dictionary<string, Texture2D> imageCache = new Dictionary<string, Texture2D>();
    private bool _isLoadingTokens = false;
    float wagerAmount;

    [Header("Loading UI")]
    public GameObject loadingPanel;
    public Text loadingPanelText;

    private void Awake()
    {
        if (instance == null) instance = this;
    }

    void Start()
    {
        wagerAmount = WalletManager.instance.wagerAmount;
        walletAddress.text = WalletManager.instance.walletAddress;
        wagerAmountTxt.text = "" + (wagerAmount * 2).ToString();
        network = WalletManager.instance.cluster;

        rpcClient = ClientFactory.GetClient(NftManager.instance.rpcUrl);
        Debug.Log("✅ RPC Client Initialized: " + network);

        // Branch based on wallet type:
        if (Web3.Instance.WalletBase.Mnemonic != null)
        {
            // In-game wallet flow: create wallet from mnemonic.
            string mnemonic = Web3.Instance.WalletBase.Mnemonic.ToString();
            userWallet = new Wallet(mnemonic, WordList.English);
            senderAccount = userWallet.GetAccount(0);
        }
        else
        {
            // External wallet flow: use the externally provided account.
            senderAccount = Web3.Instance.WalletBase.Account;
        }

        npcWallet = new Wallet(WalletManager.instance.npcMnemonic, WordList.English);
        npcAccount = npcWallet.GetAccount(0);

        UpdateBalance();
        FetchUserNFTs().Forget();
    }

    public async void UpdateBalance()
    {
        WalletManager.instance.updateData();
        // Uncomment and update as needed:
        // balance.text = "Balance: " + WalletManager.instance.Balance;
    }

    #region NFT
    private async UniTask FetchUserNFTs()
    {
        if (_isLoadingTokens) return;
        _isLoadingTokens = true;

        var tokens = await Web3.Wallet.GetTokenAccounts(Commitment.Processed);
        if (tokens == null || tokens.Length == 0)
        {
            nftNames.text = "No NFTs found.";
            _isLoadingTokens = false;
            return;
        }

        List<string> nftList = new List<string>();

        foreach (var token in tokens)
        {
            var info = token.Account.Data.Parsed.Info;
            if (info.TokenAmount.Decimals == 0 && info.TokenAmount.AmountUlong == 1)
            {
                await LoadNFT(info.Mint, nftList);
            }
        }

        nftNames.text = "NFTs: " + string.Join(", ", nftList);
        _isLoadingTokens = false;
    }

    private async UniTask LoadNFT(string mint, List<string> nftList)
    {
        var nftData = await Nft.TryGetNftData(
            mint,
            Web3.Instance.WalletBase.ActiveRpcClient,
            true,
            256,
            true,
            Commitment.Processed
        );

        if (nftData == null)
        {
            Debug.LogError($"Failed to load NFT data for mint {mint}.");
            return;
        }

        string nftName = nftData.metaplexData.data.offchainData.name;
        Texture2D nftTexture = nftData.metaplexData.nftImage?.file;
        if (nftTexture == null)
        {
            Debug.LogError($"No image found in NFT metadata for mint {mint}");
            return;
        }

        if (!imageCache.ContainsKey(mint))
        {
            imageCache[mint] = nftTexture;
        }

        // Optionally display one of the NFT images:
        // nftImg.texture = imageCache[mint];
        nftList.Add(nftName);
        Debug.Log($"Displayed NFT: {nftName} (Mint={mint}).");
    }
    #endregion

    #region SolTransfer
    public async void WinUser()
    {
        loadingPanel.SetActive(true);
        loadingPanelText.text = "Please wait while we process your transaction...";

        // Branch based on wallet type:
        Account playerAccount;
        if (Web3.Instance.WalletBase.Mnemonic != null)
        {
            // In-game wallet flow: create wallet from mnemonic.
            string mnemonic = Web3.Instance.WalletBase.Mnemonic.ToString();
            var playerWallet = new Wallet(mnemonic, WordList.English);
            playerAccount = playerWallet.GetAccount(0);
        }
        else
        {
            // External wallet flow: use the externally provided account.
            playerAccount = Web3.Instance.WalletBase.Account;
        }

        // Await the SOL transfer (wagerAmount * 2) from npcAccount to playerAccount.
        bool transferSuccess = await TransferSol2(npcAccount, playerAccount, wagerAmount * 2);

        if (transferSuccess)
        {
            transactionStatusTxt.text = "✅ User Won! Wager Transferred.";
            UpdateBalance();
            // Wait for 0.5 seconds (UniTask.Delay is WebGL-friendly)
            await UniTask.Delay(500);
            SceneManager.LoadScene("MainHub");
        }
        else
        {
            transactionStatusTxt.text = "❌ Transfer Failed";
        }

        loadingPanel.SetActive(false);
    }

    public async UniTask<bool> TransferSol2(Account npcAccount, Account playerAccount, float amountSol)
    {
        try
        {
            // Get a recent blockhash for the transaction.
            var transferBlockHash = await rpcClient.GetLatestBlockHashAsync();
            if (!transferBlockHash.WasSuccessful)
            {
                Debug.Log($"❌ Failed to get blockhash: {transferBlockHash.Reason}");
                return false;
            }

            // Build the SOL transfer transaction from npcAccount to playerAccount.
            // (In this case, the NPC account always uses the mnemonic-based wallet.)
            byte[] solTransferTx = new TransactionBuilder()
                .SetFeePayer(npcAccount)
                .SetRecentBlockHash(transferBlockHash.Result.Value.Blockhash)
                .AddInstruction(SystemProgram.Transfer(
                    npcAccount.PublicKey,
                    playerAccount.PublicKey,
                    (ulong)(amountSol * 1000000000)
                ))
                .Build(npcAccount);

            // Send the transaction.
            var transferResult = await rpcClient.SendTransactionAsync(solTransferTx);
            if (!transferResult.WasSuccessful)
            {
                Debug.Log($"❌ SOL Transfer failed: {transferResult.Reason}");
                return false;
            }

            Debug.Log($"✅ SOL Transfer successful! TX: {transferResult.Result}");

            // Poll for transaction confirmation.
            float timeout = 30f;      // Timeout after 30 seconds.
            float pollInterval = 2f;  // Check every 2 seconds.
            float elapsedTime = 0f;

            while (elapsedTime < timeout)
            {
                var confirmation = await rpcClient.GetTransactionAsync(transferResult.Result);
                if (confirmation.WasSuccessful && confirmation.Result != null)
                {
                    Debug.Log("✅ Transaction confirmed!");
                    return true;
                }

                await UniTask.Delay((int)(pollInterval * 1000));
                elapsedTime += pollInterval;
            }

            Debug.Log("❌ Transaction not confirmed within timeout.");
            return false;
        }
        catch (Exception ex)
        {
            Debug.Log($"❌ Error transferring SOL: {ex.Message}");
            return false;
        }
    }

    public void LooseUser()
    {
        loadingPanel.SetActive(true);
        loadingPanelText.text = "Please wait...";
        SceneManager.LoadScene("mainhub");
    }

    // Fallback transfer function using external signing if needed.
    private async Task<bool> TransferSol(Account fromAccount, string toPublicKey, float amountSol)
    {
        try
        {
            var transferBlockHash = await rpcClient.GetLatestBlockHashAsync();
            if (!transferBlockHash.WasSuccessful)
            {
                Debug.LogError("❌ Failed to get blockhash: " + transferBlockHash.Reason);
                return false;
            }

            var transaction = new TransactionBuilder()
                .SetFeePayer(fromAccount)
                .SetRecentBlockHash(transferBlockHash.Result.Value.Blockhash)
                .AddInstruction(SystemProgram.Transfer(
                    fromAccount.PublicKey,
                    new PublicKey(toPublicKey),
                    (ulong)(amountSol * 1000000000)
                ))
                .Build(fromAccount);

            var transferResult = await rpcClient.SendTransactionAsync(transaction);
            if (!transferResult.WasSuccessful)
            {
                Debug.LogError("❌ SOL Transfer failed: " + transferResult.Reason);
                return false;
            }

            Debug.Log("✅ SOL Transfer successful! TX: " + transferResult.Result);
            loadingPanel.SetActive(false);
            return true;
        }
        catch (System.Exception ex)
        {
            Debug.LogError("❌ Error transferring SOL: " + ex.Message);
            return false;
        }
    }
    #endregion
}
