using UnityEngine;

namespace InsaneSystems.RTSStarterKit.UI
{
	public class PauseMenu : MonoBehaviour
	{
		[SerializeField] GameObject selfObject;
		
		void Start()
		{
			Hide();
		}

		public void ShowOrHide()
		{
			if (selfObject.activeSelf)
				Hide();
			else
				Show();
		}
		
		public void Show()
		{
			selfObject.SetActive(true);
			GameController.SetPauseState(true);
		}
		
		public void Hide()
		{
			selfObject.SetActive(false);
			GameController.SetPauseState(false);
		}

		public void ExitToMenu()
		{
			GameController.instance.ReturnToLobby();
		}
	}
}