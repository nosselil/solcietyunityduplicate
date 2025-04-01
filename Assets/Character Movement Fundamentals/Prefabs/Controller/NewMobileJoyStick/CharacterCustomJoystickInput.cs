using UnityEngine;

namespace CMF
{
    public class CharacterCustomJoystickInput : CharacterInput
    {
        public Joystick joystick; // Reference to the custom joystick
        public UnityEngine.UI.Button jumpButton; // Jump button reference

     

        private bool isJumpPressed = false;

        public override float GetHorizontalMovementInput()
        {
            return joystick.GetHorizontal();
        }

        public override float GetVerticalMovementInput()
        {
            return joystick.GetVertical();
        }

        public override bool IsJumpKeyPressed()
        {
            if (isJumpPressed)
            {
                isJumpPressed = false;
                return true;
            }

            return false;
        }

        public void SetJumpButtonPressed()
        {
            isJumpPressed = true;
        }

        public void SetJumpButtonReleased()
        {
            isJumpPressed = false;
        }

    

        private void Start()
        {
            if (jumpButton != null)
            {
                jumpButton.onClick.AddListener(SetJumpButtonPressed);
            }
        }
    }
}
