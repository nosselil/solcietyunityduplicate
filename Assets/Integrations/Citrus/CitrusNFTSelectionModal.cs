using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Solana.Unity.Rpc.Types;
using Solana.Unity.Rpc;
using Solana.Unity.SDK;
using Solana.Unity.SDK.Example;
using Solana.Unity.SDK.Nft;

public class CitrusNFTSelectionModal : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject modalPanel;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private Transform nftContainer;
    [SerializeField] private GameObject nftItemPrefab;
    [SerializeField] private Button closeButton;
    [SerializeField] private Button confirmButton;
    [SerializeField] private TextMeshProUGUI selectedNFTText;
    [SerializeField] private GameObject loadingPanel;
    [SerializeField] private TextMeshProUGUI noNFTsText;

    // Item UI is provided by the prefab via CitrusNFTItemUI

    private List<CitrusNFTItem> nftItems = new List<CitrusNFTItem>();
    private CitrusNFTItem selectedNFT;
    private string currentCollectionId;
    private string currentCollectionName;
    private System.Action<string> onNFTSelectedCallback;

    [System.Serializable]
    public class ServerNFT
    {
        public string mint;
        public string name;
        public string imageUrl;
        public string collection;
    }

    [System.Serializable]
    public class UserNFTsResponse
    {
        public bool success;
        public ServerNFT[] nfts;
        public int count;
    }

    [System.Serializable]
    public class CitrusNFTItem
    {
        public string mintAddress;
        public string name;
        public string imageUrl;
        public GameObject uiObject;
        public Button selectButton;
        public CitrusNFTItemUI uiComponent;
        public Texture2D nftTexture; // Store the texture directly
    }

    void Awake()
    {
        // Setup button listeners
        if (closeButton != null)
            closeButton.onClick.AddListener(CloseModal);

        if (confirmButton != null)
            confirmButton.onClick.AddListener(OnConfirmSelected);
    }

    public void Show(string collectionId, string collectionName, System.Action<string> callback)
    {
        currentCollectionId = collectionId;
        currentCollectionName = collectionName;
        onNFTSelectedCallback = callback;

        // Update title
        if (titleText != null)
            titleText.text = $"Select NFT from {collectionName}";

        // Show modal
        modalPanel.SetActive(true);

        // Clear previous selection
        selectedNFT = null;
        if (selectedNFTText != null)
            selectedNFTText.text = "No NFT selected";

        // Load NFTs for this collection
        LoadNFTsForCollection().Forget();
    }

    private async UniTask LoadNFTsForCollection()
    {
        Debug.Log($"CitrusNFTSelectionModal: Starting to load NFTs for collection: {currentCollectionId}");

        // Show loading
        if (loadingPanel != null)
            loadingPanel.SetActive(true);

        if (noNFTsText != null)
            noNFTsText.gameObject.SetActive(false);

        // Clear previous NFTs
        ClearNFTItems();

        // Require a connected wallet (CitrusAPI also checks, but we provide early UI feedback)
        string wallet = WalletManager.instance != null ? WalletManager.instance.walletAddress : null;
        if (string.IsNullOrEmpty(wallet))
        {
            Debug.LogWarning("CitrusNFTSelectionModal: No wallet connected; cannot fetch user NFTs.");
            ShowNoNFTsMessage();
            if (loadingPanel != null) loadingPanel.SetActive(false);
            return;
        }

        if (CitrusAPI.Instance == null)
        {
            Debug.LogError("CitrusNFTSelectionModal: CitrusAPI.Instance is null. Ensure CitrusAPI is present in the scene.");
            ShowNoNFTsMessage();
            if (loadingPanel != null) loadingPanel.SetActive(false);
            return;
        }

        // Fetch JSON from CitrusAPI (includes wallet)
        var tcs = new UniTaskCompletionSource<string>();
        CitrusAPI.Instance.FetchUserNftsByCollection(currentCollectionId, json => tcs.TrySetResult(json));
        string jsonResponse = await tcs.Task;

        if (string.IsNullOrEmpty(jsonResponse))
        {
            Debug.LogError("CitrusNFTSelectionModal: Empty response from server.");
            ShowNoNFTsMessage();
            if (loadingPanel != null) loadingPanel.SetActive(false);
            return;
        }

        Debug.Log($"CitrusNFTSelectionModal: Server response: {jsonResponse}");

        // Parse only
        UserNFTsResponse response = null;
        try
        {
            response = JsonUtility.FromJson<UserNFTsResponse>(jsonResponse);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"CitrusNFTSelectionModal: JSON parse error: {e.Message}");
            ShowNoNFTsMessage();
            if (loadingPanel != null) loadingPanel.SetActive(false);
            return;
        }

        if (response != null && response.success && response.nfts != null)
        {
            Debug.Log($"CitrusNFTSelectionModal: Found {response.nfts.Length} NFTs from server");

            // Validate required UI refs before creating items
            if (nftItemPrefab == null || nftContainer == null)
            {
                Debug.LogError("CitrusNFTSelectionModal: NFT item prefab or container is not assigned in the inspector.");
                ShowNoNFTsMessage();
                if (loadingPanel != null) loadingPanel.SetActive(false);
                return;
            }

            // Process each NFT from server
            foreach (var serverNFT in response.nfts)
            {
                if (string.IsNullOrEmpty(serverNFT?.mint))
                    continue;

                IRpcClient rpcClient = Web3.Rpc ?? ClientFactory.GetClient("https://blissful-tiniest-aura.solana-mainnet.quiknode.pro/8305bf1921b2c1cc4067111258d59f82a873d509/");
                Nft nftData = null;
                try
                {
                    nftData = await Nft.TryGetNftData(
                        serverNFT.mint,
                        rpcClient,
                        commitment: Commitment.Processed);
                }
                catch (System.Exception ex)
                {
                    Debug.LogError($"CitrusNFTSelectionModal: Error fetching NFT data for {serverNFT.mint}: {ex.Message}");
                }
                if (nftData != null)
                {
                    Debug.Log($"CitrusNFTSelectionModal: Got NFT data for {serverNFT.mint}: {nftData.metaplexData?.data?.offchainData?.name}");

                    if (IsNFTInCollection(nftData, currentCollectionId))
                    {
                        Debug.Log($"CitrusNFTSelectionModal: NFT {serverNFT.mint} belongs to collection {currentCollectionId}");

                        CitrusNFTItem nftItem = new CitrusNFTItem
                        {
                            mintAddress = serverNFT.mint,
                            name = nftData.metaplexData?.data?.offchainData?.name ?? "Unknown NFT",
                            imageUrl = "",
                            uiObject = null,
                            selectButton = null
                        };

                        if (nftData.metaplexData?.nftImage?.file != null)
                        {
                            nftItem.nftTexture = nftData.metaplexData.nftImage.file;
                        }

                        await CreateNFTItemUI(nftItem);
                    }
                    else
                    {
                        Debug.Log($"CitrusNFTSelectionModal: NFT {serverNFT.mint} does NOT belong to collection {currentCollectionId}");
                    }
                }
                else
                {
                    Debug.Log($"CitrusNFTSelectionModal: No NFT data found for {serverNFT.mint}");
                }
            }
        }
        else
        {
            Debug.LogError("CitrusNFTSelectionModal: Failed to parse server response or empty nfts array");
            ShowNoNFTsMessage();
        }

        // Hide loading
        if (loadingPanel != null)
            loadingPanel.SetActive(false);
    }

    private bool IsNFTInCollection(Nft nftData, string collectionId)
    {
        // Use off-chain metadata collection name/family and attributes
        var offchain = nftData.metaplexData?.data?.offchainData;
        if (offchain?.collection != null)
        {
            // Match against the selected collection name first (most reliable cross-source)
            if (!string.IsNullOrEmpty(currentCollectionName))
            {
                if (offchain.collection.family == currentCollectionName || offchain.collection.name == currentCollectionName)
                    return true;
            }
            // Fallback: sometimes the collection id matches family/name
            if (!string.IsNullOrEmpty(collectionId))
            {
                if (offchain.collection.family == collectionId || offchain.collection.name == collectionId)
                    return true;
            }
        }

        // Fall back to attributes that may store collection name
        if (offchain?.attributes != null && !string.IsNullOrEmpty(currentCollectionName))
        {
            foreach (var attribute in offchain.attributes)
            {
                if ((attribute.trait_type == "Collection" || attribute.trait_type == "Collection Name") && attribute.value == currentCollectionName)
                    return true;
            }
        }

        return false;
    }

    private async UniTask CreateNFTItemUI(CitrusNFTItem nftItem)
    {
        if (nftItemPrefab == null || nftContainer == null)
        {
            Debug.LogError("CitrusNFTSelectionModal: Cannot create NFT item UI because prefab or container is null.");
            return;
        }
        // Instantiate NFT item prefab
        GameObject nftObject = Instantiate(nftItemPrefab, nftContainer);
        nftItem.uiObject = nftObject;

        // Prefer dedicated UI component on the prefab
        nftItem.uiComponent = nftObject.GetComponentInChildren<CitrusNFTItemUI>(true);
        if (nftItem.uiComponent != null)
        {
            Sprite sprite = null;
            if (nftItem.nftTexture != null)
            {
                sprite = Sprite.Create(
                    nftItem.nftTexture,
                    new Rect(0, 0, nftItem.nftTexture.width, nftItem.nftTexture.height),
                    new Vector2(0.5f, 0.5f)
                );
            }
            nftItem.uiComponent.SetNFTData(nftItem.name, nftItem.mintAddress, sprite);
        }

        // Find/select button on the prefab and wire up click
        nftItem.selectButton = nftObject.GetComponentInChildren<Button>(true);
        if (nftItem.selectButton != null)
        {
            nftItem.selectButton.onClick.RemoveAllListeners();
            nftItem.selectButton.onClick.AddListener(() => OnNFTSelected(nftItem));
        }

        nftItems.Add(nftItem);
    }



    private void OnNFTSelected(CitrusNFTItem nftItem)
    {
        // Deselect previous NFT
        if (selectedNFT != null)
        {
            if (selectedNFT.uiComponent != null)
            {
                selectedNFT.uiComponent.SetSelected(false);
            }
            else if (selectedNFT.selectButton != null)
            {
                var img = selectedNFT.selectButton.GetComponent<Image>();
                if (img != null) img.color = Color.white;
            }
        }

        // Select new NFT
        selectedNFT = nftItem;
        if (selectedNFT != null)
        {
            if (selectedNFT.uiComponent != null)
            {
                selectedNFT.uiComponent.SetSelected(true);
            }
            else if (selectedNFT.selectButton != null)
            {
                var img = selectedNFT.selectButton.GetComponent<Image>();
                if (img != null) img.color = Color.green;
            }
        }

        // Update selected text
        if (selectedNFTText != null)
        {
            selectedNFTText.text = $"Selected: {selectedNFT.name}";
        }
    }

    private void OnConfirmSelected()
    {
        if (selectedNFT != null)
        {
            onNFTSelectedCallback?.Invoke(selectedNFT.mintAddress);
            CloseModal();
        }
        else
        {
            Debug.LogWarning("No NFT selected");
        }
    }

    private void ShowNoNFTsMessage()
    {
        if (loadingPanel != null)
            loadingPanel.SetActive(false);

        if (noNFTsText != null)
        {
            noNFTsText.gameObject.SetActive(true);
            noNFTsText.text = $"No NFTs found in {currentCollectionName} collection";
        }
    }

    private void ClearNFTItems()
    {
        foreach (var nftItem in nftItems)
        {
            if (nftItem.uiObject != null)
                Destroy(nftItem.uiObject);
        }
        nftItems.Clear();
    }

    private void CloseModal()
    {
        modalPanel.SetActive(false);
        ClearNFTItems();
    }

    private string ShortenAddress(string address)
    {
        if (string.IsNullOrEmpty(address) || address.Length < 8)
            return address;

        return address.Substring(0, 4) + "..." + address.Substring(address.Length - 4);
    }
}