using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Watermelon 
{ 
    public class OppositeMover : MonoBehaviour
    {
        [SerializeField] FloatToggle speedMultiplier;

        private bool isMoving;
        private float moveSpeed;

        public virtual void SetMoveSpeed(bool isMoving, float moveSpeed)
        {
            this.isMoving = isMoving;
            this.moveSpeed = moveSpeed;
        }
        
        protected virtual void RegisterMovement()
        {
            MovementManager.RegisterOppositeMover(this);
        }


        protected virtual void RemoveMover()
        {
            MovementManager.RemoveOppositeMover(this);
        }

        protected virtual void FixedUpdate()
        {
            if (GameController.IsGameplayActive && isMoving)
            {
                float speed = moveSpeed;
                if (speedMultiplier.Enabled) speed *= speedMultiplier.Value;

                transform.position += Vector3.back * speed * Time.fixedDeltaTime;
            }
        }
    }
}
