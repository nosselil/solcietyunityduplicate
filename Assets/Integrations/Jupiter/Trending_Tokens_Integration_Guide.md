# Trending Tokens → Jupiter Search Integration

This guide explains how to connect your trending tokens display with the Jupiter search functionality.

## 🎯 **What This Adds**

When users click on a trending token, it will:
- ✅ **Open Jupiter Canvas** (if not already open)
- ✅ **Open Search Panel** automatically
- ✅ **Fill Contract Address** with the clicked token's mint address
- ✅ **Trigger Search** automatically to show token info
- ✅ **Set as Token B** when user confirms

## 🔧 **Setup Instructions**

### **Step 1: Update JupiterTokenItemUI Prefab**

1. **No Manual References Needed!** The script automatically finds references at runtime:
   - `Jupiter Token Search`: Automatically found via `JupiterTokenSearch.Instance`
   - `Jupiter Canvas`: Automatically found by name ("JupiterCanvas", "Jupiter Canvas", or tag "JupiterCanvas")

2. **Make Token Items Clickable**:
   - Ensure the token item has a Button or Image component
   - The script now implements `IPointerClickHandler` automatically

### **Step 2: Ensure Scene Setup**

Make sure your scene has:

```
Scene Hierarchy:
├── JupiterCanvas (or "Jupiter Canvas")
│   ├── JupiterTokenSearch component
│   └── Search Panel
└── Trending Tokens Canvas
    └── Token Item Prefabs (with JupiterTokenItemUI)
```

**Note:** The script automatically finds references, so no manual assignment needed!

### **Step 3: Test the Integration**

1. **Click any trending token**
2. **Jupiter canvas opens** (if not already open)
3. **Search panel opens** automatically
4. **Contract address is filled** with the token's mint address
5. **Token info appears** automatically
6. **Click "Confirm"** to set as Token B for swaps

## 🎮 **User Flow**

### **Before Integration:**
1. User sees trending tokens
2. User manually opens Jupiter search
3. User manually types/pastes contract address
4. User searches for token

### **After Integration:**
1. User clicks trending token
2. Jupiter search opens automatically
3. Contract address is pre-filled
4. Token info appears instantly
5. User can immediately swap

## 📋 **Required Components**

### **JupiterTokenItemUI Script:**
- ✅ Implements `IPointerClickHandler`
- ✅ Stores current token data
- ✅ Opens Jupiter canvas
- ✅ Sets contract address
- ✅ Triggers automatic search

### **JupiterTokenSearch Script:**
- ✅ Public `SearchTokenByAddress()` method
- ✅ Public `contractAddressInput` field
- ✅ Public `searchPanel` field

## 🔍 **Example Usage**

```csharp
// When user clicks a trending token:
// 1. Token ID is automatically set in search input
// 2. Search panel opens
// 3. Search is triggered automatically
// 4. Token info is displayed
// 5. User can confirm to set as Token B
```

## 🐛 **Troubleshooting**

### **"JupiterTokenSearch not found"**
- Make sure JupiterTokenSearch component is in the scene
- Ensure only one JupiterTokenSearch exists (for singleton pattern)
- Check that the component is enabled

### **"Jupiter Canvas not found"**
- Name your Jupiter canvas "JupiterCanvas" or "Jupiter Canvas"
- Or add the tag "JupiterCanvas" to your canvas
- Make sure the canvas exists in the scene

### **Click not working**
- Ensure the token item has a Button or Image component
- Check that the GameObject has a Collider2D (for 2D) or Collider (for 3D)
- Verify the Canvas Group allows interaction

### **Search not triggering**
- Check that the contract address is valid (44 characters)
- Verify the JupiterTokenSearch component is properly assigned
- Check console for error messages

## 🚀 **Advanced Features**

### **Custom Click Behavior**
You can modify the `OnPointerClick` method to add:
- Sound effects
- Visual feedback
- Analytics tracking
- Custom animations

### **Multiple Search Panels**
If you have multiple search panels, you can:
- Add a panel selector
- Choose which panel to open
- Handle different search contexts

### **Error Handling**
Add error handling for:
- Invalid token addresses
- Network failures
- Missing references

## 📞 **Support**

If you encounter issues:
1. Check the Unity Console for error messages
2. Verify all references are assigned
3. Test with known working token addresses
4. Ensure the Jupiter search functionality works independently

---

**Happy Token Clicking! 🎉** 