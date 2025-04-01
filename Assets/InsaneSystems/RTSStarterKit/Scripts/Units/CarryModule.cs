using System.Collections.Generic;
using InsaneSystems.RTSStarterKit.Abilities;
using UnityEngine;

namespace InsaneSystems.RTSStarterKit
{
	/// <summary> This module allows unit to carry other units on board. </summary>
	public class CarryModule : Module
	{
		public event OnCarryStateChanged onCarryStateChanged;
		public event UnitsExitAction allUnitsExited;
		public event UnitExitAction onUnitExit;
		
		public readonly List<Unit> carryingUnits = new List<Unit>();
		/// <summary> List of units which will be taken on board by this carrier when reach take distance.</summary>
		readonly List<Unit> unitsToTake = new List<Unit>();

		readonly List<Vector3> randomedOffsets = new List<Vector3>();

		[SerializeField] Ability carryOutAbility;
		
		public delegate void OnCarryStateChanged(bool isCarried);
		public delegate void UnitsExitAction();
		public delegate void UnitExitAction(Unit unit);

		void Start()
		{
			selfUnit.GetModule<AbilitiesModule>().GetOrAddAbility(carryOutAbility);
			
			for (var i = 0; i < selfUnit.data.canCarryUnitsCount; i++)
			{
				var randomedX = Mathf.Sin(Random.Range(-1f, 1f) * Mathf.PI) * 0.2f;
				var randomedZ = Mathf.Cos(Random.Range(-1f, 1f) * Mathf.PI) * 0.2f;

				randomedOffsets.Add(new Vector3(randomedX, 0f, randomedZ));
			}
		}

		void Update()
		{
			for (var i = 0; i < carryingUnits.Count; i++)
				carryingUnits[i].transform.position = transform.position + randomedOffsets[i];

			for (var i = unitsToTake.Count - 1; i >= 0; i--)
			{
				if (!CanCarryOneMoreUnit())
				{
					unitsToTake[i].EndCurrentOrder();
					unitsToTake.RemoveAt(i);
					continue;
				}

				if (Vector3.Distance(unitsToTake[i].transform.position, transform.position) < 1.75f)
				{
					CarryUnit(unitsToTake[i]);
					unitsToTake.RemoveAt(i);
				}
			}
		}

		public bool CanCarryOneMoreUnit()
		{
			return selfUnit.data.canCarryUnitsCount > carryingUnits.Count;
		}
		
		public int CanCarryCount()
		{
			return Mathf.Clamp(selfUnit.data.canCarryUnitsCount - carryingUnits.Count, 0, selfUnit.data.canCarryUnitsCount);
		}

		public void PrepareToCarryUnits(List<Unit> units)
		{
			for (var i = 0; i < units.Count; i++)
				PrepareToCarryUnit(units[i]);
		}
		
		public void PrepareToCarryUnit(Unit unit)
		{
			if (!unit || !unit.IsInMyTeam(selfUnit) || !unit.data.canBeCarried)
				return;

			var order = new FollowOrder
			{
				followTarget = selfUnit
			};

			unit.AddOrder(order, false);
			
			if (!unitsToTake.Contains(unit))
				unitsToTake.Add(unit);
		}
		
		public void CarryUnit(Unit unit)
		{
			if (!CanCarryOneMoreUnit() || unit.isBeingCarried)
				return;
			
			SetUnitCarryState(unit, true);
			carryingUnits.Add(unit);

			UI.UIController.instance.carryingUnitList.Redraw();
		}

		public void ExitUnit(Unit unit)
		{
			SetUnitCarryState(unit, false);

			var randomedX = Mathf.Sin(Random.Range(-1f, 1f) * Mathf.PI);
			var randomedZ = Mathf.Cos(Random.Range(-1f, 1f) * Mathf.PI);
			
			unit.transform.position = transform.position + new Vector3(randomedX, 0f, randomedZ);

			var order = new MovePositionOrder();
			order.movePosition = unit.transform.position + new Vector3(randomedX, 0f, randomedZ) * 2f;
			unit.AddOrder(order, false, false);
			
			carryingUnits.Remove(unit);
			
			if (onUnitExit != null) onUnitExit.Invoke(unit);
		}

		void SetUnitCarryState(Unit unit, bool isCarried)
		{
			if (unit.data.hasMoveModule && isCarried)
				unit.GetModule<Movable>().Stop();
			
			unit.SetCarryState(isCarried);
			
			if (onCarryStateChanged != null) onCarryStateChanged.Invoke(isCarried);

			if (carryingUnits.Count == 0 && allUnitsExited != null)
				allUnitsExited.Invoke();
		}

		public void ExitAllUnits(bool dieExit = false)
		{
			for (var i = carryingUnits.Count - 1; i >= 0; i--)
			{
				if (dieExit && carryingUnits[i].GetModule<Damageable>())
					carryingUnits[i].GetModule<Damageable>().TakeDamage(carryingUnits[i].data.maxHealth / 2f);
				
				ExitUnit(carryingUnits[i]);
			}

			UI.UIController.instance.carryingUnitList.Redraw();
			
			if (allUnitsExited != null) allUnitsExited.Invoke();
		}
	}
}