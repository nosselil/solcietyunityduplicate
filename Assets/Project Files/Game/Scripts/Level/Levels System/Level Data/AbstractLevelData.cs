using UnityEngine;

namespace Watermelon
{
    public abstract class AbstractLevelData
    {
        public string id;
        public string Id => id;

        public Vector3 position;
        public Vector3 Position => position;
    }
}
