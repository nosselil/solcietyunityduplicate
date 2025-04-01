using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

namespace InsaneSystems.RTSStarterKit.Triggers
{
	[CustomEditor(typeof(TriggerController))]
	public class TriggerControllerEditor : Editor
	{
		public override void OnInspectorGUI()
		{
			GUILayout.Label("Trigger editor", InsaneEditorStyles.headerTextStyle);

			var triggerController = target as TriggerController;
			var triggerDatasProperty = serializedObject.FindProperty("triggerDatas");
			
			for (int i = 0; i < triggerDatasProperty.arraySize; i++)
			{
				var triggerDataProperty = triggerDatasProperty.GetArrayElementAtIndex(i);
				var triggerTypeProperty = triggerDataProperty.FindPropertyRelative("triggerType");
				var triggerTextIdProperty = triggerDataProperty.FindPropertyRelative("triggerTextId");
				var triggerProperty = triggerDataProperty.FindPropertyRelative("trigger");

				GUILayout.Label(triggerTextIdProperty.stringValue != "" ? triggerTextIdProperty.stringValue : "New trigger", InsaneEditorStyles.smallHeaderTextStyle);

				EditorGUI.BeginChangeCheck();
				EditorGUILayout.PropertyField(triggerTypeProperty, true);

				var triggerName = triggerTypeProperty.enumNames[triggerTypeProperty.enumValueIndex];

				if (EditorGUI.EndChangeCheck())
				{
					if (triggerProperty.objectReferenceValue)
						DestroyImmediate((triggerProperty.objectReferenceValue as TriggerBase).gameObject);

					if (triggerName != "None")
					{
						var type = GetAssemblyType("InsaneSystems.RTSStarterKit.Triggers." + triggerName + "Trigger");

						var targetGO = new GameObject(triggerName);
						targetGO.transform.SetParent(triggerController.gameObject.transform);

						var addedComponent = targetGO.AddComponent(type);
						triggerProperty.objectReferenceValue = addedComponent;
					}
				}

				if (triggerName != "None" && triggerName != "")
					EditorGUILayout.PropertyField(triggerTextIdProperty, true);

				if (triggerProperty.objectReferenceValue)
				{
					GUILayout.Label("Trigger parameters", EditorStyles.boldLabel);
					var editor = Editor.CreateEditor(triggerProperty.objectReferenceValue);
					editor.DrawDefaultInspectorWithoutScriptField();
				}

				GUI.color = new Color(1f, 0.8f, 0.8f, 1f);
				if (GUILayout.Button("Delete trigger"))
				{
					if (triggerProperty.objectReferenceValue)
						DestroyImmediate((triggerProperty.objectReferenceValue as TriggerBase).gameObject);

					triggerDatasProperty.DeleteArrayElementAtIndex(i);
				}
				GUI.color = Color.white;

				//EditorGUILayout.PropertyField(triggerProperty, true);
				InsaneEditorStyles.DrawUILine(Color.gray, 1, 20);
			}
			
			if (GUILayout.Button("Add trigger"))
			{
				triggerDatasProperty.InsertArrayElementAtIndex(triggerDatasProperty.arraySize);

				var triggerDataProperty = triggerDatasProperty.GetArrayElementAtIndex(triggerDatasProperty.arraySize - 1);
				var triggerTypeProperty = triggerDataProperty.FindPropertyRelative("triggerType");
				var triggerProperty = triggerDataProperty.FindPropertyRelative("trigger");

				triggerTypeProperty.enumValueIndex = 0;
				triggerProperty.objectReferenceValue = null;
			}

			serializedObject.ApplyModifiedProperties();
			serializedObject.Update();
			EditorUtility.SetDirty(target);
			Repaint();
		}

		public static Type GetAssemblyType(string typeName)
		{
			var type = Type.GetType(typeName);
			if (type != null)
				return type;

			foreach (var a in AppDomain.CurrentDomain.GetAssemblies())
			{
				type = a.GetType(typeName);
				if (type != null)
					return type;
			}

			return null;
		}
	}

	public static class InsaneEditorExtension
	{
		public static bool DrawDefaultInspectorWithoutScriptField(this Editor Inspector)
		{
			EditorGUI.BeginChangeCheck();

			Inspector.serializedObject.Update();

			SerializedProperty Iterator = Inspector.serializedObject.GetIterator();
			Iterator.NextVisible(true);

			while (Iterator.NextVisible(false))
				EditorGUILayout.PropertyField(Iterator, true);

			Inspector.serializedObject.ApplyModifiedProperties();

			return EditorGUI.EndChangeCheck();
		}
	}
}