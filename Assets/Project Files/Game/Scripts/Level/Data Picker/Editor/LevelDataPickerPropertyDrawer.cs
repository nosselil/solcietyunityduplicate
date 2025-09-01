using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace Watermelon
{
    [CustomPropertyDrawer(typeof(LevelDataPickerAttribute))]
    public class LevelDataPickerPropertyDrawer : PropertyDrawer
    {
        private static Dictionary<SerializedProperty, PickerData> pickerDataLink = new Dictionary<SerializedProperty, PickerData>();

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (property.propertyType != SerializedPropertyType.String)
            {
                EditorGUI.HelpBox(position, "Unsupported property type!", MessageType.Error);

                return;
            }

            if(!EditorLevelDataPicker.IsDatabaseExists)
            {
                EditorGUI.HelpBox(position, "Levels Database can't be found!", MessageType.Error);

                return;
            }

            LevelDataPickerAttribute pickerAttribute = (LevelDataPickerAttribute)attribute;

            PickerData data = GetPickerData(property, pickerAttribute.DataType, property.stringValue);

            EditorGUI.BeginProperty(position, label, property);

            EditorGUI.BeginChangeCheck();
            data.Index = EditorGUI.Popup(position, label.text, data.Index, data.Items);
            if(EditorGUI.EndChangeCheck())
            {
                if(data.Items.IsInRange(data.Index))
                {
                    property.stringValue = data.Items[data.Index];
                }
            }

            EditorGUI.EndProperty();
        }

        private PickerData GetPickerData(SerializedProperty property, LevelDataType levelDataType, string currentValue)
        {
            PickerData pickerData;
            if (!pickerDataLink.TryGetValue(property, out pickerData))
            {
                pickerData = new PickerData(levelDataType, currentValue);

                pickerDataLink.Add(property, pickerData);
            }

            return pickerData;
        }

        private class PickerData
        {
            public string[] Items { get; private set; }
            public int Index;

            public PickerData(LevelDataType type, string currentValue)
            {
                Items = EditorLevelDataPicker.GetItems(type);

                if (Items != null)
                {
                    Index = System.Array.FindIndex(Items, x => x == currentValue);
                }
                else
                {
                    Index = -1;
                }
            }
        }
    }
}
