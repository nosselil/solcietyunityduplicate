using UnityEngine;

public class NFTMetadataHolder : MonoBehaviour
{
    public string nftName;   // Art name
    public string artistName; // Artist name
    public string mintAddress; // Mint address
    public string devnetMintAddress; // Mint address
    public string testnetMintAddress; // Mint address
    public string uri;
    private void Start()
    {
      //  Debug.LogError(gameObject.name + " : NFTMetadataHolder");
      //  if (!WalletManager.instance.isMainnet) mintAddress = devnetMintAddress;
        if(WalletManager.instance.currentNetwork == WalletManager.Network.MainNet)
        {
            mintAddress = mintAddress;
        }
        else if (WalletManager.instance.currentNetwork == WalletManager.Network.DevNet)
        {
            mintAddress = devnetMintAddress;
        }
        else if (WalletManager.instance.currentNetwork == WalletManager.Network.TestNet)
        {
            mintAddress = testnetMintAddress;
        }
    }


    private void Update()
    {
       
    }
}
    