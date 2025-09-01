using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Watermelon
{
    public class GunBehavior : MonoBehaviour
    {
        [SerializeField] Transform shootPointTransform;
        [SerializeField] GameObject bulletPrefab;
        [SerializeField] bool magnetBullets;

        [Space]
        [SerializeField] ParticleSystem muzzleParticle;

        public GunData GunData { get; private set; }

        private PoolGeneric<BulletBehavior> bulletPool;
        private float BulletRange { get; set; }

        private void Awake()
        {
            string booletPoolName = "PlayerBullet_" + bulletPrefab.name;
            if (!PoolManager.PoolExists(booletPoolName))
            {
                bulletPool = new PoolGeneric<BulletBehavior>(bulletPrefab, booletPoolName);
            }
            else
            {
                bulletPool = (PoolGeneric<BulletBehavior>)PoolManager.GetPoolByName(booletPoolName);
            }
        }

        private void OnDestroy()
        {
            bulletPool?.Destroy();
        }

        public void RecalculateBulletRange(List<StatModifier> modifiers, float minRange)
        {
            BulletRange = GunData.BulletRangeMultiplier;

            for (int i = 0; i < modifiers.Count; i++)
            {
                StatModifier statModifier = modifiers[i];

                BulletRange = statModifier.ApplyValue(BulletRange);
            }

            if (BulletRange < minRange) BulletRange = minRange;
        }

        public void Shoot(float damage, bool isPlayerShooting)
        {
            for (int i = 0; i < GunData.BulletsCount; i++)
            {
                BulletBehavior bullet = bulletPool.GetPooledComponent();

                bullet.transform.position = shootPointTransform.position;
                bullet.transform.rotation = Quaternion.identity;
                bullet.transform.localScale = Vector3.one;

                float bulletDamage = damage;

                Vector3 direction = isPlayerShooting ? Vector3.forward : Vector3.back;

                float spread = GunData.Spread;
                float halfSpread = GunData.Spread / 2f;

                if (!Mathf.Approximately(spread, 0))
                {
                    float angle = Random.Range(-halfSpread, halfSpread);

                    if(i == 0)
                    {
                        angle *= 0.1f;
                    }

                    Quaternion rotation = Quaternion.Euler(0, angle, 0);
                    direction = rotation * direction;
                }

                bullet.transform.rotation = Quaternion.FromToRotation(Vector3.forward, direction);

                bool applyMagnet = magnetBullets;

                // disable magnet for enemies
                if (!isPlayerShooting)
                    applyMagnet = false;

                bullet.Init(this, bulletDamage, GunData.BulletSpeed, BulletRange, isPlayerShooting, applyMagnet);

                if (muzzleParticle != null) muzzleParticle.Play();
            }
        }

        public void SetData(GunData data)
        {
            GunData = data;
        }
    }
}