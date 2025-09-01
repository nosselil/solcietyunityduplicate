using UnityEditor;
using System.Linq;

namespace Watermelon
{
    public static class EditorLevelDataPicker
    {
        public static LevelsDatabase LevelsDatabase { get; private set; }

        public static bool IsDatabaseExists => LevelsDatabase != null;

        [InitializeOnLoadMethod]
        private static void Init()
        {
            LevelsDatabase = EditorUtils.GetAsset<LevelsDatabase>();
        }

        public static string[] GetItems(LevelDataType levelDataType)
        {
            if (LevelsDatabase == null) return null;

            if(levelDataType == LevelDataType.Character)
            {
                return LevelsDatabase.CharactersData.Select(x => x.Id).ToArray();    
            }
            else if(levelDataType == LevelDataType.Weapon)
            {
                return LevelsDatabase.GunsData.Select(x => x.Id).ToArray();
            }
            else if (levelDataType == LevelDataType.Camera)
            {
                return LevelsDatabase.CameraData.Select(x => x.Id).ToArray();
            }
            else if (levelDataType == LevelDataType.Road)
            {
                return LevelsDatabase.RoadsData.Select(x => x.Id).ToArray();
            }
            else if (levelDataType == LevelDataType.Environment)
            {
                return LevelsDatabase.EnvironmentData.Select(x => x.Id).ToArray();
            }

            // Unknown LevelDataType
            return null;
        }
    }
}
