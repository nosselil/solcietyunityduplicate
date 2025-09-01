# Jupiter Token Search Setup Guide

This guide will help you add the token search functionality to your Jupiter canvas.

## 🎯 **What This Adds**

- **Search Button**: Click to open search panel
- **Contract Address Input**: Enter any token's contract address
- **Token Information Display**: Shows name, symbol, market cap, volume, price
- **Auto-Set as Token B**: Automatically sets searched token as the target token for swaps

## 📋 **UI Setup Requirements**

### **1. Search Panel UI Elements**
```
Search Panel (GameObject)
├── Search Button (Button)
├── Search Panel (GameObject) - Initially hidden
│   ├── Contract Address Input (TMP_InputField)
│   ├── Search Confirm Button (Button)
│   └── Search Cancel Button (Button)
└── Token Info Panel (GameObject) - Initially hidden
    ├── Token Name Text (TMP_Text)
    ├── Token Symbol Text (TMP_Text)
    ├── Token Market Cap Text (TMP_Text)
    ├── Token Volume Text (TMP_Text)
    ├── Token Price Text (TMP_Text)
    └── Token Icon Image (Image)
```

### **2. Required Components**
- `JupiterTokenSearch` script on the search panel
- `JupiterSDKManager` reference assigned
- All UI elements properly connected

## 🔧 **Setup Steps**

### **Step 1: Create the Search Panel**

1. **Create Search Button**
   - Add a Button to your Jupiter canvas
   - Set text to "Search Token" or use an icon
   - Position it near your token dropdowns

2. **Create Search Panel**
   - Create a Panel GameObject as child of your main canvas
   - Add a background image for the panel
   - Set it to inactive initially

3. **Add Input Field**
   - Add TMP_InputField to the search panel
   - Set placeholder text: "Enter token contract address..."
   - Set character limit to 44 (Solana address length)

4. **Add Action Buttons**
   - Add "Search" and "Cancel" buttons
   - Position them below the input field

### **Step 2: Create Token Info Display**

1. **Create Info Panel**
   - Create a Panel GameObject for displaying token info
   - Set it to inactive initially

2. **Add Text Elements**
   - Token Name: "Token Name"
   - Token Symbol: "SYMBOL"
   - Market Cap: "Market Cap: $0"
   - 24h Volume: "24h Volume: $0"
   - Price: "Price: $0.000000"

3. **Add Icon Image**
   - Add an Image component for the token icon
   - Set default size (e.g., 50x50)

### **Step 3: Add the Script**

1. **Add JupiterTokenSearch Component**
   - Add the `JupiterTokenSearch` script to your search panel
   - Assign all UI references in the inspector

2. **Connect to JupiterSDKManager**
   - Drag your `JupiterSDKManager` object to the `jupiterManager` field

## 📝 **Inspector Setup**

### **Search UI References**
- `Search Button`: Your search button
- `Search Panel`: The panel containing the search input
- `Contract Address Input`: TMP_InputField for contract address
- `Search Confirm Button`: Button to confirm search
- `Search Cancel Button`: Button to cancel search

### **Token Info Display**
- `Token Name Text`: TMP_Text for token name
- `Token Symbol Text`: TMP_Text for token symbol
- `Token Market Cap Text`: TMP_Text for market cap
- `Token Volume Text`: TMP_Text for 24h volume
- `Token Price Text`: TMP_Text for price
- `Token Icon Image`: Image for token icon
- `Token Info Panel`: Panel containing token info

### **Jupiter SDK Reference**
- `Jupiter Manager`: Reference to your JupiterSDKManager

### **Settings**
- `Auto Set As Token B`: Automatically set searched token as Token B
- `Show Token Info`: Display token information panel

## 🎮 **Usage Flow**

1. **User clicks "Search Token" button**
   - Search panel opens
   - Input field is focused

2. **User enters contract address**
   - Can paste or type the address
   - Press Enter or click Search

3. **System searches for token**
   - Tries Jupiter API first
   - Falls back to Helius API if needed
   - Shows loading state

4. **Token info is displayed**
   - Name, symbol, market cap, volume, price
   - Token icon (if available)
   - Confirm button becomes active

5. **User clicks "Confirm"**
   - Token is set as Token B
   - Search panel closes
   - Quote is updated automatically

## 🔍 **Example Contract Addresses for Testing**

```
BONK: DezXAZ8z7PnrnRJjz3wXBoRgixCa6xjnB7YaB1pPB263
JUP: JUPyiwrYJFskUPiHa7hkeR8VUtAeFoSYbKedZNsDvCN
RAY: 4k3Dyjzvzp8eMZWUXbBCjEvwSkkk59S5iCNLY3QrkX6R
SRM: SRMuApVNdxXokk5GT7XD5cUUgXMBCoAz2LHeuAoKWRt
```

## 🐛 **Troubleshooting**

### **"Token not found" error**
- Check if the contract address is correct
- Verify the token exists on Solana
- Check internet connection

### **Search panel not opening**
- Verify the search button is assigned
- Check if the search panel is active in hierarchy
- Ensure the script is enabled

### **Token B not updating**
- Verify JupiterSDKManager reference is assigned
- Check console for error messages
- Ensure the token data is valid

### **Token info not displaying**
- Check all text components are assigned
- Verify the token info panel is active
- Check console for API errors

## 🚀 **Advanced Features**

### **Custom Styling**
- Modify the panel backgrounds and colors
- Add animations for panel transitions
- Customize the token info layout

### **Additional Data**
- Add more token information fields
- Display token holder count
- Show token verification status

### **Error Handling**
- Add retry functionality for failed searches
- Show more detailed error messages
- Add offline fallback options

## 📞 **Support**

If you encounter issues:
1. Check the Unity Console for error messages
2. Verify all UI references are assigned
3. Test with known contract addresses
4. Check your internet connection

---

**Happy Token Searching! 🎉** 