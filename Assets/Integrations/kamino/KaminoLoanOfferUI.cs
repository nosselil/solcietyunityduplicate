using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class KaminoLoanOfferUI : MonoBehaviour
{
    public TextMeshProUGUI symbolText;
    public TextMeshProUGUI ltvText;
    public TextMeshProUGUI supplyApyText;
    public TextMeshProUGUI borrowApyText;
    public Button lendButton;
    public Button borrowButton; // Assign in the inspector
    public KaminoLendModal lendModal; // Assign this in the inspector to the modal in your scene
    public KaminoBorrowModal borrowModal; // Assign in inspector

    private KaminoLoanOffer currentOffer;

    public void SetOffer(KaminoLoanOffer offer)
    {
        Debug.Log($"SetOffer called for: {offer.symbol}");
        currentOffer = offer;
        symbolText.text = offer.symbol;
        ltvText.text = $"{offer.ltv}%";
        supplyApyText.text = $"{offer.supplyApy}%";
        borrowApyText.text = $"{offer.borrowApy}%";

        if (lendButton != null)
        {
            Debug.Log($"lendButton assigned for: {offer.symbol}");
            lendButton.onClick.RemoveAllListeners();
            lendButton.onClick.AddListener(OnLendClicked);
        }
        else
        {
            Debug.LogError($"lendButton is NULL for: {offer.symbol}");
        }
        if (borrowButton != null)
        {
            borrowButton.onClick.RemoveAllListeners();
            borrowButton.onClick.AddListener(OnBorrowClicked);
        }
        else
        {
            Debug.LogError($"borrowButton is NULL for: {offer.symbol}");
        }
        if (lendModal == null)
        {
            Debug.LogError($"lendModal is NULL for: {offer.symbol}");
        }
        if (borrowModal == null)
        {
            Debug.LogError($"borrowModal is NULL for: {offer.symbol}");
        }
    }

    private void OnLendClicked()
    {
        Debug.Log("OnLendClicked called for: " + (currentOffer != null ? currentOffer.symbol : "null"));
        if (lendModal != null && currentOffer != null)
        {
            // Show loading spinner immediately when lend button is clicked
            if (KaminoAPI.Instance.loadingSpinner != null)
            {
                KaminoAPI.Instance.loadingSpinner.Show("Opening lend modal...");
            }
            
            Debug.Log("Calling lendModal.Show for: " + currentOffer.symbol);
            lendModal.Show(currentOffer);
            
            // Hide loading spinner after a short delay (modal opening is instant)
            if (KaminoAPI.Instance.loadingSpinner != null)
            {
                StartCoroutine(HideLoadingAfterDelay(0.3f));
            }
        }
        else
        {
            Debug.LogError("lendModal or currentOffer is null in OnLendClicked");
        }
    }
    
    private System.Collections.IEnumerator HideLoadingAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (KaminoAPI.Instance.loadingSpinner != null)
        {
            KaminoAPI.Instance.loadingSpinner.Hide();
        }
    }

    private void OnBorrowClicked()
    {
        Debug.Log("OnBorrowClicked called for: " + (currentOffer != null ? currentOffer.symbol : "null"));
        if (currentOffer != null && borrowModal != null)
        {
            // Show loading spinner immediately when borrow button is clicked
            if (KaminoAPI.Instance.loadingSpinner != null)
            {
                KaminoAPI.Instance.loadingSpinner.Show("Loading borrow options...");
            }
            
            // Fetch user's borrowing capacity with callback
            KaminoAPI.Instance.FetchUserBorrowingCapacity((borrowingCapacity) => {
                // Hide loading spinner when data is ready
                if (KaminoAPI.Instance.loadingSpinner != null)
                {
                    KaminoAPI.Instance.loadingSpinner.Hide();
                }
                
                float available = borrowingCapacity != null ? borrowingCapacity.availableToBorrow : 0f;
                float collateral = borrowingCapacity != null ? borrowingCapacity.totalCollateral : 0f;
                
                borrowModal.Show(currentOffer, available, collateral);
            });
        }
        else
        {
            Debug.LogError("currentOffer or borrowModal is null in OnBorrowClicked");
        }
    }
} 