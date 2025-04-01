using UnityEngine;
using UnityEngine.UI;

namespace InsaneSystems.RTSStarterKit.UI
{
	public class ElectricityBar : MonoBehaviour
	{
		[SerializeField] GameObject selfObject;
		[SerializeField] Image electricityFill;
		[SerializeField] Image usedElectricityFill;

		bool isElectricityUsed;

		void Start()
		{
			isElectricityUsed = GameController.instance.MainStorage.isElectricityUsedInGame;

			selfObject.SetActive(isElectricityUsed);

			if (isElectricityUsed)
				Player.localPlayerElectricityChangedEvent += OnElectricityChanged;
		}

		void OnElectricityChanged(int totalElectricity, int usedElectricity)
		{
			electricityFill.fillAmount = totalElectricity / 100f;
			usedElectricityFill.fillAmount = usedElectricity / 100f;
		}

		void OnDestroy()
		{
			if (isElectricityUsed)
				Player.localPlayerElectricityChangedEvent -= OnElectricityChanged;
		}
	}
}