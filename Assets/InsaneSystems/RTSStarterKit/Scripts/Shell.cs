using UnityEngine;

namespace InsaneSystems.RTSStarterKit
{
	public class Shell : MonoBehaviour
	{
		[SerializeField] [Range(0, 10000)] protected float damage = 50f;
		[SerializeField] [Range(0, 1000)] protected float flySpeed = 3f;
		[SerializeField] [Range(0, 30)] protected float lifeTime = 5f;
		[Tooltip("If true, this shell will fly like auto-aiming missile, following attack target. Otherwise it can miss target.")]
		[SerializeField] bool autoAim;
		[Header("Artillery settings")]
		[SerializeField] bool isArtilleryShell;
		[SerializeField] float maxHeight = 10f;

		[SerializeField] GameObject explosionEffect;
		
		/// <summary> Unit, which shooted this shell. Note that if this unit dead, this field will be null. </summary>
		public Unit ownerUnit { get; private set; }
		
		Vector3 startPosition;
		float currentFlyTime;
		float currentHeight;

		byte playerOwner;
		Transform target;
		Unit targetUnitComponent;

		UnitData selfUnitData;

		float artilleryFlyTime = 1f;
		Vector3 firstTargetPosition;

		void Start()
		{
			startPosition = transform.position;
		}

		void Update()
		{
			var deltaTime = Time.deltaTime;
			
			Fly(deltaTime);

			lifeTime -= deltaTime;

			if (lifeTime <= 0)
				Destroy(gameObject);
		}

		protected virtual void Fly(float deltaTime)
		{
			if (isArtilleryShell)
			{
				transform.position = Vector3.Lerp(startPosition, firstTargetPosition, currentFlyTime / artilleryFlyTime);

				currentHeight = Mathf.Sin((currentFlyTime / artilleryFlyTime) * Mathf.PI) * maxHeight;
				transform.position = new Vector3(transform.position.x, startPosition.y + currentHeight, transform.position.z);

				currentFlyTime += deltaTime;
			}
			else
			{
				transform.position += transform.forward * (flySpeed * deltaTime);
			}

			if (autoAim && target)
			{
				var targetPosition = target.position;

				if (targetUnitComponent)
					targetPosition.y += targetUnitComponent.GetSize().y / 2f;

				transform.rotation = Quaternion.LookRotation(targetPosition - transform.position);
			}
		}

		protected virtual void OnTriggerEnter(Collider other)
		{
			Shell otherShell = other.GetComponent<Shell>();

			if (otherShell)
				return;

			var unit = other.GetComponent<Unit>();
		
			if (unit)
			{
				if (selfUnitData.allowShootThroughUnitObstacles && unit != targetUnitComponent)
					return;

				var damageable = unit.GetModule<Damageable>();

				if (!damageable || unit.IsInMyTeam(playerOwner))
					return;

				damageable.TakeDamage(damage);
				DestroyAction();
			}
			else if (isArtilleryShell)
			{
				DestroyAction();
			}
		}

		void DestroyAction()
		{
			if (explosionEffect)
				Instantiate(explosionEffect, transform.position, Quaternion.identity);

			Destroy(gameObject);
		}

		public void SetUnitOwner(Unit owner)
		{
			ownerUnit = owner;
			playerOwner = owner.OwnerPlayerId;
			selfUnitData = owner.data;
		}

		public void SetCustomDamage(float damageValue) { damage = damageValue; }

		public void SetTarget(Unit target)
		{
			this.target = target.transform;
			targetUnitComponent = target;

			firstTargetPosition = target.transform.position;
		}
	}
}