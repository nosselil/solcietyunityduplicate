using UnityEngine;
using System.Runtime.InteropServices;
using System.Diagnostics;

public class ClipboardJSBridge : MonoBehaviour
{
    /*[Tooltip("The text that will be copied when the user clicks the Unity canvas")]
    [SerializeField]
    private string textToCopy = "Sample text to copy";

    // Import the JS function from your .jslib
#if UNITY_WEBGL && !UNITY_EDITOR
    [DllImport("__Internal")]
    private static extern void RegisterCanvasCopy(string text);
#endif

    private void Start()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        // Register the canvas click - copy logic, passing in the desired text
        UnityEngine.Debug.Log("CLIPBOARD: Before register canvas copy");
        RegisterCanvasCopy(textToCopy);
        UnityEngine.Debug.Log("CLIPBOARD: After canvas copy");
#else
        UnityEngine.Debug.Log("CLIPBOARD: ClipboardJSBridge: WebGL only");
#endif
    }*/
}