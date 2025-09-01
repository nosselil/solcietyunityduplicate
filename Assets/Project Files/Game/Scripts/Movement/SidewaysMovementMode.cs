using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Watermelon
{
    public class SidewaysMovementMode : MovementMode
    {
        private float leftBoundary;
        private float rightBoundary;

        public SidewaysMovementMode(MovementModeData data) : base(data)
        {

        }

        public void SetRoadWidth(float width)
        {
            leftBoundary = -width / 2;
            rightBoundary = width / 2;
        }

        public override void ProcessPointerInput(Vector2 delta)
        {
            Position += new Vector3(delta.x, 0, 0);

            if (Position.x < leftBoundary)
            {
                Position = Position.SetX(leftBoundary);
            }
            else if (Position.x > rightBoundary)
            {
                Position = Position.SetX(rightBoundary);
            }
        }

        public override void Update()
        {

        }
    }
}