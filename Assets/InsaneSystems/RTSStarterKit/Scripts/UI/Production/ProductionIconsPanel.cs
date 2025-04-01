using System.Collections.Generic;
using UnityEngine;

namespace InsaneSystems.RTSStarterKit.UI
{
	public class ProductionIconsPanel : MonoBehaviour
	{
		[SerializeField] RectTransform iconsPanel;

		readonly List<UnitIcon> spawnedIcons = new List<UnitIcon>();

		void Awake()
		{
			SelectProductionNumberPanel.selectedProductionChanged += ProductionChangedAction;
		}

		void ProductionChangedAction(Production production) { Redraw(); }

		public void Redraw()
		{
			ClearDrawn();

			var selectedProduction = SelectProductionNumberPanel.selectedBuildingProduction;

			if (selectedProduction == null)
				return;
		
			for (int i = 0; i < selectedProduction.AvailableUnits.Count; i++)
			{
				var spawnedObject = Instantiate(GameController.instance.MainStorage.unitProductionIconTemplate, iconsPanel);
				var unitIconComponent = spawnedObject.GetComponent<UnitIcon>();

				unitIconComponent.SetupWithUnitData(this, selectedProduction.AvailableUnits[i]);
				unitIconComponent.SetHotkeyById(i);
				
				if (SelectProductionTypePanel.selectedProductionCategory.isBuildings && selectedProduction.unitsQueue.Count > 0)
				{
					bool isCurrentBuildingInQueue = selectedProduction.unitsQueue[0] != selectedProduction.AvailableUnits[i];
					
					unitIconComponent.SetActive(isCurrentBuildingInQueue);
				}

				spawnedIcons.Add(unitIconComponent);
			}
		}

		public UnitIcon GetIcon(int id)
		{
			if (spawnedIcons.Count > id)
				return spawnedIcons[id];

			return null;
		}

		void ClearDrawn()
		{
			for (int i = 0; i < spawnedIcons.Count; i++)
				Destroy(spawnedIcons[i].gameObject);

			spawnedIcons.Clear();
		}

		void OnDestroy()
		{
			SelectProductionNumberPanel.selectedProductionChanged -= ProductionChangedAction;
		}
	}
}