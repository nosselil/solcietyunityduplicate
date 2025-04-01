//31-01-15

using UnityEngine;
using Solana.Unity.Rpc;
using Solana.Unity.Rpc.Types;
using Solana.Unity.Rpc.Builders;
using Solana.Unity.Rpc.Core.Http;
using Solana.Unity.SDK;
using Solana.Unity.Wallet;
using Solana.Unity.Wallet.Bip39;
using Solana.Unity.Programs;
using TMPro;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Threading.Tasks;

public class NpcNftTransfer : MonoBehaviour
{
    [Header("RPC Client Settings")]
    public Cluster cluster = Cluster.MainNet;

    [Header("NPC Wallet Mnemonic")]
    [TextArea(3, 5)]
    public string npcMnemonic =
        "miss gate front unique liberty gap bind choice lumber clown loan absorb";

    [Header("User Public Key")]
    public string userPublicKey = "";//6tyYVbCJ2Ru7gkeVF6qefvk5szvL6HdwxBpLquEzRqPg

    [Header("UI Feedback (Optional)")]
    public TextMeshProUGUI feedbackTxt;

    // Reference to NFTAddress scripts (array of GameObjects that hold NFTAddress scripts)
    public GameObject[] nftObjects;

    private IRpcClient rpcClient;

    private void Awake()
    {
        rpcClient = ClientFactory.GetClient(cluster);
        Debug.LogError("THIS: "+gameObject.name);
    }

    public async void TransferNfts()
    {
        Debug.LogError("THIS: " + gameObject.name);
        userPublicKey = WalletManager.instance.walletAddress;
        foreach (var nftObject in nftObjects)
        {
            var nftAddressScript = nftObject.GetComponent<NFTAddress>();
            if (nftAddressScript != null && !string.IsNullOrEmpty(nftAddressScript.nftAddress))
            {
                LogFeedback($"Found NFT Mint Address: {nftAddressScript.nftAddress}");
            }
        }
        LogFeedback("Starting NFT transfers...");

        var npcWallet = new Wallet(npcMnemonic, WordList.English);
        var npcAccount = npcWallet.GetAccount(0);

        LogFeedback($"NPC Wallet Address: {npcAccount.PublicKey.Key}");

        var userPK = new PublicKey(userPublicKey);

        // Collect NFT mint addresses from the NFTAddress scripts on each GameObject
        List<string> nftMintAddresses = new List<string>();
        foreach (var nftObject in nftObjects)
        {
            var nftAddressScript = nftObject.GetComponent<NFTAddress>();
            if (nftAddressScript != null && !string.IsNullOrEmpty(nftAddressScript.nftAddress))
            {
                nftMintAddresses.Add(nftAddressScript.nftAddress);
                LogFeedback($"Found NFT Mint Address: {nftAddressScript.nftAddress}");
            }
        }

        foreach (string nftMintAddress in nftMintAddresses)
        {
            LogFeedback($"Transferring NFT with mint address: {nftMintAddress}");

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
                Debug.LogError($"Failed to fetch token balance for NFT {nftMintAddress}: {fromBalance.Reason}");
                continue; // Skip to next NFT
            }

            // 🔹 Ensure NPC owns the NFT
            var npcTokenAccountInfo = await rpcClient.GetAccountInfoAsync(fromTokenAccount);
            if (npcTokenAccountInfo.Result?.Value == null)
            {
                LogFeedback($"❌ The NPC wallet does NOT own this NFT ({nftMintAddress}). Transfer cannot proceed.", true);
                continue; // Skip to next NFT
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
                    LogFeedback($"❌ Failed to create User's ATA for NFT {nftMintAddress}: {createResult.Reason}", true);
                    continue; // Skip to next NFT
                }

                LogFeedback($"✅ User's ATA created successfully for NFT {nftMintAddress}! TX: {createResult.Result}");
                await Task.Delay(2000); // Wait for confirmation
            }
            else
            {
                LogFeedback($"✅ User's ATA already exists for NFT {nftMintAddress}. Skipping creation...");
            }

            // 🔹 Fetch latest blockhash for transaction
            var transferBlockHash = await rpcClient.GetLatestBlockHashAsync();
            if (!transferBlockHash.WasSuccessful)
            {
                LogFeedback($"❌ Failed to get blockhash for NFT {nftMintAddress}: {transferBlockHash.Reason}", true);
                continue; // Skip to next NFT
            }

            // 🔹 Ensure NPC has enough SOL for transaction fees
            var npcBalance = await rpcClient.GetBalanceAsync(npcAccount.PublicKey);
            if (npcBalance.Result.Value < 5000000) // 0.005 SOL in lamports
            {
                LogFeedback("❌ NPC does not have enough SOL for the transfer. Send at least 0.05 SOL.", true);
                return; // Stop all transfers if not enough SOL
            }

            // 🔹 Build and send transfer transaction for each NFT
            LogFeedback($"Sending NFT transfer transaction for {nftMintAddress}...");

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
                LogFeedback($"❌ NFT Transfer failed for {nftMintAddress}: {transferResult.Reason}", true);
            }
            else
            {
                LogFeedback($"🎉 NFT Transfer successful for {nftMintAddress}! Transaction: {transferResult.Result}");
            }

            // Optionally, you can add a delay to ensure the network can process the transactions one at a time
            await Task.Delay(1000);
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