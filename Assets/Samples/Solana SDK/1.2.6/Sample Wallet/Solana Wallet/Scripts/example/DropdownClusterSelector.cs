using Solana.Unity.SDK;
using TMPro;
using UnityEngine;
using Cysharp.Threading.Tasks;


// ReSharper disable once CheckNamespace

public class DropdownClusterSelector : MonoBehaviour
{
    void OnEnable()
    {
        // DEBUG: Instead of reading the cluster from player prefs, we just set it to DevNet for now
        int rpcDefault = (int)RpcCluster.DevNet; // PlayerPrefs.GetInt("rpcCluster", 0);
        RpcNodeDropdownSelected(rpcDefault);
        Web3.OnWalletInstance += () => RpcNodeDropdownSelected(rpcDefault);
        GetComponent<TMP_Dropdown>().value = rpcDefault;

        gameObject.SetActive(false); // DEBUG: For now, deactivate the dropdown to prevent selection
    }

    public void RpcNodeDropdownSelected(int value)
    {
        if (Web3.Instance == null) return;
        Web3.Instance.rpcCluster = (RpcCluster)value;
        Web3.Instance.customRpc = value switch
        {
            (int)RpcCluster.MainNet => "https://rpc.mainnet-alpha.sonic.game", // Corrected MainNet URL
            (int)RpcCluster.TestNet => "https://api.testnet.sonic.game",       // TestNet URL
            _ => "https://rpc.magicblock.app/devnet/"                         // MagicBlock DevNet
        };
        Web3.Instance.webSocketsRpc = value switch
        {
            (int)RpcCluster.MainNet => "wss://rpc.mainnet-alpha.sonic.game",  // Updated WebSocket
            (int)RpcCluster.TestNet => "wss://api.testnet.sonic.game",        // WebSocket URL
            _ => "wss://rpc.magicblock.app/devnet/"
        };

        PlayerPrefs.SetInt("rpcCluster", value);
        PlayerPrefs.Save();
        Web3.Instance.LoginXNFT().AsUniTask().Forget();
    }



}
