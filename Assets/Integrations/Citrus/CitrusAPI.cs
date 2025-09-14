using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using Solana.Unity.SDK;
using Solana.Unity.Rpc;
using Solana.Unity.Rpc.Models;
using Solana.Unity.Rpc.Types;

[Serializable]
public class Collection
{
    public string name;
    public string id;
    public float bestApy;
    public long bestOffer;
    public int duration;
    public int loansAvailable;
    public int loansTotal;
    public long pool;
    public float floor;
    public long latestLoan;
    public long volume;
    public AverageTerms averageTerms;
    public string tProjectId;
    public string imageUrl;
}

[Serializable]
public class AverageTerms
{
    public int duration;
    public int apy;
}

[Serializable]
public class CollectionsResponse
{
    public List<Collection> collections;
}

[Serializable]
public class Terms
{
    public long principal;
    public int apy;
    public int duration;
}

[Serializable]
public class LtvTerms
{
    public int ltvBps;
    public long maxOffer;
}

[Serializable]
public class LoanOffer
{
    public Terms terms;
    public string status;
    public string loanAccount;
    public string collectionConfig;
    public long creationTime;
    public LtvTerms ltvTerms;
    public string lender;
}

[Serializable]
public class LoanOfferListWrapper
{
    public LoanOffer[] offers;
}

public class CitrusAPI : MonoBehaviour
{
    private const string API_BASE_URL = "https://solcietyserver.vercel.app/api";
    private const string TENSOR_IMAGE_BASE_URL = "https://tensor.so/api/v1/collection/";

    public static CitrusAPI Instance { get; private set; }

    [Header("UI References")]
    [SerializeField] private Transform collectionsContainer; // Parent for collection elements
    [SerializeField] private GameObject collectionTemplate; // Template prefab for each collection
    [SerializeField] private Transform offersContainer; // Parent for user loan elements (My Offers)
    [SerializeField] private GameObject offerTemplate; // Template prefab for each offer
    [SerializeField] private GameObject availableOffersPanel; // Panel for available offers to borrow
    [SerializeField] private Transform availableOffersContainer; // Container for available offers
    [SerializeField] private GameObject availableOfferTemplate; // Template for available offers
    [SerializeField] private Button closeAvailableOffersButton; // Button to close available offers panel
    [SerializeField] private Button borrowButton; // Button to borrow selected offer
    
    [Header("NFT Selection Modal")]
    [SerializeField] private CitrusNFTSelectionModal nftSelectionModal; // Reference to the NFT selection modal

    // Add serialized reference to borrow manager
    [SerializeField] private CitrusClientBorrowManager borrowManager;

    private List<Collection> collections = new List<Collection>();
    private LoanOffer selectedOffer; // Currently selected offer for borrowing
    private string currentCollectionName; // Name of collection being viewed
    private string currentCollectionId; // ID of collection being viewed

    void Awake()
    {
        Instance = this;
        
        // Find and assign the borrow manager if not set
        if (borrowManager == null)
        {
            borrowManager = FindObjectOfType<CitrusClientBorrowManager>(includeInactive: true);
        }
    }

    void Start()
    {
        Debug.Log("CitrusAPI: Start() called!");
        Debug.Log("CitrusAPI: Starting to fetch collections...");
        
        // Initialize UI
        if (availableOffersPanel != null) availableOffersPanel.SetActive(false);
        if (closeAvailableOffersButton != null) closeAvailableOffersButton.onClick.AddListener(CloseAvailableOffersPanel);
        
        FetchCollections();
        FetchAndDisplayUserOffers();
    }

    private void OnEnable()
    {
        if (borrowManager != null)
        {
            borrowManager.OnBorrowStarted.AddListener(OnBorrowStarted_UI);
            borrowManager.OnBorrowSignature.AddListener(OnBorrowSignature_UI);
            borrowManager.OnBorrowFailed.AddListener(OnBorrowFailed_UI);
        }
    }

    private void OnDisable()
    {
        if (borrowManager != null)
        {
            borrowManager.OnBorrowStarted.RemoveListener(OnBorrowStarted_UI);
            borrowManager.OnBorrowSignature.RemoveListener(OnBorrowSignature_UI);
            borrowManager.OnBorrowFailed.RemoveListener(OnBorrowFailed_UI);
        }
    }

    public void FetchCollections()
    {
        Debug.Log("CitrusAPI: FetchCollections called");
        StartCoroutine(FetchCollectionsCoroutine());
    }

    private IEnumerator FetchCollectionsCoroutine()
    {
        string url = $"{API_BASE_URL}/citrus-collections";
        Debug.Log($"CitrusAPI: Fetching from URL: {url}");
        
        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                string jsonResponse = request.downloadHandler.text;
                Debug.Log($"CitrusAPI: Raw JSON response: {jsonResponse}");

                try
                {
                    // Create a wrapper object for the JSON array
                    string wrappedJson = "{\"collections\":" + jsonResponse + "}";
                    CollectionsResponse response = JsonUtility.FromJson<CollectionsResponse>(wrappedJson);
                    
                    if (response == null || response.collections == null)
                    {
                        Debug.LogError("CitrusAPI: Failed to parse collections response");
                        yield break;
                    }

                    collections = response.collections;
                    Debug.Log($"CitrusAPI: Successfully parsed {collections.Count} collections");
                    
                    // Clear existing collection elements
                    for (int i = collectionsContainer.childCount - 1; i >= 0; i--)
                    {
                        Destroy(collectionsContainer.GetChild(i).gameObject);
                    }
                    
                    // Create collection elements dynamically
                    for (int i = 0; i < collections.Count; i++)
                    {
                        var collection = collections[i];
                        var collectionObject = Instantiate(collectionTemplate, collectionsContainer);
                        
                        if (collectionObject == null) continue;
                        
                        collection.imageUrl = $"https://api.tensor.so/sol/collections/{collection.tProjectId}/image";
                        Debug.Log($"CitrusAPI: Processing collection {i + 1}: {collection.name}");
                        
                        // Update UI elements
                        UpdateCollectionUI(collectionObject, collection, i);
                        
                        // Load the image - Commented out for now
                        // StartCoroutine(LoadCollectionImage(collectionObject, collection));
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError($"CitrusAPI: Error parsing collections: {e.Message}\nStack trace: {e.StackTrace}");
                    Debug.LogError($"CitrusAPI: JSON that caused the error: {jsonResponse}");
                }
            }
            else
            {
                Debug.LogError($"CitrusAPI: Error fetching collections: {request.error}\nResponse: {request.downloadHandler.text}");
            }
        }
    }

    private void UpdateCollectionUI(GameObject collectionObject, Collection collection, int index)
    {
        var ui = collectionObject.GetComponent<CollectionUI>();
        
        if (ui == null) return;
        
        // Use averageTerms if available for display
        int displayApy = collection.bestApy > 0 ? (int)collection.bestApy : (collection.averageTerms != null ? collection.averageTerms.apy : 0);
        int displayDuration = collection.duration > 0 ? collection.duration : (collection.averageTerms != null ? collection.averageTerms.duration : 0);

        // If averageTerms are in raw format (apy=18000, duration=604800), convert for display
        if (displayApy > 1000) displayApy = displayApy / 100; // 18000 -> 180
        if (displayDuration > 1000) displayDuration = displayDuration / 86400; // 604800 -> 7

        // Update existing UI elements
        if (ui.nameText != null) ui.nameText.text = collection.name;
        if (ui.floorPriceText != null) ui.floorPriceText.text = $"Floor: {collection.floor} SOL";
        if (ui.apyText != null) ui.apyText.text = $"APY: {displayApy}%";
        if (ui.durationText != null) ui.durationText.text = $"Duration: {displayDuration} days";

        // Set up the offer button
        if (ui.offerButton != null)
        {
            ui.offerButton.onClick.RemoveAllListeners();
            ui.offerButton.onClick.AddListener(() => OnOfferButtonClicked(index, collection));
        }

        // Set up the input field
        if (ui.offerAmountInput != null)
        {
            ui.offerAmountInput.placeholder.GetComponent<TextMeshProUGUI>().text = "Enter amount in SOL";
            ui.offerAmountInput.contentType = TMP_InputField.ContentType.DecimalNumber;
        }

        // Set up the view offers button
        if (ui.viewOffersButton != null)
        {
            ui.viewOffersButton.onClick.RemoveAllListeners();
            ui.viewOffersButton.onClick.AddListener(() => OnViewOffersButtonClicked(index, collection));
        }
    }

    private void OnOfferButtonClicked(int index, Collection collection)
    {
        // Get the collection object from the container
        if (index >= collectionsContainer.childCount) return;
        
        var collectionObject = collectionsContainer.GetChild(index).gameObject;
        var ui = collectionObject.GetComponent<CollectionUI>();
        if (ui == null || ui.offerAmountInput == null) return;

        string amountText = ui.offerAmountInput.text;
        if (string.IsNullOrEmpty(amountText))
        {
            Debug.LogWarning("Please enter an amount");
            return;
        }

        if (float.TryParse(amountText, out float amount))
        {
            StartCoroutine(CreateLoanOffer(collection.id, amount));
        }
        else
        {
            Debug.LogError("Invalid amount entered");
        }
    }

    private IEnumerator CreateLoanOffer(string collectionId, float amount)
    {
        string url = $"{API_BASE_URL}/citrus-offer-loan";
        
        // Use averageTerms if available for offer
        int apy = 200;
        int duration = 3;
        var collection = collections.Find(c => c.id == collectionId);
        if (collection != null && collection.averageTerms != null)
        {
            apy = collection.averageTerms.apy;
            duration = collection.averageTerms.duration;
            if (apy > 1000) apy = apy / 100; // 18000 -> 180
            if (duration > 1000) duration = duration / 86400; // 604800 -> 7
        }
        string jsonBody = $"{{\"collectionId\":\"{collectionId}\",\"principal\":{amount},\"apy\":{apy},\"duration\":{duration}}}";
        
        Debug.Log($"CitrusAPI: Creating loan offer for collection {collectionId}");
        Debug.Log($"CitrusAPI: Input amount: {amount} SOL");
        Debug.Log($"CitrusAPI: Request body: {jsonBody}");

        using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonBody);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                string response = request.downloadHandler.text;
                Debug.Log($"CitrusAPI: Loan offer created successfully: {response}");
                FetchAndDisplayUserOffers();
            }
            else
            {
                string errorResponse = request.downloadHandler.text;
                Debug.LogError($"CitrusAPI: Error creating loan offer: {request.error}\nResponse: {errorResponse}");
            }
        }
    }

    private IEnumerator LoadCollectionImage(GameObject collectionObject, Collection collection)
    {
        if (string.IsNullOrEmpty(collection.tProjectId))
        {
            Debug.LogWarning($"CitrusAPI: No tProjectId for collection {collection.name}");
            yield break;
        }

        // Updated Tensor API endpoint for collection images
        string imageUrl = $"https://api.tensor.so/sol/collections/{collection.tProjectId}/image";
        Debug.Log($"CitrusAPI: Loading image from URL: {imageUrl}");

        using (UnityWebRequest request = UnityWebRequestTexture.GetTexture(imageUrl))
        {
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                Texture2D texture = DownloadHandlerTexture.GetContent(request);
                if (texture != null)
                {
                    if (collectionObject != null)
                    {
                        var collectionUI = collectionObject.GetComponent<CollectionUI>();
                        if (collectionUI != null && collectionUI.collectionImage != null)
                        {
                            collectionUI.collectionImage.texture = texture;
                            Debug.Log($"CitrusAPI: Successfully loaded image for {collection.name}");
                        }
                    }
                }
            }
            else
            {
                Debug.LogError($"CitrusAPI: Error loading image for {collection.name}: {request.error}\nURL: {imageUrl}");
            }
        }
    }

    public void FetchAndDisplayUserOffers()
    {
        Debug.Log("CitrusAPI: FetchAndDisplayUserOffers called");
        StartCoroutine(FetchUserOffersCoroutine());
        StartCoroutine(FetchBorrowedLoansCoroutine());
    }

    private IEnumerator FetchUserOffersCoroutine()
    {
        string url = $"{API_BASE_URL}/citrus-user-loans";
        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                string json = request.downloadHandler.text;
                Debug.Log($"CitrusAPI: Raw JSON response for user loans: {json}");

                string wrappedJson = "{\"offers\":" + json + "}";
                LoanOfferListWrapper offers = JsonUtility.FromJson<LoanOfferListWrapper>(wrappedJson);

                if (offers != null && offers.offers != null && offers.offers.Length > 0)
                {
                    // PRIORITIZE: any offer with loanAccount or lender starting with "iwuA" goes first
                    offers.offers = PrioritizeByAddressPrefix(offers.offers, "iwuA");

                    Debug.Log($"CitrusAPI: Parsed {offers.offers.Length} offers successfully.");
                    // Clear all existing offer elements
                    for (int i = offersContainer.childCount - 1; i >= 0; i--)
                    {
                        Destroy(offersContainer.GetChild(i).gameObject);
                    }

                    // Create new elements for each offer (unchanged)
                    for (int i = 0; i < offers.offers.Length; i++)
                    {
                        var offer = offers.offers[i];
                        var offerObject = Instantiate(offerTemplate, offersContainer);
                        if (offerObject == null) continue;
                        if (offer.terms == null)
                        {
                            Debug.LogWarning($"CitrusAPI: Offer {offer.loanAccount} has null terms.");
                            continue;
                        }

                        Debug.Log($"CitrusAPI: Processing offer {offer.loanAccount}. Raw terms - Principal: {offer.terms.principal}, APY: {offer.terms.apy}, Duration: {offer.terms.duration}");

                        float principalSol;
                        string principalDisplay;

                        if (offer.terms.principal == 0 && offer.ltvTerms != null)
                        {
                            principalSol = offer.ltvTerms.maxOffer / 1_000_000_000f;
                            int ltvPercent = offer.ltvTerms.ltvBps / 100;
                            principalDisplay = $"Up to {principalSol:F2} SOL ({ltvPercent}% LTV)";
                        }
                        else
                        {
                            principalSol = offer.terms.principal / 1_000_000_000f;
                            principalDisplay = $"{principalSol:F2} SOL";
                        }

                        int apyPercent = offer.terms.apy / 100;
                        int durationDays = offer.terms.duration / 86400;

                        string collectionName = "Unknown Collection";
                        var collection = collections.Find(c => c.id == offer.collectionConfig);
                        if (collection != null)
                        {
                            collectionName = collection.name;
                        }

                        System.DateTime creationDate = System.DateTimeOffset.FromUnixTimeSeconds(offer.creationTime).DateTime;
                        string formattedDate = creationDate.ToString("MMM dd, yyyy");

                        var textComponent = offerObject.GetComponentInChildren<TextMeshProUGUI>();
                        if (textComponent != null)
                        {
                            textComponent.text = $"<color=#FFA500><b>{collectionName}</b></color>\n";
                            textComponent.text += $"<color=#FFA500>Principal:</color> {principalDisplay}, ";
                            textComponent.text += $"<color=#FFA500>APY:</color> {apyPercent}%, ";
                            textComponent.text += $"<color=#FFA500>Duration:</color> {durationDays} days\n";
                            textComponent.text += $"<color=#FFA500>Created:</color> {formattedDate}, ";
                            textComponent.text += $"<color=#FFA500>Status:</color> {offer.status}";
                        }

                        var cancelButton = offerObject.GetComponentInChildren<Button>();
                        if (cancelButton != null)
                        {
                            cancelButton.onClick.RemoveAllListeners();
                            cancelButton.onClick.AddListener(() => OnCancelOfferClicked(offer.loanAccount));
                        }
                    }
                }
                else
                {
                    Debug.LogWarning("CitrusAPI: No offers found in parsed JSON or failed to parse.");
                    for (int i = offersContainer.childCount - 1; i >= 0; i--)
                    {
                        Destroy(offersContainer.GetChild(i).gameObject);
                    }
                }
            }
            else
            {
                Debug.LogError($"CitrusAPI: Failed to fetch offers: {request.error}");
                for (int i = offersContainer.childCount - 1; i >= 0; i--)
                {
                    Destroy(offersContainer.GetChild(i).gameObject);
                }
            }
        }
    }

    private IEnumerator FetchBorrowedLoansCoroutine()
    {
        // Add a small delay to allow blockchain state to settle
        yield return new WaitForSeconds(0.5f);
        
        string url = $"{API_BASE_URL}/citrus-user-borrowed-loans";
        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                string json = request.downloadHandler.text;
                Debug.Log($"CitrusAPI: Raw JSON response for user borrowed loans: {json}");

                string wrappedJson = "{\"offers\":" + json + "}";
                LoanOfferListWrapper offers = JsonUtility.FromJson<LoanOfferListWrapper>(wrappedJson);

                if (offers != null && offers.offers != null && offers.offers.Length > 0)
                {
                    Debug.Log($"CitrusAPI: Parsed {offers.offers.Length} borrowed loans successfully.");
                    DisplayBorrowedLoans(offers.offers);
                }
                else
                {
                    Debug.Log("CitrusAPI: No borrowed loans found or failed to parse.");
                }
            }
            else
            {
                Debug.LogError($"CitrusAPI: Failed to fetch borrowed loans: {request.error}");
            }
        }
    }

    private void OnCancelOfferClicked(string loanAccount)
    {
        Debug.Log($"CitrusAPI: Cancel offer clicked for loan account: {loanAccount}");
        StartCoroutine(CancelLoanOffer(loanAccount));
    }

    private void OnRepayLoanClicked(string loanAccount)
    {
        Debug.Log($"CitrusAPI: Repay loan clicked for loan account: {loanAccount}");
        StartCoroutine(RepayLoan(loanAccount));
    }

    private IEnumerator CancelLoanOffer(string loanAccount)
    {
        string url = $"{API_BASE_URL}/citrus-cancel-loan";
        string jsonBody = $"{{\"loanAccount\":\"{loanAccount}\"}}";
        
        Debug.Log($"CitrusAPI: Canceling loan offer for loan account: {loanAccount}");
        Debug.Log($"CitrusAPI: Request body: {jsonBody}");

        using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonBody);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                string response = request.downloadHandler.text;
                Debug.Log($"CitrusAPI: Loan offer canceled successfully: {response}");
                FetchAndDisplayUserOffers();
            }
            else
            {
                string errorResponse = request.downloadHandler.text;
                Debug.LogError($"CitrusAPI: Error canceling loan offer: {request.error}\nResponse: {errorResponse}");
            }
        }
    }

    private IEnumerator RepayLoan(string loanAccount)
    {
        string url = $"{API_BASE_URL}/citrus-repay-loan";
        string jsonBody = $"{{\"loanAccount\":\"{loanAccount}\"}}";

        Debug.Log($"CitrusAPI: Repaying loan for loan account: {loanAccount}");
        Debug.Log($"CitrusAPI: Request body: {jsonBody}");

        using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonBody);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                string response = request.downloadHandler.text;
                Debug.Log($"CitrusAPI: Loan repaid successfully: {response}");
                // Refresh lists
                FetchAndDisplayUserOffers();
            }
            else
            {
                string errorResponse = request.downloadHandler.text;
                Debug.LogError($"CitrusAPI: Error repaying loan: {request.error}\nResponse: {errorResponse}");
            }
        }
    }

    private void OnViewOffersButtonClicked(int index, Collection collection)
    {
        Debug.Log($"CitrusAPI: View offers clicked for collection: {collection.name} (ID: {collection.id})");
        StartCoroutine(FetchCollectionOffers(collection.id, collection.name));
    }

    private IEnumerator FetchCollectionOffers(string collectionId, string collectionName)
    {
        string url = $"{API_BASE_URL}/citrus-collection-offers?collectionId={collectionId}";
        Debug.Log($"CitrusAPI: Fetching offers for collection {collectionName} from URL: {url}");
        
        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                string json = request.downloadHandler.text;
                Debug.Log($"CitrusAPI: Raw JSON response for collection offers: {json}");
                
                string wrappedJson = "{\"offers\":" + json + "}";
                LoanOfferListWrapper offers = JsonUtility.FromJson<LoanOfferListWrapper>(wrappedJson);

                if (offers != null && offers.offers != null && offers.offers.Length > 0)
                {
                    Debug.Log($"CitrusAPI: Found {offers.offers.Length} offers for {collectionName}");
                    DisplayAvailableOffers(offers.offers, collectionName);
                }
                else
                {
                    Debug.LogWarning($"CitrusAPI: No offers found for collection {collectionName}");
                    DisplayNoOffersMessage(collectionName);
                }
            }
            else
            {
                Debug.LogError($"CitrusAPI: Failed to fetch collection offers: {request.error}");
            }
        }
    }

    private void DisplayAvailableOffers(LoanOffer[] offers, string collectionName)
    {
        if (availableOffersPanel != null) availableOffersPanel.SetActive(true);
        currentCollectionName = collectionName;

        for (int i = availableOffersContainer.childCount - 1; i >= 0; i--)
            Destroy(availableOffersContainer.GetChild(i).gameObject);

        var prioritized = PrioritizeByAddressPrefix(offers, "iwuA");

        for (int i = 0; i < prioritized.Length; i++)
        {
            var offer = prioritized[i];

            // Add this log to see the exact IDs you need:
            Debug.Log($"[Citrus] Offer loanAccount={offer.loanAccount} lender={offer.lender} status={offer.status}");

            var offerObject = Instantiate(availableOfferTemplate, availableOffersContainer);
            if (offerObject == null) continue;

            if (offer.terms == null)
            {
                Debug.LogWarning($"CitrusAPI: Offer {offer.loanAccount} has null terms.");
                continue;
            }

            float principalSol;
            string principalDisplay;

            if (offer.terms.principal == 0 && offer.ltvTerms != null)
            {
                principalSol = offer.ltvTerms.maxOffer / 1_000_000_000f;
                int ltvPercent = offer.ltvTerms.ltvBps / 100;
                principalDisplay = $"Up to {principalSol:F2} SOL ({ltvPercent}% LTV)";
            }
            else
            {
                principalSol = offer.terms.principal / 1_000_000_000f;
                principalDisplay = $"{principalSol:F2} SOL";
            }

            int apyPercent = offer.terms.apy / 100;
            int durationDays = offer.terms.duration / 86400;

            var textComponent = offerObject.GetComponentInChildren<TextMeshProUGUI>();
            if (textComponent != null)
            {
                textComponent.text = $"<color=#FFA500><b>Available Offer for {collectionName}</b></color>\n";
                textComponent.text += $"<color=#FFA500>Principal:</color> {principalDisplay}, ";
                textComponent.text += $"<color=#FFA500>APY:</color> {apyPercent}%, ";
                textComponent.text += $"<color=#FFA500>Duration:</color> {durationDays} days\n";
                // lender may be null/short; guard Substring
                string lenderShort = !string.IsNullOrEmpty(offer.lender) && offer.lender.Length >= 8 ? offer.lender.Substring(0, 8) : (offer.lender ?? "unknown");
                textComponent.text += $"<color=#FFA500>Lender:</color> {lenderShort}...";
            }

            var selectButton = offerObject.GetComponentInChildren<Button>();
            if (selectButton != null)
            {
                selectButton.onClick.RemoveAllListeners();
                selectButton.onClick.AddListener(() => OnSelectOfferClicked(offer, collectionName));
                var buttonText = selectButton.GetComponentInChildren<TextMeshProUGUI>();
                if (buttonText != null) buttonText.text = "Borrow";
            }
        }
    }

    private void DisplayBorrowedLoans(LoanOffer[] offers)
    {
        // Append borrowed loans to the same container with a Repay action
        for (int i = 0; i < offers.Length; i++)
        {
            var offer = offers[i];
            var offerObject = Instantiate(offerTemplate, offersContainer);

            if (offerObject == null) continue;

            if (offer.terms == null)
            {
                Debug.LogWarning($"CitrusAPI: Borrowed loan {offer.loanAccount} has null terms.");
                continue;
            }

            float principalSol = offer.terms.principal / 1_000_000_000f;
            int apyPercent = offer.terms.apy / 100;
            int durationDays = offer.terms.duration / 86400;

            string collectionName = "Unknown Collection";
            var collection = collections.Find(c => c.id == offer.collectionConfig);
            if (collection != null)
            {
                collectionName = collection.name;
            }

            var textComponent = offerObject.GetComponentInChildren<TextMeshProUGUI>();
            if (textComponent != null)
            {
                textComponent.text = $"<color=#FFA500><b>Borrowed Loan - {collectionName}</b></color>\n";
                textComponent.text += $"<color=#FFA500>Principal:</color> {principalSol:F2} SOL, ";
                textComponent.text += $"<color=#FFA500>APY:</color> {apyPercent}%, ";
                textComponent.text += $"<color=#FFA500>Duration:</color> {durationDays} days\n";
                textComponent.text += $"<color=#FFA500>Status:</color> {offer.status}";
            }

            var actionButton = offerObject.GetComponentInChildren<Button>();
            if (actionButton != null)
            {
                // Only show repay button for active loans (not repaid)
                if (offer.status != "repaid")
                {
                    actionButton.onClick.RemoveAllListeners();
                    actionButton.onClick.AddListener(() => OnRepayLoanClicked(offer.loanAccount));
                    var buttonText = actionButton.GetComponentInChildren<TextMeshProUGUI>();
                    if (buttonText != null) buttonText.text = "Repay";
                }
                else
                {
                    // Hide the button for repaid loans
                    actionButton.gameObject.SetActive(false);
                }
            }
        }
    }

    private void DisplayNoOffersMessage(string collectionName)
    {
        // Clear existing offer elements
        for (int i = availableOffersContainer.childCount - 1; i >= 0; i--)
        {
            Destroy(availableOffersContainer.GetChild(i).gameObject);
        }
        
        // Create a message element
        var messageObject = Instantiate(availableOfferTemplate, availableOffersContainer);
        var textComponent = messageObject.GetComponentInChildren<TextMeshProUGUI>();
        if (textComponent != null)
        {
            textComponent.text = $"<color=#FFA500><b>No offers available for {collectionName}</b></color>\n";
            textComponent.text += "Check back later or try another collection.";
        }
        
        // Hide the button
        var button = messageObject.GetComponentInChildren<Button>();
        if (button != null)
        {
            button.gameObject.SetActive(false);
        }
    }

    private void OnSelectOfferClicked(LoanOffer offer, string collectionName)
    {
        Debug.Log($"CitrusAPI: Select offer clicked for loan account: {offer.loanAccount}, collection: {collectionName}");
        
        // Store the selected offer and collection info
        selectedOffer = offer;
        currentCollectionName = collectionName;
        
        // Find the collection ID for the current collection
        Collection currentCollection = collections.Find(c => c.name == collectionName);
        if (currentCollection != null)
        {
            currentCollectionId = currentCollection.id;
            
            // Show NFT selection modal
            if (nftSelectionModal != null)
            {
                nftSelectionModal.Show(currentCollectionId, currentCollectionName, OnNFTSelected);
            }
            else
            {
                Debug.LogError("CitrusAPI: NFT Selection Modal not assigned!");
            }
        }
        else
        {
            Debug.LogError($"CitrusAPI: Could not find collection with name: {collectionName}");
        }
    }
    
    private void OnNFTSelected(string selectedNFTMint)
    {
        Debug.Log($"CitrusAPI: NFT selected: {selectedNFTMint}");

        if (selectedOffer != null)
        {
            Debug.Log($"CitrusAPI: Borrowing loan with NFT: {selectedNFTMint}");
            Debug.Log($"CitrusAPI: Offer details - Principal: {selectedOffer.terms.principal / 1_000_000_000f:F2} SOL, APY: {selectedOffer.terms.apy / 100}%");

            if (borrowManager == null)
            {
                borrowManager = FindObjectOfType<CitrusClientBorrowManager>(includeInactive: true);
            }

            if (borrowManager != null)
            {
                borrowManager.BeginBorrow(selectedOffer.loanAccount, selectedNFTMint);
            }
            else
            {
                Debug.LogError("CitrusAPI: CitrusClientBorrowManager not found in scene.");
            }
        }
        else
        {
            Debug.LogError("CitrusAPI: No offer selected for borrowing");
        }
    }

    // -------- Client-signed borrow flow (build -> sign -> send -> confirm) --------

    [Serializable]
    private class BuildBorrowRequest
    {
        public string loanAccount;
        public string nftMint;
        public string wallet;
    }

    [Serializable]
    private class BuildTxResponse
    {
        public string transaction;
        public string blockhash;
        public string lastValidBlockHeight;
        public string error;
        public string rpcUrl;
        public string minContextSlot;
    }

    [Serializable]
    private class RpcError { public int code; public string message; public string data; }
    [Serializable]
    private class RpcRespString
    {
        public string jsonrpc;
        public string id;
        public string result;
        public RpcError error;
    }

    private static int SafeLen(string s) => string.IsNullOrEmpty(s) ? 0 : s.Length;
    private static string Truncate(string s, int max) => string.IsNullOrEmpty(s) ? "<null>" : (s.Length <= max ? s : s.Substring(0, max));

    private IEnumerator SendTransactionJsonRpc(IRpcClient rpc, string signedB64, string rpcUrl, string minContextSlot,
        Commitment preflightCommitment, bool skipPreflight,
        Action<bool, string, string> callback)
    {
        string endpoint = !string.IsNullOrEmpty(rpcUrl)
            ? rpcUrl
            : (rpc?.NodeAddress != null ? rpc.NodeAddress.ToString() : null);

        if (string.IsNullOrEmpty(endpoint))
        {
            callback?.Invoke(false, null, "No RPC endpoint available.");
            yield break;
        }

        ulong mcs = 0UL;
        bool haveMcs = !string.IsNullOrEmpty(minContextSlot) && ulong.TryParse(minContextSlot, out mcs);

        string cfg = haveMcs
            ? $"{{\"skipPreflight\":{skipPreflight.ToString().ToLowerInvariant()},\"preflightCommitment\":\"{preflightCommitment.ToString().ToLowerInvariant()}\",\"encoding\":\"base64\",\"minContextSlot\":{mcs}}}"
            : $"{{\"skipPreflight\":{skipPreflight.ToString().ToLowerInvariant()},\"preflightCommitment\":\"{preflightCommitment.ToString().ToLowerInvariant()}\",\"encoding\":\"base64\"}}";

        string payload = $"{{\"jsonrpc\":\"2.0\",\"id\":1,\"method\":\"sendTransaction\",\"params\":[\"{signedB64}\",{cfg}]}}";

        using (var uwr = new UnityWebRequest(endpoint, "POST"))
        {
            byte[] body = System.Text.Encoding.UTF8.GetBytes(payload);
            uwr.uploadHandler = new UploadHandlerRaw(body);
            uwr.downloadHandler = new DownloadHandlerBuffer();
            uwr.SetRequestHeader("Content-Type", "application/json");
            yield return uwr.SendWebRequest();

            string raw = uwr.downloadHandler != null ? uwr.downloadHandler.text : null;

            if (uwr.result != UnityWebRequest.Result.Success)
            {
                callback?.Invoke(false, null, $"HTTP {uwr.responseCode} {uwr.error}. Raw: {Truncate(raw, 800)}");
                yield break;
            }

            RpcRespString resp = null;
            try { resp = JsonUtility.FromJson<RpcRespString>(raw); } catch { }

            if (resp != null && !string.IsNullOrEmpty(resp.result))
            {
                callback?.Invoke(true, resp.result, raw);
            }
            else
            {
                string msg = resp?.error != null ? $"{resp.error.code}: {resp.error.message}" : "Unknown RPC error";
                callback?.Invoke(false, null, $"RPC error: {msg}. Raw: {Truncate(raw, 800)}");
            }
        }
    }

    private IEnumerator WaitForConfirmation(IRpcClient rpc, string signature, float timeoutSeconds)
    {
        float elapsed = 0f;
        const float step = 0.8f;
        while (elapsed < timeoutSeconds)
        {
            var statusTask = rpc.GetSignatureStatusesAsync(new List<string> { signature }, false);
            while (!statusTask.IsCompleted) yield return null;

            var rr = statusTask.Result;
            if (rr != null && rr.WasSuccessful && rr.Result != null && rr.Result.Value != null && rr.Result.Value.Count > 0)
            {
                var info = rr.Result.Value[0];
                if (info != null && (info.ConfirmationStatus == "confirmed" || info.ConfirmationStatus == "finalized"))
                {
                    Debug.Log($"CitrusAPI: Borrow confirmed signature={signature} status={info.ConfirmationStatus}");
                    yield break;
                }
            }

            yield return new WaitForSeconds(step);
            elapsed += step;
        }

        Debug.LogWarning("CitrusAPI: Borrow confirmation timeout for " + signature);
    }

    private IEnumerator BorrowLoan(string loanAccount, string nftMint)
    {
        string borrower = WalletManager.instance != null ? WalletManager.instance.walletAddress : null;
        if (string.IsNullOrEmpty(borrower))
        {
            Debug.LogError("CitrusAPI: BorrowLoan aborted: no wallet connected.");
            yield break;
        }

        string url = $"{API_BASE_URL}/citrus-borrow-loan";
        var payload = new BuildBorrowRequest
        {
            loanAccount = loanAccount,
            nftMint = nftMint,
            wallet = borrower
        };
        string jsonBody = JsonUtility.ToJson(payload);

        Debug.Log($"CitrusAPI: Borrowing loan (build) for loanAccount={loanAccount}, nftMint={nftMint}, wallet={borrower}");
        Debug.Log($"CitrusAPI: Request body: {jsonBody}");

        using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonBody);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            var respText = request.downloadHandler != null ? request.downloadHandler.text : null;
            Debug.Log($"CitrusAPI: Borrow build HTTP {request.responseCode} respLen={SafeLen(respText)}");

            if (request.result != UnityWebRequest.Result.Success)
            {
                string errorMsg = string.IsNullOrEmpty(respText) ? request.error : respText;
                Debug.LogError("CitrusAPI: Borrow build failed: " + errorMsg);
                yield break;
            }

            var buildResp = JsonUtility.FromJson<BuildTxResponse>(respText);
            if (buildResp == null || !string.IsNullOrEmpty(buildResp.error) || string.IsNullOrEmpty(buildResp.transaction))
            {
                string e = buildResp != null && !string.IsNullOrEmpty(buildResp.error) ? buildResp.error : "Invalid borrow build response";
                string rawSnippet = string.IsNullOrEmpty(respText) ? "<null>" : Truncate(respText, 512);
                Debug.LogError($"CitrusAPI: Borrow build parse error: {e}. Raw (truncated): {rawSnippet}");
                yield break;
            }

            Debug.Log($"CitrusAPI: Borrow build OK: blockhash={buildResp.blockhash} lastValid={buildResp.lastValidBlockHeight} txB64Len={SafeLen(buildResp.transaction)}");

            // 1) Decode + quick legacy ix-count check
            byte[] txBytes = null;
            bool isVersioned = false;
            VersionedTransaction vtx = null;
            Transaction ltx = null;
            string decodeErr = null;

            try
            {
                txBytes = Convert.FromBase64String(buildResp.transaction);
                if (txBytes == null || txBytes.Length == 0)
                    throw new Exception("Empty transaction payload");

                // Quick check: if legacy and ixs==0, fail early with a clear message
                if (TryReadLegacyIxCount(txBytes, out var ixCount) && ixCount == 0)
                {
                    Debug.LogError("CitrusAPI: Server returned an empty transaction (no instructions). Borrow cannot proceed.");
                    InspectLegacyTx(txBytes); // keep the helpful log
                    yield break;
                }

                byte first = txBytes[0];
                isVersioned = (first & 0x80) != 0; // OK for sigCount<128; server returns legacy here
                if (isVersioned)
                {
                    int version = first & 0x7F;
                    Debug.Log($"CitrusAPI: Borrow tx detected as Versioned v{version}, bytes={txBytes.Length}");
                    vtx = VersionedTransaction.Deserialize(txBytes);
                }
                else
                {
                    Debug.Log($"CitrusAPI: Borrow tx detected as Legacy, bytes={txBytes.Length}");
                    ltx = Transaction.Deserialize(txBytes);
                }
            }
            catch (Exception ex)
            {
                decodeErr = ex.Message;
            }

            if (decodeErr != null)
            {
                Debug.LogError("CitrusAPI: Borrow invalid transaction data: " + decodeErr);
                try { InspectLegacyTx(txBytes); } catch { }
                yield break;
            }

            // 2) Sign (yields allowed here)
            string signedB64 = null;

            if (isVersioned)
            {
                var signTaskV = Web3.Wallet.SignTransaction(vtx);
                while (!signTaskV.IsCompleted) yield return null;

                if (signTaskV.IsFaulted || signTaskV.Result == null)
                {
                    Debug.LogError("CitrusAPI: Borrow sign task (versioned) faulted: " + (signTaskV.Exception != null ? signTaskV.Exception.Message : "unknown"));
                    yield break;
                }

                // Serialize result (no yield in try/catch)
                bool serOk = true;
                string serErr = null;
                try { signedB64 = Convert.ToBase64String(signTaskV.Result.Serialize()); }
                catch (Exception ex) { serOk = false; serErr = ex.Message; }

                if (!serOk)
                {
                    Debug.LogError("CitrusAPI: Borrow serialize error (versioned): " + serErr);
                    yield break;
                }
            }
            else
            {
                var signTaskL = Web3.Wallet.SignTransaction(ltx);
                while (!signTaskL.IsCompleted) yield return null;

                if (signTaskL.IsFaulted || signTaskL.Result == null)
                {
                    Debug.LogError("CitrusAPI: Borrow sign task (legacy) faulted: " + (signTaskL.Exception != null ? signTaskL.Exception.Message : "unknown"));
                    yield break;
                }

                bool serOk = true;
                string serErr = null;
                try { signedB64 = Convert.ToBase64String(signTaskL.Result.Serialize()); }
                catch (Exception ex) { serOk = false; serErr = ex.Message; }

                if (!serOk)
                {
                    Debug.LogError("CitrusAPI: Borrow serialize error (legacy): " + serErr);
                    yield break;
                }
            }

            // 3) Send via JSON-RPC with preflight=confirmed and minContextSlot
            var chosenRpc = !string.IsNullOrEmpty(buildResp.rpcUrl)
                ? ClientFactory.GetClient(buildResp.rpcUrl)
                : Web3.Rpc;

            bool sendOk = false;
            string signature = null;
            string rawRpcResponse = null;

            yield return StartCoroutine(SendTransactionJsonRpc(
                chosenRpc,
                signedB64,
                buildResp.rpcUrl,
                buildResp.minContextSlot,
                Commitment.Confirmed,
                false,
                (ok, sig, raw) => { sendOk = ok; signature = sig; rawRpcResponse = raw; }
            ));

            if (!sendOk || string.IsNullOrEmpty(signature))
            {
                string endpoint = chosenRpc?.NodeAddress != null ? chosenRpc.NodeAddress.ToString() : "<null>";
                Debug.LogError($"CitrusAPI: Borrow send failed. Endpoint={endpoint}\nRaw RPC: {Truncate(rawRpcResponse, 1200)}");
                yield break;
            }

            Debug.Log($"CitrusAPI: Borrow sent, signature={signature}");

            // 4) Confirm
            yield return StartCoroutine(WaitForConfirmation(chosenRpc, signature, 45f));
            Debug.Log("CitrusAPI: Borrow confirmation finished (either confirmed or timed out)");

            // Clear selection and close panel
            selectedOffer = null;
            CloseAvailableOffersPanel();

            // Refresh user loans to show the new borrowed loan
            FetchAndDisplayUserOffers();
        }
    }

    private void CloseAvailableOffersPanel()
    {
        if (availableOffersPanel != null) availableOffersPanel.SetActive(false);
        selectedOffer = null;
        currentCollectionName = "";
        currentCollectionId = "";
        
        // Hide borrow button
        if (borrowButton != null) borrowButton.gameObject.SetActive(false);
    }

    // New: centralize user NFT fetch, injecting the connected wallet. Returns raw JSON.
    public void FetchUserNftsByCollection(string collectionId, Action<string> callback)
    {
        StartCoroutine(FetchUserNftsByCollectionCoroutine(collectionId, callback));
    }

    private IEnumerator FetchUserNftsByCollectionCoroutine(string collectionId, Action<string> callback)
    {
        var wallet = WalletManager.instance != null ? WalletManager.instance.walletAddress : null;
        if (string.IsNullOrEmpty(wallet))
        {
            Debug.LogWarning("CitrusAPI: FetchUserNftsByCollection: no wallet connected.");
            callback?.Invoke(null);
            yield break;
        }

        string url = $"{API_BASE_URL}/citrus-user-nfts-by-collection?collectionId={UnityWebRequest.EscapeURL(collectionId)}&wallet={UnityWebRequest.EscapeURL(wallet)}";
        Debug.Log($"CitrusAPI: FetchUserNftsByCollection URL: {url}");

        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                string jsonResponse = request.downloadHandler.text;
                callback?.Invoke(jsonResponse);
            }
            else
            {
                Debug.LogError($"CitrusAPI: Error fetching user NFTs: {request.error} | {request.downloadHandler.text}");
                callback?.Invoke(null);
            }
        }
    }

    private static bool OfferAddressStartsWith(LoanOffer o, string prefix)
    {
        if (o == null || string.IsNullOrEmpty(prefix)) return false;
        if (!string.IsNullOrEmpty(o.loanAccount) && o.loanAccount.StartsWith(prefix)) return true;
        if (!string.IsNullOrEmpty(o.lender) && o.lender.StartsWith(prefix)) return true;
        return false;
    }

    private static LoanOffer[] PrioritizeByAddressPrefix(LoanOffer[] offers, string prefix)
    {
        if (offers == null || offers.Length == 0 || string.IsNullOrEmpty(prefix)) return offers;
        return offers
            .OrderByDescending(o => OfferAddressStartsWith(o, prefix))
            .ToArray();
    }

    // Put these helpers inside CitrusAPI
    private static ulong DecodeShortVec(byte[] data, ref int offset)
    {
        ulong result = 0; int shift = 0;
        while (true)
        {
            if (offset >= data.Length) throw new Exception("shortvec decode past end");
            byte b = data[offset++]; result |= (ulong)(b & 0x7FUL) << shift;
            if ((b & 0x80) == 0) break;
            shift += 7; if (shift > 63) throw new Exception("shortvec overflow");
        }
        return result;
    }

    private static void InspectLegacyTx(byte[] txBytes)
    {
        try
        {
            int off = 0;
            ulong sigCount = DecodeShortVec(txBytes, ref off);
            off += checked((int)(sigCount * 64UL));
            byte numRequiredSignatures = txBytes[off++];
            byte numReadOnlySigned = txBytes[off++];
            byte numReadOnlyUnsigned = txBytes[off++];
            ulong acctCnt = DecodeShortVec(txBytes, ref off);
            off += checked((int)acctCnt * 32);
            off += 32; // blockhash
            ulong ixCnt = DecodeShortVec(txBytes, ref off);
            Debug.Log($"Citrus Inspect: sigCount={sigCount}, hdr.numRequiredSignatures={numRequiredSignatures}, accounts={acctCnt}, ixs={ixCnt}");
        }
        catch (Exception e) { Debug.LogWarning("Citrus Inspect failed: " + e.Message); }
    }

    // Returns true and sets ixCount for legacy wire format; false on parse failure
    private static bool TryReadLegacyIxCount(byte[] txBytes, out ulong ixCount)
    {
        ixCount = 0UL;
        try
        {
            int off = 0;
            // sigs
            ulong sigCount = DecodeShortVec(txBytes, ref off);
            off += checked((int)(sigCount * 64UL));
            // header (3 bytes)
            off += 3;
            // accounts
            ulong acctCnt = DecodeShortVec(txBytes, ref off);
            off += checked((int)acctCnt * 32);
            // blockhash
            off += 32;
            // instructions count
            ixCount = DecodeShortVec(txBytes, ref off);
            return true;
        }
        catch
        {
            return false;
        }
    }

    private void OnBorrowStarted_UI()
    {
        Debug.Log("CitrusAPI: Borrow started (manager).");
        // Optional: show a spinner, disable buttons, etc.
    }

    private void OnBorrowSignature_UI(string sig)
    {
        Debug.Log($"CitrusAPI: Borrow sent (manager) signature={sig}");
        // Close panels and refresh lists on success
        selectedOffer = null;
        CloseAvailableOffersPanel();
        FetchAndDisplayUserOffers();
    }

    private void OnBorrowFailed_UI(string err)
    {
        Debug.LogError("CitrusAPI: Borrow failed (manager): " + err);
        // Optional: surface error to UI
    }
}