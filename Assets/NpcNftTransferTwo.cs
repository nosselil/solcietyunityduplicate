using System.Threading.Tasks;
using UnityEngine;
// --- Solana Unity SDK Imports ---
using Solana.Unity.Rpc;
using Solana.Unity.Rpc.Core.Http;
using Solana.Unity.Rpc.Types;
using Solana.Unity.Rpc.Builders;
using Solana.Unity.Wallet;
using Solana.Unity.Wallet.Bip39;
using Solana.Unity.Programs;

public class NpcNftTransferTwo : MonoBehaviour
{
    // --- Use the Correct Data ---
    private IRpcClient rpc = ClientFactory.GetClient(Cluster.MainNet); // Use MainNet if testing live

    public string npcMnemonic = "miss gate front unique liberty gap bind choice lumber clown loan absorb"; // NPC wallet seed phrase
    public string userPublicKey = "FTrgVbyqFfh7XhT89eReJkKmCeNVchtteadPqENt4aQE"; // Your wallet
    public string nftMintAddress = "6tyYVbCJ2Ru7gkeVF6qefvk5szvL6HdwxBpLquEzRqPg"; // NFT mint address

    public void start()
    {
        userPublicKey = WalletManager.instance.walletAddress;
    }
    /// <summary>
    /// Call this method to start the NFT transfer from NPC to your wallet.
    /// </summary>
    public async void TransferNft()
    {
        Debug.Log("🚀 Starting NFT Transfer...");

        // 1️⃣ Load NPC wallet
        var npcWallet = new Wallet(npcMnemonic, WordList.English);
        var npcAccount = npcWallet.GetAccount(0);

        Debug.Log($"🔹 NPC Wallet Address: {npcAccount.PublicKey.Key}");

        // 2️⃣ Check NPC's SOL balance
        var balance = await rpc.GetBalanceAsync(npcAccount.PublicKey);
        Debug.Log($"🔹 NPC Balance: {balance.Result.Value / 1_000_000_000} SOL");

        if (balance.Result.Value == 0)
        {
            Debug.LogError("❌ NPC wallet has 0 SOL! Cannot pay for transactions.");
            return;
        }

        // 3️⃣ Parse the user's public key
        var userPK = new PublicKey(userPublicKey);
        Debug.Log($"🔹 User's Wallet Address: {userPK.Key}");

        // 4️⃣ Parse the NFT mint address
        var mintPK = new PublicKey(nftMintAddress);
        Debug.Log($"🔹 NFT Mint Address: {mintPK.Key}");

        // 5️⃣ Derive NPC and User token accounts
        var fromTokenAccount = AssociatedTokenAccountProgram.DeriveAssociatedTokenAccount(npcAccount.PublicKey, mintPK);
        var toTokenAccount = AssociatedTokenAccountProgram.DeriveAssociatedTokenAccount(userPK, mintPK);

        Debug.Log($"🔹 NPC's Token Account (ATA): {fromTokenAccount.Key}");
        Debug.Log($"🔹 User's Token Account (ATA): {toTokenAccount.Key}");

        // 6️⃣ Check if NPC actually owns the NFT
        var npcTokenAccountInfo = await rpc.GetAccountInfoAsync(fromTokenAccount);
        if (npcTokenAccountInfo.Result?.Value == null)
        {
            Debug.LogError("❌ NPC's token account (ATA) does not exist! Does the NPC actually own this NFT?");
            return;
        }
        else
        {
            Debug.Log("✅ NPC's token account exists! Proceeding with transfer...");
        }

        // 7️⃣ Check if the user's ATA exists
        var toAccountInfo = await rpc.GetAccountInfoAsync(toTokenAccount);
        if (toAccountInfo.Result?.Value == null)
        {
            Debug.Log("ℹ️ User's ATA does not exist. Creating a new one...");

            var blockHashResp = await rpc.GetLatestBlockHashAsync();
            if (!blockHashResp.WasSuccessful)
            {
                Debug.LogError("❌ Failed to get recent blockhash: " + blockHashResp.Reason);
                return;
            }

            var createAtaTx = new TransactionBuilder()
                .SetFeePayer(npcAccount)
                .SetRecentBlockHash(blockHashResp.Result.Value.Blockhash)
                .AddInstruction(
                    AssociatedTokenAccountProgram.CreateAssociatedTokenAccount(
                        npcAccount.PublicKey,
                        userPK,
                        mintPK
                    )
                )
                .Build(npcAccount);

            var createResult = await rpc.SendTransactionAsync(createAtaTx);
            if (!createResult.WasSuccessful)
            {
                Debug.LogError("❌ Failed to create user's ATA: " + createResult.Reason);
                return;
            }

            Debug.Log($"✅ User's ATA created successfully! TX: {createResult.Result}");

            // Optional: Wait a moment for on-chain confirmation
            await Task.Delay(2000);
        }
        else
        {
            Debug.Log("✅ User's ATA already exists! Proceeding to transfer...");
        }

        // 8️⃣ Build the NFT Transfer Transaction
        var transferBlockHash = await rpc.GetLatestBlockHashAsync();
        if (!transferBlockHash.WasSuccessful)
        {
            Debug.LogError("❌ Failed to get blockhash for transfer: " + transferBlockHash.Reason);
            return;
        }

        var transferTx = new TransactionBuilder()
            .SetFeePayer(npcAccount)
            .SetRecentBlockHash(transferBlockHash.Result.Value.Blockhash)
            .AddInstruction(
                TokenProgram.TransferChecked(
                    fromTokenAccount,      // source ATA
                    toTokenAccount,        // destination ATA
                    1UL,                   // amount of NFT (ulong)
                    0,                     // decimals = 0 for NFT (int)
                    npcAccount.PublicKey,  // authority (owns source ATA)
                    mintPK                 // mint address
                )
            )
            .Build(npcAccount); // signs with npcAccount’s private key

        Debug.Log("🚀 Sending NFT transfer transaction...");

        // 9️⃣ Send the transaction
        var transferResult = await rpc.SendTransactionAsync(transferTx);
        if (!transferResult.WasSuccessful)
        {
            Debug.LogError("❌ NFT Transfer failed: " + transferResult.Reason);
        }
        else
        {
            Debug.Log($"🎉 NFT Transfer successful! Transaction Signature: {transferResult.Result}");
        }
    }
}
