using UnityEngine;

namespace Watermelon
{
    public abstract class AbstractData
    {
        [SerializeField] string id;
        public string Id => id;

        [SerializeField] GameObject prefab;
        public GameObject Prefab => prefab;
    }
}
