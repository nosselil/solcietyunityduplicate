using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace InsaneSystems.RTSStarterKit.Triggers
{
	public class TriggerEditor : EditorWindow
	{
		static TriggerController triggerController;
		static TriggerController[] foundTriggerControllers;

		Vector2 scrollPosition;

		[MenuItem("RTS Starter Kit/Trigger Editor", priority = 1)]
		static void Init()
		{
			TriggerEditor window = (TriggerEditor)EditorWindow.GetWindow(typeof(TriggerEditor));
			window.titleContent = new GUIContent("RTS Trigger Editor");
			window.Show();
		}

		private void OnGUI()
		{
			if (!triggerController)
			{
				foundTriggerControllers = FindObjectsOfType<TriggerController>();

				if (foundTriggerControllers.Length == 0)
				{
					var triggerControllerObject = new GameObject("TriggerController");
					foundTriggerControllers = new TriggerController[1];
					foundTriggerControllers[0] = triggerControllerObject.AddComponent<TriggerController>();
				}

				triggerController = foundTriggerControllers[0];
			}

			if (foundTriggerControllers.Length > 1)
				EditorGUILayout.HelpBox("Several Trigger Controllers found on scene. Your scene should have only one TriggerController.", MessageType.Warning);

			scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, InsaneEditorStyles.paddedBoxStyle);

			var editor = Editor.CreateEditor(triggerController);
			editor.OnInspectorGUI();

			EditorGUILayout.EndScrollView();
		}
	}
}