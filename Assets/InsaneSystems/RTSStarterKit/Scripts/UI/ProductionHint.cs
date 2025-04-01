using UnityEngine;
using UnityEngine.UI;

namespace InsaneSystems.RTSStarterKit.UI
{
	public class ProductionHint : MonoBehaviour
	{
		[SerializeField] GameObject selfObject;
		[SerializeField] Text nameText;
		[SerializeField] Text descriptionText;

		RectTransform rectTransform;

		void Start()
		{
			rectTransform = selfObject.GetComponent<RectTransform>();

			Hide();
		}

		public void Show(UnitData unitData, Vector2 position)
		{
			selfObject.SetActive(true);

			rectTransform.anchoredPosition = position;
			nameText.text = unitData.textId;

			string electricityText = "";

			var textLibrary = GameController.instance.textsLibrary;

			if (GameController.instance.MainStorage.isElectricityUsedInGame)
			{
				if (unitData.addsElectricity > 0)
					electricityText = textLibrary.GetUITextById("addsElectricity") + ": " + unitData.addsElectricity + "\n";

				if (unitData.usesElectricity > 0)
					electricityText =  textLibrary.GetUITextById("usesElectricity") + ": " + unitData.usesElectricity + "\n";
			}

			descriptionText.text = electricityText + textLibrary.GetUITextById("price") + ": " + unitData.price  ;
			for (int i = 0; i < unitData.ResourceRequirements.Count; i++)
			{
				descriptionText.text += "\n" + unitData.ResourceRequirements[i].gameResource.ResourceName + ":" + unitData.ResourceRequirements[i].ResourceRequired.ToString();
            }

            LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)descriptionText.transform); // needed for content size fitter ## необходимо для того, чтобы Content Size Fitter обновлял размер
		}

		public void ShowForAbility(Abilities.Ability ability, Vector2 position)
		{
			selfObject.SetActive(true);

			rectTransform.anchoredPosition = position;
			nameText.text = ability.abilityName;
			descriptionText.text = "";

			LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)descriptionText.transform);
		}

		public void Hide()
		{
			selfObject.SetActive(false);
		}
	}
}