using UnityEngine;

public class GatedPortalMessage : MonoBehaviour
{
    Transform cameraTransform;

    private void Awake()
    {
        cameraTransform = Camera.main.transform;
        //nicknameText.text = string.Empty;
    }

    private void LateUpdate()
    {
        // Rotate nameplate toward camera
        transform.rotation = cameraTransform.rotation;
    }
}
