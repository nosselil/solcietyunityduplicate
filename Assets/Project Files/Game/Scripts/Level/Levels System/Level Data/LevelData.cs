using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Watermelon
{
    public class LevelData : ScriptableObject
    {
        [SerializeField, LevelEditorSetting] string note; //Level editor field

        [Header("Settings")]
        [SerializeField] MovementModeType movementMode = MovementModeType.Classic;
        public MovementModeType MovementMode => movementMode;

        [LevelDataPicker(LevelDataType.Camera)]
        [SerializeField] string cameraDataId;
        public string CameraDataId => cameraDataId;

        [Space(5)]
        [SerializeField] CurrencyType rewardCurrency = CurrencyType.Money;
        public CurrencyType RewardCurrency => rewardCurrency;

        [SerializeField] int rewardAmount = 20;
        public int RewardAmount => rewardAmount;

        [Space(5)]
        [SerializeField] float playerSpawnPositionZ;
        public float PlayerSpawnPositionZ => playerSpawnPositionZ;

        [SerializeField] CharacterType characterType;
        public CharacterType CharacterType => characterType;

        [SerializeField] GameObject overrideBonusStage;
        public GameObject OverrideBonusStage => overrideBonusStage;

        [Header("Content")]
        [SerializeField, LevelEditorSetting] List<GateLevelData> gatesData = new List<GateLevelData>();
        public List<GateLevelData> GatesData => gatesData;

        [SerializeField, LevelEditorSetting] List<BoosterLevelData> boostersData = new List<BoosterLevelData>();
        public List<BoosterLevelData> BoostersData => boostersData;

        [SerializeField, LevelEditorSetting] List<ObstacleLevelData> obstaclesData = new List<ObstacleLevelData>();
        public List<ObstacleLevelData> ObstaclesData => obstaclesData;

        [SerializeField, LevelEditorSetting] List<EnemyLevelData> enemiesData = new List<EnemyLevelData>();
        public List<EnemyLevelData> EnemiesData => enemiesData;

        [SerializeField, LevelEditorSetting] List<FinishLevelData> finishLevelData = new List<FinishLevelData>();
        public List<FinishLevelData> FinishLevelData => finishLevelData;

        [Header("Environment")]
        [SerializeField, LevelEditorSetting] List<RoadLevelData> roadsData = new List<RoadLevelData>();
        public List<RoadLevelData> RoadsData => roadsData;

        [SerializeField, LevelEditorSetting] List<EnvironmentLevelData> environmentLevelData = new List<EnvironmentLevelData>();
        public List<EnvironmentLevelData> EnvironmentLevelData => environmentLevelData;
    }

}