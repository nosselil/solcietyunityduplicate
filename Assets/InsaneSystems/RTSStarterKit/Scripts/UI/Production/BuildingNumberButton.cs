using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace InsaneSystems.RTSStarterKit.UI
{
	public class BuildingNumberButton : MonoBehaviour
	{
		[SerializeField] Text numberText;
		[SerializeField] Button selfButton;

		[SerializeField] [Range(0, 4)] int buildingId;

		SelectProductionNumberPanel controllerPanel;

		Image selfImage;
		Color defaultColor;

		private void Awake()
		{
			selfImage = GetComponent<Image>();
			defaultColor = selfImage.color;
		}

		public void SetActive() { selfImage.color = Color.green; }
		public void SetUnactive() { selfImage.color = defaultColor; }
		public void SetEnabled() { selfButton.interactable = true; }
		public void SetDisabled() { selfButton.interactable = false; }

		public void OnClick() { controllerPanel.SelectBuildingWithNumber(buildingId); }

		public void SetupBuildingId(int id)
		{
			buildingId = id;

			numberText.text = (buildingId + 1).ToString();
		}

		public void SetupWithController(SelectProductionNumberPanel controllerPanel) { this.controllerPanel = controllerPanel; }
	}
}