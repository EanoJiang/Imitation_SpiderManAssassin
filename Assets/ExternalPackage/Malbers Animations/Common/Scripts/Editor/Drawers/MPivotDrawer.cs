using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


namespace MalbersAnimations.Controller
{
    [CustomPropertyDrawer(typeof(MPivots))]
    public class MPivotDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            var indent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;
            var height = EditorGUIUtility.singleLineHeight;

            var name = new Rect(position.x, position.y, position.width / 2 - 90, height);
            var vector = new Rect(position.width / 2 - 45, position.y, position.width / 2 , height);
            var multiplier = new Rect(position.width - 45, position.y, 40, height);

            var button = new Rect(position.width - 2, position.y, 16, height);


            EditorGUI.PropertyField(name, property.FindPropertyRelative("name"), GUIContent.none);
            EditorGUI.PropertyField(vector, property.FindPropertyRelative("position"), GUIContent.none);
            EditorGUIUtility.labelWidth = 13;
            EditorGUI.PropertyField(multiplier, property.FindPropertyRelative("multiplier"), new GUIContent(" ","Multiplier"));

            var buttonProperty = property.FindPropertyRelative("EditorModify");

            buttonProperty.boolValue =  GUI.Toggle(button, buttonProperty.boolValue, new GUIContent("•","Edit on the Scene"), EditorStyles.miniButton);

            EditorGUIUtility.labelWidth = 0;


            EditorGUI.indentLevel = indent;
            EditorGUI.EndProperty();

        }
    }
}