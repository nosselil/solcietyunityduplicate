using UnityEngine;
using Fusion;
using Fusion.Addons.SimpleKCC;
using UnityEngine.InputSystem;

namespace Starter.Platformer
{
    /// <summary>
    /// Main player scrip - controls player movement and animations.
    /// </summary>
    public sealed class Player : NetworkBehaviour
    {
        public Transform cameraTransform;

        [Header("References")]
        public SimpleKCC KCC;
        public PlayerInput PlayerInput;
        public Animator Animator;
        public Transform CameraPivot;
        public Transform CameraHandle;
        public Transform ScalingRoot;        

        [Header("Movement Setup")]
        public float WalkSpeed = 2f;
        [HideInInspector] public float SprintSpeed = 5f;
        public float JumpImpulse = 10f;
        public float UpGravity = 25f;
        public float DownGravity = 40f;
        public float RotationSpeed = 8f;

        [Header("Movement Accelerations")]
        public float GroundAcceleration = 55f;
        public float GroundDeceleration = 25f;
        public float AirAcceleration = 25f;
        public float AirDeceleration = 1.3f;

        [Header("Sounds")]
        public AudioSource FootstepSound;
        public AudioClip JumpAudioClip;
        public AudioClip LandAudioClip;
        public AudioClip CoinCollectedAudioClip;

        [Header("VFX")]
        public ParticleSystem DustParticles;

        [Networked, HideInInspector, Capacity(24)] //, OnChangedRender(nameof(OnNicknameChanged))]
        public string Nickname { get; set; }
        [Networked, HideInInspector]
        public int CollectedCoins { get; set; }

        [Networked]
        private NetworkBool _isJumping { get; set; }

        // Animation IDs
        private int _animIDSpeed;
        private int _animIDGrounded;

        private Vector3 _moveVelocity;
        

        public void Respawn(Vector3 position, bool resetCoins)
        {
            KCC.SetPosition(position);
            KCC.SetLookRotation(0f, 0f);

            _moveVelocity = Vector3.zero;

            if (resetCoins)
            {
                CollectedCoins = 0;
            }
        }

        public override void Spawned()
        {
            if (HasStateAuthority)
            {                

                // Set player nickname that is saved in UIGameMenu
                Nickname = PlayerPrefs.GetString("PlayerName");
            }

            // In case the nickname is already changed,
            // we need to trigger the change manually
            //OnNicknameChanged();
        }

        public override void FixedUpdateNetwork()
        {
            Debug.Log("KCC: PLAYER.CS Fixed Update Network, process player input");

            ProcessInput(PlayerInput.CurrentInput);

            if (KCC.IsGrounded)
            {
                // Stop jumping
                _isJumping = false;
            }

            PlayerInput.ResetInput();
        }

        public override void Render()
        {
            //Animator.SetFloat(_animIDSpeed, KCC.RealSpeed);
            //Animator.SetBool(_animIDGrounded, KCC.IsGrounded);

            //FootstepSound.enabled = KCC.IsGrounded && KCC.RealSpeed > 1f;
            //FootstepSound.pitch = KCC.RealSpeed > SprintSpeed - 1 ? 1.5f : 1f;

            //ScalingRoot.localScale = Vector3.Lerp(ScalingRoot.localScale, Vector3.one, Time.deltaTime * 8f);

            //var emission = DustParticles.emission;
            //emission.enabled = KCC.IsGrounded && KCC.RealSpeed > 1f;
        }

        private void Awake()
        {
            //AssignAnimationIDs();
        }

        private void LateUpdate()
        {
            // Only local player needs to update the camera
            if (HasStateAuthority == false)
                return;



            // Update camera pivot and transfer properties from camera handle to Main Camera.
            //CameraPivot.rotation = Quaternion.Euler(PlayerInput.CurrentInput.LookRotation);
            //Camera.main.transform.SetPositionAndRotation(CameraHandle.position, CameraHandle.rotation);
        }

        private void ProcessInput(GameplayInput input)
        {
            float jumpImpulse = 0f;

            if (KCC.IsGrounded && input.Jump)
            {
                // Set world space jump vector
                jumpImpulse = JumpImpulse;
                _isJumping = true;
            }

            // It feels better when the player falls quicker
            KCC.SetGravity(KCC.RealVelocity.y >= 0f ? UpGravity : DownGravity);

            float speed = input.Sprint ? SprintSpeed : WalkSpeed;

            //Debug.Log("CAMERA MAIN: " + Camera.main.name);
            var lookRotation = Quaternion.Euler(0f, cameraTransform.eulerAngles.y, 0f); //Quaternion.Euler(0f, input.LookRotation.y, 0f);

            // Calculate correct move direction from input (rotated based on camera look)
            var moveDirection = lookRotation * new Vector3(input.MoveDirection.x, 0f, input.MoveDirection.y);
            var desiredMoveVelocity = moveDirection * speed;

            float acceleration;
            if (desiredMoveVelocity == Vector3.zero)
            {
                // No desired move velocity - we are stopping
                acceleration = KCC.IsGrounded ? GroundDeceleration : AirDeceleration;
            }
            else
            {
                // Rotate the character towards move direction over time
                var currentRotation = KCC.TransformRotation;
                var targetRotation = Quaternion.LookRotation(moveDirection);
                var nextRotation = Quaternion.Lerp(currentRotation, targetRotation, RotationSpeed * Runner.DeltaTime);

                KCC.SetLookRotation(nextRotation.eulerAngles);

                acceleration = KCC.IsGrounded ? GroundAcceleration : AirAcceleration;
            }

            _moveVelocity = Vector3.Lerp(_moveVelocity, desiredMoveVelocity, acceleration * Runner.DeltaTime);

            KCC.Move(_moveVelocity, jumpImpulse);

        }

        /*private void AssignAnimationIDs()
        {
            _animIDSpeed = Animator.StringToHash("Speed");
            _animIDGrounded = Animator.StringToHash("Grounded");
        }*/

        
    }
}
