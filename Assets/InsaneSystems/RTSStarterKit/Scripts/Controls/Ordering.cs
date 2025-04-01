using System.Collections.Generic;
using InsaneSystems.RTSStarterKit.UI;
using UnityEngine;
using UnityEngine.EventSystems;

namespace InsaneSystems.RTSStarterKit.Controls
{
	public static class Ordering
	{
		public static void GiveOrder(Vector2 screenPosition, bool isAdditive)
		{
			if (EventSystem.current.IsPointerOverGameObject())
				return;
			
			if (Selection.selectedUnits.Count == 0 || Selection.selectedUnits[0].data.isBuilding)
				return;

			var ray = GameController.cachedMainCamera.ScreenPointToRay(screenPosition);
			RaycastHit hit;

			if (Physics.Raycast(ray, out hit, 1000))
			{
				var unit = hit.collider.GetComponent<Unit>();
				Order order = null;

				if (unit)
				{
					if (!GameController.instance.playersController.IsPlayersInOneTeam(unit.OwnerPlayerId, Player.localPlayerId))
					{
						order = new AttackOrder();
						(order as AttackOrder).attackTarget = unit;
						SpawnEffect(hit.point, GameController.instance.MainStorage.attackOrderEffect);
					}
					else
					{
						order = new FollowOrder();
					
						(order as FollowOrder).followTarget = unit;
						SpawnEffect(hit.point, GameController.instance.MainStorage.moveOrderEffect);

						var carryModule = unit.GetModule<CarryModule>();
						if (unit.data.canCarryUnitsCount > 0 && carryModule && carryModule.CanCarryOneMoreUnit())
							carryModule.PrepareToCarryUnits(Selection.selectedUnits);
					}
				}
				else
				{
					order = new MovePositionOrder();
					(order as MovePositionOrder).movePosition = hit.point;
					SpawnEffect(hit.point, GameController.instance.MainStorage.moveOrderEffect);
				}

				SendOrderToSelection(order, isAdditive);
			}
		}

		public static void GiveMapOrder(Vector2 mapPoint)
		{
			if (Selection.selectedUnits.Count == 0 || Selection.selectedUnits[0].data.isBuilding)
				return;
			
			var worldPoint = Minimap.GetMapPointInWorldCoords(mapPoint);
			
			var ray = new Ray(worldPoint + Vector3.up * 100f, Vector3.down);
			RaycastHit hit;

			if (Physics.Raycast(ray, out hit, 1000))
			{
				var order = new MovePositionOrder();
				
				(order as MovePositionOrder).movePosition =  hit.point;
				SpawnEffect(hit.point, GameController.instance.MainStorage.moveOrderEffect);
				
				SendOrderToSelection(order, false);
			}
		}

		static void SpawnEffect(Vector3 position, GameObject effect)
		{
			GameObject.Instantiate(effect, position, Quaternion.identity);
		}

		static void SendOrderToSelection(Order order, bool isAdditive)
		{
			SendOrderToUnits(Selection.selectedUnits, order, isAdditive);
		}

		public static void SendOrderToUnits(List<Unit> units, Order order, bool isAdditive)
		{
			var wayPoints = new List<Vector3>();

			bool isMovePositionOrder = order.GetType() == typeof(MovePositionOrder);

			var usedFormation = GameController.instance.MainStorage.unitsFormation;
			var movePosition = Vector3.zero;

			if (order is MovePositionOrder)
				movePosition = (order as MovePositionOrder).movePosition;
			if (order is FollowOrder)
				movePosition = (order as FollowOrder).followTarget.transform.position;
				
			if (usedFormation == UnitsFormation.Default)
				wayPoints = UnitsFormations.GetWaypointsForUnitsGroup(movePosition, units);
			else if (usedFormation == UnitsFormation.SquarePredict)
				wayPoints = UnitsFormations.GetWaypointsCominedMethods(movePosition, units);
			
			for (int i = 0; i < units.Count; i++)
			{
				var personalOrderForUnit = order.Clone();
				var customMoveOrder = false;

				if (order is FollowOrder && units[i].data.moveType == UnitData.MoveType.Flying)
				{
					personalOrderForUnit = new MovePositionOrder();
					customMoveOrder = true;
				}

				if (isMovePositionOrder || customMoveOrder)
					(personalOrderForUnit as MovePositionOrder).movePosition = wayPoints[i];

				units[i].AddOrder(personalOrderForUnit, isAdditive, i == 0);
			}
		}
	}
}