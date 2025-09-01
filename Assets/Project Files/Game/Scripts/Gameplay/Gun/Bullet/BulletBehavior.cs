using UnityEngine;

namespace Watermelon
{
    public class BulletBehavior : MonoBehaviour
    {
        private const string HIT_PARTICLE_NAME = "Hit";

        [SerializeField] TrailRenderer trailRenderer;

        protected float damage;
        protected float speed;

        protected float selfDestroyDistance;
        protected float distanceTraveled = 0;

        protected bool isPlayerShooting;
        protected bool applyMagnet;

        private float bulletStartX;
        private float bulletFinalX;

        public GunBehavior Gun { get; private set; }

        public void Init(GunBehavior gun, float damage, float speed, float selfDestroyDistance, bool isPlayerShooting, bool applyMagnet)
        {
            Gun = gun;
            this.damage = damage;
            this.speed = speed;

            this.isPlayerShooting = isPlayerShooting;
            this.applyMagnet = applyMagnet;

            this.selfDestroyDistance = selfDestroyDistance;

            if (selfDestroyDistance == 0)
                selfDestroyDistance = -1f;

            if (applyMagnet)
            {
                bulletStartX = gameObject.transform.position.x;
                bulletFinalX = PlayerBehavior.Instance.CurrentSideShift + (gameObject.transform.position.x - PlayerBehavior.Instance.CurrentSideShift) * 0.1f;
            }

            distanceTraveled = 0;

            trailRenderer.gameObject.SetActive(true);
        }

        protected virtual void Update()
        {
            // moving forward
            transform.position += transform.forward * speed * Time.deltaTime;

            if (selfDestroyDistance != -1)
            {
                distanceTraveled += speed * Time.fixedDeltaTime;

                if (applyMagnet)
                {
                    float currentPosX = Mathf.Lerp(bulletStartX, bulletFinalX, distanceTraveled / selfDestroyDistance);

                    // moving sideways - bullet magnet effect
                    transform.position = transform.position.SetX(currentPosX);
                }

                if (distanceTraveled >= selfDestroyDistance)
                {
                    SelfDestroy();
                }
            }
        }

        protected virtual void OnTriggerEnter(Collider other)
        {
            ITargetable target = other.GetComponent<ITargetable>();

            if (target != null)
            {
                if (isPlayerShooting)
                {
                    target.GetHit(damage, transform.position, Gun.GunData.DamageMultiplier);
                }

                SelfDestroy();
            }
            else
            {
                if (!isPlayerShooting)
                {
                    CharacterBehavior character = other.GetComponent<CharacterBehavior>();

                    if (character != null)
                    {
                        character.TakeDamage(damage, transform.position);
                        SelfDestroy();
                    }
                }
            }

            if (gameObject.activeSelf)
            {
                BulletBehavior otherBullet = other.GetComponent<BulletBehavior>();

                if (otherBullet != null && otherBullet.isPlayerShooting != isPlayerShooting)
                {
                    ParticleCase particleCase = ParticlesController.PlayParticle(HIT_PARTICLE_NAME);

                    particleCase.SetPosition(transform.position);

                    SelfDestroy();
                }

                if (other.gameObject.layer == PhysicsHelper.LAYER_OBSTACLE)
                {
                    SelfDestroy();
                }
            }
        }

        public void SelfDestroy()
        {
            // Disable bullet
            trailRenderer.Clear();
            gameObject.SetActive(false);
            trailRenderer.gameObject.SetActive(false);
        }
    }
}