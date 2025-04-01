using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace InsaneSystems.RTSStarterKit.UI
{
	public class HarvesterBar : MonoBehaviour
	{
		static Camera mainCamera;
		static readonly List<HarvesterBar> spawnedBars = new List<HarvesterBar>();

		[SerializeField] Image fillBar;
		RectTransform rectTransform;

		public Harvester harvester { get; protected set; }

		private void Awake()
		{
			rectTransform = GetComponent<RectTransform>();
			rectTransform.SetAsFirstSibling();

			if (!mainCamera)
				mainCamera = Camera.main;
		}

		void Update()
		{
			if (!harvester)
			{
				Destroy(gameObject);
				return;
			}

			rectTransform.anchoredPosition = mainCamera.WorldToScreenPoint(harvester.transform.position + Vector3.up);
		}

		public void SetupWithHarvester(Harvester harvester)
		{
			this.harvester = harvester;

			harvester.resourcesChangedEvent += OnResourcesChanged;

			OnResourcesChanged(harvester.harvestedResources, harvester.MaxResources);
		}

		void OnResourcesChanged(float newValue, float maxValue)
		{
			fillBar.fillAmount = newValue / maxValue;
		}

		public static void RemoveBarOfHarvester(Harvester harvester)
		{
			for (int i = 0; i < spawnedBars.Count; i++)
				if (spawnedBars[i].harvester == harvester)
				{
					Destroy(spawnedBars[i].gameObject);
					spawnedBars.RemoveAt(i);

					return;
				}
		}

		public static void SpawnForHarvester(Harvester harvester)
		{
			var spawnedBar = Instantiate(GameController.instance.MainStorage.harvesterBarTemplate, UIController.instance.MainCanvas.transform);
			var harvesterBar = spawnedBar.GetComponent<HarvesterBar>();

			harvesterBar.SetupWithHarvester(harvester);

			spawnedBars.Add(harvesterBar);
		}

		void OnDestroy()
		{
			if (harvester)
				harvester.resourcesChangedEvent -= OnResourcesChanged;
		}
	}
}