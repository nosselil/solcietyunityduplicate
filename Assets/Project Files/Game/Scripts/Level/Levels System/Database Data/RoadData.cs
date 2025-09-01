using UnityEngine;

namespace Watermelon
{
    [System.Serializable]
    public class RoadData : AbstractData
    {
        [SerializeField] float lengthAlongZ;
        public float LengthAlongZ => lengthAlongZ;
        [SerializeField] float roadWidth;
        public float RoadWidth => roadWidth;
    }
}
