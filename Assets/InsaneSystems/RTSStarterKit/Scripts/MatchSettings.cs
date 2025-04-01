using System.Collections.Generic;

namespace InsaneSystems.RTSStarterKit
{
	public class MatchSettings
	{
		public static MatchSettings currentMatchSettings;

		public List<PlayerSettings> playersSettings { get; set; }
		public MapSettings selectedMap { get; protected set; }

		public MatchSettings() { Reset(); }

		public void Reset() { playersSettings = new List<PlayerSettings>(); }

		public void AddPlayerSettings(PlayerSettings playerSettings) { playersSettings.Add(playerSettings); }
		public void RemovePlayerSettingsById(byte id) { playersSettings.RemoveAt(id); }

		public void RemovePlayerSettings(PlayerSettings playerSettings) { playersSettings.Remove(playerSettings); }
		public void SelectMap(MapSettings selectedMap) { this.selectedMap = selectedMap; }
	}
}