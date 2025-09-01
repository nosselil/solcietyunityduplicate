using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Watermelon
{
    public class SkinsManager
    {
        private LevelsDatabase database;

        private Dictionary<string, PoolGeneric<GateBehavior>> gatePools;
        private Dictionary<string, PoolGeneric<ObstacleBehavior>> obstaclePools;
        private Dictionary<string, PoolGeneric<BoosterBehavior>> boosterPools;
        private Dictionary<string, PoolGeneric<GunBehavior>> gunPools;
        private Dictionary<string, PoolGeneric<EnvironmentBehavior>> environmentPools;
        private Dictionary<string, PoolGeneric<EnemyBehavior>> enemyPools;
        private Dictionary<string, PoolGeneric<FirstStageFinishBehavior>> finishPools;
        private Dictionary<string, Pool> roadPools;

        private Dictionary<CharacterType, string> defaultCharacterIds;

        public string DefaultGunId { get; private set; }

        #region Init

        public void Init(LevelsDatabase levelsDatabase)
        {
            database = levelsDatabase;

            gatePools = InitPoolsData<GateBehavior, GateData>(levelsDatabase.GatesData);
            obstaclePools = InitPoolsData<ObstacleBehavior, ObstacleData>(levelsDatabase.ObstaclesData);
            gunPools = InitPoolsData<GunBehavior, GunData>(levelsDatabase.GunsData);
            environmentPools = InitPoolsData<EnvironmentBehavior, EnvironmentData>(levelsDatabase.EnvironmentData);
            enemyPools = InitPoolsData<EnemyBehavior, EnemyData>(levelsDatabase.EnemiesData);
            boosterPools = InitPoolsData<BoosterBehavior, BoosterData>(levelsDatabase.BoostersData);

            finishPools = InitPoolsData<FirstStageFinishBehavior, FinishData>(levelsDatabase.FinishData);
            roadPools = InitPoolsData(levelsDatabase.RoadsData);

            defaultCharacterIds = new Dictionary<CharacterType, string>();

            for (int i = 0; i < levelsDatabase.CharactersData.Count; i++)
            {
                CharacterData characterData = levelsDatabase.CharactersData[i];
                if (characterData.IsDefault)
                {
                    if (defaultCharacterIds.ContainsKey(characterData.CharacterType))
                    {
                        Debug.LogError($"There are multiple default characters of type '{characterData.CharacterType}' in the Level Database");
                    } else
                    {
                        defaultCharacterIds.Add(characterData.CharacterType, characterData.Id);
                    }
                }
            }

            DefaultGunId = "";

            for (int i = 0; i < levelsDatabase.GunsData.Count; i++)
            {
                GunData gunData = levelsDatabase.GunsData[i];
                if (gunData.IsDefault)
                {
                    DefaultGunId += gunData.Id;
                    break;
                }
            }

            if (DefaultGunId == "")
            {
                Debug.LogError("There is no default gun in the Level Database");
            }
        }

        public void Unload()
        {
            DestroyPools(ref gatePools);
            DestroyPools(ref obstaclePools);
            DestroyPools(ref gunPools);
            DestroyPools(ref environmentPools);
            DestroyPools(ref enemyPools);
            DestroyPools(ref boosterPools);
            DestroyPools(ref finishPools);
            DestroyPools(ref roadPools);
        }

        private Dictionary<string, PoolGeneric<T>> InitPoolsData<T, K>(List<K> list) where T : Component where K : AbstractData
        {
            Dictionary<string, PoolGeneric<T>> dictionary = new Dictionary<string, PoolGeneric<T>>();

            for (int i = 0; i < list.Count; i++)
            {
                AbstractData data = list[i];
                GameObject prefab = data.Prefab;

                if (prefab == null) continue;

                string id = data.Id;

                PoolGeneric<T> pool = new PoolGeneric<T>(prefab, $"{prefab}_{id}");

                dictionary.Add(id, pool);
            }

            return dictionary;
        }

        private Dictionary<string, Pool> InitPoolsData<K>(List<K> list) where K : AbstractData
        {
            Dictionary<string, Pool> dictionary = new Dictionary<string, Pool>();

            for (int i = 0; i < list.Count; i++)
            {
                AbstractData data = list[i];
                GameObject prefab = data.Prefab;
                string id = data.Id;

                Pool pool = new Pool(prefab, $"{prefab}_{id}");

                dictionary.Add(id, pool);
            }

            return dictionary;
        }

        private void DestroyPools<T>(ref Dictionary<string, PoolGeneric<T>> dictionary) where T : Component
        {
            if(dictionary != null)
            {
                foreach (IPool pool in dictionary.Values)
                {
                    pool?.Destroy();
                }

                dictionary.Clear();
            }
        }

        private void DestroyPools(ref Dictionary<string, Pool> dictionary)
        {
            if (dictionary != null)
            {
                foreach (IPool pool in dictionary.Values)
                {
                    pool?.Destroy();
                }

                dictionary.Clear();
            }
        }
        #endregion

        #region Getters

        public GateBehavior GetGate(string id)
        {
            if (gatePools.ContainsKey(id))
            {
                return gatePools[id].GetPooledComponent();
            }

            Debug.LogError("Gate with id " + id + " is not found.");
            return null;
        }

        public BoosterBehavior GetBooster(string id)
        {
            if (boosterPools.ContainsKey(id))
            {
                return boosterPools[id].GetPooledComponent();
            }

            Debug.LogError("Booster with id " + id + " is not found.");
            return null;
        }

        public ObstacleBehavior GetObstacle(string id)
        {
            if (obstaclePools.ContainsKey(id))
            {
                return obstaclePools[id].GetPooledComponent();
            }

            Debug.LogError("Obstacle with id " + id + " is not found.");
            return null;
        }

        public CharacterBehavior GetCharacter(string id)
        {
            CharacterData data = database.GetCharacterData(id);
            if (data != null)
            {
                GameObject characterObject = GameObject.Instantiate(data.Prefab);
                CharacterBehavior character = characterObject.GetComponent<CharacterBehavior>();

                character.SetData(data);

                return character;
            }

            Debug.LogError("Character with id " + id + " is not found.");
            return null;
        }

        public CharacterBehavior GetDefaultCharacter(CharacterType type)
        {
            if (defaultCharacterIds.ContainsKey(type))
            {
                string characterId = defaultCharacterIds[type];

                return GetCharacter(characterId);
            }

            Debug.LogError($"There are no default character of type '{type} in the LevelsDatabase'");

            return null;
        }

        public GunBehavior GetGun(string id)
        {
            if (gunPools.ContainsKey(id))
            {
                GunBehavior gun = gunPools[id].GetPooledComponent();
                GunData data = database.GetGunData(id);

                gun.SetData(data);

                return gun;
            }

            Debug.LogError("Gun with id " + id + " is not found.");
            return null;
        }

        public GameObject GetRoad(string id)
        {
            if (roadPools.ContainsKey(id))
            {
                GameObject road = roadPools[id].GetPooledObject();

                return road;
            }

            Debug.LogError("Road with id " + id + " is not found.");
            return null;
        }

        public EnvironmentBehavior GetEnvironent(string id)
        {
            if (environmentPools.ContainsKey(id))
            {
                EnvironmentBehavior environment = environmentPools[id].GetPooledComponent();

                return environment;
            }

            Debug.LogError("Environment with id " + id + " is not found.");
            return null;
        }

        public FirstStageFinishBehavior GetFinish(string id)
        {
            if(finishPools.ContainsKey(id))
            {
                FirstStageFinishBehavior finish = finishPools[id].GetPooledComponent();

                return finish;
            }

            Debug.LogError("Finish with id " + id + " is not found.");
            return null;
        }

        public EnemyBehavior GetEnemy(string id)
        {
            if(enemyPools.ContainsKey(id))
            {
                EnemyBehavior enemy = enemyPools[id].GetPooledComponent();

                return enemy;
            }

            Debug.LogError("Enemy with id " + id + " is not found.");
            return null;
        }

        #endregion
    }
}