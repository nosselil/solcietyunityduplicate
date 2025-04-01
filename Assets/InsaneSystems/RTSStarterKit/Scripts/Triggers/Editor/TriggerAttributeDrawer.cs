using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace InsaneSystems.RTSStarterKit.Triggers
{
	[CustomPropertyDrawer(typeof(TriggerAttribute))]
	public class TriggerPropertyDrawer : PropertyDrawer
	{
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			var triggerController = GameObject.FindObjectOfType<TriggerController>();
			if (!triggerController)
			{
				GUI.Label(position, "No Trigger Controller on scene. Create it first.");
				return;
			}

			if (property.propertyType != SerializedPropertyType.String)
			{
				GUI.Label(position, "This attribute can be used only on string variables.");
				return;
			}

			if (triggerController.GetTriggersCount() == 0)
			{
				GUI.Label(position, "No triggers available. Setup at least one.");
				return;
			}

			var currentIndex = triggerController.GetTriggerIndexByName(property.stringValue);

			if (currentIndex < 0)
				currentIndex = 0;

			var names = triggerController.GetTriggersNames();

			int newIndex = EditorGUI.Popup(position, property.displayName, currentIndex, names);
			property.stringValue = triggerController.GetNameByIndex(newIndex);
		}
	}
}