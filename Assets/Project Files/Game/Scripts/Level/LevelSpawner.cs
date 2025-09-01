using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Watermelon
{
    public class LevelSpawner
    {
        SkinsManager skinsManager;

        private List<GateBehavior> gates = new List<GateBehavior>();
        private List<BoosterBehavior> boosters = new List<BoosterBehavior>();
        private List<ObstacleBehavior> obstacles = new List<ObstacleBehavior>();
        private List<GameObject> roads = new List<GameObject>();
        private List<EnvironmentBehavior> environments = new List<EnvironmentBehavior>();
        private List<FirstStageFinishBehavior> finishes = new List<FirstStageFinishBehavior>();
        private List<EnemyBehavior> enemies = new List<EnemyBehavior>();

        private BonusStageSave bonusStageSave;
        private BonusStageBehavior bonusStage;
        private List<HorizontalGroup> pickableGroups = new List<HorizontalGroup>();

        public void Init(SkinsManager skinsManager)
        {
            this.skinsManager = skinsManager;

            bonusStageSave = SaveController.GetSaveObject<BonusStageSave>("Bonus Stage Save");
        }

        public void SpawnLevel(LevelData levelData)
        {
            SpawnList(levelData.GatesData, gates, skinsManager.GetGate);
            SpawnList(levelData.ObstaclesData, obstacles, skinsManager.GetObstacle);
            SpawnList(levelData.EnvironmentLevelData, environments, skinsManager.GetEnvironent);
            SpawnList(levelData.RoadsData, roads, skinsManager.GetRoad);
            SpawnList(levelData.FinishLevelData, finishes, skinsManager.GetFinish);
            SpawnList(levelData.EnemiesData, enemies, skinsManager.GetEnemy);
            SpawnList(levelData.BoostersData, boosters, skinsManager.GetBooster);

            for(int i = 0; i < boosters.Count; i++)
            {
                BoosterBehavior booster = boosters[i];

                for(int j = 0; j < obstacles.Count; j++)
                {
                    ObstacleBehavior obstacle = obstacles[j];

                    bool similarX = Mathf.Approximately(booster.transform.position.x, obstacle.transform.position.x);
                    bool similarZ = Mathf.Approximately(booster.transform.position.z, obstacle.transform.position.z);

                    if (similarX && similarZ)
                    {
                        booster.LinkToObstacle(obstacle);

                        break;
                    }
                }
            }

            FirstStageFinishBehavior lastFinish = finishes[^1];
            BonusStageData bonusStageData = LevelController.Database.BonusStages[bonusStageSave.BonusStageId % LevelController.Database.BonusStages.Count];

            GameObject bonusStagePrefab = bonusStageData.Prefab;

            if (levelData.OverrideBonusStage != null)
                bonusStagePrefab = levelData.OverrideBonusStage;

            GameObject bonusStageObject = UnityEngine.Object.Instantiate(bonusStagePrefab) as GameObject;

            bonusStage = bonusStageObject.GetComponent<BonusStageBehavior>();
            bonusStage.Init(bonusStageData, lastFinish);

            CreatePickagleGroups(boosters);
            CreatePickagleGroups(gates);
        }

        private void CreatePickagleGroups<T>(List<T> pickables) where T : IPickableGameplayElement
        {
            for (int i = 0; i < pickables.Count; i++)
            {
                IPickableGameplayElement booster = pickables[i];

                bool added = false;
                for (int j = 0; j < pickableGroups.Count; j++)
                {
                    HorizontalGroup group = pickableGroups[j];
                    if (group.TryAdd(booster))
                    {
                        added = true;
                        break;
                    }
                }

                if (!added)
                {
                    HorizontalGroup group = new HorizontalGroup(booster);
                    pickableGroups.Add(group);
                }
            }
        }

        private void SpawnList<T, K>(List<T> dataList, List<K> entityList, Func<string, K> skinManagerCallback) where T : AbstractLevelData where K : ILevelInitable
        {
            for (int i = 0; i < dataList.Count; i++)
            {
                T data = dataList[i];

                string id = data.Id;
                K entity = skinManagerCallback(id);

                entity.Init(data);

                entityList.Add(entity);
            }
        }

        public void SpawnList<T>(List<T> dataList, List<GameObject> entityList, Func<string, GameObject> skinManagerCallback) where T : AbstractLevelData
        {
            for (int i = 0; i < dataList.Count; i++)
            {
                T data = dataList[i];

                string id = data.Id;
                GameObject entity = skinManagerCallback(id);

                entity.transform.position = data.Position;

                entityList.Add(entity);
            }
        }

        private void ClearList<T>(List<T> list) where T : IClearable
        {
            for(int i = 0; i < list.Count; i++)
            {
                IClearable clearable = list[i];
                if(clearable != null)
                {
                    clearable.Clear();
                }
            }

            list.Clear();
        }

        private void ClearList(List<GameObject> list)
        {
            for (int i = 0; i < list.Count; i++)
            {
                GameObject obj = list[i];
                if (obj != null)
                {
                    obj.SetActive(false);
                }
            }

            list.Clear();
        }

        public void Clear()
        {
            ClearList(gates);
            ClearList(boosters);
            ClearList(obstacles);
            ClearList(environments);
            ClearList(enemies);
            ClearList(finishes);
            ClearList(roads);

            if(pickableGroups != null)
                pickableGroups.Clear();

            if(bonusStage != null)
                bonusStage.Clear();

            if(bonusStage != null)
                GameObject.Destroy(bonusStage.gameObject);
        }
    }
}
