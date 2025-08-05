using UnityEngine;
using UnityEditor;
using System.Collections;

namespace MalbersAnimations.Weapons
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(MBow))]
    public class MBowEditor : MWeaponEditor
    {
        string[] axis = { "+X", "-X", "+Y", "-Y", "+Z", "-Z" };
        SerializedProperty
              UpperBn, LowerBn , UpperIndex, BowTension, LowerIndex, MaxTension, holdTime/*, tensionLimit*/;

        MBow myBow;

        private void OnEnable()
        {
            SetOnEnable();
            myBow = (MBow)target;
            UpperBn = serializedObject.FindProperty("UpperBn");
            LowerBn = serializedObject.FindProperty("LowerBn");
            UpperIndex = serializedObject.FindProperty("UpperIndex");
            LowerIndex = serializedObject.FindProperty("LowerIndex");
            holdTime = serializedObject.FindProperty("holdTime");
            BowTension = serializedObject.FindProperty("BowTension");
            MaxTension = serializedObject.FindProperty("MaxTension");
            //tensionLimit = serializedObject.FindProperty("tensionLimit");

            myBow.InitializeBow();
            EditorUtility.SetDirty(myBow);
            serializedObject.ApplyModifiedProperties();
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            MalbersEditor.DrawDescription("Bow Weapons Properties");


            EditorGUILayout.BeginVertical(MalbersEditor.StyleGray);
            {
                CommonWeaponProperties();
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUI.indentLevel++;
                EditorGUI.BeginChangeCheck();
                myBow.BonesFoldout = EditorGUILayout.Foldout(myBow.BonesFoldout, new GUIContent("Bow Joints", "All References for the Bow Bones"));

                if (myBow.BonesFoldout)
                {
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("knot"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("arrowPoint"));
                    EditorGUILayout.BeginVertical(EditorStyles.helpBox);

                    EditorGUI.BeginChangeCheck();
                    EditorGUILayout.PropertyField(UpperBn, new GUIContent("Upper Chain", "Upper bone chain of the bow"), true);
                    EditorGUILayout.PropertyField(LowerBn, new GUIContent("Lower Chain", "Lower bone chain of the bow"), true);

                    if (EditorGUI.EndChangeCheck())
                    {
                        myBow.InitializeBow();
                        EditorUtility.SetDirty(myBow);
                    }
                    EditorGUILayout.EndVertical();
                }
                EditorGUI.indentLevel--;
                EditorGUILayout.EndVertical();

                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                UpperIndex.intValue = EditorGUILayout.Popup("Upper Rot Axis", UpperIndex.intValue, axis);
                LowerIndex.intValue = EditorGUILayout.Popup("Lower Rot Axis", LowerIndex.intValue, axis);
                EditorGUILayout.EndVertical();

                myBow.RotUpperDir = Axis(myBow.UpperIndex);
                myBow.RotLowerDir = Axis(myBow.LowerIndex);

                EditorGUI.BeginChangeCheck();
                {
                    EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                    EditorGUILayout.PropertyField(MaxTension, new GUIContent("Max Tension", "Max Angle that the Bow can Bent"));
                    //EditorGUILayout.PropertyField(tensionLimit, new GUIContent("Tension Limit", "Bow Tension Normalized"));
                    EditorGUILayout.PropertyField(holdTime, new GUIContent("Hold Time", "Time to stretch the string to the Max Tension"));
                    EditorGUILayout.EndVertical();

                    if (myBow.BowIsSet)
                    {
                        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                        EditorGUILayout.PropertyField(BowTension, new GUIContent("Bow Tension (V)", "Bow Tension Normalized"));

                        if (BowTension.floatValue > 0)
                        {
                            EditorGUILayout.HelpBox("This is for visual purpose only, please return the Bow Tension to 0", MessageType.Warning);
                        }
                        EditorGUILayout.EndVertical();
                    }
                }
                if (EditorGUI.EndChangeCheck())
                {
                   
                    if (MaxTension.floatValue < 0)
                    {
                        MaxTension.floatValue = 0;
                    }

                    if (myBow.BowIsSet)
                        myBow.BendBow(myBow.BowTension);

                    EditorUtility.SetDirty(myBow);
                }
               
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                {
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("releaseArrow"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("arrow"));
                   // myBow.arrow =(GameObject)  EditorGUILayout.ObjectField(new GUIContent("Arrow", "Arrow Prefab"), myBow.arrow,typeof(GameObject),false );
                }
                EditorGUILayout.EndVertical();

                SoundsList();

                EventList();
                CheckWeaponID();
            }
            EditorGUILayout.EndVertical();

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(target, "MBow Inspector");
                //  EditorUtility.SetDirty(target);
            }

            serializedObject.ApplyModifiedProperties();
        }

        public override void UpdateSoundHelp()
        {
            SoundHelp = "0:Draw   1:Store   2:Hold   3:Release";
        }

        public override void CustomEvents()
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("OnLoadArrow"), new GUIContent("On Load Arrow"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("OnHold"), new GUIContent("On Hold Arrow"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("OnReleaseArrow"), new GUIContent("On Release Arrow"));
        }

        protected override string CustomEventsHelp()
        {
            return "\n\nOn Load Arrow: Invoked when the arrow is instantiated.\n (GameObject) the instance of the Arrow. \n\nOnHold: Invoked when the bow is being bent (0 to 1)\n\nOn Release Arrow: Invoked when the Arrow is released.\n (GameObject) the instance of the Arrow.";
        }
        Vector3 Axis(int Index)
        {
            switch (Index)
            {
                case 0: return Vector3.right;
                case 1: return -Vector3.right;
                case 2: return Vector3.up;
                case 3: return -Vector3.up;
                case 4: return Vector3.forward;
                case 5: return -Vector3.forward;
                default: return Vector3.zero;
            }
        }
    }
}