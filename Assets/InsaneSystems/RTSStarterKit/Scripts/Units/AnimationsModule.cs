using UnityEngine;

namespace InsaneSystems.RTSStarterKit
{
	/// <summary> This module allows to use animations on units. To enable it work, you need to set checkbox Use Animations in UnitData of your unit to checked. </summary>
	public class AnimationsModule : Module
	{
		[SerializeField] Animator animator;


		static readonly int attackId = Animator.StringToHash("Attack");
		static readonly int moveId = Animator.StringToHash("Move");
		static readonly int dieId = Animator.StringToHash("Die");
		static readonly int harvestId = Animator.StringToHash("Harvest");
		
		void Start()
		{
			if (!selfUnit.data.useAnimations)
			{
				enabled = false;
				return;
			}
			
			if (!animator)
				animator = GetComponent<Animator>();

			if (!animator)
			{
				Debug.LogWarning("Unit " + name + " does not have Animator component! It will have NO animations, if you're not add it.");
				return;
			}

			if (!animator.runtimeAnimatorController && selfUnit.data.animatorController)
				animator.runtimeAnimatorController = selfUnit.data.animatorController;

			var attackable = selfUnit.GetModule<Attackable>();
			attackable.startAttackEvent += OnStartAttack;
			attackable.stopAttackEvent += OnStopAttack;
			
			var movable = selfUnit.GetModule<Movable>();
			movable.startMoveEvent += OnStartMove;
			movable.stopMoveEvent += OnStopMove;
			
			selfUnit.GetModule<Damageable>().damageableDiedEvent += OnDie;
			
			var harvester = selfUnit.GetModule<Harvester>();

			if (harvester)
			{
				harvester.startHarvest += OnStartHarvest;
				harvester.stopHarvest += OnStopHarvest;
			}
		}

		void OnStartAttack() { SetAnimatorBool(attackId, true); }
		void OnStopAttack() { SetAnimatorBool(attackId, false); }
		
		void OnStartMove() { SetAnimatorBool(moveId, true); }
		void OnStopMove() { SetAnimatorBool(moveId, false); }

		void OnStartHarvest() { SetAnimatorBool(harvestId, true); }
		void OnStopHarvest() { SetAnimatorBool(harvestId, false); }
		
		void OnDie(Unit unit) { SetAnimatorBool(dieId, true); }
		
		void SetAnimatorBool(int valueId, bool value)
		{
			if (animator.isActiveAndEnabled)
				animator.SetBool(valueId, value);
		}
		
		public Animator GetAnimator() { return animator; }
	}
}