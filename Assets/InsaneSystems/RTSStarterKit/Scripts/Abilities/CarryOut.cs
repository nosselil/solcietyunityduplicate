using InsaneSystems.RTSStarterKit.Controls;
using UnityEngine;

namespace InsaneSystems.RTSStarterKit.Abilities
{
	[CreateAssetMenu(fileName = "CarryOut", menuName = "RTS Starter Kit/Abilities/Carry Out")]
	public class CarryOut : Ability
	{
		protected override void StartUseAction()
		{
			if (Selection.selectedUnits.Count == 0) return;

			for (int i = 0; i < Selection.selectedUnits.Count; i++)
			{
				var carryModule = Selection.selectedUnits[i].GetModule<CarryModule>();

				if (carryModule) carryModule.ExitAllUnits();
			}
		}
	}
}