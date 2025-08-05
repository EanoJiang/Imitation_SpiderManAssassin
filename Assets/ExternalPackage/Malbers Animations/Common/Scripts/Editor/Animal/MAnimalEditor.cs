using MalbersAnimations.Scriptables;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace MalbersAnimations.Controller
{
    [CustomEditor(typeof(MAnimal))]
    public class MAnimalEditor : Editor 
    {
        private List<Type> StatesType = new List<Type>();
        private ReorderableList Reo_List_States;
        private ReorderableList Reo_List_Pivots;
        private ReorderableList Reo_List_Modes;
        private ReorderableList Reo_List_Speeds;

        SerializedProperty
            S_StateList, S_PivotsList, Height, S_Mode_List, Editor_Tabs1, Editor_Tabs2, /*ModeIndexSelected,*/ ModeShowAbilities, OnEnterExitStates, RB,Anim,
            m_Vertical, m_Horizontal, m_IDFloat, m_IDInt, m_State, m_LastState, m_Mode, m_Grounded, m_Movement, m_SpeedMultiplier, m_UpDown, OnMainPlayer,
            m_Stance, m_Slope, m_Type, m_StateTime, m_DeltaAngle, lockInput, lockMovement, Rotator, animalType, RayCastRadius, /*AnimatorUpdatePhysics,*/
            /*UpdateParameters,*/ triggerInteraction, AnimatorSpeed, OnMovementLocked, OnInputLocked, OnSprintEnabled, OnGrounded, OnStanceChange, OnStateChange, OnModeChange,
            OnSpeedChange, OnAnimationChange, showPivots, ShowpivotColor, GroundLayer, maxAngleSlope, AlignPosLerp, AlignRotLerp, gravityDirection, GravityForce, useCameraUp,
            GravityMultiplier,useSprintGlobal, hitLayer, SmoothVertical, TurnMultiplier, UpDownLerp, rootMotion, Player,OverrideStartState, CloneStates, S_Speed_List, UseCameraInput;
           

        MAnimal m;
        private MonoScript script;
        private GenericMenu addMenu;

        private void FindSerializedProperties()
        {
            S_PivotsList = serializedObject.FindProperty("pivots");
            S_Mode_List = serializedObject.FindProperty("modes");
            S_Speed_List = serializedObject.FindProperty("speedSets");

            hitLayer = serializedObject.FindProperty("hitLayer");
            triggerInteraction = serializedObject.FindProperty("triggerInteraction");
            RB = serializedObject.FindProperty("RB");
            Anim = serializedObject.FindProperty("Anim");

            // mainCamera = serializedObject.FindProperty("MainCamera");
            UseCameraInput = serializedObject.FindProperty("useCameraInput");
            useCameraUp = serializedObject.FindProperty("useCameraUp");

            OnEnterExitStates = serializedObject.FindProperty("OnEnterExitStates");
            ModeShowAbilities = serializedObject.FindProperty("ModeShowAbilities");

            Height = serializedObject.FindProperty("height");
           // ModeIndexSelected = serializedObject.FindProperty("ModeIndexSelected");

            Editor_Tabs1 = serializedObject.FindProperty("Editor_Tabs1");
            Editor_Tabs2 = serializedObject.FindProperty("Editor_Tabs2");

            m_Vertical = serializedObject.FindProperty("m_Vertical");
            m_Horizontal = serializedObject.FindProperty("m_Horizontal");
            m_IDFloat = serializedObject.FindProperty("m_IDFloat");
            m_IDInt = serializedObject.FindProperty("m_IDInt");
            m_State = serializedObject.FindProperty("m_State");
            m_LastState = serializedObject.FindProperty("m_LastState");
            m_Mode = serializedObject.FindProperty("m_Mode");
            m_Grounded = serializedObject.FindProperty("m_Grounded");
            m_Movement = serializedObject.FindProperty("m_Movement");
            m_SpeedMultiplier = serializedObject.FindProperty("m_SpeedMultiplier");
            m_UpDown = serializedObject.FindProperty("m_UpDown");
            m_Stance = serializedObject.FindProperty("m_Stance");
            m_Slope = serializedObject.FindProperty("m_Slope");
            m_Type = serializedObject.FindProperty("m_Type");
            m_StateTime = serializedObject.FindProperty("m_StateTime");
            m_DeltaAngle = serializedObject.FindProperty("m_DeltaAngle");
            lockInput = serializedObject.FindProperty("lockInput");
            lockMovement = serializedObject.FindProperty("lockMovement");
            Rotator = serializedObject.FindProperty("Rotator");
            animalType = serializedObject.FindProperty("animalType");
            RayCastRadius = serializedObject.FindProperty("RayCastRadius");
            //AnimatorUpdatePhysics = serializedObject.FindProperty("AnimatorUpdatePhysics");
           // UpdateParameters = serializedObject.FindProperty("UpdateParameters");
            AnimatorSpeed = serializedObject.FindProperty("AnimatorSpeed");

            OnMovementLocked = serializedObject.FindProperty("OnMovementLocked");
            OnInputLocked = serializedObject.FindProperty("OnInputLocked");
            OnSprintEnabled = serializedObject.FindProperty("OnSprintEnabled");
            OnGrounded = serializedObject.FindProperty("OnGrounded");
            OnStanceChange = serializedObject.FindProperty("OnStanceChange");
            OnStateChange = serializedObject.FindProperty("OnStateChange");
            OnModeChange = serializedObject.FindProperty("OnModeChange");
            OnMainPlayer = serializedObject.FindProperty("OnMainPlayer");
            OnSpeedChange = serializedObject.FindProperty("OnSpeedChange");
            OnAnimationChange = serializedObject.FindProperty("OnAnimationChange");
            showPivots = serializedObject.FindProperty("showPivots");
            ShowpivotColor = serializedObject.FindProperty("ShowpivotColor");
            GroundLayer = serializedObject.FindProperty("GroundLayer");
            maxAngleSlope = serializedObject.FindProperty("maxAngleSlope");
            AlignPosLerp = serializedObject.FindProperty("AlignPosLerp");
            AlignRotLerp = serializedObject.FindProperty("AlignRotLerp");

            gravityDirection = serializedObject.FindProperty("gravityDirection");
            GravityForce = serializedObject.FindProperty("GravityForce");
            GravityMultiplier = serializedObject.FindProperty("GravityMultiplier");

            useSprintGlobal = serializedObject.FindProperty("useSprintGlobal");
            SmoothVertical = serializedObject.FindProperty("SmoothVertical");
            TurnMultiplier = serializedObject.FindProperty("TurnMultiplier");
            UpDownLerp = serializedObject.FindProperty("UpDownLerp");
            rootMotion = serializedObject.FindProperty("rootMotion");
            Player = serializedObject.FindProperty("isPlayer");
            OverrideStartState = serializedObject.FindProperty("OverrideStartState");
            CloneStates = serializedObject.FindProperty("CloneStates");
        }

        private void OnEnable()
        {
            m = (MAnimal)target;
            script = MonoScript.FromMonoBehaviour(target as MonoBehaviour);
            StatesType.Clear();

            FindSerializedProperties();



            // Set the array of types and type names of subtypes of Reaction.
            SetStatesNamesArray_New();

        //    SetStateNamesArray_Old();

            S_StateList = serializedObject.FindProperty("states");
           

            Reo_List_Pivots = new ReorderableList(serializedObject, S_PivotsList, true, true, false, false)
            {
                drawElementCallback = DrawElement_Pivots,
              //  onAddCallback = OnAddCallback_Pivots,
                drawHeaderCallback = DrawHeaderCallback_Pivots,
            };


            Reo_List_States = new ReorderableList(serializedObject, S_StateList, true, true, false, false)
            {
                drawElementCallback = Draw_Element_State,
                onReorderCallback = OnReorderCallback_States,
                drawHeaderCallback = Draw_Header_State,
            };

            Reo_List_Modes = new ReorderableList(serializedObject, S_Mode_List, true, true, false, false)
            {
                drawElementCallback = Draw_Element_Modes,
                drawHeaderCallback = Draw_Header_Modes,
                onAddCallback = OnAddCallback_Modes,
            };

            Reo_List_Speeds = new ReorderableList(serializedObject, S_Speed_List, true, true, false, false)
            {
                drawElementCallback = Draw_Element_Speed,
                drawHeaderCallback = Draw_Header_Speed,
              //  onAddCallback = OnAddCallback_Speed,
            };
        }



        public override void OnInspectorGUI()
        {
            MalbersEditor.DrawDescription("Animal Controller");
            EditorGUILayout.BeginVertical(MalbersEditor.StyleGray);
            MalbersEditor.DrawScript(script);
         

            serializedObject.Update();

            Editor_Tabs1.intValue = GUILayout.Toolbar(Editor_Tabs1.intValue, new string[] { "General", "Speeds", "States", "Modes" });

            if (Editor_Tabs1.intValue != 4)
            {
                Editor_Tabs2.intValue = 4;
            }

            Editor_Tabs2.intValue = GUILayout.Toolbar(Editor_Tabs2.intValue, new string[] { "Advanced", "Animator","Events" ,"Debug"});

            if (Editor_Tabs2.intValue != 4)
            {
                Editor_Tabs1.intValue = 4;
            }

            //First Tabs
            int Selection = Editor_Tabs1.intValue;

            if (Selection == 0) ShowGeneral();
            else if (Selection == 1)ShowSpeeds();
            else if (Selection == 2) ShowStates();
            else if (Selection == 3)ShowModes();

            //2nd Tabs
            Selection = Editor_Tabs2.intValue;

            if (Selection == 0) ShowAdvanced();
            else if (Selection == 1) ShowAnimParam();
            else if (Selection == 2) ShowEvents();
            else if (Selection == 3) ShowDebug();

            serializedObject.ApplyModifiedProperties();
            EditorGUILayout.EndVertical();
        }

        private void ShowAdvanced()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("Animator", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(Anim, new GUIContent("Animator"));
            EditorGUILayout.PropertyField(RB, new GUIContent("RigidBody"));

            EditorGUILayout.PropertyField(rootMotion, G_rootMotion);
            //EditorGUILayout.PropertyField(AnimatorUpdatePhysics, G_AnimatorUpdatePhysics);
            //EditorGUILayout.PropertyField(UpdateParameters, G_UpdateParameters);
            EditorGUILayout.PropertyField(AnimatorSpeed,G_AnimatorSpeed);
            EditorGUILayout.EndVertical();
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("Misc", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(RayCastRadius, G_RayCastRadius);
            EditorGUILayout.PropertyField(animalType, G_animalType);
            EditorGUILayout.PropertyField(Rotator,G_Rotator);
           // EditorGUILayout.PropertyField(mainCamera, G_mainCamera);
            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("Inputs", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(lockInput, G_LockInput);
            EditorGUILayout.PropertyField(lockMovement, G_lockMovement);
            EditorGUILayout.EndVertical();

        }

        private void ShowAnimParam()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField( "Main Animator Parameters", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField( m_Vertical);
            EditorGUILayout.PropertyField( m_Horizontal);
            EditorGUILayout.Space();
            EditorGUILayout.PropertyField( m_IDFloat);
            EditorGUILayout.PropertyField( m_IDInt);
            EditorGUILayout.Space();
            EditorGUILayout.PropertyField( m_State);
            EditorGUILayout.PropertyField( m_LastState);
            EditorGUILayout.PropertyField( m_Mode);
            EditorGUILayout.Space();
            EditorGUILayout.PropertyField( m_Grounded);
            EditorGUILayout.PropertyField( m_Movement);
            EditorGUILayout.PropertyField( m_SpeedMultiplier);
            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField( "Optional Animator Parameters", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField( m_UpDown);
            EditorGUILayout.PropertyField( m_Stance);
            EditorGUILayout.PropertyField( m_Slope);
            EditorGUILayout.PropertyField( m_Type);
            EditorGUILayout.PropertyField( m_StateTime);
            EditorGUILayout.PropertyField( m_DeltaAngle);

            EditorGUILayout.EndVertical();
        }


        private void ShowEvents()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("Events", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(OnMovementLocked);
            EditorGUILayout.PropertyField(OnInputLocked);
            EditorGUILayout.PropertyField(OnSprintEnabled);
            EditorGUILayout.PropertyField(OnGrounded);
            EditorGUILayout.PropertyField(OnStanceChange);
            EditorGUILayout.PropertyField(OnStateChange);
            EditorGUILayout.PropertyField(OnModeChange);
            EditorGUILayout.PropertyField(OnSpeedChange);
            EditorGUILayout.PropertyField(OnAnimationChange);
            EditorGUILayout.PropertyField(OnMainPlayer);
            EditorGUILayout.EndVertical();
        }

        private void ShowDebug()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            var Deb = serializedObject.FindProperty("debugStates");
            var DebM = serializedObject.FindProperty("debugModes");
            var DebG = serializedObject.FindProperty("debugGizmos");

            EditorGUILayout.PropertyField(Deb, new GUIContent("Debug States"));
            EditorGUILayout.PropertyField(DebM, new GUIContent("Debug Modes"));
            EditorGUILayout.PropertyField(DebG, new GUIContent("Debug Gizmos"));

            if (Application.isPlaying)
            {
                EditorGUI.BeginDisabledGroup(true);
                EditorGUILayout.LabelField("RB Speed: " + m.HorizontalSpeed.ToString("F4"));
                EditorGUILayout.LabelField("Current Speed Modifier: " + m.CurrentSpeedModifier.name);
                EditorGUILayout.LabelField("Current Speed Index: " + m.CurrentSpeedIndex);
                EditorGUILayout.ToggleLeft("Grounded", m.Grounded);
                EditorGUILayout.ToggleLeft("RootMotion", m.RootMotion);
                EditorGUILayout.ToggleLeft("Use Custom Rotation", m.UseCustomAlign);
                EditorGUILayout.ToggleLeft("Orient To Ground", m.UseOrientToGround);
                EditorGUILayout.Space();
                EditorGUILayout.ToggleLeft("Gravity", m.UseGravity);
                EditorGUILayout.Vector3Field("Gravity Vel", m.GravityStoredVelocity);
                EditorGUILayout.FloatField("Gravity Acel", m.GravityStoredAceleration);
                EditorGUILayout.Space();
                EditorGUILayout.ToggleLeft("Sprint", m.Sprint);
                EditorGUILayout.ToggleLeft("Free Movement", m.FreeMovement);
                EditorGUILayout.ToggleLeft("Input Locked", m.LockInput);
                EditorGUILayout.ToggleLeft("Movement Locked", m.LockMovement);
                EditorGUILayout.ToggleLeft("Is Playing Mode", m.IsPlayingMode);
                //EditorGUILayout.ToggleLeft("Use Rot Speed", m.UseAdditiveRot);
                EditorGUILayout.ToggleLeft("Use Pos Speed", m.UseAdditivePos);
                EditorGUILayout.Space();
                EditorGUILayout.FloatField("Terrain Slope", m.TerrainSlope);
                EditorGUILayout.FloatField("Slope Normalized", m.SlopeNormalized);
                EditorGUILayout.Space();
                EditorGUILayout.Vector3Field("Movement" , m.MovementAxis);
                EditorGUILayout.Vector3Field("Movement Smooth" ,m.MovementAxisSmoothed);
                EditorGUILayout.Vector3Field("Additive Pos " ,m.AdditivePosition);
                EditorGUILayout.FloatField("Delta Angle" ,m.DeltaAngle);
                EditorGUILayout.Space();
                EditorGUILayout.FloatField("Current Anim Tag", m.AnimStateTag);
                EditorGUILayout.Space();
                EditorGUILayout.IntField("Stance", m.Stance);
              //  EditorGUILayout.LabelField(m.ActiveMode == null ? " ActiveMode: Null" : ("ActiveMode: " + m.ActiveMode.ID.name));

                EditorGUI.EndDisabledGroup();
            }

            EditorGUILayout.EndVertical();
        }

        private void ShowModes()
        {
            //-------------------------Modes-------------------------------------------

            //EditorGUI.indentLevel++;
            //var showModes = serializedObject.FindProperty("showModes");
            //EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            //showModes.boolValue = EditorGUILayout.Foldout(showModes.boolValue, "Modes");
            //EditorGUI.indentLevel--;

            //if (Application.isPlaying)
            //{
            //    EditorGUILayout.LabelField(m.ActiveMode == null ? "Active Mode: Null" : ("Active Mode: " + m.ActiveMode.ID.name));
            //    EditorGUILayout.LabelField(m.InputMode == null ? "Input Mode: Null" : ("Input Mode: " + m.InputMode.ID.name));
            //}

            {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                Reo_List_Modes.DoLayoutList();        //Paint the Reordable List
                EditorGUILayout.EndVertical();
                var Index = Reo_List_Modes.index;

                // ModeIndexSelected.intValue = Reo_List_Modes.index;

                if (Index != -1)
                {
                    var SelectedMode = S_Mode_List.GetArrayElementAtIndex(Index);

                    var animationTag = SelectedMode.FindPropertyRelative("AnimationTag");
                 //   var PlayingMode = SelectedMode.FindPropertyRelative("PlayingMode");
                    var Input = SelectedMode.FindPropertyRelative("Input");
                    var CoolDown = SelectedMode.FindPropertyRelative("CoolDown");
                    var properties = SelectedMode.FindPropertyRelative("GlobalProperties");
                    var AbilityIndex = SelectedMode.FindPropertyRelative("abilityIndex");
                    var OnAbilityIndex = SelectedMode.FindPropertyRelative("OnAbilityIndex");
                    var DefaultIndex = SelectedMode.FindPropertyRelative("DefaultIndex");
                    var allowRotation = SelectedMode.FindPropertyRelative("allowRotation");
                    // var TotalAbilities = SelectedMode.FindPropertyRelative("TotalAbilities");
                    var ResetToDefault = SelectedMode.FindPropertyRelative("ResetToDefault");
                    var Abilities = SelectedMode.FindPropertyRelative("Abilities");
                    var modifier = SelectedMode.FindPropertyRelative("modifier");
                    //var AffectStates = SelectedMode.FindPropertyRelative("affectStates");
                    //var affect = SelectedMode.FindPropertyRelative("affect");
                    //var events = SelectedMode.FindPropertyRelative("events");


                    EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                    EditorGUILayout.PropertyField(Input);
                    EditorGUILayout.PropertyField(animationTag);
                    EditorGUILayout.PropertyField(CoolDown, G_CoolDown);
                    EditorGUILayout.PropertyField(allowRotation, new GUIContent("Allow Rotation","Allows rotate while is on the Mode"));
                    EditorGUILayout.PropertyField(modifier, G_Modifier);

                    //   if (InterruptTime.floatValue < 0) InterruptTime.floatValue = 0;

                    EditorGUILayout.EndVertical();

                    EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                    EditorGUI.indentLevel++;
                    EditorGUILayout.PropertyField(properties, true);
                    EditorGUI.indentLevel--;
                    EditorGUILayout.EndVertical();

                    EditorGUILayout.BeginVertical(EditorStyles.helpBox);

                    EditorGUI.indentLevel++;
                    ModeShowAbilities.boolValue = EditorGUILayout.Foldout(ModeShowAbilities.boolValue, "Abilities");
                    EditorGUI.indentLevel--;
                    if (ModeShowAbilities.boolValue)
                    {
                        //  string SetTo = string.Empty;

                        //IntReference ai = (IntReference) AbilityIndex.objectReferenceValue;

                        //if (ai != null)
                        //{
                        //    if (ai.Value == -1)
                        //    {
                        //        SetTo = "(Set to Random)";
                        //    }
                        //    else if (ai.Value == 0)
                        //    {
                        //        SetTo = "(Set to None)";
                        //    }
                        //}



                        EditorGUILayout.BeginVertical(EditorStyles.helpBox);

                        EditorGUILayout.PropertyField(AbilityIndex, G_AbilityIndex, true);
                        EditorGUILayout.PropertyField(DefaultIndex, G_DefaultIndex, true);
                        EditorGUILayout.PropertyField(ResetToDefault, G_ResetToDefault, true);
                        EditorGUILayout.PropertyField(OnAbilityIndex);


                        //EditorGUI.BeginDisabledGroup(true);
                        //EditorGUILayout.PropertyField(TotalAbilities, new GUIContent("Total", "If "), true);
                        //EditorGUI.EndDisabledGroup();

                        EditorGUILayout.EndVertical();

                        EditorGUI.indentLevel++;
                        EditorGUILayout.Space();
                        EditorGUILayout.PropertyField(Abilities, G_Abilities, true);
                        EditorGUI.indentLevel--;
                    }
                    EditorGUILayout.EndVertical();

                    Mode activeMode = m.modes[Index];

                    if (Application.isPlaying)
                    {
                        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                        {
                            EditorGUILayout.LabelField("Debug", EditorStyles.boldLabel);

                            EditorGUI.BeginDisabledGroup(true);
                            if (activeMode != null)
                            {
                                EditorGUILayout.Toggle("Playing Mode: ", activeMode.PlayingMode);
                                EditorGUILayout.Toggle("Input value", activeMode.InputValue);
                                EditorGUILayout.LabelField("Active Ability: " + (activeMode.ActiveAbility != null ? activeMode.ActiveAbility.Name : "Null"));
                            }
                        }
                        EditorGUI.EndDisabledGroup();
                        EditorGUILayout.EndVertical();
                    }
                }
            }
         //   EditorGUILayout.EndVertical();
        }

        private void ShowStates()
        {
            EditorGUI.indentLevel++;
            var showStates = serializedObject.FindProperty("showStates");
            

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.BeginHorizontal();
            showStates.boolValue = EditorGUILayout.Foldout(showStates.boolValue, "States");
            CloneStates.boolValue = GUILayout.Toggle(CloneStates.boolValue, G_CloneStates, EditorStyles.miniButton, GUILayout.Width(75));
            EditorGUILayout.EndHorizontal();
            EditorGUI.indentLevel--;

            if (showStates.boolValue)
            {
                EditorGUILayout.PropertyField(OverrideStartState,G_OverrideStartState);
                Reo_List_States.DoLayoutList();        //Paint the Reordable List
            }

            EditorGUILayout.EndVertical();

            var Index = Reo_List_States.index;

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(OnEnterExitStates,true);
            EditorGUI.indentLevel--;
            EditorGUILayout.EndVertical();
             
        }

        private void ShowSpeeds()
        {

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            Reo_List_Speeds.DoLayoutList();        //Paint the Reordable List PIVOTS

            if (Reo_List_Speeds.index != -1)
            {
                var SelectedSpeed = S_Speed_List.GetArrayElementAtIndex(Reo_List_Speeds.index);
                var states = SelectedSpeed.FindPropertyRelative("states");
                var StartVerticalSpeed = SelectedSpeed.FindPropertyRelative("StartVerticalIndex");
                var Speeds = SelectedSpeed.FindPropertyRelative("Speeds");
                var TopIndex = SelectedSpeed.FindPropertyRelative("TopIndex");
                EditorGUILayout.PropertyField(StartVerticalSpeed, new GUIContent("Start Index", "Which Speed the Set will start, This value is the Index for the Speed Modifier List, Starting the first index with (1) instead of (0)"));
                EditorGUILayout.PropertyField(TopIndex, new GUIContent("Top Index", "Set the Top Index when Increasing the Speed using SpeedUP"));
                EditorGUILayout.Space();
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(states, new GUIContent("States", "States that will activate these Speeds"), true);
                EditorGUILayout.PropertyField(Speeds, new GUIContent("Speeds", "Speeds for this speed Set"), true);
                EditorGUI.indentLevel--;    
            }
            EditorGUILayout.EndVertical();
        }

        private void ShowGeneral()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.PropertyField(Player, G_Player);
            EditorGUILayout.EndVertical();

            //-------------------------PIVOTS-------------------------------------------
            EditorGUI.indentLevel++;
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.BeginHorizontal();
            showPivots.boolValue = EditorGUILayout.Foldout(showPivots.boolValue, "Pivots");
            if (showPivots.boolValue)
            {
                EditorGUILayout.PropertyField(ShowpivotColor, GUIContent.none, GUILayout.MaxWidth(50));
            }
            EditorGUILayout.EndHorizontal();


            EditorGUI.indentLevel--;
            if (showPivots.boolValue)
            {
                Reo_List_Pivots.DoLayoutList();        //Paint the Reordable List PIVOTS
            }
            EditorGUILayout.EndVertical();


            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            {
                EditorGUILayout.PropertyField(hitLayer, new GUIContent("Hit Layer","What the Animal can hit using the Attack Triggers"));
                EditorGUILayout.PropertyField(triggerInteraction, new GUIContent("Hit Colliders","Does the Attack Triggers Interact with triggers?"));
            }
            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            {
                EditorGUILayout.LabelField("Ground", EditorStyles.boldLabel);

                EditorGUILayout.BeginHorizontal();
                {
                    EditorGUILayout.PropertyField(Height, G_Height);
                    if (GUILayout.Button(G_Calculate_H, EditorStyles.miniButton, GUILayout.Width(18)))
                    {
                        if (!CalculateHeight())
                        {
                            EditorGUILayout.HelpBox("No pivots found, please add at least one Pivot (CHEST or HIP)", MessageType.Warning);
                        }
                    }
                }
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.PropertyField(GroundLayer, G_GroundLayer);
                EditorGUILayout.PropertyField(maxAngleSlope, G_maxAngleSlope);
                EditorGUILayout.PropertyField(AlignPosLerp, G_AlignPosLerp);
                EditorGUILayout.PropertyField(AlignRotLerp, G_AlignRotLerp);
            }
            EditorGUILayout.EndVertical();


            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            {
                EditorGUILayout.LabelField("Gravity", EditorStyles.boldLabel);
                EditorGUILayout.PropertyField(gravityDirection, G_gravityDirection);
                EditorGUILayout.PropertyField(GravityForce, G_GravityForce);
                EditorGUILayout.PropertyField(GravityMultiplier, G_GravityMultiplier);
            }
            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            {
                EditorGUILayout.LabelField("Movement", EditorStyles.boldLabel);
                EditorGUILayout.PropertyField(UseCameraInput, new GUIContent("Camera Input", "The Animal uses the Camera Forward Diretion to Move"));
                EditorGUILayout.PropertyField(useCameraUp, new GUIContent("Use Camera Up", "Uses the Camera Up Vector to move UP or Down while flying or Swiming UnderWater. if this is false the Animal will need an UPDOWN Input to move higher or lower"));
                EditorGUILayout.PropertyField(useSprintGlobal, G_useSprintGlobal);
                EditorGUILayout.PropertyField(SmoothVertical, G_SmoothVertical);
                EditorGUILayout.PropertyField(TurnMultiplier, G_TurnMultiplier);
                EditorGUILayout.PropertyField(UpDownLerp, G_UpDownLerp);
            }
            EditorGUILayout.EndVertical();
             
        }

        private void SetStatesNamesArray_New()
        {
            // Store the States type.
            Type stateType = typeof(State);

            // Get all the types that are in the same Assembly (all the runtime scripts) as the Reaction type.
            Type[] allTypes = stateType.Assembly.GetTypes();

            // Create an empty list to store all the types that are subtypes of Reaction.
            List<Type> StatesSubTypeList = new List<Type>();

            // Go through all the types in the Assembly...
            for (int i = 0; i < allTypes.Length; i++)
            {
                // ... and if they are a non-abstract subclass of Reaction then add them to the list.
                if (allTypes[i].IsSubclassOf(stateType) && !allTypes[i].IsAbstract)
                {
                    StatesSubTypeList.Add(allTypes[i]);
                }
            }

            // Convert the list to an array and store it.
            StatesType = StatesSubTypeList;

            // Create an empty list of strings to store the names of the Reaction types.
            List<string> reactionTypeNameList = new List<string>();

            // Go through all the State types and add their names to the list.
            for (int i = 0; i < StatesType.Count; i++)
            {
                reactionTypeNameList.Add(StatesType[i].Name);
            }

            //// Convert the list to an array and store it.
            //StateTypeNames = reactionTypeNameList.ToArray();
        }

        private void Draw_Header_Speed(Rect rect)
        {
            var height = EditorGUIUtility.singleLineHeight;
            var name = new Rect(rect.x, rect.y, rect.width / 2, height);

            EditorGUI.LabelField(name, "   Speed Sets");

            Rect R_2 = new Rect(rect.width - 7, rect.y + 2, 18, EditorGUIUtility.singleLineHeight - 3);
            Rect R_3 = new Rect(rect.width + 13, rect.y + 2, 18, EditorGUIUtility.singleLineHeight - 3);


            if (GUI.Button(R_2, new GUIContent("+", "New Speed Set"), EditorStyles.miniButton))
            {
                OnAddCallback_Speeds(Reo_List_Speeds);
            }

            if (GUI.Button(R_3, new GUIContent("-", "Remove Selected Speed Set"), EditorStyles.miniButton))
            {
                if (Reo_List_Speeds.index != -1) //If there's a selected Ability
                {
                    OnRemoveCallback_Speeds(Reo_List_Speeds);
                }
            }
        }

        private void OnRemoveCallback_Speeds(ReorderableList list)
        {
            S_Speed_List.DeleteArrayElementAtIndex(list.index);
            list.index -= 1;

            if (list.index == -1 && S_Speed_List.arraySize > 0)  //In Case you remove the first one
            {
                list.index = 0;
            }

            EditorUtility.SetDirty(m);
        }

        private void OnAddCallback_Speeds(ReorderableList reo_List_Speeds)
        {
            if (m.speedSets == null) m.speedSets = new List<MSpeedSet>();

            m.speedSets.Add(new MSpeedSet());

            EditorUtility.SetDirty(m);
        }
       

        private void Draw_Element_Speed(Rect rect, int index, bool isActive, bool isFocused)
        {
            if (S_Speed_List.arraySize <= index) return;

            var nameRect = rect;

            nameRect.y += 1;
            nameRect.height -= 3;
            Rect activeRect = nameRect;

            var speedSet = S_Speed_List.GetArrayElementAtIndex(index);
            var nameSpeedSet = speedSet.FindPropertyRelative("name");
            nameRect.width /= 2;
            nameRect.width += 10;
            EditorGUI.PropertyField(nameRect, nameSpeedSet, GUIContent.none);

            activeRect.x = rect.width / 2 + 60;
            activeRect.width = rect.width / 2 - 20;

            //    EditorGUI.TextField(activeRect, "(Active)");

            if (Application.isPlaying)
            {
                if (m.speedSets[index] == m.CurrentSpeedSet)
                {
                    EditorGUI.LabelField(activeRect, "(" + m.CurrentSpeedModifier.name + ")" /*"(Active)"*/, EditorStyles.boldLabel);
                }
            }
        }


        #region Draw Pivots
        //-------------------------PIVOTS-----------------------------------------------------------
        private void DrawHeaderCallback_Pivots(Rect rect)
        {
            var height = EditorGUIUtility.singleLineHeight;
            var name = new Rect(rect.x, rect.y, rect.width / 2 - 60, height);
            var vector = new Rect(rect.width / 2 - 45, rect.y, rect.width / 2, height);

            EditorGUI.LabelField(name, "   Name");
            EditorGUI.LabelField(vector, "    Position");

            Rect R_2 = new Rect(rect.width -7, rect.y + 2, 18, EditorGUIUtility.singleLineHeight - 3);
            Rect R_3 = new Rect(rect.width + 13, rect.y + 2, 18, EditorGUIUtility.singleLineHeight - 3);


            if (GUI.Button(R_2, new GUIContent("+", "Add a new Pivot"), EditorStyles.miniButton))
            {
                OnAddCallback_Pivots(Reo_List_Pivots);
            }

            if (GUI.Button(R_3, new GUIContent("-", "Remove the selected Pivot"), EditorStyles.miniButton))
            {
                if (Reo_List_Pivots.index != -1) //If there's a selected Ability
                {
                    OnRemoveCallback_Pivots(Reo_List_Pivots);
                }
            }
        }
        private void DrawElement_Pivots(Rect rect, int index, bool isActive, bool isFocused)
        {
            rect.y += 2;
            if (S_PivotsList.arraySize <= index) return;

            var pivot = S_PivotsList.GetArrayElementAtIndex(index);
            rect.width += 30;
            EditorGUI.PropertyField(rect, pivot);
        }
        private void OnAddCallback_Pivots(ReorderableList list)
        {
            if (m.pivots == null) m.pivots = new List<MPivots>();

            m.pivots.Add(new MPivots("Pivot", Vector3.up, 1));

            EditorUtility.SetDirty(m);
        }
        private void OnRemoveCallback_Pivots(ReorderableList list)
        {
           // var state = S_PivotsList.GetArrayElementAtIndex(list.index);

            // The reference value must be null in order for the element to be removed from the SerializedProperty array.
            S_PivotsList.DeleteArrayElementAtIndex(list.index);
            list.index -= 1;

            if (list.index == -1 && S_PivotsList.arraySize > 0)  //In Case you remove the first one
            {
                list.index = 0;
            }

            EditorUtility.SetDirty(m);
        }
        //-------------------------PIVOTS-----------------------------------------------------------
        #endregion

        #region DrawStates 
        //-------------------------STATES-----------------------------------------------------------
        private void Draw_Header_State(Rect rect)
        {
            var r = rect;
            r.x += 13;
            EditorGUI.LabelField(r, new GUIContent("     States [Double clic to modify them]", "States are the common things the Animals can do but they cannot overlap each other"));

            Rect R_2 = new Rect(rect.width - 7, rect.y + 2, 18, EditorGUIUtility.singleLineHeight - 3);
            Rect R_3 = new Rect(rect.width + 13, rect.y + 2, 18, EditorGUIUtility.singleLineHeight - 3);


            if (GUI.Button(R_2, new GUIContent("+", "Add a new State"), EditorStyles.miniButton))
            {
                OnAddCallback_State(Reo_List_States);
            }

            if (GUI.Button(R_3, new GUIContent("-", "Remove the selected State"), EditorStyles.miniButton))
            {
                if (Reo_List_States.index != -1)
                {
                    if (EditorUtility.DisplayDialog("Warning!", "Are you sure you want to delete from the Project the selected state?\n If you choose 'No' it will simply remove it from the list ", "Yes", "No Just Remove it"))
                    {
                        OnRemoveCallback_State(Reo_List_States, true);       //If there's a selected Ability
                    }
                    else
                    {
                        OnRemoveCallback_State(Reo_List_States, false);       //If there's a selected Ability
                    }
                }
            }
        }
        private void Draw_Element_State(Rect rect, int index, bool isActive, bool isFocused)
        {
            rect.y += 2;
            if (S_StateList.arraySize <= index) return;

            var stateProperty = S_StateList.GetArrayElementAtIndex(index);
            State state = stateProperty.objectReferenceValue as State;


            // Remove the ability if it no longer exists.
            if (ReferenceEquals(state, null))
            {
                S_StateList.DeleteArrayElementAtIndex(index);
               // Reo_List_States.index = m.SelectedState = -1;
                S_StateList.serializedObject.ApplyModifiedProperties();
                EditorUtility.SetDirty(m);

                // It's not easy removing a null component.
                var components = m.GetComponents<Component>();
                for (int i = components.Length - 1; i > -1; --i)
                {
                    if (ReferenceEquals(components[i], null))
                    {
                        var serializedObject = new SerializedObject(m.gameObject);
                        var componentProperty = serializedObject.FindProperty("m_Component");
                        componentProperty.DeleteArrayElementAtIndex(i);
                        serializedObject.ApplyModifiedProperties();
                    }
                }
                return;
            }
            if (state.hideFlags != HideFlags.HideInInspector) state.hideFlags = HideFlags.HideInInspector; //Hide the Script on the Inspector


            var activeRect = rect;
            activeRect.width -= 20;
            activeRect.x += 20;
            var label = state.GetType().Name;

            state.Active = EditorGUI.Toggle(new Rect(rect.x, rect.y, 20, activeRect.height), GUIContent.none, state.Active);


            var active = "";

            if (Application.isPlaying)
            {
                if (m.ActiveState == state)
                {
                    if (state.IsPending)
                        active = " [Pending] ";
                    else
                        active = " [Active] ";
                }
                else if (state.Sleep)
                {
                    active = " [Sleep] ";
                }
                else if (state.OnQueue)
                {
                    active = " [Queued] ";
                }

            }

           // EditorGUI.LabelField(activeRect,label + active);

            EditorGUI.ObjectField(new Rect(activeRect.x , activeRect.y, activeRect.width-78, activeRect.height-5), stateProperty, GUIContent.none);


            EditorGUI.LabelField(new Rect(activeRect.width -20  ,activeRect.y,60,activeRect.height), active);
            EditorGUI.LabelField(new Rect(activeRect.width +40 ,activeRect.y,25,activeRect.height),  "(" + (S_StateList.arraySize - index-1) + ")");

            EditorGUI.BeginChangeCheck();
            activeRect = rect;
            activeRect.x += activeRect.width - 34;
            activeRect.width = 20;

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(target, "MAnimal Inspector Changed");
               // EditorUtility.SetDirty(target);
            }
        }
        //private void Draw_State(State state)
        //{
        //    if (state == null) return;

        //    SerializedObject State_Serialized;

        //    State_Serialized = new SerializedObject(state);

        //    State_Serialized.Update();
        //    EditorGUILayout.BeginVertical(EditorStyles.helpBox);

        //    EditorGUI.BeginChangeCheck();

        //    var property = State_Serialized.GetIterator();
            
        //    //property.NextVisible(true);
        //    property.NextVisible(true);
        //    do
        //    {
        //        if (property.hasVisibleChildren) EditorGUI.indentLevel++;
        //        EditorGUILayout.PropertyField(property, true);
        //        if (property.hasVisibleChildren) EditorGUI.indentLevel--;

        //    }
        //    while (property.NextVisible(false));

        // //   EditorGUI.indentLevel--;

        //    EditorGUILayout.EndVertical();
        //    if (EditorGUI.EndChangeCheck())
        //    {
        //        Undo.RecordObject(state, "Animal Inspector Changed");
        //        State_Serialized.ApplyModifiedProperties();
        //        EditorUtility.SetDirty(state);
        //    }
        //}
        private void OnReorderCallback_States(ReorderableList list)
        {
            for (int i = 0; i < S_StateList.arraySize; ++i)
            {
                m.states[i].Index = i;
                EditorUtility.SetDirty(m);
            }
            EditorUtility.SetDirty(target);
        }
        private void OnAddCallback_State(ReorderableList list)
        {
            addMenu = new GenericMenu();

            for (int i = 0; i < StatesType.Count; i++)
            {
                Type st = StatesType[i];

                bool founded = false;
                for (int j = 0; j < m.states.Count; j++)
                {
                    if (m.states[j].GetType() == st)
                    {
                        founded = true;
                    }
                }

                if (!founded)
                {
                    addMenu.AddItem(new GUIContent(st.Name), false, () => AddState(st));
                }
            }

            addMenu.ShowAsContext();
        }
        #endregion

        //-------------------------MODES-----------------------------------------------------------
        private void Draw_Header_Modes(Rect rect)
        {
            var r = rect;
            r.x += 13;
            EditorGUI.LabelField(r, new GUIContent("     Modes", "Modes are the Animations that can be played on top of the States"));

            Rect R_2 = new Rect(rect.width - 7, rect.y + 2, 18, EditorGUIUtility.singleLineHeight - 3);
            Rect R_3 = new Rect(rect.width + 13, rect.y + 2, 18, EditorGUIUtility.singleLineHeight - 3);

            var activeRect = rect;
            activeRect.width -= 20;
            activeRect.x += 20;
            EditorGUI.LabelField(new Rect(activeRect.width-20, activeRect.y, 25, activeRect.height), new GUIContent("ID "));


            if (GUI.Button(R_2, new GUIContent("+", "Add a new Mode"), EditorStyles.miniButton))
            {
                OnAddCallback_Modes(Reo_List_Modes);
            }

            if (GUI.Button(R_3, new GUIContent("-", "Remove the selected Mode"), EditorStyles.miniButton))
            {
                if (Reo_List_Modes.index != -1) OnRemoveCallback_Mode(Reo_List_Modes);       //If there's a selected Ability
            }
        }
        private void Draw_Element_Modes(Rect rect, int index, bool isActive, bool isFocused)
        {
            rect.y += 2;
            if (S_StateList.arraySize <= index) return;

            var ModeProperty = S_Mode_List.GetArrayElementAtIndex(index);
            var active = ModeProperty.FindPropertyRelative("active");
          //  var ModeName = ModeProperty.FindPropertyRelative("Name");
            var ID = ModeProperty.FindPropertyRelative("ID");

            var activeRect = rect;
            activeRect.width -= 20;
            activeRect.x += 20;

            var activeRect1 = new Rect(rect.x, rect.y, 20, rect.height);

            //var nameRect = new Rect(rect.x+20, rect.y, rect.width / 2 - 20 , EditorGUIUtility.singleLineHeight);
            var IDRect = new Rect(rect.x+40, rect.y, rect.width - 40 , EditorGUIUtility.singleLineHeight);
           // var IDRect = new Rect(rect.width / 2 + 65 , rect.y, rect.width / 2 -22 , EditorGUIUtility.singleLineHeight);

            active.boolValue = EditorGUI.Toggle(activeRect1, GUIContent.none, active.boolValue);
            //EditorGUI.PropertyField(nameRect, ModeName, GUIContent.none);
            EditorGUI.PropertyField(IDRect, ID, GUIContent.none);

           // EditorGUI.PropertyField(new Rect(activeRect.x, activeRect.y, activeRect.width - 78, activeRect.height - 5), ModeProperty, GUIContent.none);



            EditorGUI.BeginChangeCheck();
            activeRect = rect;
            activeRect.x += activeRect.width - 34;
            activeRect.width = 20;

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(target, "Inspector");
                EditorUtility.SetDirty(target);
            }
        }
        private void OnAddCallback_Modes(ReorderableList list)
        {
            if (m.pivots == null) m.modes = new List<Mode>();

            m.modes.Add(new Mode());

            EditorUtility.SetDirty(m);
        }
        private void OnRemoveCallback_Mode(ReorderableList list)
        {
            // The reference value must be null in order for the element to be removed from the SerializedProperty array.
            S_Mode_List.DeleteArrayElementAtIndex(list.index);
            list.index -= 1;

            if (list.index == -1 && S_Mode_List.arraySize > 0)  //In Case you remove the first one
            {
                list.index = 0;
            }

            list.index = Mathf.Clamp(list.index, 0, list.index - 1);


            EditorUtility.SetDirty(m);
        }


        public void SetPivots()
        {
            m.Pivot_Hip = m.pivots.Find(item => item.name.ToUpper() == "HIP");
            m.Pivot_Chest = m.pivots.Find(item => item.name.ToUpper() == "CHEST");
        }

        private bool CalculateHeight()
        {
            SetPivots();
            MPivots pivot = m.Pivot_Hip;
            if (pivot == null) return false;


            Ray newHeight = new Ray()
            {
                origin = pivot.World(m.transform),
                direction = -Vector3.up * 5
            };

            RaycastHit hit;
            if (Physics.Raycast(newHeight, out hit, pivot.multiplier * m.transform.lossyScale.y, m.GroundLayer))
            {
                m.height = hit.distance;
                serializedObject.ApplyModifiedProperties();
            }
            return false;
        }


        /// <summary> The ReordableList remove button has been pressed. Remove the selected ability.</summary>
        private void OnRemoveCallback_State(ReorderableList list, bool DeleteAsset)
        {

            if (DeleteAsset)
            {
                State state = S_StateList.GetArrayElementAtIndex(list.index).objectReferenceValue as State;
                string Path = AssetDatabase.GetAssetPath(state);
                AssetDatabase.DeleteAsset(Path);
            }
            else
            {
                S_StateList.DeleteArrayElementAtIndex(list.index);
                list.index -= 1;
            }

            list.index = Mathf.Clamp(list.index,0,list.index - 1);



            EditorUtility.SetDirty(m);
        }

        /// <summary>Adds a new State of the specified type.</summary>       
        private void AddState(Type selectedState)
        {
            Debug.Log(selectedState);

            State state = (State)CreateInstance(selectedState);

            AssetDatabase.CreateAsset(state, "Assets/" + m.name + " " + selectedState.Name + ".asset");

            EditorUtility.SetDirty(m);

           // state.SetAnimal(m);
            EditorUtility.SetDirty(state);
            S_StateList.AddToObjectArray(state);
        }


        #region GUICONTENT

        //readonly GUIContent G_mainCamera = new GUIContent("Main Camera", "Stores the Camera.Main transform to use the Camera Direction for Input");
        readonly GUIContent G_LockInput = new GUIContent("Lock Input", "Locks Input on the Animal, Ingore inputs like Jumps, Attacks , Actions etc");
        readonly GUIContent G_lockMovement = new GUIContent("Lock Movement", "Locks the Movement Entries on the Animal (Horizontal, Vertical)");
        readonly GUIContent G_Rotator = new GUIContent("Rotator", "Used to add extra Rotations to the Animal");
        readonly GUIContent G_RayCastRadius = new GUIContent("RayCast Radius", "Instead of using Raycast for checking the ground beneath the animal we use SphereCast, this is the Radius of that Sphere");
        readonly GUIContent G_animalType = new GUIContent("Type", "Modifier for Additive Pose Fixing");
        //readonly GUIContent G_AnimatorUpdatePhysics = new GUIContent("Update Physics?", "if True the it will use FixedUpdate for all his calculations. Use this if you are using the creature as the Main Character");
        //readonly GUIContent G_UpdateParameters = new GUIContent("Update Params", "Update all Parameters in the Animator Controller");
        readonly GUIContent G_AnimatorSpeed = new GUIContent("Animator Speed", "Global multiplier for the Animator Speed");
        readonly GUIContent G_AbilityIndex = new GUIContent("Active Index", "Active Ability Index \n(if set to -1 it will Play a Random Ability )\n(if set to 0 it wont do anything)");
        readonly GUIContent G_DefaultIndex = new GUIContent("Default Index", "Default Ability Index to return to when exiting the mode");
        readonly GUIContent G_ResetToDefault = new GUIContent("Reset to Default", "When Exiting the mode the Active Index will reset to the Default ");
        readonly GUIContent G_Abilities = new GUIContent("Abilities", "All the abilities inluded in this Mode");
        readonly GUIContent G_CloneStates = new GUIContent("Clone States", "Creates instances of the States so they cannot be overwriten by other animal using the same scriptable objects");
        readonly GUIContent G_Height = new GUIContent("Height", "Distance from Animal Hip to the ground");
        readonly GUIContent G_Calculate_H = new GUIContent("C", "Calculate the Height of the Animal, the Chest or Hip Pivot must be setted");
        readonly GUIContent G_GroundLayer = new GUIContent("Ground Layer", "Layers the Animal considers ground");
        readonly GUIContent G_maxAngleSlope = new GUIContent("Max Angle Slope", "If the Terrain slope angle is greater than this value, the animal will fall");
        readonly GUIContent G_AlignPosLerp = new GUIContent("Align Pos Lerp", "Smoothness value to Snap to ground while Grounded");
        readonly GUIContent G_AlignRotLerp = new GUIContent("Align Rot Lerp", "Smoothness value to Aling to ground slopes while Grounded");
        readonly GUIContent G_CoolDown = new GUIContent("Cool Down", "Elapsed time to be able to play the Mode Again.\n If = 0 then the Mode cannot be interrupted until it finish the Animation");
        readonly GUIContent G_Modifier = new GUIContent("Modifier", "Extra Logic to give the Animal when Entering or Exiting the Modes");

        readonly GUIContent G_gravityDirection = new GUIContent("Direction", "Direction of the Gravity");
        readonly GUIContent G_GravityForce = new GUIContent("Force", "How Fast the Animal will fall to the ground ");
        readonly GUIContent G_GravityMultiplier = new GUIContent("Multiplier", "Gravity acceleration multiplier");

        readonly GUIContent G_useSprintGlobal = new GUIContent("Use Sprint", "Can the Animal Sprint?");
        readonly GUIContent G_SmoothVertical = new GUIContent("Smooth Vertical", "Used for Joysticks to increase the speed by the Stick Pressure");
        readonly GUIContent G_TurnMultiplier = new GUIContent("Turn Multiplier", "Global turn multiplier to increase rotation on the animal");
        readonly GUIContent G_UpDownLerp = new GUIContent("Up Down Lerp", "Lerp Value for the UpDown Axis");
        readonly GUIContent G_rootMotion = new GUIContent("Root Motion", "Enable Disable the Root motion on the Animator");

        readonly GUIContent G_Player = new GUIContent("Player", "True if this will be your main Character Player, used for Respawing characters");
        readonly GUIContent G_OverrideStartState = new GUIContent("Override Start State", "Overrides the Start State");
        #endregion

        //-------------------------STATES-----------------------------------------------------------
        void OnSceneGUI()
        {
            foreach (var pivot in m.pivots)
            {
                if (pivot.EditorModify)
                {
                    Transform t = m.transform;
                    EditorGUI.BeginChangeCheck();

                    Vector3 piv = t.TransformPoint(pivot.position);
                    Vector3 NewPivPosition = Handles.PositionHandle(piv, t.rotation);
                    //   pivot.position = m.transform.InverseTransformPoint(NewPivPosition);

                    float multiplier = Handles.ScaleSlider(pivot.multiplier, piv, -t.up, Quaternion.identity, HandleUtility.GetHandleSize(piv), 0f);

                    if (EditorGUI.EndChangeCheck())
                    {
                        Undo.RecordObject(m, "Pivots");
                        pivot.position = t.InverseTransformPoint(NewPivPosition);
                        pivot.multiplier = multiplier;
                    }
                }
            }
        }
    }
}