using UnityEngine;
using TMPro;

public class ArtworkInteractable : MonoBehaviour
{
    public string Title;
    public string Description;
    public float MinBidSOL;
    public string TimeRemaining;
    public string[] PreviousBids;
    public string ArtistName;
    public Texture2D ArtworkTexture;

    public ArtworkModal Modal;
    public GameObject interactionPrompt; // Assign "Press E" Text here
    public string MintAddress; // Required by Mallow API
    public string AuctionAddress;

    private MallowBidManager bidManager;

    private void Start()
    {
        if (interactionPrompt != null)
            interactionPrompt.SetActive(false);

        bidManager = FindObjectOfType<MallowBidManager>();
        if (bidManager == null)
            Debug.LogWarning("⚠️ No MallowBidManager found in the scene.");
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && interactionPrompt != null)
        {
            interactionPrompt.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") && interactionPrompt != null)
        {
            interactionPrompt.SetActive(false);
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player") && Input.GetKeyDown(KeyCode.E))
        {
            ShowModal();
            if (interactionPrompt != null)
                interactionPrompt.SetActive(false);
        }
    }

    public void ShowModal()
    {
        if (Modal == null)
        {
            Debug.LogWarning("⚠️ ArtworkModal is not assigned.");
            return;
        }

        Modal.Open(
    Title,
    Description,
    MinBidSOL,
    TimeRemaining,
    PreviousBids,
    ArtistName,
    ArtworkTexture,
    MintAddress,
    AuctionAddress, // ✅ add this line
    () =>
    {
        if (bidManager != null)
        {
            float userBid = Modal.GetUserBid();
            bidManager.StartCoroutine(bidManager.SubmitBid(MintAddress, AuctionAddress, userBid, MinBidSOL));
        }
        else
        {
            Debug.LogError("❌ No bidManager found during bid submission.");
        }
    }
);

    }
}
