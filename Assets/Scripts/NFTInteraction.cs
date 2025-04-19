using UnityEngine;
using UnityEngine.UI; // For RawImage
using TMPro; // For TextMeshPro
using Newtonsoft.Json.Linq; // For JSON Parsing
using System;
using System.Collections;

public class NFTInteraction : MonoBehaviour
{
    public GameObject popupPanel; // Assign UI Panel in Inspector
    public TextMeshProUGUI artistText; // Assign Artist TextMeshProUGUI
    public TextMeshProUGUI artworkText; // Assign Artwork Name TextMeshProUGUI
    public TextMeshProUGUI artworkText2; // Assign another TextMeshProUGUI for second artwork name
    public RawImage artworkImage; // Assign RawImage UI in Inspector
    public NftManager buyNFTfromGalleryScript; // Reference to the BuyNFTfromGallery script
    string nftMintAddress;

    private string currentMetadata; // Store metadata of collided NFT
    string nftArtist;



    public void ShowNftDetailsPopup()
 {
 if ( currentMetadata != null) {
 Debug.LogError("ShowNftDetailsPopup");
 ShowNFTDetails(currentMetadata);
 }
}
   private void OnTriggerEnter(Collider other)
    {
        Debug.Log("ART: Entered trigger");

        NFTMetadataHolder metadataHolder = other.gameObject.GetComponent<NFTMetadataHolder>();
        if (metadataHolder != null)
        {
            Debug.Log("ART: Metadata holder is present");
            currentMetadata = metadataHolder.nftName; // Store metadata
            nftArtist = metadataHolder.artistName;
            // Get the mint address directly from the NFTMetadataHolder
            nftMintAddress = metadataHolder.mintAddress;


            buyNFTfromGalleryScript.mintName = metadataHolder.nftName;
            buyNFTfromGalleryScript.mintUri = metadataHolder.uri;

            Debug.LogError(buyNFTfromGalleryScript.mintUri);

            if (!string.IsNullOrEmpty(nftMintAddress))
            {
                Debug.Log("NFT Mint Address: " + nftMintAddress);
                if (buyNFTfromGalleryScript != null)
                {
                   buyNFTfromGalleryScript.nftMintAddress = nftMintAddress ; // Assign mint address to BuyNFTfromGallery
                }
            }
            else
            {
                Debug.LogWarning("Mint address not found in NFTMetadataHolder.");
            }
        }

        // Check if the collided object has a renderer and texture
        Renderer renderer = other.gameObject.GetComponent<Renderer>();
        if (renderer != null)
        {
            Material material = renderer.material; // Get the material of the mesh
            Texture texture = material.mainTexture; // Get the main texture from the material

            if (texture != null && artworkImage != null)
            {
                // Display the texture on the RawImage
                artworkImage.texture = texture;
            }
            else
            {

                Debug.LogWarning(gameObject.name+" No texture found on the collided object's material.");
            }
        }
    }
    private void OnTriggerExit(Collider other)
    {
        currentMetadata = null;
        ClosePopup();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.E) && currentMetadata != null) // Press 'E' Key
        {
            ShowNFTDetails(currentMetadata);
        }
    }

    private void ShowNFTDetails(string metadata)
    {
            // Assign values to UI elements
            artworkText.text = metadata;
            artworkText2.text = metadata;
            artistText.text = nftArtist;

            if (popupPanel != null)
                popupPanel.SetActive(true);
       
    }


    public void ClosePopup()
    {
        if (popupPanel != null)
            popupPanel.SetActive(false);
       // currentMetadata = null; // Reset metadata
    }
}
