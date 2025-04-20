using Fusion;
using Starter;
using UnityEngine;

public class PlayerAttributes : NetworkBehaviour
{
    [Networked, Capacity(64)] public string LocalWalletAddress { get => default; set { } } // TODO: Change later on with the actual wallet address // TODO: Probably shouldn't be public

    [Networked, HideInInspector, Capacity(32), OnChangedRender(nameof(OnNicknameChanged))] public string Nickname { get; set; } = "";
    [Networked, OnChangedRender(nameof(OnCapColorIndexChanged))] public int CapColorIndex { get => default; set { } } // Represents the cap color index for each player, ranges from 0 to 32 or so. If >= 32 (or other limit), use modulo to loop the index.

    public UINameplate nameplate;
    
    [SerializeField] private Material capMaterial;
    [SerializeField] private Texture2D[] capMaterialTextures;
    [SerializeField] private Renderer capRenderer;

    private const string PLAYER_PREFS_CAP_COLOR_INDEX_KEY = "capColor";

    public override void Spawned()
    {
        //Debug.Log("NICK: PlayerAttributes.cs");

        Invoke("SetNickname", 0.1f); // Wait for the player spawner initialization to finish, then assign the nick name to the newly spawned player. Since Nickname is a networked property, all players will
        // see the nickname               
        OnCapColorIndexChanged();
    }

    private void SetNickname()
    {
        Debug.Log("NICK: Set nickname for " + LocalWalletAddress);
        Nickname = WalletUtilities.ShortenWalletAddress(LocalWalletAddress);
        OnNicknameChanged(); // Call this manually to make sure that all other players update their nickname as well
        // TODO: Logic could probably be cleaned up a little bit
    }

    /*private void OnLocalWalletAddressChanged()
        
    {

    }*/


    private void OnNicknameChanged()
    {
        //Debug.Log("NICK: OnNicknameChanged");
        nameplate.SetNickname(Nickname);
    }

    public void SetCapColorIndex()//int capColorIndex)
    {
        //Debug.Log("CAP: Set cap color index");
        CapColorIndex = PlayerPrefs.GetInt(PLAYER_PREFS_CAP_COLOR_INDEX_KEY, -1);

        if (CapColorIndex == -1) // Has not been set yet
        {
            Random.InitState((int)System.DateTime.Now.Ticks); // Seed the RNG for better randomness
            CapColorIndex = Random.Range(0, capMaterialTextures.Length);
            PlayerPrefs.SetInt(PLAYER_PREFS_CAP_COLOR_INDEX_KEY, CapColorIndex);
        }

        OnCapColorIndexChanged(); // Manually triggering here as well to ensure all clients execute this
    }

    private void OnCapColorIndexChanged()
    {
        //Debug.Log("CAP: OnCapColorIndexChanged triggered, CapColorIndex = " + CapColorIndex);

        // Calculate a valid index using modulo to ensure we don't go out-of-bounds.
        int validIndex = Mathf.Abs(CapColorIndex) % capMaterialTextures.Length;
        //Debug.Log("CAP: Valid texture index = " + validIndex);

        // Instantiate a new instance of the base cap material (so that changes only affect this instance).
        Material newCapMaterial = Instantiate(capMaterial);
        newCapMaterial.mainTexture = capMaterialTextures[validIndex];

        capRenderer.material = newCapMaterial;
        //Debug.Log("CAP: Assigned new cap material with texture index " + validIndex);

    }
}
