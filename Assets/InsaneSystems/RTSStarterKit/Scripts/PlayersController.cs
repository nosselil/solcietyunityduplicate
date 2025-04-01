using System.Collections.Generic;
using UnityEngine;

namespace InsaneSystems.RTSStarterKit
{
	public class PlayersController : MonoBehaviour
	{
		public List<Player> playersIngame { get; protected set; }

		public static event Production.ProductionAction productionModuleAddedToPlayer;

		/// <summary>Should be called it from GameController's Awake before players initialized. </summary>
		public void PreAwake()
		{
			Production.productionModuleSpawned += AddProductionBuildingToPlayer;
		}

		void AddProductionBuildingToPlayer(Production productionModule)
		{
			var playerOwner = productionModule.selfUnit.OwnerPlayerId;

			if (!playersIngame[playerOwner].playerProductionBuildings.Contains(productionModule))
			{
				playersIngame[playerOwner].AddProduction(productionModule);
				
				if (productionModuleAddedToPlayer != null)
					productionModuleAddedToPlayer.Invoke(productionModule);
			}
		}

		public bool IsPlayersInOneTeam(byte playerAId, byte playerBId)
		{
			if (playersIngame.Count <= playerAId || playersIngame.Count <= playerBId)
				return false;

			return playersIngame[playerAId].teamIndex == playersIngame[playerBId].teamIndex;
		}

		public void AddPlayer(Player player)
		{
			if (playersIngame == null)
				playersIngame = new List<Player>();

			player.id = (byte)playersIngame.Count;
			playersIngame.Add(player);
		}

		void OnDestroy()
		{
			Production.productionModuleSpawned -= AddProductionBuildingToPlayer;
		}
	}
}