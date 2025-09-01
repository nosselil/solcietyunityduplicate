using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Watermelon
{
    public class EnemyBehavior : OppositeMover, ITargetable, ILevelInitable, IClearable, IWeaponPlacementReceiver
    {
        private static readonly int MOVEMENT_SPEED_FLOAT_HASH = Animator.StringToHash("Movement Speed");

        [SerializeField] Animator animator;

        [SerializeField] CharacterGraphicsBehavior characterGraphicsBehavior;
        [SerializeField] Collider enemyCollider;

        [SerializeField] Transform gunHolder;

        [SerializeField] EnemyAnimationCallbackHandler enemyAnimationCallbackHandler;

        [SerializeField] ParticleSystem hitParticle;
        [SerializeField] Renderer enemyRenderer;

        [SerializeField] List<WeaponPlacementData> weaponPlacementData;
        public List<WeaponPlacementData> WeaponPlacementData => weaponPlacementData;

        public EnemyLevelData LevelData { get; private set; }

        public GameObject GameObject => gameObject;
        public Transform GunHolder => gunHolder;

        public HealthBehavior Health { get; private set; }
        public float Damage { get; private set; }

        private GunBehavior gunBehavior;

        private Coroutine shootingCoroutine;

        private TweenCase overlayCase;
        private TweenCase disablingCase;

        private FloatingTextBehavior floatingText;
        private float lastTimeFloatingText;
        private float flyingDamage;

        public void Init(AbstractLevelData data)
        {
            LevelData = (EnemyLevelData)data;

            if (LevelData == null)
            {
                Debug.LogError("You are trying to init EnemyBehavior with the wrong data!");

                return;
            }

            Health = GetComponent<HealthBehavior>();
            Health.Initialise(LevelData.Health);

            Health.ShowOnChange = true;
            Health.HideOnFull = true;

            Damage = LevelData.Damage;

            transform.position = LevelData.Position;
            transform.rotation = Quaternion.Euler(0, 180, 0);

            RegisterMovement();

            LoadGun();

            StopShooting();

            enemyAnimationCallbackHandler.Init(this);

            disablingCase.KillActive();

            characterGraphicsBehavior.Init();

            enemyCollider.enabled = true;
        }

        private void LoadGun()
        {
            string gunId = LevelData.GunId;
            if(gunId == "none")
            {
                characterGraphicsBehavior.SetShootingPose(null);

                return;
            }

            gunBehavior = LevelController.SkinsManager.GetGun(gunId);

            if (gunBehavior != null)
            {
                gunBehavior.transform.SetParent(gunHolder);

                bool placedWeapon = false;
                for (int i = 0; i < weaponPlacementData.Count; i++)
                {
                    WeaponPlacementData placementData = weaponPlacementData[i];

                    if (placementData.WeaponId == gunBehavior.GunData.Id)
                    {
                        TransferWeaponData(placementData.WeaponId, gunBehavior.transform);

                        placedWeapon = true;
                        break;
                    }
                }

                if (!placedWeapon)
                {
                    gunBehavior.transform.localPosition = Vector3.zero;
                    gunBehavior.transform.localRotation = Quaternion.identity;
                    gunBehavior.transform.localScale = Vector3.one;
                }

                gunBehavior.RecalculateBulletRange(new List<StatModifier>(), 0.1f);

                characterGraphicsBehavior.SetShootingPose(gunBehavior.GunData.PoseAnimation);
            }
            else
            {
                characterGraphicsBehavior.SetShootingPose(null);
            }
        }

        private IEnumerator ShootingCoroutine()
        {
            while (true)
            {
                if (GameController.IsGameplayActive)
                {
                    characterGraphicsBehavior.PlayShootingImpactAnimation();
                    
                    if (gunBehavior != null) gunBehavior.Shoot(Damage, false);
                }

                yield return new WaitForSeconds(1 / LevelData.FireRate);
            }
        }

        public override void SetMoveSpeed(bool isMoving, float moveSpeed)
        {
            base.SetMoveSpeed(isMoving, moveSpeed);

            animator.SetFloat(MOVEMENT_SPEED_FLOAT_HASH, isMoving ? 1 : 0);
        }

        public void GetHit(float damage, Vector3 hitPoint, float gunModifier)
        {
            Health.Subtract(damage);

            if (hitParticle != null) hitParticle.Play();

            if (Health.IsDepleted)
            {
                Die();
            }
            else
            {
                if (!overlayCase.ExistsAndActive())
                {
                    overlayCase = enemyRenderer.material.DOColor(Shader.PropertyToID("_EmissionColor"), Color.red, 0.15f).SetCustomEasing(Ease.GetCustomEasingFunction("Reversable Bounce")).OnComplete(() => enemyRenderer.material.SetColor(Shader.PropertyToID("_EmissionColor"), Color.clear));
                }

                FloatDamageText(damage);

                characterGraphicsBehavior.PlayShootingImpactAnimation();
            }
        }

        private void Die()
        {
            if (LevelData.DropType != DropableItemType.None)
            {
                for (int i = 0; i < LevelData.DropItemsCount; i++)
                {
                    Drop.DropItem(new DropData() { amount = (int)LevelData.DropItemValue, currencyType = LevelData.DropCurrencyType, dropType = LevelData.DropType }, transform.position, Vector3.zero, DropFallingStyle.Default);
                }
            }

            RemoveMover();

            StopShooting();

            characterGraphicsBehavior.PlayDyingAnimation();
            characterGraphicsBehavior.DoDeathOverlayColor();

            disablingCase = transform.DOMove(transform.position.AddToZ(3f), 2f).SetEasing(Ease.Type.QuintOut).OnComplete(Clear);

            enemyCollider.enabled = false;

            AudioController.PlaySound(AudioController.AudioClips.screams.GetRandomItem());
            Haptic.Play(Haptic.HAPTIC_LIGHT);
        }

        private void FloatDamageText(float damage)
        {
            if (floatingText != null && Time.time - lastTimeFloatingText < 0.3f)
            {
                flyingDamage += damage;

                string damageText = $"-{Mathf.RoundToInt(flyingDamage * 10f) / 10f}";
                floatingText.SetText(damageText);
            }
            else
            {
                flyingDamage = damage;

                string damageText = $"-{Mathf.RoundToInt(flyingDamage * 10f) / 10f}";
                floatingText = FloatingTextController.SpawnFloatingText("Damage", damageText, transform.position.AddToY(2f), Quaternion.identity, 1.0f, Color.red) as FloatingTextBehavior;
            }

            lastTimeFloatingText = Time.time;
        }

        public void Clear()
        {
            RemoveMover();
            StopShooting();

            if(this != null)
            {
                if (gameObject != null)
                    gameObject.SetActive(false);

                if (gunBehavior != null)
                {
                    gunBehavior.transform.SetParent(PoolManager.DefaultContainer);
                    gunBehavior.gameObject.SetActive(false);
                    gunBehavior = null;
                }

                if (enemyCollider != null)
                    enemyCollider.enabled = true;
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (gunBehavior == null) return;
            if (other.GetComponent<TargetDetector>() != null)
            {
                StartShooting();
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (gunBehavior == null) return;
            if (other.GetComponent<TargetDetector>() != null)
            {
                StopShooting();
            }
        }

        public void Shoot()
        {
            if (gunBehavior != null) gunBehavior.Shoot(Damage, false);
        }

        public void StartShooting()
        {
            if (shootingCoroutine == null)
            {
                shootingCoroutine = StartCoroutine(ShootingCoroutine());
            }
        }

        public void StopShooting()
        {
            if (this == null) return;

            if (shootingCoroutine != null)
            {
                StopCoroutine(shootingCoroutine);

                shootingCoroutine = null;
            }
        }

        #region Weapon Placement

        public void TransferWeaponData(string weaponId, Transform weaponTransform)
        {
            if (weaponPlacementData == null) weaponPlacementData = new List<WeaponPlacementData>();

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
            if (weaponPlacementData == null) weaponPlacementData = new List<WeaponPlacementData>();

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
            if (weaponPlacementData == null) weaponPlacementData = new List<WeaponPlacementData>();

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
            if (weaponPlacementData == null) weaponPlacementData = new List<WeaponPlacementData>();
            weaponPlacementData.Clear();

            for (int i = 0; i < other.WeaponPlacementData.Count; i++)
            {
                weaponPlacementData.Add(other.WeaponPlacementData[i].Clone());
            }
        }

        #endregion
    }
}
