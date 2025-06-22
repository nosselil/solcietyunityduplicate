using Fusion.Statistics;
using PixelCrushers.DialogueSystem.OpenAIAddon.ElevenLabs;
using Privy;
using System;
using System.Threading.Tasks;
using UnityEngine;

public class PrivyController : MonoBehaviour
{
    private const string PRIVY_APP_ID = "cmc4xv73a00pdjv0moslhtjzo";
    private const string PRIVY_CLIENT_ID = "client-WY6MsNbgydosB9pzWHD5Ygib47fCCeJe1LYYNrukRaJPB";

    IEmbeddedSolanaWallet wallet;

    public static PrivyController Instance { get; private set; }

    async void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject); // Prevent duplicate instances
            return;
        }        
        Instance = this;
        DontDestroyOnLoad(gameObject); // Persist across scene loads
        await Init();

        Debug.Log("PRIVY: Instance Ready: " + PrivyManager.Instance.IsReady);
        Debug.Log("PRIVY: User id is " + PrivyManager.Instance.User.Id);

        /*Debug.Log("PRIVY: Creating an embedded wallet...");
        await CreateEmbeddedWallet();
        Debug.Log("PRIVY: Wallet created, creating a mock RPC request...");
        await CreateRpcRequest();*/
    }

    async Task CreateRpcRequest()
    {
        /*try
        {
            IEmbeddedSolanaWallet embeddedWallet = wallet; //PrivyManager.Instance.User.EmbeddedSolanaWallets[0];

            var rpcRequest = new RpcRequest
            {
                Method = "personal_sign",
                Params = new string[] { "A message to sign", embeddedWallet.Address }  // Use the 'new' keyword here
            };

            RpcResponse personalSignResponse = await embeddedWallet.RpcProvider.Request(rpcRequest);

            Debug.Log(personalSignResponse.Data);
        }
        catch (PrivyException.EmbeddedWalletException ex)
        {
            Debug.LogError($"Could not sign message due to error: {ex.Error} {ex.Message}");
        }
        catch (Exception ex)
        {
            Debug.LogError($"Could not sign message exception {ex.Message}");
        }*/
    }

    async Task Init()
    {
        // Initialize Privy SDK or other setup here
        var config = new PrivyConfig
        {
            AppId = PRIVY_APP_ID,
            ClientId = PRIVY_CLIENT_ID
        };

        PrivyManager.Initialize(config);
        Debug.Log("PRIVY: Initialization started...");
        await PrivyManager.AwaitReady();
        Debug.Log("PRIVY: Init completed.");

    }

    async Task CreateEmbeddedWallet()
    {
        try
        {
            PrivyUser privyUser = PrivyManager.Instance.User;

            if (privyUser != null)
            {
                wallet = await PrivyManager.Instance.User.CreateSolanaWallet();
                Debug.Log("New wallet created with address: " + wallet.Address + ", index: " + wallet.WalletIndex + ", recovery method: " + wallet.RecoveryMethod);
            }
        }
        catch
        {
            Debug.Log("Error creating embedded wallet.");
        }
    }



    void Login()
    {
        
    }

}
