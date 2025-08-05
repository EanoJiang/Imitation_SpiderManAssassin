using UnityEngine;
using UnityEditor;

namespace MalbersAnimations.Controller
{
    [CustomEditor(typeof(Zone))]
   // [CanEditMultipleObjects]
    public class ZoneEditor : Editor
    {
        private Zone M;

        SerializedProperty
            HeadOnly,
            HeadName,
            zoneType,
            stateID,
            modeID,
            ID,
            ActionID,
            auto,
            AutomaticDisabled,
            AlignPos,
            AlignRot,
            AlignLookAt,
            AlingPoint,
            AlingPoint2,
            AlignTime,
            StatModifier,
            stanceID
            ;


        MonoScript script;
        private void OnEnable()
        {
            M = ((Zone)target);
            script = MonoScript.FromMonoBehaviour((MonoBehaviour)target);

            HeadOnly = serializedObject.FindProperty("HeadOnly");
            HeadName = serializedObject.FindProperty("HeadName");
            zoneType = serializedObject.FindProperty("zoneType");
            stateID = serializedObject.FindProperty("stateID");
            modeID = serializedObject.FindProperty("modeID");
            stanceID = serializedObject.FindProperty("stanceID");
            ID = serializedObject.FindProperty("ID");
            ActionID = serializedObject.FindProperty("ActionID");
            auto = serializedObject.FindProperty("automatic");
            AutomaticDisabled = serializedObject.FindProperty("AutomaticDisabled");

            AlignPos = serializedObject.FindProperty("AlignPos");
            AlignRot = serializedObject.FindProperty("AlignRot");
            AlignLookAt = serializedObject.FindProperty("AlignLookAt");
            AlingPoint = serializedObject.FindProperty("AlingPoint");
            AlingPoint2 = serializedObject.FindProperty("AlingPoint2");
            AlignTime = serializedObject.FindProperty("AlignTime");
            StatModifier = serializedObject.FindProperty("StatModifier");
        }


        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            MalbersEditor.DrawDescription("Zone Activate |States| or |Modes| on an Animal");

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.BeginVertical(MalbersEditor.StyleGray);
            {
                MalbersEditor.DrawScript(script);

                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                    //zoneType.intValue = (int)(ZoneType)EditorGUILayout.EnumPopup(new GUIContent("Zone Type", "Choose between a Mode or a State for the Zone"), (ZoneType)zoneType.intValue);
                    EditorGUILayout.PropertyField(zoneType, new GUIContent("Zone Type", "Choose between a Mode or a State for the Zone"));

                    ZoneType zone = (ZoneType)zoneType.intValue;


                switch (zone)
                {
                    case ZoneType.Mode:
                        EditorGUILayout.PropertyField(modeID, new GUIContent("Mode ID", "Which Mode to Set when entering the Zone"));

                        serializedObject.ApplyModifiedProperties();


                        if (M.modeID != null && M.modeID == 4)
                        {
                            EditorGUILayout.PropertyField(ActionID, new GUIContent("Action ID", "Which Action to Set when entering the Zone"));

                            if (ActionID.objectReferenceValue == null)
                            {
                                EditorGUILayout.HelpBox("Please Select an Action ID", MessageType.Error);
                            }


                        }
                        else
                        {
                            EditorGUILayout.PropertyField(ID, new GUIContent("Ability ID", "Which Ability to Set when entering the Zone"));
                            if (ActionID.objectReferenceValue == null)
                            {
                                EditorGUILayout.HelpBox("Please Select an Ability ID", MessageType.Error);
                            }
                        }
                        break;
                    case ZoneType.State:
                        EditorGUILayout.PropertyField(stateID, new GUIContent("State ID", "Which State will Activate when entering the Zone"));
                        if (stateID.objectReferenceValue == null)
                        {
                            EditorGUILayout.HelpBox("Please Select an State ID", MessageType.Error);
                        }
                        break;
                    case ZoneType.Stance:
                        EditorGUILayout.PropertyField(stanceID, new GUIContent("Stance ID", "Which Stance will Activate when entering the Zone"));
                        if (stanceID.objectReferenceValue == null)
                        {
                            EditorGUILayout.HelpBox("Please Select an Stance ID", MessageType.Error);
                        }
                        break;
                    default:
                        break;
                }

   

                EditorGUILayout.EndVertical();

                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                {
                    EditorGUILayout.PropertyField(HeadOnly, new GUIContent("Head Only", "Activate when the Head Bone the Zone.\nThe Head Bone needs a collider and be Named Head"));
                    if (HeadOnly.boolValue)
                    {
                        EditorGUILayout.PropertyField(HeadName, new GUIContent("Head Name", "Name for the Head Bone"));
                    }
                }
                EditorGUILayout.EndVertical();

                if (zone == ZoneType.Mode)
                {
                    EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                    {
                        EditorGUILayout.PropertyField(auto, new GUIContent("Automatic", "As soon as the animal enters the zone play the action"));

                        if (auto.boolValue)
                        {
                            EditorGUILayout.PropertyField(AutomaticDisabled, new GUIContent("Disabled", "if true the Trigger will be disabled for this value in seconds"));

                            if (AutomaticDisabled.floatValue < 0) AutomaticDisabled.floatValue = 0;
                        }
                    }
                    EditorGUILayout.EndVertical();
                }

                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                {
                    EditorGUILayout.BeginHorizontal();

                    var aling = serializedObject.FindProperty("Align");
                    EditorGUILayout.PropertyField(aling, new GUIContent("Align", "Aligns the Animal to the Align Position and Rotation of the AlignPoint"));

                    if (aling.boolValue)
                    {
                        AlignPos.boolValue = GUILayout.Toggle(AlignPos.boolValue, new GUIContent("P", "Align Position"), EditorStyles.miniButton, GUILayout.MaxWidth(25));
                        AlignRot.boolValue = GUILayout.Toggle(AlignRot.boolValue, new GUIContent("R", "Align Rotation"), EditorStyles.miniButton, GUILayout.MaxWidth(25));

                        if (M.AlignPos || M.AlignRot)
                        {
                            M.AlignLookAt = false;
                        }

                        AlignLookAt.boolValue = GUILayout.Toggle(AlignLookAt.boolValue, new GUIContent("L", "Align Looking at the Zone"), EditorStyles.miniButton, GUILayout.MaxWidth(25));

                        if (AlignLookAt.boolValue)
                        {
                            AlignPos.boolValue = AlignRot.boolValue = false;
                        }
                    }

                    if (AlingPoint.objectReferenceValue == null)
                    {
                        AlingPoint.objectReferenceValue = M.transform;
                    }
                    serializedObject.ApplyModifiedProperties();


                    EditorGUILayout.EndHorizontal();

                    if (aling.boolValue)
                    {
                        if (AlignRot.boolValue || AlignPos.boolValue)
                        {
                            EditorGUILayout.PropertyField(serializedObject.FindProperty("DoubleSided"), new GUIContent("Double Sided", "When Rotation is Enabled then It will find the closest Rotation"));
                        }
                        EditorGUILayout.PropertyField(AlingPoint, new GUIContent("Aling Point", "the Animal will move to the Position of the Align Point"));
                        EditorGUILayout.PropertyField(AlingPoint2, new GUIContent("Aling Point End", "If Point End is Active then the Animal will align to the closed position from the 2 align points line"));
                        EditorGUILayout.PropertyField(AlignTime, new GUIContent("Align Time", "Time to aling"));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("AlignCurve"), new GUIContent("Align Curve"));

                        if (AlignTime.floatValue < 0) AlignTime.floatValue = 0;
                    }
                }
                EditorGUILayout.EndVertical();


                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                {
                    EditorGUI.indentLevel++;
                    EditorGUILayout.PropertyField(StatModifier,true);
                    EditorGUI.indentLevel--;
                }
                EditorGUILayout.EndVertical();


                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUI.indentLevel++;
                M.EditorShowEvents = EditorGUILayout.Foldout(M.EditorShowEvents, "Events");
                EditorGUI.indentLevel--;

                if (M.EditorShowEvents)
                {
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("OnEnter"), new GUIContent("On Animal Enter"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("OnExit"), new GUIContent("On Animal Exit"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("OnZoneActivation"), new GUIContent("On Zone Active"));
                }
                EditorGUILayout.EndVertical();


                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUI.indentLevel++;
                M.EditorAI = EditorGUILayout.Foldout(M.EditorAI, "AI");
                if (M.EditorAI)
                {
                    EditorGUI.indentLevel--;
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("stoppingDistance"), new GUIContent("Stopping Distance"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("waitTime"), new GUIContent("Wait Time", "Wait Time after the Action ended"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("pointType"), new GUIContent("Zone Type", "Which type is this action zone"));
                    EditorGUI.indentLevel++;
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("nextTargets"), new GUIContent("Next Targets", "Posible Targets to go after finishing the Action"), true);
                    EditorGUI.indentLevel--;
                }
                EditorGUI.indentLevel--;
                EditorGUILayout.EndVertical();

                //      base.OnInspectorGUI();
            }
            EditorGUILayout.EndVertical();

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(target, "Zone Inspector");
                EditorUtility.SetDirty(target);
            }

            serializedObject.ApplyModifiedProperties();

        }
    }
}