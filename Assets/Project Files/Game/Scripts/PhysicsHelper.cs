using UnityEngine;

namespace Watermelon
{
    public static class PhysicsHelper
    {
        public static readonly int LAYER_OBSTACLE = LayerMask.NameToLayer("Obstacle");

        public const string TAG_PLAYER = "Player";
    }
}