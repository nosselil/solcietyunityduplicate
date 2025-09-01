using System;
using UnityEngine;
using System.Collections.Generic;
using TMPro;

// Modal for displaying user's Kamino lend/borrow activity
public class KaminoUserActivityModal : MonoBehaviour
{
    [Header("UI References")]
    public Transform contentParent; // The parent for position entries (e.g., VerticalLayoutGroup)
    public GameObject positionPrefab; // Prefab for each position row (should have KaminoUserActivityPositionUI)
    public TextMeshProUGUI statusText; // Status/error message display

    // Track position UI instances for removal
    private List<KaminoUserActivityPositionUI> positionUIs = new List<KaminoUserActivityPositionUI>();

    // Show the modal with a list of user positions
    public void Show(List<UserPosition> positions)
    {
        // Debug log all positions
        Debug.Log($"[KaminoUserActivityModal] Show() received {positions.Count} positions:");
        foreach (var pos in positions)
        {
            Debug.Log($"Symbol: {pos.symbol}, Amount: {pos.amount}, APY: {pos.apy}, isLend: {pos.isLend}, Date: {pos.date}, cTokenBalance: {pos.cTokenBalance}");
        }
        // Clear old entries
        ClearAllPositions();

        // Populate with new positions
        foreach (var pos in positions)
        {
            var go = Instantiate(positionPrefab, contentParent);
            var ui = go.GetComponent<KaminoUserActivityPositionUI>();
            if (ui != null)
            {
                ui.Setup(pos, this);
                positionUIs.Add(ui);
            }
        }

        gameObject.SetActive(true);
    }

    // Clear all position UIs
    private void ClearAllPositions()
    {
        foreach (Transform child in contentParent)
            Destroy(child.gameObject);
        positionUIs.Clear();
    }

    // Remove a specific position UI
    public void RemovePosition(KaminoUserActivityPositionUI positionUI)
    {
        if (positionUIs.Contains(positionUI))
        {
            positionUIs.Remove(positionUI);
            Destroy(positionUI.gameObject);
        }
    }

    // Hide the modal
    public void Hide()
    {
        gameObject.SetActive(false);
    }

    // Set a status message (success or error)
    public void SetStatus(string message, bool isError)
    {
        if (statusText != null)
        {
            statusText.text = message;
            statusText.color = isError ? Color.red : Color.green;
        }
    }

    // Called by prefab button for withdraw
    public void OnWithdraw(UserPosition pos)
    {
        Debug.Log($"[KaminoUserActivityModal] OnWithdraw called for symbol: {pos.symbol}, cTokenBalance: {pos.cTokenBalance}");
        // Only allow withdraw if cTokenBalance is present and > 0
        if (string.IsNullOrEmpty(pos.cTokenBalance) || float.Parse(pos.cTokenBalance) <= 0f)
        {
            SetStatus($"No withdrawable cTokens for {pos.symbol}. The cToken account may not exist. Try supplying a small amount first to create it.", true);
            return;
        }
        
        // Show loading spinner
        if (KaminoAPI.Instance.loadingSpinner != null)
        {
            KaminoAPI.Instance.loadingSpinner.Show("Processing withdraw transaction...");
        }
        
        float cTokenAmount = float.Parse(pos.cTokenBalance);
        KaminoAPI.Instance.Withdraw(pos.symbol, cTokenAmount, (success, response) => {
            // Hide loading spinner
            if (KaminoAPI.Instance.loadingSpinner != null)
            {
                KaminoAPI.Instance.loadingSpinner.Hide();
            }
            if (success)
            {
                SetStatus($"Withdraw successful for {pos.symbol}, cToken amount: {cTokenAmount}", false);
                // Remove the position UI from the modal
                RemovePositionBySymbol(pos.symbol, true);
            }
            else
            {
                // Parse the error response to get user-friendly message
                try
                {
                    // Try to parse as JSON to get the error message
                    if (response.Contains("\"error\":"))
                    {
                        // Extract error message from JSON response
                        int errorStart = response.IndexOf("\"error\":\"") + 9;
                        int errorEnd = response.IndexOf("\"", errorStart);
                        if (errorStart > 8 && errorEnd > errorStart)
                        {
                            string errorMessage = response.Substring(errorStart, errorEnd - errorStart);
                            SetStatus($"Withdraw failed: {errorMessage}", true);
                        }
                        else
                        {
                            SetStatus($"Withdraw failed: {response}", true);
                        }
                    }
                    else
                    {
                        SetStatus($"Withdraw failed: {response}", true);
                    }
                }
                catch
                {
                    SetStatus($"Withdraw failed: {response}", true);
                }
            }
        });
    }

    // Called by prefab button for repay
    public void OnRepay(UserPosition pos)
    {
        // Show loading spinner
        if (KaminoAPI.Instance.loadingSpinner != null)
        {
            KaminoAPI.Instance.loadingSpinner.Show("Processing repay transaction...");
        }
        
        KaminoAPI.Instance.Repay(pos.symbol, pos.amount, (success, response) => {
            // Hide loading spinner
            if (KaminoAPI.Instance.loadingSpinner != null)
            {
                KaminoAPI.Instance.loadingSpinner.Hide();
            }
            if (success)
            {
                SetStatus($"Repay successful for {pos.symbol}, amount: {pos.amount}", false);
                // Remove the position UI from the modal
                RemovePositionBySymbol(pos.symbol, false);
            }
            else
            {
                // Parse the error response to get user-friendly message
                try
                {
                    // Try to parse as JSON to get the error message
                    if (response.Contains("\"error\":"))
                    {
                        // Extract error message from JSON response
                        int errorStart = response.IndexOf("\"error\":\"") + 9;
                        int errorEnd = response.IndexOf("\"", errorStart);
                        if (errorStart > 8 && errorEnd > errorStart)
                        {
                            string errorMessage = response.Substring(errorStart, errorEnd - errorStart);
                            SetStatus($"Repay failed: {errorMessage}", true);
                        }
                        else
                        {
                            SetStatus($"Repay failed: {response}", true);
                        }
                    }
                    else
                    {
                        SetStatus($"Repay failed: {response}", true);
                    }
                }
                catch
                {
                    SetStatus($"Repay failed: {response}", true);
                }
            }
        });
    }

    // Remove position UI by symbol and type (lend/borrow)
    private void RemovePositionBySymbol(string symbol, bool isLend)
    {
        for (int i = positionUIs.Count - 1; i >= 0; i--)
        {
            var positionUI = positionUIs[i];
            if (positionUI != null && positionUI.GetPosition() != null)
            {
                var pos = positionUI.GetPosition();
                if (pos.symbol == symbol && pos.isLend == isLend)
                {
                    RemovePosition(positionUI);
                    break;
                }
            }
        }
    }
} 