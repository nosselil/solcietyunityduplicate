using InsaneSystems.RTSStarterKit.UI;
using UnityEngine;

namespace InsaneSystems.RTSStarterKit.Abilities
{
	/// <summary> Base class for any ability. Derive from it. </summary>
	public abstract class Ability : ScriptableObject
	{
		[Tooltip("Ability name. Shown in game interface.")]
		public string abilityName;
		[Tooltip("Ability icon image. Shown in game interface.")]
		public Sprite icon;
		[Sound] public AudioClip soundToPlayOnUse;
		
		[Tooltip("Is ability can be used by default? If false, it can be enabled only from other code or ability (upgrades for example).")]
		public bool isActiveByDefault = true;
		
		// realtime gameplay parameters, changes every run
		public Unit unitOwner { get; protected set; }
		public bool isActive { get; set; }
		protected AbilitiesModule unitOwnerAbilities { get; private set; }
		
		public void Init(Unit unitOwner)
		{
			this.unitOwner = unitOwner;
			unitOwnerAbilities = unitOwner.GetModule<AbilitiesModule>();
			
			isActive = isActiveByDefault;
			
			InitAction();
		}

		/// <summary> Do here ability initialization. It being called once on unit spawn. </summary>
		protected virtual void InitAction() { }

		public void Update()
		{
			UpdateAction();
		}
		
		/// <summary> Do here ability action. </summary>
		protected virtual void UpdateAction() { }

		public void StartUse()
		{
			if (!CanUse()) return;
			
			StartUseAction();
			
			UIController.instance.unitAbilities.Redraw();
		}
		
		protected virtual void StartUseAction() { }
		
		public virtual bool CanUse() { return true; }
	}
}