using CMF;
using UnityEngine;
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

        // Camera
        cameraTransform.GetComponent<SmoothPosition>().target = transform;
        cameraTransform.GetComponent<SmoothRotation>().target = transform;

        Debug.Log("SMOOTH: cameraTransform.smoothPosition GO: " + cameraTransform.gameObject.name + ", target transform: " + cameraTransform.GetComponent<SmoothPosition>().target + " current object name: " + gameObject.name);
        Debug.Log("SMOOTH: cameraTransform.smoothRotation GO: " + cameraTransform.gameObject.name + " target transform: " + cameraTransform.GetComponent<SmoothRotation>().target + ", current object name: " + gameObject.name);

        // Chat Canvas doesn't require any additional setup

        // Mobile UI
        //TODO: We need to set JumpBtn and InteractBtn actions by adding ProximitySelector.InteractBtn from capGuy and CharacterKeyBoardInput.jumpBtn to jumpBtn from the parent player object

        Debug.Log("Player setup completed.");

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
