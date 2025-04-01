using UnityEngine;
using UnityEngine.UI;

namespace InsaneSystems.RTSStarterKit.UI
{
	/// <summary> Handles icon of unit in Carry module UI panel. </summary>
	public class CarryingUnitIcon : MonoBehaviour
	{
		[SerializeField] Image iconImage;

		Unit selfUnit, carryingUnit;

		CarryingUnitList carryList;

		public void Setup(Unit unit, Unit carryingUnit, CarryingUnitList carryList)
		{
			selfUnit = unit;
			iconImage.sprite = unit.data.icon;

			selfUnit = unit;
			this.carryingUnit = carryingUnit;
			this.carryList = carryList;
		}

		public void OnClick()
		{
			carryingUnit.GetModule<CarryModule>().ExitUnit(selfUnit);

			carryList.MoveIconToPool(gameObject);
		}
	}
}