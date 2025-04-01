using System.Collections.Generic;
using UnityEngine;

namespace InsaneSystems.RTSStarterKit.UI
{
	public class CarryingUnitList : MonoBehaviour
	{
		const float updateTime = 0.25f;
		const float pooledIconsCount = 10;

		readonly List<GameObject> pooledIcons = new List<GameObject>();

		[SerializeField] GameObject selfObject;
		[SerializeField] RectTransform unitsIconsParent;

		readonly List<CarryingUnitIcon> drawnIcons = new List<CarryingUnitIcon>();

		float updateTimer;

		bool isPoolLoaded;

		Unit selectedUnit;

		void Start()
		{
			Controls.Selection.unitSelected += OnUnitSelected;
			Controls.Selection.selectionCleared += OnClearSelection;

			LoadPool();

			Hide();
		}

		void LoadPool()
		{
			if (isPoolLoaded)
				return;

			for (int i = 0; i < pooledIconsCount; i++)
			{
				var spawnedIconObject = Instantiate(GameController.instance.MainStorage.unitCarryingIcon, unitsIconsParent);
				spawnedIconObject.SetActive(false);

				pooledIcons.Add(spawnedIconObject);
			}

			isPoolLoaded = true;
		}

		void Update()
		{
			if (!selfObject.activeSelf)
				return;

			if (updateTimer > 0)
			{
				updateTimer -= Time.deltaTime;
				return;
			}

			updateTimer = updateTime;
		}

		public void OnClearSelection() { Hide(); }

		public void OnUnitSelected(Unit unit)
		{
			selectedUnit = unit;

			Redraw();
		}

		public void Redraw()
		{
			if (!selectedUnit)
				return;

			var carryModule = selectedUnit.GetModule<CarryModule>();

			if (!carryModule)
				return;

			Show();
			ClearDrawn();

			var iconTemplate = GameController.instance.MainStorage.unitMultiselectionIconTemplate;

			for (int i = 0; i < carryModule.carryingUnits.Count; i++)
			{
				GameObject iconObject = null;

				if (pooledIcons.Count > 0)
					iconObject = TakeIconFromPool();
				else
					iconObject = Instantiate(iconTemplate, unitsIconsParent);

				var spawnedIconComponent = iconObject.GetComponent<CarryingUnitIcon>();
				spawnedIconComponent.Setup(carryModule.carryingUnits[i], selectedUnit, this);

				drawnIcons.Add(spawnedIconComponent);
			}

			if (carryModule.carryingUnits.Count == 0)
				Hide();
		}

		void ClearDrawn()
		{
			for (int i = 0; i < drawnIcons.Count; i++)
				if (drawnIcons[i] != null)
					MoveIconToPool(drawnIcons[i].gameObject);

			drawnIcons.Clear();
		}

		void Show() { selfObject.SetActive(true); }
		void Hide() { selfObject.SetActive(false); }

		GameObject TakeIconFromPool()
		{
			var iconFromPool = pooledIcons[0];
			pooledIcons.RemoveAt(0);

			iconFromPool.SetActive(true);

			return iconFromPool;
		}

		public void MoveIconToPool(GameObject iconObject)
		{
			if (!pooledIcons.Contains(iconObject))
				pooledIcons.Add(iconObject);

			iconObject.SetActive(false);
		}

		void OnDestroy()
		{
			Controls.Selection.unitSelected -= OnUnitSelected;
			Controls.Selection.selectionCleared -= OnClearSelection;
		}
	}
}