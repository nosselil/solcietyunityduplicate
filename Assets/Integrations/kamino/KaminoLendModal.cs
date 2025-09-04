using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Globalization;

public class KaminoLendModal : MonoBehaviour
{
    public TextMeshProUGUI titleText;
    public TMP_InputField amountInput;
    public TextMeshProUGUI apyText;
    public TextMeshProUGUI yearlyEstimateText;
    public TextMeshProUGUI resultText; // For success/error messages
    public Button confirmButton;

    private KaminoLoanOffer currentOffer;
    private const string LogTag = "[KaminoLendModal]";
    private bool isSubmitting = false;

    public void Show(KaminoLoanOffer offer)
    {
        currentOffer = offer;
        Debug.Log($"{LogTag} Show({offer.symbol})");
        titleText.text = $"Lend {offer.symbol}";
        apyText.text = $"<b>APY:</b>\n<size=80%>{offer.supplyApy}%</size>";
        amountInput.text = "";
        yearlyEstimateText.text = "";
        resultText.text = "";
        isSubmitting = false;
        if (confirmButton != null) confirmButton.interactable = true;
        gameObject.SetActive(true);

        amountInput.onValueChanged.RemoveAllListeners();
        amountInput.onValueChanged.AddListener(OnAmountChanged);

        confirmButton.onClick.RemoveAllListeners();
        confirmButton.onClick.AddListener(OnConfirmClicked);
    }

    private void OnAmountChanged(string input)
    {
        if (TryParseFloatFlexible(input, out float amount))
        {
            Debug.Log($"{LogTag} OnAmountChanged input='{input}' parsed={amount}");
            var apyStr = (currentOffer.supplyApy ?? "").Trim().TrimEnd('%');
            if (float.TryParse(apyStr, NumberStyles.Float, CultureInfo.InvariantCulture, out float apy))
            {
                float estimate = amount * (1 + apy / 100f);
                yearlyEstimateText.text = $"<b>1 year return:</b>\n<size=80%>{estimate:F4} {currentOffer.symbol}</size>";
            }
            else
            {
                yearlyEstimateText.text = "";
            }
        }
        else
        {
            Debug.Log($"{LogTag} OnAmountChanged invalid input='{input}'");
            yearlyEstimateText.text = "";
        }
    }

    private void OnConfirmClicked()
    {
        if (isSubmitting)
        {
            Debug.Log($"{LogTag} Ignored click while submitting.");
            return;
        }

        Debug.Log($"{LogTag} OnConfirmClicked amountInput='{amountInput.text}'");
        if (TryParseFloatFlexible(amountInput.text, out float amount) && amount > 0f)
        {
            Debug.Log($"{LogTag} Parsed amount={amount}, calling KaminoAPI.Lend");
            isSubmitting = true;
            if (confirmButton != null) confirmButton.interactable = false;
            SetResult("Submitting...", false, false); // neutral status; API will update success/error
            KaminoAPI.Instance.Lend(currentOffer, amount);
        }
        else
        {
            Debug.LogWarning($"{LogTag} Invalid amount '{amountInput.text}'");
            SetResult("Please enter a valid amount.", true);
        }
    }

    public void ShowError(string message)
    {
        isSubmitting = false;
        if (confirmButton != null) confirmButton.interactable = true;
        SetResult(message, true);
    }

    public void ShowSuccess(string message)
    {
        isSubmitting = false;
        if (confirmButton != null) confirmButton.interactable = true;
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

    private static bool TryParseFloatFlexible(string s, out float value)
    {
        value = 0f;
        if (string.IsNullOrWhiteSpace(s)) return false;
        string normalized = s.Trim().Replace(',', '.');
        return float.TryParse(normalized, NumberStyles.Float, CultureInfo.InvariantCulture, out value);
    }
}