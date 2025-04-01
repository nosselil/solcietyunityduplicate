using UnityEngine;

namespace InsaneSystems.RTSStarterKit
{
	public class Damageable : Module
	{
		public static event OnDamageableDied onDamageableDied;
		public static event DamageableTakeDamage damageableTakeDamageEvent;

		public event OnDamageableDied damageableDiedEvent;

		public delegate void OnDamageableDied(Unit unitOwner);
		public delegate void DamageableTakeDamage(Unit damagedUnit, float damageValue);

		public float health { get; protected set; }

		protected void Start()
		{
			health = selfUnit.data.maxHealth;

			OnStart();
		}

		protected virtual void OnStart() { }

		public virtual void TakeDamage(float value)
		{
			health = Mathf.Clamp(health - value, 0, selfUnit.data.maxHealth);

			if (damageableTakeDamageEvent != null)
				damageableTakeDamageEvent.Invoke(selfUnit, value);

			if (selfUnit.OwnerPlayerId == Player.localPlayerId)
				UI.UIController.instance.minimapSignal.ShowFor(selfUnit);

			if (health <= 0)
				Die();
		}
		
		public virtual void AddHealth(float value)
		{
			if (value <= 0)
				return;
			
			health = Mathf.Clamp(health + value, 0, selfUnit.data.maxHealth);

			if (damageableTakeDamageEvent != null)
				damageableTakeDamageEvent.Invoke(selfUnit, value);
		}

		public float GetHealthPercents() { return health / selfUnit.data.maxHealth; }

		public virtual void Die()
		{
			PlayDeathEffect();

			Destroy(gameObject);

			if (onDamageableDied != null)
				onDamageableDied.Invoke(selfUnit);

			if (damageableDiedEvent != null)
				damageableDiedEvent.Invoke(selfUnit);
		}

		protected virtual void PlayDeathEffect()
		{
			if (selfUnit.data.explosionEffect)
				Instantiate(selfUnit.data.explosionEffect, transform.position, transform.rotation);
		}
	}
}