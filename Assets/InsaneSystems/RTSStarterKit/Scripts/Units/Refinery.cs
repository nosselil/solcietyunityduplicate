using System.Collections.Generic;
using UnityEngine;

namespace InsaneSystems.RTSStarterKit
{
	public class Refinery : Module
	{
		public static List<Refinery> allRefineries { get; private set; }
		
		[SerializeField] Transform carryOutResourcesPoint;
		
		[Tooltip("Harvester unit data which will be spawned on this refinery at start.")]
		[SerializeField] UnitData harversterUnitData;

		public Transform CarryOutResourcesPoint { get { return carryOutResourcesPoint; } }

		protected override void AwakeAction()
		{
			if (allRefineries == null)
				allRefineries = new List<Refinery>();
			
			allRefineries.Add(this);
		}

		void Start()
		{
			SpawnHarvester();
		}

		public void AddResources(int amount,ResourcesField resourcesField)
		{
			if(resourcesField.ResourceData == null)
			{
                GameController.instance.playersController.playersIngame[selfUnit.OwnerPlayerId].AddMoney(amount);
			}
			else
			{
				GameResourcesManager.instance.AddResource(amount,resourcesField.ResourceData);
			}
        }

		void SpawnHarvester()
		{
			var spawnedHarvester = SpawnController.SpawnUnit(harversterUnitData, selfUnit.OwnerPlayerId, carryOutResourcesPoint);
			spawnedHarvester.GetComponent<Harvester>().SetRefinery(this);
		}

		void OnDestroy()
		{
			allRefineries.Remove(this);
		}
	}
}