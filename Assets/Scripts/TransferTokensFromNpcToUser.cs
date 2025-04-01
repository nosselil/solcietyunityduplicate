using System;
using System.Threading.Tasks;
using Solana.Unity.Rpc;
using Solana.Unity.Rpc.Builders;
using Solana.Unity.Rpc.Core.Http;
using Solana.Unity.Wallet;
using TMPro;
using UnityEngine;
using Solana.Unity.Programs;
using Solana.Unity.Rpc.Types;
using Solana.Unity.Wallet.Bip39;

namespace Solana.Unity.SDK.Example
{
    public class TransferTokensFromNpcToUser : SimpleScreen
    {
        public TextMeshProUGUI errorTxt;
        public string fromMnemonic = "miss gate front unique liberty gap bind choice lumber clown loan absorb"; // Fixed spaces
        public string toPublicKeyInput;
        public string nftMintAddressInput = "DMLo5BJVqtUHcvXTe6tWu4VRMzzZADPHv3e5uCaNQwGz";
        public string amountInput = "1";

        private void Start()
        {
            /*Debug.LogError($"Sender Mnemonic: {fromMnemonic}");
            Debug.LogError($"Receiver Public Key (Before Assignment): {toPublicKeyInput}");
            Debug.LogError($"NFT Mint Address: {nftMintAddressInput}");
            Debug.LogError($"Transfer Amount: {amountInput}");*/
        }

        public void TryTransfer()
        {
            toPublicKeyInput = WalletManager.instance.walletAddress;
           // Debug.LogError($"Receiver Public Key (After Assignment): {toPublicKeyInput}");

            if (CheckInput())
            {
                TransferNft();
            }
        }

        private async void TransferNft()
        {
            if (!ulong.TryParse(amountInput, out ulong amount))
            {
                errorTxt.text = "Invalid transfer amount.";
                Debug.LogError("Error: Transfer amount is invalid.");
                return;
            }

            try
            {
                var rpcClient = ClientFactory.GetClient(Cluster.DevNet);

                if (!PublicKey.IsValid(toPublicKeyInput))
                {
                    errorTxt.text = "Invalid receiver public key format.";
                    Debug.LogError("Error: Invalid receiver public key format.");
                    return;
                }

                Debug.Log("Fetching account info for receiver...");
                var accountInfo = await rpcClient.GetAccountInfoAsync(toPublicKeyInput);
                Debug.Log($"Account Info Response: {accountInfo.RawRpcResponse}");

                var wallet = new Wallet.Wallet(fromMnemonic, WordList.English);
                var fromAccount = wallet.GetAccount(0);
                Debug.Log($"Derived Public Key (Sender): {fromAccount.PublicKey.Key}");
                Debug.Log($"Derived Private Key (Sender): {fromAccount.PrivateKey.Key}");

                var toPublicKey = new PublicKey(toPublicKeyInput);
                var mintPublicKey = new PublicKey(nftMintAddressInput);
                var fromTokenAccount = AssociatedTokenAccountProgram.DeriveAssociatedTokenAccount(fromAccount.PublicKey, mintPublicKey);
                var toTokenAccount = AssociatedTokenAccountProgram.DeriveAssociatedTokenAccount(toPublicKey, mintPublicKey);

                Debug.Log($"Sender Token Account: {fromTokenAccount}");
                Debug.Log($"Receiver Token Account: {toTokenAccount}");

                var toTokenAccountInfo = await rpcClient.GetAccountInfoAsync(toTokenAccount);
                Debug.Log($"Receiver Token Account Info: {toTokenAccountInfo.RawRpcResponse}");

                if (toTokenAccountInfo.Result?.Value == null)
                {
                    Debug.Log("Receiver's token account does not exist. Creating one...");

                    var blockHashResponse = await rpcClient.GetLatestBlockHashAsync();
                    if (!blockHashResponse.WasSuccessful)
                    {
                        errorTxt.text = "Failed to fetch the recent block hash.";
                        Debug.LogError("Error: Failed to fetch the recent block hash.");
                        return;
                    }

                    string recentBlockHash = blockHashResponse.Result.Value.Blockhash;

                    var createAccountTx = new TransactionBuilder()
                        .SetRecentBlockHash(recentBlockHash)
                        .SetFeePayer(fromAccount)
                        .AddInstruction(AssociatedTokenAccountProgram.CreateAssociatedTokenAccount(
                            fromAccount.PublicKey,
                            toPublicKey,
                            mintPublicKey))
                        .Build(fromAccount);

                    Debug.Log($"Sending token account creation transaction...");
                    var createAccountResult = await rpcClient.SendTransactionAsync(createAccountTx);
                    Debug.Log($"Create Account Transaction Response: {createAccountResult.RawRpcResponse}");

                    if (!createAccountResult.WasSuccessful)
                    {
                        errorTxt.text = $"Failed to create associated token account: {createAccountResult.Reason}";
                        Debug.LogError($"Error: Failed to create associated token account: {createAccountResult.Reason}");
                        return;
                    }

                    Debug.Log($"Account created successfully: {createAccountResult.Result}");
                    await Task.Delay(1000);
                }

                var transferBlockHashResponse = await rpcClient.GetLatestBlockHashAsync();
                if (!transferBlockHashResponse.WasSuccessful)
                {
                    errorTxt.text = "Failed to fetch the recent block hash for the transfer.";
                    Debug.LogError("Error: Failed to fetch the recent block hash for the transfer.");
                    return;
                }

                string transferRecentBlockHash = transferBlockHashResponse.Result.Value.Blockhash;

                var transferTx = new TransactionBuilder()
                    .SetRecentBlockHash(transferRecentBlockHash)
                    .SetFeePayer(fromAccount)
                    .AddInstruction(TokenProgram.Transfer(
                        fromTokenAccount,
                        toTokenAccount,
                        amount,
                        fromAccount))
                    .Build(fromAccount);

                Debug.Log($"Sending transfer transaction...");
                var transferResult = await rpcClient.SendTransactionAsync(transferTx);
                Debug.Log($"Transfer Transaction Response: {transferResult.RawRpcResponse}");

                if (transferResult.WasSuccessful)
                {
                    errorTxt.text = "Transfer successful.";
                    Debug.Log("Success: NFT Transfer completed.");
                }
                else
                {
                    errorTxt.text = $"Transfer failed: {transferResult.Reason}";
                    Debug.LogError($"Error: Transfer failed: {transferResult.Reason}");
                }
            }
            catch (Exception ex)
            {
                errorTxt.text = $"Error: {ex.Message}";
                Debug.LogError($"Transfer failed: {ex}");
            }

            Debug.LogError($"Final UI Error Text: {errorTxt.text}");
        }

        private bool CheckInput()
        {
            if (string.IsNullOrEmpty(amountInput))
            {
                errorTxt.text = "Please input transfer amount";
                Debug.LogError("Error: Transfer amount is empty.");
                return false;
            }

            if (string.IsNullOrEmpty(toPublicKeyInput))
            {
                errorTxt.text = "Please enter receiver public key";
                Debug.LogError("Error: Receiver public key is empty.");
                return false;
            }

            if (string.IsNullOrEmpty(nftMintAddressInput))
            {
                errorTxt.text = "Please enter NFT mint address";
                Debug.LogError("Error: NFT mint address is empty.");
                return false;
            }

            if (string.IsNullOrEmpty(fromMnemonic))
            {
                errorTxt.text = "Please enter sender's mnemonic (secret phrase)";
                Debug.LogError("Error: Sender's mnemonic is empty.");
                return false;
            }

            errorTxt.text = "";
            return true;
        }
    }
}
