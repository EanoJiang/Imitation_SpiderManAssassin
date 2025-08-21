using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace FS_ThirdPerson
{
    [CustomPropertyDrawer(typeof(AnimationEventData))]
    public class AnimationEventDataPropertyDrawer : PropertyDrawer
    {
        private const float LINE_HEIGHT = 18f;
        private const float HEADER_HEIGHT = 20f;
        private const float VERTICAL_SPACING = 2f;
        private const float PARAM_SPACING = 1f;
        private const float SECTION_SPACING = 10f;
        private const float SPACING = 5f;
        private const float TYPE_WIDTH = 120f;

        public static T GetEnumValueByIndex<T>(int index) where T : Enum
        {
            var values = Enum.GetValues(typeof(T));
            if (index < 0 || index >= values.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(index), "Index is out of range for the enum.");
            }
            return (T)values.GetValue(index);
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);
            var headerRect = new Rect(position.x, position.y, position.width, HEADER_HEIGHT);
            property.isExpanded = EditorGUI.Foldout(headerRect, property.isExpanded, label, true);

            if (!property.isExpanded)
            {
                EditorGUI.EndProperty();
                return;
            }

            DrawPropertyContent(position, property);
            EditorGUI.EndProperty();
        }

        private void DrawPropertyContent(Rect position, SerializedProperty property)
        {
            EditorGUI.indentLevel++;
            float currentY = position.y + HEADER_HEIGHT + VERTICAL_SPACING;

            var contentRect = EditorGUI.IndentedRect(new Rect(position.x, currentY, position.width, position.height));

            // Draw main properties
            currentY = DrawMainProperties(contentRect, currentY, property);

            // Draw parameters section
            DrawParametersSection(contentRect, currentY, property);

            EditorGUI.indentLevel--;
        }

        private float DrawMainProperties(Rect contentRect, float currentY, SerializedProperty property)
        {
            // Time property
            var timeRect = new Rect(contentRect.x, currentY, contentRect.width, LINE_HEIGHT);
            var normalizedTime = property.FindPropertyRelative("normalizedTime");
            var clipProp = property.FindPropertyRelative("clip");
            EditorGUI.BeginChangeCheck();
            EditorGUI.PropertyField(timeRect, property.FindPropertyRelative("normalizedTime"),
                new GUIContent("Time", "Normalized time value (0-1)"));
            if (EditorGUI.EndChangeCheck())
                AnimationPreviewHelper.SetAnimationTime(normalizedTime.floatValue, (AnimationClip)clipProp.objectReferenceValue);



            currentY += LINE_HEIGHT + VERTICAL_SPACING;

            // Target object property
            var targetRect = new Rect(contentRect.x, currentY, contentRect.width, LINE_HEIGHT);
            var targetObjectProp = property.FindPropertyRelative("targetObject");

            var methodNameProp = property.FindPropertyRelative("methodName");
            var parametersProp = property.FindPropertyRelative("parameters");
            var methodsProp = property.FindPropertyRelative("methodInfos");
            var isGameObject = property.FindPropertyRelative("isGameObject");
            var targetName = property.FindPropertyRelative("targetName");

            EditorGUI.BeginChangeCheck();
            EditorGUI.PropertyField(targetRect, targetObjectProp,
                new GUIContent("Target", "Target object containing the method"));
            MethodInfo[] methodInfos = new MethodInfo[0];
            if (EditorGUI.EndChangeCheck())
            {
                if (targetObjectProp.objectReferenceValue != null)
                {
                    isGameObject.boolValue = targetObjectProp.objectReferenceValue is Component;
                    targetName.stringValue = isGameObject.boolValue ? targetObjectProp.objectReferenceValue.GetType().Name : targetObjectProp.objectReferenceValue.name;
                    AnimGraphClipInfo.FindTypeByName(targetName.stringValue);
                }
                RefreshMethods(property, 0, ref methodInfos);
            }
            currentY += LINE_HEIGHT + VERTICAL_SPACING;

            // Method selection
            if (targetObjectProp.objectReferenceValue != null)
            {
                currentY = DrawMethodSelection(contentRect, currentY, property, ref methodInfos);
            }

            return currentY;
        }

        private float DrawMethodSelection(Rect contentRect, float currentY, SerializedProperty property, ref MethodInfo[] methodInfos)
        {
            //EditorGUI.indentLevel--;

            var methodRect = new Rect(contentRect.x, currentY, contentRect.width - 100, LINE_HEIGHT);
            var methodRefreshBtnRect = new Rect(methodRect.x + methodRect.width, currentY, 100, LINE_HEIGHT);

            var methodNameProp = property.FindPropertyRelative("methodName");
            var methodsProp = property.FindPropertyRelative("methodInfos");

            if (GUI.Button(methodRefreshBtnRect, "Refresh"))
            {
                RefreshMethods(property, 0, ref methodInfos);
            }

            string[] methods = new string[methodsProp.arraySize];
            for (int i = 0; i < methodsProp.arraySize; i++)
            {
                methods[i] = methodsProp.GetArrayElementAtIndex(i).stringValue;
            }

            //EditorGUI.indentLevel++;
            int selectedMethodIndex = Array.IndexOf(methods, methodNameProp.stringValue);
            EditorGUI.BeginChangeCheck();
            selectedMethodIndex = EditorGUI.Popup(methodRect, "Method", selectedMethodIndex, methods);
            //EditorGUI.indentLevel--;

            if (EditorGUI.EndChangeCheck() && selectedMethodIndex >= 0)
            {
                RefreshMethods(property, selectedMethodIndex, ref methodInfos);
            }

            return currentY + LINE_HEIGHT + VERTICAL_SPACING;
        }

        private void DrawParametersSection(Rect contentRect, float currentY, SerializedProperty property)
        {
            var parametersProp = property.FindPropertyRelative("parameters");
            var foldOut = property.FindPropertyRelative("foldOut");

            // Parameters header
            var headerBgRect = new Rect(contentRect.x + LINE_HEIGHT, currentY, contentRect.width - LINE_HEIGHT, HEADER_HEIGHT);
            EditorGUI.DrawRect(headerBgRect, new Color(0.1f, 0.1f, 0.1f, 0.1f));

            var headerContent = new GUIContent($"Parameters ({parametersProp.arraySize})");
            var headerLabelRect = new Rect(headerBgRect.x - LINE_HEIGHT + 4, headerBgRect.y + 2,
                                         headerBgRect.width - 60, HEADER_HEIGHT - 4);
            foldOut.boolValue = EditorGUI.Foldout(headerLabelRect, foldOut.boolValue, headerContent);

            currentY += HEADER_HEIGHT + VERTICAL_SPACING;

            if (parametersProp.arraySize > 0 && foldOut.boolValue)
            {
                DrawParameters(contentRect, currentY, parametersProp);
            }
        }

        private void DrawParameters(Rect contentRect, float currentY, SerializedProperty parametersProp)
        {
            var listRect = new Rect(contentRect.x + LINE_HEIGHT, currentY, contentRect.width - LINE_HEIGHT,
                GetParametersHeight(parametersProp));
            var listBgRect = new Rect(listRect.x - 1, listRect.y, listRect.width + 2,
                                    GetParametersHeight(parametersProp) + 1);

            EditorGUI.DrawRect(listBgRect, new Color(0.8f, 0.8f, 0.8f, 0.1f));

            for (int i = 0; i < parametersProp.arraySize; i++)
            {
                DrawParameter(contentRect, ref currentY, parametersProp.GetArrayElementAtIndex(i));
            }
        }

        private void DrawParameter(Rect contentRect, ref float currentY, SerializedProperty parameter)
        {
            var typeRect = new Rect(contentRect.x + VERTICAL_SPACING, currentY + 1, TYPE_WIDTH, LINE_HEIGHT);
            float valueWidth = contentRect.width - TYPE_WIDTH - SPACING * 1.75f + LINE_HEIGHT;
            var valueRect = new Rect(typeRect.x + typeRect.width + SPACING - LINE_HEIGHT + EditorGUIUtility.singleLineHeight, currentY + 1, valueWidth, LINE_HEIGHT);

            var typeProp = parameter.FindPropertyRelative("type");
            var currentType = (AnimationEventData.ParameterData.ParameterType)typeProp.enumValueIndex;

            AnimationEventData.ParameterData.ParameterType enumValue =
                GetEnumValueByIndex<AnimationEventData.ParameterData.ParameterType>(typeProp.enumValueIndex);
            EditorGUI.LabelField(typeRect, parameter.FindPropertyRelative("parameterName").stringValue, EditorStyles.helpBox);

            DrawParameterValue(valueRect, parameter, currentType);

            currentY += GetParameterHeight(parameter) + PARAM_SPACING;
        }

        private void DrawParameterValue(Rect valueRect, SerializedProperty parameter,
    AnimationEventData.ParameterData.ParameterType currentType)
        {
            switch (currentType)
            {
                case AnimationEventData.ParameterData.ParameterType.Integer:
                    EditorGUI.PropertyField(valueRect, parameter.FindPropertyRelative("intValue"), GUIContent.none);
                    break;
                case AnimationEventData.ParameterData.ParameterType.Float:
                    EditorGUI.PropertyField(valueRect, parameter.FindPropertyRelative("floatValue"), GUIContent.none);
                    break;
                case AnimationEventData.ParameterData.ParameterType.String:
                    EditorGUI.PropertyField(valueRect, parameter.FindPropertyRelative("stringValue"), GUIContent.none);
                    break;
                case AnimationEventData.ParameterData.ParameterType.Boolean:
                    EditorGUI.PropertyField(valueRect, parameter.FindPropertyRelative("boolValue"), GUIContent.none);
                    break;
                case AnimationEventData.ParameterData.ParameterType.Vector2:
                    EditorGUI.PropertyField(valueRect, parameter.FindPropertyRelative("vector2Value"), GUIContent.none);
                    break;
                case AnimationEventData.ParameterData.ParameterType.Vector3:
                    EditorGUI.PropertyField(valueRect, parameter.FindPropertyRelative("vector3Value"), GUIContent.none);
                    break;
                case AnimationEventData.ParameterData.ParameterType.GameObject:
                    EditorGUI.PropertyField(valueRect, parameter.FindPropertyRelative("gameObjectValue"), GUIContent.none);
                    break;
                case AnimationEventData.ParameterData.ParameterType.HumanBodyBone:
                    EditorGUI.PropertyField(valueRect, parameter.FindPropertyRelative("humanBodyBoneValue"), GUIContent.none);
                    break;
                case AnimationEventData.ParameterData.ParameterType.Component:
                    EditorGUI.PropertyField(valueRect, parameter.FindPropertyRelative("componentValue"), GUIContent.none);
                    break;
                case AnimationEventData.ParameterData.ParameterType.Color:
                    EditorGUI.PropertyField(valueRect, parameter.FindPropertyRelative("colorValue"), GUIContent.none);
                    break;
                case AnimationEventData.ParameterData.ParameterType.AnimationCurve:
                    EditorGUI.PropertyField(valueRect, parameter.FindPropertyRelative("animationCurveValue"), GUIContent.none);
                    break;
                case AnimationEventData.ParameterData.ParameterType.AudioClip:
                    EditorGUI.PropertyField(valueRect, parameter.FindPropertyRelative("audioClipValue"), GUIContent.none);
                    break;
                case AnimationEventData.ParameterData.ParameterType.Texture:
                    EditorGUI.PropertyField(valueRect, parameter.FindPropertyRelative("textureValue"), GUIContent.none);
                    break;
                case AnimationEventData.ParameterData.ParameterType.Material:
                    EditorGUI.PropertyField(valueRect, parameter.FindPropertyRelative("materialValue"), GUIContent.none);
                    break;
                case AnimationEventData.ParameterData.ParameterType.Sprite:
                    EditorGUI.PropertyField(valueRect, parameter.FindPropertyRelative("spriteValue"), GUIContent.none);
                    break;
                case AnimationEventData.ParameterData.ParameterType.LayerMask:
                    EditorGUI.PropertyField(valueRect, parameter.FindPropertyRelative("layerMaskValue"), GUIContent.none);
                    break;
                case AnimationEventData.ParameterData.ParameterType.Object:
                    EditorGUI.PropertyField(valueRect, parameter.FindPropertyRelative("_object"), GUIContent.none);
                    break;
                case AnimationEventData.ParameterData.ParameterType.CameraSettings:
                    EditorGUI.PropertyField(valueRect, parameter.FindPropertyRelative("cameraSettings"), true);
                    break;
                default:
                    EditorGUI.LabelField(valueRect, "Unsupported Type");
                    break;
            }
        }


        private void RefreshMethods(SerializedProperty property, int selectedMethodIndex, ref MethodInfo[] methodInfos)
        {
            var targetObjectProp = property.FindPropertyRelative("targetObject");
            var methodNameProp = property.FindPropertyRelative("methodName");
            var parametersProp = property.FindPropertyRelative("parameters");
            var methodsProp = property.FindPropertyRelative("methodInfos");
            var isGameObject = property.FindPropertyRelative("isGameObject");
            var targetName = property.FindPropertyRelative("targetName");

            if (targetObjectProp.objectReferenceValue == null)
            {
                parametersProp.ClearArray();
                methodsProp.ClearArray();
                methodNameProp.stringValue = "";
                return;
            }

            try
            {
                object instance = targetObjectProp.objectReferenceValue;
                var classType = AnimGraphClipInfo.FindTypeByName(targetName.stringValue);

                if (classType != null)
                {
                    if (typeof(MonoBehaviour).IsAssignableFrom(classType))
                    {
                        var tempObj = new GameObject();
                        instance = tempObj.AddComponent(classType);
                        GameObject.DestroyImmediate(tempObj);
                    }
                    else
                        instance = Activator.CreateInstance(classType);
                }
                methodInfos = instance.GetType()
                    .GetMethods()
                    .Where(m => m.ReturnType != typeof(IEnumerator) && (isGameObject.boolValue || m.DeclaringType == classType))
                    .ToArray();

                parametersProp.ClearArray();
                methodsProp.ClearArray();
                methodNameProp.stringValue = "";

                for (int i = 0; i < methodInfos.Length; i++)
                {
                    methodsProp.arraySize++;
                    methodsProp.GetArrayElementAtIndex(i).stringValue = methodInfos[i].Name;
                }

                ParameterInfo[] methodParams = new ParameterInfo[0];
                if (methodsProp.arraySize > 0)
                {
                    var method = methodsProp.GetArrayElementAtIndex(selectedMethodIndex).stringValue;
                    methodNameProp.stringValue = method;
                    var selectedMethod = methodInfos.First(m => m.Name == method);
                    methodParams = selectedMethod.GetParameters();
                }

                List<ParameterInfo> mp = new List<ParameterInfo>();
                mp = methodParams.ToList();
                if (methodParams.Length > 0)
                    mp.RemoveAt(0);

                methodParams = mp.ToArray();

                UpdateParameterList(ref parametersProp, ref methodParams);
            }
            catch (Exception)
            { }
        }

        private void UpdateParameterList(ref SerializedProperty parametersProp, ref ParameterInfo[] methodParams)
        {
            parametersProp.ClearArray();

            foreach (var param in methodParams)
            {
                parametersProp.arraySize++;
                var paramProp = parametersProp.GetArrayElementAtIndex(parametersProp.arraySize - 1);
                var typeProp = paramProp.FindPropertyRelative("type");
                var paramType = param.ParameterType;
                paramProp.FindPropertyRelative("parameterName").stringValue = param.Name;

                if (!SetParameterType(typeProp, paramType))
                {
                    parametersProp.arraySize--;
                }
            }
        }

        private bool SetParameterType(SerializedProperty typeProp, Type paramType)
        {
            if (paramType == typeof(int))
                typeProp.enumValueIndex = (int)AnimationEventData.ParameterData.ParameterType.Integer;
            else if (paramType == typeof(float))
                typeProp.enumValueIndex = (int)AnimationEventData.ParameterData.ParameterType.Float;
            else if (paramType == typeof(string))
                typeProp.enumValueIndex = (int)AnimationEventData.ParameterData.ParameterType.String;
            else if (paramType == typeof(bool))
                typeProp.enumValueIndex = (int)AnimationEventData.ParameterData.ParameterType.Boolean;
            else if (paramType == typeof(Vector2))
                typeProp.enumValueIndex = (int)AnimationEventData.ParameterData.ParameterType.Vector2;
            else if (paramType == typeof(Vector3))
                typeProp.enumValueIndex = (int)AnimationEventData.ParameterData.ParameterType.Vector3;
            else if (paramType == typeof(GameObject))
                typeProp.enumValueIndex = (int)AnimationEventData.ParameterData.ParameterType.GameObject;
            else if (paramType == typeof(HumanBodyBones))
                typeProp.enumValueIndex = (int)AnimationEventData.ParameterData.ParameterType.HumanBodyBone;
            else if (typeof(Component).IsAssignableFrom(paramType))
                typeProp.enumValueIndex = (int)AnimationEventData.ParameterData.ParameterType.Component;
            else if (paramType == typeof(Color))
                typeProp.enumValueIndex = (int)AnimationEventData.ParameterData.ParameterType.Color;
            else if (paramType == typeof(AnimationCurve))
                typeProp.enumValueIndex = (int)AnimationEventData.ParameterData.ParameterType.AnimationCurve;
            else if (paramType == typeof(AudioClip))
                typeProp.enumValueIndex = (int)AnimationEventData.ParameterData.ParameterType.AudioClip;
            else if (paramType == typeof(Texture))
                typeProp.enumValueIndex = (int)AnimationEventData.ParameterData.ParameterType.Texture;
            else if (paramType == typeof(Material))
                typeProp.enumValueIndex = (int)AnimationEventData.ParameterData.ParameterType.Material;
            else if (paramType == typeof(Sprite))
                typeProp.enumValueIndex = (int)AnimationEventData.ParameterData.ParameterType.Sprite;
            else if (paramType == typeof(LayerMask))
                typeProp.enumValueIndex = (int)AnimationEventData.ParameterData.ParameterType.LayerMask;
            else if (paramType == typeof(UnityEngine.Object))
                typeProp.enumValueIndex = (int)AnimationEventData.ParameterData.ParameterType.Object;
            else if (paramType == typeof(CameraSettings))
                typeProp.enumValueIndex = (int)AnimationEventData.ParameterData.ParameterType.CameraSettings;
            else
                return false;

            return true;
        }


        private float GetParametersHeight(SerializedProperty parametersProp)
        {
            float height = 0f;
            for (int i = 0; i < parametersProp.arraySize; i++)
            {
                // Corrected: removed the extra period.
                SerializedProperty param = parametersProp.GetArrayElementAtIndex(i);
                height += GetParameterHeight(param) + EditorGUIUtility.standardVerticalSpacing * 0.5f;
            }
            // Corrected: Removed multiplication by 10.
            return height;
        }

        private float GetParameterHeight(SerializedProperty paramProp)
        {
            // Start with one line for the parameter label (or base field).
            float height = EditorGUIUtility.singleLineHeight;

            // Get the parameter type (assuming your enum is stored in a property called "type")
            SerializedProperty typeProp = paramProp.FindPropertyRelative("type");
            AnimationEventData.ParameterData.ParameterType paramType =
                (AnimationEventData.ParameterData.ParameterType)typeProp.enumValueIndex;

            // If the parameter type is CameraSettings, add the height of the cameraSettings property.
            if (paramType == AnimationEventData.ParameterData.ParameterType.CameraSettings)
            {
                SerializedProperty csProp = paramProp.FindPropertyRelative("cameraSettings");
                // Get the complete height of the CameraSettings property, including its children.
                height = EditorGUI.GetPropertyHeight(csProp, true);
            }

            return height;
        }


        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (!property.isExpanded)
                return HEADER_HEIGHT;

            float height = HEADER_HEIGHT + VERTICAL_SPACING;  // Foldout
            height += (LINE_HEIGHT + VERTICAL_SPACING);   // Time, Target
            height += SECTION_SPACING * 2;                        // Extra space before parameters
            height += HEADER_HEIGHT + VERTICAL_SPACING;       // Parameters header

            var parametersProp = property.FindPropertyRelative("parameters");
            var targetObjectProp = property.FindPropertyRelative("targetObject");
            var foldOut = property.FindPropertyRelative("foldOut");

            if (foldOut.boolValue && parametersProp.arraySize > 0)
                height += GetParametersHeight(parametersProp);
            if (targetObjectProp.objectReferenceValue != null)
                height += (LINE_HEIGHT + VERTICAL_SPACING); // Method

            return height;
        }
    }
}