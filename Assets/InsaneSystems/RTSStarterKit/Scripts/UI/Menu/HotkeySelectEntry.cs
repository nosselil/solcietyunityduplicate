using System.Text;
using InsaneSystems.RTSStarterKit.Controls;
using UnityEngine;
using UnityEngine.UI;

namespace InsaneSystems.RTSStarterKit
{
	public class HotkeySelectEntry : MonoBehaviour
	{
		[SerializeField] KeyActionType keyActionType;
		[SerializeField] Text keyEntryText;
		[SerializeField] Button keyChangeButton;

		KeyAction keyAction;
		
		HotkeysMenu hotkeysMenu;

		Text buttonText;
		
		bool isChangeActive;
		
		void Start()
		{
			hotkeysMenu = FindObjectOfType<HotkeysMenu>();
			buttonText = keyChangeButton.transform.Find("Text").GetComponent<Text>();

			Reload();
		}

		public void Reload()
		{
			keyAction = Keymap.loadedKeymap.GetAction(keyActionType);
			keyEntryText.text = InsertSpaceBeforeUpperCase(keyAction.type.ToString());
			buttonText.text = keyAction.key.ToString();
		}
		
		void Update()
		{
			if (!isChangeActive)
				return;
			
			if (Input.GetKeyDown(KeyCode.Escape))
			{
				SetKeyChangeState(false);
				return;
			}

			var pressedKey = GetPressedKey();

			if (pressedKey != null)
			{
				keyAction.key = (KeyCode)pressedKey;
				buttonText.text = keyAction.key.ToString();
				SetKeyChangeState(false);
				
				Keymap.Save();
			}
		}
		
		KeyCode? GetPressedKey()
		{
			foreach (KeyCode vKey in System.Enum.GetValues(typeof(KeyCode)))
				if (Input.GetKey(vKey))
					return vKey;

			return null;
		}

		public void SetKeyChangeState(bool isEnabled)
		{
			isChangeActive = isEnabled;
			hotkeysMenu.SetPressTextState(isEnabled);
		}
		
		public string InsertSpaceBeforeUpperCase(string str)
		{   
			var sb = new StringBuilder();

			char previousChar = char.MinValue; // Unicode '\0'

			foreach (char c in str)
			{
				if (char.IsUpper(c))
					if (sb.Length != 0 && previousChar != ' ')
						sb.Append(' ');

				sb.Append(c);
				previousChar = c;
			}

			return sb.ToString();
		}
	}
}