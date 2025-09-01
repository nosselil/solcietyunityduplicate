using UnityEngine;
using UnityEngine.UI;

namespace Watermelon
{
    public class GateBehavior : OppositeMover, ITargetable, IClearable, ILevelInitable, IPickableGameplayElement
    {
        private const string PARTICLE_HIT_NAME = "Gate Hit";

        public GateLevelData Data { get; private set; }

        [SerializeField] Collider gateCollider;
        [SerializeField] ParticleSystem gatePickUpParticle;
        [SerializeField] Transform gateVisuals;
        [SerializeField] RectTransform bouncableVisuals;

        private bool IsInited { get; set; }
        public bool CanBePickedUp { get; set; }
        public float PosZ => Data.position.z;

        private TweenCase bounceCase;
        public event IPickableGameplayElement.PickedUp OnPickedUp;

        public virtual void Init(AbstractLevelData data)
        {
            Data = (GateLevelData)data;

            if (Data == null)
            {
                Debug.LogError("You are trying to init GateBehavior with the wrong data!");

                return;
            }

            transform.position = data.Position;

            RegisterMovement();

            IsInited = true;

            CanBePickedUp = true;
        }

        public virtual void GetHit(float damage, Vector3 hitPoint, float gunModifier)
        {
            ParticlesController.PlayParticle(PARTICLE_HIT_NAME).SetPosition(hitPoint);

            AudioController.PlaySound(AudioController.AudioClips.objectHit);

            if (!bounceCase.ExistsAndActive())
            {
                bounceCase = bouncableVisuals.DOScale(1.1f, 0.15f).SetCustomEasing(Ease.GetCustomEasingFunction("Reversable Bounce")).OnComplete(() => bouncableVisuals.localScale = Vector3.one);
            }
        }

        protected virtual void Apply(PlayerBehavior player)
        {
            if (gatePickUpParticle != null)
            {
                PlayParticle();
                gateVisuals.transform.DOScale(Vector3.zero, 0.4f).SetCustomEasing(Ease.GetCustomEasingFunction("BackInMiddle")).OnComplete(Clear);

                gateCollider.enabled = false;
            }
            else
            {
                Clear();
            }
        }

        protected void PlayGateSound(bool positive)
        {
            if (positive)
            {
                AudioController.PlaySound(AudioController.AudioClips.buff);
            }
            else
            {
                AudioController.PlaySound(AudioController.AudioClips.debuff);
            }
        }

        protected virtual void PlayParticle()
        {
            ParticlesController.PlayParticle(gatePickUpParticle).SetOnDisabled(Clear);
        }

        private void OnTriggerEnter(Collider other)
        {
            CharacterBehavior character = other.GetComponent<CharacterBehavior>();

            if (character != null && CanBePickedUp)
            {
                Apply(character.Player);

                OnPickedUp?.Invoke(this);
            }
        }

        public void Clear()
        {
            if (!IsInited) return;

            IsInited = false;

            if(this != null)
            {
                if (gameObject != null)
                    gameObject.SetActive(false);

                if (gateCollider != null)
                    gateCollider.enabled = true;

                if (gateVisuals != null)
                    gateVisuals.transform.localScale = Vector3.one;
            }

            RemoveMover();
        }
    }
}