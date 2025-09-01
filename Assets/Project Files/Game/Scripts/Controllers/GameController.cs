using UnityEngine;

namespace Watermelon
{
    public class GameController : MonoBehaviour
    {
        private static readonly string LEVEL_NUMBER_SAVE_NAME = "Level Number Save";

        private static GameController gameController;

        [DrawReference]
        [SerializeField] GameData data;

        [LineSpacer]
        [SerializeField] UIController uiController;
        [SerializeField] CameraController cameraController;
        [SerializeField] MusicSource musicSource;

        [LineSpacer]
        [SerializeField] DropableItemSettings dropSetting;

        private static ParticlesController particlesController;
        private static FloatingTextController floatingTextController;
        private static LevelController levelController;
        private static UpgradesController upgradesController;

        public static GameData Data => gameController.data;

        public static bool IsGameplayActive { get; private set; }

        private static LevelNumberSave save;

        public static SimpleCallback OnLevelChangedEvent;

        private void Awake()
        {
            IsGameplayActive = false;

            gameController = this;

            // Cache components
            gameObject.CacheComponent(out particlesController);
            gameObject.CacheComponent(out floatingTextController);
            gameObject.CacheComponent(out levelController);
            gameObject.CacheComponent(out upgradesController);

            // Initialise UI Controller to let other classes use UIController.GetPage method
            uiController.Init();

            musicSource.Init();
            musicSource.Activate();

            // Initialise other controlles
            cameraController.Initialise();
            upgradesController.Initialise();
            particlesController.Init();
            floatingTextController.Init();
            levelController.Init();

            Drop.Initialise(dropSetting);

            // Initialise currency cloud and pages
            uiController.InitPages();
        }

        private void Start()
        {
            save = SaveController.GetSaveObject<LevelNumberSave>(LEVEL_NUMBER_SAVE_NAME);

            // Display default page
            UIController.ShowPage<UIMainMenu>();

            // Load menu/map/level
            if (Data.LoadLevelInMainMenu)
            {
                levelController.LoadLevel(save.LevelNumber);
            }
        }

        public static void StartLevel()
        {
            if (!LevelController.LevelLoaded)
            {
                levelController.LoadLevel(save.LevelNumber);
            }

            levelController.StartLevel();

            UIController.HidePage<UIMainMenu>(UIController.ShowPage<UIGame>);

            SavePresets.CreateSave("Level " + (save.LevelNumber + 1).ToString("000"), "Levels");

            IsGameplayActive = true;
        }

        public static void OnGameFail()
        {
            IsGameplayActive = false;
            UIController.HidePage<UIGame>(UIController.ShowPage<UIGameOver>);

            AudioController.PlaySound(AudioController.AudioClips.lose);
        }

        public static void OnRevive()
        {
            UIController.HidePage<UIGameOver>();
            UIController.ShowPage<UIGame>();

            IsGameplayActive = true;

            levelController.Revive();
        }

        public static void OnChestReached()
        {
            IsGameplayActive = false;
        }

        public static void OnLevelComplete()
        {
            IsGameplayActive = false;

            UIController.HidePage<UIGame>();
            UIController.ShowPage<UIComplete>();

            save.IncrementLevelNumber();
            OnLevelChangedEvent?.Invoke();
         
            AudioController.PlaySound(AudioController.AudioClips.win);
        }

        public static void ReturnToMainMenu()
        {
            UIController.ShowPage<UIMainMenu>();
            levelController.UnloadLevel();

            if (Data.LoadLevelInMainMenu)
            {
                levelController.LoadLevel(save.LevelNumber);
            }
        }

        public static void Unload(SimpleCallback unloadCallback)
        {
            levelController.DestroyLevel();

            Drop.Unload();

            unloadCallback?.Invoke();
        }
    }
}