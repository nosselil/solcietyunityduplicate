using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SocialPlatforms;

namespace Watermelon.LevelEditor
{
    [CreateAssetMenu(fileName = "Levels Generator Database", menuName = "Data/Level System/Levels Generator Database")]
    public class LevelGeneratorDatabase : ScriptableObject
    {
        [Header("General"), Order(0)]
        public int linesAmount;
        [Order(0)]
        public FloatToggle overrideLineOffset;

        [Range(0f, 1f), Order(0)]
        public float singleElementPerLineChance;
        [Order(0)]
        protected bool NoSingleElementsPerLile => singleElementPerLineChance == 0f;

        [HideIf("NoSingleElementsPerLile"), Order(0)]
        public EditorSingleItemPerLinePositionType singleItemSpawnPositionType;

        [Range(0f, 1f), Order(0)]
        public float twoElementsPerLineChance;

        [Range(0f, 1f), Order(0)]
        public float thereElementsPerLineChance;

        [Space(5)]
        [LineSpacer("Content Types")]

        [Space(-10f)]
        [Header("Gates"), Range(0f, 1f), Order(1)]
        public float gatesChance;

        [Space(5), Range(0f, 1f), Foldout("gate", "Gate Settings", 2)]
        public float characterGateChance;
        [Foldout("gate")]
        public List<WeightedItem<ExplicitGateAndBoosterSettings>> characterGateOptions;

        [Space(5), Range(0f, 1f), Foldout("gate")]
        public float weaponGateChance;
        [Foldout("gate")]
        public List<WeightedItem<ExplicitGateAndBoosterSettings>> weaponGateOptions;

        [Space(5), Range(0f, 1f), Foldout("gate")]
        public float healthGateChance;
        [Foldout("gate")]
        public List<WeightedItem<GateGenerationSettings>> healthGateSettings;

        [Space(5), Range(0f, 1f), Foldout("gate")]
        public float damageGateChance;
        [Foldout("gate")]
        public List<WeightedItem<GateGenerationSettings>> damageGateSettings;

        [Space(5), Range(0f, 1f), Foldout("gate")]
        public float fireRateGateChance;
        [Foldout("gate")]
        public List<WeightedItem<GateGenerationSettings>> fireRateGateSettings;

        [Space(5), Range(0f, 1f), Foldout("gate")]
        public float rangeGateChance;
        [Foldout("gate")]
        public List<WeightedItem<GateGenerationSettings>> rangeGateSettings;

        [Space(5), Range(0f, 1f), Foldout("gate")]
        public float moneyGateChance;
        [Foldout("gate")]
        public List<WeightedItem<GateGenerationSettings>> moneyGateSettings;

        /////////////////////////////////////////////////////////////////////

        [Header("Boosters"), Range(0f, 1f), Order(3)]
        public float boostersChance;

        [Space(5), Range(0f, 1f), Foldout("booster", "Boosters Settings", 4)]
        public float characterBoosterChance;
        [Foldout("booster")]
        public List<WeightedItem<ExplicitGateAndBoosterSettings>> characterBoosterOptions;

        [Space(5), Range(0f, 1f), Foldout("booster")]
        public float weaponBoosterChance;
        [Foldout("booster")]
        public List<WeightedItem<ExplicitGateAndBoosterSettings>> weaponBoosterOptions;

        [Space(5), Range(0f, 1f), Foldout("booster")]
        public float healthBoosterChance;
        [Foldout("booster")]
        public List<WeightedItem<BoosterGenerationSettings>> healthBoosterSettings;

        [Space(5), Range(0f, 1f), Foldout("booster")]
        public float damageBoosterChance;
        [Foldout("booster")]
        public List<WeightedItem<BoosterGenerationSettings>> damageBoosterSettings;

        [Space(5), Range(0f, 1f), Foldout("booster")]
        public float fireRateBoosterChance;
        [Foldout("booster")]
        public List<WeightedItem<BoosterGenerationSettings>> fireRateBoosterSettings;

        [Space(5), Range(0f, 1f), Foldout("booster")]
        public float rangeBoosterChance;
        [Foldout("booster")]
        public List<WeightedItem<BoosterGenerationSettings>> rangeBoosterSettings;

        /////////////////////////////////////////////////////////////////////

        [Header("Obstacles"), Range(0f, 1f), Order(5)]
        public float obstaclesChance;
        [Order(5)]
        public List<WeightedItem<ObstacleGenerationSettings>> obstacleGenerationSettings;

        /////////////////////////////////////////////////////////////////////

        [Header("Enemies"), Range(0f, 1f), Order(5)]
        public float enemiesChance;
        [Order(5)]
        public List<WeightedItem<EnemyGenerationSettings>> enemyGenerationSettings;

        /////////////////////////////////////////////////////////////////////
        [Space(5)]
        [LineSpacer("Environment"), LevelDataPicker(LevelDataType.Road), Order(5)]
        public string roadType;

        [LevelDataPicker(LevelDataType.Environment), Order(5)]
        public string environmentType;

    }

    [System.Serializable]
    public class ExplicitGateAndBoosterSettings
    {
        public string explicitID;
        public int explicitAmount = 1;
    }

    [System.Serializable]
    public class GateGenerationSettings
    {
        public OperationType operationType;
        public DuoFloat valueRange;
        public int decimalPlacesAfterRounding;
        public bool updateOnHit;
        [ShowIf("updateOnHit")]
        public float step;
    }

    [System.Serializable]
    public class BoosterGenerationSettings
    {
        public OperationType operationType;
        public DuoFloat valueRange;
        public int decimalPlacesAfterRounding;
    }

    [System.Serializable]
    public class EnemyGenerationSettings
    {
        public string enemyId;
        public DuoInt health;
        public DuoInt damage;
        public DuoFloat fireRate;
        public string gunId;
       
        // drop settings
        [Space(5)]
        public List<WeightedItem<DropSettings>> dropSettings;

        // drop weighted list used to get random settings
        private WeightedList<DropSettings> dropWeightedList = new WeightedList<DropSettings>();

        public void InitDropWeightedList()
        {
            dropWeightedList = new WeightedList<DropSettings>(dropSettings);
        }

        public DropSettings GetRandomDropSettings()
        {
            if(dropWeightedList != null)
            {
                return dropWeightedList.GetRandomItem();
            }

            return new DropSettings();
        }
    }

    [System.Serializable]
    public class ObstacleGenerationSettings
    {
        public string obstacleId;
        public DuoInt obstacleHealth;
        [Range(0f, 1f)]
        public float doubleObstacleChance;
        [Range(0f, 1f)]
        public float chanceToSpawnBoosterOnTop;
        
        // drop settings
        [Space(5)]
        public List<WeightedItem<DropSettings>> dropSettings;

        // drop weighted list used to get random settings
        private WeightedList<DropSettings> dropWeightedList = new WeightedList<DropSettings>();

        public void InitDropWeightedList()
        {
            dropWeightedList = new WeightedList<DropSettings>(dropSettings);
        }

        public DropSettings GetRandomDropSettings()
        {
            if (dropWeightedList != null && dropSettings.Count > 0)
            {
                return dropWeightedList.GetRandomItem();
            }

            return new DropSettings();
        }
    }

    [System.Serializable]
    public class DropSettings
    {
        public DropableItemType dropType = DropableItemType.None;
        public CurrencyType dropCurrencyType;
        public DuoInt dropItemsCount = new DuoInt(1, 1);
        public DuoInt dropItemValue = new DuoInt(1, 1);
    }

    public enum EditorSingleItemPerLinePositionType
    {
        Random = 0,
        Center = 1,
        Inside2Lines = 2,
        Inside3Lines = 3,
    }
}
