using InsaneSystems.RTSStarterKit.Controls;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace InsaneSystems.RTSStarterKit
{
	public class HotkeysMenu : MonoBehaviour
	{
		[SerializeField] GameObject pressAnyKeyText;
		
		HotkeySelectEntry[] hotkeyEntries;

		void Start()
		{
			pressAnyKeyText.SetActive(false);
			hotkeyEntries = FindObjectsOfType<HotkeySelectEntry>();
		}

		public void SetPressTextState(bool isEnabled)
		{
			pressAnyKeyText.SetActive(isEnabled);
		}

		public void BackToLobby()
		{
			SceneManager.LoadScene("Lobby");
		}

		public void ResetToDefault()
		{
			Keymap.loadedKeymap.SetupDefaultScheme();

			for (var i = 0; i < hotkeyEntries.Length; i++)
				hotkeyEntries[i].Reload();
		}
	}
}