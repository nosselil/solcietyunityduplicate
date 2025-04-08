using Fusion;
using UnityEngine;

public class PlayerAttributes : NetworkBehaviour
{
    [Networked]
    [Capacity(64)] public string LocalWalletAddress { get => default; set { } } // TODO: Change later on with the actual wallet address // TODO: Probably shouldn't be public

    [Networked]
    [Capacity(64)] public int CapColorIndex { get => default; set { } } // Represents the cap color index for each player, ranges from 0 to 32 or so. If >= 32 (or other limit), use modulo to loop the index.

    public static int CAP_INDEX_COUNT = 32; // Used to determine when we need to loop and start using previous colors

}
