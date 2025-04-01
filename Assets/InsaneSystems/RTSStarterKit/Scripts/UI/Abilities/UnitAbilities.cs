using System.Collections.Generic;
using InsaneSystems.RTSStarterKit.Controls;
using UnityEngine;
using UnityEngine.UI;

namespace InsaneSystems.RTSStarterKit.UI
{
	public class UnitAbilities : MonoBehaviour
	{
		[SerializeField] GameObject selfObject;
		[SerializeField] Transform abilitiesPanel;

		readonly List<GameObject> drawnIcons = new List<GameObject>();

		Unit selectedUnit;

		void Start()
		{
			Controls.Selection.onUnitsListSelected += OnUnitsListSelected;
			Controls.Selection.unitSelected += Show;
			Controls.Selection.selectionCleared += Hide;
			
			Hide();
		}

		void OnUnitsListSelected(List<Unit> units)
		{
			for (int i = 0; i < units.Count; i++) // temporary multi-selection abilities window for carry ability. In future should be another solution
			{
				var carryModule = units[i].GetModule<CarryModule>();

				if (!carryModule)
				{
					Hide();
					return;
				}
			}
			
			if (IsNeedToBeShown(units[0]))
				Show(units[0]);
		}

		public bool IsNeedToBeShown(Unit forUnit)
		{
			var abilitiesModule = forUnit.GetModule<AbilitiesModule>();

			bool needToShow = true;

			if (!abilitiesModule || abilitiesModule.abilities.Count == 0)
				needToShow = false;

			if (forUnit.data.canCarryUnitsCount > 0) // it is temporary condition
				needToShow = true; 

			return needToShow; 
		}

		public void Show(Unit forUnit)
		{
			if (!IsNeedToBeShown(forUnit))
				return;
			
			selectedUnit = forUnit;
			selfObject.SetActive(true);

			Redraw();
		}

		public void Redraw()
		{
			var abilitiesModule = selectedUnit.GetModule<AbilitiesModule>();

			ClearDrawn();

			var iconTemplate = GameController.instance.MainStorage.unitAbilityIcon;

			for (int i = 0; i < abilitiesModule.abilities.Count; i++)
			{
				var iconObject = Instantiate(iconTemplate, abilitiesPanel);

				var spawnedIconComponent = iconObject.GetComponent<AbilityIcon>();
				spawnedIconComponent.Setup(abilitiesModule.abilities[i]);
				spawnedIconComponent.SetupHotkeyByNumber(i);
				
				drawnIcons.Add(iconObject);
			}
		}

		void ClearDrawn()
		{
			for (int i = 0; i < drawnIcons.Count; i++)
				if (drawnIcons[i] != null)
					Destroy(drawnIcons[i]);

			drawnIcons.Clear();
		}

		public void Hide()
		{
			selfObject.SetActive(false);
		}

		void OnDestroy()
		{
			Controls.Selection.onUnitsListSelected -= OnUnitsListSelected;
			Controls.Selection.unitSelected -= Show;
			Controls.Selection.selectionCleared -= Hide;
		}
	}
}