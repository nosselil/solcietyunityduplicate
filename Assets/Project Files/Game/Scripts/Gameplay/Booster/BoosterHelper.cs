using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Watermelon
{
    public static class BoosterHelper
    {
        public static OperationType RevertOperation(OperationType operation)
        {
            switch (operation)
            {
                case OperationType.Minus: return OperationType.Plus;
                case OperationType.Plus: return OperationType.Minus;
                case OperationType.Divide: return OperationType.Multiply;
                case OperationType.Multiply: return OperationType.Divide;
            }

            return operation;
        }

        public static float ApplyBoosterToValue(OperationType operation, float value, float boosterValue)
        {
            switch (operation)
            {
                case OperationType.Plus: return value + boosterValue;
                case OperationType.Minus: return value - boosterValue;
                case OperationType.Multiply: return value * boosterValue;
                case OperationType.Divide: return value / boosterValue;
            }

            return value;
        }

        public static string GetSymbol(OperationType operation)
        {
            switch (operation)
            {
                case OperationType.Minus: return "-";
                case OperationType.Plus: return "+";
                case OperationType.Multiply: return "X";
                case OperationType.Divide: return "/";
                default: return "";
            }
        }
    }
}
