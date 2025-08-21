using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


namespace FS_ThirdPerson
{
    public enum SystemState
    {
        Locomotion,
        Parkour,
        Climbing,
        Combat,
        GrapplingHook,
        Swing,
        Other
    }
    public enum SubSystemState
    {
       None,
       Combat,
       Climbing,
       Other
    }

    [DefaultExecutionOrder(-20)]
    public class PlayerController : MonoBehaviour
    {
        public List<SystemBase> managedScripts = new List<SystemBase>();

        [field: SerializeField]
        public SystemState CurrentSystemState { get; private set; }
        public SystemState PreviousSystemState { get; private set; }
        public SystemState DefaultSystemState => SystemState.Locomotion; 
        public SystemState FocusedSystemState => FocusedScript == null ? DefaultSystemState : FocusedScript.State;
        public void SetSystemState(SystemState newState)
        {
            PreviousSystemState = CurrentSystemState;
            CurrentSystemState = newState;
            if (PreviousSystemState != CurrentSystemState) {
                managedScripts.FirstOrDefault((s) => s.State == PreviousSystemState)?.OnStateExited?.Invoke();
                managedScripts.FirstOrDefault((s) => s.State == CurrentSystemState)?.OnStateEntered?.Invoke();
            }
        }
        public void ResetState()
        {
            PreviousSystemState = CurrentSystemState;
            CurrentSystemState = DefaultSystemState;
            if (PreviousSystemState != CurrentSystemState)
            {
                managedScripts.FirstOrDefault((s) => s.State == PreviousSystemState)?.OnStateExited?.Invoke();
                managedScripts.FirstOrDefault((s) => s.State == CurrentSystemState)?.OnStateEntered?.Invoke();
            }
        }
        public void PreviousState()
        {
            CurrentSystemState = PreviousSystemState;
        }

        public bool WaitToStartSystem { get; set; } = false;
        // Register a script to be managed
        public void Register(SystemBase script, bool reorderScripts = false)
        {
            if (!managedScripts.Contains(script))
                managedScripts.Add(script);

            if (reorderScripts)
            {
                managedScripts = managedScripts.OrderByDescending(x => x.Priority).ToList();
            }
        }

        private void OnValidate()
        {
        }

        // Unregister a script
        public void Unregister(SystemBase script)
        {
            managedScripts.Remove(script);
        }

        public Quaternion CameraPlanarRotation { get => Quaternion.LookRotation(Vector3.Scale(cameraGameObject.transform.forward, new Vector3(1, 0, 1))); }
        public GameObject cameraGameObject { get; set; }
        public Animator animator { get; set; }
        //public CharacterController characterController { get; set; }
        //public EnvironmentScanner environmentScanner { get; set`; }
        public ICharacter player { get; set; }

        public Action<float, float> OnStartCameraShake;

        public Action<float, float> OnLand;

        public bool IsInAir { get; set; }
        public bool PreventRotation { get; set; }

        public bool PreventFallingFromLedge { get; set; } = true;


        // Awake all registered scripts
        void Awake()
        {
            player = GetComponent<ICharacter>();
            cameraGameObject = Camera.main.gameObject;
            animator = player.Animator;

            foreach (var script in managedScripts)
            {
                script?.HandleAwake();
            }
        }

        // Start all registered scripts
        void Start()
        {
            foreach (var script in managedScripts)
            {
                script?.HandleStart();
            }
        }

        public SystemBase FocusedScript { get => managedScripts.FirstOrDefault(x => x.IsInFocus); }

        // FixedUpdate all registered scripts
        void FixedUpdate()
        {
            if (player.PreventAllSystems) return;
            var focusedScript = FocusedScript;
            if (focusedScript)
                focusedScript.HandleFixedUpdate();
            else
                foreach (var script in managedScripts)
                {
                    if (script.enabled)
                        script.HandleFixedUpdate();
                }
        }
        // Update all registered scripts

        void Update()
        {
            if (player.PreventAllSystems) return;
            var focusedScript = FocusedScript;
            if (focusedScript)
            {

                focusedScript.HandleUpdate();
            }
            else
                foreach (var script in managedScripts)
                {
                    if (script.enabled)
                        script.HandleUpdate();
                }
        }
        void OnAnimatorMove()
        {
            if (player.UseRootMotion)
            {
                var focusedScript = FocusedScript;
                if (focusedScript)
                {
                    focusedScript.HandleOnAnimatorMove(animator);
                    return;
                }

                if (animator.deltaPosition != Vector3.zero)
                    transform.position += animator.deltaPosition;
                transform.rotation *= animator.deltaRotation;
            }
        }

        public IEnumerator OnStartSystem(SystemBase system)
        {
            player.OnStartSystem(system);
            if (WaitToStartSystem)
                yield return new WaitUntil(() => WaitToStartSystem == false);
        }

        public void UnfocusAllSystem()
        {
            foreach (var system in managedScripts)
            {
                if (system.IsInFocus)
                    player.OnEndSystem(system);
            }
        }

        private void OnGUI()
        {
            // GUILayout.Label(FocusedSystemState.ToString(), new GUIStyle() { fontSize = 24 }); ;
        }
    }
}