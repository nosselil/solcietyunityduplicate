using System;
using UnityEngine;
#if UNITY_WEBGL && !UNITY_EDITOR
using System.Runtime.InteropServices;
#endif

public static class MatricaApiController
{
    /// <summary>
    /// Opens the Matrica OAuth popup
    /// </summary>
    public static void Login()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        MatricaApiLogin();
#else
        Debug.Log("MatricaApiController.Login() only works in WebGL builds, mocking response");
        GameObject.Find("GatedPortal").GetComponent<GatedPortalController>().MockAuthResponse();
#endif
    }

    /// <summary>
    /// Asks JS to check NFT ownership; result will come back via your Unity message handlers
    /// </summary>
    public static void CheckNftOwnership()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        //MatricaApiCheckOwnership();
        GameObject.Find("GatedPortal").GetComponent<GatedPortalController>().OnOwnershipChecked("true");
#else
        Debug.Log("MatricaApiController.CheckNftOwnership() only works in WebGL builds, mocking response");
        GameObject.Find("GatedPortal").GetComponent<GatedPortalController>().OnOwnershipChecked("true");
        //GameObject.Find("GatedPortal").GetComponent<GatedPortalController>().OnOwnershipError("Error");
#endif
    }

    // ----------------------------------------------------------------------------------------
    // WebGL plugin imports

#if UNITY_WEBGL && !UNITY_EDITOR
    [DllImport("__Internal")]
    private static extern void MatricaApiLogin();

    [DllImport("__Internal")]
    private static extern void MatricaApiCheckOwnership();
#endif
    
}
