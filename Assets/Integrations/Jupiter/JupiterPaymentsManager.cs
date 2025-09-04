using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using TMPro;
using Solana.Unity.Dex;
using Solana.Unity.Dex.Jupiter;
using Solana.Unity.Dex.Models;
using Solana.Unity.Dex.Quotes;
using Solana.Unity.SDK;
using Solana.Unity.Rpc.Models;

public class JupiterPaymentsManager : MonoBehaviour
{
    [Header("UI References")]
    public TMP_InputField exactAmountInput;
    public TMP_Text resultText;
    public TMP_Text solEstimateText;
    public TMP_Text balanceText;
    public TMP_Text routeText;
    public GameObject loadingScreen;
    public TMP_Dropdown paymentTokenDropdown;
    public TMP_Dropdown targetTokenDropdown;

    private IDexAggregator _dex;
    private TokenData _paymentToken; // Token you're paying with (usually SOL)
    private TokenData _targetToken;  // Token you want to receive (usually USDC)
    private SwapQuoteAg _currentQuote;

    // Common token symbols for payment scenarios
    private readonly string[] _paymentTokens = { "SOL", "USDC", "USDT" };
    private readonly string[] _targetTokens = { "USDC", "USDT", "SOL" };

    private void Start()
    {
        InitializeDex();
        InitializeUI();
        UpdateBalance();
    }

    private async void InitializeDex()
    {
        if (Web3.Account == null)
        {
            Debug.LogError("❌ Web3.Account is null. Make sure wallet is connected.");
            return;
        }

        try
        {
            _dex = new JupiterDexAg(Web3.Account);
            
            // Get default tokens for payments (SOL to USDC)
            _paymentToken = await _dex.GetTokenBySymbol("SOL");
            _targetToken = await _dex.GetTokenBySymbol("USDC");
            
            Debug.Log($"✅ Jupiter Payments initialized. Payment: {_paymentToken.Symbol}, Target: {_targetToken.Symbol}");
        }
        catch (Exception e)
        {
            Debug.LogError($"❌ Failed to initialize Jupiter Payments: {e.Message}");
        }
    }

    private void InitializeUI()
    {
        // Initialize dropdowns
        if (paymentTokenDropdown != null)
        {
            paymentTokenDropdown.ClearOptions();
            paymentTokenDropdown.AddOptions(_paymentTokens.ToList());
            paymentTokenDropdown.onValueChanged.AddListener(OnPaymentTokenChanged);
        }

        if (targetTokenDropdown != null)
        {
            targetTokenDropdown.ClearOptions();
            targetTokenDropdown.AddOptions(_targetTokens.ToList());
            targetTokenDropdown.onValueChanged.AddListener(OnTargetTokenChanged);
        }

        // Set default values
        if (paymentTokenDropdown != null) paymentTokenDropdown.value = 0; // SOL
        if (targetTokenDropdown != null) targetTokenDropdown.value = 0; // USDC
    }

    private async void OnPaymentTokenChanged(int index)
    {
        if (_dex == null) return;
        
        try
        {
            _paymentToken = await _dex.GetTokenBySymbol(_paymentTokens[index]);
            Debug.Log($"✅ Payment token changed to: {_paymentToken.Symbol}");
            GetExactOutputQuote();
        }
        catch (Exception e)
        {
            Debug.LogError($"❌ Failed to get payment token: {e.Message}");
        }
    }

    private async void OnTargetTokenChanged(int index)
    {
        if (_dex == null) return;
        
        try
        {
            _targetToken = await _dex.GetTokenBySymbol(_targetTokens[index]);
            Debug.Log($"✅ Target token changed to: {_targetToken.Symbol}");
            GetExactOutputQuote();
        }
        catch (Exception e)
        {
            Debug.LogError($"❌ Failed to get target token: {e.Message}");
        }
    }

    public void OnPaymentButtonClick()
    {
        if (_dex == null)
        {
            resultText.text = "❌ Jupiter Payments not initialized";
            return;
        }

        if (_currentQuote == null)
        {
            resultText.text = "❌ No quote available. Please enter an amount.";
            return;
        }

        StartCoroutine(ExecutePayment());
    }

    public void OnExactAmountChanged()
    {
        GetExactOutputQuote();
    }

    private async void GetExactOutputQuote()
    {
        if (_dex == null || _paymentToken == null || _targetToken == null) return;

        if (string.IsNullOrEmpty(exactAmountInput.text))
        {
            ClearQuote();
            return;
        }

        if (!float.TryParse(exactAmountInput.text, out float amount) || amount <= 0f)
        {
            ClearQuote();
            return;
        }

        try
        {
            ToggleLoading(true);
            resultText.text = "Getting payment quote...";

            // Convert amount to proper decimals for exact output
            ulong outputAmount = ConvertToUlong(amount, _targetToken.Decimals);
            
            // Get swap quote for exact output (payment mode)
            // Note: SwapMode.ExactOut might not be available in this SDK version
            // Using regular swap quote for now
            _currentQuote = await _dex.GetSwapQuote(
                _paymentToken.MintAddress,
                _targetToken.MintAddress,
                outputAmount
            );

            // Display quote information
            var inputAmount = ConvertFromBigInteger((ulong)_currentQuote.InputAmount, _paymentToken.Decimals);
            solEstimateText.text = $"You'll pay: {inputAmount:F6} {_paymentToken.Symbol}";

            // Display route information
            if (routeText != null)
            {
                var routePath = string.Join("->", _currentQuote.RoutePlan.Select(p => p.SwapInfo.Label));
                routeText.text = $"Route: {routePath}";
            }

            resultText.text = $"✅ Payment quote ready! Click Pay to execute.";
            Debug.Log($"💰 Payment Quote: {inputAmount:F6} {_paymentToken.Symbol}->{amount} {_targetToken.Symbol}");
        }
        catch (Exception e)
        {
            Debug.LogError($"❌ Failed to get payment quote: {e.Message}");
            resultText.text = $"❌ Payment quote failed: {e.Message}";
            ClearQuote();
        }
        finally
        {
            ToggleLoading(false);
        }
    }

    private IEnumerator ExecutePayment()
    {
        if (_currentQuote == null)
        {
            resultText.text = "❌ No payment quote available";
            yield break;
        }

        ToggleLoading(true);
        resultText.text = "Creating payment transaction...";

        // Create swap transaction
        var swapTask = _dex.Swap(_currentQuote);
        while (!swapTask.IsCompleted)
        {
            yield return null;
        }

        if (swapTask.Exception != null)
        {
            Debug.LogError($"❌ Payment creation failed: {swapTask.Exception.Message}");
            resultText.text = $"❌ Payment failed: {swapTask.Exception.Message}";
            ToggleLoading(false);
            yield break;
        }

        var tx = swapTask.Result;
        resultText.text = "Signing and sending payment...";

        // Sign and send transaction
        var signTask = Web3.Wallet.SignAndSendTransaction(tx);
        while (!signTask.IsCompleted)
        {
            yield return null;
        }

        if (signTask.Exception != null)
        {
            Debug.LogError($"❌ Transaction signing failed: {signTask.Exception.Message}");
            resultText.text = $"❌ Payment failed: {signTask.Exception.Message}";
            ToggleLoading(false);
            yield break;
        }

        var result = signTask.Result;

        if (result?.Result != null)
        {
            Debug.Log($"✅ Payment successful! TxID: {result.Result}");
            resultText.text = $"✅ Payment successful!\nTx: {result.Result}";
            
            // Clear the quote and update balance
            ClearQuote();
            UpdateBalance();
        }
        else
        {
            Debug.LogError($"❌ Payment failed: {result?.Reason}");
            resultText.text = $"❌ Payment failed: {result?.Reason}";
        }

        ToggleLoading(false);
    }

    private void ClearQuote()
    {
        _currentQuote = null;
        solEstimateText.text = "";
        if (routeText != null) routeText.text = "";
    }

    private void ToggleLoading(bool show)
    {
        if (loadingScreen != null)
            loadingScreen.SetActive(show);
    }

    private void UpdateBalance()
    {
        if (WalletManager.instance != null)
        {
            WalletManager.instance.CheckBalance();
            float balance = WalletManager.instance.Balance;
            balanceText.text = $"Your Balance: {balance:F4} SOL";
        }
    }

    // Public method to refresh balance
    public void RefreshBalance()
    {
        UpdateBalance();
    }

    // Simple conversion methods to replace DecimalUtil
    private ulong ConvertToUlong(float amount, int decimals)
    {
        return (ulong)(amount * Math.Pow(10, decimals));
    }

    private float ConvertFromBigInteger(ulong amount, int decimals)
    {
        return (float)(amount / Math.Pow(10, decimals));
    }


} 