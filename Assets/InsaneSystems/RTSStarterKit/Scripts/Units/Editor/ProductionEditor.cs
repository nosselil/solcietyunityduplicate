using UnityEditor;

namespace InsaneSystems.RTSStarterKit
{
	[CustomEditor(typeof(Production))]
	public class ProductionEditor : Editor
	{
		public override void OnInspectorGUI()
		{
			var production = target as Production;
			var categoryIdProperty = serializedObject.FindProperty("categoryId");
			var spawnPointProperty = serializedObject.FindProperty("spawnPoint");
			var wayPointProperty = serializedObject.FindProperty("moveWaypoint");
			
			EditorGUILayout.PropertyField(categoryIdProperty);
			
			if (!production.SpawnPoint) 
				EditorGUILayout.HelpBox("If this production creates units (not buildings), so you need to setup spawn point. This can be simple transform point, where units will spawn.", MessageType.Info);

			EditorGUILayout.PropertyField(spawnPointProperty);
			EditorGUILayout.PropertyField(wayPointProperty);
			
			
			serializedObject.ApplyModifiedProperties();
		}
	}
}
