using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Solana.Unity.Wallet;
using Solana.Unity.Wallet.Bip39;
using Solana.Unity.SDK;
using Cysharp.Threading.Tasks;

public class WagerPopUp : MonoBehaviour
{
    private Wallet userWallet;
    private Account playerAccount;
    public InputField amountInputField;
    public Text feedbackText;
    private float enteredAmount;

    public float minAmount = 0f;
    public float maxAmount = 0.1f;

    public GameObject loadingPanel;

    void Start()
    {
        if (amountInputField != null)
        {
            amountInputField.text = maxAmount.ToString("F4");
            enteredAmount = maxAmount;
            amountInputField.onValueChanged.AddListener(delegate { ValidateInput(); });
        }

        if (loadingPanel == null) loadingPanel = NftManager.instance.loadingPanel;
    }

    // Now using async/await instead of coroutine.
    public async void NextSceneWager(string sceneName)
    {
        if (enteredAmount < minAmount || enteredAmount > maxAmount)
        {
            feedbackText.text = $"This is just a friendly match let's keep the wagers under {maxAmount}";
            return;
        }

        if (float.TryParse(amountInputField.text, out float parsedAmount))
        {
            if (parsedAmount < minAmount || parsedAmount > maxAmount)
            {
                feedbackText.text = $"This is just a friendly match let's keep the wagers under {maxAmount}";
                return;
            }

            enteredAmount = parsedAmount;

            // Instead of always creating a wallet from a mnemonic,
            // check if a mnemonic exists. If not, use the externally provided account.
            if (Web3.Instance.WalletBase.Mnemonic != null)
            {
                // In-game wallet flow: create wallet from mnemonic.
                string mnemonic = Web3.Instance.WalletBase.Mnemonic.ToString();
                userWallet = new Wallet(mnemonic, WordList.English);
                playerAccount = userWallet.GetAccount(0);
            }
            else if (Web3.Instance.WalletBase.Account != null)
            {
                // External wallet flow: use the account provided by the wallet adapter/Web3Auth.
                playerAccount = Web3.Instance.WalletBase.Account;
            }
            else
            {
                feedbackText.text = "Error: No valid wallet available.";
                return;
            }

            loadingPanel.SetActive(true);
            NftManager.instance.loadpanelTxt.text = "We are processing your transaction, Pleae wait..";
            // Await the SOL transfer using UniTask.
            bool transferSuccess = await NftManager.instance.TransferSolToNpc(playerAccount, enteredAmount);

            loadingPanel.SetActive(false);

            if (transferSuccess)
            {
                SceneManager.LoadScene(sceneName);
            }
            else
            {
                feedbackText.text = "Transaction failed. Please try again.";
            }
        }
        else
        {
            feedbackText.text = "Invalid input. Please enter a valid number.";
        }
    }

    private void ValidateInput()
    {
        if (float.TryParse(amountInputField.text, out float parsedAmount))
        {
            enteredAmount = parsedAmount;
            if (parsedAmount < minAmount)
                feedbackText.text = $"Min is {minAmount}";
            else if (parsedAmount > maxAmount)
                feedbackText.text = $"This is just a friendly match let's keep the wagers under {maxAmount}";
            else
                feedbackText.text = "";
        }
        else
        {
            feedbackText.text = "Invalid input";
        }
    }
}
