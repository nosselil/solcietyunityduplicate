using Fusion;
using UnityEngine;

public class PlayerAttributes : NetworkBehaviour
{
    [Networked]
    [Capacity(64)] public string LocalWalletAddress { get => default; set { } } // TODO: Change later on with the actual wallet address // TODO: Probably shouldn't be public


    
}
