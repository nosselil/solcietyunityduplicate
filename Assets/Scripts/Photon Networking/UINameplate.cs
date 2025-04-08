using TMPro;
using UnityEngine;

namespace Starter
{
    /// <summary>
    /// Component that handles showing nicknames above player
    /// </summary>
    public class UINameplate : MonoBehaviour
    {
        public TextMeshProUGUI nicknameText;

        private Transform cameraTransform;

        public void SetNickname(string nickname)
        {
            nicknameText.text = nickname;
        }

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
}
