using FS_ThirdPerson;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
namespace FS_ThirdPerson
{
    public static partial class AnimatorParameters
    {
        public static int grapplingAngle = Animator.StringToHash("Grappling Angle");
    }
}

namespace FS_GrapplingSystem
{
    public enum Modes
    {
        [Tooltip("Uses only the grappling hook feature.")]
        GrapplingHookOnly,

        [Tooltip("Uses only the swing action feature.")]
        SwingOnly,

        [Tooltip("Combines both features but limits the swing action to a certain angle between the start and end points.")]
        BothWithLimitedSwing
    }
    public class GrapplingHookController : SystemBase
    {
        public bool InAction { get; private set; }
        public bool InSwing { get; private set; }

        Animator animator;
        ICharacter player;
        EnvironmentScanner environmentScanner;
        PlayerController playerController;
        Rope rope;
        GrapplingData grapplingData;
        AnimGraph animGraph;


        public bool enableHook = true;
        public Modes mode = Modes.BothWithLimitedSwing;

        [Tooltip("The maximum allowable angle for the swing action")]
        public float maxSwingAngleLimit = 10;

        [Tooltip("Swing action is allowed only if a hookable point exists between the start and end points.")]
        public bool swingOnlyIfHookPointExists = false;

        public KeyCode hookInput = KeyCode.F;
        public string hookInputButton;
        public AnimationClip ropeThrowingClip;
        public float animationSpeed = 1;

        public float maxDistance = 100f;
        public float maxSwingDistance = 40;
        public float minDistance = 4f;

        [Tooltip("The movement speed when using the grappling hook.")]
        public float grapplingHookMoveSpeed = 12;

        [Tooltip("The movement speed during the swing action.")]
        public float swingActionMoveSpeed = 25;


        public float cameraShakeAmount = .3f;
        public float cameraShakeDuration = .3f;

        public GameObject crosshairPrefab;
        public float crosshairSize = .3f;
        public Transform ropeStartPoint;
        public Transform hookObject;

        public bool debug;

        [Tooltip("Specifies the thickness of the rope used for the grappling hook.")]
        public float ropeRadius = .025f;

        [Tooltip("The material applied to the rope for visual appearance.")]
        public Material ropeMaterial;

        [Tooltip("The speed at which the rope is thrown towards the target point.")]
        public float ropeThrowSpeed = 4f;

        [Tooltip("Determines the number of segments used to render the rope, affecting its smoothness.")]
        public int ropeResolution = 200;

        [Tooltip("The radius of the spiral motion when the rope is thrown.")]
        public float spiralRadius = 0.1f;

        [Tooltip("Controls the frequency of the spiral effect in the rope's motion.")]
        public float spiralFrequency = 5f;

        GameObject crosshairObj;
        Crosshair crosshair;
        float rotateSpeed = 15;
        Vector3 hookPos;

        public UnityEvent RopeReleased;
        public UnityEvent RopeHooked;
        public UnityEvent movementStarted;
        public UnityEvent TargetPointReached;

        public float swingYOffset = 2f;

#if inputsystem
        GrapplingInputAction input;
#endif
        public override SystemState State { get; } = SystemState.GrapplingHook;

        private void Start()
        {
            player = GetComponent<ICharacter>();
            animator = player.Animator;
            environmentScanner = GetComponent<EnvironmentScanner>();
            animGraph = GetComponent<AnimGraph>();
            playerController = GetComponent<PlayerController>();

            rope = new Rope(ropeRadius, ropeMaterial, ropeThrowSpeed, ropeResolution, spiralRadius, spiralFrequency, this.transform, hookObject);

            if (ropeStartPoint == null)
                ropeStartPoint = animator.GetBoneTransform(HumanBodyBones.RightHand);
            SetCrosshair();
        }

        #region Update

        public void FixedUpdate()
        {
            if (playerController.IsInAir && !InAction)
            {
                GrapplingAndSwingHandler();
            }
        }
        public override void HandleFixedUpdate()
        {
            if (!InAction && !playerController.IsInAir)
            {
                GrapplingAndSwingHandler();
            }
        }

        void GrapplingAndSwingHandler()
        {
            crosshair.size = crosshairSize;
            if (enableHook)
            {
                grapplingData = environmentScanner.GetGrapplingLedgeData(maxDistance, minDistance, ropeStartPoint, debug);
                if (grapplingData.hasLedge)
                {

                    var dir = (grapplingData.hookPosition - Camera.main.transform.position).normalized;
                    var offset = dir * crosshair.GetMeshBoundsSize();
                    crosshairObj.transform.position = grapplingData.hookPosition - offset;
                    crosshairObj.SetActive(true);
                }
                else
                    crosshairObj.SetActive(false);
#if inputsystem
                var grapplingInput = input.Grappling.GrapplingHook.inProgress;
#else
                var grapplingInput = Input.GetKey(hookInput) || (!string.IsNullOrEmpty(hookInputButton) && Input.GetButton(hookInputButton));
#endif
                if (grapplingInput && grapplingData.hasLedge)
                {
                    StartCoroutine(DoAction());
                }
            }
        }

        #endregion

        #region grappling and swing

        IEnumerator DoAction()
        {
            if (InAction) yield break;

            InAction = true;
            playerController.PreventRotation = true;

            yield return SetRotation();

            if (playerController.IsInAir || playerController.FocusedScript == null)
            {
                crosshairObj.SetActive(false);
                RopeReleased?.Invoke();
                handIk = true;

                bool hasHookPointToSwing = Physics.Raycast(transform.position + grapplingData.directionToHook * grapplingData.distance * .5f, Vector3.up, out RaycastHit hookPointHit, maxDistance);



                var swingHookPos = hasHookPointToSwing ? hookPointHit.point : transform.position + grapplingData.directionToHook * grapplingData.distance * .5f + Vector3.up * grapplingData.distance / 2;

                var isSwing = IsSwing() && (!swingOnlyIfHookPointExists || hasHookPointToSwing) && grapplingData.distance <= maxSwingDistance;
                isSwing = isSwing && !Physics.Raycast(ropeStartPoint.position, (swingHookPos - ropeStartPoint.position).normalized, out hookPointHit, Vector3.Distance(swingHookPos, ropeStartPoint.position) - .5f, environmentScanner.ObstacleLayer) &&
                            !Physics.Raycast(grapplingData.hookPosition + (swingHookPos - grapplingData.hookPosition).normalized * .1f, (swingHookPos - grapplingData.hookPosition).normalized, out hookPointHit, Vector3.Distance(swingHookPos, grapplingData.hookPosition) - .5f, environmentScanner.ObstacleLayer);

                //Debug.DrawRay(ropeStartPoint.position, (swingHookPos - ropeStartPoint.position).normalized * (Vector3.Distance(swingHookPos, ropeStartPoint.position) - .5f));
                //Debug.DrawRay(grapplingData.hookPosition + (swingHookPos - grapplingData.hookPosition).normalized * .1f, (swingHookPos - grapplingData.hookPosition).normalized * (Vector3.Distance(swingHookPos, grapplingData.hookPosition) - .5f));

                hookPos = isSwing ? swingHookPos : grapplingData.hookPosition;
                yield return animGraph.CrossfadeAvatarMaskAnimationAsync(ropeThrowingClip, Mask.Hand, transitionBack: true, transitionInTime: .2f, removeMaskAfterComplete: false, animationSpeed: animationSpeed);
                rope.ThrowRope(ropeStartPoint, hookPos);

                while (!rope.HasRopeReachedEndpoint())
                {
                    var hp = grapplingData.hookPosition;
                    hp.y = transform.position.y;
                    var direction = (hp - transform.position).normalized;
                    var dir = Quaternion.LookRotation(direction);
                    transform.rotation = dir;

                    if (!playerController.IsInAir && playerController.FocusedScript != null)
                    {
                        CancelHook();
                        break;
                    }
                    yield return null;
                }

                animGraph.RemoveAvatarMask(0.2f);
                RopeHooked?.Invoke();
                movementStarted?.Invoke();
                handIk = false;

                TargetMatchParameters matchParameters = isSwing ? GetSwingActionMatchParams() : GetGrapplingHookMatchParams();

                var currDist = Vector3.Distance(grapplingData.hookPosition, transform.position);
                if (currDist > minDistance && (playerController.IsInAir || playerController.FocusedScript == null))
                {
                    player.OnStartSystem(this);
                    if (playerController.WaitToStartSystem) yield return new WaitUntil(() => playerController.WaitToStartSystem == false);

                    var angle = Vector3.Angle(-transform.up, (grapplingData.hookPosition - transform.position).normalized);
                    animator.SetFloat(AnimatorParameters.grapplingAngle, angle);
                    var speed = isSwing ? swingActionMoveSpeed : grapplingHookMoveSpeed;
                    yield return PlayAnimation(matchParameters, grapplingData.distance, speed, 0.05f);
                }

                TargetPointReached?.Invoke();
                rope.RetractRope();
            }
            CancelHook();
            player.OnEndSystem(this);
        }

        void CancelHook()
        {
            crosshairObj.SetActive(false);
            playerController.PreventRotation = false;
            InAction = false;
        }


        bool IsSwing()
        {
            bool swing = false;
            switch (mode)
            {
                case Modes.GrapplingHookOnly:
                    swing = false;
                    break;
                case Modes.SwingOnly:
                    swing = true;
                    break;
                case Modes.BothWithLimitedSwing:
                    var angle = Vector3.Angle(grapplingData.forwardDirection, grapplingData.directionToHook);
                    Vector3 pathCheckExtend = new Vector3(0.05f, 1f, grapplingData.distance * .5f - 1);
                    bool hasPath = Physics.CheckBox(transform.position + Vector3.up * pathCheckExtend.y * .5f + grapplingData.directionToHook * 1.5f + grapplingData.directionToHook * pathCheckExtend.z, pathCheckExtend, Quaternion.LookRotation(grapplingData.directionToHook));
                    //BoxCastDebug.DrawBoxCastBox(transform.position + Vector3.up * pathCheckExtend.y * .5f+ grapplingData.directionToHook * 1.5f + grapplingData.directionToHook * pathCheckExtend.z, pathCheckExtend, Quaternion.LookRotation(grapplingData.directionToHook), grapplingData.directionToHook,0, Color.red);

                    if (angle <= maxSwingAngleLimit && !hasPath && (!playerController.IsInAir || grapplingData.hookPosition.y < transform.position.y))
                        swing = true;
                    else
                        swing = false;
                    break;
                default:
                    break;
            }
            return swing;
        }


        public IEnumerator PlayAnimation(TargetMatchParameters matchParams, float distance, float moveSpeed, float crossFadeTime = .2f)
        {
            animator.SetFloat(AnimatorParameters.grapplingAngle, animator.GetFloat(AnimatorParameters.grapplingAngle));
            EnableRootMotion();
            if (matchParams != null) Mathf.Min(crossFadeTime, matchParams.matchStartTime + 0.02f);
            animator.CrossFadeInFixedTime(matchParams.animationName, crossFadeTime);
            yield return null;
            var animState = animator.GetNextAnimatorStateInfo(0);
            float timer = 0f;
            bool cameraShakePlayed = false;

            var prevAnimSpeed = animator.speed;
            bool isInFinalMatching = false;

            AvatarTarget target = matchParams.initialTarget;
            var startTime = matchParams.matchStartTime;
            var endTime = matchParams.matchEndTime;


            while (timer <= animState.length)
            {
                if (!player.UseRootMotion)
                    EnableRootMotion();
                var normalizedTime = timer / animState.length;
                if (normalizedTime > matchParams.matchStartTime && normalizedTime < matchParams.matchEndTime)
                {
                    if (!cameraShakePlayed)
                    {
                        cameraShakePlayed = true;
                        playerController.OnStartCameraShake?.Invoke(cameraShakeAmount, cameraShakeDuration);
                    }

                    rope.UpdateRopePull(ropeStartPoint.position);
                }

                if (matchParams != null)
                {
                    if (normalizedTime > matchParams.matchEndTime)
                    {
                        target = AvatarTarget.Root;
                        startTime = matchParams.matchEndTime;
                        endTime = matchParams.finalMatchEndTime;
                        rope.RetractRope();
                        isInFinalMatching = true;
                    }
                    if (!animator.isMatchingTarget && !animator.IsInTransition(0))
                    {
                        if (isInFinalMatching)
                            animator.speed = prevAnimSpeed;
                        else
                            animator.speed = Mathf.Clamp(moveSpeed / (distance), 0, 2);


                        animator.MatchTarget(matchParams.position, matchParams.rotation, target, new MatchTargetWeightMask(matchParams.positionWeight, 0),
                            startTime, endTime);
                    }
                }

                timer += Time.deltaTime * animator.speed;
                yield return null;
            }
            animator.speed = prevAnimSpeed;
            ResetRootMotion();
        }


        TargetMatchParameters GetGrapplingHookMatchParams()
        {
            return new TargetMatchParameters()
            {
                animationName = "Grappling Hook",
                rootmotionMovement = 5,
                position = grapplingData.hookPosition,
                matchStartTime = 0,
                matchEndTime = .64f,
                finalMatchEndTime = .98f,
                initialTarget = AvatarTarget.RightHand,
                finalTarget = AvatarTarget.Root,
                positionWeight = Vector3.one
            };
        }

        TargetMatchParameters GetSwingActionMatchParams()
        {
            return new TargetMatchParameters()
            {
                animationName = "Swing",
                rootmotionMovement = 7,
                position = grapplingData.hookPosition,
                matchStartTime = 0f,
                matchEndTime = .84f,
                finalMatchEndTime = .9f,
                initialTarget = AvatarTarget.RightFoot,
                finalTarget = AvatarTarget.Root,
                positionWeight = Vector3.one
            };

        }

        #endregion

        #region rootmotion

        bool prevRootMotionVal;
        public void EnableRootMotion()
        {
            prevRootMotionVal = player.UseRootMotion;
            player.UseRootMotion = true;
        }
        public void ResetRootMotion()
        {
            player.UseRootMotion = prevRootMotionVal;
        }

        #endregion

        #region IK

        bool handIk;
        float ikWeight = 0;
        float timer = 0;

        private void OnAnimatorIK(int layerIndex)
        {
            if (handIk)
            {
                animator.SetIKPosition(AvatarIKGoal.RightHand, hookPos);
                animator.SetLookAtPosition(grapplingData.hookPosition);

                ikWeight = timer / ropeThrowingClip.length * animationSpeed * .25f;

                animator.SetLookAtWeight(ikWeight * 4);
                animator.SetIKPositionWeight(AvatarIKGoal.RightHand, ikWeight);
                timer += Time.deltaTime;
            }
            else
            {
                //animator.SetIKPositionWeight(AvatarIKGoal.RightHand, 0);
                //animator.SetIKRotationWeight(AvatarIKGoal.RightHand, 0);
                timer = 0;
                ikWeight = 0;
            }
        }

        #endregion

        #region input system
        private void OnEnable()
        {
#if inputsystem
            input = new GrapplingInputAction();
            input.Enable();
#endif
        }

        private void OnDisable()
        {
#if inputsystem
            input.Disable();
#endif
        }

        #endregion

        IEnumerator SetRotation()
        {
            var dir = Quaternion.LookRotation(grapplingData.forwardDirection);
            while (Quaternion.Angle(transform.rotation, dir) > 0.1f)
            {
                transform.rotation = Quaternion.RotateTowards(transform.rotation, dir, rotateSpeed * Time.deltaTime * 50);
                yield return null;
            }
            transform.rotation = dir;

            var angle = Vector3.Angle(-transform.up, grapplingData.directionToHook);
            animator.SetFloat(AnimatorParameters.grapplingAngle, angle);
        }
        void SetCrosshair()
        {
            crosshairObj = Instantiate(crosshairPrefab);
            crosshairObj.name = crosshairPrefab.name;
            crosshair = crosshairObj.AddComponent<Crosshair>();
            crosshairObj.transform.parent = this.transform.parent;
            crosshairObj.transform.localScale = Vector3.zero;
            crosshairObj.SetActive(false);
        }
    }

    public class TargetMatchParameters
    {
        public string animationName;
        public float rootmotionMovement;

        public Vector3 position;
        public Quaternion rotation;

        public AvatarTarget initialTarget;
        public AvatarTarget finalTarget;

        public float matchStartTime;
        public float matchEndTime;
        public float finalMatchEndTime;

        public Vector3 positionWeight = Vector3.one;
    }
}