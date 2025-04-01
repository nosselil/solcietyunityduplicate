using System;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using Solana.Unity.Extensions.Models.TokenMint;
using Solana.Unity.Rpc.Models;
using Solana.Unity.SDK.Utility;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using WebSocketSharp;


namespace Solana.Unity.SDK.Example
{
    public class TokenItem : MonoBehaviour
    {
        public RawImage logo;
        

        public TokenAccount TokenAccount;
        private Nft.Nft _nft;
        private SimpleScreen _parentScreen;
        private Texture2D _texture;

        private void Awake()
        {
            logo = GetComponentInChildren<RawImage>();
        }

        private void Start()
        {

        }
        public async UniTask InitializeData(TokenAccount tokenAccount, SimpleScreen screen, Solana.Unity.SDK.Nft.Nft nftData = null)
        {
            _parentScreen = screen;
            TokenAccount = tokenAccount;
            if (nftData != null && ulong.Parse(tokenAccount.Account.Data.Parsed.Info.TokenAmount.Amount) == 1)
            {
                await UniTask.SwitchToMainThread();
                _nft = nftData;
            

                if (logo != null)
                {
                    logo.texture = nftData.metaplexData?.nftImage?.file;
                }
            }
            else
            {

       
                if (nftData?.metaplexData?.data?.offchainData?.default_image != null)
                {
                    await LoadAndCacheTokenLogo(nftData.metaplexData?.data?.offchainData?.default_image, tokenAccount.Account.Data.Parsed.Info.Mint);
                }
                else
                {
                    var tokenMintResolver = await WalletScreen.GetTokenMintResolver();
                    TokenDef tokenDef = tokenMintResolver.Resolve(tokenAccount.Account.Data.Parsed.Info.Mint);
                    if (tokenDef.TokenName.IsNullOrEmpty() || tokenDef.Symbol.IsNullOrEmpty()) return;
        
                    await LoadAndCacheTokenLogo(tokenDef.TokenLogoUrl, tokenDef.TokenMint);
                }
            }
        }



        /// <summary>
        /// If the given URL uses a specific Pinata subdomain (and includes a token),
        /// rewrite it to use the public Pinata gateway instead.
        /// </summary>
      


        private async Task LoadAndCacheTokenLogo(string logoUrl, string tokenMint)
        {
            if(logoUrl.IsNullOrEmpty() || tokenMint.IsNullOrEmpty() || logo is null) return;
            var texture = await FileLoader.LoadFile<Texture2D>(logoUrl);
            _texture = FileLoader.Resize(texture, 75, 75);
            FileLoader.SaveToPersistentDataPath(Path.Combine(Application.persistentDataPath, $"{tokenMint}.png"), _texture);
            logo.texture = _texture;
        }

        public void TransferAccount()
        {
            if (_nft != null)
            {
                _parentScreen.manager.ShowScreen(_parentScreen, "transfer_screen", _nft);
            }
            else
            {
                Debug.Log("_nft is null");  
            }
        }

        public void UpdateAmount(string newAmount)
        {
         //   MainThreadDispatcher.Instance().Enqueue(() => { amount_txt.text = newAmount; });
        }


        public void ApplyRawImageTextureToMeshRenderer  ( MeshRenderer meshRenderer)
        {

            // Ensure that the RawImage and MeshRenderer are not null
            if (logo != null && logo.texture != null && meshRenderer != null)
            {
                // Get the texture from the RawImage
                Texture2D texture = logo.texture as Texture2D;
              
                if (texture != null)
                {
                    // Create a new material using the Standard Shader
                    Material material = new Material(Shader.Find("Standard"));

                    // Assign the texture to the material's main texture
                    material.mainTexture = texture;

                    // Apply the material to the MeshRenderer
                    meshRenderer.material = material;
                }
                else
                {
                    Debug.LogError("The RawImage does not have a valid texture.");
                }
            }
            else
            {
                Debug.LogError("RawImage or MeshRenderer is null.");
            }
        }
    }
}
