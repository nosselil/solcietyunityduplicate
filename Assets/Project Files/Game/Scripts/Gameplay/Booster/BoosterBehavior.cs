using UnityEngine;

namespace Watermelon
{
    public abstract class BoosterBehavior : OppositeMover, ILevelInitable, IClearable, IPickableGameplayElement
    {
        protected BoosterLevelData BoosterData { get; private set; }

        protected ObstacleBehavior linkedObstacle;

        [SerializeField] protected Collider boosterCollider;
        [SerializeField] bool applyOnObstacleDestroyed;
        [SerializeField] ParticleSystem pickUpParticle;
        [SerializeField] Transform visuals;

        public bool CanBePickedUp { get; set; }
        public float PosZ => BoosterData.position.z;

        protected abstract void Apply(PlayerBehavior character);

        private bool IsInited { get; set; }

        public event IPickableGameplayElement.PickedUp OnPickedUp;

        private TweenCase floatTweenCase;

        public virtual void Init(AbstractLevelData data)
        {
            BoosterData = (BoosterLevelData)data;

            if (BoosterData == null)
            {
                Debug.LogError("You are trying to init BoosterBehavior with the wrong data!");

                return;
            }

            transform.position = BoosterData.Position;

            Init();

            RegisterMovement();

            IsInited = true;

            visuals.localScale = Vector3.one;

            CanBePickedUp = true;
        }

        protected virtual void Init()
        {
            boosterCollider.enabled = linkedObstacle == null;
        }

        public void LinkToObstacle(ObstacleBehavior obstacle)
        {
            linkedObstacle = obstacle;
            boosterCollider.enabled = false;

            transform.position = transform.position.SetY(obstacle.BoosterHeight);

            ObstacleBehavior.OnObstacleDestroyed += OnObstacleDestroyed;
        }

        private void OnObstacleDestroyed(ObstacleBehavior obstacle)
        {
            if (obstacle != linkedObstacle) return;
            ObstacleBehavior.OnObstacleDestroyed -= OnObstacleDestroyed;
            linkedObstacle = null;

            if (applyOnObstacleDestroyed)
            {
                CharacterBehavior firstCharacter = PlayerBehavior.Instance.GetFirstCharacter();
                if (firstCharacter != null)
                {
                    float distance = Vector3.Distance(transform.position, firstCharacter.transform.position);

                    float time = distance / 20;

                    Vector3 startPosition = transform.position;

                    floatTweenCase = Tween.DoFloat(0, 1, time, (float t) =>
                    {
                        transform.position = Vector3.Lerp(startPosition, firstCharacter.transform.position, t);
                    }).SetEasing(Ease.Type.SineIn).OnComplete(() => {
                        PickUpBooster(PlayerBehavior.Instance);
                    });
                }
            }
            else
            {
                boosterCollider.enabled = true;
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            CharacterBehavior character = other.GetComponent<CharacterBehavior>();

            if (character != null && CanBePickedUp)
            {
                PickUpBooster(character.Player);
                OnPickedUp?.Invoke(this);
            }
        }

        protected virtual void PickUpBooster(PlayerBehavior player)
        {
            Apply(player);

            if (pickUpParticle != null)
            {
                ParticlesController.PlayParticle(pickUpParticle).SetOnDisabled(Clear);
            }

            visuals.DOScale(0, 0.4f).SetCustomEasing(Ease.GetCustomEasingFunction("BackInMiddle")).OnComplete(Clear);

            PlayBoosterSound();
        }

        private void PlayBoosterSound()
        {
            // if it's numerical and negative
            if (BoosterData.BoosterType != BoosterType.GiveWeapon && BoosterData.BoosterType != BoosterType.GiveCharacter && (BoosterData.OperationType == OperationType.Minus || BoosterData.OperationType == OperationType.Divide))
            {
                AudioController.PlaySound(AudioController.AudioClips.debuff);
            }
            else
            {
                AudioController.PlaySound(AudioController.AudioClips.buff);
            }
        }

        public virtual void Clear()
        {
            if (!IsInited) return;

            floatTweenCase.KillActive();

            IsInited = false;

            linkedObstacle = null;

            if(this != null)
            {
                if (boosterCollider != null)
                    boosterCollider.enabled = true;

                if (gameObject != null)
                    gameObject.SetActive(false);

                if (visuals != null)
                    visuals.localScale = Vector3.one;
            }

            RemoveMover();
        }
    }
}