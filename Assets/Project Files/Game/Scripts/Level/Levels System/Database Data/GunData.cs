using UnityEngine;

namespace Watermelon
{
    [System.Serializable]
    public class GunData : AbstractData
    {
        [SerializeField] bool isDefault = false;
        public bool IsDefault => isDefault;

        [SerializeField] float fireRateMultiplier = 1f;
        public float FireRateMultiplier => fireRateMultiplier;

        [SerializeField] float damageMultiplier = 1f;
        public float DamageMultiplier => damageMultiplier;

        [SerializeField] float bulletRangeMultiplier = 1f;
        public float BulletRangeMultiplier => bulletRangeMultiplier;

        [SerializeField] float bulletSpeed = 15f;
        public float BulletSpeed => bulletSpeed;

        [SerializeField] int bulletsCount = 1;
        public int BulletsCount => bulletsCount;

        [SerializeField] float spread = 0f;
        public float Spread => spread;

        [SerializeField] Sprite previewSprite;
        public Sprite PreviewSprite => previewSprite;

        [SerializeField] AnimationClip poseAnimation;
        public AnimationClip PoseAnimation => poseAnimation;

        [SerializeField] float shootingImpactDistance = 0.1f;
        public float ShootingImpactDistance => shootingImpactDistance;

        [SerializeField] float shootingImpactDuration = 0.1f;
        public float ShootingImpactDuration => shootingImpactDuration;

        [SerializeField] AnimationCurve shootingImpactCurve;
        public AnimationCurve ShootingImpactCurve => shootingImpactCurve;

        [SerializeField] string displayName;
        public string DisplayName => displayName;
    }
}
