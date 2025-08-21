using FS_ThirdPerson;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Animations;
#endif
using UnityEngine;

namespace FS_ParkourSystem
{

    //[CreateAssetMenu(menuName = "Parkour System/New Predictive Action")]
    public class PredictiveAction : ScriptableObject
    {
        [Tooltip("Name of the animation to play for performing the parkour action")]
        [SerializeField] public string animName;

        [Tooltip("Minimum height of the obstacle on which this parkour action can be performed")]
        [SerializeField] float minHeight;
        [Tooltip("Maximum height of the obstacle on which this parkour action can be performed")]
        [SerializeField] float maxHeight;

        [Tooltip("If true, the player will rotate towards the obstacle while performing the parkour action")]
        [SerializeField] bool rotateToObstacle;

        [Tooltip("Determines if this parkour action makes the player vault over or vault onto the obstacle")]
        [SerializeField] VaultType vaultType;

        [Tooltip("If obstacle tag is given, then this parkour action will only be performed on obstacles with the same tag")]
        [SerializeField] string obstacleTag;

        [Tooltip("Delay before giving the control to the player after the parkour action. Useful for action's that requires an additional transition animation.")]
        [SerializeField] float postActionDelay;

        [Header("Target Matching")]

        [Tooltip("If true, the parkour action will use target matching. This is useful for adapting the same animation to obstacles of different sizes")]
        [SerializeField] bool enableTargetMatching = true;

        [Tooltip("The body part that should be used for target matching")]
        [SerializeField] protected AvatarTarget matchBodyPart;

        [Tooltip("Normalized time of the animation at which the target matching should start")]
        [SerializeField] float matchStartTime;

        [Tooltip("Normalized time of the animation at which the target matching should end")]
        [SerializeField] float matchTargetTime;

        [Tooltip("Determines the axes that the target matching should affect")]
        [SerializeField] Vector3 matchPosWeight = new Vector3(0, 1, 0);
        [SerializeField] public AnimationClip clip;

        [Header("Additional Settings")]
        [Tooltip("Minimum horizontal length of the obstacle on which this parkour action can be performed")]
        [SerializeField] float minimumObstacleHorizontalDistance = 0f;

        [Tooltip("Required movement speed to perform parkour")]
        [SerializeField] public float movementThreshold = 0f;

        public Quaternion TargetRotation { get; set; }
        public Vector3 MatchPos { get; set; }
        public bool Mirror { get; set; }

        public virtual bool CheckIfPossible(JumpData hitData, Transform player)
        {

            // Height Tag
            float height = hitData.rootPosition.y - player.position.y;
            if (height < minHeight || height > maxHeight)
                return false;


            var dir = hitData.rootPosition - player.position;
            dir.y = 0;
            if (rotateToObstacle)
                TargetRotation = Quaternion.LookRotation(dir);
            //if (rotateToObstacle)
            //    TargetRotation = Quaternion.LookRotation(Vector3.Scale(-hitData.forwardHit.normal, new Vector3(1, 0, 1)));

            if (enableTargetMatching)
                MatchPos = hitData.rootPosition;

            if (minimumObstacleHorizontalDistance > 0)
            {
                if ((player.position - hitData.rootPosition).magnitude > minimumObstacleHorizontalDistance)
                    return false;
            }

            return true;
        }


        public string AnimName
        {
            get => animName;
            set
            {
                animName = value;
            }
        }
        public bool RotateToObstacle => rotateToObstacle;
        public float PostActionDelay => postActionDelay;

        public bool EnableTargetMatching => enableTargetMatching;
        public AvatarTarget MatchBodyPart => matchBodyPart;
        public float MatchStartTime => matchStartTime;
        public float MatchTargetTime => matchTargetTime;
        public Vector3 MatchPosWeight => matchPosWeight;
    }

#if UNITY_EDITOR

    [CustomEditor(typeof(PredictiveAction))]
    public class PredictiveActionEditor : Editor
    {
        RuntimeAnimatorController animator;

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            var action = target as PredictiveAction;
            GUILayout.Space(10);
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Generate") || action.clip == null)
            {
                if (!string.IsNullOrEmpty(action.animName))
                {
                    if (animator == null)
                        GetAnimatorController();

                    var controller = animator as AnimatorController;

                    var rootStateMachine = controller.layers[0].stateMachine;

                    var parkourStateMachines = rootStateMachine.stateMachines.FirstOrDefault(s => s.stateMachine.name == "Jump Actions");
                    var state = parkourStateMachines.stateMachine.states.FirstOrDefault(s => s.state.name == action.animName);
                    if (action.clip != null)
                    {

                        if (state.state == null)
                        {
                            var newState = parkourStateMachines.stateMachine.AddState(action.animName);
                            var locomotionBT = rootStateMachine.states.FirstOrDefault(s => s.state.name == "Locomotion").state;
                            newState.motion = action.clip;
                            var transition = newState.AddTransition(locomotionBT);
                            transition.exitTime = .9f;
                            //var pos = parkourStateMachines.stateMachine.states.OrderByDescending(s => s.position.y).Last().position;
                            var pos = parkourStateMachines.stateMachine.states.OrderByDescending(s => s.position.y).First().position;
                            var addedState = parkourStateMachines.stateMachine.states.FirstOrDefault(s => s.state.name == action.animName);
                            addedState.position = new Vector3(pos.x + 200, pos.y, pos.z);
                        }
                        else if (state.state.motion != action.clip)
                            state.state.motion = action.clip;
                    }
                    else if (state.state != null)
                        action.clip = state.state.motion as AnimationClip;
                }

            }
            if (GUILayout.Button("Remove"))
            {
                if (animator == null)
                    GetAnimatorController();
                var controller = animator as AnimatorController;

                var rootStateMachine = controller.layers[0].stateMachine;

                var parkourStateMachines = rootStateMachine.stateMachines.FirstOrDefault(s => s.stateMachine.name == "Jump Actions");
                var state = parkourStateMachines.stateMachine.states.FirstOrDefault(s => s.state.name == action.animName);
                if (state.state != null)
                {
                    parkourStateMachines.stateMachine.RemoveState(state.state);
                }
            }
            GUILayout.EndHorizontal();

            EditorGUILayout.Space();
            // Assuming yourSO has a public AnimationClip field
            if (action.clip != null)
            {
                Texture2D previewTexture = AssetPreview.GetAssetPreview(action.clip);
                if (previewTexture != null)
                {
                    GUILayout.Label("", GUILayout.Height(70), GUILayout.Width(70));
                    Rect lastRect = GUILayoutUtility.GetLastRect();
                    GUI.DrawTexture(lastRect, previewTexture);
                }
            }


        }
        void GetAnimatorController()
        {
            var pc = (GameObject)Resources.Load("Parkour Controller");
            animator = pc.GetComponentInChildren<Animator>().runtimeAnimatorController;
        }
    }
#endif
}