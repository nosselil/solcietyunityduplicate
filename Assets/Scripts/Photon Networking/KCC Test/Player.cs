using UnityEngine;
using Fusion;
using Fusion.Addons.SimpleKCC;
using UnityEngine.InputSystem;
using CMF;
using UnityEngine.SceneManagement;

namespace Starter.Platformer
{
    /// <summary>
    /// Main player scrip - controls player movement and animations.
    /// </summary>
    public sealed class Player : NetworkBehaviour
    {
        //public Transform cameraTransform;        
        [SerializeField] AnimationControl animationControl;        

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


        //[Networked, HideInInspector]
        //public int CollectedCoins { get; set; }

        [Networked]
        private NetworkBool _isJumping { get; set; }

        // Animation IDs
        private int _animIDSpeed;
        private int _animIDGrounded;

        private Vector3 _moveVelocity;


        private bool previouslyGrounded = false; // Used to keep track of when we land

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.LeftControl))                                                            
                SceneManager.LoadScene("mainGallery");

            if (Input.GetKeyDown(KeyCode.Q))
                SceneManager.LoadScene("mainStartingarea");
        }

        public void Respawn(Vector3 position, bool resetCoins)
        {
            KCC.SetPosition(position);
            KCC.SetLookRotation(0f, 0f);

            _moveVelocity = Vector3.zero;

            /*if (resetCoins)
            {
                CollectedCoins = 0;
            }*/
        }

        public override void Spawned()
        {
            if (HasStateAuthority)
            {                

                // Set player nickname that is saved in UIGameMenu
               // Nickname = PlayerPrefs.GetString("PlayerName");
            }

            // In case the nickname is already changed,
            // we need to trigger the change manually
            //OnNicknameChanged();
        }

        public override void FixedUpdateNetwork()
        {
            //if (LocalChatWindowController.Instance.IsChatWindowActive)
            //    return;

            ProcessInput(PlayerInput.CurrentInput);
            
            if (!previouslyGrounded && KCC.IsGrounded)
            {
                Debug.Log("LAND: Player just landed with velocity " + KCC.RealVelocity);
                animationControl.OnLand(KCC.RealVelocity);
            }

            if (KCC.IsGrounded)
            {
                //Debug.Log("KCC: Player.cs Stop jumping");
                
                // Stop jumping
                _isJumping = false;
            }

            PlayerInput.ResetInput();

            previouslyGrounded = KCC.IsGrounded;
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
            AssignAnimationIDs();            
        }

        private void LateUpdate()
        {
            // Only local player needs to update the camera
            if (HasStateAuthority == false)
                return;

            // Update camera pivot and transfer properties from camera handle to Main Camera.
            CameraPivot.rotation = Quaternion.Euler(PlayerInput.CurrentInput.LookRotation);
            Camera.main.transform.SetPositionAndRotation(CameraHandle.position, CameraHandle.rotation);
        }

        private void ProcessInput(GameplayInput input)
        {
            float jumpImpulse = 0f;

            if (KCC.IsGrounded && input.Jump)
            {
                // Set world space jump vector.
                jumpImpulse = JumpImpulse;
                _isJumping = true;
            }

            // Adjust gravity based on whether the player is rising or falling.
            KCC.SetGravity(KCC.RealVelocity.y >= 0f ? UpGravity : DownGravity);

            float speed = input.Sprint ? SprintSpeed : WalkSpeed;

            var lookRotation = Quaternion.Euler(0f, input.LookRotation.y, 0f);

            // Calculate movement direction using the camera's forward and right vectors.
            // Here, input.MoveDirection.y maps to forward/backward and input.MoveDirection.x maps to right/left.
            var moveDirection = lookRotation * new Vector3(input.MoveDirection.x, 0f, input.MoveDirection.y);            
            Vector3 desiredMoveVelocity = moveDirection * speed;

            float acceleration;
            if (desiredMoveVelocity == Vector3.zero)
            {
                // If there's no input, use deceleration.
                acceleration = KCC.IsGrounded ? GroundDeceleration : AirDeceleration;
            }
            else
            {
                // Rotate the character smoothly towards the move direction.
                Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
                Quaternion nextRotation = Quaternion.Lerp(KCC.TransformRotation, targetRotation, RotationSpeed * Runner.DeltaTime);
                KCC.SetLookRotation(nextRotation.eulerAngles);

                acceleration = KCC.IsGrounded ? GroundAcceleration : AirAcceleration;
            }

            // Smoothly interpolate to the desired velocity.
            _moveVelocity = Vector3.Lerp(_moveVelocity, desiredMoveVelocity, acceleration * Runner.DeltaTime);

            // Move the character controller with the calculated velocity and any jump impulse.
            KCC.Move(_moveVelocity, jumpImpulse);
        }


        private void AssignAnimationIDs()
        {
            _animIDSpeed = Animator.StringToHash("Speed");
            _animIDGrounded = Animator.StringToHash("Grounded");
        }


    }
}
