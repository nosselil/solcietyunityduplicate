using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using Unity.VisualScripting;
using UnityEditor;
using UnityEditor.Presets;
using UnityEngine;
using UnityEngine.UIElements;
using Preset = UnityEditor.Presets.Preset;
using Random = UnityEngine.Random;

namespace Watermelon.LevelEditor
{
    public class LevelGenerator
    {
        private const string DEFAULT_SAVE_DIRECTORY = "Assets/Project Files/Data/Level System/Generator Presets";
        private const string PRESET_EXTENSION = "preset";
        private LevelsDatabase levelsDatabase;
        private LevelGeneratorDatabase levelGeneratorDatabase;
        private CustomInspector generatorDatabaseEditor;

        public LevelGenerator(LevelsDatabase levelsDatabase, LevelGeneratorDatabase levelGeneratorDatabase)
        {
            this.levelsDatabase = levelsDatabase;
            this.levelGeneratorDatabase = levelGeneratorDatabase;
            generatorDatabaseEditor = (CustomInspector)Editor.CreateEditor(levelGeneratorDatabase, typeof(CustomInspector));
            generatorDatabaseEditor.SetScriptFieldState(false);
        }

        public void DrawGUI()
        {
            Rect contentRect = EditorGUILayout.BeginVertical(EditorCustomStyles.windowSpacedContent);
            EditorGUILayout.Space(3f);


            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Level Generator Presets", EditorCustomStyles.labelMediumBold);
            GUILayout.FlexibleSpace();

            if (GUILayout.Button("Load", EditorCustomStyles.buttonBlue))
            {
                Load();
            }

            if (GUILayout.Button("Save", EditorCustomStyles.buttonGreen))
            {
                Save();
            }

            EditorGUILayout.EndHorizontal();
            float backupLabelWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = Mathf.Max(backupLabelWidth, (contentRect.width - 8) /2f);
            generatorDatabaseEditor.OnInspectorGUI();
            EditorGUIUtility.labelWidth = backupLabelWidth;

            if (GUI.changed)
            {
                generatorDatabaseEditor.SaveChanges();
            }

            EditorGUILayout.Space(10f);

            if (GUILayout.Button("Generate Full Level", GUILayout.Height(40f)))
            {
                GenerateFullLevel();
            }

            EditorGUILayout.Space(15f);
            EditorGUILayout.LabelField("Line by line generation", EditorCustomStyles.labelMediumBold);

            

            if (!lineByLineInited)
            {
                if (GUILayout.Button("Start Generation", GUILayout.Height(40f)))
                {

                    PrepareToLevelSpawn();
                    SpawnEnvironmentBasedOnTheContent();
                    lineByLineInited = true;
                }
            }
            else
            {
                EditorGUILayout.BeginHorizontal();

                if (GUILayout.Button("Reset", GUILayout.Height(40f)))
                {

                    PrepareToLevelSpawn();
                    SpawnEnvironmentBasedOnTheContent();
                }


                if (currentContentSpawnPos.z > levelsDatabase.DefaultContentSpawnPositionZ)
                {
                    EditorGUILayout.Space(5f);

                    if (GUILayout.Button("Respawn line", GUILayout.Height(40f)))
                    {
                        DeleteLastLineSpawnedObjects();

                        currentContentSpawnPos -= Vector3.forward * (levelGeneratorDatabase.overrideLineOffset.Enabled ? levelGeneratorDatabase.overrideLineOffset.Value : levelsDatabase.DefaultContentOffsetZ);

                        SpawnALine();
                        SpawnFinish();
                    }

                }

                EditorGUILayout.Space(5f);

                if (GUILayout.Button("Add line", GUILayout.Height(40f)))
                {
                    EditorSceneController.Instance.FocusCameraPosition(currentContentSpawnPos, 10f);
                    EditorSceneController.Instance.ClearBackground(); //clear environment and roads

                    SpawnALine();
                    SpawnFinish();

                    SpawnEnvironmentBasedOnTheContent();
                    
                }

                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.EndVertical();
        }

        public void OnLevelOpened()
        {
            lineByLineInited = false;
        }

        private void Load()
        {
            string absolutePath = EditorUtility.OpenFilePanel("Load LevelGeneratorDatabase preset", DEFAULT_SAVE_DIRECTORY, PRESET_EXTENSION);
            string relativePath = EditorUtils.ConvertToRelativePath(absolutePath);
            Preset preset = AssetDatabase.LoadAssetAtPath<Preset>(relativePath);

            if (preset != null)
            {
                preset.ApplyTo(levelGeneratorDatabase);
            }
        }


        private void Save()
        {
            Preset preset = new Preset(levelGeneratorDatabase);

            string path = EditorUtility.SaveFilePanelInProject("Save LevelGeneratorDatabase preset", "newPreset", PRESET_EXTENSION, "Message", DEFAULT_SAVE_DIRECTORY);

            if (path != string.Empty)
            {
                AssetDatabase.CreateAsset(preset, path);
            }
        }

        // Generator variables
        private WeightedList<GateData> gatesChancesData;
        private WeightedList<BoosterData> boostersChancesData;
        private WeightedList<ObstacleGenerationSettings> obstaclesChancesData;
        private WeightedList<EnemyGenerationSettings> enemyChancesData;
        private RoadData roadData;
        private Vector3 currentContentSpawnPos;
        private List<GameObject> lastLineObjects = new List<GameObject>();
        private GameObject lastSpawnedFinish;
        private bool lineByLineInited;

        private void GenerateFullLevel()
        {
            PrepareToLevelSpawn();

            // content spawn
            for (int i = 0; i < levelGeneratorDatabase.linesAmount; i++)
            {
                SpawnALine();
            }

            SpawnFinish();

            SpawnEnvironmentBasedOnTheContent();
        }

        private void PrepareToLevelSpawn()
        {
            if (levelsDatabase.RoadsData.Count == 0)
            {
                Debug.LogError("Level generator needs at least 1 road prefab.");
                return;
            }

            if (levelsDatabase.FinishData.Count == 0)
            {
                Debug.LogError("Level generator needs at least 1 finish prefab.");
                return;
            }

            gatesChancesData = new WeightedList<GateData>(new List<WeightedItem<GateData>>
            {
                new WeightedItem<GateData>(levelGeneratorDatabase.characterGateChance, levelsDatabase.GetGateData(GateType.GiveCharacter)),
                new WeightedItem<GateData>(levelGeneratorDatabase.weaponGateChance, levelsDatabase.GetGateData(GateType.GiveWeapon)),
                new WeightedItem<GateData>(levelGeneratorDatabase.healthGateChance, levelsDatabase.GetGateData(GateType.Health)),
                new WeightedItem<GateData>(levelGeneratorDatabase.damageGateChance, levelsDatabase.GetGateData(GateType.Damage)),
                new WeightedItem<GateData>(levelGeneratorDatabase.fireRateGateChance, levelsDatabase.GetGateData(GateType.FireRate)),
                new WeightedItem<GateData>(levelGeneratorDatabase.rangeGateChance, levelsDatabase.GetGateData(GateType.Range)),
                new WeightedItem<GateData>(levelGeneratorDatabase.moneyGateChance, levelsDatabase.GetGateData(GateType.Money)),
            });

            boostersChancesData = new WeightedList<BoosterData>(new List<WeightedItem<BoosterData>>
            {
                new WeightedItem<BoosterData>(levelGeneratorDatabase.characterBoosterChance, levelsDatabase.GetBoosterData(BoosterType.GiveCharacter)),
                new WeightedItem<BoosterData>(levelGeneratorDatabase.weaponBoosterChance, levelsDatabase.GetBoosterData(BoosterType.GiveWeapon)),
                new WeightedItem<BoosterData>(levelGeneratorDatabase.healthBoosterChance, levelsDatabase.GetBoosterData(BoosterType.Health)),
                new WeightedItem<BoosterData>(levelGeneratorDatabase.damageBoosterChance, levelsDatabase.GetBoosterData(BoosterType.Damage)),
                new WeightedItem<BoosterData>(levelGeneratorDatabase.fireRateBoosterChance, levelsDatabase.GetBoosterData(BoosterType.FireRate)),
                new WeightedItem<BoosterData>(levelGeneratorDatabase.rangeBoosterChance, levelsDatabase.GetBoosterData(BoosterType.Range)),
            });

            obstaclesChancesData = new WeightedList<ObstacleGenerationSettings>(levelGeneratorDatabase.obstacleGenerationSettings);

            for (int i = 0; i < levelGeneratorDatabase.obstacleGenerationSettings.Count; i++)
            {
                levelGeneratorDatabase.obstacleGenerationSettings[i].Item.InitDropWeightedList();
            }

            enemyChancesData = new WeightedList<EnemyGenerationSettings>(levelGeneratorDatabase.enemyGenerationSettings);

            for (int i = 0; i < levelGeneratorDatabase.enemyGenerationSettings.Count; i++)
            {
                levelGeneratorDatabase.enemyGenerationSettings[i].Item.InitDropWeightedList();
            }

            roadData = levelsDatabase.GetRoadData(levelGeneratorDatabase.roadType);


            EditorSceneController.Instance.Clear(); //clear scene

            currentContentSpawnPos = new Vector3(0, 0, levelsDatabase.DefaultContentSpawnPositionZ);

        }

        private void SpawnALine()
        {
            lastLineObjects.Clear();

            float contentTypesChancesTotal = levelGeneratorDatabase.gatesChance + levelGeneratorDatabase.boostersChance + levelGeneratorDatabase.obstaclesChance + levelGeneratorDatabase.enemiesChance;
            int elementsPerLine = GetElementsPerLine();
            float previousContentTypeRandom = 0f;

            for (int j = 0; j < elementsPerLine; j++)
            {
                float sidePosition = GetElementPositionOnTheLine(elementsPerLine, j, roadData);
                float contentTypeRandom = Random.Range(0f, contentTypesChancesTotal);

                // 50% chance to have similar type object on the same line
                if (j > 0 && Random.Range(0f, 1f) < 0.5f)
                {
                    contentTypeRandom = previousContentTypeRandom;
                }

                previousContentTypeRandom = contentTypeRandom;
                GameObject justSpawnedObject;

                // gates spawn
                if (contentTypeRandom < levelGeneratorDatabase.gatesChance)
                {
                    justSpawnedObject = SpawnGate(gatesChancesData.GetRandomItem(), currentContentSpawnPos.SetX(sidePosition));
                }
                // boosters spawn
                else if (contentTypeRandom < levelGeneratorDatabase.gatesChance + levelGeneratorDatabase.boostersChance)
                {
                    justSpawnedObject = SpawnBooster(boostersChancesData.GetRandomItem(), currentContentSpawnPos.SetX(sidePosition));
                }
                // obstacle spawn
                else if (contentTypeRandom < levelGeneratorDatabase.gatesChance + levelGeneratorDatabase.boostersChance + levelGeneratorDatabase.obstaclesChance)
                {
                    ObstacleGenerationSettings settings = obstaclesChancesData.GetRandomItem();

                    justSpawnedObject = SpawnObstacle(settings, currentContentSpawnPos.SetX(sidePosition));

                    if (Random.Range(0f, 1f) < settings.doubleObstacleChance)
                    {
                        lastLineObjects.Add(SpawnObstacle(settings, currentContentSpawnPos.SetX(sidePosition).AddToZ(-levelsDatabase.GetObstacleData(settings.obstacleId).ObstacleLengthAlongZ)));
                    }

                    if (Random.Range(0f, 1f) < settings.chanceToSpawnBoosterOnTop)
                    {
                        BoosterData boosterData = boostersChancesData.GetRandomItem();
                        lastLineObjects.Add(SpawnBooster(boosterData, currentContentSpawnPos.SetX(sidePosition).SetY(levelsDatabase.GetObstacleData(settings.obstacleId).DefaultBoosterHeight)));
                    }
                }
                // enemy spawn
                else
                {
                    justSpawnedObject = SpawnEnemy(enemyChancesData.GetRandomItem(), currentContentSpawnPos.SetX(sidePosition));
                }

                lastLineObjects.Add(justSpawnedObject);
            }

            currentContentSpawnPos += Vector3.forward * (levelGeneratorDatabase.overrideLineOffset.Enabled ? levelGeneratorDatabase.overrideLineOffset.Value : levelsDatabase.DefaultContentOffsetZ);
        }

        private void DeleteLastLineSpawnedObjects()
        {
            for (int i = 0; i < lastLineObjects.Count; i++)
            {
                if (lastLineObjects[i] != null)
                {
                    Tween.DestroyImmediate(lastLineObjects[i]);
                }
            }
        }

        private void SpawnFinish()
        {
            if (lastSpawnedFinish != null)
            {
                Tween.DestroyImmediate(lastSpawnedFinish);
            }

            SavableFinish savableFinish;
            FinishData finishData = levelsDatabase.FinishData[0];


            GameObject gameObject = EditorSceneController.Instance.SpawnPrefab(finishData.Prefab, currentContentSpawnPos, true, false);
            savableFinish = EditorSceneController.Instance.AddFinish(gameObject, finishData.Id);

            lastSpawnedFinish = gameObject;
        }

        private void SpawnEnvironmentBasedOnTheContent()
        {
            GameObject gameObject;

            // roads spawn
            SavableRoad savableRoad;

            int roadsAmount = Mathf.CeilToInt((currentContentSpawnPos.z - levelsDatabase.DefaultPlayerSpawnPositionZ) / roadData.LengthAlongZ) + 5;
            Vector3 currentEnvironmentSpawnPos = new Vector3(0, 0, levelsDatabase.DefaultEnvironmentSpawnPositionZ);

            for (int i = 0; i < roadsAmount; i++)
            {
                gameObject = EditorSceneController.Instance.SpawnPrefab(roadData.Prefab, currentEnvironmentSpawnPos, false, false);// we spawn prefab and get it`s ref
                savableRoad = EditorSceneController.Instance.AddRoad(gameObject, roadData.Id); // We add SavableRoad to our gameobject so it would be saved and get it`s ref

                currentEnvironmentSpawnPos = currentEnvironmentSpawnPos.AddToZ(roadData.LengthAlongZ);
            }

            // environment spawn

            EnvironmentData environmentData = levelsDatabase.GetEnvironmentData(levelGeneratorDatabase.environmentType);
            SavableEnvironment savableEnvironment;

            int environmentPiecesAmount = Mathf.CeilToInt((currentContentSpawnPos.z - levelsDatabase.DefaultPlayerSpawnPositionZ) / environmentData.LengthAlongZ) + 1;
            currentEnvironmentSpawnPos = new Vector3(0, 0, levelsDatabase.DefaultEnvironmentSpawnPositionZ);

            for (int i = 0; i < environmentPiecesAmount; i++)
            {
                gameObject = EditorSceneController.Instance.SpawnPrefab(environmentData.Prefab, currentEnvironmentSpawnPos, false, false);// we spawn prefab and get it`s ref
                savableEnvironment = EditorSceneController.Instance.AddEnvironment(gameObject, environmentData.Id); // We add SavableRoad to our gameobject so it would be saved and get it`s ref

                currentEnvironmentSpawnPos = currentEnvironmentSpawnPos.AddToZ(environmentData.LengthAlongZ);
            }
        }

        private int GetElementsPerLine()
        {
            float elementsPerLineChancesTotal = levelGeneratorDatabase.singleElementPerLineChance + levelGeneratorDatabase.twoElementsPerLineChance + levelGeneratorDatabase.thereElementsPerLineChance;

            float elementsPerLineRandom = Random.Range(0f, elementsPerLineChancesTotal);

            int elementsPerLine = 0;

            if (elementsPerLineRandom < levelGeneratorDatabase.singleElementPerLineChance)
            {
                elementsPerLine = 1;
            }
            else if (elementsPerLineRandom < levelGeneratorDatabase.singleElementPerLineChance + levelGeneratorDatabase.twoElementsPerLineChance)
            {
                elementsPerLine = 2;

            }
            else
            {
                elementsPerLine = 3;
            }

            return elementsPerLine;
        }

        private float GetElementPositionOnTheLine(int elementsPerLine, int elementIndexOnTheLine, RoadData roadData)
        {
            if (elementsPerLine == 0)
            {
                return 0f;
            }
            else if (elementsPerLine == 1)
            {
                if (levelGeneratorDatabase.singleItemSpawnPositionType == EditorSingleItemPerLinePositionType.Random)
                {
                    return Random.Range(roadData.RoadWidth * -0.35f, roadData.RoadWidth * 0.35f);
                }
                else if (levelGeneratorDatabase.singleItemSpawnPositionType == EditorSingleItemPerLinePositionType.Center)
                {
                    return 0f;
                }
                else if (levelGeneratorDatabase.singleItemSpawnPositionType == EditorSingleItemPerLinePositionType.Inside2Lines)
                {
                    return GetElementPositionOnTheLine(2, Random.Range(0, 2), roadData);
                }
                // inside 3 lines
                else
                {
                    return GetElementPositionOnTheLine(3, Random.Range(0, 3), roadData);
                }
            }
            else
            {
                return (roadData.RoadWidth * -0.5f) + (roadData.RoadWidth * (1f / elementsPerLine) * 0.5f) + (roadData.RoadWidth * (1f / elementsPerLine)) * elementIndexOnTheLine;
            }
        }

        private GameObject SpawnGate(GateData data, Vector3 position)
        {
            GameObject gameObject = EditorSceneController.Instance.SpawnPrefab(data.Prefab, position, true, false);// we spawn prefab and get it`s ref
            SavableGate savableGate = EditorSceneController.Instance.AddGate(gameObject, data.Id); // We add Savable class to our gameobject so it would be saved and get it`s ref
            savableGate.GateType = data.GateType;
            savableGate.IsNumerical = data.IsNumerical;

            // character type
            if (data.GateType == GateType.GiveCharacter)
            {
                // creating chances list of all character
                WeightedList<ExplicitGateAndBoosterSettings> characterChancesData = new WeightedList<ExplicitGateAndBoosterSettings>(levelGeneratorDatabase.characterGateOptions);
                ExplicitGateAndBoosterSettings settings = characterChancesData.GetRandomItem();

                // assigning random character
                savableGate.ExplicitId = settings.explicitID;
                savableGate.CharactersAmount = settings.explicitAmount;
            }
            // weapon type
            else if (data.GateType == GateType.GiveWeapon)
            {
                // creating chances list of all weapon settings
                WeightedList<ExplicitGateAndBoosterSettings> weaponChancesData = new WeightedList<ExplicitGateAndBoosterSettings>(levelGeneratorDatabase.weaponGateOptions);
                ExplicitGateAndBoosterSettings settings = weaponChancesData.GetRandomItem();

                // assigning random settings
                savableGate.ExplicitId = settings.explicitID;
                savableGate.CharactersAmount = settings.explicitAmount;
            }
            // health type
            else if (data.GateType == GateType.Health)
            {
                AssignNumericalGateSettings(levelGeneratorDatabase.healthGateSettings, savableGate);
            }
            // damage type
            else if (data.GateType == GateType.Damage)
            {
                AssignNumericalGateSettings(levelGeneratorDatabase.damageGateSettings, savableGate);
            }
            // fire rate type
            else if (data.GateType == GateType.FireRate)
            {
                AssignNumericalGateSettings(levelGeneratorDatabase.fireRateGateSettings, savableGate);
            }
            // range type
            else if (data.GateType == GateType.Range)
            {
                AssignNumericalGateSettings(levelGeneratorDatabase.rangeGateSettings, savableGate);

            }
            // money type
            else if (data.GateType == GateType.Money)
            {
                AssignNumericalGateSettings(levelGeneratorDatabase.moneyGateSettings, savableGate);
            }

            return gameObject;
        }

        private void AssignNumericalGateSettings(List<WeightedItem<GateGenerationSettings>> settingsList, SavableGate gateSavable)
        {
            // creating chances list of provided settings
            WeightedList<GateGenerationSettings> weightedSettingsList = new WeightedList<GateGenerationSettings>(settingsList);
            // choosing one random preset based on the weight
            GateGenerationSettings gateSettings = weightedSettingsList.GetRandomItem();

            // assigning gate settings
            gateSavable.OperationType = gateSettings.operationType;
            gateSavable.NumericalValue = (float)Math.Round(gateSettings.valueRange.Random(), gateSettings.decimalPlacesAfterRounding);
            gateSavable.UpdateOnHit = gateSettings.updateOnHit;
            gateSavable.Step = gateSettings.step;
        }

        private GameObject SpawnBooster(BoosterData data, Vector3 position)
        {
            GameObject gameObject = EditorSceneController.Instance.SpawnPrefab(data.Prefab, position, true, false);// we spawn prefab and get it`s ref
            SavableBooster savableBooster = EditorSceneController.Instance.AddBooster(gameObject, data.Id); // We add Savable class to our gameobject so it would be saved and get it`s ref
            savableBooster.BoosterType = data.BoosterType;
            savableBooster.IsNumerical = data.IsNumerical;

            // character type
            if (data.BoosterType == BoosterType.GiveCharacter)
            {
                // creating chances list of all character
                WeightedList<ExplicitGateAndBoosterSettings> characterChancesData = new WeightedList<ExplicitGateAndBoosterSettings>(levelGeneratorDatabase.characterBoosterOptions);
                ExplicitGateAndBoosterSettings settings = characterChancesData.GetRandomItem();

                // assigning random character
                savableBooster.CharacterID = settings.explicitID;
                savableBooster.CharactersAmount = settings.explicitAmount;
            }
            // weapon type
            else if (data.BoosterType == BoosterType.GiveWeapon)
            {
                // creating chances list of all weapon settings
                WeightedList<ExplicitGateAndBoosterSettings> weaponChancesData = new WeightedList<ExplicitGateAndBoosterSettings>(levelGeneratorDatabase.weaponBoosterOptions);
                ExplicitGateAndBoosterSettings settings = weaponChancesData.GetRandomItem();

                // assigning random settings
                savableBooster.WeaponID = settings.explicitID;
                savableBooster.CharactersAmount = settings.explicitAmount;
            }
            // health type
            else if (data.BoosterType == BoosterType.Health)
            {
                AssignNumericalBoosterSettings(levelGeneratorDatabase.healthBoosterSettings, savableBooster);
            }
            // damage type
            else if (data.BoosterType == BoosterType.Damage)
            {
                AssignNumericalBoosterSettings(levelGeneratorDatabase.damageBoosterSettings, savableBooster);
            }
            // fire rate type
            else if (data.BoosterType == BoosterType.FireRate)
            {
                AssignNumericalBoosterSettings(levelGeneratorDatabase.fireRateBoosterSettings, savableBooster);
            }
            // range type
            else if (data.BoosterType == BoosterType.Range)
            {
                AssignNumericalBoosterSettings(levelGeneratorDatabase.rangeBoosterSettings, savableBooster);
            }

            return gameObject;
        }

        private void AssignNumericalBoosterSettings(List<WeightedItem<BoosterGenerationSettings>> settingsList, SavableBooster boosterSavable)
        {
            // creating chances list of provided settings
            WeightedList<BoosterGenerationSettings> weightedSettingsList = new WeightedList<BoosterGenerationSettings>(settingsList);
            // choosing one random preset based on the weight
            BoosterGenerationSettings boosterSettings = weightedSettingsList.GetRandomItem();

            // assigning booster settings
            boosterSavable.OperationType = boosterSettings.operationType;
            boosterSavable.NumericalValue = (float)Math.Round(boosterSettings.valueRange.Random(), boosterSettings.decimalPlacesAfterRounding);

            // value validation - can't have less than 1 with multiply or divide operation
            if (boosterSettings.operationType == OperationType.Multiply || boosterSettings.operationType == OperationType.Divide)
            {
                boosterSavable.NumericalValue = Mathf.Clamp(boosterSavable.NumericalValue, 1, float.MaxValue);
            }
        }

        private GameObject SpawnObstacle(ObstacleGenerationSettings settings, Vector3 position)
        {
            ObstacleData data = levelsDatabase.GetObstacleData(settings.obstacleId);
            GameObject gameObject = EditorSceneController.Instance.SpawnPrefab(data.Prefab, position, true, false);// we spawn prefab and get it`s ref
            SavableObstacle obstacleSavable = EditorSceneController.Instance.AddObstacle(gameObject, data.Id); // We add Savable class to our gameobject so it would be saved and get it`s ref

            obstacleSavable.Health = settings.obstacleHealth.Random();
            obstacleSavable.BoosterHeight = data.DefaultBoosterHeight;

            DropSettings dropSettings = settings.GetRandomDropSettings();

            obstacleSavable.DropType = dropSettings.dropType;
            obstacleSavable.DropCurrencyType = dropSettings.dropCurrencyType;
            obstacleSavable.DropItemsCount = dropSettings.dropItemsCount.Random();
            obstacleSavable.DropItemValue = dropSettings.dropItemValue.Random();

            return gameObject;
        }

        private GameObject SpawnEnemy(EnemyGenerationSettings settings, Vector3 position)
        {
            EnemyData data = levelsDatabase.GetEnemyData(settings.enemyId);

            if (data == null)
            {
                Debug.LogError("Enemy with id " + settings.enemyId + " is not found in Levels Database.");
                return null;
            }

            GameObject gameObject = EditorSceneController.Instance.SpawnPrefab(data.Prefab, position, true, false);// we spawn prefab and get it`s ref
            SavableEnemy enemySavable = EditorSceneController.Instance.AddEnemy(gameObject, data.Id); // We add Savable class to our gameobject so it would be saved and get it`s ref

            enemySavable.Health = settings.health.Random();
            enemySavable.Damage = settings.damage.Random();
            enemySavable.FireRate = settings.fireRate.Random();
            enemySavable.GunId = settings.gunId;

            DropSettings dropSettings = settings.GetRandomDropSettings();

            enemySavable.DropType = dropSettings.dropType;
            enemySavable.DropCurrencyType = dropSettings.dropCurrencyType;
            enemySavable.DropItemsCount = dropSettings.dropItemsCount.Random();
            enemySavable.DropItemValue = dropSettings.dropItemValue.Random();

            return gameObject;
        }

        
    }


}
