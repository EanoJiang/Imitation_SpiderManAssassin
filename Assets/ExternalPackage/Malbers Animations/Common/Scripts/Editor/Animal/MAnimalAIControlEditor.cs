using UnityEngine;
using UnityEditor;
using MalbersAnimations.Utilities;

namespace MalbersAnimations.Controller
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(MAnimalAIControl),true)]
    public class AnimalAIControlEd : Editor
    {
        MAnimalAIControl M;
        MonoScript script;

        SerializedProperty stoppingDistance, walkDistance;

        private void OnEnable()
        {
            M = (MAnimalAIControl)target;
            script = MonoScript.FromMonoBehaviour(M);

            stoppingDistance = serializedObject.FindProperty("stoppingDistance");
            walkDistance = serializedObject.FindProperty("walkDistance");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            MalbersEditor.DrawDescription("Basic AI system for Animal Script");

            EditorGUI.BeginChangeCheck();

            EditorGUILayout.BeginVertical(MalbersEditor.StyleGray);

            MalbersEditor.DrawScript(script);

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("target"), new GUIContent("Target", "Target to follow"));

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(stoppingDistance, new GUIContent("Stopping Distance", "Agent Stopping Distance"));
            EditorGUILayout.PropertyField(walkDistance, new GUIContent("Walk Distance", "Distance to stop Runing and Start Walking"));

            if (EditorGUI.EndChangeCheck())
            {
                if (M.Agent)
                {
                    M.Agent.stoppingDistance = stoppingDistance.floatValue;
                    serializedObject.ApplyModifiedProperties();
                }
            }

            EditorGUI.BeginChangeCheck();

            EditorGUILayout.EndVertical();
 
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUI.indentLevel++;
            M.showevents = EditorGUILayout.Foldout(M.showevents, "Events");
            EditorGUI.indentLevel--;



            if (M.showevents)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("OnTargetPositionArrived"), new GUIContent("On Position Arrived"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("OnTargetArrived"), new GUIContent("On Target Arrived"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("OnActionStart"), new GUIContent("On Action Start"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("OnActionEnd"), new GUIContent("On Action End"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("OnDebug"), new GUIContent("On Debug"));
            }
            EditorGUILayout.EndVertical();


            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            var d = serializedObject.FindProperty("debug");

            EditorGUILayout.PropertyField(d);

            if (d.boolValue && Application.isPlaying)
            {
                EditorGUI.BeginDisabledGroup(true);
                EditorGUILayout.ObjectField("Next Target",M.NextTarget, typeof(Transform), false);
                EditorGUILayout.ObjectField("Next Target Zone?",M.IsZone, typeof(Zone), false);
                EditorGUILayout.ObjectField("Next Target WayPoint",M.IsWayPoint, typeof(MWayPoint), false);
                EditorGUILayout.Space();
                EditorGUILayout.FloatField("Remaining Distance", M.RemainingDistance);
                EditorGUILayout.Vector3Field("Target Position", M.TargetPosition);
                EditorGUILayout.ToggleLeft("Target is Moving?", M.TargetisMoving);
                EditorGUILayout.Space();
                EditorGUILayout.ToggleLeft("Is On Mode", M.IsOnMode);
                EditorGUILayout.ToggleLeft("Enter OffMesh", M.EnterOFFMESH);
                EditorGUILayout.ToggleLeft("Free Move", M.FreeMove);
                EditorGUILayout.ToggleLeft("Flying OffMesh", M.IsFlyingOffMesh);
                EditorGUILayout.ToggleLeft("Agent Active?", M.AgentActive);
                EditorGUILayout.ToggleLeft("Is Waiting?", M.IsWaiting);
                EditorGUI.EndDisabledGroup();
            }

            EditorGUILayout.EndVertical();

            if (!M.Agent)
            {
                EditorGUILayout.HelpBox("There's no Agent found on the hierarchy on this gameobject\nPlease add a NavMesh Agent Component", MessageType.Error);
            }


            EditorGUILayout.EndVertical();

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(target, "Animal AI Control Changed");
                //EditorUtility.SetDirty(target);
            }
            serializedObject.ApplyModifiedProperties();
        }
    }
}