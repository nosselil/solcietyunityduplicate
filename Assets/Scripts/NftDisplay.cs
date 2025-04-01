using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using Cysharp.Threading.Tasks;
using Solana.Unity.Rpc.Types;
using Solana.Unity.SDK;
using System.Linq;
using System.Collections.Generic;
using UnityEngine.Rendering;

namespace Solana.Unity.SDK.Example
{
    public class NftDisplay : SimpleScreen
    {
        public bool isStartAreaScene;
        public GameObject loadingPanel;
        private bool _isLoadingTokens = false;
        private List<TokenItem> _instantiatedTokens = new();

        [SerializeField] private GameObject tokenItem;
        [SerializeField] private GameObject tokenItemMoveable;
        [SerializeField] private Transform InventoryContent; // First UI panel
        [SerializeField] private Transform WagerContent;       // Second UI panel
        [SerializeField] private Transform TradeContent;       // Third UI panel
        [SerializeField] private Transform FirstCanvesContent;   // Fourth UI panel

        public void Start()
        {
    //        GetOwnedTokenAccounts().AsAsyncUnitUniTask().Forget();
        }

        private void OnEnable()
        {
            // Optionally refresh tokens on enable:
      //      Debug.LogError("UpdateNfts : OnEnable");
            GetOwnedTokenAccounts().AsAsyncUnitUniTask().Forget();
        }

        public void UpdateNfts()
        {
          
            Debug.LogError("UpdateNfts");
            GetOwnedTokenAccounts().AsAsyncUnitUniTask().Forget();
        }

        private async UniTask GetOwnedTokenAccounts()
        {
            Debug.Log("1111111111111111");
            if (_isLoadingTokens) return;
            _isLoadingTokens = true;

            
            if (Web3.Wallet == null) Debug.LogError("web3 wallet is null");
            
            var tokens = await Web3.Wallet.GetTokenAccounts(Commitment.Processed);
            if (tokens == null) return;

            // Remove tokens not owned anymore and update amounts
            var tkToRemove = new List<TokenItem>();
            _instantiatedTokens.ForEach(tk =>
            {
                var tokenInfo = tk.TokenAccount.Account.Data.Parsed.Info;
                var match = tokens.Where(t => t.Account.Data.Parsed.Info.Mint == tokenInfo.Mint).ToArray();
                if (match.Length == 0 || match.Any(m => m.Account.Data.Parsed.Info.TokenAmount.AmountUlong == 0))
                {
                    tkToRemove.Add(tk);
                }
                else
                {
                    var newAmount = match[0].Account.Data.Parsed.Info.TokenAmount.UiAmountString;
                    // tk.UpdateAmount(newAmount);
                }
            });

            tkToRemove.ForEach(tk =>
            {
                _instantiatedTokens.Remove(tk);
                MainThreadDispatcher.Instance().Enqueue(() =>
                {
                    Destroy(tk.gameObject);
                });
            });

            // Add new tokens
            List<UniTask> loadingTasks = new List<UniTask>();
            if (tokens is { Length: > 0 })
            {
                Debug.Log("222222222222222");
                var tokenAccounts = tokens.OrderByDescending(
                    tk => tk.Account.Data.Parsed.Info.TokenAmount.AmountUlong);
                foreach (var item in tokenAccounts)
                {
                    // Instead of breaking, skip tokens with zero amount so that subsequent tokens are processed.
                    if (!(item.Account.Data.Parsed.Info.TokenAmount.AmountUlong > 0))
                        continue;

                    if (_instantiatedTokens.All(t => t.TokenAccount.Account.Data.Parsed.Info.Mint != item.Account.Data.Parsed.Info.Mint))
                    {
                        // InventoryContent block
                        if (InventoryContent != null)
                        {
                            await MainThreadDispatcher.Instance().EnqueueAsync(() =>
                            {
                                var tk1 = Instantiate(tokenItem, InventoryContent, true);
                                tk1.transform.localScale = Vector3.one;

                                var loadTask = Nft.Nft.TryGetNftData(
                                    item.Account.Data.Parsed.Info.Mint,
                                    Web3.Instance.WalletBase.ActiveRpcClient,
                                    commitment: Commitment.Processed).AsUniTask();
                                loadingTasks.Add(loadTask);
                                loadTask.ContinueWith(nft =>
                                {
                                    TokenItem tkInstance1 = tk1.GetComponent<TokenItem>();
                                    _instantiatedTokens.Add(tkInstance1);
                                    tk1.SetActive(true);
                                    if (tkInstance1)
                                        tkInstance1.InitializeData(item, this, nft).Forget();
                                }).Forget();
                            });
                        }

                        if (!isStartAreaScene)
                        {
                            // WagerContent block
                            if (WagerContent != null)
                            {
                                await MainThreadDispatcher.Instance().EnqueueAsync(() =>
                                {
                                    var tk2 = Instantiate(tokenItem, WagerContent, true);
                                    tk2.transform.localScale = Vector3.one;

                                    var loadTask = Nft.Nft.TryGetNftData(
                                        item.Account.Data.Parsed.Info.Mint,
                                        Web3.Instance.WalletBase.ActiveRpcClient,
                                        commitment: Commitment.Processed).AsUniTask();
                                    loadingTasks.Add(loadTask);
                                    loadTask.ContinueWith(nft =>
                                    {
                                        TokenItem tkInstance2 = tk2.GetComponent<TokenItem>();
                                        _instantiatedTokens.Add(tkInstance2);
                                        tk2.SetActive(true);
                                        if (tkInstance2)
                                            tkInstance2.InitializeData(item, this, nft).Forget();
                                    }).Forget();
                                });
                            }


                            // TradeContent block
                            if (TradeContent != null)
                            {
                                await MainThreadDispatcher.Instance().EnqueueAsync(() =>
                                {
                                    var tk3 = Instantiate(tokenItem, TradeContent, true);
                                    tk3.transform.localScale = Vector3.one;

                                    var loadTask = Nft.Nft.TryGetNftData(
                                        item.Account.Data.Parsed.Info.Mint,
                                        Web3.Instance.WalletBase.ActiveRpcClient,
                                        commitment: Commitment.Processed).AsUniTask();
                                    loadingTasks.Add(loadTask);
                                    loadTask.ContinueWith(nft =>
                                    {
                                        TokenItem tkInstance3 = tk3.GetComponent<TokenItem>();
                                        _instantiatedTokens.Add(tkInstance3);
                                        tk3.SetActive(true);
                                        if (tkInstance3)
                                            tkInstance3.InitializeData(item, this, nft).Forget();
                                    }).Forget();
                                });
                            }


                            // FirstCanvesContent block
                            if (FirstCanvesContent != null)
                            {
                                int index = 0;
                                await MainThreadDispatcher.Instance().EnqueueAsync(() =>
                                {
                                    index++;
                                    var tk4 = Instantiate(tokenItemMoveable, FirstCanvesContent, true);
                                    tk4.transform.localScale = Vector3.one;
                                    tk4.GetComponent<ObjectSettings>().Id = "object" + index;

                                    var loadTask = Nft.Nft.TryGetNftData(
                                        item.Account.Data.Parsed.Info.Mint,
                                        Web3.Instance.WalletBase.ActiveRpcClient,
                                        commitment: Commitment.Processed).AsUniTask();
                                    loadingTasks.Add(loadTask);
                                    loadTask.ContinueWith(nft =>
                                    {
                                        TokenItem tkInstance4 = tk4.GetComponent<TokenItem>();
                                        _instantiatedTokens.Add(tkInstance4);
                                        tk4.SetActive(true);
                                        if (tkInstance4)
                                            tkInstance4.InitializeData(item, this, nft).Forget();
                                    }).Forget();
                                });
                            }
                        }
                    }
                }
            }
            await UniTask.WhenAll(loadingTasks);

            if (loadingPanel != null)
            loadingPanel.SetActive(false);

            _isLoadingTokens = false;
        }
    }
}
