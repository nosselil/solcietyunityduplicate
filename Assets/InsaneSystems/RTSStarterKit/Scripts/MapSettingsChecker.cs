using UnityEngine;

namespace InsaneSystems.RTSStarterKit
{
	public class MapSettingsChecker : MonoBehaviour
	{
		public MapSettings editingMapSettings;

		void OnDrawGizmos()
		{
			if (!editingMapSettings)
				return;

			Gizmos.color = Color.cyan;

			Gizmos.DrawWireCube(Vector3.zero + Vector3.one * editingMapSettings.mapSize / 2f, Vector3.one * editingMapSettings.mapSize);
		}
	}
}