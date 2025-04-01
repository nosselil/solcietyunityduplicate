using UnityEngine;

namespace com.whatereyes.gametutorial
{
    public class SimpleCameraControllerCamera : MonoBehaviour
    {
        public Transform player;
        public float distance = 8f;
        public float height = 3f;
        public float rotationSpeed = 2f;
        public float zoomSpeed = 2f;
        public float minDistance = 5f;
        public float maxDistance = 10f;
        public float maxYAngle = 80f; // Ângulo máximo de inclinação da câmera
        public float minYAngle = -80f; // Ângulo mínimo de inclinação da câmera

        private float currentYAngle = 0f;

        void LateUpdate()
        {
            // Ajusta o zoom da câmera
            float scroll = Input.GetAxis("Mouse ScrollWheel") * zoomSpeed;
            distance = Mathf.Clamp(distance - scroll, minDistance, maxDistance);

            if (Input.GetMouseButton(1)) // Botão direito do mouse
            {
                // Obtém o movimento do mouse
                float mouseX = Input.GetAxis("Mouse X") * rotationSpeed;
                float mouseY = Input.GetAxis("Mouse Y") * rotationSpeed; // Movimento vertical do mouse (invertido)

                // Atualiza o ângulo Y da câmera (invertido)
                currentYAngle -= mouseY;
                currentYAngle = Mathf.Clamp(currentYAngle, minYAngle, maxYAngle);

                // Rotaciona a câmera em torno do jogador
                transform.RotateAround(player.position, Vector3.up, mouseX);
                transform.eulerAngles = new Vector3(currentYAngle, transform.eulerAngles.y, 0); // Limita a rotação vertical
            }

            // Atualiza a posição da câmera
            transform.position = player.position - transform.forward * distance + Vector3.up * height;
            transform.LookAt(player.position + Vector3.up * height);
        }
    }
}