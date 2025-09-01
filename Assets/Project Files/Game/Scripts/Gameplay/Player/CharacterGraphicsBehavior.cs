using UnityEngine;

namespace Watermelon
{
    public class CharacterGraphicsBehavior : MonoBehaviour
    {
        private static readonly int SHOOTING_BOOL_HASH = Animator.StringToHash("Shooting");
        private static readonly int MOVEMENT_SPEED_FLOAT_HASH = Animator.StringToHash("Movement Speed");
        private static readonly int SHOOTING_ANIMATION_SPEED_FLOAT_HASH = Animator.StringToHash("Shooting Animation Speed");

        [SerializeField] Animator animator;
        [SerializeField] Renderer characterRenderer;
        [SerializeField] AnimationClip defaultPose;

        private AnimatorOverrideController animatorOverride;

        private TweenCase overlayCase;
        private TweenCase shootingImpactCase;

        private Color rampMinCache = Color.white;
        private Color rampMaxCache = Color.white;

        private void Awake()
        {
            animatorOverride = new AnimatorOverrideController(animator.runtimeAnimatorController);
            animator.runtimeAnimatorController = animatorOverride;

            if(characterRenderer.material !=  null )
            {
                rampMinCache = characterRenderer.material.GetColor("_RampMin");
                rampMaxCache = characterRenderer.material.GetColor("_RampMax");
            }
        }

        private void OnDestroy()
        {
            overlayCase.KillActive();
            shootingImpactCase.KillActive();
        }

        public void Init()
        {
            for (int i = 1; i < animator.layerCount; i++)
            {
                animator.SetLayerWeight(i, 1);
            }

            characterRenderer.material.SetColor(Shader.PropertyToID("_EmissionColor"), Color.clear);
            characterRenderer.material.SetColor(Shader.PropertyToID("_RampMin"), rampMinCache);
            characterRenderer.material.SetColor(Shader.PropertyToID("_RampMax"), rampMaxCache);
            characterRenderer.material.SetFloat("_RimOn", 1);
            characterRenderer.material.EnableKeyword("RIM_ON");
        }

        public void SetShootingAnimationSpeed(float value)
        {
            animator.SetFloat(SHOOTING_ANIMATION_SPEED_FLOAT_HASH, value);
        }

        public float GetShootingAnimationSpeed()
        {
            return animator.GetFloat(SHOOTING_ANIMATION_SPEED_FLOAT_HASH);
        }

        public void StartShootingAnimation()
        {
            animator.SetBool(SHOOTING_BOOL_HASH, true);
        }

        public void StopShootingAnimation()
        {
            animator.SetBool(SHOOTING_BOOL_HASH, false);
        }

        public void EnableMovingAnimation()
        {
            animator.SetFloat(MOVEMENT_SPEED_FLOAT_HASH, 1);
        }

        public void DisableMovingAnimation()
        {
            animator.SetFloat(MOVEMENT_SPEED_FLOAT_HASH, 0);
        }

        public void DoShootingImpact(float distance, float duration, AnimationCurve easing)
        {
            if (shootingImpactCase != null && shootingImpactCase.IsActive) return;

            Vector3 cachePosition = transform.localPosition;

            shootingImpactCase = transform.DOLocalMoveZ(transform.localPosition.z - distance, duration).SetCurveEasing(easing).OnComplete(() => {
                transform.localPosition = cachePosition;
            });
        }

        public void PlayShootingImpactAnimation()
        {
            animator.SetTrigger("Impact");
        }

        public void DoRedOverlayColor()
        {
            if (!overlayCase.ExistsAndActive())
            {
                overlayCase = characterRenderer.material.DOColor(Shader.PropertyToID("_EmissionColor"), Color.red, 0.15f).SetCustomEasing(Ease.GetCustomEasingFunction("Reversable Bounce")).OnComplete(() => characterRenderer.material.SetColor(Shader.PropertyToID("_EmissionColor"), Color.clear));
            }
        }

        public void DoDeathOverlayColor()
        {
            characterRenderer.material.SetColor(Shader.PropertyToID("_RampMax"), Color.gray);
            characterRenderer.material.SetColor(Shader.PropertyToID("_RampMin"), Color.gray);
            characterRenderer.material.SetFloat("_RimOn", 0);
            characterRenderer.material.DisableKeyword("RIM_ON");
        }

        public void SetShootingPose(AnimationClip pose)
        {
            if (pose == null)
            {
                animatorOverride["minigun_pose"] = defaultPose;
            }
            else
            {
                animatorOverride["minigun_pose"] = pose;
            }
        }

        public void PlayDyingAnimation()
        {
            animator.SetTrigger("Dying");

            for(int i = 1; i < animator.layerCount; i++)
            {
                animator.SetLayerWeight(i, 0);
            }
        }
    }
}