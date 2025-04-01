using System.Collections.Generic;
using UnityEngine;

namespace InsaneSystems.RTSStarterKit
{
	[CreateAssetMenu(fileName = "Storage", menuName = "RTS Starter Kit/Storage")]
	public class Storage : ScriptableObject
	{
		[Header("Default Game Settings")]
		[Tooltip("Here you should add all created Map Settings objects, otherwise map don't appear in maps list ingame.")]
		public List<MapSettings> availableMaps;
		public List<ProductionCategory> availableProductionCategories;
		[Tooltip("List of all available fractions in game.")]
		public List<FactionData> availableFactions;
		[Tooltip("List of all available for player colors. You can add new colors or remove existing.")]
		public List<Color> availablePlayerColors;
		
		[Tooltip("Money value, which will be added to each player on game match start.")]
		[Range(0, 100000)] public int startPlayerMoney = 10000;
		[Tooltip("This field contains maximum building distance. Player will be able to create buildings only in this radius from start point.")]
		[Range(10, 1000)] public int maxBuildDistance = 40;
		public bool allowBuildingsRotation = true;
		public bool useGridForBuildingMode = true;
		public bool allowCameraRotation = true;
		public bool allowCameraZoom = true;
		[Tooltip("This parameter means, will be visible black borders outside the map bounds or not.")]
		public bool showMapBorders = true;
		[Tooltip("If you don't need automatic NavMeshObstacle component addition to your buildings, turn this off.")]
		public bool addNavMeshObstacleToBuildings = true;
		[Tooltip("Units formation type. Default formation keeps units positions same as it was before order, Square Predict is better for square formations.")]
		public UnitsFormation unitsFormation = UnitsFormation.Default;
		
		[Header("Gameplay - Electricity")]
		[Tooltip("Check this, if your game uses electricity 'model' of gameplay. It means that some buildings uses electricity to work, and there exists some powerplants which gives electricity.")]
		public bool isElectricityUsedInGame;
		[Tooltip("Speed decrease value when electricity limit is reached. If you set 1, there will be original production speed (100%), if you set 0.5, it will be 50%. So set 0 to pause production until electricity will be restored.")]
		[Range(0f, 1f)] public float speedCoefForProductionsWithoutElectricity = 1f;

		[Header("Gameplay - Fog of War")]
		[Tooltip("Is For of War used in the game? If yes, check this toggle. Note that fog of war can be expensive for performance in games with big units count.")]
		public bool isFogOfWarOn = true;
		[Tooltip("Delay between updates of fog of war visual part. Smaller values can cause bad performance, but better quality.")]
		[Range(0f, 0.5f)] public float fowUpdateDelay = 0.05f;

		[Header("Gameplay - other")]
		[Tooltip("Health being restored per one second of building repair.")]
		public int buildingRepairHealthPerSecond = 5;
		[Tooltip("Money cost for one second of building repair.")]
		public int buildingRepairCostPerSecond = 2;
		[Tooltip("How much player will receive for selling building (in percents of default price).")]
		[Range(0f, 1f)] public float buildingSellReturnPercents = 0.5f;

		[Header("UI Settings")]
		[Tooltip("This parameter affects Carrier Module UI. Count of carried units icons on carrier unit healthbar will be limitied with this value. 0 means no icons will be shown.")]
		[Range(0, 10)] public int carriedUnitsIconsCount = 4;
		[Tooltip("Max count of units icons in multiselection interface panel. 0 for no limit. Note that high limit values or 0 can cause some lags on huge units count selection.")]
		[Range(0, 80)] public int unitIconsLimitInMultiselectionUI = 20;

		[Tooltip("Should game hotkeys be shown on UI elements to which them belong to? Units and abilities icons, for example. ")]
		public bool showHotkeysOnUI = true;
		
		[Header("Game Objects")]
		[Tooltip("Prefab of selection indicator shown on unit when unit selected. By default it is circle effect.")]
		public GameObject selectionIndicatorTemplate;
		[Tooltip("Prefab of move order effect. Spawn when player clicks on map to give selected units order to move.")]
		public GameObject moveOrderEffect;
		[Tooltip("Prefab of attack order effect. Spawn when player clicks on enemy to give selected units order to attack.")]
		public GameObject attackOrderEffect;
		[Tooltip("Prefab of repair effect. Being spawned when player click building to repair.")]
		public GameObject repairEffectTemplate;

		[Header("UI Templates")]
		[Tooltip("This and other templates used by asset UI elements. Keep it default or customize if your want.")]
		public GameObject unitMinimapIconTemplate;
		public GameObject healthbarTemplate;
		public GameObject productionButtonTemplate;
		public GameObject productionNumberButtonTemplate;
		public GameObject unitProductionIconTemplate;
		public GameObject unitMultiselectionIconTemplate;
		public GameObject harvesterBarTemplate;
		public GameObject minimapSignalTemplate;
		public GameObject unitCarryingIcon;
		public GameObject unitAbilityIcon;
		public GameObject carryCellTemplate;

		[Header("Cursors")]
		[Tooltip("Default cursor used in game. You can setup different cursors for different actions.")]
		public Texture2D defaultCursour;
		public Texture2D attackCursour;
		public Texture2D gatherResourcesCursour;
		public Texture2D giveResourcesCursour;
		[Tooltip("Cursor when player trying to do something that not allowed/not possible.")]
		public Texture2D restrictCursour;
		[Tooltip("Cursor when player ordering units to move using minimap.")]
		public Texture2D mapOrderCursor;
		public Texture2D repairCursor, sellCursor;
		
		[Header("Menu UI Templates")]
		public GameObject playerEntry;
		
		[Header("Misc")]
		public Material playerColorMaterialTemplate;

		[Tooltip("Link to Sounds Library data file, which will be used by game.")]
		public SoundLibrary soundLibrary;
		[Tooltip("Link to Texts Library data file, which will be used by game.")]
		public TextsLibrary textsLibrary;

		[Header("Layers")]
		[Tooltip("Units layer. Used for calculations in asset code.")]
		public LayerMask unitLayerMask;
		[Tooltip("List of layers which will be obstacle for shooting units when aiming target.")]
		public LayerMask obstaclesToUnitShoots;
		[Tooltip("List of layers which will be obstacle for shooting units when aiming target.")]
		public LayerMask obstaclesToUnitShootsWithoutUnitLayer;

	

		public MapSettings GetMapBySceneName(string name)
		{
			for (int i = 0; i < availableMaps.Count; i++)
				if (availableMaps[i].mapSceneName == name)
					return availableMaps[i];

			throw new System.Exception("No map with name " + name + " found!");
		}
	}

	public enum UnitsFormation
	{
		Default,
		SquarePredict
	}
}