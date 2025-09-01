# Citrus NFT Selection Modal Setup Guide

## Overview
The Citrus NFT Selection Modal allows users to select which specific NFT they want to use as collateral when taking a loan from a collection. This replaces the previous manual NFT mint address input approach.

## New Components

### 1. CitrusNFTSelectionModal.cs
- **Purpose**: Main modal controller that handles NFT loading, display, and selection
- **Features**: 
  - Loads user's NFTs filtered by collection
  - Displays NFT images, names, and shortened mint addresses
  - Handles NFT selection and confirmation
  - Shows loading states and error messages

### 2. CitrusNFTItemUI.cs
- **Purpose**: Individual NFT item UI component
- **Features**:
  - Displays NFT image, name, and mint address
  - Handles selection visual feedback
  - Reusable component for NFT items

## Setup Instructions

### Step 1: Create the Modal Prefab

1. **Create Modal Panel**:
   - Create a new UI Panel in your scene
   - Set it as a child of your Canvas
   - Add a background image/color for the modal overlay
   - Set the panel to be inactive by default

2. **Add Modal Components**:
   - Add `CitrusNFTSelectionModal` script to the modal panel
   - Create child UI elements:
     - **Title Text** (TextMeshPro): "Select NFT from [Collection Name]"
     - **NFT Container** (ScrollView): Container for NFT items
     - **Loading Panel**: Shows loading spinner
     - **No NFTs Text**: Shows when no NFTs found
     - **Selected NFT Text**: Shows currently selected NFT
     - **Close Button**: To close the modal
     - **Confirm Button**: To confirm NFT selection

### Step 2: Create NFT Item Prefab

1. **Create NFT Item**:
   - Create a UI Button as a prefab
   - Add child elements:
     - **NFT Image** (Image): For NFT thumbnail
     - **Name Text** (TextMeshPro): NFT name
     - **Mint Text** (TextMeshPro): Shortened mint address
   - Add `CitrusNFTItemUI` script to the button
   - Assign the UI components in the inspector

### Step 3: Configure CitrusAPI

1. **Update CitrusAPI**:
   - In your CitrusAPI GameObject, find the `CitrusNFTSelectionModal` field
   - Assign the modal prefab you created
   - Remove the old `nftMintInput` field (no longer needed)

### Step 4: Configure Modal References

1. **Assign UI References**:
   - In the `CitrusNFTSelectionModal` component, assign:
     - **Modal Panel**: The main modal GameObject
     - **Title Text**: The title TextMeshPro component
     - **NFT Container**: The ScrollView content transform
     - **NFT Item Prefab**: The NFT item prefab you created
     - **Close Button**: The close button
     - **Confirm Button**: The confirm button
     - **Selected NFT Text**: Text to show selected NFT
     - **Loading Panel**: Loading spinner panel
     - **No NFTs Text**: Text for when no NFTs found

## Usage Flow

1. **User clicks "Select" on a loan offer**
2. **Modal opens** and loads user's NFTs from that collection
3. **User selects an NFT** from the list
4. **User clicks "Confirm"** to proceed with the loan
5. **Modal closes** and the loan transaction is executed

## Collection Filtering

The modal filters NFTs by collection using the following logic:
- Checks `nftData.metaplexData.data.collection.key` against the collection ID
- Falls back to `nftData.metaplexData.data.offchainData.collection.family`

## Error Handling

- **No NFTs found**: Shows "No NFTs found in [Collection] collection"
- **Loading errors**: Shows loading spinner and handles network errors
- **Invalid collection**: Logs error and shows appropriate message

## Customization

### Styling
- Modify the NFT item prefab to match your UI design
- Adjust colors, fonts, and layout as needed
- Add animations for selection feedback

### Collection Detection
- If your collections use different metadata structures, modify the `IsNFTInCollection` method in `CitrusNFTSelectionModal.cs`

### Loading States
- Customize loading animations and messages
- Add progress indicators for large NFT collections

## Testing

1. **Test with collections that have NFTs**: Verify NFTs load and display correctly
2. **Test with empty collections**: Verify "no NFTs" message appears
3. **Test selection**: Verify NFT selection and confirmation works
4. **Test loan execution**: Verify the selected NFT is used in the loan transaction

## Troubleshooting

### Common Issues:
- **Modal not opening**: Check if `CitrusNFTSelectionModal` is assigned in CitrusAPI
- **No NFTs showing**: Check collection ID matching logic
- **Images not loading**: Verify NFT metadata contains valid image URLs
- **Selection not working**: Check button event listeners and UI component assignments

### Debug Information:
- Check Unity Console for detailed logs
- Verify collection IDs match between Citrus API and NFT metadata
- Test NFT loading with known working collections first 