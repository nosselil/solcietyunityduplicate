using UnityEngine;

namespace Watermelon
{
    [System.Serializable]
    public class EnvironmentData : AbstractData
    {
        [SerializeField] float lengthAlongZ;
        public float LengthAlongZ => lengthAlongZ;
    }
}
