//Wissem
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using TMPro;
using Newtonsoft.Json.Linq;
using Solana.Unity.Rpc;
using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using Solana.Unity.Rpc.Builders;
using Solana.Unity.SDK;
using Solana.Unity.Wallet;
using Solana.Unity.Wallet.Bip39;
using Solana.Unity.Programs;
using System.Linq;
using Org.BouncyCastle.Bcpg.OpenPgp;
using Solana.Unity.SDK.Example;
using System;
using Solana.Unity.Rpc.Models;
using Solana.Unity.Metaplex.NFT.Library;
using Solana.Unity.Metaplex.Utilities;
using Solana.Unity.Rpc.Types;

public class NftManager : MonoBehaviour
{
    #region OtherFields
    public static NftManager instance;
    public MeshRenderer BoxArtworkWorkMesh;
    public Transform GalleryContent_transform;
    public NftDisplay nftDisplayScript;
    public GameObject loadingPanel;
    public TMP_Text loadpanelTxt;

    public string mintName, mintUri;
    string newMinted_Sombre;
    #endregion


    #region Fields (Shared & from BuyNFTfromGallery)

    [Header("RPC Client Settings")]
    public Cluster cluster = Cluster.MainNet;

    [Header("NPC Wallet Mnemonic")]
    [TextArea(3, 5)]
    public string npcMnemonic = "miss gate front unique liberty gap bind choice lumber clown loan absorb";

    [Header("User & NFT Details")]
    public string userPublicKey = ""; //
    public string nftMintAddress = ""; // This can be dynamically updated

    [Header("UI Feedback (Optional)")]
    public TextMeshProUGUI feedbackTxt;

    private IRpcClient rpcClient;
    float requiredSol = 0.01f;

    #endregion

    #region Fields from NpcNftTransfer
    [Header("Reference to NFTAddress scripts (for multiple NFTs)")]
    public GameObject[] nftObjects;
    #endregion


    #region Fields from NPCNFTLoader
    [Header("NPC Meshes")]
    public MeshRenderer[] nftMeshes;

    [Header("NPC Images")]
    public RawImage[] nftImagesUi;

    [Header("Specific NFT Mint Addresses")]
    public string[] specificNFTs = new string[3]; // Assign specific mint addresses in the Inspector
    public string[] specificNFTsDEVNET = new string[3];
    public string[] specificNFTsTestNet = new string[3];

    private int nftObjectIndex = 0; // Separate index for nftObjects[]

   // [Header("NPC Wallet Address")]
   // public string npcWalletAddress;

    [Header("Helius API Key")]
    public string heliusApiKey; // Set in Inspector

    //   [Header("Network Selection")]
    //  public bool useMainnet = true;

    public string rpcUrl;
    private readonly Dictionary<string, Texture2D> imageCache = new Dictionary<string, Texture2D>();
    private int nftIndex = 0; // Tracks the current index for assignment

    private const string TokenProgramId = "TokenkegQfeZyiNwAJbNbGKPFXCWuBvf9Ss623VQ5DA";
    private const string RpcGetTokenAccountsByOwner = "getTokenAccountsByOwner";
    private const string RpcGetAsset = "getAsset";


    #endregion

    #region Unity Lifecycle Methods
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

       // WalletManager.instance.currentNetwork ==


         // Get cluster from WalletManager instance
         cluster = WalletManager.instance.cluster;
        // while for DevNet we use MagicBlock.
        if (cluster == Cluster.MainNet)
        {
            specificNFTs = specificNFTs;
            rpcUrl = "https://rpc.magicblock.app/mainnet/";
            Debug.Log("Fetching NFTs from: Sonic MainNet");

        }
        else if (cluster == Cluster.TestNet)
        {
            specificNFTs = specificNFTsTestNet;
            rpcUrl = "https://api.testnet.sonic.game";
       //     rpcUrl = "https://sonic.helius-rpc.com/";
            Debug.Log("Fetching NFTs from: Sonic TestNet");
        }
        else
        {
            specificNFTs = specificNFTsDEVNET;
            rpcUrl = "https://rpc.magicblock.app/devnet/";
            Debug.Log("Fetching NFTs from: MagicBlock DevNet");

        }

        // Initialize the RPC client (used in BuyNFTfromGallery and NpcNftTransfer)
        rpcClient = ClientFactory.GetClient(rpcUrl);
        if (rpcClient == null)
        {
            Debug.LogError("❌ Failed to initialize rpcClient! Check if ClientFactory is working.");
        }
        else
        {
            Debug.Log($"✅ rpcClient initialized with Cluster: {cluster}");
        }


        userPublicKey = WalletManager.instance.walletAddress;
        npcMnemonic = WalletManager.instance.npcMnemonic;
       
    }

    private async void Start()
    {
        // Set up the RPC URL based on the network:
        // - For MainNet and TestNet, use Sonic endpoints.
        // - For DevNet, use MagicBlock endpoint.



        await FetchNPCNFTs();

        loadingPanel.SetActive(false);


    }
    #endregion

    #region BuyNFTfromGallery Functions

    public async void BuyNft()
    {
        Debug.Log("📢 BuyNft() started...");
        loadingPanel.SetActive(true);
        loadpanelTxt.text = "Buying Nft...";
        if (WalletManager.instance == null)
        {
            Debug.LogError("❌ WalletManager.instance is NULL! Ensure WalletManager is set up.");
            return;
        }

        userPublicKey = WalletManager.instance.walletAddress;
        if (string.IsNullOrEmpty(userPublicKey))
        {
            Debug.LogError("❌ User Public Key is NULL or EMPTY!");
            return;
        }
        Debug.Log($"✅ User Public Key: {userPublicKey}");

        if (Web3.Instance?.WalletBase?.Mnemonic == null)
        {
            Debug.LogError("❌ Web3.Instance or WalletBase.Mnemonic is NULL! Ensure the user is logged in.");
            return;
        }

        string mnemonic = Web3.Instance.WalletBase.Mnemonic.ToString();
        Debug.Log($"✅ User Mnemonic: {mnemonic}");

        var playerWallet = new Wallet(mnemonic, WordList.English);
        var playerAccount = playerWallet.GetAccount(0);
        Debug.Log($"✅ Player Account: {playerAccount.PublicKey}");

        if (string.IsNullOrEmpty(nftMintAddress))
        {
            Debug.LogError("❌ NFT Mint Address is NULL or EMPTY!");
            return;
        }
        Debug.Log($"✅ NFT Mint Address: {nftMintAddress}");

        var npcWallet = new Wallet(npcMnemonic, WordList.English);
        var npcAccount = npcWallet.GetAccount(0);
        Debug.Log($"✅ NPC Wallet Address: {npcAccount.PublicKey}");

        if (rpcClient == null)
        {
            Debug.LogError("❌ rpcClient is NULL! Ensure the RPC client is initialized.");
            return;
        }

        var npcBalance = await rpcClient.GetBalanceAsync(npcAccount.PublicKey);
        if (npcBalance == null || !npcBalance.WasSuccessful)
        {
            Debug.LogError($"❌ Failed to fetch NPC balance: {npcBalance?.Reason ?? "Unknown error"}");
            return;
        }

        Debug.Log($"✅ NPC Balance: {npcBalance.Result.Value} SOL");

        if (npcBalance.Result.Value < 10005000) // 0.001 SOL + nft fee
        {
            LogFeedback("❌ NPC does not have enough SOL for transaction fees. Send at least 0.05 SOL.", true);
            return;
        }

        bool solTransferSuccess = await TransferSolToNpc(playerAccount, 0.05f);
        if (!solTransferSuccess)
        {
            loadpanelTxt.text = "SOL transfer failed!";
            loadingPanel.SetActive(false);
            LogFeedback("❌ SOL transfer failed. NFT transfer aborted.", true);
            return;
        }

   //     await TransferNft();
    }
    public async Task TransferNft()
    {
        LogFeedback("Starting NFT transfer...");
        loadpanelTxt.text = "Tranfering NFT!";
        var npcWallet = new Wallet(npcMnemonic, WordList.English);
        var npcAccount = npcWallet.GetAccount(0);

        var userPK = new PublicKey(userPublicKey);
        var mintPK = new PublicKey(nftMintAddress);

        var fromTokenAccount = AssociatedTokenAccountProgram.DeriveAssociatedTokenAccount(npcAccount.PublicKey, mintPK);
        var toTokenAccount = AssociatedTokenAccountProgram.DeriveAssociatedTokenAccount(userPK, mintPK);

        LogFeedback($"NPC ATA: {fromTokenAccount.Key}");
        LogFeedback($"User ATA: {toTokenAccount.Key}");

        var fromBalance = await rpcClient.GetTokenAccountBalanceAsync(fromTokenAccount);
        if (!fromBalance.WasSuccessful)
        {
            LogFeedback($"❌ Failed to fetch token balance: {fromBalance.Reason}", true);
            return;
        }

        int retryCount = 0;
        while (fromBalance.Result?.Value.UiAmount == null && retryCount < 5)
        {
            LogFeedback("ℹ Retrying to fetch NPC's NFT ownership...");
            await UniTask.Delay(2000);
            fromBalance = await rpcClient.GetTokenAccountBalanceAsync(fromTokenAccount);
            retryCount++;
        }
        if (fromBalance.Result?.Value.UiAmount == null)
        {
            loadpanelTxt.text = "SOL transfered!";
            LogFeedback("❌ NPC does not own this NFT. Aborting transfer.", true);
            return;
        }

        var userTokenAccountInfo = await rpcClient.GetAccountInfoAsync(toTokenAccount);
        if (userTokenAccountInfo.Result?.Value == null)
        {
            LogFeedback("ℹ User's ATA does not exist. Creating one...");

            var blockHash = await rpcClient.GetLatestBlockHashAsync();
            var createAtaTx = new TransactionBuilder()
                .SetFeePayer(npcAccount)
                .SetRecentBlockHash(blockHash.Result.Value.Blockhash)
                .AddInstruction(
                    AssociatedTokenAccountProgram.CreateAssociatedTokenAccount(
                        npcAccount.PublicKey, userPK, mintPK))
                .Build(npcAccount);

            var createResult = await rpcClient.SendTransactionAsync(createAtaTx);
            if (!createResult.WasSuccessful)
            {
                LogFeedback($"❌ Failed to create User's ATA: {createResult.Reason}", true);
                return;
            }

            LogFeedback($"✅ User's ATA created successfully! TX: {createResult.Result}");
            bool confirmed = await ConfirmTransaction(createResult.Result);
            if (!confirmed) return;
        }

        var transferBlockHash = await rpcClient.GetLatestBlockHashAsync();
        var transferTx = new TransactionBuilder()
            .SetFeePayer(npcAccount)
            .SetRecentBlockHash(transferBlockHash.Result.Value.Blockhash)
            .AddInstruction(
                TokenProgram.TransferChecked(
                    fromTokenAccount, toTokenAccount, 1UL, 0, npcAccount.PublicKey, mintPK
                )
            )
            .Build(npcAccount);

        var transferResult = await rpcClient.SendTransactionAsync(transferTx);

        if (!transferResult.WasSuccessful)
        {
            loadpanelTxt.text = "Transfer Failed...";
            loadingPanel.SetActive(false);
            LogFeedback($"❌ NFT Transfer failed: {transferResult.Reason}", true);
        }
        else
        {
            loadpanelTxt.text = "NFT Transfer successful!";
            await UniTask.Delay(1000);
            loadpanelTxt.text = "Updating Realtime Nfts!";
            nftDisplayScript.UpdateNfts();
            LogFeedback($"🎉 NFT Transfer successful! Transaction: {transferResult.Result}");
        }
    }


    public async Task<bool> TransferSolToNpc(Account playerAccount, float amountSol)
    {
        try
        {
            loadpanelTxt.text = "Transfering Sol!";
            var npcWallet = new Wallet(npcMnemonic, WordList.English);
            var npcAccount = npcWallet.GetAccount(0);

            var transferBlockHash = await rpcClient.GetLatestBlockHashAsync();
            if (!transferBlockHash.WasSuccessful)
            {
                LogFeedback($"❌ Failed to get blockhash: {transferBlockHash.Reason}", true);
                return false;
            }

            var solTransferTx = new TransactionBuilder()
                .SetFeePayer(playerAccount)
                .SetRecentBlockHash(transferBlockHash.Result.Value.Blockhash)
                .AddInstruction(SystemProgram.Transfer(
                    playerAccount.PublicKey,
                    npcAccount.PublicKey,
                    (ulong)(amountSol * 1000000000)
                ))
                .Build(playerAccount);

            var transferResult = await rpcClient.SendTransactionAsync(solTransferTx);
            if (!transferResult.WasSuccessful)
            {
                LogFeedback($"❌ SOL Transfer failed: {transferResult.Reason}", true);
                return false;
            }

            LogFeedback($"✅ SOL Transfer successful! TX: {transferResult.Result}");

            bool confirmed = await ConfirmTransaction(transferResult.Result);
            return confirmed;
        }
        catch (Exception ex)
        {
            LogFeedback($"❌ Error transferring SOL: {ex.Message}", true);
            return false;
        }
    }
    private async Task<bool> ConfirmTransaction(string txSignature, int maxRetries = 10, int delayMs = 2000)
    {
        int retryCount = 0;
        while (retryCount < maxRetries)
        {
            var confirmation = await rpcClient.GetTransactionAsync(txSignature);
            if (confirmation.WasSuccessful && confirmation.Result != null)
            {
                loadpanelTxt.text = "SOL transfered!";
                LogFeedback($"✅ Transaction {txSignature} confirmed!");
                return true;
            }
            await UniTask.Delay(delayMs);
            retryCount++;
        }

        LogFeedback($"❌ Transaction {txSignature} not confirmed after {maxRetries} retries.", true);
        return false;
    }



    private void LogFeedback(string message, bool isError = false)
    {
        if (isError)
            Debug.LogError(message);
        else
            Debug.Log(message);

        if (feedbackTxt != null)
        {
            feedbackTxt.text = message;
            feedbackTxt.color = isError ? Color.red : Color.white;
        }
    }

    #endregion

    #region MintingNFTfromGallery Functions

    public async void BuyNftMinting()
    {
        Debug.Log("📢 BuyNftMinting() started...");
        loadingPanel.SetActive(true);

        if (WalletManager.instance == null)
        {
            Debug.LogError("❌ WalletManager.instance is NULL! Ensure WalletManager is set up.");
            loadingPanel.SetActive(false);
            return;
        }

        userPublicKey = WalletManager.instance.walletAddress;
        if (string.IsNullOrEmpty(userPublicKey))
        {
            Debug.LogError("❌ User Public Key is NULL or EMPTY!");
            loadingPanel.SetActive(false);
            return;
        }
        Debug.Log($"✅ User Public Key: {userPublicKey}");

        // Use Web3 SDK for better account management
        if (Web3.Instance?.WalletBase?.Account == null)
        {
            Debug.LogError("❌ Web3.Instance or WalletBase.Account is NULL! Ensure the user is logged in.");
            loadingPanel.SetActive(false);
            return;
        }

        // Use Web3.Rpc for better RPC handling
        var web3Rpc = Web3.Rpc;
        if (web3Rpc == null)
        {
            Debug.LogError("❌ Web3.Rpc is NULL! Using fallback RPC client.");
            web3Rpc = rpcClient;
        }

        // Determine the player's signing account using Web3 SDK
        Account playerAccount;
        if (Web3.Instance.WalletBase.Mnemonic != null)
        {
            string mnemonic = Web3.Instance.WalletBase.Mnemonic.ToString();
            Debug.Log($"✅ User Mnemonic: {mnemonic}");
            var playerWallet = new Wallet(mnemonic, WordList.English);
            playerAccount = playerWallet.GetAccount(0);
            Debug.Log($"✅ Player Account (from mnemonic): {playerAccount.PublicKey}");
        }
        else
        {
            playerAccount = Web3.Instance.WalletBase.Account;
            Debug.Log($"✅ Player Account (external login): {playerAccount.PublicKey}");
        }

        // 1. Pre-fetch blockhash and rent once using Web3.Rpc
        var blockHashResult = await web3Rpc.GetLatestBlockHashAsync();
        if (!blockHashResult.WasSuccessful)
        {
            LogFeedback("❌ Failed to get latest blockhash: " + blockHashResult.Reason, true);
            loadingPanel.SetActive(false);
            return;
        }
        var blockhash = blockHashResult.Result.Value.Blockhash;

        var rentResult = await web3Rpc.GetMinimumBalanceForRentExemptionAsync(TokenProgram.MintAccountDataSize);
        if (!rentResult.WasSuccessful)
        {
            LogFeedback("❌ Failed to get minimum rent: " + rentResult.Reason, true);
            loadingPanel.SetActive(false);
            return;
        }
        ulong rentExemption = rentResult.Result;

        // 2. Transfer SOL in one go (pass blockhash for efficiency)
        bool solTransferSuccess = await TransferSolToNpcMintingOptimized(playerAccount, 0.01f, blockhash, web3Rpc);
        if (!solTransferSuccess)
        {
            LogFeedback("❌ SOL transfer failed. NFT minting aborted.", true);
            loadingPanel.SetActive(false);
            return;
        }

        // 3. Batch all mint instructions into one transaction with optimized confirmation
        await MintNftBatchedOptimized(playerAccount, blockhash, rentExemption, web3Rpc);

        loadingPanel.SetActive(false);
    }

    // Optimized SOL transfer using Web3 SDK
    public async Task<bool> TransferSolToNpcMintingOptimized(Account playerAccount, float amountSol, string blockhash, IRpcClient rpcClient)
    {
        try
        {
            var npcWallet = new Wallet(npcMnemonic, WordList.English);
            var npcAccount = npcWallet.GetAccount(0);

            var builder = new TransactionBuilder()
                .SetFeePayer(playerAccount)
                .SetRecentBlockHash(blockhash)
                .AddInstruction(
                    SystemProgram.Transfer(
                        playerAccount.PublicKey,
                        npcAccount.PublicKey,
                        (ulong)(amountSol * 1000000000)
                    )
                );

            byte[] solTransferTx = builder.Build(playerAccount);

            // Use Web3 SDK for transaction signing
            if (Web3.Instance.WalletBase.Mnemonic == null)
            {
                Transaction tx = Transaction.Deserialize(solTransferTx);
                tx = await Web3.Instance.WalletBase.SignTransaction(tx);
                solTransferTx = tx.Serialize();
            }

            string solTransferTxBase64 = Convert.ToBase64String(solTransferTx);

            var transferResult = await rpcClient.SendTransactionAsync(solTransferTxBase64);
            if (!transferResult.WasSuccessful)
            {
                LogFeedback($"❌ SOL Transfer failed: {transferResult.Reason}", true);
                return false;
            }

            LogFeedback($"✅ SOL Transfer successful! TX: {transferResult.Result}");

            // Use optimized confirmation with shorter intervals
            bool confirmed = await ConfirmTransactionOptimized(transferResult.Result, rpcClient);
            return confirmed;
        }
        catch (Exception ex)
        {
            LogFeedback($"❌ Error transferring SOL: {ex.Message}", true);
            return false;
        }
    }

    // Optimized batched minting with faster confirmation
    public async Task MintNftBatchedOptimized(Account playerAccount, string blockhash, ulong rentExemption, IRpcClient rpcClient)
    {
        LogFeedback("Starting NFT minting (optimized batched)...");

        var mint = new Account();
        var associatedTokenAccount = AssociatedTokenAccountProgram.DeriveAssociatedTokenAccount(playerAccount.PublicKey, mint.PublicKey);

        if (mintUri == null)
        {
            Debug.LogError("mintUri is Null");
            return;
        }

        var metadata = new Metadata()
        {
            name = mintName,
            symbol = "ART",
            uri = mintUri,
            sellerFeeBasisPoints = 0,
            creators = new List<Creator> { new Creator(playerAccount.PublicKey, 100, true) }
        };

        var transactionBuilder = new TransactionBuilder()
             .SetRecentBlockHash(blockhash)
             .SetFeePayer(playerAccount)
             .AddInstruction(
                  SystemProgram.CreateAccount(
                       playerAccount,
                       mint.PublicKey,
                       rentExemption,
                       TokenProgram.MintAccountDataSize,
                       TokenProgram.ProgramIdKey))
             .AddInstruction(
                  TokenProgram.InitializeMint(
                       mint.PublicKey,
                       0,
                       playerAccount,
                       playerAccount))
             .AddInstruction(
                  AssociatedTokenAccountProgram.CreateAssociatedTokenAccount(
                       playerAccount,
                       playerAccount,
                       mint.PublicKey))
             .AddInstruction(
                  TokenProgram.MintTo(
                       mint.PublicKey,
                       associatedTokenAccount,
                       1,
                       playerAccount))
             .AddInstruction(
                  MetadataProgram.CreateMetadataAccount(
                       PDALookup.FindMetadataPDA(mint),
                       mint.PublicKey,
                       playerAccount,
                       playerAccount,
                       playerAccount.PublicKey,
                       metadata,
                       TokenStandard.NonFungible,
                       true,
                       true,
                       null,
                       metadataVersion: MetadataVersion.V3))
             .AddInstruction(
                  MetadataProgram.CreateMasterEdition(
                       maxSupply: null,
                       masterEditionKey: PDALookup.FindMasterEditionPDA(mint),
                       mintKey: mint,
                       updateAuthorityKey: playerAccount,
                       mintAuthority: playerAccount,
                       payer: playerAccount,
                       metadataKey: PDALookup.FindMetadataPDA(mint),
                       version: CreateMasterEditionVersion.V3)
             );

        byte[] txBytes = transactionBuilder.Build(new List<Account> { playerAccount, mint });
        Transaction tx = Transaction.Deserialize(txBytes);

        // Use Web3 SDK for transaction signing
        if (Web3.Instance.WalletBase.Mnemonic == null)
        {
            tx = await Web3.Instance.WalletBase.SignTransaction(tx);
            txBytes = tx.Serialize();
        }
        string txBase64 = Convert.ToBase64String(txBytes);

        var mintResult = await rpcClient.SendTransactionAsync(txBase64);
        if (!mintResult.WasSuccessful)
        {
            LogFeedback("❌ NFT Minting failed: " + mintResult.Reason, true);
            return;
        }
        LogFeedback("Mint transaction sent. TX: " + mintResult.Result);

        // Use optimized confirmation with faster polling
        bool confirmed = await ConfirmTransactionOptimized(mintResult.Result, rpcClient);
        if (confirmed)
        {
            LogFeedback("🎉 NFT Minting succeeded! TX: " + mintResult.Result);
            
            // Update NFT display using Web3 SDK
            if (nftDisplayScript != null)
            {
                nftDisplayScript.UpdateNfts();
            }
        }
        else
        {
            LogFeedback("❌ NFT mint transaction not confirmed.", true);
        }
    }

    // Optimized transaction confirmation with faster polling
    private async Task<bool> ConfirmTransactionOptimized(string txSignature, IRpcClient rpcClient, int maxRetries = 10, int delayMs = 2000)
    {
        LogFeedback($"🔍 Confirming transaction: {txSignature}");
        
        for (int i = 0; i < maxRetries; i++)
        {
            var confirmation = await rpcClient.GetTransactionAsync(txSignature);
            if (confirmation.WasSuccessful && confirmation.Result != null)
            {
                LogFeedback($"✅ Transaction confirmed after {i + 1} attempts!");
                return true;
            }
            
            // Only log every 2nd attempt to reduce spam
            if (i % 2 == 0)
            {
                LogFeedback($"⏳ Transaction not confirmed yet... (attempt {i + 1}/{maxRetries})");
            }
            await UniTask.Delay(delayMs);
        }
        
        LogFeedback($"❌ Transaction not confirmed after {maxRetries} attempts.", true);
        return false;
    }



    #endregion

    #region NpcNftTransfer Functions
    PublicKey mintAuthorityPK;
    public async void TransferNfts()
    {

        userPublicKey = WalletManager.instance.walletAddress;
        loadingPanel.SetActive(true);
        loadpanelTxt.text = "Transfer Started!";

        var npcWallet = new Wallet(npcMnemonic, WordList.English);
        var npcAccount = npcWallet.GetAccount(0);


        var userPK = new PublicKey(userPublicKey);

        // ✅ **Clear and Set NFT Mint Address Manually for Testing**
        List<string> nftMintAddresses = new List<string>
    {
        newMinted_Sombre
    };

        foreach (string nftMintAddress in nftMintAddresses)
        {

            var mintPK = new PublicKey(nftMintAddress);

            var fromTokenAccount = AssociatedTokenAccountProgram.DeriveAssociatedTokenAccount(npcAccount.PublicKey, mintPK);
            var toTokenAccount = AssociatedTokenAccountProgram.DeriveAssociatedTokenAccount(userPK, mintPK);

            LogFeedback($"📌 NPC ATA: {fromTokenAccount.Key}");
            LogFeedback($"📌 User ATA: {toTokenAccount.Key}");

            var mintInfoResponse = await rpcClient.GetTokenMintInfoAsync(nftMintAddress);
            if (!mintInfoResponse.WasSuccessful || mintInfoResponse.Result.Value == null)
            {
                LogFeedback($"❌ Failed to retrieve mint info for {nftMintAddress}. Transfer aborted.", true);
                continue;
            }

            var mintAccountInfo = await rpcClient.GetAccountInfoAsync(nftMintAddress, Commitment.Confirmed, BinaryEncoding.Base64);
            if (!mintAccountInfo.WasSuccessful || mintAccountInfo.Result.Value == null)
            {
                LogFeedback($"❌ Failed to retrieve mint account info for {nftMintAddress}. Transfer aborted.", true);
                loadingPanel.SetActive(false);
                return;
            }

            // Decode raw mint account data from Base64
            byte[] mintData = Convert.FromBase64String(mintAccountInfo.Result.Value.Data[0]);

            // Ensure the mint account data is at least 36 bytes (minimum for mint authority)
            if (mintData.Length < 36)
            {
                LogFeedback($"❌ Invalid mint data length for {nftMintAddress}. Expected at least 36 bytes, but got {mintData.Length}.", true);
                loadingPanel.SetActive(false);
                return;
            }

            // Extract 32-byte Mint Authority (at offset 4)
            byte[] mintAuthBytes = mintData.Skip(4).Take(32).ToArray();
            mintAuthorityPK = mintAuthBytes.Length == 32 ? new PublicKey(mintAuthBytes) : null;


            // Check if Freeze Authority exists (it starts at byte 68, but might be missing)
            string freezeAuthority = "None";
            if (mintData.Length >= 100)  // Ensure data is long enough to contain freeze authority
            {
                byte[] freezeAuthBytes = mintData.Skip(68).Take(32).ToArray();
                if (freezeAuthBytes.Length == 32)
                {
                    freezeAuthority = new PublicKey(freezeAuthBytes).Key;
                }
            }



            if (mintAuthorityPK != null)
            {
                //            LogFeedback($"⚠️ WARNING: Mint Authority is still active for {nftMintAddress}! Found: {mintAuthorityPK.Key}", true);
            }

            // ✅ **Step 2: Thaw NFT if it's frozen**
            if (freezeAuthority != "None")
            {
                LogFeedback($"🔹 NFT is frozen, attempting to thaw...");
                var thawTx = new TransactionBuilder()
                    .SetFeePayer(npcAccount)
                    .SetRecentBlockHash((await rpcClient.GetLatestBlockHashAsync()).Result.Value.Blockhash)
                    .AddInstruction(
                        TokenProgram.ThawAccount(
                            fromTokenAccount,
                            mintPK,
                            new PublicKey(freezeAuthority),
                            TokenProgram.ProgramIdKey
                        )
                    )
                    .Build(npcAccount);

                var thawResult = await rpcClient.SendTransactionAsync(thawTx);
                if (!thawResult.WasSuccessful)
                {
                    LogFeedback($"❌ Failed to thaw NFT {nftMintAddress}: {thawResult.Reason}", true);
                    continue;
                }
                LogFeedback($"✅ Successfully thawed NFT {nftMintAddress}! TX: {thawResult.Result}");
                await UniTask.Delay(2000);
            }

            // ✅ **Step 3: Ensure NPC Owns the NFT**
            var fromBalance = await rpcClient.GetTokenAccountBalanceAsync(fromTokenAccount);
            int retryCount = 0;
            while ((fromBalance.Result?.Value.UiAmount == null || fromBalance.Result.Value.UiAmount < 1) && retryCount < 3) // Reduced from 5
            {
                LogFeedback($"ℹ Balance for NFT {nftMintAddress} is insufficient (current: {fromBalance.Result?.Value.UiAmount}). Retrying ({retryCount + 1}/3)...", true);
                await UniTask.Delay(1000); // Reduced from 2000ms
                fromBalance = await rpcClient.GetTokenAccountBalanceAsync(fromTokenAccount);
                retryCount++;
            }
            if (fromBalance.Result?.Value.UiAmount == null || fromBalance.Result.Value.UiAmount < 1)
            {
                LogFeedback($"❌ NPC does not own NFT {nftMintAddress}. Skipping transfer.", true);
                continue;
            }
            //      LogFeedback($"✅ NPC token account balance for {nftMintAddress}: {fromBalance.Result.Value.UiAmount}");

            // ✅ **Step 4: Ensure User's ATA Exists**
            var userTokenAccountInfo = await rpcClient.GetAccountInfoAsync(toTokenAccount);
            if (userTokenAccountInfo.Result?.Value == null)
            {
                LogFeedback("ℹ User's ATA does not exist. Creating one...");
                var blockHash = await rpcClient.GetLatestBlockHashAsync();
                var createAtaTx = new TransactionBuilder()
                    .SetFeePayer(npcAccount)
                    .SetRecentBlockHash(blockHash.Result.Value.Blockhash)
                    .AddInstruction(
                        AssociatedTokenAccountProgram.CreateAssociatedTokenAccount(
                            npcAccount.PublicKey, userPK, mintPK))
                    .Build(npcAccount);

                var createResult = await rpcClient.SendTransactionAsync(createAtaTx);
                if (!createResult.WasSuccessful)
                {
                    LogFeedback($"❌ Failed to create User's ATA for NFT {nftMintAddress}: {createResult.Reason}", true);
                    continue;
                }
                LogFeedback($"✅ User's ATA created successfully! TX: {createResult.Result}");

                // Wait for the transaction to be confirmed
                bool ataConfirmed = await ConfirmTransaction(createResult.Result);
                if (!ataConfirmed)
                {
                    LogFeedback("❌ User's ATA creation not confirmed. Skipping transfer.", true);
                    loadingPanel.SetActive(false);
                    return;
                }
                await UniTask.Delay(1000); // Reduced from 2000ms
            }

            // ✅ **Step 5: Fetch Latest Blockhash**
            var transferBlockHash = await rpcClient.GetLatestBlockHashAsync();
            if (!transferBlockHash.WasSuccessful)
            {
                LogFeedback($"❌ Failed to get blockhash for NFT {nftMintAddress}: {transferBlockHash.Reason}", true);
                continue;
            }

            // ✅ **Step 6: Check NPC's SOL Balance for Fees**
            var npcBalance = await rpcClient.GetBalanceAsync(npcAccount.PublicKey);
            if (npcBalance.Result.Value < 5000000) // 0.005 SOL in lamports
            {
                loadingPanel.SetActive(false);
                LogFeedback("❌ NPC does not have enough SOL for the transfer. Send at least 0.05 SOL.", true);
                return;
            }

            await UniTask.Delay(500);

            // ✅ **Step 7: Perform NFT Transfer**
            LogFeedback($"🚀 Sending NFT transfer transaction for {nftMintAddress}...");

            // Fetch account info for the source token account (fromTokenAccount)
            var sourceAccountInfo = await rpcClient.GetAccountInfoAsync(fromTokenAccount);
            if (!sourceAccountInfo.WasSuccessful || sourceAccountInfo.Result?.Value == null)
            {
                loadingPanel.SetActive(false);
                LogFeedback($"❌ Failed to retrieve source token account info for {fromTokenAccount.Key}", true);
                return;
            }

            // Fetch account info for the destination token account (toTokenAccount)
            var destAccountInfo = await rpcClient.GetAccountInfoAsync(toTokenAccount);
            if (!destAccountInfo.WasSuccessful || destAccountInfo.Result?.Value == null)
            {
                loadingPanel.SetActive(false);
                LogFeedback($"❌ Failed to retrieve destination token account info for {toTokenAccount.Key}", true);
                return;
            }

            // Check if the account is owned by the Token Program
            var sourceOwner = sourceAccountInfo.Result.Value.Owner;
            var destOwner = destAccountInfo.Result.Value.Owner;
            if (sourceOwner != TokenProgram.ProgramIdKey)
            {
                loadingPanel.SetActive(false);
                LogFeedback($"❌ Source token account {fromTokenAccount.Key} is not owned by the Token Program.", true);
                return;
            }
            if (destOwner != TokenProgram.ProgramIdKey)
            {
                loadingPanel.SetActive(false);
                LogFeedback($"❌ Destination token account {toTokenAccount.Key} is not owned by the Token Program.", true);
                return;
            }

            // Check if both accounts are associated with the correct mint (mintPK)
            byte[] sourceAccountData = Convert.FromBase64String(sourceAccountInfo.Result.Value.Data[0]);
            byte[] destAccountData = Convert.FromBase64String(destAccountInfo.Result.Value.Data[0]);

            byte[] sourceMintBytes = sourceAccountData.Take(32).ToArray();
            byte[] destMintBytes = destAccountData.Take(32).ToArray();

            PublicKey sourceMint = new PublicKey(sourceMintBytes);
            PublicKey destMint = new PublicKey(destMintBytes);

            if (sourceMint != mintPK)
            {
                loadingPanel.SetActive(false);
                LogFeedback($"❌ Source token account {fromTokenAccount.Key} is not associated with the correct mint. Expected: {mintPK}, Found: {sourceMint}", true);
                return;
            }

            if (destMint != mintPK)
            {
                loadingPanel.SetActive(false);
                LogFeedback($"❌ Destination token account {toTokenAccount.Key} is not associated with the correct mint. Expected: {mintPK}, Found: {destMint}", true);
                return;
            }

            // If all checks pass, then the accounts are valid for the transfer
            LogFeedback("✅ Both source and destination token accounts are correctly associated with the mint.");

            Debug.Log("NPC PK = "+ npcAccount.PublicKey);
            Debug.Log("mintPK = " + mintPK);

            var transferTx = new TransactionBuilder()
                .SetFeePayer(npcAccount)
                .SetRecentBlockHash(transferBlockHash.Result.Value.Blockhash)
                .AddInstruction(
                    TokenProgram.TransferChecked(
                        fromTokenAccount,     // NPC's ATA (source)
                        toTokenAccount,       // User's ATA (destination)
                        1UL,                  // Amount (1 NFT)
                        0,                    // Decimals (NFT = 0)
                        npcAccount.PublicKey, // Authority (NPC)
                        mintPK                // NFT Mint Address
                    )
                )
                .Build(npcAccount);

            var transferResult = await rpcClient.SendTransactionAsync(transferTx);
            if (!transferResult.WasSuccessful)
            {
                LogFeedback($"❌ NFT Transfer failed for {nftMintAddress}: {transferResult.Reason}", true);
                loadingPanel.SetActive(false);
                return;
            }

            LogFeedback($"🎉 NFT Transfer successful! TX: {transferResult.Result}");
            //   CallMoveFunctionOnNFT(specificNFTs[0]);
            CallMoveFunctionOnNFT("https://red-tough-wildcat-877.mypinata.cloud/ipfs/bafkreif7f2t73t5njpwpepnb7hbco4c5dl2nyuu4nbgrtpuq52r35irmxu?pinataGatewayToken=cpHZtQlPAxJ8AdYhHsEev_jKs5G0ABgFvMMXdRcP4VqBkZhXn8BD60cOXXcMoYl9");
        }

        await UniTask.Delay(5000); // Reduced from 25000ms - much faster!
        loadpanelTxt.text = "Updating NFT UI";
        nftDisplayScript.UpdateNfts();
    }

    private async Task ConfirmATACreation(PublicKey ata)
    {
        const int maxRetries = 5;
        for (int i = 0; i < maxRetries; i++)
        {
            var ataInfo = await rpcClient.GetAccountInfoAsync(ata);
            if (ataInfo.Result?.Value != null)
            {
                LogFeedback("✅ ATA confirmed");
                return;
            }
            await UniTask.Delay(3000);
        }
        throw new Exception("ATA creation confirmation timeout");
    }

    public void CallMoveFunctionOnNFT(string nftMintAddress)
    {
        // Find all NFT objects in the scene
        NFTAddress[] allNfts = FindObjectsOfType<NFTAddress>();

        // Search for the one with the correct address
        foreach (NFTAddress nft in allNfts)
        {
            if (nft.nftUri == nftMintAddress)
            {
                Debug.Log($"✅ Found NFT GameObject with address: {nftMintAddress}");
                nft.MoveToTarget(); // Call the function to move the NFT
                return;
            }
        }

        Debug.LogError($"❌ No NFT GameObject found with address: {nftMintAddress}");
    }
    #endregion

    #region NPCNFTLoader Functions

    private async UniTask FetchNPCNFTs()
    {
      

        // Fetch specific tradable NFTs first.
        Debug.Log("Fetching specific tradable NFTs...");
        foreach (var mint in specificNFTs.Where(m => !string.IsNullOrEmpty(m)))
        {
            Debug.Log($"Fetching specific NFT: {mint}");
            await FetchNFTMetadata(mint);
        }

        /*
         // Fetch other NFTs from the wallet.
         string jsonRpcRequest = $@"{{
         ""jsonrpc"": ""2.0"",
         ""id"": 1,
         ""method"": ""{RpcGetTokenAccountsByOwner}"",
         ""params"": [""{npcWalletAddress}"", {{""programId"": ""{TokenProgramId}""}}, {{""encoding"": ""jsonParsed""}}]
         }}";

         string responseText = await SendWebRequestAsync(rpcUrl, jsonRpcRequest);
         if (string.IsNullOrEmpty(responseText))
         {
             Debug.LogError("Failed to retrieve token accounts.");
             return;
         }
         Debug.Log($"NFT Data: {responseText}");
         ProcessNFTResponse(responseText);
         */
    }

    /*    private async UniTask FetchNFTMetadata(string mintAddress)
        {
            string jsonRpcRequest = $@"{{
                ""jsonrpc"": ""2.0"",
                ""id"": 1,
                ""method"": ""{RpcGetAsset}"",
                ""params"": [""{mintAddress}""]
            }}";

            string responseText = null;
            const int maxAttempts = 3;
            const int delayMs = 0;
            int attempts = 0;
            bool success = false;

            while (!success && attempts < maxAttempts)
            {
                try
                {
                    responseText = await SendWebRequestAsync(rpcUrl, jsonRpcRequest);
                    // If the response is empty, we treat it as an error.
                    if (string.IsNullOrEmpty(responseText))
                        throw new Exception("Empty response (possibly rate limited).");
                    success = true;
                }
                catch (Exception ex)
                {
                    attempts++;
                    Debug.LogError($"Error fetching NFT metadata for mint {mintAddress} (attempt {attempts}/{maxAttempts}): {ex.Message}");
                    if (attempts < maxAttempts)
                        await UniTask.Delay(delayMs);
                }
            }

            if (!success)
            {
                Debug.LogError($"Failed to fetch NFT metadata for mint {mintAddress} after {maxAttempts} attempts.");
                return;
            }

            Debug.Log($"Helius Metadata Response for {mintAddress}: {responseText}");
            await ProcessNFTMetadata(responseText, mintAddress);
        }

        private async UniTask ProcessNFTMetadata(string metadataJson, string mintAddress)
        {
            var parsedResponse = JObject.Parse(metadataJson);
            if (parsedResponse["error"] != null)
            {
                Debug.LogError($"Helius Error: {parsedResponse["error"]["message"]} : {mintAddress}");
                return;
            }

            string metadataUri = parsedResponse["result"]?["content"]?["json_uri"]?.ToString();
            if (string.IsNullOrEmpty(metadataUri))
            {
                Debug.LogError($"Metadata URI missing for {mintAddress}. Trying Metaplex...");
                return;
            }

            Debug.Log($"Metadata URI Found: {metadataUri}");
            await FetchExternalNFTMetadata(metadataUri, mintAddress);
        }
    */
        private async UniTask FetchExternalNFTMetadata(string metadataUrl, string mintAddress)
        {
            Debug.Log($"Fetching external NFT metadata from: {metadataUrl}");

            if (string.IsNullOrEmpty(metadataUrl))
            {
                Debug.LogError($"Metadata URL is empty for mint address: {mintAddress}");
                return;
            }

            // Use Pinata gateway URL
            string gatewayUrl = "https://gateway.pinata.cloud/ipfs/";
            // Extract the CID from the metadata URL
            string cid = metadataUrl.Substring(metadataUrl.LastIndexOf("/") + 1);
            if (string.IsNullOrEmpty(cid))
            {
                Debug.LogError($"Invalid CID in metadata URL: {metadataUrl}");
                return;
            }

            string fullMetadataUrl = gatewayUrl + cid;
            using (UnityWebRequest request = UnityWebRequest.Get(fullMetadataUrl))
            {
                try
                {
                    request.SetRequestHeader("Accept", "application/json");
                    await SendUnityWebRequest(request);

                    if (request.result != UnityWebRequest.Result.Success)
                    {
                        Debug.LogError($"❌ Failed to fetch metadata: {request.error}");
                        return;
                    }

                    string responseText = request.downloadHandler.text;
                    Debug.Log($"Raw response: {responseText}");

                    var responseHeaders = request.GetResponseHeaders();
                    if (!responseHeaders.ContainsKey("Content-Type") || !responseHeaders["Content-Type"].Contains("application/json"))
                    {
                        Debug.LogError("❌ Response is not JSON.");
                        return;
                    }

                    var metadata = JObject.Parse(responseText);
                    string nftName = metadata["name"]?.ToString();
                    string imageUrl = metadata["image"]?.ToString();
                    string artistName = metadata["attributes"]?
                        .FirstOrDefault(attr => attr["trait_type"]?.ToString() == "Artist")?["value"]?.ToString();

                    if (!string.IsNullOrEmpty(imageUrl))
                    {
                        await DownloadAndDisplayNFT(imageUrl, nftName, mintAddress, artistName);
                    }
                    else
                    {
                        Debug.LogError($"❌ Image URL missing in metadata: {metadataUrl}");
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogError($"❌ Exception while fetching metadata: {ex.Message}");
                }
            }
        }

        

    private async UniTask FetchNFTMetadata(string mintAddress)
    {
        // Sonic requires different parameters format
        string jsonRpcRequest = $@"{{
        ""jsonrpc"": ""2.0"",
        ""id"": 1,
        ""method"": ""getAsset"",
        ""params"": {{
            ""id"": ""{mintAddress}"",
            ""options"": {{
                ""showMetadata"": true,
                ""showFiles"": true,
                ""showCollection"": true
            }}
        }}
    }}";

        string responseText = null;
        const int maxAttempts = 3;
        int attempts = 0;
        bool success = false;

        while (!success && attempts < maxAttempts)
        {
            try
            {
                responseText = await SendWebRequestAsync(rpcUrl, jsonRpcRequest);

                // Check for Sonic-specific error format
                if (string.IsNullOrEmpty(responseText))
                    throw new Exception("Empty response (possibly rate limited).");

                var json = JObject.Parse(responseText);
                if (json["error"] != null)
                    throw new Exception($"Sonic RPC Error: {json["error"]["message"]}");

                success = true;
            }
            catch (Exception ex)
            {
                attempts++;
                Debug.LogError($"Error fetching NFT metadata for mint {mintAddress} (attempt {attempts}/{maxAttempts}): {ex.Message}");
                if (attempts < maxAttempts)
                    await UniTask.Delay(500 * attempts); // Add exponential backoff
            }
        }

        if (!success)
        {
            Debug.LogError($"Failed to fetch NFT metadata for mint {mintAddress} after {maxAttempts} attempts.");
            return;
        }

        Debug.Log($"Sonic Metadata Response for {mintAddress}: {responseText}");
        await ProcessSonicMetadata(responseText, mintAddress);
    }

    private async UniTask ProcessSonicMetadata(string metadataJson, string mintAddress)
    {
        try
        {
            var parsed = JObject.Parse(metadataJson);

            // Sonic-specific response format
            var metadata = parsed["result"]?["metadata"];
            if (metadata == null)
            {
                Debug.LogError($"No metadata found for {mintAddress}");
                return;
            }

            // Handle Sonic's URI format
            string metadataUri = metadata["uri"]?.ToString();
            if (string.IsNullOrEmpty(metadataUri))
            {
                Debug.LogError($"Metadata URI missing for {mintAddress}");
                return;
            }

            // Convert ar:// to HTTPS if needed
            if (metadataUri.StartsWith("ar://"))
                metadataUri = $"https://arweave.net/{metadataUri.Substring(5)}";

            await FetchExternalNFTMetadata(metadataUri, mintAddress);
        }
        catch (Exception ex)
        {
            Debug.LogError($"Sonic metadata processing failed: {ex.Message}");
        }
    }

    private async UniTask DownloadAndDisplayNFT(string imageUrl, string nftName, string nftAddress, string artistName)
    {
        if (nftIndex >= nftImagesUi.Length || nftIndex >= nftMeshes.Length)
        {
            Debug.LogError("No available slots for NFT visuals.");
            return;
        }

        if (imageCache.TryGetValue(imageUrl, out Texture2D cachedTexture))
        {
            AssignNFTVisuals(cachedTexture, nftName, nftAddress, artistName);
            return;
        }

        Texture2D texture = await DownloadTextureAsync(imageUrl);
        if (texture != null)
        {
            imageCache[imageUrl] = texture;
            AssignNFTVisuals(texture, nftName, nftAddress, artistName);
        }
        else
        {
            Debug.LogError($"Failed to download NFT image: {imageUrl}");
        }
    }

    private void AssignNFTVisuals(Texture2D texture, string nftName, string nftAddress, string artistName)
    {
        // If NFT is specific, assign it to nftObjects; otherwise use the general arrays.
        if (specificNFTs.Contains(nftAddress))
        {
            if (nftObjectIndex < nftObjects.Length)
            {
                var obj = nftObjects[nftObjectIndex];
                var rawImage = obj.GetComponent<RawImage>();
                if (rawImage != null) rawImage.texture = texture;
                var nftAddrComponent = obj.GetComponent<NFTAddress>();
                if (nftAddrComponent != null) nftAddrComponent.nftAddress = nftAddress;
                obj.name = nftName;
                Debug.Log($"Assigned specific NFT to nftObjects[{nftObjectIndex}] -> {nftName} ({nftAddress})");
                SaveMetadata(obj, nftAddress, nftName, artistName);
                nftObjectIndex++;
            }
            else
            {
                Debug.LogWarning("No available slots in nftObjects.");
            }
        }
        else
        {
            if (nftIndex < nftImagesUi.Length)
            {
                nftImagesUi[nftIndex].texture = texture;
                nftImagesUi[nftIndex].name = nftName;
                nftMeshes[nftIndex].material.mainTexture = texture;
                Debug.Log($"Assigned NFT to nftImagesUi[{nftIndex}] -> {nftName} ({nftAddress})");
                SaveMetadata(nftMeshes[nftIndex].gameObject, nftAddress, nftName, artistName);
                nftIndex++;
            }
            else
            {
                Debug.LogWarning("No available slots in nftImagesUi.");
            }
        }
    }

    private void SaveMetadata(GameObject target, string nftAddress, string nftName, string artistName)
    {
        Debug.Log($"Saving Metadata: {nftName} for NFT Address: {nftAddress}");
        NFTMetadataHolder metadataHolder = target.GetComponent<NFTMetadataHolder>();
        if (metadataHolder == null)
        {
            Debug.LogWarning($"NFTMetadataHolder not found on {target.name}");
            return;
        }
        metadataHolder.nftName = nftName;
        metadataHolder.artistName = artistName;
        metadataHolder.mintAddress = nftAddress;
        Debug.Log($"Metadata saved in {target.name}: {metadataHolder.nftName}");
    }

    private async UniTask<Texture2D> DownloadTextureAsync(string imageUrl)
    {
        using (UnityWebRequest request = UnityWebRequestTexture.GetTexture(imageUrl))
        {
            await SendUnityWebRequest(request);
            return request.result == UnityWebRequest.Result.Success ? DownloadHandlerTexture.GetContent(request) : null;
        }
    }

    private async UniTask<string> SendWebRequestAsync(string url, string jsonRpcRequest)
    {
        using (UnityWebRequest request = new UnityWebRequest(url, "POST")
        {
            uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(jsonRpcRequest)),
            downloadHandler = new DownloadHandlerBuffer()
        })
        {
            request.SetRequestHeader("Content-Type", "application/json");
            await SendUnityWebRequest(request);
            return request.result == UnityWebRequest.Result.Success ? request.downloadHandler.text : null;
        }
    }

    /// <summary>
    /// Sends the UnityWebRequest and retries if rate limited (HTTP 429).
    /// </summary>
    private async UniTask SendUnityWebRequest(UnityWebRequest request)
    {
        const int maxAttempts = 5;
        const int initialDelayMs = 1000;  // Starting with 1 second delay
        int attempt = 0;
        while (true)
        {
            await request.SendWebRequest().ToUniTask();
            if (request.result == UnityWebRequest.Result.Success)
                break;
            else if (request.error != null && request.error.Contains("429"))
            {
                attempt++;
                if (attempt >= maxAttempts)
                {
                    Debug.LogError($"Rate limit exceeded after {maxAttempts} attempts: {request.error}");
                    break;
                }
                int delayMs = initialDelayMs * (int)Math.Pow(2, attempt); // Exponential backoff
                Debug.LogWarning($"Rate limited (attempt {attempt}/{maxAttempts}). Waiting {delayMs}ms before retrying...");
                await UniTask.Delay(delayMs);
                continue;
            }
            else
            {
                break;
            }
        }
    }
    #endregion

    #region MintingNFTfromTradePOPUP Functions

    public async void TradeMintNfts()
    {
        Debug.Log("📢 TradeMintNfts() started...");
        loadingPanel.SetActive(true);

        if (WalletManager.instance == null)
        {
            Debug.LogError("❌ WalletManager.instance is NULL! Ensure WalletManager is set up.");
            loadingPanel.SetActive(false);
            return;
        }

        userPublicKey = WalletManager.instance.walletAddress;
        if (string.IsNullOrEmpty(userPublicKey))
        {
            Debug.LogError("❌ User Public Key is NULL or EMPTY!");
            loadingPanel.SetActive(false);
            return;
        }
        Debug.Log($"✅ Userr Public Key: {userPublicKey}");

        if (Web3.Instance?.WalletBase?.Account == null)
        {
            Debug.LogError("❌ Web3.Instance or WalletBase.Account is NULL! Ensure the user is logged in.");
            loadingPanel.SetActive(false);
            return;
        }

        // Use Web3.Rpc for all RPC calls
        var web3Rpc = Web3.Rpc;
        if (web3Rpc == null)
        {
            Debug.LogError("❌ Web3.Rpc is NULL! Using fallback RPC client.");
            web3Rpc = rpcClient;
        }

        // Determine the player's signing account (this is the account from the logged‐in wallet,
        // but note: for minting we want to use the NPC account).
        Account playerAccount;
        if (Web3.Instance.WalletBase.Mnemonic != null)
        {
            string mnemonic = Web3.Instance.WalletBase.Mnemonic.ToString();
            Debug.Log($"✅ User Mnemonic: {mnemonic}");
            var playerWallet = new Wallet(mnemonic, WordList.English);
            playerAccount = playerWallet.GetAccount(0);
            Debug.Log($"✅ Player Account (from mnemonic): {playerAccount.PublicKey}");
        }
        else
        {
            playerAccount = Web3.Instance.WalletBase.Account;
            Debug.Log($"✅ Player Account (external login): {playerAccount.PublicKey}");
        }

        // For NFT minting from NPC to NPC, use the NPC account.
        var npcWallet = new Wallet(npcMnemonic, WordList.English);
        var npcAccount = npcWallet.GetAccount(0);
        Debug.Log($"✅ NPC Wallet Address: {npcAccount.PublicKey}");

        if (web3Rpc == null)
        {
            Debug.LogError("❌ web3Rpc is NULL! Ensure the RPC client is initialized.");
            loadingPanel.SetActive(false);
            return;
        }

        var npcBalance = await web3Rpc.GetBalanceAsync(npcAccount.PublicKey);
        if (npcBalance == null || !npcBalance.WasSuccessful)
        {
            Debug.LogError($"❌ Failed to fetch NPC balance: {npcBalance?.Reason ?? "Unknown error"}");
            loadingPanel.SetActive(false);
            return;
        }
        Debug.Log($"✅ NPC Balance: {npcBalance.Result.Value / 1000000000f} SOL");

        if (npcBalance.Result.Value < 1000000) // 0.005 SOL
        {
            LogFeedback("❌ NPC does not have enough SOL for transaction fees. Send at least 0.05 SOL.", true);
            loadingPanel.SetActive(false);
            return;
        }

        // Fetch blockhash and rent once for the whole trade flow
        var blockHashResult = await web3Rpc.GetLatestBlockHashAsync();
        if (!blockHashResult.WasSuccessful)
        {
            LogFeedback("❌ Failed to get latest blockhash: " + blockHashResult.Reason, true);
            loadingPanel.SetActive(false);
            return;
        }
        var blockHash = blockHashResult.Result.Value.Blockhash;

        var minimumRentResult = await web3Rpc.GetMinimumBalanceForRentExemptionAsync(TokenProgram.MintAccountDataSize);
        if (!minimumRentResult.WasSuccessful)
        {
            LogFeedback("❌ Failed to get minimum rent: " + minimumRentResult.Reason, true);
            loadingPanel.SetActive(false);
            return;
        }
        ulong minimumRent = minimumRentResult.Result;

        // Use NPC account for minting, pass down blockhash/rent/rpc
        await TradeMintNftOptimized(npcAccount, blockHash, minimumRent, web3Rpc);
    }

    public async UniTask TradeMintNftOptimized(Account mintingAccount, string blockHash, ulong minimumRent, IRpcClient rpcClient)
    {
        LogFeedback("Starting NFT minting from trade pop up (optimized)");
        loadpanelTxt.text = "Minting Nft...";
        string mintName = "Sombre";
        string mintUri = "https://red-tough-wildcat-877.mypinata.cloud/ipfs/bafkreif7f2t73t5njpwpepnb7hbco4c5dl2nyuu4nbgrtpuq52r35irmxu?pinataGatewayToken=cpHZtQlPAxJ8AdYhHsEev_jKs5G0ABgFvMMXdRcP4VqBkZhXn8BD60cOXXcMoYl9";

        var mint = new Account();
        var associatedTokenAccount = AssociatedTokenAccountProgram.DeriveAssociatedTokenAccount(mintingAccount.PublicKey, mint.PublicKey);

        if (string.IsNullOrEmpty(mintUri))
        {
            Debug.LogError("mintUri is null or empty");
            loadingPanel.SetActive(false);
            return;
        }

        var metadata = new Metadata()
        {
            name = mintName,
            symbol = "ART",
            uri = mintUri,
            sellerFeeBasisPoints = 0,
            creators = new List<Creator> { new Creator(mintingAccount.PublicKey, 100, true) }
        };

        var transactionBuilder = new TransactionBuilder()
             .SetRecentBlockHash(blockHash)
             .SetFeePayer(mintingAccount)
             .AddInstruction(
                  SystemProgram.CreateAccount(
                       mintingAccount.PublicKey,
                       mint.PublicKey,
                       minimumRent,
                       TokenProgram.MintAccountDataSize,
                       TokenProgram.ProgramIdKey))
             .AddInstruction(
                  TokenProgram.InitializeMint(
                       mint.PublicKey,
                       0,
                       mintingAccount.PublicKey,
                       mintingAccount.PublicKey))
             .AddInstruction(
                  AssociatedTokenAccountProgram.CreateAssociatedTokenAccount(
                       mintingAccount.PublicKey,
                       mintingAccount.PublicKey,
                       mint.PublicKey))
             .AddInstruction(
                  TokenProgram.MintTo(
                       mint.PublicKey,
                       associatedTokenAccount,
                       1,
                       mintingAccount.PublicKey))
             .AddInstruction(
                  MetadataProgram.CreateMetadataAccount(
                       PDALookup.FindMetadataPDA(mint.PublicKey),
                       mint.PublicKey,
                       mintingAccount.PublicKey,
                       mintingAccount.PublicKey,
                       mintingAccount.PublicKey,
                       metadata,
                       TokenStandard.NonFungible,
                       true,
                       true,
                       null,
                       metadataVersion: MetadataVersion.V3))
             .AddInstruction(
                  MetadataProgram.CreateMasterEdition(
                       maxSupply: null,
                       masterEditionKey: PDALookup.FindMasterEditionPDA(mint.PublicKey),
                       mintKey: mint.PublicKey,
                       updateAuthorityKey: mintingAccount.PublicKey,
                       mintAuthority: mintingAccount.PublicKey,
                       payer: mintingAccount.PublicKey,
                       metadataKey: PDALookup.FindMetadataPDA(mint.PublicKey),
                       version: CreateMasterEditionVersion.V3)
             );

        byte[] txBytes = transactionBuilder.Build(new List<Account> { mintingAccount, mint });
        string serializedTx = Convert.ToBase64String(txBytes);

        var result = await rpcClient.SendTransactionAsync(serializedTx);

        if (!result.WasSuccessful)
        {
            LogFeedback("❌ NFT Minting failed: " + result.Reason, true);
            loadingPanel.SetActive(false);
            return;
        }
        LogFeedback("Mint transaction sent. TX: " + result.Result);

        bool confirmed = await ConfirmTransactionOptimized(result.Result, rpcClient);
        if (confirmed)
        {
            newMinted_Sombre = mint.PublicKey;
            LogFeedback("🎉 MINTED THIS NFT: " + newMinted_Sombre);
            LogFeedback("🎉 NFT Minting succeeded! TX: " + result.Result);
            await UniTask.Delay(1000);
            await TransferNftsOptimized(blockHash, rpcClient);
        }
        else
        {
            LogFeedback("❌ NFT mint transaction not confirmed.", true);
        }
    }

    public async UniTask TransferNftsOptimized(string blockHash, IRpcClient rpcClient)
    {
        userPublicKey = WalletManager.instance.walletAddress;
        loadingPanel.SetActive(true);
        loadpanelTxt.text = "Transfer Started!";

        var npcWallet = new Wallet(npcMnemonic, WordList.English);
        var npcAccount = npcWallet.GetAccount(0);
        var userPK = new PublicKey(userPublicKey);
        List<string> nftMintAddresses = new List<string> { newMinted_Sombre };

        foreach (string nftMintAddress in nftMintAddresses)
        {
            var mintPK = new PublicKey(nftMintAddress);
            var fromTokenAccount = AssociatedTokenAccountProgram.DeriveAssociatedTokenAccount(npcAccount.PublicKey, mintPK);
            var toTokenAccount = AssociatedTokenAccountProgram.DeriveAssociatedTokenAccount(userPK, mintPK);

            LogFeedback($"📌 NPC ATA: {fromTokenAccount.Key}");
            LogFeedback($"📌 User ATA: {toTokenAccount.Key}");

            // Step 1: Ensure User's ATA Exists (create if needed)
            var userTokenAccountInfo = await rpcClient.GetAccountInfoAsync(toTokenAccount);
            if (userTokenAccountInfo.Result?.Value == null)
            {
                LogFeedback("ℹ User's ATA does not exist. Creating one...");
                var createAtaTx = new TransactionBuilder()
                    .SetFeePayer(npcAccount)
                    .SetRecentBlockHash(blockHash)
                    .AddInstruction(
                        AssociatedTokenAccountProgram.CreateAssociatedTokenAccount(
                            npcAccount.PublicKey, userPK, mintPK))
                    .Build(npcAccount);

                var createResult = await rpcClient.SendTransactionAsync(createAtaTx);
                if (!createResult.WasSuccessful)
                {
                    LogFeedback($"❌ Failed to create User's ATA for NFT {nftMintAddress}: {createResult.Reason}", true);
                    continue;
                }
                LogFeedback($"✅ User's ATA created successfully! TX: {createResult.Result}");
                bool ataConfirmed = await ConfirmTransactionOptimized(createResult.Result, rpcClient);
                if (!ataConfirmed)
                {
                    LogFeedback("❌ User's ATA creation not confirmed. Skipping transfer.", true);
                    loadingPanel.SetActive(false);
                    return;
                }
                await UniTask.Delay(500);
            }

            // Step 2: Perform NFT Transfer
            LogFeedback($"🚀 Sending NFT transfer transaction for {nftMintAddress}...");
            var transferTx = new TransactionBuilder()
                .SetFeePayer(npcAccount)
                .SetRecentBlockHash(blockHash)
                .AddInstruction(
                    TokenProgram.TransferChecked(
                        fromTokenAccount,
                        toTokenAccount,
                        1UL,
                        0,
                        npcAccount.PublicKey,
                        mintPK
                    )
                )
                .Build(npcAccount);

            var transferResult = await rpcClient.SendTransactionAsync(transferTx);
            if (!transferResult.WasSuccessful)
            {
                LogFeedback($"❌ NFT Transfer failed for {nftMintAddress}: {transferResult.Reason}", true);
                loadingPanel.SetActive(false);
                return;
            }
            LogFeedback($"🎉 NFT Transfer successful! TX: {transferResult.Result}");
            CallMoveFunctionOnNFT("https://red-tough-wildcat-877.mypinata.cloud/ipfs/bafkreif7f2t73t5njpwpepnb7hbco4c5dl2nyuu4nbgrtpuq52r35irmxu?pinataGatewayToken=cpHZtQlPAxJ8AdYhHsEev_jKs5G0ABgFvMMXdRcP4VqBkZhXn8BD60cOXXcMoYl9");
        }

        await UniTask.Delay(2000); // Reduced delay
        loadpanelTxt.text = "Updating NFT UI";
        nftDisplayScript.UpdateNfts();
    }


    #endregion
}
