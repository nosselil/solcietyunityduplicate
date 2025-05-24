using UnityEngine;

public class GatedPortalController : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] GameObject gatedPortalAccessCheckVeil;
    [SerializeField] public GameObject gatedPortalAccessDeniedText;

    /// <summary>
    /// Called by your button or trigger when the player attempts to enter the portal.
    /// </summary>
    public void CheckGatedPortalAccess()
    {
        // Hide any previous “denied” message
        gatedPortalAccessDeniedText.SetActive(false);

        // Show the “checking…” veil
        gatedPortalAccessCheckVeil.SetActive(true);

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
        // Hide the “checking…” veil
        gatedPortalAccessCheckVeil.SetActive(false);

        bool hasAccess = false;
        if (!bool.TryParse(msg, out hasAccess))
        {
            Debug.LogWarning("[GatedPortal] Could not parse ownership result");
        }

        if (hasAccess)
        {
            // Player owns the NFT—allow passage
            NetworkController.Instance.SwitchRoomAndScene("Monkeydaoroom");
        }
        else
        {
            // Deny access: show denial text for 3 seconds
            gatedPortalAccessDeniedText.SetActive(true);
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
        gatedPortalAccessCheckVeil.SetActive(false);
        gatedPortalAccessDeniedText.SetActive(true);
        Invoke(nameof(HideDeniedText), 3f);
    }

    private void HideDeniedText()
    {
        gatedPortalAccessDeniedText.SetActive(false);
    }
}
