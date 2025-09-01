using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Watermelon
{
    public class LevelController : MonoBehaviour
    {
        private static LevelController instance;

        [SerializeField] LevelsDatabase database;

        [SerializeField] PlayerBehavior playerBehavior;
        [SerializeField] MovementManager movementManager;

        private LevelSpawner levelSpawner;
        private SkinsManager skinsManager;

        public static LevelsDatabase Database => instance.database;
        public static LevelSpawner LevelSpawner => instance.levelSpawner;
        public static SkinsManager SkinsManager => instance.skinsManager;
        public static MovementManager MovementManager => instance.movementManager;

        public static LevelData LevelData { get; private set; }

        public static bool LevelLoaded { get; private set; }

        public static int StageId { get; private set; }

        public void Init()
        {
            instance = this;

            levelSpawner = new LevelSpawner();
            skinsManager = new SkinsManager();

            skinsManager.Init(database);
            levelSpawner.Init(skinsManager);

            playerBehavior.Init();

            movementManager.SetPlayer(playerBehavior);

            LevelLoaded = false;
        }

        private void Start()
        {
            VirtualCamera gameplayCamera = CameraController.GetCamera(CameraType.Gameplay);
            gameplayCamera.SetTarget(playerBehavior.transform);
        }

        public void LoadLevel(int levelId)
        {
            UnloadLevel();

            if (levelId >= database.Levels.Count)
            {
                if (GameController.Data.RandomizeLevelsAfterReachingLast)
                {
                    levelId = Random.Range(0, database.Levels.Count);
                }
                else
                {
                    levelId = database.Levels.Count - 1;
                }
            }

            LevelData = database.Levels[levelId];

            if (LevelData.RoadsData.IsNullOrEmpty())
                Debug.LogError($"There are no Roads elements in the level #{levelId}!");

            RoadData roadData = database.GetRoadData(LevelData.RoadsData[^1].id);

            movementManager.Init(LevelData.MovementMode, roadData);

            levelSpawner.SpawnLevel(LevelData);

            LevelLoaded = true;

            playerBehavior.SetInitialPosition(LevelData.PlayerSpawnPositionZ);

            CameraData cameraData = database.GetCameraData(LevelData.CameraDataId);

            if (cameraData != null)
            {
                VirtualCamera gameplayCamera = CameraController.GetCamera(CameraType.Gameplay);
                gameplayCamera.SetFollowOffset(cameraData.FollowOffset);
                gameplayCamera.SetRotation(cameraData.Rotation);
                gameplayCamera.SetFov(cameraData.FOV);
            }
            else
            {
                Debug.LogError($"There are no Camera Data with '{LevelData.CameraDataId}' id in the database!");
            }

            playerBehavior.SetShouldMoveSideways(cameraData.IsMovingSideways);

            StageId = 1;
        }

        public void StartLevel()
        {
            movementManager.ResetMovement();
            movementManager.SetInitialPosition(LevelData.PlayerSpawnPositionZ);

            playerBehavior.Clear();

            playerBehavior.StartLevel(LevelData.CharacterType);
        }

        public void DestroyLevel()
        {
            if (levelSpawner != null)
                levelSpawner.Clear();

            if (skinsManager != null)
                skinsManager.Unload();

            if (playerBehavior != null)
                playerBehavior.Clear();

            LevelLoaded = false;
        }

        public void UnloadLevel()
        {
            if (levelSpawner != null)
                levelSpawner.Clear();

            if (skinsManager != null)
                movementManager.ResetMovement();

            if (playerBehavior != null)
                playerBehavior.Clear();

            LevelLoaded = false;
        }

        public void Revive()
        {
            playerBehavior.Revive(LevelData.CharacterType);
        }

        public static void ReachedSecondStage()
        {
            StageId = 2;
        }
    }
}