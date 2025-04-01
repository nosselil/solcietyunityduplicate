using UnityEngine;
using LeTai.Asset.TranslucentImage; // Add this line to reference the namespace

[RequireComponent(typeof(TranslucentImage))]
public class TranslucentImageSourceSetter : MonoBehaviour
{
    // Public variable to select the camera in the Inspector
    public Camera targetCamera;

    // Reference to the TranslucentImage component
    private TranslucentImage translucentImage;

    private void Start()
    {
        // Get the TranslucentImage component attached to the same GameObject
        translucentImage = GetComponent<TranslucentImage>();

        if (translucentImage == null)
        {
            Debug.LogError("TranslucentImage component not found on the GameObject.");
            return;
        }

        if (targetCamera == null)
        {
            Debug.LogError("Target Camera is not assigned.");
            return;
        }

        // Get the TranslucentImageSource component from the target camera
        TranslucentImageSource source = targetCamera.GetComponent<TranslucentImageSource>();

        if (source == null)
        {
            Debug.LogError("TranslucentImageSource component not found on the target camera.");
            return;
        }

        // Assign the source to the TranslucentImage component
        translucentImage.source = source;
    }
}