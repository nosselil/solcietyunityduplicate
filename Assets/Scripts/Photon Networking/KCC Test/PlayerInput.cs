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
        public float LookRotationMultiplier = 3.0f;

		/*[HideInInspector]*/ public Joystick joystick; // Reference to the mobile joystick
		private Vector2 mobileLookDirection;
		private Vector2 mobileMoveDirection;
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
			_input.LookRotation = new Vector2(InitialLookRotation, 180f); // 0
		}

		private void Update()
		{
			//Debug.Log("KCC: PlayerInput, cursor lockstate " + Cursor.lockState);

			if (LocalChatWindowController.Instance.IsChatWindowActive) // don't allow input gathering if chat is open
				return;

			// Accumulate input only if the cursor is locked.
			//if (Cursor.lockState != CursorLockMode.Locked)
			//	return;

			// Accumulate input from Keyboard/Mouse. Input accumulation is mandatory (at least for look rotation here) as Update can be
			// called multiple times before next FixedUpdateNetwork is called - common if rendering speed is faster than Fusion simulation.

			var lookRotationDelta = new Vector2(-Input.GetAxisRaw("Mouse Y"), Input.GetAxisRaw("Mouse X")) * LookRotationMultiplier;
			_input.LookRotation = ClampLookRotation(_input.LookRotation + lookRotationDelta);
			
			var moveDirection = !WalletManager.instance.isMobile ? new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")) : 
				new Vector2(joystick.GetHorizontal(), joystick.GetVertical());

			Debug.Log("MOBILE: Joystick: hor " + joystick.GetHorizontal() + " ver: " + joystick.GetVertical());

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
		public void SetMobileUIJumpPressed()
		{
			mobileJumpPressed = true;
		}
        #endregion
    }
}
