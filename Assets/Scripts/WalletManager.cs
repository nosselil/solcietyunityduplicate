using UnityEngine;
using UnityEngine.UI;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.UI;
using Solana.Unity.Wallet;
using Solana.Unity.SDK;
using Solana.Unity.Rpc;
using Solana.Unity.Rpc.Types;
using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Net.Http;
   public class WalletManager : MonoBehaviour
    {
        public static WalletManager instance;


        public Button InteractBtn;

        public IRpcClient rpcClient; // RpcClient interface
        public string walletAddress;
        public List<string> latestTransactions = new List<string>(); // Store multiple transactions
        public string latestTransaction;
        public float Balance;

        public string npcMnemonic;
        public string NEWnpcMnemonic;
        public string OLDnpcMnemonic;
        public Network currentNetwork = Network.TestNet;
        public enum Network
        {
            MainNet,
            TestNet,
            DevNet
        }



        public float wagerAmount;

        [Header("Helius API Key")]
        public string heliusApiKey;

        [Header("RPC Client Settings")]
        public Cluster cluster;

        [HideInInspector]
        public ScrollRect scrollrect;

        void Awake()
        {
            wagerAmount = 0.1f;
            if (instance == null)
            {
                instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }



        #region DeviceDetectoin
        public bool isMobile = false;
        public Text deviceTxt;

#if UNITY_WEBGL && !UNITY_EDITOR
    [DllImport("__Internal")]
    private static extern bool IsMobile();  // Method to check if it's a mobile device

    [DllImport("__Internal")]
    private static extern void LockOrientationToLandscape();  // Method to lock orientation to landscape
#endif

        void Start()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
        if (IsMobile())
        {
     //       deviceText.text = "Mob";
            // Lock the orientation to landscape for mobile devices
           isMobile = true;
            LockOrientationToLandscape();
        }
        else
        {
           deviceTxt.gameObject.SetActive(false);
       isMobile = false;
        //    deviceText.text = "NOT MB";
        }
#else            
            isMobile = false;
            deviceTxt.gameObject.SetActive(false);
        //    deviceText.text = "NOT MB";
#endif
    }

        #endregion

        public void InitializeWalletManager()
        {
            npcMnemonic = Web3.Instance.rpcCluster == RpcCluster.DevNet
                 ? OLDnpcMnemonic
                 : NEWnpcMnemonic;

            //    isMainnet = (Web3.Instance.rpcCluster == RpcCluster.MainNet);




            if (Web3.Instance.rpcCluster == RpcCluster.MainNet)
            {
                currentNetwork = Network.MainNet;
                cluster = Cluster.MainNet;
            }
            else if (Web3.Instance.rpcCluster == RpcCluster.TestNet)
            {
                currentNetwork = Network.TestNet;
                cluster = Cluster.TestNet;
            }
            else
            {
                currentNetwork = Network.DevNet;
                cluster = Cluster.DevNet;
            }


            // Always use Web3's RPC client which is already configured
            rpcClient = Web3.Rpc;

            if (rpcClient == null)
            {
                Debug.LogError("RPC Client initialization failed!");
            }


        }

    #region MobileUI
    public void makeBtn_Interactable()
    {
        InteractBtn.interactable = true;
    }
    public void makeBtn_NONInteractable()
    {
        InteractBtn.interactable = false;
    }


    #endregion





    /*   public IEnumerator InitializeWalletManagerOLD()
           {
               // Wait until the Web3 instance and its wallet are ready.
               while (Web3.Instance == null || Web3.Wallet == null)
               {
                   yield return null;
               }


           isMainnet = (Web3.Instance.rpcCluster == RpcCluster.MainNet);


           // Set the cluster based on your condition.
           cluster = isMainnet ? Cluster.MainNet : Cluster.DevNet;

               // For WebGL, attempt to use the streaming RPC client.
               if (Application.platform == RuntimePlatform.WebGLPlayer)
               {
                   try
                   {
                       // Try to get the streaming RPC client from the initialized wallet.
                       rpcClient = Web3.Rpc;
                   if (rpcClient == null)
                       {
                           Debug.LogError("WebSocket client initialization failed. Falling back to RPC client.");
                           rpcClient = ClientFactory.GetClient(cluster); // Fallback to HTTP RPC client
                   }
                   else
                   {

                   }
                   }
                   catch (Exception ex)
                   {
                       Debug.LogError($"WebSocket connection failed: {ex.Message}. Using RPC client as fallback.");
                       rpcClient = ClientFactory.GetClient(cluster); // Fallback to HTTP RPC client
                   }
               }
               else
               {
                   // For non-WebGL platforms, use the HTTP RPC client.
                   rpcClient = ClientFactory.GetClient(cluster);
               }
           } */
    public async void CheckBalance()
        {
            if (string.IsNullOrEmpty(walletAddress))
            {
                Debug.LogError("Wallet address is not set. Please log in first.");
                return;
            }

            try
            {

                // Get the balance of the wallet
                var balanceResult = await rpcClient.GetBalanceAsync(walletAddress);

                if (balanceResult.Result != null)
                {

                    float balanceInSOL = balanceResult.Result.Value / 1_000_000_000f;
                    Debug.Log($"Balance: {balanceInSOL} SOL");
                    if (WagerScene.instance != null)
                        WagerScene.instance.balance.text = balanceInSOL.ToString();




                    Balance = balanceInSOL;
                }
                else
                {
                    Debug.LogError("Error fetching balance.");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error checking balance: {ex.Message}");
            }
        }

        private void OnEnable()
        {
            Web3.OnLogin += OnLogin;
        }

        private void OnDisable()
        {
            Web3.OnLogin -= OnLogin;
        }

        public void updateData()
        {
            LastTransaction();
            CheckBalance();
        }

        private void OnLogin(Account account)
        {
            //      StartCoroutine(InitializeWalletManager());



            walletAddress = account.PublicKey;
            Debug.Log($"Wallet Address Set: {walletAddress}");
            LastTransaction();
            CheckBalance();
        }

        public async void FetchLatestTransactions()
        {
            latestTransaction = "";
            if (string.IsNullOrEmpty(walletAddress))
            {
                Debug.LogError("Wallet address is not set. Please log in first.");
                return;
            }

            try
            {
                Debug.Log($"Fetching latest transactions for wallet: {walletAddress}");

                // Fetch the latest 3 transactions
                var signaturesResult = await rpcClient.GetSignaturesForAddressAsync(walletAddress, limit: 3);

                if (signaturesResult.Result == null || signaturesResult.Result.Count == 0)
                {
                    Debug.Log("No transactions available for this wallet.");
                    latestTransactions.Clear();
                    latestTransactions.Add("No transactions available.");
                    return;
                }

                latestTransactions.Clear(); // Clear old transactions

                foreach (var signature in signaturesResult.Result)
                {
                    var transactionResult = await rpcClient.GetTransactionAsync(signature.Signature);
                    if (transactionResult.Result != null)
                    {
                        string transactionDetails = $"- TxID: {signature.Signature}\n";
                        transactionDetails += $"  Slot: {transactionResult.Result.Slot}\n";
                        transactionDetails += $"  BlockTime: {transactionResult.Result.BlockTime}\n";
                        latestTransactions.Add(transactionDetails);
                    }
                    else
                    {
                        latestTransactions.Add($"- TxID: {signature.Signature} (Details Unavailable)");
                    }
                    latestTransaction = string.Join("\n", latestTransactions);

                    //      latestTransaction = latestTransactions.ToString();
                }



                Debug.Log("Fetched Transactions:\n" + string.Join("\n", latestTransactions));
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error fetching transactions: {ex.Message}");
            }
        }

        /*      public async void PerformAirdrop()
              {
                  if (string.IsNullOrEmpty(walletAddress))
                  {
                      Debug.LogError("Wallet address is not set. Please log in first.");
                      return;
                  }

                  try
                  {
                      Debug.Log($"Requesting airdrop for wallet: {walletAddress}");

                      // Request an airdrop of 1 SOL
                      var airdropResult = await rpcClient.RequestAirdropAsync(walletAddress, 1_000_000_000); // 1 SOL
                      if (airdropResult.Result != null)
                      {
                          Debug.Log($"Airdrop successful: {airdropResult.Result}");
                      }
                      else
                      {
                          Debug.LogError("Airdrop failed.");
                      }
                  }
                  catch (Exception ex)
                  {
                      Debug.LogError($"Error during airdrop: {ex.Message}");
                  }
              } */
        public async void PerformAirdrop()
        {
            if (string.IsNullOrEmpty(walletAddress)) return;

            try
            {
                if (currentNetwork == Network.TestNet)
                {
                    // Use Sonic faucet
                    using (var client = new HttpClient())
                    {
                        var response = await client.GetAsync($"https://faucet.sonic.game/airdrop/{walletAddress}");
                        if (response.IsSuccessStatusCode)
                        {
                            Debug.Log("Sonic faucet request successful");
                        }
                    }
                }
                else if (currentNetwork == Network.DevNet)
                {
                    // MagicBlock airdrop
                    var result = await rpcClient.RequestAirdropAsync(walletAddress, 1_000_000_000);
                    if (result.Result == null) Debug.LogError("DevNet airdrop failed");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Airdrop error: {ex.Message}");
            }
        }
        public void SeeTran()
        {
            CheckBalance();
        }

        public void LastTransaction()
        {
            if (string.IsNullOrEmpty(walletAddress))
            {
                Debug.LogError("Wallet address is not set. Please log in first.");
                return;
            }

            FetchLatestTransactions();
        }
    }

