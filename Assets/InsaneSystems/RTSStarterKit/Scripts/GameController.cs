using UnityEngine;
using UnityEngine.SceneManagement;

namespace InsaneSystems.RTSStarterKit
{
	[RequireComponent(typeof(PlayersController))]
	public class GameController : MonoBehaviour
	{
		public static GameController instance { get; protected set; }
		public static Camera cachedMainCamera;
		public static bool isGamePaused { get; protected set; }
		
		[SerializeField] Storage storage;
		[SerializeField] AI.AISettings defaultAIPreset;
		[SerializeField] MapSettings mapSettings;
		[SerializeField] Renderer mapBorderRenderer;

		public PlayersController playersController { get; protected set; }	
		public Controls.Build build { get; protected set; }	
		public Controls.CameraMover cameraMover { get; protected set; }
		public TextsLibrary textsLibrary { get; protected set; }

		public Storage MainStorage { get { return storage; } }
		public MapSettings MapSettings { get { return mapSettings; } }

		bool isGameInitialized;
		bool isAiInitialized;

		void Awake()
		{
			instance = this;

			playersController = GetComponent<PlayersController>();
			build = GetComponent<Controls.Build>();
			
			cameraMover = FindObjectOfType<Controls.CameraMover>();
			cachedMainCamera = Camera.main;
			
			playersController.PreAwake();

			if (mapSettings != null && mapSettings.isThisMapForSingleplayer)
			{
				MatchSettings.currentMatchSettings = new MatchSettings();
				MatchSettings.currentMatchSettings.playersSettings = mapSettings.playersSettingsForSingleplayer;
				MatchSettings.currentMatchSettings.SelectMap(mapSettings);
			}
			else if (MatchSettings.currentMatchSettings == null)
			{
				Debug.LogWarning("<b>You can run non-singleplayer map only from Lobby.</b> To test map correctly - save it, open Lobby scene, select players, and run map.");
				SceneManager.LoadScene(0);
				return;
			}

			if (mapSettings == null)
				mapSettings = MatchSettings.currentMatchSettings.selectedMap;

			Controls.Selection.Initialize();

			if (Unit.allUnits != null)
				Unit.allUnits.Clear();

			UI.Healthbar.ResetPool();

			InitializePlayers();

			if (mapBorderRenderer)
			{
				mapBorderRenderer.material.SetInt("_MapSize", MatchSettings.currentMatchSettings.selectedMap.mapSize);

				if (!MainStorage.showMapBorders)
					mapBorderRenderer.enabled = false;
			}

			textsLibrary = storage.textsLibrary;

			if (!textsLibrary)
				Debug.LogWarning("<b>No Texts Library added to the Storage.</b> Please, add it, otherwise possible game texts problems.");
		}

		void Update()
		{
			if (!isGameInitialized)
			{
				isGameInitialized = true;
				UI.UIController.instance.OnGameInitialized();

				if (!isAiInitialized)
					InitializeAI();

				return;
			}

			Controls.Selection.Update();
		}

		void InitializePlayers()
		{
			SpawnController.InitializeStartPoints();

			for (int i = 0; i < MatchSettings.currentMatchSettings.playersSettings.Count; i++)
			{
				var currentPlayerSettings = MatchSettings.currentMatchSettings.playersSettings[i];

				Player player = new Player(currentPlayerSettings.color);
				player.teamIndex = currentPlayerSettings.team;
				player.selectedFaction = currentPlayerSettings.selectedFaction;
				player.isAIPlayer = currentPlayerSettings.isAI;

				if (mapSettings.isThisMapForSingleplayer)
					player.money = currentPlayerSettings.startMoneyForSingleplayer;
				else
					player.money = MainStorage.startPlayerMoney;

				playersController.AddPlayer(player);				
			}

			if (MapSettings.autoSpawnPlayerStabs)
				for (int i = 0; i < playersController.playersIngame.Count; i++)
					SpawnController.SpawnPlayerStab((byte)i);
		}

		void InitializeAI()
		{
			for (int i = 0; i < playersController.playersIngame.Count; i++)
			{
				if (playersController.playersIngame[i].isAIPlayer)
				{
					var aiObject = new GameObject("AI Controller " + i);
					var aiController = aiObject.AddComponent<AI.AIController>();
					aiController.SetupWithAISettings(defaultAIPreset);
					aiController.SetupAIForPlayer((byte)i);
				}
			}

			isAiInitialized = true;
		}

		public void CheckWinConditions()
		{
			var allBuildings = Unit.allUnits.FindAll(unit => unit.data.isBuilding == true);

			for (int i = 0; i < playersController.playersIngame.Count; i++)
			{
				if (!allBuildings.Find(unit => unit.OwnerPlayerId == i))
					playersController.playersIngame[i].DefeatPlayer();
			}

			var allUndefeatedPlayers = playersController.playersIngame.FindAll(player => player.isDefeated == false);

			int winnerTeam = 0;
			for (int i = 0; i < allUndefeatedPlayers.Count; i++)
			{
				if (i == 0)
					winnerTeam = allUndefeatedPlayers[i].teamIndex;
				else if (allUndefeatedPlayers[i].teamIndex != winnerTeam)
					return;
			}

			if (!Player.GetLocalPlayer().isDefeated)
				UI.UIController.instance.ShowWinScreen();
		}


		public void ReturnToLobby()
		{
			Cursors.SetDefaultCursor();
			SceneManager.LoadScene("Lobby");
		}

		void OnDestroy() { instance = null; }
		
		public static void SetPauseState(bool isPaused)
		{
			isGamePaused = isPaused;
			
			Time.timeScale = isGamePaused ? 0f : 1f;
		}
	}
}