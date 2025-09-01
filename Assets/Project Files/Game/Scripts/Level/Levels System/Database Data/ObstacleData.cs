using UnityEngine;

namespace Watermelon
{
    [System.Serializable]
    public class ObstacleData : AbstractData
    {
        [SerializeField] int defaultHealth = 5;
        public int DefaultHealth => defaultHealth;

        [SerializeField] float defaultBoosterHeight = 1f;
        public float DefaultBoosterHeight => defaultBoosterHeight;

        [SerializeField] float obstacleLengthAlongZ = 0.5f;
        public float ObstacleLengthAlongZ => obstacleLengthAlongZ;
    }
}
