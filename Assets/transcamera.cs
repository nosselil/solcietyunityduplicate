using UnityEngine;
using UnityEngine.UI;

public class TopbarScript : MonoBehaviour
{
    public GameObject cameraObject;

    void Start()
    {
        // Find the TranslucentImage GameObject (assuming it's a child of Topbar)
        GameObject translucentImageObject = transform.Find("translucent Image").gameObject; 

        if (translucentImageObject != null && cameraObject != null)
        {
            // Get the Image component from the TranslucentImage GameObject
            Image translucentImage = translucentImageObject.GetComponent<Image>(); 

            // Create a temporary RenderTexture to hold the result
            RenderTexture tempRT = RenderTexture.GetTemporary(cameraObject.GetComponent<Camera>().targetTexture.width, cameraObject.GetComponent<Camera>().targetTexture.height); 

            // Copy the camera's targetTexture to the temporary RenderTexture
            Graphics.Blit(cameraObject.GetComponent<Camera>().targetTexture, tempRT); 

            // Create a Texture2D from the temporary RenderTexture
            Texture2D tempTex = new Texture2D(tempRT.width, tempRT.height, TextureFormat.ARGB32, false);
            RenderTexture.active = tempRT;
            tempTex.ReadPixels(new Rect(0, 0, tempRT.width, tempRT.height), 0, 0);
            tempTex.Apply();
            RenderTexture.active = null;
            RenderTexture.ReleaseTemporary(tempRT);

            // Set the source of the TranslucentImage to the Texture2D
            translucentImage.sprite = Sprite.Create(tempTex, new Rect(0, 0, tempTex.width, tempTex.height), Vector2.zero); 
        }
        else
        {
            Debug.LogError("Either the TranslucentImage GameObject or cameraObject is missing.");
        }
    }
}