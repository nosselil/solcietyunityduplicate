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
        public float maxYAngle = 80f; // �ngulo m�ximo de inclina��o da c�mera
        public float minYAngle = -80f; // �ngulo m�nimo de inclina��o da c�mera

        private float currentYAngle = 0f;

        void LateUpdate()
        {
            // Ajusta o zoom da c�mera
            float scroll = Input.GetAxis("Mouse ScrollWheel") * zoomSpeed;
            distance = Mathf.Clamp(distance - scroll, minDistance, maxDistance);

            if (Input.GetMouseButton(1)) // Bot�o direito do mouse
            {
                // Obt�m o movimento do mouse
                float mouseX = Input.GetAxis("Mouse X") * rotationSpeed;
                float mouseY = Input.GetAxis("Mouse Y") * rotationSpeed; // Movimento vertical do mouse (invertido)

                // Atualiza o �ngulo Y da c�mera (invertido)
                currentYAngle -= mouseY;
                currentYAngle = Mathf.Clamp(currentYAngle, minYAngle, maxYAngle);

                // Rotaciona a c�mera em torno do jogador
                transform.RotateAround(player.position, Vector3.up, mouseX);
                transform.eulerAngles = new Vector3(currentYAngle, transform.eulerAngles.y, 0); // Limita a rota��o vertical
            }

            // Atualiza a posi��o da c�mera
            transform.position = player.position - transform.forward * distance + Vector3.up * height;
            transform.LookAt(player.position + Vector3.up * height);
        }
    }
}