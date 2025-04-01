using UnityEngine;

namespace InsaneSystems.RTSStarterKit.Abilities
{
	/* obsolete
	public class ChangeWeapon : Ability
	{
		public override void CustomAction()
		{
			isActive = false;

			var attackable = unitOwner.GetModule<Attackable>();

			if (attackable)
			{
				attackable.customAttackDistance = Data.newAttackRange;
				attackable.customDamage = Data.newAttackDamage;
				attackable.customReloadTime = Data.newAttackReloadTime;
			}

			unitOwnerAbilities.GetAbility(Data.customWeaponAbilityToEnable).isActive = true;
		}
	}
	*/
	
	[CreateAssetMenu(fileName = "ChangeWeapon", menuName = "RTS Starter Kit/Abilities/Change Weapon")]
	public class ChangeWeapon : Ability
	{
		[Header("Custom weapon ability")]
		[Tooltip("Attack distance of this weapon. If you set 0, it will be default unit attack distance. Other for next same parameters.")]
		public float newAttackRange;
		public float newAttackReloadTime;
		public float newAttackDamage;
		[Tooltip("Put here second weapon change ability, which should became active after using this - to change weapon to previous.")]
		public Ability customWeaponAbilityToEnable;
		
		protected override void StartUseAction()
		{
			isActive = false;

			var attackable = unitOwner.GetModule<Attackable>();

			if (attackable)
			{
				attackable.customAttackDistance = newAttackRange;
				attackable.customDamage = newAttackDamage;
				attackable.customReloadTime = newAttackReloadTime;
			}

			var anotherAbility = unitOwnerAbilities.GetAbility(customWeaponAbilityToEnable);
			
			if (anotherAbility)
				anotherAbility.isActive = true;
		}
	}
}