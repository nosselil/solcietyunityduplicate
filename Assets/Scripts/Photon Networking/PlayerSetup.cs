using CMF;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.AudioSettings;

public class PlayerSetup : MonoBehaviour
{
    Transform mobileUI = null, chatCanvas = null, cameraTransform = null;

    public void SetupPlayer()
    {
        Debug.Log("SMOOTH: Setup player called");
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
        }

        // Link these to the local player

        // Core dependencies are used in all scenes
        #region Core Depedencies

        // Camera
        cameraTransform.GetComponent<SmoothPosition>().target = transform;
        cameraTransform.GetComponent<SmoothRotation>().target = transform;
        cameraTransform.GetComponentInChildren<CameraDistanceRaycaster>().ignoreList[0] = GetComponent<CapsuleCollider>();
        
        // Character keyboard input

        CharacterKeyboardInput characterKeyboardInput = GetComponent<CharacterKeyboardInput>();        
        characterKeyboardInput.joystick = mobileUI.Find("JoyStickBase").GetComponent<Joystick>();
        characterKeyboardInput.jumpButton = mobileUI.Find("JumpBtn").GetComponent<Button>();
        characterKeyboardInput.interactBtn = mobileUI.Find("InteractBtn").GetComponent<Button>();
        characterKeyboardInput.thirdPersonCameraController = cameraTransform.GetComponentInChildren<ThirdPersonCameraController>();

        // Advanced Walker Controller
        AdvancedWalkerController advancedWalkerController = GetComponent<AdvancedWalkerController>();
        advancedWalkerController.cameraTransform = FindChildWithTag(cameraTransform, "CameraControls");

        #endregion

        // The following dependencies are only used in certain scenes, and will have to be assigned to the "DependencyContainer" component under LocalPlayerUtilities object
        #region Scene-Specific Dependencies 

        DependencyContainer dependencyContainer = coreUtils.GetComponentInChildren<DependencyContainer>();

        // NFT Interaction
        NFTInteraction nftInteraction = GetComponent<NFTInteraction>();
        nftInteraction.popupPanel = dependencyContainer.popupPanel;
        nftInteraction.artistText = dependencyContainer.artistText;
        nftInteraction.artworkText = dependencyContainer.artworkText;
        nftInteraction.artworkText2 = dependencyContainer.artworkText2;
        nftInteraction.artworkImage = dependencyContainer.artworkImage;
        nftInteraction.buyNFTfromGalleryScript = dependencyContainer.buyNFTfromGalleryScript;

        // TextMeshFader
        TextMeshFader textMeshFader = GetComponent<TextMeshFader>();
        textMeshFader.textMeshes = dependencyContainer.textMeshes;

        #endregion

        //nftInteraction

        Debug.Log("SMOOTH: cameraTransform.smoothPosition GO: " + cameraTransform.gameObject.name + ", target transform: " + cameraTransform.GetComponent<SmoothPosition>().target + " current object name: " + gameObject.name);
        Debug.Log("SMOOTH: cameraTransform.smoothRotation GO: " + cameraTransform.gameObject.name + " target transform: " + cameraTransform.GetComponent<SmoothRotation>().target + ", current object name: " + gameObject.name);

        // Chat Canvas doesn't require any additional setup

        // Mobile UI
        //TODO: We need to set JumpBtn and InteractBtn actions by adding ProximitySelector.InteractBtn from capGuy and CharacterKeyBoardInput.jumpBtn to jumpBtn from the parent player object

        Debug.Log("Player setup completed.");

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
