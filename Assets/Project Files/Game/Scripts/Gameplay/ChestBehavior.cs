using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Watermelon
{
    public class ChestBehavior : OppositeMover
    {
        private static readonly int SHAKE_TRIGGER = Animator.StringToHash("Shake");
        private static readonly int OPEN_TRIGGER = Animator.StringToHash("Open");

        [Header("Reward")]
        [SerializeField] CurrencyType rewardCurrency = CurrencyType.Money;
        [SerializeField] int rewardAmount = 100;

        [Header("References")]
        [SerializeField] Animator animator;

        [SerializeField] ParticleSystem moneyParticle;

        public void Init()
        {
            RegisterMovement();
        }

        public void ShakeChest()
        {
            animator.SetTrigger(SHAKE_TRIGGER);
        }

        public void OpenChest()
        {
            animator.SetTrigger(OPEN_TRIGGER);

            if(moneyParticle != null) moneyParticle.Play();

            CurrencyController.Add(rewardCurrency, rewardAmount);

            AudioController.PlaySound(AudioController.AudioClips.reward);

            RemoveMover();
        }
    }
}
