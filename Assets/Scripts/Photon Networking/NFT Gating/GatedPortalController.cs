using TMPro;
using UnityEngine;

public class GatedPortalController : MonoBehaviour
{
    [Header("UI Elements")]
    //[SerializeField] GameObject gatedPortalAccessCheckVeil;
    [SerializeField] GameObject gatedPortalAccessMessageParent;
    [SerializeField] TextMeshProUGUI gatedPortalAccessText;

    private void Start()
    {
        //gatedPortalAccessMessageParent.SetActive(true);
        gatedPortalAccessText.text = "A Solana Monkey Business NFT\r\nis required to enter this portal.";
    }

    /// <summary>
    /// Called by your button or trigger when the player attempts to enter the portal.
    /// </summary>
    public void CheckGatedPortalAccess()
    {
        // Hide any previous “denied” message
        //gatedPortalAccessText.gameObject.SetActive(true);
        gatedPortalAccessText.text = "Checking NFT Ownership...\r\nPlease complete Matrica authentication.";


        //Debug.Log("[GatedPortal] Mocking access");
        //NetworkController.Instance.SwitchRoomAndScene("Monkeydaoroom");

        // Kick off Matrica OAuth
        MatricaApiController.Login();
    }

    /// <summary>
    /// JS to Unity callback when OAuth completes successfully.
    /// </summary>
    public void OnAuthSuccess()
    {
        Debug.Log("[GatedPortal] Auth succeeded – now checking NFT ownership");
        // Once authenticated, ask JS to check NFT ownership
        MatricaApiController.CheckNftOwnership();
    }

    /// <summary>
    /// JS to Unity callback with the boolean result ("true" or "false").
    /// </summary>
    public void OnOwnershipChecked(string msg)
    {
        Debug.Log($"[GatedPortal] Ownership check returned: {msg}");                

        bool hasAccess = false;
        if (!bool.TryParse(msg, out hasAccess))
        {
            Debug.LogWarning("[GatedPortal] Could not parse ownership result");
        }

        if (hasAccess)
        {
            gatedPortalAccessText.text = "Access granted!";
            // Player owns the NFT, allow passage
            NetworkController.Instance.SwitchRoomAndScene("Monkeydaoroom");
        }
        else
        {
            // Deny access: show denial text for 3 seconds
            //gatedPortalAccessMessageParent.SetActive(true);
            gatedPortalAccessText.text = "No Solana Monkey Business NFT found\r\nin your Matrica Wallet.";
            Invoke(nameof(HideDeniedText), 3f);
        }
    }

    /// <summary>
    /// JS to Unity callback if the ownership check itself errored.
    /// (Optional, but good to cover failures.)
    /// </summary>
    public void OnOwnershipError(string err)
    {
        Debug.LogError($"[GatedPortal] Ownership check error: {err}");
        //gatedPortalAccessMessageParent.SetActive(true);
        gatedPortalAccessText.text = "There was an error checking NFT\r\nownerhship.Please try again.";
        Invoke(nameof(HideDeniedText), 3f);
    }

    private void HideDeniedText()
    {
        //gatedPortalAccessMessageParent.SetActive(false);
        gatedPortalAccessText.text = "A Solana Monkey Business NFT\r\nis required to enter this portal.";
    }

    public void MockAuthResponse()
    {
        Invoke("OnAuthSuccess", 1f);        
    }
}
