using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Watermelon
{
    public abstract class MovementMode
    {
        public MovementModeData ModeData { get; private set; }
        public Vector3 Position { get; protected set; }

        public abstract void ProcessPointerInput(Vector2 delta);
        public abstract void Update();

        public virtual void UpdateOppositeMover(OppositeMover oppositeMover)
        {
            oppositeMover.SetMoveSpeed(ModeData.ObstaclesMovementEnabled, ModeData.ObstaclesMovementSpeed);
        }

        public MovementMode(MovementModeData modeData)
        {
            ModeData = modeData;
        }

        public void SetPosition(Vector3 position)
        {
            Position = position;
        }

        public virtual void Reset()
        {
            Position = Vector3.zero;
        }
    }
}