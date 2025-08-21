using System;
using System.Collections;
using System.Linq;
using UnityEngine;
namespace FS_ThirdPerson
{
    public static partial class AnimatorParameters
    {
        public static int moveAmount = Animator.StringToHash("moveAmount");
        public static int strafeAmount = Animator.StringToHash("strafeAmount");
        public static int IsGrounded = Animator.StringToHash("IsGrounded");
        public static int fallAmount = Animator.StringToHash("fallAmount");
        public static int idleType = Animator.StringToHash("idleType");
        public static int crouchType = Animator.StringToHash("crouchType");
        public static int rotation = Animator.StringToHash("rotation");
        public static int turnback_Mirror = Animator.StringToHash("turnback Mirror");
        public static int runToStopAmount = Animator.StringToHash("RunToStopAmount");
    }

    public class LocomotionController : SystemBase, ICharacter
    {
        [Header("Movement Parameters")]

        [SerializeField] float sprintSpeed = 6.5f;
        [SerializeField] float runSpeed = 4.5f;
        [SerializeField] float walkSpeed = 2f;
        [SerializeField] float rotationSpeed = 2.5f;

        [field: Space(2)]
        public float acceleration = 8f;
        public float deceleration = 6f;

        [field: Space(2)]
        [Tooltip("Disables Sprinting")]
        public bool enableSprint = true;
        [Tooltip("sets running to default")]
        public bool setDefaultStateToRunning = true;

        [field: Space(2)]
        [Header("Additional Features")]

        public bool useMultiDirectionalAnimation;

        [Tooltip("When set to 0 player faces moving Direction. When set to 1 player faces camera Direction")]
        [Range(0, 1)]
        public float playerDirectionBlend = 1f;

        [Tooltip("If true, then player will always face the camera's forward even in idle")]
        public bool faceCameraForwardWhenIdle;

        [Tooltip("Use this multiplier if you want different speed for multi directional animations")]
        public float speedMultiplier = 0.8f;

        [Tooltip("Only sprints if player is facing forward by this percentage")]
        [Range(0, 1)]
        public float sprintDirectionThreshold = 0.9f;


        [Range(0, 1)]
        public float forwardHipRotationBlend = 0.5f;
        public bool rotateHipForBackwardAnimation = true;
        [Range(0, 1)]
        public float backwardHipRotationBlend = 0.8f;

        Vector3 currentSpeed;


        public bool verticalJump;

        [Tooltip("Defines how long it takes for the character to reach the peak of the jump")]
        public float timeToJump = 0.4f;
        [Tooltip("Movement speed while the player is in the air")]
        public float jumpMoveSpeed = 4f;

        float moveSpeed = 0;

        [field: Space(3)]
        [field: Tooltip("Automatically stopping movement on ledges")]
        [SerializeField] bool preventFallingFromLedge = true;
        [field: Tooltip("Sliding movement threshold on ledges")]
        [Range(-1, 1)]
        [SerializeField] float slidingMovementThresholdFromLedge = -1f;
        [field: Tooltip("Prevents player rotation at ledges when not able to move")]
        [SerializeField] bool preventLedgeRotation = false;

        [field: Tooltip("Prevents from near walls during locomotion")]
        [SerializeField] bool preventWallSlide = false;
        [field: Tooltip("Enables balance walking on narrow beams")]
        [field: Space(3)]
        public bool enableBalanceWalk = true;
        public BalanceWalkDetectionType balanceWalkDetectionType = BalanceWalkDetectionType.Dynamic;
        public enum BalanceWalkDetectionType { Dynamic, Tagged, Both }

        [Header("Optional Animations")]
        [field: Tooltip("plays turning animations")]
        public bool enableTurningAnim = true;
        [field: Tooltip("plays quick turn animation")]
        public bool playQuickTurnAnimation = true;
        [field: Tooltip("plays quick turn animation only above this moveAmount threshhold")]
        public float QuickTurnThreshhold = -0.01f;
        [field: Tooltip("plays quick stop animation")]
        public bool playQuickStopAnimation = false;
        [field: Tooltip("plays quick stop animation only above this moveAmount threshhold")]
        public float runToStopThreshhold = 0.4f;


        [Header("Ground Check Settings")]
        [Tooltip("Radius of ground detection sphere")]
        [SerializeField] float groundCheckRadius = 0.2f;

        [Tooltip("Offet between the player's root position and the ground detection sphere")]
        [SerializeField] Vector3 groundCheckOffset = new Vector3(0f, 0.15f, 0.07f);

        [Tooltip("All layers that should be considered as ground")]
        public LayerMask groundLayer = 1;

        float controllerDefaultHeight = .87f;
        float controllerDefaultYOffset = 1.7f;

        bool isGrounded;

        Vector3 desiredMoveDir;
        Vector3 moveInput;
        Vector3 moveDir;
        float moveAmount;
        Vector3 velocity;

        float ySpeed;
        Quaternion targetRotation;

        float rotationValue = 0;
        float crouchVal = 0;
        float DynamicCrouchVal = 0;
        float footOffset = .1f;
        float footRayHeight = .8f;

        bool turnBack;
        bool useRootMotion;
        bool useRootmotionMovement;

        Vector3 prevAngle;
        bool prevValue;

        float headIK;

        bool preventLocomotion;

        float addedMomentum = 0f;

        float jumpHeightDiff;
        float minJumpHeightForHardland = 3f;
        float jumpMaxPosY;

        float headHeightThreshold = .75f;
        float sprintModeTimer = 0;

        public Vector3 GroundCheckOffset
        {
            get { return groundCheckOffset; }
            set { groundCheckOffset = value; }
        }

        public float GroundCheckRadius => groundCheckRadius;

        public float MoveAmount => animator.GetFloat(AnimatorParameters.moveAmount);

        public override SystemState State => SystemState.Locomotion;


        FootIK footIk;
        PlayerController playerController;
        GameObject cameraGameObject;
        CharacterController characterController;
        Animator animator;
        EnvironmentScanner environmentScanner;
        LocomotionInputManager inputManager;
        private void Awake()
        {
            _walkSpeed = walkSpeed;
            _runSpeed = runSpeed;
            _sprintSpeed = sprintSpeed;
        }

        void Start()
        {
            playerController = GetComponent<PlayerController>();
            cameraGameObject = playerController.cameraGameObject;
            animator = GetComponent<Animator>();
            environmentScanner = GetComponent<EnvironmentScanner>();
            characterController = GetComponent<CharacterController>();
            inputManager = GetComponent<LocomotionInputManager>();
            controllerDefaultHeight = characterController.height;
            controllerDefaultYOffset = characterController.center.y;
            footIk = GetComponent<FootIK>();
            if (!(groundLayer == (groundLayer | (1 << LayerMask.NameToLayer("Ledge")))))
                groundLayer += 1 << LayerMask.NameToLayer("Ledge");
        }

        private void OnAnimatorIK(int layerIndex)
        {
            var hipPos = animator.GetBoneTransform(HumanBodyBones.Hips).transform;
            var headPos = animator.GetBoneTransform(HumanBodyBones.Head).transform.position;


            var offset = Vector3.Distance(hipPos.position, headPos);
            animator.SetLookAtPosition(cameraGameObject.transform.position + cameraGameObject.transform.forward * (5f) + new Vector3(0, offset, 0));
            //animator.SetLookAtWeight(headIK);
        }

        public override void HandleFixedUpdate()
        {
            if (enableBalanceWalk)
            {
                if (balanceWalkDetectionType == BalanceWalkDetectionType.Dynamic)
                    HandleBalanceOnNarrowBeam();
                else if (balanceWalkDetectionType == BalanceWalkDetectionType.Tagged)
                    HandleBalanceOnNarrowBeamWithTag();
                else if (balanceWalkDetectionType == BalanceWalkDetectionType.Both)
                {
                    if (DynamicCrouchVal < 0.5f)
                        HandleBalanceOnNarrowBeamWithTag();
                    if (crouchVal < 0.5f || DynamicCrouchVal > 0.5f)
                    {
                        HandleBalanceOnNarrowBeam();
                        DynamicCrouchVal = crouchVal;
                    }
                }
            }
        }

        private void Update()
        {
            GetInput();
#if UNITY_ANDROID || UNITY_IOS
            if (setDefaultStateToRunning)
            {
                if (moveAmount == 1)
                    sprintModeTimer += Time.deltaTime;
                else
                    sprintModeTimer = 0;
            }
#endif
        }

        public override void HandleUpdate()
        {
            if (preventLocomotion || UseRootMotion)
            {
                ySpeed = Gravity / 4;
                return;
            }

            animator.SetFloat("locomotionType", useMultiDirectionalAnimation ? 1 : 0);

            var wasGroundedPreviously = isGrounded;
            GroundCheck();

            if (isGrounded && !wasGroundedPreviously)
            {
                if (ySpeed < Gravity)
                {
                    playerController.OnLand?.Invoke(Mathf.Clamp(Mathf.Abs(ySpeed) * 0.0007f, 0.0f, 0.01f), 1f);
                    animator.SetFloat(AnimatorParameters.fallAmount, Mathf.Clamp(Mathf.Abs(ySpeed) * 0.06f, 0.6f, 1f));
                    StartCoroutine(DoLocomotionAction("Landing", true));
                }
                else
                    animator.SetFloat(AnimatorParameters.fallAmount, 0);
            }

            velocity = Vector3.zero;

            if (inputManager.JumpKeyDown)
            {
                VerticalJump();
            }

            if (isGrounded)
            {
                ySpeed = Gravity / 2;
                //footIk.IkEnabled = true;

                setDefaultStateToRunning = inputManager.ToggleRun ? !setDefaultStateToRunning : setDefaultStateToRunning;

                float normalizedSpeed = setDefaultStateToRunning ? 1 : .2f;

                if (enableSprint)
                    normalizedSpeed = (inputManager.SprintKey || sprintModeTimer > 2f) ? 1.5f : normalizedSpeed;
                else
                    normalizedSpeed = (inputManager.SprintKey || sprintModeTimer > 2f) ? 1f : normalizedSpeed;

                //moveSpeed = normalizedSpeed == 1 ? runSpeed : walkSpeed;
                //moveSpeed = normalizedSpeed == 1.5f ? sprintSpeed : moveSpeed;
                var curSpeedDir = currentSpeed;
                curSpeedDir.y = 0;
                moveSpeed = normalizedSpeed == 0.2f ? walkSpeed : runSpeed;

                var SprintDir = Vector3.Dot(curSpeedDir.normalized, transform.forward);

                SprintDir = SprintDir > sprintDirectionThreshold ? SprintDir : 0;

                moveSpeed = normalizedSpeed == 1.5f ? Mathf.Lerp(moveSpeed, sprintSpeed, Mathf.Clamp01(SprintDir)) : moveSpeed;

                var currentRunSpeed = runSpeed;


                if (crouchVal == 1)
                    moveSpeed *= .6f;
                else if (useMultiDirectionalAnimation)
                {
                    moveSpeed *= speedMultiplier;
                    currentRunSpeed *= speedMultiplier;
                }

                animator.SetFloat(AnimatorParameters.idleType, crouchVal, 0.5f, Time.deltaTime);

                velocity = desiredMoveDir * moveSpeed;


                if (enableTurningAnim)
                    HandleTurning();

                if (inputManager.Drop && MoveDir != Vector3.zero && !preventLocomotion && IsGrounded && isOnLedge)
                {
                    var hitData = environmentScanner.ObstacleCheck(performHeightCheck: false);
                    if (!hitData.forwardHitFound && !Physics.Raycast(transform.position + Vector3.up * 0.1f, transform.forward, 0.5f, environmentScanner.ObstacleLayer))
                    {
                        StartCoroutine(DoLocomotionAction("Jump Down", useRootmotionMovement: true, targetRotation: Quaternion.LookRotation(MoveDir)));
                        isOnLedge = false;
                        animator.SetBool(AnimatorParameters.IsGrounded, isGrounded = false);
                        return;
                    }
                }

                if (velocity.magnitude != 0)
                    currentSpeed = Vector3.MoveTowards(currentSpeed, velocity, acceleration * Time.deltaTime);
                else
                    currentSpeed = Vector3.MoveTowards(currentSpeed, Vector3.zero, deceleration * Time.deltaTime);

                var characterVelocity = characterController.velocity;
                characterVelocity.y = 0;

                float forwardSpeed = Vector3.Dot(characterVelocity, transform.forward);
                animator.SetFloat(AnimatorParameters.moveAmount, forwardSpeed / currentRunSpeed, 0.2f, Time.deltaTime);

                float strafeSpeed = Vector3.Dot(characterVelocity, transform.right);
                animator.SetFloat(AnimatorParameters.strafeAmount, strafeSpeed / currentRunSpeed, 0.2f, Time.deltaTime);

                // If we're playing running animation but the velocity is close to zero, then play run to stop action
                if (playQuickStopAnimation && ((MoveAmount > runToStopThreshhold && velocity.magnitude == 0) ||
                    (forwardSpeed / currentRunSpeed < 0.1f && MoveAmount > runToStopThreshhold)) && animator.GetFloat(AnimatorParameters.idleType) < 0.2f)
                {
                    currentSpeed = Vector3.zero;
                    animator.SetBool(AnimatorParameters.turnback_Mirror, strafeSpeed > 0.03f);
                    animator.SetFloat(AnimatorParameters.runToStopAmount, MoveAmount);
                    StartCoroutine(DoLocomotionAction("Run To Stop", useRootmotionMovement: false, crossFadeTime: 0.3f, onComplete: () =>
                    {
                        animator.SetFloat(AnimatorParameters.moveAmount, 0);
                    }));
                }
                else if (playQuickTurnAnimation)
                {
                    Turnback();
                }
            }
            else
            {
                //footIk.IkEnabled = false;
                ySpeed = Mathf.Clamp(ySpeed + Gravity * Time.deltaTime, -30, Mathf.Abs(Gravity) * timeToJump);
            }
            currentSpeed.y = 0;

            // LedgeMovement will stop the player from moving if there is ledge in front.
            // Pass your moveDir and velocity to the LedgeMovment function and it will return the new moveDir and Velocity while also taking ledges to the account
            if (preventFallingFromLedge && playerController.PreventFallingFromLedge)
            {
                var (ledgeMoveDir, ledgeCurrentSpeed) = LedgeMovement(currentSpeed.normalized, currentSpeed);

                if (Vector3.Dot(currentSpeed.normalized, ledgeMoveDir.normalized) >= slidingMovementThresholdFromLedge - 0.02f)
                {
                    moveDir = ledgeMoveDir;
                    currentSpeed = ledgeCurrentSpeed;
                }
                else
                {
                    moveDir = ledgeMoveDir;
                    currentSpeed = Vector3.zero;
                }
            }
            else
            {
                moveDir = currentSpeed.normalized;
            }

            velocity.y = ySpeed;

            currentSpeed.y = ySpeed;
            //if (currentSpeed != Vector3.zero)
            characterController.Move(currentSpeed * Time.deltaTime);
            currentSpeed.y = 0;

            if (preventWallSlide && characterController.velocity.magnitude > 0.05f)
            {
                moveDir = characterController.velocity;
                moveDir.y = 0;
            }

            if (!playerController.PreventRotation)
            {
                setTargetRotation(moveDir, ref targetRotation);

                float turnSpeed = Mathf.Lerp(rotationSpeed * 100f, 2 * rotationSpeed * 100f, moveSpeed / runSpeed);
                transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, Time.deltaTime * turnSpeed);
                //transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed * 100f);
            }
            else
                targetRotation = transform.rotation;
        }
        void setTargetRotation(Vector3 moveDir, ref Quaternion targetRotation)
        {
            var cameraForward = cameraGameObject.transform.forward;
            cameraForward.y = 0;

            var dotProd = 1f;

            if (useMultiDirectionalAnimation && rotateHipForBackwardAnimation)
                dotProd = Vector3.Dot(cameraForward, moveDir) + 0.35f;

            if (moveDir.magnitude > 0f)
            {
                if (!useMultiDirectionalAnimation)
                {
                    targetRotation = Quaternion.LookRotation(moveDir);
                    return;
                }

                if (dotProd > 0 || animator.GetFloat(AnimatorParameters.idleType) > 0.5f)
                    targetRotation = Quaternion.LookRotation(Vector3.Lerp(moveDir, cameraForward, playerDirectionBlend - animator.GetFloat(AnimatorParameters.idleType)));
                else
                    targetRotation = Quaternion.LookRotation(Vector3.Lerp(-moveDir, cameraForward, playerDirectionBlend - backwardHipRotationBlend - animator.GetFloat(AnimatorParameters.idleType)));

                var playerMoveDirDot = Vector3.Dot(targetRotation * Vector3.forward, moveDir.normalized) - (1 - forwardHipRotationBlend);
                targetRotation = Quaternion.LookRotation(Vector3.Lerp(targetRotation * Vector3.forward, moveDir, playerMoveDirDot - animator.GetFloat(AnimatorParameters.idleType)));
            }
            else if (faceCameraForwardWhenIdle)
            {
                targetRotation = Quaternion.LookRotation(cameraForward);
            }
        }

        void HandleBalanceOnNarrowBeam()
        {
            bool leftFootHit, rightFootHit;
            int hitCount = 0;

            Vector3 right = transform.right * 0.3f, forward = transform.forward * 0.3f, up = Vector3.up * 0.2f;

            hitCount += Physics.CheckCapsule(transform.position - right + up, transform.position - right - up, 0.1f, groundLayer) ? 1 : 0;
            hitCount += Physics.CheckCapsule(transform.position + right + up, transform.position + right - up, 0.1f, groundLayer) ? 1 : 0;
            hitCount += (rightFootHit = Physics.CheckCapsule(transform.position + forward + up, transform.position + forward - up, 0.1f, groundLayer)) ? 1 : 0;
            hitCount += (leftFootHit = Physics.CheckCapsule(transform.position - forward + up, transform.position - forward - up, 0.1f, groundLayer)) ? 1 : 0;


            if ((rightFootHit || leftFootHit) && !Physics.Linecast(transform.position + up, transform.position - up, groundLayer)) // for predictive jump cases
                hitCount -= 1;
            crouchVal = hitCount > 2 ? 0f : 1f;
            animator.SetFloat(AnimatorParameters.idleType, crouchVal, 0.2f, Time.deltaTime);
            if (animator.GetFloat(AnimatorParameters.idleType) > .2f)
            {
                var hasSpace = leftFootHit && rightFootHit;
                animator.SetFloat(AnimatorParameters.crouchType, hasSpace ? 0 : 1, 0.2f, Time.deltaTime);
            }
            characterController.center = new Vector3(characterController.center.x, crouchVal == 1 ? controllerDefaultYOffset * .7f : controllerDefaultYOffset, characterController.center.z);
            characterController.height = crouchVal == 1 ? controllerDefaultHeight * .7f : controllerDefaultHeight;
        }
        void HandleBalanceOnNarrowBeamWithTag()
        {
            var hitObjects = Physics.OverlapSphere(transform.TransformPoint(new Vector3(0f, 0.15f, 0.07f)), .2f).ToList().Where(g => g.gameObject.tag == "NarrowBeam" || g.gameObject.tag == "SwingableLedge").ToArray();
            crouchVal = hitObjects.Length > 0 ? 1f : 0;
            animator.SetFloat(AnimatorParameters.idleType, crouchVal, 0.2f, Time.deltaTime);

            if (animator.GetFloat(AnimatorParameters.idleType) > .2f)
            {
                var leftFootHit = Physics.SphereCast(transform.position - transform.forward * 0.3f + Vector3.up * footRayHeight / 2, 0.1f, Vector3.down, out RaycastHit leftHit, footRayHeight + footOffset, groundLayer);
                var rightFootHit = Physics.SphereCast(transform.position + transform.forward * 0.3f + Vector3.up * footRayHeight / 2, 0.1f, Vector3.down, out RaycastHit rightHit, footRayHeight + footOffset, groundLayer);
                var hasSpace = leftFootHit && rightFootHit;
                animator.SetFloat(AnimatorParameters.crouchType, hasSpace ? 0 : 1, 0.2f, Time.deltaTime);
            }
            characterController.center = new Vector3(characterController.center.x, crouchVal == 1 ? controllerDefaultYOffset * .7f : controllerDefaultYOffset, characterController.center.z);
            characterController.height = crouchVal == 1 ? controllerDefaultHeight * .7f : controllerDefaultHeight;
        }

        void HandleTurning()
        {
            var rotDiff = transform.eulerAngles - prevAngle;
            var threshold = moveSpeed >= runSpeed ? 0.025 : 0.1;
            if (rotDiff.sqrMagnitude < threshold)
            {
                rotationValue = 0;
            }
            else
            {
                rotationValue = Mathf.Sign(rotDiff.y) * .5f;
            }

            animator.SetFloat(AnimatorParameters.rotation, rotationValue, 0.35f, Time.deltaTime);

            prevAngle = transform.eulerAngles;


        }

        bool isTurning = false;

        void GetInput()
        {
            float h = inputManager.DirectionInput.x;
            float v = inputManager.DirectionInput.y;

            //moveAmount = Mathf.Clamp01(Mathf.Abs(h) + Mathf.Abs(v));
            moveInput = (new Vector3(h, 0, v));
            moveAmount = moveInput.magnitude;
            desiredMoveDir = playerController.CameraPlanarRotation * moveInput;
            desiredMoveDir = Vector3.ClampMagnitude(desiredMoveDir, 1);
            //if (desiredMoveDir.magnitude < 0.2f)
            //    desiredMoveDir = Vector3.zero;

            //desiredMoveDir = Vector3.MoveTowards(prevDir, cameraController.PlanarRotation * moveInput,Time.deltaTime * rotationSpeed * 2);
            moveDir = desiredMoveDir;

        }

        Quaternion velocityRotation;
        bool Turnback()
        {
            if (moveInput == Vector3.zero || desiredMoveDir == Vector3.zero) return false;

            setTargetRotation(desiredMoveDir, ref velocityRotation);
            var angle = Vector3.SignedAngle(transform.forward, velocityRotation * Vector3.forward, Vector3.up);

            if (Mathf.Abs(angle) > 130 && MoveAmount > QuickTurnThreshhold && animator.GetFloat(AnimatorParameters.idleType) < 0.2f && Physics.Raycast(transform.position + Vector3.up * 0.1f + transform.forward * 0.3f + transform.forward * MoveAmount / 1.5f, Vector3.down, 0.3f) && !Physics.Raycast(transform.position + Vector3.up * 0.1f, transform.forward, 0.6f))
            {
                turnBack = true;
                animator.SetBool(AnimatorParameters.turnback_Mirror, angle <= 0);
                bool isInLocomotionBlendTree = animator.GetCurrentAnimatorStateInfo(0).IsName("Locomotion");

                StartCoroutine(DoLocomotionAction("Running Turn 180", onComplete: () =>
                {

                    //animator.SetFloat(AnimatorParameters.moveAmount, 0.3f);

                    currentSpeed = runSpeed * transform.forward * (MoveAmount);
                    characterController.Move(currentSpeed * Time.deltaTime);

                    targetRotation = transform.rotation;
                    turnBack = false;
                }, crossFadeTime: isInLocomotionBlendTree ? 0.08f : 0.2f, setMoveAmount: true)); ;
                return true;
            }
            return false;
        }


        void GroundCheck()
        {
            isGrounded = Physics.CheckSphere(transform.TransformPoint(groundCheckOffset), groundCheckRadius, groundLayer);
            animator.SetBool(AnimatorParameters.IsGrounded, isGrounded);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(0, 1, 0, 0.5f);
            Gizmos.DrawSphere(transform.TransformPoint(groundCheckOffset), groundCheckRadius);
        }

        public void HandleTurningAnimation(bool enable)
        {
            enableTurningAnim = enable;
            if (!enable)
                animator.SetFloat(AnimatorParameters.rotation, 0);
        }

        public IEnumerator DoLocomotionAction(string anim, bool useRootmotionMovement = false, Action onComplete = null, float crossFadeTime = .2f, Quaternion? targetRotation = null, bool setMoveAmount = false)
        {
            //if (!enableTurningAnim) yield break;
            preventLocomotion = true;
            this.useRootmotionMovement = useRootmotionMovement;
            EnableRootMotion();
            animator.CrossFade(anim, crossFadeTime);

            yield return null;
            var animState = animator.GetNextAnimatorStateInfo(0);

            float timer = 0f;
            while (timer <= animState.length)
            {
                if (!turnBack && Turnback()) yield break;

                if (setMoveAmount)
                    animator.SetFloat(AnimatorParameters.moveAmount, moveAmount * (setDefaultStateToRunning ? 1f : 0.5f), animState.length * 1.3f, 1f * Time.deltaTime);

                if (targetRotation.HasValue && !playerController.PreventRotation) transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation.Value, 500f * Time.deltaTime);

                timer += Time.deltaTime;
                yield return null;
            }
            DisableRootMotion();
            this.useRootmotionMovement = false;
            onComplete?.Invoke();
            preventLocomotion = false;
        }

        public void EnableRootMotion()
        {
            prevValue = useRootMotion;
            useRootMotion = true;
        }

        public void DisableRootMotion()
        {
            prevValue = useRootMotion;
            useRootMotion = false;
        }

        void OnAnimatorMove()
        {
            if (useRootMotion)
            {
                transform.rotation *= animator.deltaRotation;
                if (useRootmotionMovement)
                    transform.position += animator.deltaPosition;
            }
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

        void VerticalJump()
        {
            if (!verticalJump || !IsGrounded) return;
            var headHit = Physics.SphereCast(animator.GetBoneTransform(HumanBodyBones.Head).position, .15f, Vector3.up, out RaycastHit headHitData, headHeightThreshold, environmentScanner.ObstacleLayer);
            if (!headHit)
                StartCoroutine(HandleVerticalJump());
        }

        public IEnumerator HandleVerticalJump()
        {
            yield return new WaitForFixedUpdate();

            if (playerController.CurrentSystemState != State) yield break;

            jumpMaxPosY = transform.position.y - 1;
            var velocity = Vector3.zero;
            //Calculates the initial vertical velocity required for jumping
            var velocityY = Mathf.Abs(Gravity) * timeToJump;
            preventLocomotion = true;
            currentSpeed *= 0.1f;

            animator.SetFloat(AnimatorParameters.moveAmount, 0);
            isGrounded = false;
            animator.CrossFadeInFixedTime("Vertical Jump", .2f);

            //yield return new WaitForSeconds(0.2f);

            while (!isGrounded)
            {
                playerController.IsInAir = true;
                velocityY += Gravity * Time.deltaTime;
                velocity = new Vector3((moveDir * jumpMoveSpeed).x, velocityY, (moveDir * jumpMoveSpeed).z);

                characterController.Move(velocity * Time.deltaTime);
                if (velocityY < 0)
                    GroundCheck();

                // To get max jump height
                if (jumpMaxPosY < transform.position.y)
                    jumpMaxPosY = transform.position.y;

                if (moveDir != Vector3.zero && !playerController.PreventRotation)
                {
                    if (useMultiDirectionalAnimation)
                        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, Time.deltaTime * 100 * rotationSpeed);
                    else
                        transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(moveDir), Time.deltaTime * 100 * rotationSpeed);
                }
                yield return null;

                if (playerController.CurrentSystemState != State)
                    yield break;
            }
            targetRotation = transform.rotation;
            playerController.IsInAir = false;
            yield return VerticalJumpLanding();
            //parkourController.IsJumping = false;
            preventLocomotion = false;
        }

        IEnumerator VerticalJumpLanding()
        {
            jumpHeightDiff = Mathf.Abs(jumpMaxPosY - transform.position.y);
            if (jumpHeightDiff > minJumpHeightForHardland)
            {
                characterController.Move(Vector3.down);
                var halfExtends = new Vector3(.3f, .9f, 0.01f);
                var hasSpaceForRoll = Physics.BoxCast(transform.position + Vector3.up, halfExtends, transform.forward, Quaternion.LookRotation(transform.forward), 2.5f, environmentScanner.ObstacleLayer);

                halfExtends = new Vector3(.1f, .9f, 0.01f);
                var heightHiting = true;
                for (int i = 0; i < 6 && heightHiting; i++)
                    heightHiting = Physics.BoxCast(transform.position + Vector3.up * 1.8f + transform.forward * (i * .5f + .5f), halfExtends, Vector3.down, Quaternion.LookRotation(Vector3.down), 2.2f + i * .1f, environmentScanner.ObstacleLayer);

                OnStartSystem(this);
                EnableRootMotion();
                //if (!hasSpaceForRoll && heightHiting)
                //    yield return DoLocomotionAction("FallingToRoll", crossFadeTime: .1f);
                //else
                yield return DoLocomotionAction("Landing", crossFadeTime: .1f);
                DisableRootMotion();
                OnEndSystem(this);
            }
            else
                animator.CrossFadeInFixedTime("LandAndStepForward", .1f);
        }
        public bool isOnLedge { get; set; }

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
            //Debug.DrawRay(positionOffset + Vector3.up * yOffset, Vector3.down);
            //Debug.DrawRay(rightLeg + Vector3.up * yOffset, Vector3.down);
            //Debug.DrawRay(leftLeg + Vector3.up * yOffset, Vector3.down);

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
                    if (!(!rightFound && !leftFound) && preventLedgeRotation) //to restrict rot
                        currMoveDir = Vector3.zero;
                    currVelocity = Vector3.zero;
                }
            }
            else if ((!rightFound || !leftFound))
            {
                if (rightFound)
                {
                    if (Physics.SphereCast(leftLeg + Vector3.up * yOffset, 0.1f, Vector3.down, out leftHit, yOffset + environmentScanner.ledgeHeightThreshold, environmentScanner.ObstacleLayer))
                        currVelocity = (newHit.point - leftHit.point).normalized * velocityMag;
                    else
                        currVelocity = (newHit.point - leftLeg).normalized * velocityMag;
                }
                else if (leftFound)
                {
                    if (Physics.SphereCast(rightLeg + Vector3.up * yOffset, 0.1f, Vector3.down, out rightHit, yOffset + environmentScanner.ledgeHeightThreshold, environmentScanner.ObstacleLayer))
                        currVelocity = (newHit.point - rightHit.point).normalized * velocityMag;
                    else
                        currVelocity = (newHit.point - rightLeg).normalized * velocityMag;
                }
                else if ((rightHit.transform != null && Vector3.Angle(Vector3.up, rightHit.normal) > maxAngle) || (leftHit.transform != null && Vector3.Angle(Vector3.up, leftHit.normal) > maxAngle))
                    currVelocity = Vector3.zero;
            }

            if (currVelocity == Vector3.zero)
                return (currMoveDir, currVelocity);
            return (new Vector3(currVelocity.x, 0, currVelocity.z), currVelocity);
        }

        #region changeSpeed

        float _walkSpeed;
        float _runSpeed;
        float _sprintSpeed;
        public void ChangeMoveSpeed(float walk, float run, float sprint)
        {
            walkSpeed = walk;
            runSpeed = run;
            sprintSpeed = sprint;
        }

        public void ResetMoveSpeed()
        {
            walkSpeed = _walkSpeed;
            runSpeed = _runSpeed;
            sprintSpeed = _sprintSpeed;
        }

        #endregion

        #region Interface

        public void OnStartSystem(SystemBase systemBase)
        {
            playerController.UnfocusAllSystem();
            systemBase.FocusScript();
            systemBase.EnterSystem();
            preventLocomotion = true;
            currentSpeed *= 0f;
            playerController.SetSystemState(systemBase.State);
            targetRotation = transform.rotation;
            isGrounded = false;
            StartCoroutine(TweenVal(animator.GetFloat(AnimatorParameters.moveAmount), 0, 0.15f, (lerpVal) => { animator.SetFloat(AnimatorParameters.moveAmount, lerpVal); }));
            StartCoroutine(TweenVal(animator.GetFloat(AnimatorParameters.rotation), 0, 0.15f, (lerpVal) => { animator.SetFloat(AnimatorParameters.rotation, lerpVal); }));
            StartCoroutine(TweenVal(animator.GetFloat(AnimatorParameters.idleType), 0, 0.15f, (lerpVal) => { animator.SetFloat(AnimatorParameters.idleType, lerpVal); }));
        }

        public void OnEndSystem(SystemBase systemBase)
        {
            systemBase.UnFocusScript();
            systemBase.ExitSystem();
            playerController.ResetState();
            targetRotation = transform.rotation;
            preventLocomotion = false;
        }
        public Vector3 MoveDir { get { return desiredMoveDir; } set { desiredMoveDir = value; } }
        public bool IsGrounded => isGrounded;
        public float Gravity => -20;
        public bool PreventAllSystems { get; set; } = false;
        public Animator Animator
        {
            get { return animator == null ? GetComponent<Animator>() : animator; }
            set
            {
                animator = value;
            }
        }

        public bool UseRootMotion { get; set; }
        #endregion

        public bool IsCrouching => crouchVal > 0.5f;
    }

    public class TargetMatchParams
    {
        public Vector3 pos;
        public Quaternion rot;
        public AvatarTarget target;
        public AvatarTarget startTarget;
        public float startTime;
        public float endTime;

        public Vector3 startPos;
        public Vector3 endPos;

        public Vector3 posWeight;
    }
}
