using UnityEngine;
using LeTai.Asset.TranslucentImage;

public class TranslucentImageSourceAssigner : MonoBehaviour
{
    [Header("Settings")]
    public float checkInterval = 0.5f; // How often to check for player camera
    public float maxWaitTime = 10f; // Maximum time to wait for player camera
    
    private float lastCheckTime;
    private float startTime;
    private TranslucentImageSource playerCameraSource;
    private TranslucentImage[] translucentImages;

    void Start()
    {
        startTime = Time.time;
        translucentImages = FindObjectsOfType<TranslucentImage>();
        Debug.Log($"Found {translucentImages.Length} TranslucentImage components");
    }

    void Update()
    {
        // Stop checking after max wait time
        if (Time.time - startTime > maxWaitTime)
        {
            enabled = false;
            return;
        }

        // Check periodically
        if (Time.time - lastCheckTime > checkInterval)
        {
            lastCheckTime = Time.time;
            TryAssignPlayerCameraSource();
        }
    }

    void TryAssignPlayerCameraSource()
    {
        // Try to find player camera with TranslucentImageSource
        if (playerCameraSource == null)
        {
            // Look for camera with "Player" tag or "MainCamera" tag
            Camera[] cameras = FindObjectsOfType<Camera>();
            
            foreach (Camera cam in cameras)
            {
                // Check if this is likely the player camera
                if (cam.CompareTag("Player") || cam.CompareTag("MainCamera") || 
                    cam.name.ToLower().Contains("player") || cam.name.ToLower().Contains("main"))
                {
                    TranslucentImageSource source = cam.GetComponent<TranslucentImageSource>();
                    if (source != null)
                    {
                        playerCameraSource = source;
                        Debug.Log($"Found player camera source: {cam.name}");
                        break;
                    }
                }
            }
        }

        // If we found the player camera source, assign it to all TranslucentImages
        if (playerCameraSource != null)
        {
            bool anyAssigned = false;
            
            foreach (TranslucentImage img in translucentImages)
            {
                if (img != null && img.source != playerCameraSource)
                {
                    img.source = playerCameraSource;
                    anyAssigned = true;
                    Debug.Log($"Assigned player camera source to: {img.name}");
                }
            }

            if (anyAssigned)
            {
                Debug.Log("Successfully assigned player camera source to all TranslucentImage components");
                enabled = false; // Stop checking once assigned
            }
        }
    }

    // Public method to manually trigger assignment (useful for testing)
    [ContextMenu("Force Assign Player Camera Source")]
    public void ForceAssignPlayerCameraSource()
    {
        playerCameraSource = null; // Reset to force re-finding
        TryAssignPlayerCameraSource();
    }
} 