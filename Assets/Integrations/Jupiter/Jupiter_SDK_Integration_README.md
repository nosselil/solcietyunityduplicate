# Jupiter SDK Integration for Unity

This project now uses the official **Solana Unity SDK Jupiter integration** instead of custom HTTP requests. This provides better performance, reliability, and access to advanced features.

## 🚀 What's New

### ✅ **Official SDK Integration**
- Uses `JupiterDexAg` from `Solana.Unity.Dex.Jupiter`
- Native Unity SDK integration
- Automatic API updates and maintenance
- Better error handling and debugging

### ✅ **Enhanced Features**
- **Route Display**: Shows which DEXes are being used for swaps
- **Token Dropdowns**: Easy token selection with common tokens
- **Exact Output Swaps**: Jupiter Payments API for payment scenarios
- **Token Information**: Search and display token metadata
- **Better UI**: More intuitive user interface

### ✅ **Two Integration Options**

1. **`JupiterSDKManager.cs`** - Regular swaps (SOL → USDC, etc.)
2. **`JupiterPaymentsManager.cs`** - Payment swaps (simplified version)

## 📁 Files Overview

### Core Files
- `JupiterSDKManager.cs` - Main swap functionality
- `JupiterPaymentsManager.cs` - Payment functionality (simplified)

### Legacy Files (Can be removed)
- `JupiterSwapManager.cs` - Old custom implementation
- `JupiterSwapUI.cs` - Old UI implementation

## 🔧 Setup Instructions

### 1. **Replace Your Old Implementation**

If you're using the old `JupiterSwapManager`, replace it with `JupiterSDKManager`:

```csharp
// OLD (Remove this)
public JupiterSwapManager swapManager;

// NEW (Use this instead)
public JupiterSDKManager jupiterManager;
```

### 2. **Update UI References**

The new manager requires these UI components:

```csharp
[Header("UI References")]
public TMP_InputField amountInput;        // Amount to swap
public TMP_Text resultText;               // Status messages
public TMP_Text usdcEstimateText;         // Quote display
public TMP_Text balanceText;              // Balance display
public TMP_Text routeText;                // Route information
public GameObject loadingScreen;          // Loading indicator
public TMP_Dropdown tokenADropdown;       // Token A selection
public TMP_Dropdown tokenBDropdown;       // Token B selection
```

### 3. **Add Event Listeners**

```csharp
// In your UI setup
amountInput.onValueChanged.AddListener(_ => jupiterManager.OnAmountChanged());
swapButton.onClick.AddListener(jupiterManager.OnSwapButtonClick);
```

## 🎯 Usage Examples

### **Basic Swap (SOL → USDC)**

```csharp
// The manager handles everything automatically
// Just call the swap method
jupiterManager.OnSwapButtonClick();
```

### **Payment Scenario (USDC Payment)**

```csharp
// Use JupiterPaymentsManager for payment swaps
// Simplified version for payment scenarios
paymentsManager.OnPaymentButtonClick();
```

### **Token Information**

```csharp
// Search for token information
var token = await dex.GetTokenBySymbol("SOL");
Debug.Log($"Price: ${token.UsdPrice}");
Debug.Log($"Market Cap: ${token.Mcap}");
Debug.Log($"Holders: {token.HolderCount}");
```

## 🔄 Migration Guide

### **Step 1: Update Your Scene**

1. Remove the old `JupiterSwapManager` component
2. Add the new `JupiterSDKManager` component
3. Reassign all UI references

### **Step 2: Update Your Code**

```csharp
// OLD CODE
yield return StartCoroutine(swapManager.GetQuoteSOLtoUSDC(lamports, callback));

// NEW CODE
// The manager handles quotes automatically when amount changes
jupiterManager.OnAmountChanged();
```

### **Step 3: Test the Integration**

1. Connect your wallet
2. Enter an amount
3. Select tokens from dropdowns
4. View the route information
5. Execute the swap

## 🌟 Key Benefits

### **1. Simpler Code**
- No more manual HTTP requests
- No more JSON parsing
- No more transaction building

### **2. Better Features**
- **Route Display**: See which DEXes are used
- **Token Selection**: Dropdown with common tokens
- **Real-time Quotes**: Automatic quote updates
- **Better Error Handling**: SDK-level error management

### **3. Future-Proof**
- Automatic API updates
- Official SDK maintenance
- New features as they're released

## 🎮 Example Scene

The `JupiterExampleScene.cs` provides a complete example with:

- **Swap Panel**: Regular token swaps
- **Payment Panel**: Payment swaps
- **Token Info Panel**: Token search and information
- **Route Display**: Shows which DEXes are used
- **Balance Updates**: Automatic balance refresh

## 🔍 Troubleshooting

### **Common Issues**

1. **"Web3.Account is null"**
   - Make sure wallet is connected before initializing Jupiter

2. **"Failed to get quote"**
   - Check internet connection
   - Verify token symbols are correct
   - Ensure sufficient balance

3. **"Swap failed"**
   - Check transaction logs for specific error
   - Verify slippage settings
   - Ensure wallet has enough SOL for fees

### **Debug Information**

The new integration provides detailed debug logs:

```
✅ Jupiter SDK initialized. Token A: SOL, Token B: USDC
📊 Quote: 1 SOL → 145.471142 USDC
🛣️ Route: Lifinity V2 → Whirlpool
✅ Swap successful! TxID: 5J7X...
```

## 📚 Additional Resources

- [Jupiter Documentation](https://docs.jup.ag/)
- [Solana Unity SDK](https://docs.solana.com/developing/clients/unity-sdk)
- [Jupiter API Reference](https://station.jup.ag/docs/apis/swap-api)

## 🚀 Next Steps

1. **Test the new integration** in your development environment
2. **Update your UI** to use the new components
3. **Remove old files** once migration is complete
4. **Add route display** to show users which DEXes are used
5. **Implement payment scenarios** using the payments manager

---

**Need Help?** Check the debug logs for detailed error information, or refer to the Jupiter documentation for API-specific issues. 