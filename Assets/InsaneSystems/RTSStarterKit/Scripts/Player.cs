using System.Collections.Generic;
using UnityEngine;

namespace InsaneSystems.RTSStarterKit
{
	[System.Serializable]
	public class Player 
	{
		public static byte localPlayerId = 0;

		public static event PlayerMoneyChangedAction localPlayerMoneyChangedEvent;
		public static event PlayerElectricityChangedAction localPlayerElectricityChangedEvent;

		public delegate void PlayerMoneyChangedAction(int newMoneyValue);
		public delegate void PlayerElectricityChangedAction(int totalElectricity, int usedElectricity);

		public string userName;
		public Color color;
		public FactionData selectedFaction;
		public byte id;
		public byte teamIndex;

		public int money = 10000;
		public int electricity, usedElectricity;

		public bool isAIPlayer;
		public bool isDefeated = false;

		public List<Production> playerProductionBuildings = new List<Production>();

		public readonly Material playerMaterial;

		public Player(Color color)
		{
			playerProductionBuildings = new List<Production>();

			this.color = color;

			playerMaterial = new Material(GameController.instance.MainStorage.playerColorMaterialTemplate);
			playerMaterial.color = color;
		}

		public bool IsHaveMoney(int amount, List<ResourceRequirement> ResourceRequirements)
		{
			bool hasMoney = money >= amount;
			bool hasExternalResources = true;
			if(ResourceRequirements != null && ResourceRequirements.Count > 0)
			{
				for (int i = 0; i < ResourceRequirements.Count; i++)
				{
					GameResourceData gameResoureData = GameResourcesManager.instance.FindResourceData(ResourceRequirements[i].gameResource);
					if(gameResoureData.CurrentAmount < ResourceRequirements[i].ResourceRequired)
					{
                        hasExternalResources =false;
						break;
                    }
				}
			}

			if(hasMoney && hasExternalResources)
			{
                for (int i = 0; i < ResourceRequirements.Count; i++)
                {
					GameResourcesManager.instance.RemoveResource(ResourceRequirements[i].ResourceRequired, ResourceRequirements[i].gameResource);
                }
            }
            return hasMoney && hasExternalResources;
		}

		public void AddMoney(int amount)
		{
			money += amount;

			if (IsLocalPlayer() && localPlayerMoneyChangedEvent != null)
				localPlayerMoneyChangedEvent.Invoke(money);
		}

		public void SpendMoney(int amount)
		{
			money = Mathf.Clamp(money - amount, 0, 1000000);

			if (IsLocalPlayer() && localPlayerMoneyChangedEvent != null)
				localPlayerMoneyChangedEvent.Invoke(money);
		}
		public void AddElectricity(int amount)
		{
			electricity += amount;

			if (IsLocalPlayer() && localPlayerElectricityChangedEvent != null)
				localPlayerElectricityChangedEvent.Invoke(electricity, usedElectricity);
		}

		public void RemoveElectricity(int amount)
		{
			electricity -= amount;

			if (IsLocalPlayer() && localPlayerElectricityChangedEvent != null)
				localPlayerElectricityChangedEvent.Invoke(electricity, usedElectricity);
		}

		public void AddUsedElectricity(int amount)
		{
			usedElectricity += amount;

			if (IsLocalPlayer() && localPlayerElectricityChangedEvent != null)
				localPlayerElectricityChangedEvent.Invoke(electricity, usedElectricity);
		}

		public void RemoveUsedElectricity(int amount)
		{
			usedElectricity = Mathf.Clamp(usedElectricity - amount, 0, 9999);

			if (IsLocalPlayer() && localPlayerElectricityChangedEvent != null)
				localPlayerElectricityChangedEvent.Invoke(electricity, usedElectricity);
		}

		public float GetElectricityUsagePercent() { return usedElectricity / (float)electricity; }
		public bool IsLocalPlayer() { return id == localPlayerId; }
		public void AddProduction(Production production) { playerProductionBuildings.Add(production); }
		public void RemoveProduction(Production production) { playerProductionBuildings.Remove(production); }

		public void DefeatPlayer()
		{
			if (isDefeated)
				return;

			isDefeated = true;

			for (int i = Unit.allUnits.Count - 1; i >= 0; i--)
			{
				if (Unit.allUnits[i] && Unit.allUnits[i].OwnerPlayerId == id)
				{
					var damageable = Unit.allUnits[i].GetModule<Damageable>();

					if (damageable)
						damageable.TakeDamage(99999);
				}
			}

			if (IsLocalPlayer())
				UI.UIController.instance.ShowDefeatScreen();
		}

		public List<Production> GetProductionBuildingsByCategory(ProductionCategory category)
		{
			var resultList = new List<Production>();

			for (int i = 0; i < playerProductionBuildings.Count; i++)
				if (playerProductionBuildings[i] != null && playerProductionBuildings[i].GetProductionCategory == category)
					resultList.Add(playerProductionBuildings[i]);

			return resultList;
		}

		public bool IsHaveProductionOfCategory(ProductionCategory category)
		{ 
			return GetProductionBuildingsByCategory(category).Count > 0;
		}

		public List<Player> GetEnemyPlayers()
		{
			var allPlayers = GameController.instance.playersController.playersIngame;
			var enemyPlayers = new List<Player>();

			for (int i = 0; i < allPlayers.Count; i++)
				if (allPlayers[i] != this && allPlayers[i].teamIndex != teamIndex)
					enemyPlayers.Add(allPlayers[i]);

			return enemyPlayers;
		}

		public static Player GetPlayerById(byte playerId) { return GameController.instance.playersController.playersIngame[playerId]; }
		public static Player GetLocalPlayer() { return GetPlayerById(localPlayerId); }
	}
}