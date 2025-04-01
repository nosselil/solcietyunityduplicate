using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace InsaneSystems.RTSStarterKit
{
	[CustomEditor(typeof(Unit))]
	public class UnitEditor : Editor
	{
		public override void OnInspectorGUI()
		{
			DrawDefaultInspector();

			var unit = target as Unit;
			GUIContent content;

			var richStyle = new GUIStyle();
			richStyle.richText = true;

			GUILayout.Label("<b>Add modules to unit:</b>", richStyle);

			GUI.enabled = !unit.GetComponent<Damageable>();
			content = new GUIContent("Damageable", "It will add component with unit health. Otherwise this unit will be invulnerable.");

			if (GUILayout.Button(content))
				unit.gameObject.AddComponent<Damageable>();

			GUI.enabled = !unit.GetComponent<Attackable>();
			content = new GUIContent("Attackable", "It will add component, allows unit to attack enemies");

			if (GUILayout.Button(content))
					unit.gameObject.AddComponent<Attackable>();

			GUI.enabled = !unit.GetComponent<Production>();
			content = new GUIContent("Production", "It will add component, allows unit to produce buildings or units.");

			if (GUILayout.Button(content))
				unit.gameObject.AddComponent<Production>();

			GUI.enabled = !unit.GetComponent<Harvester>();
			content = new GUIContent("Harvester", "It will add component, allows unit to gather resources.");

			if (GUILayout.Button(content))
				unit.gameObject.AddComponent<Harvester>();

			GUI.enabled = !unit.GetComponent<Refinery>();
			content = new GUIContent("Refinery", "It will add component, allows unit to receive gathered by Harvester resources. We recommend to use this component only on buildings.");

			if (GUILayout.Button(content))
				unit.gameObject.AddComponent<Refinery>();
		}
	}
}