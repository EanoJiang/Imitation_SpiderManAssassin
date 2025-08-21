using FS_ThirdPerson;
using System.Collections;
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
    public class Invector_IntegrationWindow : EditorWindow
    {
        public static Invector_IntegrationWindow window;

        public AnimatorController controller;
        public GameObject playerTemplate;
        bool integrationButtonClicked;

        [MenuItem("Tools/Parkour && Climbing System/Integration/Invector/Documentation", false, 600)]
        public static void GoToIntegrationDocumentation()
        {
            Application.OpenURL("https://fantacode.gitbook.io/parkour-and-climbing-system/invector-integration");
        }

#if invector
        [MenuItem("Tools/Parkour && Climbing System/Integration/Invector/Helper", false, 600)]
        public static void InItWindow()
        {

            window = GetWindow<Invector_IntegrationWindow>();
            window.titleContent = new GUIContent("Integration");
            SetWindowHeight(82);
            
        }
        private void OnGUI()
        {
            GetWindow();
            GUILayout.Space(15);
            if (controller != null && playerTemplate != null)
                SetWindowHeight(82);
            else if (integrationButtonClicked)
            {
                if (controller == null && playerTemplate == null)
                    EditorGUILayout.HelpBox("Fields are empty", MessageType.Error);
                else if (controller == null)
                    EditorGUILayout.HelpBox("Animator Controller is not assigned", MessageType.Error);
                else if (playerTemplate == null)
                    EditorGUILayout.HelpBox("Player Template is not assigned", MessageType.Error);
                SetWindowHeight(122);
            }
            playerTemplate = (GameObject)UndoField(playerTemplate, EditorGUILayout.ObjectField("Player Template", playerTemplate, typeof(GameObject), true));
            GUILayout.Space(1.5f);
            controller = (AnimatorController)UndoField(controller, EditorGUILayout.ObjectField("Animator Controller", controller, typeof(AnimatorController), true));
            GUILayout.Space(1.5f);
            if (GUILayout.Button("Integrate"))
            {
                if (playerTemplate != null)
                    InvectorIntegration();
                else
                    integrationButtonClicked = true;
            }
        }
        void InvectorIntegration()
        {
            GenerateAnimationParameters();
            GenerateTransitions();
            AttachScripts();
        }
        void AttachScripts()
        {
            //Checks if the parkour controller already exists
            //var p = GameObject.Find("Parkour Controller");
            //if (p != null && p.GetComponent<ParkourController>() != null && p.GetComponent<InvectorIntegrationHelper>())
            //    return;
            
            var pc = playerTemplate.GetComponent<ParkourController>();
            if (pc == null)
            {
                pc = playerTemplate.AddComponent<ParkourController>();
                var actions = Resources.LoadAll("Parkour Actions", typeof(ParkourAction)).ToList();
                foreach (var a in actions)
                    pc.parkourActions.Add(a as ParkourAction);
                var cc = playerTemplate.GetComponent<ClimbController>();
                if (cc == null)
                    cc = playerTemplate.AddComponent<ClimbController>();
                if (playerTemplate.GetComponent<PlayerController>() == null)
                {
                    var playerController = playerTemplate.AddComponent<PlayerController>();
                    playerController.managedScripts.Add(cc);
                    playerController.managedScripts.Add(pc);
                }
            }
            
            if (playerTemplate.GetComponent<EnvironmentScanner>() == null)
                playerTemplate.AddComponent<EnvironmentScanner>();
            if (playerTemplate.GetComponent<ParkourInputManager>() == null)
                playerTemplate.AddComponent<ParkourInputManager>();
            if (playerTemplate.GetComponent<LocomotionInputManager>() == null)
                playerTemplate.AddComponent<LocomotionInputManager>();
            if (playerTemplate.GetComponent<InvectorIntegrationHelper>() == null)
                playerTemplate.AddComponent<InvectorIntegrationHelper>();
           
        }
        void GenerateTransitions()
        {
            if (controller == null)
            {
                Debug.LogError("Animator Controller not assigned");
                return;
            }
            var rootStateMachine = controller.layers[0].stateMachine;

            List<AnimatorState> states = new List<AnimatorState>();


            var parkourSMNames = new List<string>() { "Climb Actions", "Jump Actions", "Parkour Actions" };

            var parkourStateMachines = rootStateMachine.stateMachines.
                Where(s => parkourSMNames.Contains(s.stateMachine.name)).
                ToDictionary(s => s.stateMachine.name, s => s.stateMachine);

            var locomotionSM = rootStateMachine.stateMachines.FirstOrDefault(s => s.stateMachine.name == "Locomotion").stateMachine;

            var statesToCreateTransition = new[]
            {
                new
                {
                    StateMachine = "Parkour Actions",
                    //States = new [] {"VaultOver", "VaultOn", "MediumStepUp", "Climb Up", "StepUp", "MediumStepUpM" },
                    States = new Dictionary<string,float>{{ "VaultOver", 0}, { "VaultOn", 0 }, { "MediumStepUp", 0 }, { "Climb Up", 1 }, { "StepUp", 0 }, { "MediumStepUpM", 0 } }

                },
                new
                {
                    StateMachine = "Jump Actions",
                    //States = new[] {"LandFromFall", "LandAndStepForward", "LandOnSpot", "FallingToRoll"},
                    States = new Dictionary<string,float> {{ "LandFromFall", 1f}, { "LandOnSpot", 0 }, { "FallingToRoll", 0 } }
                },
                new
                {
                    StateMachine = "Climb Actions",
                    //States = new[] {"FreeHangClimb", "BracedHangClimb" },
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
                    var t = state.AddTransition(locomotionSM, true);
                    var exitTime = stateTransition.States.GetValueOrDefault(state.name);
                    if (exitTime > 0)
                        t.exitTime = exitTime;
                }
            }

            var stateM = controller.layers[0].stateMachine;
            var bt = stateM.states.Where(s => s.state.name == "LandAndStepForward").First().state;
            (bt.motion as BlendTree).blendParameter = "InputMagnitude";
            bt.AddTransition(locomotionSM, true);
        }
        void GenerateAnimationParameters()
        {
            if (controller == null)
            {
                Debug.LogError("Animator Controller not assigned");
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


        void GetWindow()
        {
            if (window == null)
            {
                window = GetWindow<Invector_IntegrationWindow>();
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