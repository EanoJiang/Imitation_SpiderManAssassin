using System;
using UnityEditor;
using UnityEngine;
namespace FS_ThirdPerson
{

    [CustomPropertyDrawer(typeof(AnimGraphClipInfo))]
    public class AnimGraphClipInfoDrawer : PropertyDrawer
    {
        Texture2D globeIcon_on;
        Texture2D globeIcon_off;
        float previewButtonWidth = 70;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            bool updateClipRef = false;
            // Basic properties
            float y = position.y;
            float lineHeight = EditorGUIUtility.singleLineHeight;
            float verticalSpacing = EditorGUIUtility.standardVerticalSpacing;

            var clipProp = property.FindPropertyRelative("clip");


            Rect clipRect = new Rect(position.x, y, position.width - lineHeight - previewButtonWidth, lineHeight);
            EditorGUI.BeginChangeCheck();
            EditorGUI.PropertyField(clipRect, clipProp, new GUIContent("Animation Clip"));
            if (EditorGUI.EndChangeCheck())
                updateClipRef = true;

            Rect previewButtonRect = new Rect(clipRect.xMax, y, previewButtonWidth, lineHeight);
            if (GUI.Button(previewButtonRect, "Preview"))
            {
                updateClipRef = true;
                AnimationPreviewHelper.ClosePreview();
                EditorUtility.OpenPropertyEditor(clipProp.objectReferenceValue);
                //guiId.stringValue = Guid.NewGuid().ToString();
                var a = (AnimationClip)clipProp.objectReferenceValue;
                AnimationPreviewHelper.PlayPreview(a);
            }
            

            Rect showDetailsRect = new Rect(previewButtonRect.xMax, y, lineHeight, lineHeight);
            var showDetailsProp = property.FindPropertyRelative("showDetails");
            if (GUI.Button(showDetailsRect, showDetailsProp.boolValue ? "<" : ">"))
            {
                showDetailsProp.boolValue = !showDetailsProp.boolValue;
            }

            if (showDetailsProp.boolValue)
            {
                
                EditorGUI.indentLevel++;

                y += lineHeight + verticalSpacing;
                Rect customAnimationSpeedRect = new Rect(position.x, y, position.width * .5f, lineHeight);
                var customAnimationSpeedProp = property.FindPropertyRelative("customAnimationSpeed");
                EditorGUI.PropertyField(customAnimationSpeedRect, customAnimationSpeedProp);
                

                if (customAnimationSpeedProp.boolValue)
                {
                    Rect globalBGRect = new Rect(customAnimationSpeedRect.x + customAnimationSpeedRect.width - verticalSpacing * 3f, y, lineHeight, lineHeight);
                    //Rect globalIconRect = new Rect(globalBGRect.x+.5f, y + 1, lineHeight - 1, lineHeight);

                    var useAsGlobalTimeScale = property.FindPropertyRelative("useAsGlobalTimeScale");
                    if (globeIcon_on == null || globeIcon_off == null)
                    {
                        globeIcon_on = Resources.Load<Texture2D>("Icons/globe_icon_on");
                        globeIcon_off = Resources.Load<Texture2D>("Icons/globe_icon_off");
                    }
                    if (GUI.Button(globalBGRect,""))
                    {
                        useAsGlobalTimeScale.boolValue = !useAsGlobalTimeScale.boolValue;
                    }
                    GUI.Label(globalBGRect, new GUIContent(useAsGlobalTimeScale.boolValue ? globeIcon_on : globeIcon_off, "If enabled, this determines whether to use the speed as the global time scale."));

                    Rect speedModifierRect = new Rect(position.x + position.width * .5f, y, position.width * .5f, lineHeight);
                    EditorGUI.PropertyField(speedModifierRect, property.FindPropertyRelative("speedModifier"));
                }

                y += lineHeight + verticalSpacing;
                Rect useGravityRect = new Rect(position.x, y, position.width * .5f, lineHeight);
                var useGravityProp = property.FindPropertyRelative("useGravity");
                EditorGUI.PropertyField(useGravityRect, useGravityProp, new GUIContent("Use Gravity Multiplier"));

                if (useGravityProp.boolValue)
                {
                    Rect gravityModifierRect = new Rect(position.x + position.width * .5f, y, position.width * .5f, lineHeight);
                    EditorGUI.PropertyField(gravityModifierRect, property.FindPropertyRelative("gravityModifier"));
                }

                y += lineHeight + verticalSpacing;
                Rect TranistionInAndOutRect = new Rect(position.x, y, position.width, lineHeight);
                EditorGUI.PropertyField(TranistionInAndOutRect, property.FindPropertyRelative("TranistionInAndOut"));
                y += lineHeight + verticalSpacing;

                SerializedProperty eventsProp = property.FindPropertyRelative("events");
                var eventSize = eventsProp.arraySize;
                EditorGUI.PropertyField(new Rect(position.x, y, position.width, lineHeight * (eventsProp.arraySize + 1)), eventsProp, true);
                if (eventSize != eventsProp.arraySize || updateClipRef)
                {
                    for (int i = 0; i < eventsProp.arraySize; i++)
                    {
                        eventsProp.GetArrayElementAtIndex(i).FindPropertyRelative("clip").objectReferenceValue = (AnimationClip)clipProp.objectReferenceValue;
                    }
                }
                y += EditorGUI.GetPropertyHeight(eventsProp) + verticalSpacing;
                SerializedProperty onEndAnimationProp = property.FindPropertyRelative("onEndAnimation");
                var onEndAnimationSize = onEndAnimationProp.arraySize;
                EditorGUI.PropertyField(new Rect(position.x, y, position.width, lineHeight * (onEndAnimationProp.arraySize + 1)), onEndAnimationProp, new GUIContent("On End Animation Event"), true);
                if (onEndAnimationSize != onEndAnimationProp.arraySize || updateClipRef)
                {
                    for (int i = 0; i < onEndAnimationProp.arraySize; i++)
                    {
                        onEndAnimationProp.GetArrayElementAtIndex(i).FindPropertyRelative("clip").objectReferenceValue = (AnimationClip)clipProp.objectReferenceValue;
                    }
                }


                EditorGUI.indentLevel--;
            }

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var showDetailsProp = property.FindPropertyRelative("showDetails");
            float height = EditorGUIUtility.singleLineHeight;
            if (showDetailsProp.boolValue)
            {
                height += EditorGUIUtility.singleLineHeight * 3;
                height += EditorGUIUtility.standardVerticalSpacing * 4;


                SerializedProperty eventsProp = property.FindPropertyRelative("events");
                SerializedProperty onEndAnimationProp = property.FindPropertyRelative("onEndAnimation");
                height += EditorGUI.GetPropertyHeight(eventsProp, true);
                height += EditorGUI.GetPropertyHeight(onEndAnimationProp, true);
            }

            return height;
        }
    }
}