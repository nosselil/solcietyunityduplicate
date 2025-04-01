using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Solana.Unity.Metaplex.NFT.Library;    // For Metadata and related types
using Solana.Unity.Metaplex.Utilities;       // For PDALookup
using Solana.Unity.Programs;                 // For MetadataProgram
using Solana.Unity.Rpc.Types;                // For Commitment
using Solana.Unity.Rpc.Builders;             // For TransactionBuilder
using Solana.Unity.Rpc.Models;               // For Transaction
using Solana.Unity.SDK;                      // For Web3 access
using Solana.Unity.Wallet;                   // For Account and PublicKey
using Solana.Unity.Rpc.Core.Http;
using Solana.Unity.SDK.Nft;                  // For Nft.TryGetNftData
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Alias to avoid conflict with System.Security.Cryptography.X509Certificates.PublicKey
using SolanaPublicKey = Solana.Unity.Wallet.PublicKey;

public class NftUriUpdater : MonoBehaviour
{
    [Header("NFT Update Settings")]
    [Tooltip("Enter the NFT mint address (e.g. BXtUxUehYDFhQNjmQV7TpoR2njQaJYuutBtU3vJPvvmR)")]
    [SerializeField] private string nftMintAddress;

    [Tooltip("Enter the new URI for the NFT metadata")]
    [SerializeField] private string newUri;

    [Header("UI Elements")]
    [SerializeField] private Button updateButton;
    [SerializeField] private TextMeshProUGUI feedbackText;

    private void Start()
    {
      //  if (updateButton != null)
       //     updateButton.onClick.AddListener(() => { UpdateNftUriAsync().Forget(); });
    }

    private void OnEnable()
    {
        UpdateNftUriAsync().Forget();
    }
    private async UniTaskVoid UpdateNftUriAsync()
    {
        // Validate input.
        if (string.IsNullOrEmpty(nftMintAddress) || string.IsNullOrEmpty(newUri))
        {
            SetFeedback("Please provide both the NFT mint address and the new URI.", true);
            return;
        }

        // Ensure that a wallet is logged in.
        if (Web3.Account == null)
        {
            SetFeedback("Wallet is not logged in. Please log in first.", true);
            return;
        }

        // Convert the mint address string to a PublicKey.
        SolanaPublicKey mint;
        try
        {
            mint = new SolanaPublicKey(nftMintAddress);
        }
        catch (Exception ex)
        {
            SetFeedback("Invalid NFT mint address: " + ex.Message, true);
            return;
        }

        // Compute the metadata account PDA.
        SolanaPublicKey metadataAccount = PDALookup.FindMetadataPDA(mint);
        Debug.Log($"Metadata Account PDA: {metadataAccount.Key}");

        // Fetch NFT data using your working method.
        var nftData = await Nft.TryGetNftData(mint, Web3.Instance.WalletBase.ActiveRpcClient, commitment: Commitment.Processed).AsUniTask();
        if (nftData == null)
        {
            SetFeedback("Failed to fetch NFT data.", true);
            return;
        }

        // Access off-chain metadata via the 'offchainData' property.
        var offchain = nftData.metaplexData.data.offchainData;
        if (offchain == null)
        {
            SetFeedback("NFT off-chain metadata not found.", true);
            return;
        }

        // Create a new Metadata instance by copying existing values and updating the URI.
        Metadata updatedMetadata = new Metadata
        {
            name = offchain.name,                              // NFT name
            symbol = offchain.symbol,                          // NFT symbol
            uri = newUri,                                      // New URI
            sellerFeeBasisPoints = (uint)offchain.seller_fee_basis_points, // Seller fee
         //   creators = offchain.creators                        // Creators list
            // Include additional fields if needed.
        };

        // Retrieve the latest blockhash.
        var blockHashResult = await Web3.Rpc.GetLatestBlockHashAsync();
        if (!blockHashResult.WasSuccessful)
        {
            SetFeedback("Failed to retrieve the latest blockhash: " + blockHashResult.Reason, true);
            return;
        }

        // Build the transaction using the metadata update instruction.
        TransactionBuilder txBuilder = new TransactionBuilder()
           .SetRecentBlockHash(blockHashResult.Result.Value.Blockhash)
           .SetFeePayer(Web3.Account)
           .AddInstruction(
               MetadataProgram.UpdateMetadataAccount(
                   metadataAccount,    // Metadata account PDA
                   Web3.Account,       // Update authority (your wallet)
                   null,               // New update authority (null to leave unchanged)
                   updatedMetadata,    // New metadata (with updated URI)
                   null                // Primary sale happened flag (null to leave unchanged)
               )
           );

        // Build the transaction (returns a byte array) and deserialize it.
        byte[] txBytes = txBuilder.Build(new List<Account> { Web3.Account });
        Transaction tx = Transaction.Deserialize(txBytes);

        // Sign and send the transaction.
        var res = await Web3.Wallet.SignAndSendTransaction(tx);

        // Handle the response.
        if (res?.Result != null)
        {
            await Web3.Rpc.ConfirmTransaction(res.Result, Commitment.Confirmed);
            SetFeedback($"NFT URI updated successfully!\nTransaction: {res.Result}", false);
        }
        else
        {
            SetFeedback("NFT URI update failed: " + res?.Reason, true);
        }
    }

    private void SetFeedback(string message, bool isError)
    {
        if (feedbackText != null)
        {
            feedbackText.text = message;
            feedbackText.color = isError ? Color.red : Color.white;
        }
        Debug.Log(message);
    }
}
