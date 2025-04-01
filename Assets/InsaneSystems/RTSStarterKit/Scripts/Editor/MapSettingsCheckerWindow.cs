using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace InsaneSystems.RTSStarterKit
{
	public class MapSettingsCheckerWindow : EditorWindow
	{
		List<string> warnings = new List<string>();

		[MenuItem("RTS Starter Kit/Map Settings Checker")]
		public static void ShowWindow()
		{
			EditorWindow.GetWindow(typeof(MapSettingsCheckerWindow), false, "Map Checker");
		}

		void OnGUI()
		{
			var gameController = FindObjectOfType<GameController>();
			var playerStartPoints = FindObjectsOfType<PlayerStartPoint>();

			//if (!(target as MapSettingsChecker).editingMapSettings)
			//	EditorGUILayout.HelpBox("You can add current map settings to this field to see map borders in Scene Window.", MessageType.Info);

			if (!gameController)
			{
				AddWarning("No GameController found on scene. Do you added SceneBase prefab?");
			}
			else
			{
				if (!gameController.MainStorage)
					AddWarning("GameController has no Storage added. Please, add Storage object from resources, otherwise game will not work correctly.");
				if (!gameController.MapSettings)
					AddWarning("GameController has no MapSettings added. Please, add this map MapSettings asset from resources, otherwise game can work wrong.");
			}

			if (playerStartPoints.Length == 0)
				AddWarning("No PlayerStartPoint object found on scene. Don't forget at least one, otherwise player will be not able to start the game on this map. Minimum points count, required for map to work fine - 2.");

			var sceneObjects = GetSceneObjects();
			var terrainObjects = GetObjectsInLayer(sceneObjects, LayerMask.NameToLayer("Terrain"));

			if (terrainObjects.Count == 0)
				AddWarning("Level has no objects in Terrain layer. Level ground object should have this layer to allow player create buildings.");

			if (warnings.Count > 0)
			{
				for (int i = 0; i < warnings.Count; i++)
					EditorGUILayout.HelpBox(warnings[i], MessageType.Warning);
			}
			else
			{
				EditorGUILayout.HelpBox("Looks like your level setted up correctly.", MessageType.Info);
			}

			warnings = new List<string>();
		}

		void AddWarning(string warning)
		{
			warnings.Add(warning);
		}

		private static List<GameObject> GetObjectsInLayer(List<GameObject> inputObjects, int layer)
		{
			var objectsInLayer = new List<GameObject>();
			foreach (GameObject obj in inputObjects)
				if (obj.gameObject.layer == layer)
					objectsInLayer.Add(obj.gameObject);

			return objectsInLayer;
		}

		private static List<GameObject> GetSceneObjects()
		{
			return new List<GameObject>(FindObjectsOfType<GameObject>()).FindAll(go => go.hideFlags == HideFlags.None);
		}
	}
}