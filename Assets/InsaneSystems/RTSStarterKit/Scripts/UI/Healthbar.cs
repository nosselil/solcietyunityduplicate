using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace InsaneSystems.RTSStarterKit.UI
{
	public class Healthbar : MonoBehaviour
	{
		const float pooledHealthbarsCount = 20;

		static readonly List<GameObject> pooledHealthbars = new List<GameObject>();
		static readonly List<Healthbar> spawnedHealthbars = new List<Healthbar>();
		static Camera mainCamera;

		static bool isPoolLoaded;

		[SerializeField] Image fillImage;

		RectTransform rectTransform;

		float maxHealthValue;

		public Damageable damageable { get; protected set; }

		void Awake()
		{
			rectTransform = GetComponent<RectTransform>();

			if (!mainCamera)
				mainCamera = Camera.main;
		}

		void Start()
		{
			LoadPool();
		}

		void Update()
		{
			if (!damageable)
			{
				MoveHealthbarToPool(gameObject);
				spawnedHealthbars.Remove(this);
				return;
			}

			fillImage.fillAmount = damageable.health / maxHealthValue;
			UpdatePosition();
		}

		void UpdatePosition()
		{
			rectTransform.anchoredPosition = mainCamera.WorldToScreenPoint(damageable.transform.position + Vector3.up) + Vector3.up * 8;
		}
		
		public void SetupWithDamageable(Damageable damageable)
		{
			this.damageable = damageable;

			maxHealthValue = damageable.selfUnit.data.maxHealth;

			UpdatePosition();
		}

		static void LoadPool()
		{
			if (isPoolLoaded)
				return;

			for (int i = 0; i < pooledHealthbarsCount; i++)
			{
				GameObject spawnedObject = Instantiate(GameController.instance.MainStorage.healthbarTemplate, UIController.instance.MainCanvas.transform);
				spawnedObject.SetActive(false);
				spawnedObject.transform.SetAsFirstSibling();

				pooledHealthbars.Add(spawnedObject);
			}

			isPoolLoaded = true;
		}

		public static void SpawnHealthbarForUnit(Unit unit)
		{
			var damageable = unit.GetModule<Damageable>();

			if (damageable)
				SpawnHealthbarForDamageable(damageable);
		}

		public static void SpawnHealthbarForDamageable(Damageable damageable)
		{
			GameObject healthbarObject;

			if (pooledHealthbars.Count != 0)
			{
				healthbarObject = pooledHealthbars[0];
				pooledHealthbars.RemoveAt(0);

				healthbarObject.SetActive(true);
			}
			else
			{
				healthbarObject = Instantiate(GameController.instance.MainStorage.healthbarTemplate, UIController.instance.MainCanvas.transform);

				healthbarObject.transform.SetAsFirstSibling();
			}

			var healthbar = healthbarObject.GetComponent<Healthbar>();
			healthbar.SetupWithDamageable(damageable);
			spawnedHealthbars.Add(healthbar);
		}

		public static void RemoveHealthbarOfUnit(Unit unit) { RemoveHealthbarOfDamageable(unit.damageable); }
		public static void RemoveHealthbarOfDamageable(Damageable damageable)
		{ 
			for (int i = 0; i < spawnedHealthbars.Count; i++)
				if (spawnedHealthbars[i].damageable == damageable)
				{
					MoveHealthbarToPool(spawnedHealthbars[i].gameObject);
					spawnedHealthbars.RemoveAt(i);

					return;
				}
		}

		public static void RemoveAllHealthbars()
		{
			for (int i = 0; i < spawnedHealthbars.Count; i++)
				MoveHealthbarToPool(spawnedHealthbars[i].gameObject);

			spawnedHealthbars.Clear();
		}

		public static void MoveHealthbarToPool(GameObject healthBarObject)
		{
			pooledHealthbars.Add(healthBarObject);
			healthBarObject.SetActive(false);
		}

		public static void ResetPool()
		{
			if (pooledHealthbars != null)
				pooledHealthbars.Clear();

			if (spawnedHealthbars != null)
				spawnedHealthbars.Clear();

			isPoolLoaded = false;
			mainCamera = Camera.main;
		}
	}
}