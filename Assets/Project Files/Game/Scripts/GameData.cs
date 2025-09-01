using UnityEngine;

namespace Watermelon
{
    [CreateAssetMenu(fileName = "Game Data", menuName = "Data/Game Data")]
    public class GameData : ScriptableObject
    {
        [SerializeField] bool loadLevelInMainMenu;
        public bool LoadLevelInMainMenu => loadLevelInMainMenu;

        [SerializeField] bool randomizeLevelsAfterReachingLast;
        public bool RandomizeLevelsAfterReachingLast => randomizeLevelsAfterReachingLast;

        [SerializeField] FloatToggle invulnerabilityAfterRevive;
        public bool IsInvulnerableAfterRevive => invulnerabilityAfterRevive.Enabled;
        public float InvulnerabilityAfterReviveDuration => invulnerabilityAfterRevive.Value;
    }
}
