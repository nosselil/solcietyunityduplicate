using UnityEngine;
using UnityEngine.Serialization;

namespace InsaneSystems.RTSStarterKit
{
	public class Tower : Module
	{
		[FormerlySerializedAs("towerTransform")] [SerializeField] Transform turretTransform;
		[SerializeField] Transform secondAxisGun;

		float timerToNextRandom;
		float randomRotationTime;
		int randomRotateDirection;

		Quaternion secondAxisGunDefaultLocalRotation;

		void Start()
		{
			if (!selfUnit.data.hasTurret)
				Debug.LogWarning("[Tower module] Unit " + name + " has disabled Has Turret toggle, but Tower module still added to prefab. Fix this.");

			if (secondAxisGun)
				secondAxisGunDefaultLocalRotation = secondAxisGun.localRotation;
		}

		void Update()
		{
			RotateTower();
		}

		public bool IsTurretAimedToTarget(Collider target)
		{
			Vector3 otherSameToTowerYPosition = target.transform.position;
			otherSameToTowerYPosition.y = turretTransform.position.y;
			var turretForwardNoY = turretTransform.forward;
			turretForwardNoY.y = 0;

			Vector3 toOther = (otherSameToTowerYPosition - turretTransform.position).normalized;
			return Vector3.Angle(turretForwardNoY, toOther) < 3f;

			/* old version
			RaycastHit hit;
			
			if (Physics.Raycast(selfUnit.attackable.CurrentShootPoint.position, selfUnit.attackable.CurrentShootPoint.forward, out hit, 1000)) // todo check only unit layers
				return hit.collider == target;
				

			return false;
			*/
		}

		void RotateTower()
		{
			if (selfUnit.attackable.attackTarget != null)
			{
				if (!CanRotateToTarget(selfUnit.attackable.attackTarget.transform))
					return;

				Transform target = selfUnit.attackable.attackTarget.transform;

				Vector3 targetPositionSameY = target.position;
				targetPositionSameY.y = turretTransform.position.y;

				Quaternion newRotation = Quaternion.LookRotation(targetPositionSameY - turretTransform.position);

				turretTransform.rotation = Quaternion.RotateTowards(turretTransform.rotation, newRotation, selfUnit.data.turretRotationSpeed);

				if (secondAxisGun)
				{
					var newGunRotation = Quaternion.LookRotation(target.position - secondAxisGun.position);

					secondAxisGun.localRotation = Quaternion.RotateTowards(secondAxisGun.localRotation, newGunRotation, selfUnit.data.turretRotationSpeed);
					secondAxisGun.localEulerAngles = new Vector3(secondAxisGun.localEulerAngles.x, 0f, 0f);
				}
			}
			else if (selfUnit.HasOrders())
			{
				Quaternion newRotation = Quaternion.RotateTowards(turretTransform.rotation, transform.rotation, selfUnit.data.turretRotationSpeed);
				turretTransform.rotation = newRotation;

				RotateSecondAxisGunToDefault();
			}
			else if (!selfUnit.data.limitTurretRotationAngle)
			{
				if (timerToNextRandom <= 0)
				{
					randomRotationTime = Random.Range(0.2f, 1f);
					randomRotateDirection = Random.Range(0, 1);
					timerToNextRandom = 10f;
				}
				else
				{
					timerToNextRandom -= Time.deltaTime;
				}

				if (randomRotationTime > 0)
				{
					randomRotationTime -= Time.deltaTime;
					turretTransform.Rotate(Vector3.up, randomRotateDirection == 0 ? -1f : 1f);
				}

				RotateSecondAxisGunToDefault();
			}
		}

		void RotateSecondAxisGunToDefault()
		{
			if (!secondAxisGun)
				return;

			secondAxisGun.localRotation = Quaternion.RotateTowards(secondAxisGun.localRotation, secondAxisGunDefaultLocalRotation, selfUnit.data.turretRotationSpeed);
			secondAxisGun.localEulerAngles = new Vector3(secondAxisGun.localEulerAngles.x, 0f, 0f);
		}

		public bool CanRotateToTarget(Transform target)
		{
			if (!selfUnit.data.limitTurretRotationAngle)
				return true;

			var targetDirection = (target.position - transform.position).normalized;
			var angleBetween = Mathf.Abs(Vector3.SignedAngle(transform.forward, targetDirection, Vector3.up));

			return angleBetween <= selfUnit.data.maximumTurretRotationAngle;
		}

		public void SetTurretTransform(Transform turretTransform) { this.turretTransform = turretTransform; }
	}
}