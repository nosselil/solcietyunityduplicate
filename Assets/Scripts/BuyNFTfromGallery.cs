using UnityEngine;
using Solana.Unity.Rpc;
using Solana.Unity.Rpc.Types;
using Solana.Unity.Rpc.Builders;
using Solana.Unity.Rpc.Core.Http;
using Solana.Unity.SDK;
using Solana.Unity.SDK.Nft;
using Solana.Unity.Wallet;
using Solana.Unity.Wallet.Bip39;
using Solana.Unity.Programs;
using TMPro;
using System.Threading.Tasks;

public class BuyNFTfromGallery : MonoBehaviour
{
    [Header("RPC Client Settings")]
    public Cluster cluster;

    [Header("NPC Wallet Mnemonic")]
    [TextArea(3, 5)]
    public string npcMnemonic =
        "miss gate front unique liberty gap bind choice lumber clown loan absorb";

    [Header("User & NFT Details")]
    public string userPublicKey = ""; // 6tyYVbCJ2Ru7gkeVF6qefvk5szvL6HdwxBpLquEzRqPg
    public string nftMintAddress = ""; // This can be dynamically updated

    [Header("UI Feedback (Optional)")]
    public TextMeshProUGUI feedbackTxt;

    private IRpcClient rpcClient;

    float requiredSol = 0.01f;

    private void Awake()
    {
        rpcClient = ClientFactory.GetClient(cluster);
        if (rpcClient == null)
        {
            Debug.LogError("❌ Failed to initialize rpcClient! Check if ClientFactory is working.");
        }
        else
        {
            Debug.Log($"✅ rpcClient initialized with Cluster: {cluster}");
        }
    }
    public async void BuyNft()
    {
        Debug.Log("📢 BuyNft() started...");

        // ✅ Check if WalletManager is initialized
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

        // ✅ Check Web3 Instance
        if (Web3.Instance?.WalletBase?.Mnemonic == null)
        {
            Debug.LogError("❌ Web3.Instance or WalletBase.Mnemonic is NULL! Ensure the user is logged in.");
            return;
        }

        // ✅ Retrieve Player's Mnemonic and Wallet
        string mnemonic = Web3.Instance.WalletBase.Mnemonic.ToString();
        Debug.Log($"✅ User Mnemonic: {mnemonic}");

        var playerWallet = new Wallet(mnemonic, WordList.English);
        if (playerWallet == null)
        {
            Debug.LogError("❌ Player Wallet creation failed!");
            return;
        }

        var playerAccount = playerWallet.GetAccount(0);
        Debug.Log($"✅ Player Account: {playerAccount.PublicKey}");

        // ✅ Ensure NFT Mint Address is provided
        if (string.IsNullOrEmpty(nftMintAddress))
        {
            Debug.LogError("❌ NFT Mint Address is NULL or EMPTY!");
            return;
        }
        Debug.Log($"✅ NFT Mint Address: {nftMintAddress}");

        // ✅ Ensure NPC Wallet is properly initialized
        var npcWallet = new Wallet(npcMnemonic, WordList.English);
        if (npcWallet == null)
        {
            Debug.LogError("❌ NPC Wallet creation failed!");
            return;
        }

        var npcAccount = npcWallet.GetAccount(0);
        Debug.Log($"✅ NPC Wallet Address: {npcAccount.PublicKey}");

        // ✅ Check if rpcClient is initialized
        if (rpcClient == null)
        {
            Debug.LogError("❌ rpcClient is NULL! Ensure the RPC client is initialized.");
            return;
        }

        // ✅ Check NPC Balance
        var npcBalance = await rpcClient.GetBalanceAsync(npcAccount.PublicKey);
        if (npcBalance == null || !npcBalance.WasSuccessful)
        {
            Debug.LogError($"❌ Failed to fetch NPC balance: {npcBalance?.Reason ?? "Unknown error"}");
            return;
        }

        Debug.Log($"✅ NPC Balance: {npcBalance.Result.Value / 1000000000f} SOL");

        if (npcBalance.Result.Value < 5000000) // 0.005 SOL
        {
            LogFeedback("❌ NPC does not have enough SOL for transaction fees. Send at least 0.05 SOL.", true);
            return;
        }

        // ✅ Transfer SOL from Player to NPC
        bool solTransferSuccess = await TransferSolToNpc(playerAccount, requiredSol);
        if (!solTransferSuccess)
        {
            LogFeedback("❌ SOL transfer failed. NFT transfer aborted.", true);
            return;
        }

        // ✅ Proceed to Transfer NFT if SOL Transfer is successful
        await TransferNft();
    }




    private async Task<bool> TransferSolToNpc(Account playerAccount, float amountSol)
    {
        try
        {
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
            await TransferNft();
            return true; // Return success
        }
        catch (System.Exception ex)
        {
            LogFeedback($"❌ Error transferring SOL: {ex.Message}", true);
            return false;
        }
       
    }


    public async Task TransferNft()
    {
        LogFeedback("Starting NFT transfer...");

        var npcWallet = new Wallet(npcMnemonic, WordList.English);
        var npcAccount = npcWallet.GetAccount(0);

        var userPK = new PublicKey(userPublicKey);
        var mintPK = new PublicKey(nftMintAddress);

        // 🔹 Fetch latest ATAs for NFT transfer
        var fromTokenAccount = AssociatedTokenAccountProgram.DeriveAssociatedTokenAccount(npcAccount.PublicKey, mintPK);
        var toTokenAccount = AssociatedTokenAccountProgram.DeriveAssociatedTokenAccount(userPK, mintPK);

        LogFeedback($"NPC ATA: {fromTokenAccount.Key}");
        LogFeedback($"User ATA: {toTokenAccount.Key}");

        // 2) Check the token balance in fromTokenAccount
        var fromBalance = await rpcClient.GetTokenAccountBalanceAsync(fromTokenAccount);
        if (!fromBalance.WasSuccessful)
        {
            Debug.LogError($"Failed to fetch token balance: {fromBalance.Reason}");
        }
        else
        {
            // UiAmountString will show how many tokens the from account has
            Debug.Log("From account balance: " + fromBalance.Result.Value.UiAmountString);
        }

        // 🔹 Ensure NPC owns the NFT
        var npcTokenAccountInfo = await rpcClient.GetAccountInfoAsync(fromTokenAccount);
        if (npcTokenAccountInfo.Result?.Value == null)
        {
            LogFeedback("❌ The NPC wallet does NOT own this NFT. Transfer cannot proceed.", true);
            return;
        }

        // 🔹 Ensure User’s ATA Exists (Create if Needed)
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
            await Task.Delay(2000); // Wait for confirmation
        }
        else
        {
            LogFeedback("✅ User's ATA already exists. Skipping creation...");
        }

        // 🔹 Fetch latest blockhash for transaction
        var transferBlockHash = await rpcClient.GetLatestBlockHashAsync();
        if (!transferBlockHash.WasSuccessful)
        {
            LogFeedback($"❌ Failed to get blockhash: {transferBlockHash.Reason}", true);
            return;
        }

        // 🔹 Ensure NPC has enough SOL for transaction fees
        var npcBalance = await rpcClient.GetBalanceAsync(npcAccount.PublicKey);
        if (npcBalance.Result.Value < 5000000) // 0.005 SOL in lamports
        {
            LogFeedback("❌ NPC does not have enough SOL for the transfer. Send at least 0.05 SOL.", true);
            return;
        }

        // 🔹 Build and send transfer transaction
        LogFeedback("Sending NFT transfer transaction...");

        var transferTx = new TransactionBuilder()
            .SetFeePayer(npcAccount)
            .SetRecentBlockHash(transferBlockHash.Result.Value.Blockhash)
            .AddInstruction(
                TokenProgram.TransferChecked(
                    fromTokenAccount,      // NPC's ATA (source)
                    toTokenAccount,        // User's ATA (destination)
                    1UL,                   // Amount (1 NFT, must be ulong)
                    0,                     // Decimals (0 for NFT, must be int)
                    npcAccount.PublicKey,  // Authority (NPC’s Wallet)
                    mintPK                 // NFT Mint Address
                )
            )
            .Build(npcAccount);

        var transferResult = await rpcClient.SendTransactionAsync(transferTx);
        if (!transferResult.WasSuccessful)
        {
            LogFeedback($"❌ NFT Transfer failed: {transferResult.Reason}", true);
        }
        else
        {
            LogFeedback($"🎉 NFT Transfer successful! Transaction: {transferResult.Result}");
        }
    }

    private void LogFeedback(string message, bool isError = false)
    {
        if (isError) Debug.LogError(message);
        else Debug.Log(message);

        if (feedbackTxt != null)
        {
            feedbackTxt.text = message;
            feedbackTxt.color = isError ? Color.red : Color.white;
        }
    }
}
