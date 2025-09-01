using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Watermelon
{
    public class CharacterBehavior : MonoBehaviour, IWeaponPlacementReceiver
    {
        [Header("References")]
        [SerializeField] HealthBehavior healthBehavior;
        [SerializeField] Collider characterCollider;

        [Header("Particles")]
        [SerializeField] ParticleSystem hitParticle;
        [SerializeField] ParticleSystem moneyPickupParticle;

        [Header("Weapon")]
        [SerializeField] Transform gunHolder;
        public Transform GunHolder => gunHolder;

        [SerializeField] List<WeaponPlacementData> weaponPlacementData;
        public List<WeaponPlacementData> WeaponPlacementData => weaponPlacementData;

        public GameObject GameObject => gameObject;
        public HealthBehavior Health => healthBehavior;

        public CharacterData Data { get; private set; }

        public PlayerBehavior Player { get; private set; }
        protected GunBehavior GunBehavior { get; private set; }

        public float Damage { get; private set; }
        public float FireRate { get; private set; }

        public bool IsImmortal { get; private set; }
        public bool IsDead { get; protected set; }

        private Transform formationPosition;
        private Transform oldFormationPosition;
        private float formationT;
        private bool isFormationPositionChanging;

        private Coroutine shootingCoroutine;

        #region Initialization

        protected virtual void Awake()
        {
            Health.ShowOnChange = true;
            Health.HideOnFull = true;

            DisableCollider();
        }

        public virtual void Init(PlayerBehavior player, Transform formationPosition, float maxHealth)
        {
            Player = player;
            this.formationPosition = formationPosition;
            transform.position = formationPosition.position;

            Health.Initialise(maxHealth);
            Health.Restore();

            IsDead = false;

            EnableCollider();
        }

        public void SetData(CharacterData data)
        {
            Data = data;

            LoadGun();
        }

        #endregion

        #region Gun

        public void ReinitGun()
        {
            if (GunBehavior != null)
            {
                GunBehavior.transform.SetParent(PoolManager.DefaultContainer);
                GunBehavior.gameObject.SetActive(false);
            }

            LoadGun();
        }

        protected virtual void LoadGun()
        {
            if (Data.IsGunLocked)
            {
                GunBehavior = LevelController.SkinsManager.GetGun(Data.LockedGunId);
            }
            else
            {
                if (Player == null)
                {
                    GunBehavior = LevelController.SkinsManager.GetGun(PlayerBehavior.Instance.GunId);
                }
                else
                {
                    GunBehavior = LevelController.SkinsManager.GetGun(Player.GunId);
                }

            }

            GunBehavior.transform.SetParent(gunHolder);

            bool placedWeapon = false;
            for (int i = 0; i < weaponPlacementData.Count; i++)
            {
                WeaponPlacementData placementData = weaponPlacementData[i];

                if (placementData.WeaponId == GunBehavior.GunData.Id)
                {
                    TransferWeaponData(placementData.WeaponId, GunBehavior.transform);

                    placedWeapon = true;
                    break;
                }
            }

            if (!placedWeapon)
            {
                GunBehavior.transform.localPosition = Vector3.zero;
                GunBehavior.transform.localRotation = Quaternion.identity;
                GunBehavior.transform.localScale = Vector3.one;
            }

            FireRate = GunBehavior.GunData.FireRateMultiplier;

            if (Player != null)
            {
                Player.RecalculateStatsForCharacter(this);
            }

        }

        public void RecalculateBulletRange(List<StatModifier> modifiers, float minRange)
        {
            if (GunBehavior != null)
                GunBehavior.RecalculateBulletRange(modifiers, minRange);
        }

        #endregion

        #region Movement

        public void ChangeFormationPosition(Transform newFormationPosition)
        {
            isFormationPositionChanging = true;

            oldFormationPosition = formationPosition;
            formationPosition = newFormationPosition;

            formationT = 0;

            Tween.DoFloat(0, 1, 0.5f, (value) => { formationT = value; }).SetEasing(Ease.Type.SineInOut).OnComplete(() =>
            {
                isFormationPositionChanging = false;
                oldFormationPosition = null;
                formationT = 0;
            });
        }

        public void UpdatePosition(Vector3 shift)
        {
            if (isFormationPositionChanging)
            {
                Vector3 lerpedPosition = Vector3.Lerp(oldFormationPosition.position, formationPosition.position, formationT);
                transform.position = lerpedPosition + shift;
            }
            else
            {
                transform.position = formationPosition.position + shift;
            }

        }

        public virtual void EnableMovingAnimation()
        {

        }

        public virtual void DisableMovingAnimation()
        {

        }

        public virtual void DisableHealthbar()
        {
            if (Health.MaxHealth == 0)
                Health.Initialise(1);
            Health.Restore();
        }

        #endregion

        #region Shooting

        public void StartShooting()
        {
            if (shootingCoroutine == null)
            {
                shootingCoroutine = StartCoroutine(ShootingCoroutine());
            }
        }

        public void StopShooting()
        {
            if (shootingCoroutine != null)
            {
                StopCoroutine(shootingCoroutine);

                shootingCoroutine = null;
            }
        }

        private IEnumerator ShootingCoroutine()
        {
            // Helps with unusual bullet behavior on revive
            yield return null;

            while (true)
            {
                if (GameController.IsGameplayActive)
                {
                    PlayShootingAnimation();

                    GunBehavior.Shoot(Damage, true);

                    AudioController.AudioClips.shotClipHandler.Play(AudioController.AudioClips.shot);
                }

                yield return new WaitForSeconds(1f / FireRate);
            }
        }

        #endregion

        #region Combat

        public virtual void PlayShootingAnimation()
        {

        }

        public void MakeImmortal()
        {
            IsImmortal = true;
        }

        public void RemoveImmortality()
        {
            IsImmortal = false;
        }

        public void TakeDamage(float damage, Vector3 hitPoint)
        {
            if (IsImmortal)
                return;

            if (hitParticle != null)
                hitParticle.Play();

            Health.Subtract(damage);

            Haptic.Play(Haptic.HAPTIC_LIGHT);

            if (Health.IsDepleted)
            {
                Die();
            }
            else
            {
                PlayGetHitAnimation();
            }
        }

        public void AddHealth(StatModifier modifier)
        {
            var newHealth = modifier.ApplyValue(Health.CurrentHealth);

            Health.SetHealth(newHealth);

            if (Health.IsDepleted)
            {
                Die();
            }
        }

        public void AddHealth(float value)
        {
            Health.Add(value);
        }

        public virtual void PlayGetHitAnimation()
        {

        }

        public virtual void Die()
        {
            if (IsDead)
                return;

            IsDead = true;

            Player.OnCharacterDied(this);

            DisableCollider();

            Clear();
        }

        public virtual void Clear()
        {
            if (GunBehavior != null)
            {
                GunBehavior.transform.SetParent(PoolManager.DefaultContainer);
                GunBehavior.gameObject.SetActive(false);

                GunBehavior = null;
            }

            StopShooting();

            Health.Restore();

            if (gameObject != null)
                Destroy(gameObject);
        }

        #endregion

        #region Boosters

        public void RecalculateFireRate(List<StatModifier> modifiers, float minFireRate)
        {
            float prevFireRate = FireRate;

            FireRate = 0;

            for (int i = 0; i < modifiers.Count; i++)
            {
                StatModifier modifier = modifiers[i];

                FireRate = modifier.ApplyValue(FireRate);
            }

            FireRate *= GunBehavior.GunData.FireRateMultiplier;

            if (FireRate < minFireRate)
                FireRate = minFireRate;

            if (prevFireRate != FireRate && shootingCoroutine != null)
            {
                StopShooting();
                StartShooting();
            }
        }

        public void RecalculateDamage(List<StatModifier> modifiers, float minDamage)
        {
            Damage = 0;

            for (int i = 0; i < modifiers.Count; i++)
            {
                StatModifier modifier = modifiers[i];

                Damage = modifier.ApplyValue(Damage);
            }

            Damage *= GunBehavior.GunData.DamageMultiplier;

            if (Damage < minDamage)
                Damage = minDamage;
        }

        #endregion

        #region Physics

        public void EnableCollider()
        {
            characterCollider.enabled = true;
        }

        public void DisableCollider()
        {
            characterCollider.enabled = false;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.layer == 9) // Obstacle
            {
                ITargetable target = other.GetComponent<ITargetable>();

                if (target != null)
                {
                    if (target is ObstacleBehavior obstacle)
                    {
                        float obstacleHealth = obstacle.Health;
                        float health = Health.CurrentHealth;

                        TakeDamage(obstacleHealth, transform.position);

                        if (obstacleHealth >= health)
                        {
                            obstacle.GetHit(health, transform.position, 1);
                        }
                        else
                        {
                            obstacle.Destroy(this);
                        }

                    }
                    else if (target is EnemyBehavior enemy)
                    {
                        float enemyHealth = enemy.Health.CurrentHealth;
                        enemy.GetHit(Health.CurrentHealth, transform.position, 1);
                        TakeDamage(enemyHealth, transform.position);
                    }
                }
            }
            else if (other.gameObject.layer == 13) // Finish
            {
                if (other.GetComponent<FirstStageFinishBehavior>() != null)
                {
                    LevelController.ReachedSecondStage();
                }
            }
        }

        #endregion

        #region Weapon Placement

        public void TransferWeaponData(string weaponId, Transform weaponTransform)
        {
            if (weaponPlacementData == null)
                weaponPlacementData = new List<WeaponPlacementData>();

            for (int i = 0; i < weaponPlacementData.Count; i++)
            {
                WeaponPlacementData data = weaponPlacementData[i];
                if (data.WeaponId == weaponId)
                {
                    data.Apply(weaponTransform);

                    return;
                }
            }

            weaponPlacementData.Add(new WeaponPlacementData(weaponId, weaponTransform));
        }

        public void SetWeaponData(string weaponId, Transform weaponTransform)
        {
            if (weaponPlacementData == null)
                weaponPlacementData = new List<WeaponPlacementData>();

            for (int i = 0; i < weaponPlacementData.Count; i++)
            {
                WeaponPlacementData data = weaponPlacementData[i];
                if (data.WeaponId == weaponId)
                {
                    data.SetData(weaponTransform);

                    return;
                }
            }

            weaponPlacementData.Add(new WeaponPlacementData(weaponId, weaponTransform));
        }

        public bool HasWeaponDataChanged(string weaponId, Transform weaponTransform)
        {
            if (weaponPlacementData == null)
                weaponPlacementData = new List<WeaponPlacementData>();

            for (int i = 0; i < weaponPlacementData.Count; i++)
            {
                WeaponPlacementData data = weaponPlacementData[i];
                if (data.WeaponId == weaponId)
                {
                    return data.HasDataChanged(weaponTransform);
                }
            }

            return true;
        }

        public void CloneWeaponPlacementData(IWeaponPlacementReceiver other)
        {
            if (weaponPlacementData == null)
                weaponPlacementData = new List<WeaponPlacementData>();
            weaponPlacementData.Clear();

            for (int i = 0; i < other.WeaponPlacementData.Count; i++)
            {
                weaponPlacementData.Add(other.WeaponPlacementData[i].Clone());
            }
        }

        #endregion

        public void OnMoneyPickedUp()
        {
            if (moneyPickupParticle != null)
                moneyPickupParticle.Play();
        }
    }

    public enum ShootingImpactType
    {
        None = 0,
        Shift = 1,
        Animation = 2,
        Both = 3,
    }
}