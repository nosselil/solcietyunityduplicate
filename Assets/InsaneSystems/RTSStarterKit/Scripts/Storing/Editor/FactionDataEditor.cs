using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace InsaneSystems.RTSStarterKit
{
	[CustomEditor(typeof(FactionData))]
	public class FactionDataEditor : Editor
	{
		public override void OnInspectorGUI()
		{
			DrawDefaultInspector();
			EditorGUILayout.HelpBox("Note that factions are in preview, so in next versions their functionality will be improved and extended, and possible bugs will be fixed.", MessageType.Info);
		}
	}
}