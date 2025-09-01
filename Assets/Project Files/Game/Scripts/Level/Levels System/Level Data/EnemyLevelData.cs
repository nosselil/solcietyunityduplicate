using UnityEngine;

namespace Watermelon
{
    [System.Serializable]
    public class EnemyLevelData : AbstractLevelDataWithDrop
    {
        public float health;
        public float Health => health;

        [SerializeField] float damage;
        public float Damage => damage;

        [SerializeField] float fireRate;
        public float FireRate => fireRate;

        [SerializeField] string gunId;
        public string GunId => gunId;
    }
}
