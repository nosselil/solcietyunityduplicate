using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Text;
using System.Collections;
using System;

public class ArtworkModal : MonoBehaviour
{
    [Header("UI Elements")]
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI descriptionText;
    public TextMeshProUGUI bidText;
    public TextMeshProUGUI timeText;
    public TextMeshProUGUI previousBidsText;
    public TextMeshProUGUI artistNameText;
    public TextMeshProUGUI minBidRequiredText;
    public RawImage artworkImage;
    public Button bidButton;
    public TMP_InputField bidInputField;

    public GameObject modalPanel;
    private CanvasGroup canvasGroup;
    private string auctionAddress;

    [Header("Fade Settings")]
    public float fadeDuration = 0.3f;

    [Header("Bid Manager")]
    public MallowBidManager bidManager;

    // Internal state
    private Action onBidCallback;
    private string mintAddress;
    private float bidAmountSOL;

    void Awake()
    {
        canvasGroup = modalPanel.GetComponent<CanvasGroup>();
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }
    }

    public void Open(
        string title,
        string description,
        float bid,
        string time,
        string[] prevBids,
        string artist,
        Texture2D artworkTex,
        string mint,
        string auction,
        Action onBid = null)
    {
        titleText.text = title;
        descriptionText.text = description;
        bidText.text = $"Bid on artwork";
        timeText.text = time;
        auctionAddress = auction;

        if (minBidRequiredText != null)
        {
            minBidRequiredText.text = $"Minimum Bid Required: {bid:F9} SOL";
        }

        mintAddress = mint;
        bidAmountSOL = bid;
        onBidCallback = onBid;

        if (artistNameText != null)
            artistNameText.text = $"By {artist}";

        if (artworkImage != null && artworkTex != null)
            artworkImage.texture = artworkTex;

        if (prevBids != null && prevBids.Length > 0)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Previous Bidders:");
            foreach (string bidder in prevBids)
                sb.AppendLine("• " + TruncateAddress(bidder));
            previousBidsText.text = sb.ToString();
        }
        else
        {
            previousBidsText.text = "No previous bids.";
        }

        if (bidInputField != null)
        {
            bidInputField.text = bid.ToString("F2");
        }

        if (bidButton != null)
        {
            bidButton.onClick.RemoveAllListeners();
            bidButton.onClick.AddListener(() =>
            {
                Debug.Log("🟢 Bid button clicked");

                float userBid = bidAmountSOL;
                if (bidInputField != null)
                {
                    if (!float.TryParse(bidInputField.text, out userBid))
                    {
                        Debug.LogError($"❌ Invalid bid input: '{bidInputField.text}'");
                        return;
                    }
                }

                if (onBidCallback != null)
                {
                    Debug.Log("🟢 Using callback bid flow");
                    onBidCallback.Invoke();
                }
                else if (bidManager != null && !string.IsNullOrEmpty(mintAddress))
                {
                    Debug.Log("🟢 Using direct bid manager flow");
                    StartCoroutine(bidManager.SubmitBid(mintAddress, auctionAddress, userBid, bidAmountSOL));
                }
                else
                {
                    Debug.LogError("❌ BidManager or mintAddress missing.");
                }
            });
        }

        modalPanel.SetActive(true);
        StartCoroutine(FadeIn());
    }

    public void Close()
    {
        StartCoroutine(FadeOut());
    }

    IEnumerator FadeIn()
    {
        float elapsed = 0;
        Vector3 originalScale = modalPanel.transform.localScale;
        modalPanel.transform.localScale = originalScale * 0.95f;

        while (elapsed < fadeDuration)
        {
            modalPanel.transform.localScale = Vector3.Lerp(originalScale * 0.95f, originalScale, elapsed / fadeDuration);
            elapsed += Time.deltaTime;
            canvasGroup.alpha = Mathf.Clamp01(elapsed / fadeDuration);
            yield return null;
        }

        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;
    }

    IEnumerator FadeOut()
    {
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;

        float elapsed = 0;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            canvasGroup.alpha = 1 - Mathf.Clamp01(elapsed / fadeDuration);
            yield return null;
        }

        modalPanel.SetActive(false);
    }

    private string TruncateAddress(string address)
    {
        if (string.IsNullOrEmpty(address) || address.Length < 8)
            return address;

        return address.Substring(0, 4) + "..." + address[^4..];
    }

    public float GetUserBid()
    {
        if (bidInputField != null && float.TryParse(bidInputField.text, out float userBid))
            return userBid;
        return bidAmountSOL; // fallback to min bid
    }
}
