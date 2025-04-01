using InsaneSystems.RTSStarterKit.Controls;
using UnityEngine;
using UnityEngine.UI;

namespace InsaneSystems.RTSStarterKit.UI
{
	public class MultiselectionUnitIcon : MonoBehaviour
	{
		[SerializeField] Image iconImage;
		[SerializeField] Image healthbarImage;

		Unit selfUnit;

		public void SetupWithUnit(Unit unit)
		{
			selfUnit = unit;
			iconImage.sprite = unit.data.icon;

			UpdateHealthBar();
		}

		public void UpdateHealthBar()
		{
			if (selfUnit == null)
			{
				Destroy(gameObject);
				return;
			}

			healthbarImage.fillAmount = selfUnit.damageable.GetHealthPercents();
		}
		
		public void OnClick()
		{
			Controls.Selection.OnClearSelection();
			Controls.Selection.OnUnitAddToSelection(selfUnit);

			var cameraMover = CameraMover.sceneInstance;
			
			if (cameraMover)
				cameraMover.SetPosition(selfUnit.transform.position);
		}
	}
}