using System.Collections.Generic;
using UnityEngine;

namespace InsaneSystems.RTSStarterKit
{
	public static class SpawnController
	{
		static List<PlayerStartPoint> playerStartPoints;
		
		public static void InitializeStartPoints()
		{
			playerStartPoints = new List<PlayerStartPoint>(GameObject.FindObjectsOfType<PlayerStartPoint>());
		}

		public static void SpawnPlayerStab(byte playerId)
		{
			if (playerStartPoints == null)
				InitializeStartPoints();

			if (playerStartPoints.Count == 0)
			{
				Debug.LogWarning("This map is not suitable for this player counts. Add spawn player points.");
				return;
			}

			// if there point, which contains spawn settings and id of this player, it will be selected, otherwise point to spawn will be randomed
			var specificPointToSpawnPlayer = playerStartPoints.Find(point => point.IsHardLockerPlayerOnThisPoint && point.IdOfPlayerToSpawn == playerId);

			int randomedPointId = Random.Range(0, playerStartPoints.Count);
			var selectedPoint = specificPointToSpawnPlayer ? specificPointToSpawnPlayer : playerStartPoints[randomedPointId];

			playerStartPoints.Remove(selectedPoint);

			var stabToSpawn = Player.GetPlayerById(playerId).selectedFaction.factionCommandCenter;

			var spawnedStabObject = GameObject.Instantiate(stabToSpawn.selfPrefab,
				selectedPoint.transform.position, Quaternion.identity);

			var stab = spawnedStabObject.GetComponent<Unit>();
			stab.SetOwner(playerId);

			if (playerId == Player.localPlayerId)
				GameController.instance.cameraMover.SetPosition(spawnedStabObject.transform.position);
		}

		public static Unit SpawnUnit(UnitData unitData, byte playerOwner, Transform spawnPoint)
		{
			return SpawnUnit(unitData, playerOwner, spawnPoint.position, spawnPoint.rotation);
		}

		public static Unit SpawnUnit(UnitData unitData, byte playerOwner, Vector3 position, Quaternion rotation)
		{
			var spawnedUnitObject = GameObject.Instantiate(unitData.selfPrefab, position, rotation);
			var spawnedUnit = spawnedUnitObject.GetComponent<Unit>();

			spawnedUnit.SetOwner(playerOwner);

			return spawnedUnit;
		}
	}
}