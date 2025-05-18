using Solana.Unity.SDK;
using TMPro;
using UnityEngine;

public class WalletAddressUpdater : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        GetComponent<TMP_InputField>().text = Web3.Instance.WalletBase.Account.PublicKey;
    }
}
