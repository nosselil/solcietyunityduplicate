//Since we're using an iframe implementation for WebGL builds, to have communication between iframe/browser and unity, we need a game object
//The reason for this is, the unity instance in a webgl build, can only communicate with a game object, it can't send messages to a static script
//This class, is effectively a game object instantiator, and is used to receive messages from unity, and then send those messages to the WebviewManager class to handle

using UnityEngine;

namespace Privy
{
    internal class WebViewManagerBridge : MonoBehaviour
    {
        private WebViewManager _webViewManager;

        public void Initialize(WebViewManager webViewManager)
        {
            _webViewManager = webViewManager;
        }

        public void OnMessageReceived(string message)
        {
            //Unity sends message to this function, as this is the game object it can talk to
            //Then we trigger our actual message handler
            _webViewManager?.OnMessageReceived(message);
        }

        public void OnWebViewReady()
        {
            _webViewManager?.OnWebViewReady();
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void CreateBridge()
        {
            //This is creating the game object that Unity can communicate with
            GameObject bridgeObject = new GameObject("WebViewManagerBridge");
            bridgeObject.AddComponent<WebViewManagerBridge>();
            DontDestroyOnLoad(bridgeObject);

#if UNITY_IOS || UNITY_ANDROID || UNITY_STANDALONE_OSX || UNITY_EDITOR_OSX
            GameObject webViewGameObject = new GameObject("WebViewObject");
            WebViewObject webViewObject = webViewGameObject.AddComponent<WebViewObject>();
            DontDestroyOnLoad(webViewGameObject);
#endif
        }
    }
}