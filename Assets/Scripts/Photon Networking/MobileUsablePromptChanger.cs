using UnityEngine;

public class MobileUsablePromptChanger : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Invoke("ChangePromptText", 0.5f); // Change the text once the Usable has been set up properly
    }

    void ChangePromptText()
    {
        // Override the use message if we're on mobile
        if (WalletManager.instance.isMobile)
            GetComponent<PixelCrushers.DialogueSystem.Usable>().overrideUseMessage = "(Double tap to interact)";
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
