using UnityEngine;
using UnityEngine.UI;

public class RawImageNFTDebugger : MonoBehaviour
{
    private string objectName;

    void Start()
    {
    }
    public void debug()
    {
            // Get the GameObject name of this RawImage
            objectName = gameObject.name;

            // Find all NFTMetadataHolder objects in the scene
            NFTMetadataHolder[] nftObjects = FindObjectsOfType<NFTMetadataHolder>();

            foreach (NFTMetadataHolder nft in nftObjects)
            {
                // Check if the GameObject name matches the metadata object's name
                if (nft.gameObject.name == objectName)
                {
                    // Debug the mint address
                    Debug.Log($"RawImage Object: {objectName}, Mint Address: {nft.mintAddress}");
                    return;
                }
            }

            // If no matching object was found
            Debug.LogError($"No NFTMetadataHolder found with a matching name for: {objectName}");
        }
    }

