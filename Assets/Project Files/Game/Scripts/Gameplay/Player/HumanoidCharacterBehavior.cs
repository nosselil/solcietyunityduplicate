using UnityEngine;

namespace Watermelon
{
    public class HumanoidCharacterBehavior : CharacterBehavior
    {
        [SerializeField] CharacterGraphicsBehavior characterGraphicsBehavior;

        private TweenCase disablingCase;

        public override void Init(PlayerBehavior player, Transform formationPosition, float maxHealth)
        {
            base.Init(player, formationPosition, maxHealth);

            disablingCase.KillActive();
            characterGraphicsBehavior.Init();
        }

        protected override void LoadGun()
        {
            base.LoadGun();

            characterGraphicsBehavior.SetShootingPose(GunBehavior.GunData.PoseAnimation);
        }

        public override void EnableMovingAnimation()
        {
            characterGraphicsBehavior.EnableMovingAnimation();
        }

        public override void DisableMovingAnimation()
        {
            characterGraphicsBehavior.DisableMovingAnimation();
        }

        public override void PlayShootingAnimation()
        {
            if (Data.ShootingImpactType == ShootingImpactType.Shift || Data.ShootingImpactType == ShootingImpactType.Both)
            {
                characterGraphicsBehavior.DoShootingImpact(GunBehavior.GunData.ShootingImpactDistance, GunBehavior.GunData.ShootingImpactDuration, GunBehavior.GunData.ShootingImpactCurve);
            }

            if (Data.ShootingImpactType == ShootingImpactType.Animation || Data.ShootingImpactType == ShootingImpactType.Both)
            {
                characterGraphicsBehavior.PlayShootingImpactAnimation();
            }
        }

        public override void PlayGetHitAnimation()
        {
            PlayShootingAnimation();

            characterGraphicsBehavior.DoRedOverlayColor();
        }

        public override void Die()
        {
            if (IsDead)
                return;

            IsDead = true;

            Player.OnCharacterDied(this);

            characterGraphicsBehavior.PlayDyingAnimation();

            StopShooting();

            disablingCase = Tween.DelayedCall(3f, Clear);
        }

        public override void Clear()
        {
            disablingCase.KillActive();

            base.Clear();
        }
    }
}