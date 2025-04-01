using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace InsaneSystems.RTSStarterKit.UI
{
	/// <summary>
	/// This class describes component, which allows to draw more info above selected unit (like healthbar etc).
	/// Instantiates when healthbar being instantiated.
	/// </summary>
	public class UnitAboveInfo : MonoBehaviour
	{
		[SerializeField] Text selectionGroupText;
		[SerializeField] GameObject lockedIconObject;
		[SerializeField] RectTransform carryCellsPanel;
		
		readonly List<CarryCell> carryCells = new List<CarryCell>();

		Unit selfUnit;
		float updateTimer;

		Healthbar selfHealthbar;

		int totalCarryCellsCount;
		
		void Awake()
		{
			selectionGroupText.enabled = false;
			selfHealthbar = GetComponent<Healthbar>();
			
			totalCarryCellsCount = GameController.instance.MainStorage.carriedUnitsIconsCount;

			var carryCellTemplate = GameController.instance.MainStorage.carryCellTemplate;
			for (var i = 0; i < totalCarryCellsCount; i++)
			{
				var spawnedCell = Instantiate(carryCellTemplate, carryCellsPanel);
				carryCells.Add(spawnedCell.GetComponent<CarryCell>());
				carryCells[i].SetActive(false);
			}

			lockedIconObject.SetActive(false);
		}

		void Update()
		{
			updateTimer -= Time.deltaTime;

			if (updateTimer <= 0)
			{
				if (selfHealthbar && (!selfUnit || selfUnit != selfHealthbar.damageable.selfUnit))
					SetupWithUnit(selfHealthbar.damageable.selfUnit);

				lockedIconObject.SetActive(selfUnit.isMovementLockedByHotkey);
				UpdateText();
				UpdateCarryCells();
				updateTimer = 0.2f;
			}
		}

		public void SetupWithUnit(Unit unit)
		{
			selfUnit = unit;

			UpdateText();
			UpdateCarryCells();
		}

		void UpdateText()
		{
			if (!selfUnit)
			{ 
				selectionGroupText.enabled = false;
				return;
			}

			if (selfUnit.unitSelectionGroup > -1)
			{
				selectionGroupText.enabled = true;
				selectionGroupText.text = selfUnit.unitSelectionGroup.ToString();
			}
			else
			{
				selectionGroupText.enabled = false;
			}
		}

		void UpdateCarryCells()
		{
			var carrierModule = selfUnit.GetModule<CarryModule>();
			
			for (var i = 0; i < totalCarryCellsCount; i++)
			{
				if (!selfUnit.IsOwnedByPlayer(Player.localPlayerId))
				{
					carryCells[i].SetActive(false);
					continue;
				}
				
				carryCells[i].SetActive(selfUnit.data.canCarryUnitsCount > i);
				
				if (carrierModule && carrierModule.carryingUnits.Count > i)
					carryCells[i].UpdateState(carrierModule.carryingUnits[i]);
				else
					carryCells[i].UpdateState(null);
			}
		}
	}
}