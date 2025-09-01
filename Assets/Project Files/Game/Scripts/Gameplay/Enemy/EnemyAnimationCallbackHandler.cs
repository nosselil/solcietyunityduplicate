using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Watermelon
{
    public class EnemyAnimationCallbackHandler : MonoBehaviour
    {
        private EnemyBehavior enemy;

        public void Init(EnemyBehavior enemy)
        {
            this.enemy = enemy;
        }

        public void OnGunShot()
        {
            enemy.Shoot();
        }
    }
}
