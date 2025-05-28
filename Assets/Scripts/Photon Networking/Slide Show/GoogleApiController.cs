using System;
using UnityEngine;
#if UNITY_WEBGL && !UNITY_EDITOR
using System.Runtime.InteropServices;
#endif

public static class GoogleApiController
{
    // Opens the OAuth popup
    public static void Login()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        GoogleApiLogin();
#else
        Debug.Log("GoogleApiController.Login() only works in WebGL builds, mocking response");
        GameObject.Find("ProjectorSet").GetComponent<SlideShowController>().OnAuthSuccess();
#endif
    }

    // Asks JS to list slide IDs; result will come back via your Unity message handlers
    public static void ListSlides(string presentationId)
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        GoogleApiListSlides(presentationId);
#else
        Debug.Log($"GoogleApiController.ListSlides({presentationId}), mocking response");
        GameObject.Find("ProjectorSet").GetComponent<SlideShowController>().MockOnSlidesListed();
#endif
    }

    // Asks JS to fetch a single thumbnail URL
    public static void GetThumbnailUrl(string presentationId, string pageId)
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        GoogleApiGetThumbnailUrl(presentationId, pageId);
#else
        Debug.Log($"GoogleApiController.GetThumbnailUrl({presentationId}, {pageId}), mock response");
        GameObject.Find("ProjectorSet").GetComponent<SlideShowController>().MockOnThumbnailUrlReceived();
#endif
    }

    // ----------------------------------------------------------------------------------------
    // WebGL plugin imports

#if UNITY_WEBGL && !UNITY_EDITOR
    [DllImport("__Internal")]
    private static extern void GoogleApiLogin();

    [DllImport("__Internal")]
    private static extern void GoogleApiListSlides(string presentationId);

    [DllImport("__Internal")]
    private static extern void GoogleApiGetThumbnailUrl(string presentationId, string pageId);
#endif
}
