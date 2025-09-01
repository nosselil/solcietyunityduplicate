using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Watermelon
{
    [CreateAssetMenu(fileName = "Movement Mode Data", menuName = "Data/Movement/Mode")]
    public class MovementModeData : ScriptableObject
    {
        [SerializeField] MovementModeType movementModeType;
        public MovementModeType MovementModeTyple => movementModeType;

        [SerializeField] FloatToggle forwardSpeed;
        public bool ForwardMovementEnabled => forwardSpeed.Enabled;
        public float ForwardSpeed => forwardSpeed.Value;

        [SerializeField] FloatToggle obstaclesMovementSpeed;
        public bool ObstaclesMovementEnabled => obstaclesMovementSpeed.Enabled;
        public float ObstaclesMovementSpeed => obstaclesMovementSpeed.Value;
    }
}