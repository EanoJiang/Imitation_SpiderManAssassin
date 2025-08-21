using FS_ThirdPerson;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace FS_ThirdPerson 
{
    public static partial class AnimatorParameters
    {
        public static int freeHang = Animator.StringToHash("freeHang");
        public static int jumpBackDirection = Animator.StringToHash("jumpBackDirection");
        public static int BackJumpDir = Animator.StringToHash("BackJumpDir");
        public static int BackJumpMode = Animator.StringToHash("BackJumpMode");
        public static int mirrorJump = Animator.StringToHash("mirrorJump");
        public static int isFalling = Animator.StringToHash("isFalling");
        public static int mirrorAction = Animator.StringToHash("mirrorAction");
    }
}

namespace FS_ParkourSystem
{
    public class ParkourController : SystemBase
    {
        //[field: Header("Predictive Jumping")]
        [field: Tooltip("Enables Predictive Jumping")]
        public bool enablePredictiveJump = true;

        //[field: Tooltip("Enables balance walking on narrow beams")]
        //public bool enableBalanceWalk = true;

        [field: Space(10)]

        [field: Tooltip("Maximum height that the player can jump")]
        [field: SerializeField] public float MaxJumpHeight { get; private set; } = 1.2f;

        [field: Tooltip("The character should only jump when he's close to the ledge. This is the distance from ledge at which he will jump")]
        [field: SerializeField] float minDistanceFromLedgeForJumping = .5f; 

        [field: SerializeField] public float MaxJumpDistance { get; private set; } = 7f;

        [Tooltip("Radius of the foot collider. Foot collider is used check if the player's foot landed on any obstacle during the predictive jump")]
        [SerializeField] float footColliderRadius = 0.1f;

        [Tooltip("Offset between the foot collider and the foot bone")]
        [SerializeField] Vector3 footColliderOffset;

        [Tooltip("List of Parkour Actions the player can perform while standing close to the obstacle")]
        public List<ParkourAction> parkourActions = new List<ParkourAction>();

        [HideInInspector] public List<PredictiveAction> predictiveActions = new List<PredictiveAction>();


        [field: Tooltip("The speed at which the player will rotate while performing parkour or climbing actions")]
        [field: SerializeField] public float RotationSpeed { get; private set; } = 500f;

        public bool InAction { get; set; }
        public bool IsHanging { get; set; }
        public bool IsJumping { get; set; }
        public bool IsOnLedge => isOnLedge;

        public bool ControlledByParkour => InAction || IsHanging || IsJumping;


        Vector3 prevPos = Vector3.zero;

        // Predictive jump states
        bool mirrorJump = false;
        bool inPredictiveJump = false;
        bool inAirOfPredictiveJump = false;
        bool isGrounded = false;
        bool isOnLedge = false;
        bool prevControlVal;


        float ySpeed = 0f;
        JumpData _jumpPoint;

        public bool EnableWallRun;

        ParkourInputManager inputManager;
        LocomotionInputManager locomotionInputManager;
        EnvironmentScanner environmentScanner;
        Animator animator;
        PlayerController playerController;
        ICharacter player;
        ClimbController climbController;

        public override SystemState State { get; } = SystemState.Parkour;
        public override float Priority => 10;

        private void OnEnable()
        {
            playerController = GetComponent<PlayerController>();
            player = GetComponent<ICharacter>();
            animator = player.Animator;
            environmentScanner = GetComponent<EnvironmentScanner>();
            inputManager = GetComponent<ParkourInputManager>();
            locomotionInputManager = GetComponent<LocomotionInputManager>();

            climbController = GetComponent<ClimbController>();
            animator.SetFloat(AnimatorParameters.fallAmount, 1);
        }


        public override void HandleFixedUpdate()
        {
            HandleParkourUpdate();  
        }

        void HandleParkourUpdate()
        {
            //if (player.PreventActions)
            //    return;

            // We must only apply the root motion to the character's position if ControlledByParkrour is true
            if (!ControlledByParkour)
                DisableRootMotion();

            //HandleParkourEvents();
            //if (!InAction && climbController.enableClimbing)
                //climbController.HandleClimbUpdate();

            // If the character is in a predictiveJump or hanging then return
            if (inAirOfPredictiveJump || IsHanging)
            {
                prevPos = transform.position;
                isOnLedge = false;
                return;
            }

            // Pefrom Parkour Actions or Predictive Jump/Climb
            if (inputManager.Jump && player.IsGrounded)
            {
                var hitData = environmentScanner.ObstacleCheck();
                HandleParkourAction(hitData);

                HandlePredictiveJumpAndClimb();

                if (hitData.forwardHitFound && !InAction && Vector3.Angle(Vector3.up, hitData.forwardHit.normal) > 60f && !climbController.isFalling && EnableWallRun)
                    StartCoroutine(HandleWallRun(hitData));
            }

            // Jump Down from a height
            if (inputManager.Drop && player.MoveDir != Vector3.zero && !InAction && player.IsGrounded && isOnLedge)
            {
                var hitData = environmentScanner.ObstacleCheck(performHeightCheck: false);
                if (!hitData.forwardHitFound && !Physics.Raycast(transform.position + Vector3.up * 0.1f, transform.forward, 0.5f, environmentScanner.ObstacleLayer))
                {
                    StartCoroutine(DoAction("Jump Down", rotate: true, targetRot: Quaternion.LookRotation(player.MoveDir), onComplete: () => player.OnEndSystem(this)));
                    isOnLedge = false;
                    return;
                }
            }

            prevPos = transform.position;
        }

        void HandleParkourAction(ObstacleHitData hitData)
        {
            if (!InAction && hitData.forwardHitFound && hitData.heightHitFound && hitData.hasSpace
                && Vector3.Angle(Vector3.up, hitData.forwardHit.normal) > 45f
                && Vector3.Angle(Vector3.up, hitData.heightHit.normal) <= 45f)
            {
                List<ParkourAction> selectedActions = new();
                foreach (var action in parkourActions)
                {
                    if (action.CheckIfPossible(hitData, transform))
                    {
                        selectedActions.Add(action);
                    }
                }

                List<ParkourAction> priorityActions = new();

                foreach (var action in selectedActions)
                {
                    if (animator.GetFloat("moveAmount") >= action.movementThreshold)
                    {
                        StartCoroutine(DoParkourAction(action));
                        return;
                    }

                }
                if (selectedActions.Count > 0)
                {
                    StartCoroutine(DoParkourAction(selectedActions[UnityEngine.Random.Range(0, selectedActions.Count)]));
                    return;
                }
            }
        }

        void HandlePredictiveJumpAndClimb(Vector3? direction = null)
        {
            Vector3 _direction = direction != null ? direction.Value : player.MoveDir;

            if ((enablePredictiveJump || climbController.enableClimbing) && !InAction || (InAction && inPredictiveJump))
            {
                var jumpData = environmentScanner.FindPointToJump(_direction, MaxJumpDistance, MaxJumpHeight);
                if (jumpData != null)
                {
                    // Predictive climb should be performed when a jumpPoint is not found or if there is a climbPoint that's closer than the jumpPoint
                    if (climbController.enableClimbing && jumpData.isClimbable
                        && (!jumpData.jumpPointFound || (jumpData.jumpPointFound && Vector3.Distance(jumpData.climbHitData.point, transform.position) < Vector3.Distance(jumpData.rootPosition, transform.position))))
                    {
                        // if there is a ledge The character should only jump when he's close to the ledge
                        if (!jumpData.hasLedge || Vector3.Distance(transform.position, jumpData.pointBeforeledge) < minDistanceFromLedgeForJumping)
                            if (climbController.ClimbToLedge(jumpData.climbHitData.transform, jumpData.climbHitData.point, checkDirection: direction))
                                return;
                    }
                    if (enablePredictiveJump && jumpData.jumpPointFound)
                    {
                        // if there is a ledge The character should only jump when he's close to the ledge
                        if (jumpData.hasLedge && Vector3.Distance(transform.position, jumpData.pointBeforeledge) > minDistanceFromLedgeForJumping)
                            return;

                        if (customPredictiveAction)
                        {
                            List<PredictiveAction> selectedActions = new();
                            foreach (var action in predictiveActions)
                            {
                                if (action.CheckIfPossible(jumpData, transform))
                                {
                                    selectedActions.Add(action);
                                }
                            }
                        }

                        StartCoroutine(DoPredictiveJump(jumpData));
                        return;
                    }
                }
            }
        }
        bool customPredictiveAction;

        void HandleParkourEvents()
        {
            // If the value of ControlledByParkour changed, then call the OnStartParkourAction or OnEndParkourAction
            if (ControlledByParkour && !prevControlVal)
            {
                player.OnStartSystem(this);
                prevControlVal = true;
            }
            else if (!ControlledByParkour && prevControlVal)
            {
                player.OnEndSystem(this);
                prevControlVal = false;
            }
        }

        public IEnumerator DoCustomPredictiveJump(PredictiveAction predictiveAction,JumpData jumpPoint,string animName = null, float crossFadeTime = 0.2f)
        {
            DisableRootMotion();
            InAction = true;

            player.OnStartSystem(this);
            if (playerController.WaitToStartSystem) yield return new WaitUntil(() => playerController.WaitToStartSystem == false);

            _jumpPoint = jumpPoint;

            inAirOfPredictiveJump = true;
            inPredictiveJump = true;

            matchFootToTarget = false;
            isGrounded = false;
            bool isFalling = false;

            footMatchWeight = 0f;

            animator.SetBool(AnimatorParameters.IsGrounded, false);

            var dispVec = jumpPoint.rootPosition - transform.position;
            dispVec.y = 0f;
            var targetRot = Quaternion.LookRotation(dispVec);
            var anim = animName;

            Vector3 deltaRootPos = Vector3.zero;
            if (animName == null)
            {
                ////leg in the back is used for jump
                //var forward = transform.position + transform.forward;
                ////var forward = transform.position + dispVec.normalized;
                //var diff = (animator.GetBoneTransform(HumanBodyBones.RightFoot).position - forward).sqrMagnitude - (animator.GetBoneTransform(HumanBodyBones.LeftFoot).position - forward).sqrMagnitude;

                //if (Mathf.Abs(diff) < 0.1f)
                //    mirrorJump = !mirrorJump;
                //else
                //    mirrorJump = diff < 0;

                mirrorJump = !mirrorJump;

                animName = (mirrorJump) ? "Predictive JumpM" : "Predictive Jump";
                animator.SetBool(AnimatorParameters.mirrorJump, mirrorJump);

                //AnimatorHelper animatorHelper = new AnimatorHelper();
                //var _animator = animatorHelper.sampleAnimation((mirrorJump) ? "Jump Idle M" : "Jump Idle", animator);

                //deltaRootPos = animatorHelper.getTransformPos((mirrorJump) ? HumanBodyBones.LeftFoot : HumanBodyBones.RightFoot, 1) + animatorHelper.getTransformRootPos(1);
                //deltaRootPos = Vector3.zero;

                //animatorHelper.closeSampler(_animator);
            }


            animator.CrossFadeInFixedTime(animName, crossFadeTime);
            yield return null;
            var t = 0f;
            if (anim == null)
            {
                EnableRootMotion();
                while (true)
                {
                    t += Time.deltaTime;
                    if (t > .15f && Quaternion.Angle(transform.rotation, targetRot) < .5f)
                    {
                        t = 0;
                        break;
                    }
                    transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRot, RotationSpeed * 2 * Time.deltaTime);
                    yield return null;
                }
                DisableRootMotion();
            }
            Vector3 jumpVel;
            float jumpTime;

            (jumpVel, jumpTime) = CalculateJumpVelocity(transform.position, jumpPoint.rootPosition + targetRot * deltaRootPos);

            matchPosGlobal = jumpPoint.rootPosition;

            Vector3 startPos = transform.position;

            ySpeed = jumpVel.y;
            float footMatchStartTime = Mathf.Max(jumpTime * 0.9f, jumpTime - 0.1f);

            //bool playLand = true;
            float timer = 0f;



            while (true)
            {
                timer += Time.deltaTime;

                Vector3 velocity = jumpVel;
                ySpeed += player.Gravity * Time.deltaTime;
                //ySpeed = Mathf.Clamp(ySpeed, -20f, +20f);
                velocity.y = ySpeed;

                transform.position += velocity * Time.deltaTime;
                //GetComponent<CharacterController>().Move(velocity * Time.deltaTime);

                if (IsInFocus)
                    transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRot, RotationSpeed * Time.deltaTime);


                if (timer > footMatchStartTime)
                {
                    matchFootToTarget = true;
                    footMatchWeight = (timer - footMatchStartTime) / (jumpTime - footMatchStartTime);
                }

                if (timer > 0.3f)
                {

                    JumpGroundCheck(velocity * Time.deltaTime);
                    var fp = animator.GetBoneTransform(mirrorJump ? HumanBodyBones.LeftFoot : HumanBodyBones.RightFoot).position;
                    var distToTarget = Vector3.Distance(jumpPoint.footPosition, fp);

                    if (distToTarget < 0.1 || timer > jumpTime)
                        isGrounded = true;
                    if (isGrounded)
                    {
                        animator.SetBool(AnimatorParameters.IsGrounded, isGrounded);


                        if (isFalling)
                        {
                            ResetPositionY();

                            var halfExtends = new Vector3(.3f, .9f, 0.01f);
                            var hasSpaceForRoll = Physics.BoxCast(transform.position + Vector3.up, halfExtends, transform.forward, Quaternion.LookRotation(transform.forward), 2.5f, environmentScanner.ObstacleLayer);

                            halfExtends = new Vector3(.1f, .1f, 0.01f);
                            var heightHiting = true;
                            for (int i = 0; i < 6 && heightHiting; i++)
                                heightHiting = Physics.BoxCast(transform.position + Vector3.up * 1.8f + transform.forward * (i * .5f + .5f), halfExtends, Vector3.down, Quaternion.LookRotation(Vector3.down), 2.2f + i * .2f, environmentScanner.ObstacleLayer);
                            if (jumpPoint.hasSpaceToLand)
                            {
                                matchFootToTarget = false;

                                EnableRootMotion();
                                //var angle = Vector3.Angle(player.MoveDir, transform.forward);
                                //if (input.DirectionInput != Vector2.zero && angle < 10 && !hasSpaceForRoll && heightHiting)


                                if (!hasSpaceForRoll && heightHiting)
                                {
                                    playerController.OnLand?.Invoke(Mathf.Clamp((startPos.y - jumpPoint.rootPosition.y) * 0.003f, 0f, 0.05f), 1.2f);
                                    yield return DoAction("FallingToRoll");
                                }
                                else
                                {
                                    playerController.OnLand?.Invoke(Mathf.Clamp((startPos.y - jumpPoint.rootPosition.y) * 0.003f, 0f, 0.05f), .7f);
                                    yield return DoAction("LandFromFall");
                                }
                                ResetRootMotion();
                            }
                            else
                                animator.CrossFadeInFixedTime("LandOnSpot", .13f);

                            isFalling = false;
                            animator.SetBool(AnimatorParameters.isFalling, isFalling);
                        }
                        else
                        {
                            animator.CrossFadeInFixedTime((jumpPoint.hasSpaceToLand) ? "LandAndStepForward" : "LandOnSpot", .13f);
                            //int landingType = (jumpPoint.hasSpaceToLand) ? 2 : 3;
                            //animator.SetInteger("landingType", landingType);
                        }
                        break;
                    }
                }

                if (!isFalling && timer > .6f && startPos.y - jumpPoint.rootPosition.y > 3f)
                {
                    isFalling = true;
                    animator.SetBool(AnimatorParameters.isFalling, isFalling);
                }

                yield return null;
            }

            transform.rotation = targetRot;

            inAirOfPredictiveJump = false;
            IsHanging = false;

            //yield return new WaitForSeconds(0.5f);

            matchFootToTarget = false;

            if (!inAirOfPredictiveJump)
            {
                player.OnEndSystem(this);
                ResetRootMotion();
                InAction = false;
                inPredictiveJump = false;
            }

        }


        Transform jumpTarget;

        bool matchFootToTarget;
        float footMatchWeight = 0f;
        public IEnumerator DoPredictiveJump(JumpData jumpPoint, string animName = null, float crossFadeTime = 0.2f)
        {
            DisableRootMotion();
            InAction = true;

            player.OnStartSystem(this);
            if (playerController.WaitToStartSystem) yield return new WaitUntil(() => playerController.WaitToStartSystem == false);

            _jumpPoint = jumpPoint;

            inAirOfPredictiveJump = true;
            inPredictiveJump = true;

            matchFootToTarget = false;
            isGrounded = false;
            bool isFalling = false;

            footMatchWeight = 0f;

            animator.SetBool(AnimatorParameters.IsGrounded, false);

            var dispVec = jumpPoint.rootPosition - transform.position;
            dispVec.y = 0f;
            var targetRot = Quaternion.LookRotation(dispVec);
            var anim = animName;

            Vector3 deltaRootPos = Vector3.zero;
            if (animName == null)
            {
                ////leg in the back is used for jump
                //var forward = transform.position + transform.forward;
                ////var forward = transform.position + dispVec.normalized;
                //var diff = (animator.GetBoneTransform(HumanBodyBones.RightFoot).position - forward).sqrMagnitude - (animator.GetBoneTransform(HumanBodyBones.LeftFoot).position - forward).sqrMagnitude;

                //if (Mathf.Abs(diff) < 0.1f)
                //    mirrorJump = !mirrorJump;
                //else
                //    mirrorJump = diff < 0;

                mirrorJump = !mirrorJump;

                animName = (mirrorJump) ? "Predictive JumpM" : "Predictive Jump";
                animator.SetBool(AnimatorParameters.mirrorJump, mirrorJump);

                //AnimatorHelper animatorHelper = new AnimatorHelper();
                //var _animator = animatorHelper.sampleAnimation((mirrorJump) ? "Jump Idle M" : "Jump Idle", animator);

                //deltaRootPos = animatorHelper.getTransformPos((mirrorJump) ? HumanBodyBones.LeftFoot : HumanBodyBones.RightFoot, 1) + animatorHelper.getTransformRootPos(1);
                //deltaRootPos = Vector3.zero;

                //animatorHelper.closeSampler(_animator);
            }

            animator.CrossFadeInFixedTime(animName, crossFadeTime);
            yield return null;
            var t = 0f;
            if (anim == null)
            {
                EnableRootMotion();
                while (true)
                {
                    t += Time.deltaTime;
                    if (t > .15f && Quaternion.Angle(transform.rotation, targetRot) < .5f)
                    {
                        t = 0;
                        break;
                    }
                    transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRot, RotationSpeed * 2 * Time.deltaTime);
                    yield return null;
                }
                DisableRootMotion();
            }
            Vector3 jumpVel;
            float jumpTime;

            (jumpVel, jumpTime) = CalculateJumpVelocity(transform.position, jumpPoint.rootPosition + targetRot * deltaRootPos);

            matchPosGlobal = jumpPoint.rootPosition;

            Vector3 startPos = transform.position;

            ySpeed = jumpVel.y;
            float footMatchStartTime = Mathf.Max(jumpTime * 0.9f, jumpTime - 0.1f);

            //bool playLand = true;
            float timer = 0f;
            playerController.IsInAir = true;
            while (true && IsInFocus)
            {
                timer += Time.deltaTime;

                Vector3 velocity = jumpVel;
                ySpeed += player.Gravity * Time.deltaTime;
                //ySpeed = Mathf.Clamp(ySpeed, -20f, +20f);
                velocity.y = ySpeed;

                transform.position += velocity * Time.deltaTime;
                //GetComponent<CharacterController>().Move(velocity * Time.deltaTime);

                if (!playerController.PreventRotation)
                    transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRot, RotationSpeed * Time.deltaTime);


                if (timer > footMatchStartTime)
                {
                    matchFootToTarget = true;
                    footMatchWeight = (timer - footMatchStartTime) / (jumpTime - footMatchStartTime);
                }

                if (timer > 0.3f)
                {

                    JumpGroundCheck(velocity * Time.deltaTime);
                    var fp = animator.GetBoneTransform(mirrorJump ? HumanBodyBones.LeftFoot : HumanBodyBones.RightFoot).position;
                    var distToTarget = Vector3.Distance(jumpPoint.footPosition, fp);

                    if (distToTarget < 0.1 || timer > jumpTime)
                        isGrounded = true;
                    if (isGrounded)
                    {
                        animator.SetBool(AnimatorParameters.IsGrounded, isGrounded);
                        playerController.IsInAir = false;

                        if (isFalling)
                        {
                            ResetPositionY();

                            var halfExtends = new Vector3(.3f, .9f, 0.01f);
                            var hasSpaceForRoll = Physics.BoxCast(transform.position + Vector3.up, halfExtends, transform.forward, Quaternion.LookRotation(transform.forward), 2.5f, environmentScanner.ObstacleLayer);

                            halfExtends = new Vector3(.1f, .1f, 0.01f);
                            var heightHiting = true;
                            for (int i = 0; i < 6 && heightHiting; i++)
                                heightHiting = Physics.BoxCast(transform.position + Vector3.up * 1.8f + transform.forward * (i * .5f + .5f), halfExtends, Vector3.down, Quaternion.LookRotation(Vector3.down), 2.2f + i * .2f, environmentScanner.ObstacleLayer);
                            if (jumpPoint.hasSpaceToLand)
                            {
                                matchFootToTarget = false;

                                EnableRootMotion();
                                //var angle = Vector3.Angle(player.MoveDir, transform.forward);
                                //if (input.DirectionInput != Vector2.zero && angle < 10 && !hasSpaceForRoll && heightHiting)


                                if (!hasSpaceForRoll && heightHiting)
                                {
                                    playerController.OnLand?.Invoke(Mathf.Clamp((startPos.y - jumpPoint.rootPosition.y) * 0.003f, 0f, 0.05f), 1.2f);
                                    yield return DoAction("FallingToRoll");
                                }
                                else
                                {
                                    playerController.OnLand?.Invoke(Mathf.Clamp((startPos.y - jumpPoint.rootPosition.y) * 0.003f, 0f, 0.05f), .7f);
                                    yield return DoAction("LandFromFall");
                                }
                                ResetRootMotion();
                            }
                            else
                                animator.CrossFadeInFixedTime("LandOnSpot", .13f);

                            isFalling = false;
                            animator.SetBool(AnimatorParameters.isFalling, isFalling);
                        }
                        else
                        {
                            animator.CrossFadeInFixedTime((jumpPoint.hasSpaceToLand) ? "LandAndStepForward" : "LandOnSpot", .13f);
                            //int landingType = (jumpPoint.hasSpaceToLand) ? 2 : 3;
                            //animator.SetInteger("landingType", landingType);
                        }
                        break;
                    }
                }

                if (!isFalling && timer > .6f && startPos.y - jumpPoint.rootPosition.y > 3f)
                {
                    isFalling = true;
                    animator.SetBool(AnimatorParameters.isFalling, isFalling);
                }

                yield return null;
            }
            
            playerController.IsInAir = false;
            inAirOfPredictiveJump = false;
            IsHanging = false;

            //yield return new WaitForSeconds(0.5f);

            matchFootToTarget = false;


            if (IsInFocus)
            {
                transform.rotation = targetRot;
                player.OnEndSystem(this);
            }
            ResetRootMotion();
            InAction = false;
            inPredictiveJump = false;
        }

        public void ResetPositionY()
        {
            if (Physics.SphereCast(transform.position + Vector3.up * 0.6f, 0.2f, Vector3.down, out RaycastHit hit, 1f, environmentScanner.ObstacleLayer))
                StartCoroutine(TweenVal(transform.position.y, hit.point.y, 0.2f, (lerpVal) => { transform.position = new Vector3(transform.position.x, lerpVal, transform.position.z); })); ;
        }

        public IEnumerator TweenVal(float start, float end, float duration, Action<float> onLerp)
        {
            float timer = 0f;
            float percent = timer / duration;

            while (percent <= 1f)
            {
                timer += Time.deltaTime;
                percent = timer / duration;
                var lerpVal = Mathf.Lerp(start, end, percent);
                onLerp?.Invoke(lerpVal);

                yield return null;
            }
        }

        (Vector3, float) CalculateJumpVelocity(Vector3 startPos, Vector3 targetPos)
        {
            float gravity = player.Gravity;

            float displacementY = targetPos.y - startPos.y;
            var displacementXZ = new Vector3(targetPos.x - startPos.x, 0, targetPos.z - startPos.z);

            var h = environmentScanner.getJumpHeight(displacementY, displacementXZ);

            float ty = Mathf.Sqrt(-2 * h / gravity);
            float tx = Mathf.Sqrt(2 * (displacementY - h) / gravity);
            float t = tx + ty;

            var velocityY = Vector3.up * Mathf.Sqrt(-2 * gravity * h);
            var velocityXZ = displacementXZ / t;

            return (velocityY + velocityXZ, t);
        }

        public IEnumerator DoPredictiveBackJump(JumpData jumpPoint = null, ClimbPoint climbPoint = null, int climbTransition = 0, bool combo = false)
        {
            Vector3 dir;
            if (jumpPoint != null)
                dir = transform.position - jumpPoint.rootPosition;
            else
                dir = transform.position - climbPoint.transform.position;

            var angleBWvec = Vector3.SignedAngle(transform.forward, new Vector3(dir.x, 0, dir.z), Vector3.down);
            var value = angleBWvec / 90;

            //animator.SetFloat(AnimatorParameters.jumpBackDirection, Mathf.Sign(value) * 0.1f + value);

            var currentPoint = climbPoint != null ? climbController.previousPoint : climbController.currentPoint;
            if (climbController.hangType == HangType.freeHang && !currentPoint.transform.parent.CompareTag("SwingableLedge") && Mathf.Abs(value) > 1f)
            {
                if (climbController.currentPoint && climbPoint != null)
                    climbController.currentPoint = climbController.previousPoint;
                inAirOfPredictiveJump = inPredictiveJump = InAction = false;
                IsHanging = true;
                ResetRootMotion();
                yield break;
            }

            InAction = inAirOfPredictiveJump = inPredictiveJump = true;

            DisableRootMotion();

            player.OnStartSystem(climbController );

            if (playerController.WaitToStartSystem) yield return new WaitUntil(() => playerController.WaitToStartSystem == false);


            matchFootToTarget = isGrounded = false;
            animator.SetBool(AnimatorParameters.IsGrounded, false);



            var height = environmentScanner.getJumpHeight(dir.z, new Vector3(dir.x, 0, dir.z));

            mirrorJump = !mirrorJump;
            var animName = (mirrorJump) ? "Jump Idle M" : "Jump Idle";
            float stopPerc = 0.5f;


            if (climbController.hangType == HangType.bracedHang)
            {
                combo = false;
                animator.SetFloat(AnimatorParameters.jumpBackDirection, Mathf.Sign(value) * Mathf.Clamp(Mathf.Abs(value), 0.1f, 1));
                animator.CrossFade("predictiveJumpBack", 0.2f);

            }
            else
            {
                animator.SetFloat(AnimatorParameters.jumpBackDirection, Mathf.Sign(value) * Mathf.Clamp(Mathf.Abs(value), 0.1f, 2));
                animator.CrossFadeInFixedTime("predictiveJumpBackFreeHang", 0.2f);
                if (combo)
                    animator.CrossFadeInFixedTime("Freehang Forward Jump", 0.2f);

                if (Mathf.Abs(value) > 1.2f) // changeable
                {
                    //stopPerc = 0.7f;
                    stopPerc = Mathf.Clamp(0, 0.7f, height);
                }
                else
                {
                    stopPerc = 0.6f;
                    if (combo)
                    {
                        if (Mathf.Abs(value) < 0.8f)
                        {
                            animator.CrossFade("predictiveJumpBackFreeHang", 0.2f, 0, 0.4f); // 0.4f is peak time
                            stopPerc = 0.1f;
                        }
                        else stopPerc = 0.2f;

                        combo = false;  // enable combo only for forward jumps , otherwise disable
                    }
                }

                //animName = "Freehang Forward Jump Idle";
            }

            yield return null;

            var animState = animator.GetNextAnimatorStateInfo(0);
            var prevHandPos = value > 0 ? climbController.leftHand.boneTransform : climbController.rightHand.boneTransform;

            float startTimer = 0f;
            if (!combo)
            {
                animator.speed = Mathf.Clamp(dir.magnitude * 0.12f, 1f, 1.5f);
                while (startTimer <= animState.length * stopPerc)
                {
                    transform.position += animator.deltaPosition;
                    transform.rotation *= animator.deltaRotation;
                    if (climbController.hangType == HangType.freeHang)
                    {
                        //var handPos = animator.GetBoneTransform(HumanBodyBones.RightHand).position;
                        var handPos = value > 0 ? climbController.leftHand.boneTransform : climbController.rightHand.boneTransform;
                        //var offset = climbController.rightHand.currentVecPoint - handPos;
                        var offset = prevHandPos - handPos;
                        transform.position = Vector3.MoveTowards(transform.position, transform.position + offset, 10f * Time.deltaTime);
                    }
                    startTimer += Time.deltaTime;
                    yield return null;
                }
                animator.speed = 1;
            }
            else if (climbController.hangType == HangType.freeHang)
            {
                //while (startTimer <= animState.length * 0.6f)
                while (startTimer <= animState.length * Mathf.Clamp(0, 0.6f, height))
                {
                    transform.position += animator.deltaPosition;
                    transform.rotation *= animator.deltaRotation;
                    if (climbController.hangType == HangType.freeHang)
                    {
                        //var handPos = animator.GetBoneTransform(HumanBodyBones.RightHand).position;
                        var handPos = value > 0 ? climbController.leftHand.boneTransform : climbController.rightHand.boneTransform;
                        //var offset = climbController.rightHand.currentVecPoint - handPos;
                        var offset = prevHandPos - handPos;
                        transform.position = Vector3.MoveTowards(transform.position, transform.position + offset, 10f * Time.deltaTime);
                    }
                    startTimer += Time.deltaTime;
                    yield return null;
                }
            }
            player.OnEndSystem(climbController);

            if (jumpPoint != null)
                StartCoroutine(DoPredictiveJump(jumpPoint, animName, 0.3f));
            else if (climbPoint != null)
                StartCoroutine(DoPredictiveClimb(climbPoint, climbTransition, animName, 0.8f));
            else
            {
                inAirOfPredictiveJump = inPredictiveJump = InAction = false;
                //player.OnEndSystem(climbController);
                ResetRootMotion();
            }
        }

        public IEnumerator DoPredictiveClimb(ClimbPoint point, int climbTransition, string animName = null, float crossFadeTime = 0.2f)
        {

            DisableRootMotion();
            InAction = true;

            player.OnStartSystem(climbController);
            if (playerController.WaitToStartSystem) yield return new WaitUntil(() => playerController.WaitToStartSystem == false);

            inAirOfPredictiveJump = true;
            inPredictiveJump = true;

            matchFootToTarget = false;
            isGrounded = false;

            animator.SetBool(AnimatorParameters.IsGrounded, false);
            Vector3 jumpVel;
            float jumpTime;

            var hangTypeName = climbTransition == 0 ? "PredictiveToBracedhang" : "PredictiveToFreehang";


            //(jumpVel, jumpTime) = CalculateJumpVelocity(transform.position, point.transform.position + -point.transform.up * 1.8f + point.transform.forward * 0.1f);
            AnimatorHelper animatorHelper = new AnimatorHelper();
            var _animator = animatorHelper.sampleAnimation(hangTypeName, animator);

            var endTime = animatorHelper.findEndTime(AvatarTarget.RightHand);

            var pos = animatorHelper.getTransformRootPos(endTime) - animatorHelper.getTransformPos(HumanBodyBones.RightHand, endTime);

            var endCenter = (animatorHelper.getTransformPos(HumanBodyBones.LeftHand, 1) + animatorHelper.getTransformPos(HumanBodyBones.RightHand, 1)) / 2;
            var right = (animatorHelper.getTransformPos(HumanBodyBones.RightHand, 1) - endCenter) + climbController.handOffsets;
            right = animatorHelper.pointTransformWithVectorDown(point.transform, right);

            //GizmosExtend.AddByName("right", () => GizmosExtend.drawSphere(right, 0.01f, Color.grey));

            var rotatedPos = animatorHelper.pointTransformWithVectorDown(point.transform, pos, right);

            climbController.initializeBodyParts(null, animatorHelper);

            matchPosGlobal = right;

            animatorHelper.closeSampler(_animator);

            var dispVec = point.transform.position - transform.position;
            dispVec.y = 0f;
            var targetRot = Quaternion.LookRotation(dispVec);
            var anim = animName;
            if (animName == null)
            {
                //leg in the back is used for jump
                var forward = transform.position + transform.forward;
                //var forward = transform.position + dispVec.normalized;
                var diff = (animator.GetBoneTransform(HumanBodyBones.RightFoot).position - forward).sqrMagnitude - (animator.GetBoneTransform(HumanBodyBones.LeftFoot).position - forward).sqrMagnitude;

                if (Mathf.Abs(diff) < 0.1f)
                    mirrorJump = !mirrorJump;
                else
                    mirrorJump = diff < 0;
                //mirrorJump = !mirrorJump;
                animName = (mirrorJump) ? "Predictive JumpM" : "Predictive Jump";
                animator.SetBool(AnimatorParameters.mirrorJump, mirrorJump);
            }

            animator.CrossFadeInFixedTime(animName, crossFadeTime);
            yield return null;

            var t = 0f;
            if (anim == null)
            {
                EnableRootMotion();
                while (true)
                {
                    t += Time.deltaTime;
                    if (t > .15f)
                    {
                        t = 0;
                        break;
                    }
                    transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRot, RotationSpeed * 5 * Time.deltaTime);
                    yield return null;
                }
                DisableRootMotion();
            }
            (jumpVel, jumpTime) = CalculateJumpVelocity(transform.position, rotatedPos);

            //hangChange
            animator.SetFloat(AnimatorParameters.freeHang, climbTransition);

            Vector3 startPos = transform.position;

            ySpeed = jumpVel.y;

            AnimatorStateInfo animState = animator.GetNextAnimatorStateInfo(0);

            var start = true;
            var idleStart = true;
            var camShake = true;
            float timer = 0f;

            var temp = jumpTime;
            var transitionTime = jumpTime * 0.3f;  // changeble
            var idleTransitionTime = jumpTime * 0.7f;  // changeble


            bool DoCombo = false;
            bool DoQuickCombo = false;
            float comboTime = 0f;
            playerController.IsInAir = true;
            while (timer <= temp && climbController.IsInFocus)
            {

                if (timer <= jumpTime)
                {
                    Vector3 velocity = jumpVel;
                    velocity.y = ySpeed;
                    transform.position += velocity * Time.deltaTime;
                    ySpeed += player.Gravity * Time.deltaTime;

                    if (timer >= jumpTime - transitionTime && start)
                    {
                        animator.CrossFade(hangTypeName, 0.3f);
                        animator.Update(0);
                        animState = animator.GetNextAnimatorStateInfo(0);
                        start = false;
                        targetRot = Quaternion.LookRotation(-point.transform.forward);
                        temp += animState.length; // small delay, should be endTime here
                        animator.speed = endTime * (animState.length) / transitionTime;
                    }
                    else if (timer >= jumpTime - idleTransitionTime && idleStart)
                    {
                        IsHanging = false;
                        animator.CrossFade("FallIdle", 0.6f);
                        idleStart = false;
                    }
                }
                else
                {

                    IsHanging = true;

                    animator.speed = 1;
                    playerController.IsInAir = false;
                    var handPos = climbController.rightHand.boneTransform;

                    if (camShake)
                    {
                        playerController.OnLand?.Invoke(Mathf.Clamp((startPos.y - transform.position.y) * 0.003f, 0f, 0.05f), 0.7f);

                        camShake = false;
                    }

                    if (climbController.rightHand.boneTransform == Vector3.zero)
                        handPos = animator.GetBoneTransform(HumanBodyBones.RightHand).position;

                    var offset = right - handPos;
                    //transform.position += offset;
                    transform.position = Vector3.MoveTowards(transform.position, transform.position + offset, 10f * Time.deltaTime);

                    //freeHang Jump
                    comboTime += Time.deltaTime;

                    if (climbController.currentPoint.transform.parent.CompareTag("SwingableLedge") && comboTime >= (animState.length - transitionTime) * (0.3f)) // peak Time percentage
                    {
                        var cam = Camera.allCameras.First(c => c.enabled).transform;
                        var dir = (transform.position - cam.position).normalized;
                        if (environmentScanner.alwaysUsePlayerForward) dir = transform.forward;
                        dir.y = 0f;
                        var angleBWvec = Vector3.SignedAngle(transform.forward, new Vector3(dir.x, 0, dir.z), Vector3.down);

                        if (Mathf.Abs(angleBWvec) > 130 || Mathf.Abs(angleBWvec) < 60)

                            if ((DoCombo && player.MoveDir != Vector3.zero) || DoQuickCombo)
                            {
                                //var camereController = Camera.main.GetComponent<CameraController>();
                                //var dir = camereController.PlanarRotation * Vector3.forward;

                                var moveDir = ((locomotionInputManager.DirectionInput.x * cam.right) + (locomotionInputManager.DirectionInput.y * cam.forward)).normalized;

                                dir = DoQuickCombo ? dir : moveDir;

                                //var jumpData = GetComponent<EnvironmentScanner>().FindPointToJump(transform.forward, 4.5f, 1f, false);  //if you just want forward movement
                                var jumpData = environmentScanner.FindPointToJump(dir, 4.5f, 1f, false);
                                if (jumpData != null)
                                {
                                    if (jumpData.isClimbable && (!jumpData.jumpPointFound || (jumpData.jumpPointFound && jumpData.climbHitData.point.y > jumpData.rootPosition.y)))
                                    {
                                        if (jumpData.climbHitData.point.y - transform.position.y < 2f)
                                        {
                                            if (climbController.ClimbToLedge(jumpData.climbHitData.transform, jumpData.climbHitData.point, combo: true))//transform.position);
                                                yield break;
                                        }
                                    }
                                    if (jumpData.jumpPointFound)
                                    {
                                        //IsHanging = false;
                                        //parkourController.ResetRootMotion();
                                        StartCoroutine(DoPredictiveBackJump(jumpData, combo: true));
                                        yield break;
                                    }
                                }
                            }
                    }
                    else if (inputManager.JumpFromHang && climbController.hangType == HangType.freeHang) DoCombo = true;
                    else if (inputManager.Jump && climbController.hangType == HangType.freeHang) DoQuickCombo = true;

                }

                if (!playerController.PreventRotation)
                    transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRot, RotationSpeed * Time.deltaTime);

                timer += Time.deltaTime;
                yield return null;
            }
            if (climbController.IsInFocus)
                transform.rotation = targetRot;
            else
                IsHanging = false;
            playerController.IsInAir = false;
            inAirOfPredictiveJump = false;
            ResetRootMotion();
            inPredictiveJump = false;
            InAction = false;
                
        }

        void JumpGroundCheck(Vector3 deltaPosition)
        {
            var jumpingFoot = mirrorJump ? HumanBodyBones.LeftFoot : HumanBodyBones.RightFoot;

            var footTransform = animator.GetBoneTransform(jumpingFoot);
            var footPos = footTransform.position;

            if (!animator.GetBool(AnimatorParameters.isFalling))
            {
                footPos = footTransform.TransformPoint(footColliderOffset);
            }

            isGrounded = Physics.CheckSphere(footPos, footColliderRadius, environmentScanner.ObstacleLayer);
            if (Physics.Linecast(transform.position - deltaPosition, transform.position, out RaycastHit hit, environmentScanner.ObstacleLayer))
            {
                transform.position = hit.point;
                isGrounded = true;
            }

            //isGrounded = Physics.CheckSphere(transform.position + Vector3.up * 0.1f, 0.1f, environmentScanner.ObstacleLayer);
        }

        public void HandlePredictiveBackJump(Vector3 checkDir)
        {
            var jumpData = GetComponent<EnvironmentScanner>().FindPointToJump(checkDir, 4.5f, 1f, false);
            if (jumpData != null)
            {
                //return;
                if (jumpData.isClimbable && (!jumpData.jumpPointFound || (jumpData.jumpPointFound && jumpData.climbHitData.point.y > jumpData.rootPosition.y)))
                {
                    if (jumpData.climbHitData.point.y - transform.position.y < 2f)
                    {
                        animator.SetFloat(AnimatorParameters.BackJumpDir, 0);
                        animator.SetBool(AnimatorParameters.BackJumpMode, false);
                        if (climbController.ClimbToLedge(jumpData.climbHitData.transform, jumpData.climbHitData.point, checkDirection: checkDir))//transform.position);
                            return;
                    }
                }
                if (jumpData.jumpPointFound)
                {
                    animator.SetFloat(AnimatorParameters.BackJumpDir, 0);
                    animator.SetBool(AnimatorParameters.BackJumpMode, false);
                    //parkourController.ResetRootMotion();
                    StartCoroutine(DoPredictiveBackJump(jumpData));
                    return;
                }
            }
        }

        IEnumerator HandleWallRun(ObstacleHitData hitData)
        {
            if (!Physics.SphereCast(transform.position + Vector3.up * 0.3f, 0.1f, Vector3.down, out RaycastHit hit, 0.5f, environmentScanner.ObstacleLayer))
                yield break;

            InAction = true;
            EnableRootMotion();
            inPredictiveJump = true;

            player.OnStartSystem(this);
            if (playerController.WaitToStartSystem) yield return new WaitUntil(() => playerController.WaitToStartSystem == false);

            animator.CrossFade("Wall Run", 0.1f);
            yield return null;
            var animState = animator.GetNextAnimatorStateInfo(0);

            var time = 0f;
            while (time <= animState.length)
            {

                MatchTarget(hitData.forwardHit.point, Quaternion.LookRotation(-hitData.forwardHit.normal), AvatarTarget.LeftFoot, new MatchTargetWeightMask(new Vector3(0, 0, 1), 0),
                        0.1f, 0.3f);

                if (inputManager.JumpKeyDown)
                    HandlePredictiveJumpAndClimb(-transform.forward);

                if (inAirOfPredictiveJump) yield break;

                transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(-hitData.forwardHit.normal),
                    200f * Time.deltaTime);

                time += Time.deltaTime;
                yield return null;
            }
            climbController.isFalling = true;
            InAction = false;
            inPredictiveJump = false;

            player.OnEndSystem(this);
            DisableRootMotion();
        }

        public IEnumerator DoParkourAction(ParkourAction action)
        {
            TargetMatchParams matchParams = null;

            if (action.EnableTargetMatching)
            {
                matchParams = new TargetMatchParams()
                {
                    pos = action.MatchPos,
                    startTime = action.MatchStartTime,
                    endTime = action.MatchTargetTime,
                    target = action.MatchBodyPart,
                    posWeight = action.MatchPosWeight
                };
            }

            matchPosGlobal = action.MatchPos;

            player.OnStartSystem(this);
            if (playerController.WaitToStartSystem) yield return new WaitUntil(() => playerController.WaitToStartSystem == false);

            yield return DoAction(action.AnimName, action.RotateToObstacle, action.TargetRotation,
                matchParams, action.PostActionDelay, action.Mirror);
            player.OnEndSystem(this);

        }

        public IEnumerator DoAction(string anim, bool rotate = false,
        Quaternion targetRot = new Quaternion(), TargetMatchParams matchParams = null,
        float postDelay = 0f, bool mirror = false, Action onComplete = null, float crossFadeTime = .2f)
        {
            InAction = true;
            EnableRootMotion();
            animator.SetBool(AnimatorParameters.mirrorAction, mirror);

            //animator.CrossFadeInFixedTime(anim, 0.2f
            //yield return new WaitForEndOfFrame();
            //yield return new WaitForEndOfFrame();

            if (matchParams != null) Mathf.Min(crossFadeTime, matchParams.startTime + 0.02f);
            animator.CrossFade(anim, crossFadeTime);

            yield return null;
            //animator.Update(0);

            var animState = animator.GetNextAnimatorStateInfo(0);

            float timer = 0f;
            while (timer <= animState.length)
            {
                timer += Time.deltaTime;
                float normalTime = timer / animState.length;

                if (matchParams != null)
                {
                    MatchTarget(matchParams.pos, matchParams.rot, matchParams.target, new MatchTargetWeightMask(matchParams.posWeight, 0),
                        matchParams.startTime, matchParams.endTime);

                    if (rotate && normalTime >= matchParams.startTime)
                        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRot,
                            RotationSpeed * Time.deltaTime);
                }
                else if (rotate)
                {
                    transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRot,
                        RotationSpeed * Time.deltaTime);
                }

                if (animator.IsInTransition(0) && timer > 0.5f)
                    break;

                yield return null;
            }

            yield return new WaitForSeconds(postDelay);

            InAction = false;
            ResetRootMotion();

            onComplete?.Invoke();
        }

        /// <summary>
        /// LedgeMovement will stop the player from moving if there is ledge in front.
        /// Pass your moveDir and velocity to the LedgeMovment function and it will return the new moveDir and Velocity while also taking ledges to the account
        /// </summary>
        /// <param name="currMoveDir"></param>
        /// <param name="currVelocity"></param>
        /// <returns></returns>
        /// 
        bool ledgeFound;

        RaycastHit lastRightHit, lastLeftHit;
        public (Vector3, Vector3) LedgeMovement(Vector3 currMoveDir, Vector3 currVelocity)
        {
            if (currMoveDir == Vector3.zero) return (currMoveDir, currVelocity);

            float yOffset = 0.5f;
            float xOffset = 0.4f;
            float forwardOffset = xOffset / 2f; // can control moveAngle here


            var radius = xOffset / 2; // can control moveAngle here


            if (animator.GetFloat(AnimatorParameters.idleType) > 0.5f)
            {
                xOffset = 0.2f;
                radius = xOffset / 2f;       // crouch angle
                forwardOffset = xOffset / 2f; // can control moveAngle here
            }

            float maxAngle = 60f;
            float velocityMag = currVelocity.magnitude;
            var moveDir = currMoveDir;
            RaycastHit rightHit, leftHit, newHit;
            var positionOffset = transform.position + currMoveDir * xOffset;
            var rigthVec = Vector3.Cross(Vector3.up, currMoveDir);
            var rightLeg = transform.position + currMoveDir * forwardOffset + rigthVec * xOffset / 2; //animator.GetBoneTransform(HumanBodyBones.RightFoot).position;
            var leftLeg = transform.position + currMoveDir * forwardOffset - rigthVec * xOffset / 2; //animator.GetBoneTransform(HumanBodyBones.LeftFoot).position;
            Debug.DrawRay(positionOffset + Vector3.up * yOffset, Vector3.down);
            Debug.DrawRay(rightLeg + Vector3.up * yOffset, Vector3.down);
            Debug.DrawRay(leftLeg + Vector3.up * yOffset, Vector3.down);

            var rightFound = (Physics.Raycast(rightLeg + Vector3.up * yOffset, Vector3.down, out rightHit, yOffset + environmentScanner.ledgeHeightThreshold, environmentScanner.ObstacleLayer) && (rightHit.distance - yOffset) < environmentScanner.ledgeHeightThreshold && Vector3.Angle(Vector3.up, rightHit.normal) < maxAngle);
            var leftFound = (Physics.Raycast(leftLeg + Vector3.up * yOffset, Vector3.down, out leftHit, yOffset + environmentScanner.ledgeHeightThreshold, environmentScanner.ObstacleLayer) && (leftHit.distance - yOffset) < environmentScanner.ledgeHeightThreshold && Vector3.Angle(Vector3.up, leftHit.normal) < maxAngle);

            if (!rightFound) positionOffset += rigthVec * xOffset / 2;
            if (!leftFound) positionOffset -= rigthVec * xOffset / 2;

            //var radius = xOffset / 3; // can control moveAngle here

            //if (!rightFound && !leftFound)
            //    radius = xOffset / 2;

            isOnLedge = false;

            if (!(Physics.SphereCast(positionOffset + Vector3.up * yOffset /* + Vector3.up * radius */, radius, Vector3.down, out newHit, yOffset + environmentScanner.ledgeHeightThreshold, environmentScanner.ObstacleLayer)) || ((newHit.distance - yOffset) > environmentScanner.ledgeHeightThreshold && Vector3.Angle(Vector3.up, newHit.normal) > maxAngle))
            {
                isOnLedge = true;

                if (!rightFound || !leftFound)
                {
                    if (!(!rightFound && !leftFound)) //to restrict rot
                        currMoveDir = Vector3.zero;
                    currVelocity = Vector3.zero;
                }
            }
            else if (!rightFound || !leftFound)
            {
                if (rightFound)
                    currVelocity = (newHit.point - leftLeg).normalized * velocityMag;
                else if (leftFound)
                    currVelocity = (newHit.point - rightLeg).normalized * velocityMag;
                else if ((rightHit.transform != null && Vector3.Angle(Vector3.up, rightHit.normal) > maxAngle) || (leftHit.transform != null && Vector3.Angle(Vector3.up, leftHit.normal) > maxAngle))
                    currVelocity = Vector3.zero;

                //currVelocity.y = 0;
                //currMoveDir = currVelocity.normalized;
                //currMoveDir = Vector3.RotateTowards(moveDir, currVelocity.normalized, 10f * Time.deltaTime, 0f);
            }
            //environmentScanner.DrawAxis(newHit.point, 0.1f, Color.green);
            //environmentScanner.DrawAxis(positionOffset, xOffset / 2, Color.yellow);
            //Debug.DrawRay(transform.position, currVelocity, Color.blue);
            if (currVelocity == Vector3.zero)
                return (currMoveDir, currVelocity);
            return (new Vector3(currVelocity.x, 0, currVelocity.z), currVelocity);
        }

        Vector3 matchPosGlobal;
        void MatchTarget(Vector3 matchPos, Quaternion rotation, AvatarTarget target, MatchTargetWeightMask weightMask, float startTime, float endTime)
        {
            if (animator.isMatchingTarget || animator.IsInTransition(0)) return;

            //matchPosGlobal = matchPos;
            animator.MatchTarget(matchPos, rotation, target, weightMask, startTime, endTime);
        }

        bool prevRootMotionVal;

        public void EnableRootMotion()
        {
            prevRootMotionVal = player.UseRootMotion;
            player.UseRootMotion = true;
        }

        public void DisableRootMotion()
        {
            prevRootMotionVal = player.UseRootMotion;
            player.UseRootMotion = false;
        }

        public void ResetRootMotion()
        {
            player.UseRootMotion = prevRootMotionVal;
        }

        private void OnDrawGizmosSelected()
        {
            if (inAirOfPredictiveJump)
            {
                Gizmos.color = new Color(0, 1, 0, 0.5f);

                var jumpingFoot = mirrorJump ? HumanBodyBones.LeftFoot : HumanBodyBones.RightFoot;
                //Gizmos.DrawSphere(animator.GetBoneTransform(jumpingFoot).TransformPoint(footColliderOffset), 0.15f);
            }
        }

        bool enableBalanceWalk;
        public void HandleBalanceWalk()
        {
            float crouchVal;
            float footOffset = .1f;
            float footRayHeight = .8f;
            //Crouch
            if (enableBalanceWalk)
            {
                var hitObjects = Physics.OverlapSphere(transform.TransformPoint(new Vector3(0f, 0.15f, 0.07f)), .2f).ToList().Where(g => g.gameObject.tag == "NarrowBeam" || g.gameObject.tag == "SwingableLedge").ToArray();
                crouchVal = hitObjects.Length > 0 ? 1f : 0;
                animator.SetFloat(AnimatorParameters.idleType, crouchVal, 0.2f, Time.deltaTime);

                if (animator.GetFloat(AnimatorParameters.idleType) > .2f)
                {
                    var leftFootHit = Physics.SphereCast(transform.position - transform.forward * 0.3f + Vector3.up * footRayHeight / 2, 0.1f, Vector3.down, out RaycastHit leftHit, footRayHeight + footOffset, environmentScanner.ObstacleLayer);
                    var rightFootHit = Physics.SphereCast(transform.position + transform.forward * 0.3f + Vector3.up * footRayHeight / 2, 0.1f, Vector3.down, out RaycastHit rightHit, footRayHeight + footOffset, environmentScanner.ObstacleLayer);
                    var hasSpace = leftFootHit && rightFootHit;
                    animator.SetFloat(AnimatorParameters.crouchType, hasSpace ? 0 : 1, 0.2f, Time.deltaTime);
                }
            }
        }

        //private void Reset()
        //{
        //    priority = 9;
        //    playerController = GetComponent<PlayerController>();
        //    if (playerController)
        //        playerController.Register(this,true);
        //}


        private void OnGUI()
        {
            //var style = new GUIStyle() { fontSize = 28 };
            // GUILayout.Label("In Action = " + InAction, style);
            //GUILayout.Label("Has Control = " + guishow + animator.isMatchingTarget, style);
            //GUILayout.Label("target matching = " + animator.isMatchingTarget, style);
            //if (animator.GetCurrentAnimatorClipInfo(0).Length > 0)
            //    GUILayout.Label("" + animator.GetCurrentAnimatorClipInfo(0)?[0].clip?.name, style);
            //if (animator.GetNextAnimatorClipInfo(0).Length > 0)
            //GUILayout.Label("" + animator.GetNextAnimatorClipInfo(0)[0].clip.name, style);
        }
        private void OnDrawGizmos()
        {
            //environmentScanner?.actions?.Reverse();
            //environmentScanner?.actions?.ForEach(x => x.Invoke());

            //GizmosExtend.Show();
            //GizmosExtend.Clear();
            //if (_jumpPoint != null)
            //{
            //    Gizmos.color = new Color(0, 1, 0, 0.5f);
            //    Gizmos.DrawSphere(_jumpPoint.rootPosition, 0.1f);

            //    Gizmos.color = new Color(0, 0, 1, 0.5f);
            //    Gizmos.DrawSphere(_jumpPoint.footPosition, 0.1f);
            //}
            //Gizmos.color = Color.black;

            //if (matchPosGlobal != Vector3.zero)
            //    Gizmos.DrawSphere(matchPosGlobal, 0.2f);
        }
    }
}
