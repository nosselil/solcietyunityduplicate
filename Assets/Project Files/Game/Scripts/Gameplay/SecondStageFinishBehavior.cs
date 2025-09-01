using System.Collections.Generic;
using UnityEngine;

namespace Watermelon
{
    public class SecondStageFinishBehavior : OppositeMover, IClearable
    {
        [SerializeField] List<ParticleSystem> particles;
        [SerializeField] Collider finishCollider;

        public event SimpleCallback onFinishReached;

        public void Init()
        {
            RegisterMovement();

            finishCollider.enabled = true;
        }

        public void Clear()
        {
            if(this != null)
            {
                if (!particles.IsNullOrEmpty())
                {
                    for (int i = 0; i < particles.Count; i++)
                    {
                        if (particles[i] != null)
                        {
                            particles[i].Stop();
                            particles[i].Clear();
                        }
                    }
                }

                if (gameObject != null)
                    gameObject.SetActive(false);
            }

            RemoveMover();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.GetComponent<CharacterBehavior>() != null)
            {
                for (int i = 0; i < particles.Count; i++)
                {
                    if (particles[i] != null) particles[i].Play();
                }

                BonusStageSave bonusStageSave = SaveController.GetSaveObject<BonusStageSave>("Bonus Stage Save");

                bonusStageSave.BonusStageId++;

                onFinishReached?.Invoke();

                finishCollider.enabled = false;
            }
        }
    }
}
