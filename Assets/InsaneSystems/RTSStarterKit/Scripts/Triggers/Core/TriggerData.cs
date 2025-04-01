using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace InsaneSystems.RTSStarterKit.Triggers
{
	[System.Serializable]
	public class TriggerData
	{
		public TriggerType triggerType;
		public string triggerTextId = "Trigger ID";
		public TriggerBase trigger;
	}

	/// <summary>List of all available trigger types. Add new types only to the end of enum list. Don't forget to add new trigger types when you create it.</summary>
	public enum TriggerType
	{
		None,
		SpawnUnits,
		AddMoney,
		ChangeOwner,
		MoveOrder,
		ProductionAddUnit,
		Givemoney
	}

	public class TriggerAttribute : PropertyAttribute { }
}