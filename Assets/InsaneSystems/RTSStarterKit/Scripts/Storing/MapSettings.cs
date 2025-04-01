using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace InsaneSystems.RTSStarterKit
{
	[CreateAssetMenu(fileName = "MapSettings", menuName = "RTS Starter Kit/Map Settings")]
	public class MapSettings : ScriptableObject
	{
		[Tooltip("Name of scene object of map. Note that this scene object should be added to Build Settings to work properly.")]
		public string mapSceneName;
		[Tooltip("Max players count which can play on this map. This value should be similar (or smaller) to map Player Start Points count.")]
		[Range(1, 16)] public int maxMapPlayersCount = 4;
		public Sprite mapPreviewImage;
		[Range(16, 1024)] public int mapSize = 256;

		[Tooltip("If you need ambience sound or music on your map, place sounds in this array, and them will be played randomly during game match on this map.")]
		public AudioClip[] ambientSoundsTracks;

		[Header("Singleplayer parameters")]
		[Tooltip("Mark this toggle, if you're working on singleplayer and this map should work as singleplayer (run without lobby).")]
		public bool isThisMapForSingleplayer;
		[Tooltip("Setup all players parameters on this map. You can set colors, teams, etc. Note that one player should be non-AI.")]
		public List<PlayerSettings> playersSettingsForSingleplayer;
		[Tooltip("Do you want game to auto-spawn player command center buildings on default map spawn points on start? Set this to false to singleplayer games, if you have custom player base.")]
		public bool autoSpawnPlayerStabs = true;

		public void LoadMap()
		{
			SceneManager.LoadScene(mapSceneName);
		}
	}
}
