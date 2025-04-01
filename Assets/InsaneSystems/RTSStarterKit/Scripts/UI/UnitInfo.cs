using UnityEngine;
using UnityEngine.UI;

namespace InsaneSystems.RTSStarterKit.UI
{
	public class UnitInfo : MonoBehaviour
	{
		[SerializeField] GameObject selfObject;
		[SerializeField] Image unitIcon;
		[SerializeField] Text unitName;
		
		void Start()
		{
			Controls.Selection.unitSelected += SelectUnit;
			Controls.Selection.selectionCleared += UnselectUnit;
			UnselectUnit();
		}

		public void SelectUnit(Unit unit)
		{
			selfObject.SetActive(true);

			unitName.text = unit.data.textId;
			unitIcon.sprite = unit.data.icon;
		}

		public void UnselectUnit()
		{
			selfObject.SetActive(false);
		}

		void OnDestroy()
		{
			Controls.Selection.unitSelected -= SelectUnit;
			Controls.Selection.selectionCleared -= UnselectUnit;
		}
	}
}