using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Watermelon
{
    [CreateAssetMenu(fileName = "Levels Database", menuName = "Data/Level System/Levels Database")]
    public class LevelsDatabase : ScriptableObject
    {
        [SerializeField, LevelEditorSetting] List<LevelData> levels = new List<LevelData>();
        public List<LevelData> Levels => levels;

        [Space]
        [Header("Data")]
        [SerializeField] List<CameraData> cameraData = new List<CameraData>();
        public List<CameraData> CameraData => cameraData;

        [SerializeField] List<BoosterData> boostersData = new List<BoosterData>();
        public List<BoosterData> BoostersData => boostersData;

        [SerializeField] List<ObstacleData> obstaclesData = new List<ObstacleData>();
        public List<ObstacleData> ObstaclesData => obstaclesData;

        [SerializeField] List<EnemyData> enemiesData = new List<EnemyData>();
        public List<EnemyData> EnemiesData => enemiesData;

        [SerializeField] List<FinishData> finishData = new List<FinishData>();
        public List<FinishData> FinishData => finishData;

        [Space(5)]
        [SerializeField] List<RoadData> roadsData = new List<RoadData>();
        public List<RoadData> RoadsData => roadsData;

        [SerializeField] List<EnvironmentData> environmentData = new List<EnvironmentData>();
        public List<EnvironmentData> EnvironmentData => environmentData;

        [SerializeField] List<GateData> gatesData = new List<GateData>();
        public List<GateData> GatesData => gatesData;

        [Space]
        [Header("Characters")]
        [SerializeField] List<CharacterData> charactersData = new List<CharacterData>();
        public List<CharacterData> CharactersData => charactersData;

        [SerializeField] List<GunData> gunsData = new List<GunData>();
        public List<GunData> GunsData => gunsData;

        [Space]
        [Header("Bonus Stages")]
        [SerializeField] List<BonusStageData> bonusStages = new List<BonusStageData>();
        public List<BonusStageData> BonusStages => bonusStages;

        [Space]
        [Header("Spawn Settigns")]
        [SerializeField] float defaultEnvironmentSpawnPositionZ = 0f;
        [SerializeField] float defaultPlayerSpawnPositionZ = 5f;
        [SerializeField] float defaultContentSpawnPositionZ = 15f;
        [SerializeField] float defaultContentOffsetZ = 10f;

        [LevelDataPicker(LevelDataType.Camera)]
        [SerializeField] string defaultCameraDataId = "default_camera";

        public float DefaultEnvironmentSpawnPositionZ => defaultEnvironmentSpawnPositionZ;
        public float DefaultPlayerSpawnPositionZ => defaultPlayerSpawnPositionZ;
        public float DefaultContentSpawnPositionZ => defaultContentSpawnPositionZ;
        public float DefaultContentOffsetZ => defaultContentOffsetZ;
        public string DefaultCameraDataId => defaultCameraDataId;


        public CharacterData GetCharacterData(string id)
        {
            for (int i = 0; i < charactersData.Count; i++)
            {
                CharacterData data = charactersData[i];

                if (data.Id == id)
                {
                    return data;
                }
            }

            return null;
        }

        public GunData GetGunData(string id)
        {
            for (int i = 0; i < gunsData.Count; i++)
            {
                GunData data = gunsData[i];

                if (data.Id == id)
                {
                    return data;
                }
            }

            return null;
        }

        public CameraData GetCameraData(string id)
        {
            for (int i = 0; i < cameraData.Count; i++)
            {
                CameraData data = cameraData[i];

                if (data.Id == id)
                {
                    return data;
                }
            }

            return null;
        }

        public GateData GetGateData(GateType type)
        {
            return gatesData.Find(g => g.GateType == type);
        }

        public BoosterData GetBoosterData(BoosterType type)
        {
            return boostersData.Find(b => b.BoosterType == type);
        }

        public EnemyData GetEnemyData(string id)
        {
            return enemiesData.Find(e => e.Id == id);
        }

        public ObstacleData GetObstacleData(string id)
        {
            return obstaclesData.Find(o => o.Id == id);
        }

        public RoadData GetRoadData(string id)
        {
            return roadsData.Find(r => r.Id == id);
        }

        public EnvironmentData GetEnvironmentData(string id)
        {
            return environmentData.Find(r => r.Id == id);
        }
    }
}
