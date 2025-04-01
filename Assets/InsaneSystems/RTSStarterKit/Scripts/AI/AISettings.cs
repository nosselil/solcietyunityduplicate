using UnityEngine;

namespace InsaneSystems.RTSStarterKit.AI
{
    [CreateAssetMenu(fileName = "AISettings", menuName = "RTS Starter Kit/AI Settings")]
    public class AISettings : ScriptableObject
    {
        [Tooltip("Time in seconds represents delay between any AI actions")]
        [Range(0f, 20f)] public float thinkTime = 0.2f;

        [Tooltip("Delay from game start in seconds before AI will start build any buildings.")]
        [Range(0f, 720f)] public float delayBeforeStartCreateBuildings = 0f;

        [Tooltip("Delay from game start in seconds before AI will start build any attacking units.")]
        [Range(0f, 720f)] public float delayBeforeStartBuyingUnits = 0f;

        [Tooltip("Priority of AI Building. First buildings have bigger priority in AI building queue.")]
        public UnitData[] buildingPriority;

        [Tooltip("Add in this list all categories, which contains units and allowed for build by AI.")]
        public ProductionCategory[] unitsCategories;

        [Tooltip("The faction that this AI will use.")]
        public FactionData faction; // Add this line
    }
}