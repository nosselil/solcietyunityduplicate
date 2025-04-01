using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace InsaneSystems.RTSStarterKit.Triggers
{
	public class SpawnUnitsTrigger : TriggerBase
	{
		public List<UnitData> unitsToSpawn;
		[Tooltip("Transform point, in which position will be spawned units. You can use assign GameObject here.")]
		public Transform spawnPoint;
		[Range(0, 15)] public int playerOwner = 0;

		protected override void ExecuteAction()
		{
			for (int i = 0; i < unitsToSpawn.Count; i++)
				SpawnController.SpawnUnit(unitsToSpawn[i], (byte)playerOwner, spawnPoint);
		}
	}
}