using UnityEngine;
using Solana.Unity.SDK.Example;
using UnityEngine.UI;
public class Object : MonoBehaviour
{

  //  public MeshRenderer NftHolder;
    public TokenItem tokenItem;
    //   public ScrollRect scrollrect;
    //   public GameObject content;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void ApplyTexture()
    {
   //     NftHolder = NftManager.instance.NftHolder;
        tokenItem.ApplyRawImageTextureToMeshRenderer(NftManager.instance.BoxArtworkWorkMesh);
    }

    /*
    public void OndragBegin()
    {
        if (scrollrect != null)
        {
            scrollrect.enabled = false;
            gameObject.transform.parent = null;
        }
    }

    public void OnDropFailed()
    {
        if (scrollrect != null)
        {
            scrollrect.enabled = true;
            gameObject.transform.parent = content.gameObject.transform;
        }

    }*/
}
