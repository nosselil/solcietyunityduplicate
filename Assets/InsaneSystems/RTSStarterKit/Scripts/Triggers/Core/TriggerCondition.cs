using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace InsaneSystems.RTSStarterKit.Triggers
{
	[System.Serializable]
	public class TriggerCondition
	{
		public TriggerConditionType conditionType = TriggerConditionType.ByEnteringZoneUnits;

		[Header("Units enter/exit zone settings")]
		public bool isUnitsShouldBeOwnedBySpecificPlayer = false;
		[Range(0, 15)] public int unitsPlayerOwnerShouldBe = 0;
		public bool isUnitsShouldBeOfSpecificType = false;
		public List<UnitData> unitsShouldBeOneOfTheseTypes = new List<UnitData>();

		[Header("Time condition settings")]
		public float timeToStartTrigger = 0f;

		public bool IsConditionTrue(Unit unitTriggeredZone = null)
		{
			if (conditionType == TriggerConditionType.ByTimeLeft)
			{
				return Time.timeSinceLevelLoad >= timeToStartTrigger;
			}
			else if (conditionType == TriggerConditionType.ByEnteringZoneUnits || conditionType == TriggerConditionType.ByExitingZoneUnits)
			{
				if (unitTriggeredZone == null)
					return false;

				if (isUnitsShouldBeOwnedBySpecificPlayer && !unitTriggeredZone.IsOwnedByPlayer(unitsPlayerOwnerShouldBe))
					return false;

				if (isUnitsShouldBeOfSpecificType && !unitsShouldBeOneOfTheseTypes.Contains(unitTriggeredZone.data))
					return false;

				return true;
			}

			return false;
		}
	}

	public enum TriggerConditionType
	{
		ByEnteringZoneUnits,
		ByExitingZoneUnits,
		ByTimeLeft,
		//ByUnitsCountOnMap,
		//ByKilledUnitsCount,
	}
}