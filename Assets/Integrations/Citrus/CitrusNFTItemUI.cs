using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CitrusNFTItemUI : MonoBehaviour
{
    [Header("UI Components")]
    [SerializeField] private Image nftImage;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI mintText;
    [SerializeField] private Button selectButton;
    
    public void SetNFTData(string name, string mintAddress, Sprite image = null)
    {
        if (nameText != null)
            nameText.text = name;
        
        if (mintText != null)
            mintText.text = ShortenAddress(mintAddress);
        
        if (nftImage != null && image != null)
            nftImage.sprite = image;
    }
    
    public void SetSelected(bool selected)
    {
        if (selectButton != null)
        {
            Image buttonImage = selectButton.GetComponent<Image>();
            if (buttonImage != null)
            {
                buttonImage.color = selected ? Color.green : Color.white;
            }
        }
    }
    
    private string ShortenAddress(string address)
    {
        if (string.IsNullOrEmpty(address) || address.Length < 8)
            return address;
        
        return address.Substring(0, 4) + "..." + address.Substring(address.Length - 4);
    }
} 