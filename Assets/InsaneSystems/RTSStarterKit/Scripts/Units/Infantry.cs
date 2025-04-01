using InsaneSystems.RTSStarterKit.Misc;
using UnityEngine;

namespace InsaneSystems.RTSStarterKit
{
	/// <summary> This module allows to implement infantry features. </summary>
	public class Infantry : Module
	{
		void Start()
		{
			selfUnit.GetModule<Damageable>().damageableDiedEvent += OnDie;
		}
		
		void OnDie(Unit unit)
		{
			var animations = selfUnit.GetModule<AnimationsModule>();

			if (animations)
			{
				var animatorObject = animations.GetAnimator().gameObject;
				
				animatorObject.transform.SetParent(null);
				var timedRemover = animatorObject.AddComponent<TimedObjectDestructor>();
				timedRemover.SetCustomTime(5f); // will remove corpses after 5 seconds.
			}
		}
	}
}