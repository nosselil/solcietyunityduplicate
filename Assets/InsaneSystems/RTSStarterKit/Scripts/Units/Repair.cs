using UnityEngine;

namespace InsaneSystems.RTSStarterKit
{
	/// <summary> This is repair component. When added to unit, it will restore its health by spending player money.</summary>
	public class Repair : MonoBehaviour
	{
		int repairPerSecond = 5, repairCost = 2;

		float repairTimer;

		Damageable damageable;
		Unit unit;

		GameObject repairEffectSpawned;
		
		public void Start()
		{
			unit = GetComponent<Unit>();
			damageable = unit.GetModule<Damageable>();

			if (!unit || !damageable)
			{
				enabled = false;
				return;
			}

			repairPerSecond = GameController.instance.MainStorage.buildingRepairHealthPerSecond;
			repairCost = GameController.instance.MainStorage.buildingRepairCostPerSecond;
			
			repairEffectSpawned = Instantiate(GameController.instance.MainStorage.repairEffectTemplate, transform.position, Quaternion.identity, transform);
		}

		public void Update()
		{
			if (repairTimer > 0)
			{
				repairTimer -= Time.deltaTime;
				return;
			}

			if (!unit.GetOwnerPlayer().IsHaveMoney(repairCost,null))
				return;
			
			if (damageable.GetHealthPercents() >= 1f)
			{
				RemoveRepair();
				return;
			}

			damageable.AddHealth(repairPerSecond);
			unit.GetOwnerPlayer().SpendMoney(repairCost);
			
			repairTimer = 1f;
		}

		public void RemoveRepair()
		{
			Destroy(repairEffectSpawned);
			Destroy(this);
		}
	}
}