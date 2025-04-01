using System.Collections.Generic;
using UnityEngine;
using InsaneSystems.RTSStarterKit.Controls;
	
namespace InsaneSystems.RTSStarterKit.AI
{
	/// <summary>
	/// AI Controller of specific AI Player. Each AI player has it own AI Controller. It controls basic AI logic - units, building base, etc
	/// 
	/// <para>Here you can code any new logic of AI. But in next updates there will be possibility to create derived classes from this one.</para>
	/// </summary>
	public class AIController : MonoBehaviour
	{
		[SerializeField] AISettings aiSettings;
		AISettings ownAISettings;

		byte selfPlayerId = 0;

		float thinkTimer;

		bool isPlayerSettedUp;

		#region Building parameters
		Production selfCommandCenter;
		readonly List<Unit> finishedBuildings = new List<Unit>();
		readonly List<Unit> selfUnits = new List<Unit>();
		#endregion

		#region Units parameters
		readonly List<UnitsGroup> unitGroups = new List<UnitsGroup>();
		#endregion

		void Awake()
		{
			Unit.unitSpawnedEvent += OnUnitSpawned;
		}

		void Update()
		{
			if (!isPlayerSettedUp)
				return;

			ownAISettings.delayBeforeStartBuyingUnits -= Time.deltaTime;
			ownAISettings.delayBeforeStartCreateBuildings -= Time.deltaTime;

			if (thinkTimer > 0)
			{
				thinkTimer -= Time.deltaTime;
				return;
			}

			DoAction();

			thinkTimer = aiSettings.thinkTime;
		}

		void DoAction()
		{
			HandleBuilding();
			HandleUnitsBuilding();
			HandleUnitsControls();
		}

		void HandleBuilding()
		{
			if (ownAISettings.delayBeforeStartCreateBuildings > 0 || !selfCommandCenter)
				return;

			for (int i = 0; i < aiSettings.buildingPriority.Length; i++)
			{
				var currentBuilding = aiSettings.buildingPriority[i];

				if (currentBuilding == null)
					continue;

				if (!selfCommandCenter.IsUnitOfTypeInQueue(currentBuilding) && finishedBuildings.Find(unit => unit.data == currentBuilding) == null)
				{
					selfCommandCenter.AddUnitToQueue(currentBuilding);
					break;
				}
			}

			if (selfCommandCenter.unitsQueue.Count > 0 && selfCommandCenter.IsBuildingReady())
			{
				var commandCenterPosition = selfCommandCenter.transform.position;
				var randomedOffset = new Vector3(Random.Range(-20, 20), 0, Random.Range(-20, 20));
				var finalPoint = commandCenterPosition + randomedOffset;

				var currentBuilding = selfCommandCenter.unitsQueue[0];
				var buildingSize = Build.GetBuildingSize(currentBuilding);
				var buildingRotation = Quaternion.Euler(0, 180, 0);
				
				var canBuild = Build.CheckZoneToBuild(finalPoint, buildingSize, buildingRotation, selfPlayerId);

				if (canBuild)
				{
					var spawnedBuildingObject = Build.CreateBuilding(currentBuilding.selfPrefab, finalPoint, buildingRotation, selfPlayerId);
					var spawnedBuildingUnit = spawnedBuildingObject.GetComponent<Unit>();
					finishedBuildings.Add(spawnedBuildingUnit);
					selfCommandCenter.RemoveUnitFromQueue(currentBuilding, false);
				}
			}
		}

		void HandleUnitsBuilding()
		{
			if (ownAISettings.delayBeforeStartBuyingUnits > 0)
				return;

			var playerAI = Player.GetPlayerById(selfPlayerId);

			if (playerAI.GetEnemyPlayers().Count == 0)
				return;

			var selectedUnitCategory = aiSettings.unitsCategories[Random.Range(0, aiSettings.unitsCategories.Length)];

			var allSuitableProductions = playerAI.GetProductionBuildingsByCategory(selectedUnitCategory);

			if (allSuitableProductions.Count == 0)
				return;

			var randomedProduction = allSuitableProductions[Random.Range(0, allSuitableProductions.Count)];

			var attackableUnits = selectedUnitCategory.availableUnits.FindAll(unitData => unitData.hasAttackModule); 
			var randomedUnitToBuild = attackableUnits[Random.Range(0, attackableUnits.Count)];

			randomedProduction.AddUnitToQueue(randomedUnitToBuild);
		}

		void OnUnitSpawned(Unit unit)
		{
			if (unit.OwnerPlayerId != selfPlayerId)
				return;

			if (unit.GetComponent<Harvester>())
			{
				// you can do any harvester things here, but this unit not needed in attack groups, so it is a reason for return directive below.
				return;
			}

			selfUnits.Add(unit);

			bool isNewGroupNeeded = true;
			int selectedGroup = unitGroups.Count; ;
			for (int i = 0; i < unitGroups.Count; i++)
			{
				if (unitGroups[i].IsGroupNeedsUnits())
				{
					selectedGroup = i;
					isNewGroupNeeded = false;
					break;
				}
			}

			if (isNewGroupNeeded)
				unitGroups.Add(new UnitsGroup());

			unitGroups[selectedGroup].AddUnit(unit);
		}

		void HandleUnitsControls()
		{
			if (unitGroups.Count == 0)
				return;

			var enemyPlayers = Player.GetPlayerById(selfPlayerId).GetEnemyPlayers();

			bool isAIHaveProductions = Player.GetPlayerById(selfPlayerId).playerProductionBuildings.Count > 0; // if AI have no productions, groups will be always not full, so in this case not full units groups should attack.

			var fullUnitGroups = unitGroups.FindAll(unitGroup => (!unitGroup.IsGroupNeedsUnits() || !isAIHaveProductions) && !unitGroup.IsGroupHaveOrder());

			if (fullUnitGroups.Count == 0 || enemyPlayers.Count == 0)
				return;

			var randomlySelectedGroup = fullUnitGroups[Random.Range(0, fullUnitGroups.Count)];
			var selectedEnemy = enemyPlayers[Random.Range(0, enemyPlayers.Count)];
			var selectedTarget = Unit.allUnits.Find(unit => unit.OwnerPlayerId == selectedEnemy.id);

			var attackOrder = new AttackOrder();
			attackOrder.attackTarget = selectedTarget;

			randomlySelectedGroup.AddOrderToGroup(attackOrder);
		}

		public void SetupWithAISettings(AISettings newAISettings)
		{
			aiSettings = newAISettings;
			ownAISettings = Instantiate(aiSettings); // cloning AI settings to prevent changes in game resources AI assets.
		}

		public void SetupAIForPlayer(byte playerId)
		{
			selfPlayerId = playerId;

			var aiPlayer = Player.GetPlayerById(playerId);
			if (aiPlayer.playerProductionBuildings.Count > 0)
				selfCommandCenter = aiPlayer.playerProductionBuildings[0];

			isPlayerSettedUp = true;
		}
		
		void OnDestroy() { Unit.unitSpawnedEvent -= OnUnitSpawned; }
	}
}