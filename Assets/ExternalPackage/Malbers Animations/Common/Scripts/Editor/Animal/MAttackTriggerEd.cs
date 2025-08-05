using MalbersAnimations.Scriptables;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace MalbersAnimations.Controller
{
    [CustomEditor(typeof(MAttackTrigger))]
    [CanEditMultipleObjects]
    public class MAttackTriggerEd : Editor
    {

        MonoScript script;
        SerializedProperty SelfStatEnter, Trigger, PushForce, index, SelfStatExit, EnemyStatEnter, EnemyStatExit, debug, DebugColor, hitLayer, triggerInteraction;

        private void OnEnable()
        {
           // M = ((MAttackTrigger)target);
            script = MonoScript.FromMonoBehaviour((MonoBehaviour)target);


        //private MAttackTrigger M;
            index = serializedObject.FindProperty("index");
            PushForce = serializedObject.FindProperty("PushForce");
            Trigger = serializedObject.FindProperty("Trigger");
            SelfStatEnter = serializedObject.FindProperty("SelfStatEnter");
            SelfStatExit = serializedObject.FindProperty("SelfStatExit");
            EnemyStatEnter = serializedObject.FindProperty("EnemyStatEnter");
            EnemyStatExit = serializedObject.FindProperty("EnemyStatExit");
            debug = serializedObject.FindProperty("debug");
            DebugColor = serializedObject.FindProperty("DebugColor");
            hitLayer = serializedObject.FindProperty("hitLayer");
            triggerInteraction = serializedObject.FindProperty("triggerInteraction");

        }


        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            MalbersEditor.DrawDescription("Attack Trigger Logic. By default should be Disabled.");

            EditorGUI.BeginChangeCheck();

            EditorGUILayout.BeginVertical(MalbersEditor.StyleGray);
            {
                MalbersEditor.DrawScript(script);

                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                {
                    EditorGUILayout.PropertyField(index);
                    EditorGUILayout.PropertyField(Trigger);
                    EditorGUILayout.PropertyField(PushForce);
                }
                EditorGUILayout.EndVertical();

                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                {
                    EditorGUILayout.PropertyField(hitLayer);
                    EditorGUILayout.PropertyField(triggerInteraction);
                }
                EditorGUILayout.EndVertical();

                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                {
                    EditorGUILayout.LabelField("Owner Stat", EditorStyles.boldLabel);
                    EditorGUI.indentLevel++;
                    EditorGUILayout.PropertyField(SelfStatEnter, true);
                    EditorGUILayout.PropertyField(SelfStatExit, true);
                    EditorGUI.indentLevel--;
                }
                EditorGUILayout.EndVertical();

                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                {
                    EditorGUILayout.LabelField("Enemy Stat", EditorStyles.boldLabel);
                    EditorGUI.indentLevel++;
                    EditorGUILayout.PropertyField(EnemyStatEnter,true);
                    EditorGUILayout.PropertyField(EnemyStatExit, true);
                    EditorGUI.indentLevel--;
                }

                EditorGUILayout.EndVertical();

                EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
                {
                    EditorGUILayout.PropertyField(debug);
                    if (debug.boolValue)
                    {
                        EditorGUILayout.PropertyField(DebugColor, GUIContent.none);
                    }
                }
                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.EndVertical();
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(target, "Strafe Inspector");
               // EditorUtility.SetDirty(target);
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}