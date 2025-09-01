using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Watermelon
{
    public class StatModifier
    {
        public StatModifier(OperationType operationType, float modifier) 
        {
            this.operationType = operationType;
            this.modifier = modifier;
        }

        private OperationType operationType;
        public OperationType OperationType => operationType;

        private float modifier;
        public float Modifier => modifier;

        public float ApplyValue(float initialValue)
        {
            switch (operationType)
            {
                case OperationType.Plus: return initialValue + modifier;
                case OperationType.Minus: return initialValue - modifier;
                case OperationType.Multiply: return initialValue * modifier;
                case OperationType.Divide: return initialValue / modifier;
            }

            return initialValue;
        }

        public static StatModifier Multiply(float modifier)
        {
            return new StatModifier(OperationType.Multiply, modifier);
        }

        public static StatModifier Plus(float modifier)
        {
            return new StatModifier(OperationType.Plus, modifier);
        }
    }
}
