using UnityEngine;
using UnityEditor;

namespace MalbersAnimations
{
    [CustomEditor(typeof(RigidConstraintsB))]
    public class RigidConstraintsBEd : Editor
    {
        private RigidConstraintsB M;

        private void OnEnable()
        {
            M = ((RigidConstraintsB)target);
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            MalbersEditor.DrawDescription("Modify the Rigidbody Constraints attached to this Animator");

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.BeginVertical(MalbersEditor.StyleGray);
            {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                {
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("OnEnterDrag"));
                }
                EditorGUILayout.EndVertical();

                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                {
                    M.OnEnter = EditorGUILayout.Toggle("On Enter", M.OnEnter, EditorStyles.radioButton);
                    if (M.OnEnter)
                        M.OnExit = false;
                    else
                        M.OnExit = true;
                    M.OnExit = EditorGUILayout.Toggle("On Exit", M.OnExit, EditorStyles.radioButton);

                    if (M.OnExit)
                        M.OnEnter = false;
                    else
                        M.OnEnter = true;
                }
                EditorGUILayout.EndVertical();

                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                {
                    EditorGUILayout.BeginHorizontal();
                    {
                        EditorGUILayout.LabelField("Constraints  ", EditorStyles.boldLabel, GUILayout.MaxWidth(105));
                        EditorGUILayout.LabelField(" ", GUILayout.MaxWidth(15));
                        EditorGUILayout.LabelField("X", EditorStyles.boldLabel, GUILayout.MaxWidth(15));
                        EditorGUILayout.LabelField(" ", GUILayout.MaxWidth(15));
                        EditorGUILayout.LabelField("Y", EditorStyles.boldLabel, GUILayout.MaxWidth(15));
                        EditorGUILayout.LabelField("     Z", EditorStyles.boldLabel, GUILayout.MaxWidth(35));
                    }
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.BeginHorizontal();
                    {
                        EditorGUILayout.LabelField("Position ", GUILayout.MaxWidth(105));
                        EditorGUILayout.LabelField(" ", GUILayout.MaxWidth(15));
                        M.PosX = EditorGUILayout.Toggle(M.PosX, GUILayout.MaxWidth(15));
                        EditorGUILayout.LabelField(" ", GUILayout.MaxWidth(15));
                        M.PosY = EditorGUILayout.Toggle(M.PosY, GUILayout.MaxWidth(15));
                        EditorGUILayout.LabelField(" ", GUILayout.MaxWidth(15));
                        M.PosZ = EditorGUILayout.Toggle(M.PosZ, GUILayout.MaxWidth(15));
                    }
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.BeginHorizontal();
                    {
                        EditorGUILayout.LabelField("Rotation ", GUILayout.MaxWidth(105));
                        EditorGUILayout.LabelField(" ", GUILayout.MaxWidth(15));
                        M.RotX = EditorGUILayout.Toggle(M.RotX, GUILayout.MaxWidth(15));
                        EditorGUILayout.LabelField(" ", GUILayout.MaxWidth(15));
                        M.RotY = EditorGUILayout.Toggle(M.RotY, GUILayout.MaxWidth(15));
                        EditorGUILayout.LabelField(" ", GUILayout.MaxWidth(15));
                        M.RotZ = EditorGUILayout.Toggle(M.RotZ, GUILayout.MaxWidth(15));
                    }
                    EditorGUILayout.EndHorizontal();
                }
                EditorGUILayout.EndVertical();
            }
            EditorGUILayout.EndVertical();


            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(target, "Rigid Constraint Inspector");
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}