using System;
using TMPro;
using UnityEngine;

namespace Watermelon
{
    public class ObstacleBehavior : OppositeMover, ITargetable, ILevelInitable, IClearable
    {
        private static readonly int ROLLING_SPEED_HASH = Animator.StringToHash("Rolling Speed");
        private static readonly int EMISSION_COLOR_HASH = Shader.PropertyToID("_EmissionColor");
        private static readonly int REVERSABLE_BOUNCE_HASH = "Reversable Bounce".GetHashCode();

        [SerializeField] string hitParticleName;
        [SerializeField] float autoPickUpDelay = 0.5f;

        [Header("References")]
        [SerializeField] TMP_Text healthText;
        [SerializeField] ParticleSystem destructionParticle;
        [SerializeField] Collider obstacleCollider;
        [SerializeField] Transform visuals;
        [SerializeField] Animator animator;
        [SerializeField] MeshRenderer meshRenderer;

        [Space]
        [SerializeField] DropFallingStyle dropFallingStyle;

        public ObstacleLevelData LevelData { get; private set; }

        public int MaxHealth => LevelData.Health;
        public float Health { get; private set; }

        public float BoosterHeight => LevelData.BoosterHeight;

        public DropableItemType DropType => LevelData.DropType;
        public float DropItemValue => LevelData.DropItemValue;

        public delegate void ObstacleCallback(ObstacleBehavior obstacle);
        public static event ObstacleCallback OnObstacleDestroyed;

        private float rotationSpeed;
        private float desiredRotationSpeed;

        private TweenCase bounceCase;
        private TweenCase overlayCase;
        private TweenCaseCollection dropTweenCollection;

        private ParticleCase particleCase;

        public void Init(AbstractLevelData data)
        {
            LevelData = (ObstacleLevelData)data;

            if (LevelData == null)
            {
                Debug.LogError("You are trying to init Obstaclebehavior with the wrong data!");

                return;
            }

            transform.position = LevelData.Position;

            Health = MaxHealth;
            UpdateText();

            obstacleCollider.enabled = true;

            RegisterMovement();

            rotationSpeed = 0;
            animator.SetFloat(ROLLING_SPEED_HASH, 0);
        }

        protected override void FixedUpdate()
        {
            base.FixedUpdate();

            if (GameController.IsGameplayActive)
            {
                if (rotationSpeed != desiredRotationSpeed)
                {
                    rotationSpeed = desiredRotationSpeed;
                    animator.SetFloat(ROLLING_SPEED_HASH, rotationSpeed);
                }
            }
            else
            {
                if (rotationSpeed != 0)
                {
                    rotationSpeed = 0;
                    animator.SetFloat(ROLLING_SPEED_HASH, 0);
                }
            }
        }

        public override void SetMoveSpeed(bool isMoving, float moveSpeed)
        {
            base.SetMoveSpeed(isMoving, moveSpeed);

            desiredRotationSpeed = isMoving ? moveSpeed / 5f : 0;
        }

        public float GetDamage()
        {
            return Health;
        }

        public void GetHit(float damage, Vector3 hitPoint, CharacterBehavior character)
        {
            if (Health <= 0)
                return;

            Health -= damage;

            AudioController.PlaySound(AudioController.AudioClips.objectHit);

            if (Health <= 0)
            {
                obstacleCollider.enabled = false;

                OnObstacleDestroyed?.Invoke(this);
                if (destructionParticle != null)
                {
                    visuals.gameObject.SetActive(false);
                    particleCase = ParticlesController.PlayParticle(destructionParticle).SetOnDisabled(OnParticleDisabled);
                }
                else
                {
                    Clear();
                }

                AudioController.PlaySound(AudioController.AudioClips.obstcleDestroyed, 0.4f);
                Haptic.Play(Haptic.HAPTIC_LIGHT);

                if (LevelData.DropType != DropableItemType.None)
                {
                    dropTweenCollection = Tween.BeginTweenCaseCollection();
                    for (int i = 0; i < LevelData.DropItemsCount; i++)
                    {
                        GameObject dropObject = Drop.DropItem(new DropData() { amount = (int)LevelData.DropItemValue, currencyType = LevelData.DropCurrencyType, dropType = LevelData.DropType }, transform.position, Vector3.zero, dropFallingStyle);

                        if (character != null)
                        {
                            IDropableItem dropItem = dropObject.GetComponent<IDropableItem>();

                            Tween.DelayedCall(autoPickUpDelay, () => dropItem.Pick(character));
                        }
                    }
                    Tween.EndTweenCaseCollection();
                }
            }
            else
            {
                if (!bounceCase.ExistsAndActive())
                {
                    bounceCase = visuals.DOScale(1.1f, 0.15f).SetCustomEasing(Ease.GetCustomEasingFunction(REVERSABLE_BOUNCE_HASH)).OnComplete(() => visuals.localScale = Vector3.one);
                }

                if (!overlayCase.ExistsAndActive())
                {
                    overlayCase = meshRenderer.material.DOColor(EMISSION_COLOR_HASH, Color.red, 0.15f).SetCustomEasing(Ease.GetCustomEasingFunction(REVERSABLE_BOUNCE_HASH)).OnComplete(() => 
                    { 
                        if(meshRenderer != null)
                        {
                            meshRenderer.material.SetColor(EMISSION_COLOR_HASH, Color.clear);
                        }
                    });
                }

                if (ParticlesController.HasParticle(hitParticleName))
                {
                    ParticlesController.PlayParticle(hitParticleName).SetPosition(hitPoint);
                }

                UpdateText();
            }
        }

        public void GetHit(float damage, Vector3 hitPoint, float gunModifier)
        {
            GetHit(damage, hitPoint, null);
        }

        public void Destroy(CharacterBehavior character)
        {
            GetHit(Health + 1, transform.position, character);
        }

        private void UpdateText()
        {
            healthText.text = Mathf.CeilToInt(Health).ToString();
        }

        private void OnParticleDisabled()
        {
            particleCase = null;

            Clear();
        }

        public void Clear()
        {
            overlayCase.KillActive();
            bounceCase.KillActive();

            dropTweenCollection.KillActive();

            if(this != null)
            {
                if (gameObject != null)
                    gameObject.SetActive(false);

                if (visuals != null)
                    visuals.gameObject.SetActive(true);

                if (particleCase != null)
                    particleCase.ForceDisable(ParticleSystemStopBehavior.StopEmittingAndClear);
            }

            RemoveMover();
        }
    }
}