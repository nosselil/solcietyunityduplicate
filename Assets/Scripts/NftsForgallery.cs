//old

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using Cysharp.Threading.Tasks;
using Solana.Unity.Rpc.Types;
using Solana.Unity.SDK;
using System.Linq;
using System.Collections.Generic;

namespace Solana.Unity.SDK.Example
{
    public class NftsForgallery : SimpleScreen
    {

        public ScrollRect scrollrect;

        private bool _isLoadingTokens = false;
        private List<TokenItem> _instantiatedTokens = new();

        [SerializeField] private GameObject tokenItem;
        //  [SerializeField] private Transform tokenContainer;

        [SerializeField] private Transform[] spawnPoints; // Assign your 3 GameObjects here in the Inspector
        private int spawnIndex = 0; // Track which spawn point to use next

        public void Start()
        {
            WalletManager.instance.scrollrect = scrollrect;
            GetOwnedTokenAccounts().AsAsyncUnitUniTask().Forget();
        }

        private async UniTask GetOwnedTokenAccounts()
        {
            if (_isLoadingTokens) return;
            _isLoadingTokens = true;

            var tokens = await Web3.Wallet.GetTokenAccounts(Commitment.Processed);
            if (tokens == null) return;

            // Remove tokens not owned anymore
            var tkToRemove = new List<TokenItem>();
            _instantiatedTokens.ForEach(tk =>
            {
                var tokenInfo = tk.TokenAccount.Account.Data.Parsed.Info;
                var match = tokens.Where(t => t.Account.Data.Parsed.Info.Mint == tokenInfo.Mint).ToArray();
                if (match.Length == 0 || match.Any(m => m.Account.Data.Parsed.Info.TokenAmount.AmountUlong == 0))
                {
                    tkToRemove.Add(tk);
                }
            });

            tkToRemove.ForEach(tk =>
            {
                _instantiatedTokens.Remove(tk);
                MainThreadDispatcher.Instance().Enqueue(() => Destroy(tk.gameObject));
            });

            // Add new tokens
            List<UniTask> loadingTasks = new List<UniTask>();
            if (tokens is { Length: > 0 })
            {
                var tokenAccounts = tokens.OrderByDescending(
                    tk => tk.Account.Data.Parsed.Info.TokenAmount.AmountUlong);

                foreach (var item in tokenAccounts)
                {
                    if (!(item.Account.Data.Parsed.Info.TokenAmount.AmountUlong > 0)) break;
                    if (_instantiatedTokens.All(t => t.TokenAccount.Account.Data.Parsed.Info.Mint != item.Account.Data.Parsed.Info.Mint))
                    {
                        await MainThreadDispatcher.Instance().EnqueueAsync(() =>
                        {
                            // Instantiate NFT at the assigned spawn point
                            var tk = Instantiate(tokenItem, spawnPoints[spawnIndex], false);
                            tk.transform.localScale = Vector3.one;
                            tk.SetActive(true);

                            // Cycle through spawn points
                            spawnIndex = (spawnIndex + 1) % spawnPoints.Length;

                            var loadTask = Nft.Nft.TryGetNftData(item.Account.Data.Parsed.Info.Mint,
                                Web3.Instance.WalletBase.ActiveRpcClient, commitment: Commitment.Processed).AsUniTask();
                            loadingTasks.Add(loadTask);
                            loadTask.ContinueWith(nft =>
                            {
                                TokenItem tkInstance = tk.GetComponent<TokenItem>();
                                _instantiatedTokens.Add(tkInstance);
                                if (tkInstance)
                                {
                                    tkInstance.InitializeData(item, this, nft).Forget();
                                }
                            }).Forget();
                        });
                    }
                }
            }

            await UniTask.WhenAll(loadingTasks);
            _isLoadingTokens = false;
        }
    }
}