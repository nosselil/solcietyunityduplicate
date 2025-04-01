using System.Collections.Generic;
using UnityEngine;

namespace InsaneSystems.RTSStarterKit
{
	[CreateAssetMenu(fileName = "ProductionCategory", menuName = "RTS Starter Kit/Production Category")]
	public class ProductionCategory : ScriptableObject
	{
		public string textId;
		public Sprite icon;
		public List<UnitData> availableUnits;
		public bool isBuildings;
	}
}