using TMPro;
using UnityEngine;
using UnityEngine.UI;

// This class will allow you to set up dependencies through drag and drop in the inspector for all player components that are only used in specific scenes. For example, the NFT Interaction
// is only used in gallery, so you can set up the the required dependencies in this script via the inspector in the Gallery scene.
// These dependencies will then by copied to the corresponding components in the player once the networked Player object is spawned (this is done in PlayerSetup.cs).
public class DependencyContainer : MonoBehaviour
{
    [Header("NFT Interaction")]
    public GameObject popupPanel;
    public TextMeshProUGUI artistText, artworkText, artworkText2;
    public RawImage artworkImage; //Check type
    public NftManager buyNFTfromGalleryScript;

    [Header("TextMeshFader")]
    public GameObject[] textMeshes;
    
}
