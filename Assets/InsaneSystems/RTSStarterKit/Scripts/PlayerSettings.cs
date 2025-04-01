using UnityEngine;

namespace InsaneSystems.RTSStarterKit
{
	[System.Serializable]
	public class PlayerSettings
	{
		public string nickName = "Player";
		public byte team;
		public Color color = Color.white;
		public bool isAI;
		public FactionData selectedFaction;
		[Range(0, 100000)] public int startMoneyForSingleplayer = 10000;

		public PlayerEntry playerLobbyEntry;

		public PlayerSettings(byte team, Color color, bool isAI = false)
		{
			this.team = team;
			this.color = color;
			this.isAI = isAI;
		}
	}
}