using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
// Add this with other using statements:
using UnityEngine;
using PixelCrushers.DialogueSystem.UnityGUI;
using System.Collections.Generic;
using UnityEngine.Serialization;
//using global::WalletManager; // Add this line


namespace CMF
{
    //This character movement input class is an example of how to get input from a keyboard to control the character;
    public class CharacterKeyboardInput : CharacterInput
    {

        public Joystick joystick; // Reference to the custom joystick
        public Button jumpButton, interactBtn; // Jump button reference
        private bool isJumpPressed = false;
        public GameObject mobileUI;

        public string horizontalInputAxis = "Horizontal";
        public string verticalInputAxis = "Vertical";
        public KeyCode jumpKey = KeyCode.Space;

        //If this is enabled, Unity's internal input smoothing is bypassed;
        public bool useRawInput = true;
        bool isMobile = false;

        public ThirdPersonCameraController thirdPersonCameraController;

        private void Awake()
        {
            isMobile = WalletManager.instance.isMobile;

            if (isMobile)
            {
           //     thirdPersonCameraController.enabled = false;
                mobileUI.SetActive(true);
            }
            else
            {
                mobileUI.SetActive(false);
            }


        }

        public override float GetHorizontalMovementInput()
        {
            if (isMobile)
            {
                return joystick.GetHorizontal();
            }
            else
            {



                if (useRawInput)
                    return Input.GetAxisRaw(horizontalInputAxis);
                else
                    return Input.GetAxis(horizontalInputAxis);
            }
        }
        public override float GetVerticalMovementInput()
        {
            if (isMobile)
            {
                return joystick.GetVertical();
            }
            else
            {




                if (useRawInput)
                    return Input.GetAxisRaw(verticalInputAxis);
                else
                    return Input.GetAxis(verticalInputAxis);
            }
        }


        public void JumpBtn()
        {
            isJumpPressed = true;
        }
        public override bool IsJumpKeyPressed()
        {


            if (isMobile)
            {
                if (isJumpPressed)
                {
                    isJumpPressed = false;
                    return true;
                }

                return false;
            }
            else
            {


                return Input.GetKey(jumpKey);
            }
        }



        #region MobileUI
        public void makeBtn_Interactable()
        {
            interactBtn.interactable = true;
        }
        public void makeBtn_NONInteractable()
        {
            interactBtn.interactable = false;
        }


        #endregion
    }
}
