using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System;

namespace FS_ThirdPerson
{

    [CustomEditor(typeof(FootStepEffects))]
    public class FootStepEffectsEditor : Editor
    {
        SerializedProperty footStepSounds;
        SerializedProperty footStepParticles;
        SerializedProperty overrideType;
        SerializedProperty groundLayer;
        SerializedProperty soundIgnoreStates;
        SerializedProperty particleIgnoreStates;

        SerializedProperty overrideStepEffects;

        SerializedProperty adjustVolumeBasedOnSpeed;
        SerializedProperty minVolume;

        private bool soundIgnoreStatesFoldout = false;
        private bool particleIgnoreStatesFoldout = false;
        private bool overrideStepEffectsFoldout = false;

        private void OnEnable()
        {
            footStepSounds = serializedObject.FindProperty("footStepSounds");
            footStepParticles = serializedObject.FindProperty("footStepParticles");
            overrideType = serializedObject.FindProperty("overrideType");
            groundLayer = serializedObject.FindProperty("groundLayer");
            soundIgnoreStates = serializedObject.FindProperty("soundIgnoreStates");
            particleIgnoreStates = serializedObject.FindProperty("particleIgnoreStates");

            overrideStepEffects = serializedObject.FindProperty("overrideStepEffects");

            adjustVolumeBasedOnSpeed = serializedObject.FindProperty("adjustVolumeBasedOnSpeed");
            minVolume = serializedObject.FindProperty("minVolume");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            GUILayout.Space(5);
            EditorGUILayout.PropertyField(footStepSounds);
            EditorGUILayout.PropertyField(footStepParticles);

            GUILayout.Space(5);
            EditorGUILayout.PropertyField(overrideType);

            overrideStepEffectsFoldout = EditorGUILayout.Foldout(overrideStepEffectsFoldout, "Override Step Effects");

            if (overrideStepEffectsFoldout)
            {
                EditorGUI.indentLevel++;

                // Start box for the step effects list
                EditorGUILayout.BeginVertical("box");

                for (int i = 0; i < overrideStepEffects.arraySize; i++)
                {
                    SerializedProperty overrideStepEffect = overrideStepEffects.GetArrayElementAtIndex(i);


                    var tagProp = overrideStepEffect.FindPropertyRelative("tag");
                    var materialNameProp = overrideStepEffect.FindPropertyRelative("materialName");
                    var textureNameProp = overrideStepEffect.FindPropertyRelative("textureName");

                    var overrideFootStepSoundsProp = overrideStepEffect.FindPropertyRelative("ovverideFootStepSounds");
                    var overrideFootStepParticlesProp = overrideStepEffect.FindPropertyRelative("overrideFootStepParticles");

                    var footStepSoundsProp = overrideStepEffect.FindPropertyRelative("footStepSounds");
                    var footStepParticlesProp = overrideStepEffect.FindPropertyRelative("footStepParticles");

                    // Box around each step effect with foldout
                    EditorGUILayout.BeginVertical("helpBox");

                    EditorGUILayout.BeginHorizontal();
                    overrideStepEffect.isExpanded = EditorGUILayout.Foldout(overrideStepEffect.isExpanded, $"Step Effect {i + 1}", true, EditorStyles.foldoutHeader);
                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button("-", GUILayout.Width(25)))
                    {
                        overrideStepEffects.DeleteArrayElementAtIndex(i);
                        break;
                    }
                    EditorGUILayout.EndHorizontal();

                    if (overrideStepEffect.isExpanded)
                    {
                        EditorGUI.indentLevel++;

                        switch (overrideType.enumValueIndex)
                        {
                            case (int)StepEffectsOverrideType.MaterialName:
                                EditorGUILayout.PropertyField(materialNameProp, new GUIContent("Material Name"));
                                break;
                            case (int)StepEffectsOverrideType.Tag:
                                EditorGUILayout.PropertyField(tagProp, new GUIContent("Tag"));
                                break;
                            case (int)StepEffectsOverrideType.TextureName:
                                EditorGUILayout.PropertyField(textureNameProp, new GUIContent("Texture Name"));
                                break;
                            default:
                                break;
                        }

                        var footSound = overrideFootStepSoundsProp.boolValue;
                        if (footSound)
                            EditorGUILayout.BeginVertical("box");

                        EditorGUILayout.PropertyField(overrideFootStepSoundsProp, new GUIContent("Override Footstep Sounds"));
                        if (overrideFootStepSoundsProp.boolValue)
                        {
                            EditorGUI.indentLevel++;

                            for (int j = 0; j < footStepSoundsProp.arraySize; j++)
                            {
                                EditorGUILayout.BeginHorizontal();

                                SerializedProperty soundElement = footStepSoundsProp.GetArrayElementAtIndex(j);
                                EditorGUILayout.PropertyField(soundElement, new GUIContent($"Sound {j + 1}"));

                                if (GUILayout.Button("-", GUILayout.Width(25)))
                                {
                                    footStepSoundsProp.DeleteArrayElementAtIndex(j);
                                    break;
                                }

                                EditorGUILayout.EndHorizontal();
                            }

                            EditorGUILayout.BeginHorizontal();
                            GUILayout.FlexibleSpace();
                            if (GUILayout.Button("+", GUILayout.Width(25)))
                            {
                                AddElementToSerializedList(footStepSoundsProp);
                            }
                            EditorGUILayout.EndHorizontal();

                            EditorGUI.indentLevel--;
                        }

                        if (footSound)
                            EditorGUILayout.EndVertical();

                        var footStep = overrideFootStepParticlesProp.boolValue;
                        if (footStep)
                            EditorGUILayout.BeginVertical("box");

                        EditorGUILayout.PropertyField(overrideFootStepParticlesProp, new GUIContent("Override Footstep Particles"));
                        if (overrideFootStepParticlesProp.boolValue)
                        {
                            EditorGUI.indentLevel++;

                            for (int j = 0; j < footStepParticlesProp.arraySize; j++)
                            {
                                EditorGUILayout.BeginHorizontal();

                                SerializedProperty particleElement = footStepParticlesProp.GetArrayElementAtIndex(j);
                                EditorGUILayout.PropertyField(particleElement, new GUIContent($"Particle {j + 1}"));

                                if (GUILayout.Button("-", GUILayout.Width(25)))
                                {
                                    footStepParticlesProp.DeleteArrayElementAtIndex(j);
                                    break;
                                }

                                EditorGUILayout.EndHorizontal();
                            }

                            EditorGUILayout.BeginHorizontal();
                            GUILayout.FlexibleSpace();
                            if (GUILayout.Button("+", GUILayout.Width(25)))
                            {
                                AddElementToSerializedList(footStepParticlesProp);
                            }
                            EditorGUILayout.EndHorizontal();

                            EditorGUI.indentLevel--;
                        }
                        if (footStep)
                            EditorGUILayout.EndVertical();

                        EditorGUI.indentLevel--;
                    }

                    EditorGUILayout.EndVertical(); // Close the box for the current step effect
                }

                EditorGUILayout.BeginHorizontal();
                if(overrideStepEffects.arraySize > 0)
                    GUILayout.FlexibleSpace();

                var label = overrideStepEffects.arraySize > 0 ? "+" : "Add Element";
                var width = overrideStepEffects.arraySize > 0 ? 25 : 150;

                if (GUILayout.Button(label, GUILayout.Width(width)))
                {
                    AddElementToSerializedList(overrideStepEffects);
                }
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.EndVertical(); // Close the box for the entire step effects list

                EditorGUI.indentLevel--;
            }




            DrawEnumCheckboxes(ref soundIgnoreStatesFoldout, "Sound Ignore States", soundIgnoreStates);
            DrawEnumCheckboxes(ref particleIgnoreStatesFoldout, "Particle Ignore States", particleIgnoreStates);

            GUILayout.Space(5);

            EditorGUILayout.PropertyField(adjustVolumeBasedOnSpeed);
            if (adjustVolumeBasedOnSpeed.boolValue)
                EditorGUILayout.PropertyField(minVolume);

            EditorGUILayout.PropertyField(groundLayer);

            serializedObject.ApplyModifiedProperties();
        }


        

        public void DrawEnumCheckboxes(ref bool foldout,  string label, SerializedProperty prop)
        {
            foldout = EditorGUILayout.Foldout(foldout, label);
            if (foldout)
            {
                EditorGUILayout.BeginVertical("box");
                EditorGUI.indentLevel++;
                SystemState[] allStates = (SystemState[])Enum.GetValues(typeof(SystemState));

                for (int i = 0; i < allStates.Length; i++)
                {
                    SystemState state = allStates[i];

                    bool isSelected = CheckStateContains(prop, state);

                    EditorGUI.BeginChangeCheck();
                    bool toggle = EditorGUILayout.Toggle(state.ToString(), isSelected);
                    if (EditorGUI.EndChangeCheck())
                    {
                        if (toggle && !isSelected)
                        {
                            prop.arraySize++;
                            prop.GetArrayElementAtIndex(prop.arraySize - 1).enumValueIndex = (int)state;

                        }
                        else if (!toggle && isSelected)
                        {
                            for (int j = 0; j < prop.arraySize; j++)
                            {
                                if ((int)state == prop.GetArrayElementAtIndex(j).intValue)
                                    prop.DeleteArrayElementAtIndex(j);

                            }
                        }
                    }
                }
                EditorGUI.indentLevel--;
                EditorGUILayout.EndVertical();
            }
        }


        bool CheckStateContains(SerializedProperty systemStatesProp, SystemState state)
        {
            for (int i = 0; i < systemStatesProp.arraySize; i++)
            {
                if ((int)state == systemStatesProp.GetArrayElementAtIndex(i).intValue)
                    return true;
            }
            return false;
        }

        private void AddElementToSerializedList(SerializedProperty listProp)
        {
            listProp.arraySize++;  // Increase the array size by 1
            SerializedProperty newElement = listProp.GetArrayElementAtIndex(listProp.arraySize - 1);  // Access the new element
            newElement.isExpanded = true;  // Initialize it (optional, based on type)
        }
    }
}