using UnityEngine;

namespace Starter.Platformer
{
	/// <summary>
	/// Structure holding player input.
	/// </summary>
	public struct GameplayInput
	{		
		public Vector2 LookRotation;
		public Vector2 MoveDirection;
		public bool Jump;
		public bool Sprint;
	}

	/// <summary>
	/// PlayerInput handles accumulating player input from Unity.
	/// </summary>
	public sealed class PlayerInput : MonoBehaviour
	{
		public float InitialLookRotation = 18f;
		public float InitialLookRotationY = 180f; // 270 in main hub
        public float LookRotationMultiplier = 3.0f;

        // Sensitivity for mobile touch-look rotation.
        public float mobileLookSensitivity = 0.08f;

        /*[HideInInspector]*/
        public Joystick joystick; // Reference to the mobile joystick
		//private Vector2 mobileLookDirection;
		//private Vector2 mobileMoveDirection;
		private bool mobileJumpPressed;

		//[SerializeField] float maxCameraYaw, minCameraYaw

        public GameplayInput CurrentInput => _input;
		private GameplayInput _input;

		public void ResetInput()
		{
			// Reset input after it was used to detect changes correctly again
			_input.MoveDirection = default;
			_input.Jump = false;
			_input.Sprint = false;
		}		

		private void Start()
		{
			// Set initial camera rotation
			_input.LookRotation = new Vector2(InitialLookRotation, InitialLookRotationY); // 0
			//Debug.Log("PLAYER INPUT: ")
		}

		private void Update()
		{
			Debug.Log("PLAYER INPUT: wallet manager is mobile: " + WalletManager.instance.isMobile);

			//Debug.Log("KCC: PlayerInput, cursor lockstate " + Cursor.lockState);

			if (LocalChatWindowController.Instance.IsChatWindowActive) // don't allow input gathering if chat is open
				return;

            // Accumulate input only if the cursor is locked.
            //if (Cursor.lockState != CursorLockMode.Locked)
            //	return;

            // Accumulate input from Keyboard/Mouse. Input accumulation is mandatory (at least for look rotation here) as Update can be
            // called multiple times before next FixedUpdateNetwork is called - common if rendering speed is faster than Fusion simulation.

            Vector2 lookRotationDelta = !WalletManager.instance.isMobile ? new Vector2(-Input.GetAxisRaw("Mouse Y"), Input.GetAxisRaw("Mouse X")) * LookRotationMultiplier
				: GetMobileLookRotationDelta();

            _input.LookRotation = ClampLookRotation(_input.LookRotation + lookRotationDelta);
			
			var moveDirection = !WalletManager.instance.isMobile ? new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")) : 
				new Vector2(joystick.GetHorizontal(), joystick.GetVertical());

			//Debug.Log("MOBILE: Joystick: hor " + joystick.GetHorizontal() + " ver: " + joystick.GetVertical());

			_input.MoveDirection =  moveDirection.normalized;

			_input.Jump |= !WalletManager.instance.isMobile ? Input.GetButtonDown("Jump") : mobileJumpPressed;
			//_input.Sprint |= Input.GetButton("Sprint");

			if (WalletManager.instance.isMobile)
				mobileJumpPressed = false; // Mark this as consumed

			//Debug.Log("KCC: Move direction: " + _input.MoveDirection + ", jump: " + _input.Jump);
		}

		private Vector2 ClampLookRotation(Vector2 lookRotation)
		{
			lookRotation.x = Mathf.Clamp(lookRotation.x, -25f, 60f); //-30f, 70f);
			return lookRotation;
		}

        #region Mobile Input Detection

        private Vector2 GetMobileLookRotationDelta()
        {
            if (Input.touchCount > 0)
            {
                // Use the deltaPosition of the first touch.
                Touch touch = Input.GetTouch(0);
                return new Vector2(-touch.deltaPosition.y, touch.deltaPosition.x) * mobileLookSensitivity;
            }
            return Vector2.zero;
        }

        public void SetMobileUIJumpPressed()
		{
			mobileJumpPressed = true;
		}
        #endregion
    }
}
