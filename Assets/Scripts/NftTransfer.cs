using System;
using System.Linq;
using System.Threading.Tasks;
using Solana.Unity.Rpc;
using Solana.Unity.Rpc.Models;
using Solana.Unity.Wallet;
using UnityEngine;

public class NftTransfer : MonoBehaviour
{
  /*  // Setup the network (Mainnet, Testnet, Devnet)
    private static string networkUrl = "https://api.mainnet-beta.solana.com"; // You can use testnet for testing
    private static string senderPrivateKey = "YOUR_PRIVATE_KEY"; // Wallet private key (Secret Phrase)
    private static string receiverWalletAddress = "RECEIVER_WALLET_ADDRESS"; // Address of the recipient
    private static string nftMintAddress = "NFT_MINT_ADDRESS"; // Mint address of the NFT
    private static string senderWalletAddress = "SENDER_WALLET_ADDRESS"; // Sender's wallet address

    // Reference to the RPC client and wallet
    private RpcClient rpcClient;
    private Wallet wallet;

    void Start()
    {
        // Initialize RPC client and wallet on start
        rpcClient = new RpcClient(networkUrl);
        wallet = new Wallet(senderPrivateKey); // Load wallet using private key
    }

    // Method to transfer NFT
    public async void TransferNft()
    {
        // Fetch the token accounts for the sender's wallet
        var tokenAccounts = await rpcClient.GetTokenAccountsByOwnerAsync(senderWalletAddress);
        var senderNftTokenAccount = tokenAccounts.Result.FirstOrDefault(account =>
            account.Account.Data.Parsed.Info.Mint == nftMintAddress); // Match the NFT mint address

        if (senderNftTokenAccount == null)
        {
            Debug.LogError("Sender does not own the specified NFT.");
            return;
        }

        // Prepare the transfer instruction
        var transferInstruction = SystemProgram.Transfer(
            senderNftTokenAccount.Pubkey,
            receiverWalletAddress,
            nftMintAddress, // Mint address of the NFT
            wallet.PublicKey
        );

        // Create the transaction
        var transaction = new Transaction
        {
            Instructions = new[] { transferInstruction }
        };

        // Sign and send the transaction
        var result = await rpcClient.SendTransactionAsync(transaction, wallet);
        if (result.IsSuccess)
        {
            Debug.Log("NFT transfer successful!");
        }
        else
        {
            Debug.LogError($"Error transferring NFT: {result.ErrorMessage}");
        }
    }*/
}
