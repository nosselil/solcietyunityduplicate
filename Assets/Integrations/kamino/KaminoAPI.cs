using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;

[System.Serializable]
public class KaminoLoanOffer
{
    public string symbol;
    public float ltv;
    public string supplyApy;
    public string borrowApy;
}

[System.Serializable]
public class KaminoLoanOffersResponse
{
    public List<KaminoLoanOffer> offers;
}

[System.Serializable]
public class KaminoLendRequest
{
    public string symbol;
    public string amount;
    public KaminoLendRequest(string symbol, string amount)
    {
        this.symbol = symbol;
        this.amount = amount;
    }
}

[System.Serializable]
public class UserBorrowingCapacityResponse
{
    public bool success;
    public float totalCollateral;
    public float totalBorrowed;
    public float availableToBorrow;
}

[System.Serializable]
public class UserPosition
{
    public string symbol;
    public float amount;
    public float apy;
    public bool isLend;
    public string date;
    public string cTokenAccount;
    public string cTokenBalance; // string to match backend JSON
}

public class KaminoAPI : MonoBehaviour
{
    public static KaminoAPI Instance { get; private set; }
    void Awake() { Instance = this; }

    private const string API_URL = "https://solcietyserver.vercel.app/api/kamino-loan-offers";

    public KaminoLoanOfferUI offerPrefab;
    public Transform offersContainer;
    public KaminoLendModal lendModal; // Assign this in the inspector to the modal in your scene
    public KaminoBorrowModal borrowModal; // Assign this in the inspector to the modal in your scene
    public KaminoUserActivityModal activityModal;
    public LoadingSpinner loadingSpinner; // Assign this in the inspector

    // Automatically fetch Kamino loan offers when the script starts
    void Start()
    {
        FetchKaminoLoanOffers();
        FetchUserBorrowingCapacity(); // Also fetch user's borrowing capacity
    }

    // Call this method to fetch Kamino loan offers
    public void FetchKaminoLoanOffers()
    {
        // Show loading spinner
        if (loadingSpinner != null)
        {
            loadingSpinner.Show("Loading Kamino tokens...");
        }
        
        StartCoroutine(FetchKaminoLoanOffersCoroutine());
    }

    private IEnumerator FetchKaminoLoanOffersCoroutine()
    {
        using (UnityWebRequest request = UnityWebRequest.Get(API_URL))
        {
            yield return request.SendWebRequest();

            // Hide loading spinner
            if (loadingSpinner != null)
            {
                loadingSpinner.Hide();
            }

            if (request.result == UnityWebRequest.Result.Success)
            {
                var json = request.downloadHandler.text;
                KaminoLoanOffersResponse response = JsonUtility.FromJson<KaminoLoanOffersResponse>(json);
                if (response != null && response.offers != null)
                {
                    Debug.Log($"Fetched {response.offers.Count} Kamino loan offers.");
                    // Clear old offers
                    foreach (Transform child in offersContainer)
                        Destroy(child.gameObject);

                    // Instantiate new offers
                    foreach (var offer in response.offers)
                    {
                        var offerUI = Instantiate(offerPrefab, offersContainer);
                        offerUI.SetOffer(offer);
                        offerUI.lendModal = lendModal; // <-- Assign the scene modal here!
                        offerUI.borrowModal = borrowModal; // <-- Assign the scene borrow modal here!
                    }
                }
                else
                {
                    Debug.LogWarning("No Kamino loan offers found or failed to parse response.");
                }
            }
            else
            {
                Debug.LogError($"Failed to fetch Kamino loan offers: {request.error}");
            }
        }
    }

    public void Lend(KaminoLoanOffer offer, float amount)
    {
        // Show loading spinner
        if (loadingSpinner != null)
        {
            loadingSpinner.Show("Processing lend transaction...");
        }
        
        StartCoroutine(LendCoroutine(offer, amount));
    }

    private IEnumerator LendCoroutine(KaminoLoanOffer offer, float amount)
    {
        var payload = new KaminoLendRequest(offer.symbol, amount.ToString("F8"));
        string json = JsonUtility.ToJson(payload);
        Debug.Log("Lend payload: " + json);

                    using (UnityWebRequest request = new UnityWebRequest("https://solcietyserver.vercel.app/api/kamino-lend", "POST"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            // Hide loading spinner
            if (loadingSpinner != null)
            {
                loadingSpinner.Hide();
            }
            
            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("Lend successful: " + request.downloadHandler.text);
                // Show success message in the modal
                if (lendModal != null)
                {
                    lendModal.ShowSuccess("Lend successful! Your tokens have been deposited.");
                }
            }
            else
            {
                // For HTTP errors, the actual error message is in the response body
                string errorMessage = request.downloadHandler.text;
                if (string.IsNullOrEmpty(errorMessage))
                {
                    errorMessage = request.error;
                }
                Debug.LogError("Lend failed: " + errorMessage);
                
                // Show error message in the modal
                if (lendModal != null)
                {
                    lendModal.ShowError("Lend failed: " + errorMessage);
                }
            }
        }
    }

    public void Borrow(KaminoLoanOffer offer, float amount)
    {
        // Show loading spinner
        if (loadingSpinner != null)
        {
            loadingSpinner.Show("Processing borrow transaction...");
        }
        
        StartCoroutine(BorrowCoroutine(offer, amount));
    }

    public void FetchUserBorrowingCapacity()
    {
        StartCoroutine(FetchUserBorrowingCapacityCoroutine());
    }

    public void FetchUserBorrowingCapacity(System.Action<UserBorrowingCapacityResponse> callback)
    {
        StartCoroutine(FetchUserBorrowingCapacityCoroutine(callback));
    }

    private IEnumerator FetchUserBorrowingCapacityCoroutine()
    {
        return FetchUserBorrowingCapacityCoroutine(null);
    }

    private IEnumerator FetchUserBorrowingCapacityCoroutine(System.Action<UserBorrowingCapacityResponse> callback)
    {
                    using (UnityWebRequest request = UnityWebRequest.Get("https://solcietyserver.vercel.app/api/kamino-user-borrowing-capacity"))
        {
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                var json = request.downloadHandler.text;
                var response = JsonUtility.FromJson<UserBorrowingCapacityResponse>(json);
                if (response != null && response.success)
                {
                    Debug.Log($"User borrowing capacity - Collateral: {response.totalCollateral}, Borrowed: {response.totalBorrowed}, Available: {response.availableToBorrow}");
                    // Store the data for use in borrow modal
                    currentUserBorrowingCapacity = response;
                    
                    // Call the callback if provided
                    callback?.Invoke(response);
                }
                else
                {
                    Debug.LogWarning("Failed to parse user borrowing capacity response.");
                    callback?.Invoke(null);
                }
            }
            else
            {
                Debug.LogError($"Failed to fetch user borrowing capacity: {request.error}");
                callback?.Invoke(null);
            }
        }
    }

    private UserBorrowingCapacityResponse currentUserBorrowingCapacity;

    public UserBorrowingCapacityResponse GetCurrentUserBorrowingCapacity()
    {
        return currentUserBorrowingCapacity;
    }

    private IEnumerator BorrowCoroutine(KaminoLoanOffer offer, float amount)
    {
        var payload = new KaminoLendRequest(offer.symbol, amount.ToString("F8"));
        string json = JsonUtility.ToJson(payload);
        Debug.Log("Borrow payload: " + json);

                    using (UnityWebRequest request = new UnityWebRequest("https://solcietyserver.vercel.app/api/kamino-borrow", "POST"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            // Hide loading spinner
            if (loadingSpinner != null)
            {
                loadingSpinner.Hide();
            }
            
            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("Borrow successful: " + request.downloadHandler.text);
                // Show success message in the modal
                if (borrowModal != null)
                {
                    borrowModal.SetError("Borrow successful! Your tokens have been borrowed.", false, true);
                }
            }
            else
            {
                // For HTTP errors, the actual error message is in the response body
                string errorMessage = request.downloadHandler.text;
                if (string.IsNullOrEmpty(errorMessage))
                {
                    errorMessage = request.error;
                }
                Debug.LogError("Borrow failed: " + errorMessage);
                
                // Show error message in the modal
                if (borrowModal != null)
                {
                    borrowModal.ShowError("Borrow failed: " + errorMessage);
                }
            }
        }
    }

    public void FetchUserActivity(string wallet, System.Action<List<UserPosition>> callback)
    {
        StartCoroutine(FetchUserActivityCoroutine(wallet, callback));
    }

    private IEnumerator FetchUserActivityCoroutine(string wallet, System.Action<List<UserPosition>> callback)
    {
                    string url = $"https://solcietyserver.vercel.app/api/kamino-user-activity?wallet={wallet}";
        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                var json = request.downloadHandler.text;
                UserPositionsWrapper wrapper = JsonUtility.FromJson<UserPositionsWrapper>(json);
                callback?.Invoke(wrapper.positions);
            }
            else
            {
                Debug.LogError("Failed to fetch user activity: " + request.error);
                callback?.Invoke(new List<UserPosition>());
            }
        }
    }

    [System.Serializable]
    public class UserPositionsWrapper
    {
        public List<UserPosition> positions;
    }

    public void OnViewActivityClicked(string wallet)
    {
        // Show loading spinner
        if (loadingSpinner != null)
        {
            loadingSpinner.Show("Fetching your activity...");
        }
        
        KaminoAPI.Instance.FetchUserActivity(wallet, (positions) => {
            // Hide loading spinner
            if (loadingSpinner != null)
            {
                loadingSpinner.Hide();
            }
            
            activityModal.Show(positions);
        });
    }

    public void Repay(string symbol, float amount, System.Action<bool, string> callback = null)
    {
        StartCoroutine(RepayCoroutine(symbol, amount, callback));
    }

    private IEnumerator RepayCoroutine(string symbol, float amount, System.Action<bool, string> callback)
    {
                    string url = "https://solcietyserver.vercel.app/api/kamino-repay";
        var payload = new RepayRequest { symbol = symbol, amount = amount.ToString("F8") };
        string json = JsonUtility.ToJson(payload);

        using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("Repay successful: " + request.downloadHandler.text);
                callback?.Invoke(true, request.downloadHandler.text);
            }
            else
            {
                // For HTTP errors, the actual error message is in the response body
                string errorMessage = request.downloadHandler.text;
                if (string.IsNullOrEmpty(errorMessage))
                {
                    errorMessage = request.error;
                }
                Debug.LogError("Repay failed: " + errorMessage);
                callback?.Invoke(false, errorMessage);
            }
        }
    }

    [System.Serializable]
    public class RepayRequest
    {
        public string symbol;
        public string amount;
    }

    public void Withdraw(string symbol, float cTokenAmount, System.Action<bool, string> callback = null)
    {
        StartCoroutine(WithdrawCoroutine(symbol, cTokenAmount, callback));
    }

    private IEnumerator WithdrawCoroutine(string symbol, float cTokenAmount, System.Action<bool, string> callback)
    {
                    string url = "https://solcietyserver.vercel.app/api/kamino-withdraw";
        var payload = new WithdrawRequest { symbol = symbol, amount = cTokenAmount.ToString("F8") };
        string json = JsonUtility.ToJson(payload);

        using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("Withdraw successful: " + request.downloadHandler.text);
                callback?.Invoke(true, request.downloadHandler.text);
            }
            else
            {
                // For HTTP errors, the actual error message is in the response body
                string errorMessage = request.downloadHandler.text;
                if (string.IsNullOrEmpty(errorMessage))
                {
                    errorMessage = request.error;
                }
                Debug.LogError("Withdraw failed: " + errorMessage);
                callback?.Invoke(false, errorMessage);
            }
        }
    }

    [System.Serializable]
    public class WithdrawRequest
    {
        public string symbol;
        public string amount;
    }
} 