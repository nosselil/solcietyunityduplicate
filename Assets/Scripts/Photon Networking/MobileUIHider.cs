using TMPro;
using UnityEngine;

public class MobileUIHider : MonoBehaviour
{
    private void Start()
    {
        if (!WalletManager.instance.isMobile)
            gameObject.SetActive(false); // Deactivate this game object
    }
}

