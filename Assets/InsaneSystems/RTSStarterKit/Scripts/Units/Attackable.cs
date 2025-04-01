using System.Collections.Generic;
using UnityEngine;

namespace InsaneSystems.RTSStarterKit
{
	public class Attackable : Module
	{
		const float enemySearchDelayTime = 0.2f, maxEnemySearchRadius = 18f;

		public event AttackAction startAttackEvent, stopAttackEvent, shootEvent;

		[SerializeField] Transform[] shootPoints = new Transform[1];

		public Unit attackTarget { get; protected set; }
		public Transform[] ShootPoints { get { return shootPoints; } }
		public Transform CurrentShootPoint { get { return shootPoints[currentShootPoint]; } }

		public float customDamage { get; set; }
		public float customAttackDistance { get; set; }
		public float customReloadTime { get; set; }
		public GameObject customShell { get; set; }

		float currentReloadTime;
		bool attackingTarget;
		int currentShootPoint;
		float enemySearchDelay, targetSearchDelay;

		readonly Collider[] objectsInSearchRadius = new Collider[100];
		Unit[] unitComponentsFound = new Unit[1];

		float squaredAttackDistance;

		int unitLayermask;
		int obstaclesToShootLayerMask, obstaclesToShootWithoutUnitLayerMask;

		public delegate void AttackAction();

		void Start()
		{
			if (!selfUnit.data.hasAttackModule)
				Debug.LogWarning("[Attack module] Unit " + name + " has disabled Has Attack Module toggle, but Attack Module still added to prefab. Fix this.");
			
			squaredAttackDistance = selfUnit.data.attackDistance * selfUnit.data.attackDistance;
			unitLayermask = GameController.instance.MainStorage.unitLayerMask;
			obstaclesToShootLayerMask = GameController.instance.MainStorage.obstaclesToUnitShoots;
			obstaclesToShootWithoutUnitLayerMask = GameController.instance.MainStorage.obstaclesToUnitShootsWithoutUnitLayer;
		}

		void Update()
		{
			var deltaTime = Time.deltaTime;
			
			if (enemySearchDelay > 0)
				enemySearchDelay -= deltaTime;
			else
				SearchForEnemy();

			if (targetSearchDelay > 0)
				targetSearchDelay -= deltaTime;
			else
				SearchForTarget();

			HandleAttack(deltaTime);
		}

		protected virtual void SearchForTarget()
		{
			if (selfUnit.HasOrders() || attackTarget != null)
				return;
			
			float objectsCount = Physics.OverlapSphereNonAlloc(transform.position, maxEnemySearchRadius, objectsInSearchRadius, unitLayermask);

			for (int i = 0; i < objectsCount; i++)
			{
				unitComponentsFound = objectsInSearchRadius[i].GetComponents<Unit>();

				if (unitComponentsFound.Length == 0)
					continue;

				var unit = unitComponentsFound[0];

				if (selfUnit.IsInMyTeam(unit) || unit == selfUnit || !unit.GetModule<FogOfWarModule>().isVisibleInFOW || !CanAttackTargetByMoveType(unit))
					continue;

				var order = new AttackOrder
				{
					attackTarget = unit
				};
				
				selfUnit.AddOrder(order, false);
				
				break;
			}

			targetSearchDelay = enemySearchDelayTime;
		}
		
		protected virtual void SearchForEnemy()
		{
			if (attackTarget != null) 
				return;

			float objectsCount = Physics.OverlapSphereNonAlloc(transform.position, GetAttackDistance(), objectsInSearchRadius, unitLayermask);

			for (int i = 0; i < objectsCount; i++)
			{
				unitComponentsFound = objectsInSearchRadius[i].GetComponents<Unit>();

				if (unitComponentsFound.Length == 0)
					continue;

				Unit unit = unitComponentsFound[0];

				if (selfUnit.IsInMyTeam(unit) || unit == selfUnit || !CanAttackTargetByMovePossibility(unit.transform) || !CanAttackTargetByMoveType(unit))
					continue;

				SetTarget(unit);
				break;
			}

			enemySearchDelay = enemySearchDelayTime;
		}

		void HandleAttack(float deltaTime)
		{
			if (!attackTarget)
			{
				StopAttack(false);
				return;
			}

			if (!CanAttackTargetByMoveState())
			{
				StopAttack(false);
				return;
			}

			if (selfUnit.HasOrders() && selfUnit.orders[0] is AttackOrder)
			{
				var orderTarget = ((AttackOrder) selfUnit.orders[0]).attackTarget;

				if (orderTarget != attackTarget && IsTargetInAttackRange(orderTarget) && CanAttackTargetByMoveType(orderTarget))
					attackTarget = orderTarget;
			}

			if (!IsTargetInAttackRange(attackTarget))
			{
				StopAttack(true);
				return;
			}

			if ((!selfUnit.tower || !selfUnit.tower.CanRotateToTarget(attackTarget.transform)) && selfUnit.movable && selfUnit.data.stillTryRotateToTargetWhenNoAimNeeded)
			{
				var targetSameYPosition = attackTarget.transform.position;
				targetSameYPosition.y = transform.position.y;

				var rotationToTarget = Quaternion.LookRotation(targetSameYPosition - transform.position);

				transform.rotation = Quaternion.Lerp(transform.rotation, rotationToTarget, deltaTime * 5f);
			}

			if (currentReloadTime > 0)
			{
				currentReloadTime -= deltaTime;
				return;
			}

			if (IsFireLineFree(attackTarget) && IsTurretAimedToTarget(attackTarget))
				DoShoot();
		}

		void StopAttack(bool removeTarget = false)
		{
			if (removeTarget)
				attackTarget = null;

			attackingTarget = false;

			if (stopAttackEvent != null)
				stopAttackEvent.Invoke();
		}

		void DoShoot()
		{
			var curShootPoint = shootPoints[currentShootPoint];
			var spawnedObject = Instantiate(GetShellTemplate(), curShootPoint.position, curShootPoint.rotation);
			var shell = spawnedObject.GetComponent<Shell>();

			if (shell)
			{
				shell.SetUnitOwner(selfUnit);
				shell.SetTarget(attackTarget);

				if (selfUnit.data.usedDamageType == UnitData.UsedDamageType.UseCustomDamageValue)
					shell.SetCustomDamage(selfUnit.data.attackDamage);

				if (customDamage > 0)
					shell.SetCustomDamage(customDamage);
			}

			selfUnit.PlayShootSound();

			if (selfUnit.data.shootEffect)
				Instantiate(selfUnit.data.shootEffect, curShootPoint.position, curShootPoint.rotation);
		
			currentReloadTime = selfUnit.data.reloadTime;

			if (customReloadTime > 0)
				currentReloadTime = customReloadTime;

			if (currentShootPoint < shootPoints.Length - 1)
				currentShootPoint++;
			else
				currentShootPoint = 0;

			if (!attackingTarget && startAttackEvent != null)
				startAttackEvent.Invoke();
			
			if (shootEvent != null)
				shootEvent.Invoke();
			
			attackingTarget = true;
		}

		/// <summary> Requires unit to shoot now in current target without reload and attack conditions check. Use only if you know why you need it. </summary>
		public void DoCustomShoot(GameObject newCustomShell = null)
		{
			if (newCustomShell)
				customShell = newCustomShell;
			
			DoShoot();

			if (newCustomShell)
				customShell = null;
		}

		void SetTarget(Unit target) { attackTarget = target; }

		public virtual bool IsTargetInAttackRange(Unit target)
		{
			if (!target)
				return false;

			return (transform.position - target.transform.position).sqrMagnitude <= GetSquaredAttackDistance();
		}

		public virtual bool IsFireLineFree(Unit target)
		{
			if (!target)
				return false;

			if (selfUnit.data.allowShootThroughAnyObstacles)
				return true;

			var targetCenter = target.GetCenterPoint();

			var ray = new Ray(CurrentShootPoint.position, targetCenter - CurrentShootPoint.position);
			RaycastHit hit;

			// in first case will check only static obstacles, otherwise will check unit obstacles too. If no obstacles returns true. If obstacle is target also returns true.
			var layerMask = selfUnit.data.allowShootThroughUnitObstacles ? obstaclesToShootWithoutUnitLayerMask : obstaclesToShootLayerMask;

			if (Physics.Raycast(ray, out hit, GetAttackDistance(), layerMask))
				return hit.collider.gameObject == target.gameObject;

			return true;
		}

		public virtual bool IsTurretAimedToTarget(Unit target)
		{
			if (!selfUnit.data.needAimToTargetToShoot)
				return true;

			if (!selfUnit.tower)
			{
				Vector3 otherSameToSelfYPosition = target.transform.position;
				otherSameToSelfYPosition.y = transform.position.y;
				var selfForwardNoY = transform.forward;
				selfForwardNoY.y = 0;

				Vector3 toOther = (otherSameToSelfYPosition - transform.position).normalized;
				return Vector3.Angle(selfForwardNoY, toOther) < 3f;

				/* old version
				RaycastHit hit;

				Vector3 checkFrom = selfUnit.transform.position;
				// checkFrom.y = target.transform.position.y; // this is more correct due to units can be in different height levels. But somehow it works wrong
				// Debug.DrawRay(checkFrom, selfUnit.transform.forward * 10f, Color.red, 0.2f);

				if (Physics.Raycast(checkFrom, selfUnit.transform.forward, out hit, selfUnit.data.attackDistance, unitLayermask))
					return hit.collider.transform == target.transform;

				return false;
				*/
			}

			return selfUnit.tower.IsTurretAimedToTarget(target.GetComponent<BoxCollider>());
		}

		public virtual bool CanAttackTargetByMoveState()
		{
			if (!selfUnit.tower && selfUnit.movable && selfUnit.movable.isMoving)
				return false;
			if (selfUnit.tower && selfUnit.movable && selfUnit.movable.isMoving && !selfUnit.data.canMoveWhileAttackingTarget)
				return false;

			return true;
		}

		public bool CanAttackTargetByMoveType(Unit target)
		{
			var attackPossibility = selfUnit.data.attackPossibility;
			if ((attackPossibility == UnitData.AttackPossibility.Land || attackPossibility == UnitData.AttackPossibility.LandAndAir) && target.data.moveType == UnitData.MoveType.Ground)
				return true;
			if ((attackPossibility == UnitData.AttackPossibility.Air || attackPossibility == UnitData.AttackPossibility.LandAndAir) && target.data.moveType == UnitData.MoveType.Flying)
				return true;

			return false;
		}

		bool CanAttackTargetByMovePossibility(Transform target)
		{
			return (selfUnit.tower && selfUnit.tower.CanRotateToTarget(target)) || selfUnit.movable;
		}

		public virtual bool CanMove()
		{
			if (!selfUnit.tower && attackingTarget)
				return false;

			if (!selfUnit.data.canMoveWhileAttackingTarget && attackingTarget)
				return false;

			return true;
		}

		public void SetShootPoints(List<Transform> shootPointsTransforms) { shootPoints = shootPointsTransforms.ToArray(); }

		public float GetAttackDistance()
		{
			if (customAttackDistance > 0)
				return customAttackDistance;

			return selfUnit.data.attackDistance;
		}

		public float GetSquaredAttackDistance()
		{
			if (customAttackDistance > 0)
				return customAttackDistance * customAttackDistance;

			return squaredAttackDistance;
		}

		GameObject GetShellTemplate()
		{
			if (customShell)
				return customShell;

			return selfUnit.data.attackShell;
		}
		
		void OnDrawGizmosSelected()
		{
			Unit selfUnitTemporary = GetComponent<Unit>();
			UnitData selfData = null;

			if (selfUnitTemporary)
				selfData = selfUnitTemporary.data;

			if (selfData != null)
			{
				Gizmos.color = Color.red;
				Gizmos.DrawWireSphere(transform.position, selfData.attackDistance);
			}
		}

		void OnDrawGizmos()
		{
			if (shootPoints.Length == 0)
				return;

			for (int i = 0; i < shootPoints.Length; i++)
			{
				if (shootPoints[i] == null)
					continue;

				var endPoint = shootPoints[i].transform.position + shootPoints[i].transform.forward;

				Gizmos.color = Color.yellow;

				Gizmos.DrawLine(shootPoints[i].transform.position, endPoint);
				Gizmos.DrawLine(endPoint, endPoint - shootPoints[i].transform.forward / 3f - shootPoints[i].transform.right / 5f);
				Gizmos.DrawLine(endPoint, endPoint - shootPoints[i].transform.forward / 3f + shootPoints[i].transform.right / 5f);
			}
		}
	}
}