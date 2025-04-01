using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace InsaneSystems.RTSStarterKit.UI
{
	public class SelectProductionNumberPanel : MonoBehaviour
	{
		public static int selectedBuildingNumber { get; protected set; }
		public static Production selectedBuildingProduction { get; protected set; }

		public static event SelectedProductionChangedAction selectedProductionChanged;
		public delegate void SelectedProductionChangedAction(Production selectedProduction);

		[SerializeField] RectTransform iconsPanel;
		[SerializeField] List<BuildingNumberButton> buildNumberIcons = new List<BuildingNumberButton>();

		private void Awake()
		{
			SelectProductionTypePanel.productionCategoryChanged += OnProductionCategoryChanged;
		}

		void OnProductionCategoryChanged(ProductionCategory newCategory) { SelectBuildingWithNumber(0); }

		public void SelectBuildingWithNumber(int number)
		{
			List<Production> playerProductions = GetPlayerProductionsByCategory(SelectProductionTypePanel.selectedProductionCategory);

			if (number >= playerProductions.Count)
				return;

			selectedBuildingNumber = number;
			selectedBuildingProduction = GetPlayerProductionByTypeAndNumber(SelectProductionTypePanel.selectedProductionCategory, selectedBuildingNumber);
			Redraw(SelectProductionTypePanel.selectedProductionCategory);

			if (selectedProductionChanged != null)
				selectedProductionChanged.Invoke(selectedBuildingProduction);
		}

		void Redraw(ProductionCategory newCategory)
		{
			List<Production> playerProductions = GetPlayerProductionsByCategory(newCategory);

			for (int i = 0; i < buildNumberIcons.Count; i++)
			{
				var buildingNumberButton = buildNumberIcons[i];

				buildingNumberButton.SetupBuildingId(i);
				buildingNumberButton.SetupWithController(this);

				if (i < playerProductions.Count)
					buildingNumberButton.SetEnabled();
				else
					buildingNumberButton.SetDisabled();

				if (i == selectedBuildingNumber)
					buildingNumberButton.SetActive();
				else
					buildingNumberButton.SetUnactive();
			}
		}

		Production GetPlayerProductionByTypeAndNumber(ProductionCategory category, int number)
		{
			List<Production> playerProductions = GetPlayerProductionsByCategory(category);
			
			return playerProductions.Count > 0 ? playerProductions[number] : null;
		}

		List<Production> GetPlayerProductionsByCategory(ProductionCategory category)
		{
			Player localPlayer = GameController.instance.playersController.playersIngame[Player.localPlayerId];
			List<Production> playerProductions = localPlayer.GetProductionBuildingsByCategory(category);

			return playerProductions;
		}

		void OnDestroy()
		{
			SelectProductionTypePanel.productionCategoryChanged -= OnProductionCategoryChanged;
		}
	}
}