using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class KaminoLendModal : MonoBehaviour
{
    public TextMeshProUGUI titleText;
    public TMP_InputField amountInput;
    public TextMeshProUGUI apyText;
    public TextMeshProUGUI yearlyEstimateText;
    public TextMeshProUGUI resultText; // For success/error messages
    public Button confirmButton;

    private KaminoLoanOffer currentOffer;

    public void Show(KaminoLoanOffer offer)
    {
        currentOffer = offer;
        titleText.text = $"Lend {offer.symbol}";
        apyText.text = $"<b>APY:</b>\n<size=80%>{offer.supplyApy}%</size>";
        amountInput.text = "";
        yearlyEstimateText.text = "";
        resultText.text = "";
        gameObject.SetActive(true);

        amountInput.onValueChanged.RemoveAllListeners();
        amountInput.onValueChanged.AddListener(OnAmountChanged);

        confirmButton.onClick.RemoveAllListeners();
        confirmButton.onClick.AddListener(OnConfirmClicked);
    }

    private void OnAmountChanged(string input)
    {
        if (float.TryParse(input, out float amount))
        {
            float apy = float.Parse(currentOffer.supplyApy);
            float estimate = amount * (1 + apy / 100f);
            yearlyEstimateText.text = $"<b>1 year return:</b>\n<size=80%>{estimate:F4} {currentOffer.symbol}</size>";
        }
        else
        {
            yearlyEstimateText.text = "";
        }
    }

    private void OnConfirmClicked()
    {
        if (float.TryParse(amountInput.text, out float amount) && amount > 0)
        {
            KaminoAPI.Instance.Lend(currentOffer, amount);
            SetResult("Lend transaction submitted!", false, true);
            // Do not close the modal
        }
        else
        {
            SetResult("Please enter a valid amount.", true);
        }
    }

    public void ShowError(string message)
    {
        SetResult(message, true);
    }

    public void ShowSuccess(string message)
    {
        SetResult(message, false, true);
    }

    private void SetResult(string message, bool isError, bool isSuccess = false)
    {
        if (resultText != null)
        {
            if (isSuccess)
                resultText.color = Color.green;
            else if (isError)
                resultText.color = Color.red;
            else
                resultText.color = Color.white;
            resultText.text = message;
        }
    }
} 