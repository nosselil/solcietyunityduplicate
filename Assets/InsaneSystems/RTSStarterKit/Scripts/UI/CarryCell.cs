using UnityEngine;
using UnityEngine.UI;

namespace InsaneSystems.RTSStarterKit.UI
{
	public class CarryCell : MonoBehaviour
	{
		[SerializeField] Image iconImage;

		public void SetActive(bool enabledValue)
		{
			gameObject.SetActive(enabledValue);
		}
		
		public void UpdateState(Unit unitIn)
		{
			if (!unitIn)
			{
				iconImage.enabled = false;
				return;
			}

			iconImage.enabled = true;
			iconImage.sprite = unitIn.data.icon;
		}
	}
}