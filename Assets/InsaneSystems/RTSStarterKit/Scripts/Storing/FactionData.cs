using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace InsaneSystems.RTSStarterKit
{
	[CreateAssetMenu(fileName = "FactionData", menuName = "RTS Starter Kit/Faction Data")]
	public class FactionData : ScriptableObject
	{
		public string textId = "Faction Name";
		[Tooltip("Command center of faction, which will be spawned on game start.")]
		public UnitData factionCommandCenter;
		[Tooltip("Default house color of faction.")]
		public Color defaultColor = Color.red;
		[Tooltip("Drag here all production categories which belongs to this faction.")]
		public List<ProductionCategory> ownProductionCategories = new List<ProductionCategory>();
	}
}