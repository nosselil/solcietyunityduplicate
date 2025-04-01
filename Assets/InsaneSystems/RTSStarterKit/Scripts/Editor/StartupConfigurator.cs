using UnityEngine;
using System.Collections.Generic;
using UnityEditor;

namespace InsaneSystems.RTSStarterKit
{
	[InitializeOnLoad]
	public class Startup
	{
		static bool isFirstRun = false;

		static Startup()
		{
			if (LayerMask.LayerToName(9) != "Unit")
			{
				isFirstRun = true;
				CreateLayer(9, "Unit");
			}

			if (LayerMask.LayerToName(10) != "Terrain")
				CreateLayer(10, "Terrain");
			if (LayerMask.LayerToName(11) != "FogOfWar")
				CreateLayer(11, "FogOfWar");
			if (LayerMask.LayerToName(12) != "ResourceField")
				CreateLayer(12, "ResourceField");

			if (!PlayerPrefs.HasKey("EditorScenesInitialized"))
			{
				try
				{
					var settings = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);

					var sceneToAdd = new EditorBuildSettingsScene("Assets/InsaneSystems/RTSStarterKit/Scenes/Lobby.unity", true);
					if (sceneToAdd != null && settings.FindAll(sceneSettings => sceneSettings.path == sceneToAdd.path).Count == 0)
						settings.Add(sceneToAdd);

					sceneToAdd = new EditorBuildSettingsScene("Assets/InsaneSystems/RTSStarterKit/Scenes/Example.unity", true);
					if (sceneToAdd != null && settings.FindAll(sceneSettings => sceneSettings.path == sceneToAdd.path).Count == 0)
						settings.Add(sceneToAdd);

					EditorBuildSettings.scenes = settings.ToArray();

					PlayerPrefs.SetInt("EditorScenesInitialized", 1);
				}
				catch (System.Exception ex)
				{
					Debug.LogWarning("Startup configuration failed at scene addition to Build Settings. Error is: " + ex.ToString());
				}
			}

			Debug.Log("Insane Systems RTS Starter Kit Initialized.");
			 
			if (isFirstRun)
			{
				if (UnityEditor.PlayerSettings.colorSpace != ColorSpace.Linear)
				{
					UnityEditor.PlayerSettings.colorSpace = ColorSpace.Linear;
					Debug.Log("Color space set to <b>Linear</b> for better graphics quality.");
				}
			}
			
			isFirstRun = false;
		}

		public static void CreateLayer(int atPosition, string name)
		{
			if (string.IsNullOrEmpty(name))
				throw new System.ArgumentNullException("name", "New layer name string is either null or empty.");

			var tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
			var layerProps = tagManager.FindProperty("layers");
			var propCount = layerProps.arraySize;

			SerializedProperty targetProp = layerProps.GetArrayElementAtIndex(atPosition);

			if (targetProp == null)
			{
				UnityEngine.Debug.LogError("Maximum limit of " + propCount + " layers exceeded. Layer \"" + name + "\" not created.");
				return;
			}

			targetProp.stringValue = name;
			tagManager.ApplyModifiedProperties();
		}
	}
}