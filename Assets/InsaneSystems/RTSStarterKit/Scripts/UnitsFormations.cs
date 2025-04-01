using System.Collections.Generic;
using UnityEngine;

namespace InsaneSystems.RTSStarterKit
{
	public static class UnitsFormations
	{
		/// <summary> Makes square formation</summary>
		public static List<Vector3> GetWaypointsForUnitsGroupSquare(Vector3 centerPosition, int unitsCount, float maxRadiusOfUnit = 1f)
		{
			if (unitsCount == 1)
				return new List<Vector3> { centerPosition };

			maxRadiusOfUnit *= 2f; // diameter

			var resultPositions = new List<Vector3>();

			int rowsCount = Mathf.FloorToInt(Mathf.Sqrt(unitsCount));
			int unitsPerRow = rowsCount;
			int row = 0, cell = 0;

			var centerOffset = new Vector3(rowsCount * maxRadiusOfUnit / 2f, 0, unitsPerRow * maxRadiusOfUnit / 2f);

			for (int i = 0; i < unitsCount; i++)
			{
				resultPositions.Add(centerPosition - centerOffset + new Vector3(maxRadiusOfUnit * row + Random.Range(-maxRadiusOfUnit / 10f, maxRadiusOfUnit / 10f), 0, maxRadiusOfUnit * cell + Random.Range(-0.175f, 0.175f))); // adding a little random because fully straight lines of units looks stupid

				cell++;

				if (cell >= unitsPerRow)
				{
					row++;
					cell = 0;
				}
			}

			return resultPositions;
		}

		/// <summary>Saves current units formation</summary>
		public static List<Vector3> GetWaypointsForUnitsGroup(Vector3 destination, List<Unit> units)
		{
			var resultWaypoints = new List<Vector3>();
			var currentGroupCenterPoint = Vector3.zero;

			int aliveUnitsCount = units.Count;

			for (int i = 0; i < units.Count; i++)
				if (units[i])
					currentGroupCenterPoint += units[i].transform.position;
				else
					aliveUnitsCount--;

			currentGroupCenterPoint /= aliveUnitsCount;

			for (int i = 0; i < units.Count; i++)
			{
				if (!units[i])
					continue;

				Vector3 unitOffset = units[i].transform.position - currentGroupCenterPoint;
				Vector3 unitWaypoint = destination + unitOffset / 4f; // decreasing distances between units

				//Debug.DrawRay(unitWaypoint, Vector3.up, Color.green, 1f);
				resultWaypoints.Add(unitWaypoint);
			}
			
			// this method restorces distances between units if they is to small

			for (int i = 0; i < resultWaypoints.Count; i++)
			{
				var currentWayPointToMove = resultWaypoints[i];

				for (int k = 0; k < resultWaypoints.Count; k++)
				{
					var checkingPoint = resultWaypoints[k];
					var distance =
						Vector3.Distance(currentWayPointToMove, checkingPoint); // todo: needs to be optimized

					if (distance <= 1f) // todo insert unit size?
					{
						var direction = checkingPoint - currentWayPointToMove;
						currentWayPointToMove -= direction * distance;

						resultWaypoints[i] = currentWayPointToMove;
					}
				}

				//Debug.DrawRay(currentWayPointToMove, Vector3.up, Color.magenta, 1f);
			}


			return resultWaypoints;
		}
		
		
		/// <summary>Returns nearest to unit waypoint and removes it from waypoints list (to prevent selection of this point by other unit)</summary>
		public static Vector3 GetNearestWaypointToUnit(Vector3 unitPos, List<Vector3> waypoints)
		{
			var selectedWaypoint = waypoints[0];
			var minimumDistance = (unitPos - selectedWaypoint).sqrMagnitude;
			int waypointToRemoveId = 0;

			for (int i = 1; i < waypoints.Count; i++)
			{
				var currentDistance = (unitPos - waypoints[i]).sqrMagnitude;

				if (currentDistance <= minimumDistance)
				{
					minimumDistance = currentDistance;
					selectedWaypoint = waypoints[i];
					waypointToRemoveId = i;
				}
			}

			waypoints.RemoveAt(waypointToRemoveId);
			
			return selectedWaypoint;
		}

		/// <summary> Uses combined method of offsetted positions and square formations. </summary>
		public static List<Vector3> GetWaypointsCominedMethods(Vector3 destination, List<Unit> units)
		{
			var identicalWaypoints = GetWaypointsForUnitsGroup(destination, units);
			var squareWaypoints = GetWaypointsForUnitsGroupSquare(destination, units.Count);
			var finalWaypoints = new List<Vector3>();

			//if (units[0].data.moveType == UnitData.MoveType.Flying)
			//	return squareWaypoints;
			
			for (int i = 0; i < units.Count; i++)
			{
				var finalPosition = GetNearestWaypointToUnit(identicalWaypoints[i], squareWaypoints);
				
				finalWaypoints.Add(finalPosition);
			}

			return finalWaypoints;
		}
	}
}