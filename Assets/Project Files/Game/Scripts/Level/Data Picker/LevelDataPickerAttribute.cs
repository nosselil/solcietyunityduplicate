using System;
using UnityEngine;

namespace Watermelon
{
    [AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
    public class LevelDataPickerAttribute : PropertyAttribute
    {
        public LevelDataType DataType { get; private set; }

        public LevelDataPickerAttribute(LevelDataType dataType)
        {
            DataType = dataType;
        }
    }
}
