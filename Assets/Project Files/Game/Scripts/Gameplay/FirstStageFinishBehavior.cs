using System.Collections.Generic;
using UnityEngine;

namespace Watermelon
{
    public class FirstStageFinishBehavior : OppositeMover, ILevelInitable, IClearable
    {
        [SerializeField] List<ParticleSystem> particles;

        public event SimpleCallback onFinishReached;

        public void Init(AbstractLevelData data)
        {
            RegisterMovement();

            transform.position = data.Position;

            onFinishReached = null;
        }

        public void Clear()
        {
            RemoveMover();

            if(this != null)
            {
                if (gameObject != null)
                    gameObject.SetActive(false);

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
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if(other.GetComponent<CharacterBehavior>() != null)
            {
                for(int i = 0; i < particles.Count; i++)
                {
                    if (particles[i] != null) particles[i].Play();
                }

                onFinishReached?.Invoke();
            }
        }
    }
}
