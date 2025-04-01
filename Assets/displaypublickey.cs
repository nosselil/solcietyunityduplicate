using System;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Solana.Unity.Rpc;
using Solana.Unity.Wallet; 

public class DisplayPublicKey : MonoBehaviour
{
    private TextMeshProUGUI _txtPublicKey;

    void Start()
    {
        _txtPublicKey = GetComponent<TextMeshProUGUI>(); 
    }

  

    private void OnLogin(Account account)
    {
        _txtPublicKey.text = account.PublicKey.ToString(); 
    }
}