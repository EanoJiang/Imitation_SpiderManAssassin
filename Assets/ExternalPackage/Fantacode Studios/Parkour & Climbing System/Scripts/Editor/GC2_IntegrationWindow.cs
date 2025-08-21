#if gameCreator2
using GameCreator.Runtime.Characters;
#endif
using System.Collections.Generic;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Animations;
#endif
using UnityEngine;

namespace FS_ParkourSystem
{
#if UNITY_EDITOR
    public class GC2_IntegrationWindow : EditorWindow
    {
        public static GC2_IntegrationWindow window;

        public AnimatorController controller;
        bool integrationButtonClicked;
        //bool needBalanceOnNarrowBeam;


        [MenuItem("Tools/Parkour && Climbing System/Integration/GameCreator2/Documentation", false, 600)]
        public static void GoToIntegrationDocumentation()
        {
            Application.OpenURL("https://fantacode.gitbook.io/parkour-and-climbing-system/gamecreator2-integration");
        }

#if gameCreator2

        public Character character;
        [MenuItem("Tools/Parkour && Climbing System/Integration/GameCreator2/Helper", false, 600)]
        public static void InItWindow()
        {

            window = GetWindow<GC2_IntegrationWindow>();
            window.titleContent = new GUIContent("Integration");
            SetWindowHeight(82);

        }
        private void OnGUI()
        {
            GetWindow();
            GUILayout.Space(15);
            if (controller != null)
                SetWindowHeight(82);
            else if (integrationButtonClicked)
            {
                if (character == null)
                    EditorGUILayout.HelpBox("Character is not assigned", MessageType.Error);
                else if (controller == null)
                    EditorGUILayout.HelpBox("Animator Controller is not assigned", MessageType.Error);

                SetWindowHeight(122);
            }
            character = (Character)UndoField(character, EditorGUILayout.ObjectField("Player Template", character, typeof(Character), true));
            GUILayout.Space(1.5f);
            controller = (AnimatorController)UndoField(controller, EditorGUILayout.ObjectField("Animator Controller", controller, typeof(AnimatorController), true));
            GUILayout.Space(1.5f);

            //needBalanceOnNarrowBeam = (bool)UndoField(needBalanceOnNarrowBeam, EditorGUILayout.Toggle(new GUIContent("Balance On Narrowbeam", "Enable this option to perform balancing animations on narrow beams with your controller, providing idle, walk, and run animations."), needBalanceOnNarrowBeam));
            //GUILayout.Space(1.5f);

            if (GUILayout.Button("Integrate"))
            {//Enable this option , if you want balance on narrow beam featur for your controoller. it will playing balance idle,walk and run animations on narrowbeams
                //GameObject p = new GameObject("sample", typeof(ParkourController));
                //MonoScript script = MonoScript.FromMonoBehaviour(p.GetComponent<ParkourController>());
                //if (script != null)
                //{
                //    MonoImporter.SetExecutionOrder(script, -115);
                //}

                if (character != null)
                    GC2Integration();
                else
                    integrationButtonClicked = true;
            }
        }

        void GC2Integration()
        {
            GenerateAnimationParameters();
            GenerateTransitions();
            AttachScripts();
        }
        void AttachScripts()
        {
            //Checks if the parkour controller already exists

            var parkourAnimator = character.Kernel.Animim.Mannequin.GetComponent<Animator>();
            if (parkourAnimator != null)
                GameObject.DestroyImmediate(parkourAnimator);
            var gc2Animator = character.GetComponentInChildren<Animator>();
            parkourAnimator = (Animator)Undo.AddComponent(character.Kernel.Animim.Mannequin.gameObject, gc2Animator.GetType());
            EditorUtility.CopySerialized(gc2Animator, parkourAnimator);
            var helperScript = character.GetComponentInChildren<GC2_IntegrationHelper>();
            if (helperScript != null)
                GameObject.DestroyImmediate(helperScript.gameObject);
            var obj = (GameObject)Resources.Load("GC2 Parkour Controller");
            var parkourController = Instantiate(obj, parkourAnimator.transform);
            parkourController.name = obj.name;
            parkourController.transform.localPosition = Vector3.zero;
            helperScript = parkourController.GetComponent<GC2_IntegrationHelper>();

            parkourAnimator.enabled = false;
            helperScript.character = character;
            helperScript.parkourAnimator = parkourAnimator;

        }
        void GenerateTransitions()
        {


            if (controller == null)
            {
                Debug.LogWarning("Animator Controller not assigned");
                return;
            }
            var rootStateMachine = controller.layers[0].stateMachine;

            List<AnimatorState> states = new List<AnimatorState>();


            var parkourSMNames = new List<string>() { "Climb Actions", "Jump Actions", "Parkour Actions"};

            var parkourStateMachines = rootStateMachine.stateMachines.
                Where(s => parkourSMNames.Contains(s.stateMachine.name)).
                ToDictionary(s => s.stateMachine.name, s => s.stateMachine);

            var locomotionBT = rootStateMachine.states.FirstOrDefault(s => s.state.name == "Locomotion").state;

            var statesToCreateTransition = new[]
            {
                new
                {
                    StateMachine = "Parkour Actions",
                    States = new Dictionary<string,float>{{ "VaultOver", .95f}, { "VaultOn", .95f }, { "MediumStepUp", .95f }, { "Climb Up", 1 }, { "StepUp", .95f }, { "MediumStepUpM", .95f } }

                },
                new
                {
                    StateMachine = "Jump Actions",
                    States = new Dictionary<string,float> {{ "LandFromFall", 1f}, { "LandOnSpot", 0 }, { "FallingToRoll", 0 } }
                },
                new
                {
                    StateMachine = "Climb Actions",
                    States = new Dictionary<string,float> {{ "FreeHangClimb", 1}, { "BracedHangClimb", 1 } }
                }
            };
            foreach (var stateTransition in statesToCreateTransition)
            {
                var sm = parkourStateMachines[stateTransition.StateMachine];
                foreach (var state in sm.states.Where(s => stateTransition.States.ContainsKey(s.state.name)).Select(s => s.state))
                {
                    foreach (var transition in state.transitions)
                        state.RemoveTransition(transition);
                    var t = state.AddTransition(locomotionBT, true);
                    var exitTime = stateTransition.States.GetValueOrDefault(state.name);
                    if (exitTime > 0)
                        t.exitTime = exitTime;
                }
            }
            var stateM = controller.layers[0].stateMachine;
            var bt = stateM.states.Where(s => s.state.name == "LandAndStepForward").First().state;
            (bt.motion as BlendTree).blendParameter = "Movement";
            bt.AddTransition(locomotionBT, true);
        }
        void GenerateAnimationParameters()
        {
            if (controller == null)
            {
                Debug.LogWarning("Animator Controller not assigned");
                return;
            }

            var currParams = controller.parameters.ToDictionary(p => p.name);

            var paramsToAdd = new Dictionary<string, AnimatorControllerParameterType>()
            {
                { "moveAmount", AnimatorControllerParameterType.Float },
                { "IsGrounded", AnimatorControllerParameterType.Bool },
                { "mirrorAction", AnimatorControllerParameterType.Bool },
                { "freeHang", AnimatorControllerParameterType.Float },
                { "x", AnimatorControllerParameterType.Float },
                { "y", AnimatorControllerParameterType.Float },
                { "isFalling", AnimatorControllerParameterType.Bool },
                { "landingType", AnimatorControllerParameterType.Int },
                { "jumpInputPressed", AnimatorControllerParameterType.Bool },
                { "mirrorJump", AnimatorControllerParameterType.Bool },
                { "rotation", AnimatorControllerParameterType.Float },
                { "idleType", AnimatorControllerParameterType.Float },
                { "leftFootIK", AnimatorControllerParameterType.Float },
                { "rightFootIK", AnimatorControllerParameterType.Float },
                { "crouchType", AnimatorControllerParameterType.Float },
                { "jumpBackDirection", AnimatorControllerParameterType.Float },
                { "BackJumpMode", AnimatorControllerParameterType.Bool },
                { "BackJumpDir", AnimatorControllerParameterType.Float },
                { "fallAmount", AnimatorControllerParameterType.Float }
            };

            foreach (var p in paramsToAdd)
            {
                if (!currParams.ContainsKey(p.Key))
                    controller.AddParameter(p.Key, p.Value);
            }
            
        }

        void ChangeBlendTreeParameter(string blendTreeName, string newParameterName)
        {
            
        }
        void GetWindow()
        {
            if (window == null)
            {
                window = GetWindow<GC2_IntegrationWindow>();
                window.titleContent = new GUIContent("Integration");
                SetWindowHeight(82);
            }
        }
        static void SetWindowHeight(float height)
        {
            window.minSize = new Vector2(400, height);
            window.maxSize = new Vector2(400, height);
        }
        object UndoField(object oldValue, object newValue)
        {
            if (newValue != null && oldValue != null && newValue.ToString() != oldValue.ToString())
            {
                Undo.RegisterCompleteObjectUndo(this, "Update Field");
            }
            return newValue;
        }
#endif
    }
#endif
}