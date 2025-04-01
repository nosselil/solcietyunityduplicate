using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace InsaneSystems.RTSStarterKit
{
	[CustomEditor(typeof(Attackable))]
	public class AttackableEditor : Editor
	{
		public override void OnInspectorGUI()
		{
			var attackable = target as Attackable;

			if (attackable.ShootPoints.Length == 0 || attackable.ShootPoints[0] == null)
				EditorGUILayout.HelpBox("Setup at least one shoot point, otherwise this unit will be not able to shoot. Shoot point is point, where unit bullet/shell will be spawned from. Add it to end of gun barrel. Also don't forget to setup correct forward position of point.", MessageType.Warning);

			DrawDefaultInspector();
		}
	}
}