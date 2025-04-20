using CMF;
using Starter.Platformer;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.AudioSettings;

public class PlayerSetup : MonoBehaviour
{
    Transform mobileUI = null, chatCanvas = null, cameraTransform = null, loadingScreen = null, disconnectionHandler = null;

    public void SetupPlayer(float initialLookRotationY = 180f)
    {
        //Debug.Log("SMOOTH: Setup player called");
        // Find the local player core utilities, such as camera, chat window and mobile UI

        GameObject coreUtils = GameObject.FindGameObjectWithTag("LocalPlayerCoreUtilities");
        
        foreach (Transform child in coreUtils.transform)
        {
            if (child.CompareTag("LocalPlayerMobileUI"))
                mobileUI = child;
            else if (child.CompareTag("LocalPlayerChatCanvas"))
                chatCanvas = child;
            else if (child.CompareTag("LocalPlayerCamera"))
                cameraTransform = child;
            else if (child.CompareTag("LocalPlayerLoadingScreen"))
                loadingScreen = child;
            else if (child.CompareTag("LocalPlayerDisconnectionHandler"))
                disconnectionHandler = child;
        }

        // Link these to the local player

        // Core dependencies are used in all scenes
        #region Core Depedencies

        // Camera
        /*cameraTransform.GetComponent<SmoothPosition>().target = transform;
        cameraTransform.GetComponent<SmoothRotation>().target = transform;
        cameraTransform.GetComponentInChildren<CameraDistanceRaycaster>().ignoreList[0] = GetComponent<CapsuleCollider>();*/

        GetComponent<PlayerInput>().InitialLookRotationY = initialLookRotationY;

        // Mobile UI
        Debug.Log("MOBILE: WalletManager is mobile: " + WalletManager.instance.isMobile);

        if (WalletManager.instance.isMobile)
        {
            //mobileUI.gameObject.SetActive(true);
            Button jumpButton = mobileUI.Find("JumpBtn").GetComponent<Button>();
            //Button interactButton = mobileUI.Find("InteractBtn").GetComponent<Button>();
            Button chatButton = mobileUI.Find("ChatBtn").GetComponent<Button>();

            PlayerInput playerInput = GetComponent<PlayerInput>();
            PixelCrushers.DialogueSystem.ProximitySelector proximitySelector = GetComponentInChildren<PixelCrushers.DialogueSystem.ProximitySelector>();
            proximitySelector.defaultUseMessage = "(Double tap to interact)";


            playerInput.joystick = mobileUI.Find("JoyStickBase").GetComponent<Joystick>();
            jumpButton.onClick.AddListener(() => playerInput.SetMobileUIJumpPressed());

            chatButton.onClick.AddListener(() => MultiplayerChat.Instance.ToggleChatParent());

            // NOTE: Put these back if you want to restore the interact button which we've now replaced with the chat button
            //interactButton.onClick.AddListener(() => proximitySelector.OnMobileInteractButtonPressed());
            //proximitySelector.mobileInteractButton = interactButton; // Set a reference to the interact button so that it will correctly activate when we come close to interactable objects
        }
        else
            mobileUI.gameObject.SetActive(false);


        Invoke("DeactivateLoadingScreen", 0.5f);

        // Disconnection Handler
        DisconnectionController disconnectionController = disconnectionHandler.GetComponent<DisconnectionController>();
        disconnectionController.localPlayerGO = gameObject;
        disconnectionController.playerSpawnedInScene = true;

        Debug.Log("DISCONNECT: Player setup called, set up player spawned in scene");


        // Character keyboard input
        /*CharacterKeyboardInput characterKeyboardInput = GetComponent<CharacterKeyboardInput>();        
        characterKeyboardInput.joystick = mobileUI.Find("JoyStickBase").GetComponent<Joystick>();
        characterKeyboardInput.jumpButton = mobileUI.Find("JumpBtn").GetComponent<Button>();
        characterKeyboardInput.interactBtn = mobileUI.Find("InteractBtn").GetComponent<Button>();*/
        //characterKeyboardInput.thirdPersonCameraController = cameraTransform.GetComponentInChildren<ThirdPersonCameraController>();

        // Player
        //GetComponent<Starter.Platformer.Player>().cameraTransform = FindChildWithTag(cameraTransform, "CameraControls"); // TODO: Proper namespace referencing
        // CameraControls-tagged object is not actually the object that has the camera controls, but its rotation is the same as the camera's so we can use its transform

        // Advanced Walker Controller
        //AdvancedWalkerController advancedWalkerController = GetComponent<AdvancedWalkerController>();
        //advancedWalkerController.cameraTransform = FindChildWithTag(cameraTransform, "CameraControls");

        // Chat





        #endregion

        // The following dependencies are only used in certain scenes, and will have to be assigned to the "DependencyContainer" component under LocalPlayerUtilities object
        #region Scene-Specific Dependencies 

        DependencyContainer dependencyContainer = coreUtils.GetComponentInChildren<DependencyContainer>();

        // NFT Interaction (used in gallery scenes)
        NFTInteraction nftInteraction = GetComponent<NFTInteraction>();
        if (nftInteraction != null)
        {
            nftInteraction.popupPanel = dependencyContainer.popupPanel;
            nftInteraction.artistText = dependencyContainer.artistText;
            nftInteraction.artworkText = dependencyContainer.artworkText;
            nftInteraction.artworkText2 = dependencyContainer.artworkText2;
            nftInteraction.artworkImage = dependencyContainer.artworkImage;
            nftInteraction.buyNFTfromGalleryScript = dependencyContainer.buyNFTfromGalleryScript;

            // Now find the ArtContainer and hook up the onUse event on every Usable below it:
            GameObject artContainer = GameObject.FindGameObjectWithTag("ArtContainer");
            if (artContainer != null)
            {
                // This will grab Usable components on all children (recursively).
                PixelCrushers.DialogueSystem.Wrappers.Usable[] usables = artContainer.GetComponentsInChildren<PixelCrushers.DialogueSystem.Wrappers.Usable>(includeInactive: true);
                foreach (var usable in usables)
                {
                    if (usable.events != null)
                    {
                        // Add the ShowNftDetailsPopup callback to the onUse UnityEvent.
                        usable.events.onUse.AddListener(nftInteraction.ShowNftDetailsPopup);
                    }
                }
            }
        }    

        // TextMeshFader
        TextMeshFader textMeshFader = GetComponent<TextMeshFader>();
        if (textMeshFader != null)
            textMeshFader.textMeshes = dependencyContainer.textMeshes;

        #endregion

        //nftInteraction

        //Debug.Log("SMOOTH: cameraTransform.smoothPosition GO: " + cameraTransform.gameObject.name + ", target transform: " + cameraTransform.GetComponent<SmoothPosition>().target + " current object name: " + gameObject.name);
        //Debug.Log("SMOOTH: cameraTransform.smoothRotation GO: " + cameraTransform.gameObject.name + " target transform: " + cameraTransform.GetComponent<SmoothRotation>().target + ", current object name: " + gameObject.name);

        // Chat Canvas doesn't require any additional setup

        // Mobile UI
        //TODO: We need to set JumpBtn and InteractBtn actions by adding ProximitySelector.InteractBtn from capGuy and CharacterKeyBoardInput.jumpBtn to jumpBtn from the parent player object

        Debug.Log("Player setup completed.");

    }

    private void DeactivateLoadingScreen()
    {
        loadingScreen.gameObject.SetActive(false); // If the player setup scripts gets triggered, it means that our player must have spawned and the loading has been completed
    }

    public Transform FindChildWithTag(Transform parent, string tag)
    {
        foreach (Transform child in parent)
        {
            if (child.CompareTag(tag))
                return child;
            Transform result = FindChildWithTag(child, tag);
            if (result != null)
                return result;
        }
        return null;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
