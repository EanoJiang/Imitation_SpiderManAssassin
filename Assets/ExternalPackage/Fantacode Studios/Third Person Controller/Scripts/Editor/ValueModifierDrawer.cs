using UnityEditor;
using UnityEngine;
namespace FS_ThirdPerson
{

    [CustomPropertyDrawer(typeof(ValueModifier))]
    public class ValueModifierDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            float lineHeight = EditorGUIUtility.singleLineHeight;
            float verticalSpacing = EditorGUIUtility.standardVerticalSpacing;

            var currentModifierIndexProp = property.FindPropertyRelative("currentModifierType");

            Rect fieldRect = new Rect(position.x, position.y, position.width - lineHeight - verticalSpacing, lineHeight);
            var constant = property.FindPropertyRelative("constant");
            var curve = property.FindPropertyRelative("curve");
            if (currentModifierIndexProp.enumValueIndex == 0)
                EditorGUI.PropertyField(fieldRect, property.FindPropertyRelative("constant"), GUIContent.none);
            else if (currentModifierIndexProp.enumValueIndex == 1)
                curve.animationCurveValue = EditorGUI.CurveField(fieldRect, curve.animationCurveValue, Color.green, property.FindPropertyRelative("defaultCurveRect").rectValue);

            Rect buttonRect = new Rect(fieldRect.x + fieldRect.width + verticalSpacing, position.y, lineHeight, lineHeight);
            if (EditorGUI.DropdownButton(buttonRect, new GUIContent("Select Option"), FocusType.Passive))
            {
                GenericMenu menu = new GenericMenu();

                for (int i = 0; i < currentModifierIndexProp.enumDisplayNames.Length; i++)
                {
                    int index = i; 
                    menu.AddItem(new GUIContent(currentModifierIndexProp.enumDisplayNames[i]),
                                 currentModifierIndexProp.enumValueIndex == i,
                                 () =>
                                 {
                                     currentModifierIndexProp.enumValueIndex = index; // Update the value
                         currentModifierIndexProp.serializedObject.ApplyModifiedProperties(); // Apply changes
                     });
                }

                menu.DropDown(buttonRect); 
            }

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight;
        }
    }
}