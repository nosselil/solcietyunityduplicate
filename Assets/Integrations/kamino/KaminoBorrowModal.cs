using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class KaminoBorrowModal : MonoBehaviour
{
    public TextMeshProUGUI titleText;
    public TMP_InputField amountInput;
    public TextMeshProUGUI availableText;
    public TextMeshProUGUI apyText;
    public TextMeshProUGUI collateralText; // Displays user's current collateral
    public TextMeshProUGUI errorText; // For errors/collateral info
    public Button halfButton;
    public Button maxButton;
    public Button confirmButton;

    private KaminoLoanOffer currentOffer;
    private float availableToBorrow = 0f;

    public void Show(KaminoLoanOffer offer, float available, float collateral)
    {
        currentOffer = offer;
        availableToBorrow = available;
        titleText.text = $"Borrow {offer.symbol}";
        availableText.text = "<b>Available to borrow:</b> \n<size=80%>" + availableToBorrow.ToString("F2") + "</size>";
        apyText.text = "<b>APY:</b> <size=80%>" + offer.borrowApy + "%</size>";
        collateralText.text = "<b>Collateral you put:</b> \n<size=80%>" + collateral.ToString("F2") + "</size>";
        amountInput.text = "";
        errorText.text = "";
        gameObject.SetActive(true);

        amountInput.onValueChanged.RemoveAllListeners();
        amountInput.onValueChanged.AddListener(OnAmountChanged);

        halfButton.onClick.RemoveAllListeners();
        halfButton.onClick.AddListener(() => SetAmount(availableToBorrow / 2f));

        maxButton.onClick.RemoveAllListeners();
        maxButton.onClick.AddListener(() => SetAmount(availableToBorrow));

        confirmButton.onClick.RemoveAllListeners();
        confirmButton.onClick.AddListener(OnConfirmClicked);
    }

    private void SetAmount(float amount)
    {
        amountInput.text = amount.ToString("F4");
    }

    private void OnAmountChanged(string input)
    {
        if (float.TryParse(input, out float amount))
        {
            if (amount > availableToBorrow)
            {
                SetError("Amount exceeds available to borrow.", true);
            }
            else
            {
                SetError("", false);
            }
        }
        else
        {
            SetError("", false);
        }
    }

    private void OnConfirmClicked()
    {
        if (float.TryParse(amountInput.text, out float amount) && amount > 0 && amount <= availableToBorrow)
        {
            KaminoAPI.Instance.Borrow(currentOffer, amount);
            SetError("Borrow successful!", false, true);
            // Do not close the modal
        }
        else
        {
            SetError("Invalid amount or insufficient collateral.", true);
        }
    }

    public void ShowError(string message)
    {
        SetError(message, true);
    }

    public void UpdateCollateral(float collateral)
    {
        if (collateralText != null)
            collateralText.text = "<b>Collateral:</b> <size=80%>" + collateral.ToString("F2") + "</size>";
    }

    public void SetError(string message, bool isError, bool isSuccess = false)
    {
        if (errorText != null)
        {
            if (isSuccess)
                errorText.color = Color.green;
            else if (isError)
                errorText.color = Color.red;
            else
                errorText.color = Color.white;
            errorText.text = message;
        }
    }
}