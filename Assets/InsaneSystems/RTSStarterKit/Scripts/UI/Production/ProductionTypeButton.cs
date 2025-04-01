using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

namespace InsaneSystems.RTSStarterKit.UI
{
	public class ProductionTypeButton : MonoBehaviour
	{
		[SerializeField] ProductionCategory productionCategory;
		
		SelectProductionTypePanel controllerPanel;

		Image selfImage;
		Button selfButton;
		Color defaultColor;

		public ProductionCategory GetProductionCategory
		{
			get { return productionCategory; }
		}

		void Awake()
		{
			selfImage = GetComponent<Image>();
			selfButton = GetComponent<Button>();
			defaultColor = selfImage.color;
		}

		void Start() { Redraw(); }
        private void Update()
        {
			if (productionCategory.textId == "Vehicles")
			{
                Unit[] unitsSpawned = FindObjectsOfType<Unit>();
                Unit _selectedUnit = null;
                for (int i = 0; i < unitsSpawned.Length; i++)
				{
                    if (Player.GetLocalPlayer().id == unitsSpawned[i].OwnerPlayerId)
					{
                        _selectedUnit = unitsSpawned[i];
                        break;
                    }
                }
				if (_selectedUnit == null) return;
				for (int i = 0; i < _selectedUnit.data.productionCategories.Count; i++)
				{
					if (_selectedUnit.data.productionCategories[i].textId == productionCategory.textId)
					{
                        productionCategory = _selectedUnit.data.productionCategories[i];
						return;
                    }
				}
            }
        }





public void SetActive()
{
    if (UnityEngine.ColorUtility.TryParseHtmlString("#77c4ff", out Color newColor))
    {
        selfImage.color = newColor;
    }
    else
    {
        Debug.LogError("Invalid hex code!");
    }
}		public void SetUnactive() { selfImage.color = defaultColor; }
		public void OnClick() { controllerPanel.OnSelectButtonClick(productionCategory); }

		public void Redraw()
		{
			selfButton.interactable = Player.GetLocalPlayer().IsHaveProductionOfCategory(productionCategory);
		}

		public void SetupWithController(SelectProductionTypePanel typePanel) { controllerPanel = typePanel; }

		public void SetupWithProductionCategory(ProductionCategory category)
		{
			productionCategory = category;

			selfImage.sprite = category.icon;

			Redraw();
		}
	}
}