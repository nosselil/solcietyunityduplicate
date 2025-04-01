using InsaneSystems.RTSStarterKit.Controls;
using UnityEngine;
using UnityEngine.UI;

namespace InsaneSystems.RTSStarterKit.Scripts.UI
{
	public class ControlsButtons : MonoBehaviour
	{
		[SerializeField] Button repairButton, sellButton;
		
		void Start()
		{
			if (repairButton)
				repairButton.onClick.AddListener(delegate
				{
					InputHandler.sceneInstance.SetCustomControls(CustomControls.Repair); 
				});
			
			if (sellButton)
				sellButton.onClick.AddListener(delegate
				{
					InputHandler.sceneInstance.SetCustomControls(CustomControls.Sell); 
				});
		}
	}
}