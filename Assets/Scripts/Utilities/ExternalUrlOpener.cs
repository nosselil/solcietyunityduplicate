using System.Runtime.InteropServices;
using UnityEngine;

public class ExternalUrlOpener : MonoBehaviour
{
    // Import the JavaScript function from the `externalUrlOpener.jslib`
    [DllImport("__Internal")]
    private static extern void OpenExternalUrl(string url);

    // Static method to open an external URL
    public static void OpenExternalLink(string url)
    {
        // This check ensures it only runs in a WebGL build
#if UNITY_WEBGL && !UNITY_EDITOR
        OpenExternalUrl(url);
#else
        // Fallback to Unity's standard URL opening for non-WebGL builds
        Application.OpenURL(url);
#endif
    }
}
