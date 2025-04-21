using UnityEngine;
using System.Runtime.InteropServices;
using System.Diagnostics;

public static class ClipboardUtilities
{
    /// <summary>
    /// The text that will be copied on the next canvas click (WebGL only).
    /// </summary>
    public static string preparedText = "";

    /// <summary>
    /// True if PrepareCopy has been called and we're waiting for the user
    /// to click the canvas to actually perform the copy.
    /// </summary>
    public static bool isCopyPrepared = false;

#if UNITY_WEBGL && !UNITY_EDITOR
    // JS function in your ClipboardPlugin.jslib
    [DllImport("__Internal")]
    private static extern void PrepareCanvasCopy(string text);
#endif

    /// <summary>
    /// In WebGL: arms the next canvas click to copy the given text.
    /// In Editor/Standalone: copies immediately.
    /// </summary>
    public static void PrepareCopy(string text)
    {
        UnityEngine.Debug.Log($"CLIPBOARD: PrepareCopy() called with \"{text}\"");

        preparedText = text;
        isCopyPrepared = true;

#if UNITY_WEBGL && !UNITY_EDITOR
        PrepareCanvasCopy(text);
        UnityEngine.Debug.Log("CLIPBOARD: Armed WebGL canvas listener for next click.");
#else
        GUIUtility.systemCopyBuffer = text;
        UnityEngine.Debug.Log("CLIPBOARD: Editor/Standalone copy complete.");
        // no need to wait in Editor – reset immediately
        isCopyPrepared = false;
#endif
    }

    /// <summary>
    /// Cancels any pending WebGL copy (hides your "click anywhere" UI, etc.).
    /// </summary>
    public static void CancelPreparedCopy()
    {
        if (!isCopyPrepared) return;

        UnityEngine.Debug.Log("CLIPBOARD: CancelPreparedCopy()");
        isCopyPrepared = false;
        preparedText = "";
    }
}

/*using UnityEngine;
using System.Runtime.InteropServices;
using System.Diagnostics;

public static class ClipboardUtilities
{
    [DllImport("__Internal")]
    private static extern void CopyTextToClipboard(string text);

    /// <summary>
    /// Copies `text` to the system clipboard.
    /// On WebGL it will use the execCommand fallback above.
    /// In the Editor or Standalone builds it falls back to Unity’s systemCopyBuffer.
    /// </summary>
    public static void Copy(string text)
    {
        UnityEngine.Debug.Log("CLIPBOARD: Utilities copy function");

#if UNITY_WEBGL && !UNITY_EDITOR
        CopyTextToClipboard(text);
#else
        GUIUtility.systemCopyBuffer = text;
#endif
    }


}*/
