using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Watermelon
{
    public class GunCharacterBehavior : CharacterBehavior
    {
        private static readonly int IMPACT_TRIGGER = Animator.StringToHash("Impact");

        [SerializeField] Animator animator;

        public override void PlayShootingAnimation()
        {
            animator.SetTrigger(IMPACT_TRIGGER);
        }

        public override void PlayGetHitAnimation()
        {
            animator.SetTrigger(IMPACT_TRIGGER);
        }
    }
}
