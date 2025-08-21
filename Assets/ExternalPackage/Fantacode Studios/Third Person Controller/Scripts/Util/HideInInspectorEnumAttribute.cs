using System.Collections;
using System.Collections.Generic;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
namespace FS_ThirdPerson
{
    public class HideInInspectorEnumAttribute : PropertyAttribute
    {
        public int[] HiddenValues { get; private set; }

        public HideInInspectorEnumAttribute(params int[] hiddenValues)
        {
            HiddenValues = hiddenValues;
        }
    }
#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(HideInInspectorEnumAttribute))]
    public class HideInInspectorEnumDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            HideInInspectorEnumAttribute hideAttribute = (HideInInspectorEnumAttribute)attribute;

            if (property.propertyType == SerializedPropertyType.Enum)
            {
                EditorGUI.BeginProperty(position, label, property);

                // Get the enum names and values
                string[] enumNames = property.enumDisplayNames;
                int enumValueIndex = property.enumValueIndex;

                // Filter out hidden values
                var visibleNames = new List<string>();
                for (int i = 0; i < enumNames.Length; i++)
                {
                    if (hideAttribute.HiddenValues.Contains(i)) continue;
                    visibleNames.Add(enumNames[i]);
                }

                // Display the popup
                int selectedIndex = System.Array.IndexOf(visibleNames.ToArray(), enumNames[enumValueIndex]);
                if (selectedIndex < 0) selectedIndex = 0;

                selectedIndex = EditorGUI.Popup(position, label.text, selectedIndex, visibleNames.ToArray());
                property.enumValueIndex = System.Array.IndexOf(enumNames, visibleNames[selectedIndex]);

                EditorGUI.EndProperty();
            }
            else
            {
                EditorGUI.LabelField(position, label.text, "Use HideInInspectorEnum with Enum.");
            }
        }
    }
#endif
}