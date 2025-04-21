using Solana.Unity.SDK.Example;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using codebase.utility;
using Solana.Unity.Rpc.Types;
using Solana.Unity.SDK;
using UnityEditor;

// ReSharper disable once CheckNamespace

public class ReceiveScreen : SimpleScreen
{
    public Button airdrop_btn;
    public Button close_btn;

    public TextMeshProUGUI publicKey_txt;
    public RawImage qrCode_img;

    public TMP_InputField publicKeyInputField;

    private void Start()
    {
        airdrop_btn.onClick.AddListener(RequestAirdrop);

        close_btn.onClick.AddListener(() =>
        {
            manager.ShowScreen(this, "wallet_screen");
        });
        Web3.OnWalletChangeState += CheckAndToggleAirdrop;
    }
    
    private void OnEnable()
    {
        var isDevnet = IsDevnet();
        airdrop_btn.enabled = isDevnet;
        airdrop_btn.interactable = isDevnet;
    }

    public override void ShowScreen(object data = null)
    {
        base.ShowScreen();
        gameObject.SetActive(true);

        CheckAndToggleAirdrop();

        GenerateQr();

        Debug.Log("Show screen, address text in public key field currently: " + publicKey_txt);

        publicKeyInputField.text = Web3.Instance.WalletBase.Account.PublicKey;
        //publicKey_txt.text = Web3.Instance.WalletBase.Account.PublicKey;
    }

    private void CheckAndToggleAirdrop()
    {
        if(Web3.Wallet == null) return;
        airdrop_btn.gameObject.SetActive(Web3.Wallet.RpcCluster == RpcCluster.DevNet);
    }

    private void GenerateQr()
    {
        Texture2D tex = QRGenerator.GenerateQRTexture(Web3.Instance.WalletBase.Account.PublicKey, 256, 256);
        qrCode_img.texture = tex;
    }

   


    private async void RequestAirdrop()
    {
        Loading.StartLoading();
        var result = await Web3.Wallet.RequestAirdrop();
        if (result?.Result == null)
        {
            Debug.LogError("Airdrop failed, you may have reached the limit, try later or use a public faucet");
        }
        else
        {
            // Confirm the transaction and log the result
            var confirmation = await Web3.Rpc.ConfirmTransaction(result.Result, Commitment.Confirmed);
            Debug.Log($"Confirmation result: {confirmation}");

            // Update the balance after the airdrop
            await Web3.UpdateBalance();

            Debug.Log("Airdrop success, see transaction at https://explorer.solana.com/tx/"
                      + result.Result + "?cluster=devnet");
            manager.ShowScreen(this, "wallet_screen");
        }
        Loading.StopLoading();
    }



    private static bool IsDevnet()
    {
        return  Web3.Rpc.NodeAddress.AbsoluteUri.Contains("devnet");
    }

    public void CopyPublicKeyToClipboard()
    {
        Debug.Log("CLIPBOARD: Helper function called");
        //ClipboardUtilities.PrepareCopy(Web3.Instance.WalletBase.Account.PublicKey.ToString());

        //ClipboardUtilities.Copy(Web3.Instance.WalletBase.Account.PublicKey.ToString());
        Debug.Log("CLIPBOARD: Clipboard utils found");
        Clipboard.Copy(Web3.Instance.WalletBase.Account.PublicKey.ToString());
        gameObject.GetComponent<Toast>()?.ShowToast("Public Key copied to clipboard", 3);
    }

    public override void HideScreen()
    {
        base.HideScreen();
        gameObject.SetActive(false);
    }

    public void OnClose()
    {
        var wallet = GameObject.Find("wallet");
        wallet.SetActive(false);
    }
}