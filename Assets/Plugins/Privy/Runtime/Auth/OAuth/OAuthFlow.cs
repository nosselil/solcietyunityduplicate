using System.Threading.Tasks;
using UnityEngine;
using Application = UnityEngine.Device.Application;

namespace Privy
{
    internal interface OAuthFlow
    {
        Task<OAuthResultData> PerformOAuthFlow(string oAuthUrl, string redirectUri);

        internal static OAuthFlow GetPlatformOAuthFlow()
        {
            switch (Application.platform)
            {
                case RuntimePlatform.IPhonePlayer:
                    return new OAuthIOSWebAuthenticationFlow();
                case RuntimePlatform.WebGLPlayer:
                    return new OAuthWebGLPopupFlow();
                default:
                    return new OAuthExternalBrowserFlow();
            }
        }
    }
}