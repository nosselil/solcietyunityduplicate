using UnityEngine;

namespace com.whatereyes.gametutorial
{
    [RequireComponent(typeof(CharacterController))]
    public class SimpleCharacterController : MonoBehaviour
    {
        public float moveSpeed = 5f;
        public float jumpSpeed = 8f;
        public float gravity = 20f;
        public Transform cameraTransform;

        private CharacterController controller;
        private Vector3 moveDirection = Vector3.zero;

        void Start()
        {
            controller = GetComponent<CharacterController>();
        }

        void Update()
        {
            // Movimento do personagem
            if (controller.isGrounded)
            {
                float moveDirectionY = moveDirection.y;
                float moveDirectionX = Input.GetAxis("Horizontal") * moveSpeed;
                float moveDirectionZ = Input.GetAxis("Vertical") * moveSpeed;

                moveDirection = new Vector3(moveDirectionX, moveDirectionY, moveDirectionZ);
                moveDirection = cameraTransform.TransformDirection(moveDirection);
                moveDirection.y = 0;

                if (Input.GetButton("Jump"))
                {
                    moveDirection.y = jumpSpeed;
                }
            }

            // Aplica a gravidade
            moveDirection.y -= gravity * Time.deltaTime;

            // Move o personagem
            controller.Move(moveDirection * Time.deltaTime);
        }
    }
}