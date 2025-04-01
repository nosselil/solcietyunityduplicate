using System.Collections.Generic;

namespace InsaneSystems.RTSStarterKit.AI
{
	public class UnitsGroup
	{
		const int defaultGroupSize = 3;

		Order groupOrder;

		readonly List<Unit> unitsInGroup = new List<Unit>();

		public void AddOrderToGroup(Order order, bool isAdditive = false)
		{
			groupOrder = order;

			var units = GetAliveUnitsOfGroup();

			for (int i = 0; i < units.Count; i++)
				units[i].AddOrder(order, isAdditive, i == 0);
		}

		public void AddUnit(Unit unit) { unitsInGroup.Add(unit); }

		public bool IsGroupNeedsUnits()
		{
			int groupSize = GetAliveUnitsOfGroup().Count;

			return groupSize < defaultGroupSize;
		}

		public bool IsGroupHaveOrder()
		{
			var attackOrder = groupOrder as AttackOrder;
			
			if (attackOrder != null && attackOrder.attackTarget == null)
				groupOrder = null;

			return groupOrder != null;
		}

		List<Unit> GetAliveUnitsOfGroup()
		{
			var aliveUnits = new List<Unit>();

			for (int i = 0; i < unitsInGroup.Count; i++)
				if (unitsInGroup[i] != null)
					aliveUnits.Add(unitsInGroup[i]);

			return aliveUnits;
		}
	}
}