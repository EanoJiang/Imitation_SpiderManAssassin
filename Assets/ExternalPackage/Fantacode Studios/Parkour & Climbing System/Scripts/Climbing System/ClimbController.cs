using FS_ThirdPerson;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FS_ParkourSystem
{
    public enum HangType { bracedHang, freeHang }
    [RequireComponent(typeof(ParkourController))]
    public class ClimbController : SystemBase
    {
        [field: Tooltip("Enables Climbing")]
        public bool enableClimbing = true;

        [SerializeField] bool pressInputToClimb = false;

        [Tooltip("Offset between the hand and the climbpoint while bracedhanging. Adjust this value and make sure that the hand is correctly placed on the climbing ledge .")]
        public Vector3 handOffsets = new Vector3(0f, -0.08f, 0.05f);
        //[Tooltip("Offset of the player while freehanging. Some character may need y offsets when freeHanging.")]
        //public float freehangYOffset = 0;

        //public Vector3 handOffsets  { get => animator.GetFloat(AnimatorParameters.freeHang) < 0.5f ? bracedHandOffsets : freehangHandOffsets; }


        [Tooltip("Offset between the foot bone and position on the wall where the foot is place. Adjust this value and make sure that the foot is correctly placed on the wall.")]
        [SerializeField] float footPlacementOffset = 0.15f;

        [Tooltip("Length of the raycast used for finding the foot ik position on the wall during braced climb")]
        [SerializeField] float footIkRayLength = 0.5f;




        public HangType hangType { get => animator.GetFloat(AnimatorParameters.freeHang) < 0.5f ? HangType.bracedHang : HangType.freeHang; }
        //bool turnOnGizmos = false;

        [SerializeField] float hipRayLength = 0.3f;


        Vector3 wallRayOffset = new Vector3(-0.15f, -0.9f, 0.14f);
        Vector3 obstacleRayOffset = new Vector3(0, -0.3f, 0.2f);


        [HideInInspector]
        private ClimbPoint _currentPoint;

        [HideInInspector]
        public ClimbPoint currentPoint
        {
            get { return _currentPoint; }
            set
            {
                if (_currentPoint != null)
                    _currentPoint.hasOwner = false;
                if (value != null)
                    value.hasOwner = true;

                _currentPoint = value;
            }
        }


        [HideInInspector]
        public ClimbPoint previousPoint;

        Transform currentLedge;

        public bool isFalling { get; set; }
        public float isFallingThreshold { get; set; } = 0.2f;
        public float isFallingTimer { get; set; }

        bool ikEnabled = false;

        ParkourInputManager inputManager;
        LocomotionInputManager locomotionInputManager;
        EnvironmentScanner envScanner;
        ICharacter player;
        ParkourController parkourController;
        Animator animator;

        Vector3 lookAtPosition;
        float lookAtWeight;
        PlayerController playerController;

        public override SystemState State { get; } = SystemState.Climbing;

        public void OnEnable()
        {
            player = GetComponent<ICharacter>();
            playerController = GetComponent<PlayerController>();
            animator = player.Animator;
            inputManager = GetComponent<ParkourInputManager>();
            locomotionInputManager = GetComponent<LocomotionInputManager>();
            parkourController = GetComponent<ParkourController>();
            envScanner = GetComponent<EnvironmentScanner>();
        }

        [HideInInspector]
        public IKweights rightHand, leftHand, rightFoot, leftFoot, head;

        [Serializable]
        public class IKweights
        {
            public float current;
            public float target;
            public float weight;
            public ClimbPoint currentPoint;
            public ClimbPoint previousPoint;

            public Vector3 currentIkVecPoint;
            public Vector3 previousIkVecPoint;
            public Vector3 currentVecPoint;
            public Vector3 previousVecPoint;

            public bool currentVecPointOn;
            public bool previousVecPointOn;

            public Vector3 boneTransform;
            public Vector3 bodyPartOffset;

            public Vector3 startBodyPartOffset;
            public Vector3 endBodyPartOffset;
            public Vector3 IKPoint;
            public float lerpValue;
        }


        Vector3 hipIk, currenthipIk, rightHandIK, leftHandIK;
        public void OnAnimatorIK(int layerIndex)
        {
            rightHand.boneTransform = animator.GetBoneTransform(HumanBodyBones.RightHand).position;
            leftHand.boneTransform = animator.GetBoneTransform(HumanBodyBones.LeftHand).position;
            rightFoot.boneTransform = animator.GetBoneTransform(HumanBodyBones.RightFoot).position;
            leftFoot.boneTransform = animator.GetBoneTransform(HumanBodyBones.LeftFoot).position;

            if (animator && parkourController.IsHanging)
            {
                if (currentPoint)
                {
                    if (lookAtPosition == Vector3.zero) lookAtPosition = currentPoint.transform.position;

                    lookAtPosition = Vector3.MoveTowards(lookAtPosition, currentPoint.transform.position, 2f * Time.deltaTime);
                    animator.SetLookAtPosition(lookAtPosition);
                    lookAtWeight = Mathf.Clamp01(lookAtWeight + 2f * Time.deltaTime) * 0.8f;
                    animator.SetLookAtWeight(lookAtWeight);
                }
                else
                {
                    lookAtWeight = Mathf.Clamp01(lookAtWeight - 1f * Time.deltaTime) * 0.5f;
                    animator.SetLookAtWeight(lookAtWeight);
                }


                if (ikEnabled)
                {
                    animator.SetIKPosition(AvatarIKGoal.RightHand, rightHand.IKPoint);
                    animator.SetIKPosition(AvatarIKGoal.LeftHand, leftHand.IKPoint);

                    //animator.SetLookAtPosition(currentPoint.transform.position);
                    //animator.SetLookAtWeight(0.4f);

                    if (rightFoot.previousIkVecPoint != Vector3.zero && leftFoot.previousIkVecPoint != Vector3.zero && rightFoot.currentIkVecPoint != Vector3.zero && leftFoot.currentIkVecPoint != Vector3.zero && hangType == HangType.bracedHang)
                    {
                        animator.SetIKPosition(AvatarIKGoal.RightFoot, rightFoot.IKPoint);
                        animator.SetIKPosition(AvatarIKGoal.LeftFoot, leftFoot.IKPoint);
                        animator.SetIKPositionWeight(AvatarIKGoal.RightFoot, rightFoot.weight);
                        animator.SetIKPositionWeight(AvatarIKGoal.LeftFoot, leftFoot.weight);
                    }
                    animator.SetIKPositionWeight(AvatarIKGoal.RightHand, rightHand.weight);
                    animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, leftHand.weight);
                }
                else
                {
                    //animator.SetLookAtPosition(currentPoint.transform.position);
                    //animator.SetLookAtWeight(0.4f);

                    void setIK(IKweights bodyPart, HumanBodyBones bone, AvatarIKGoal ikGoal)
                    {
                        Vector3 previous = bodyPart.previousVecPoint;
                        Vector3 current = bodyPart.currentVecPoint;

                        var curDis = (animator.GetBoneTransform(bone).transform.position - current).magnitude;
                        var preDis = (animator.GetBoneTransform(bone).transform.position - previous).magnitude;

                        if (currentPoint == previousPoint || previousPoint == null) preDis = 1000f;

                        if (curDis < preDis)
                        {
                            bodyPart.previousIkVecPoint = bodyPart.currentIkVecPoint;

                        }
                        animator.SetIKPosition(ikGoal, curDis < preDis ? bodyPart.currentIkVecPoint : bodyPart.previousIkVecPoint);

                        animator.SetIKPositionWeight(ikGoal, 0.75f - ((curDis < preDis ? curDis : preDis) - 0.1f) / 0.3f); // hop animation smoothness if offsets ,tweak if you want
                    }

                    //var handPos = rightHand.boneTransform;  //test
                    ////var offset = climbController.rightHand.currentVecPoint - handPos;
                    //var offset = GetHandPos(currentPoint.transform,AvatarTarget.RightHand) - handPos;
                    //transform.position += offset;
                    //transform.position = Vector3.MoveTowards(transform.position, transform.position + offset, 10f * Time.deltaTime);

                    setIK(rightHand, HumanBodyBones.RightHand, AvatarIKGoal.RightHand);
                    setIK(leftHand, HumanBodyBones.LeftHand, AvatarIKGoal.LeftHand);

                    if (hangType == HangType.bracedHang)
                    {
                        setIK(rightFoot, HumanBodyBones.RightFoot, AvatarIKGoal.RightFoot);
                        setIK(leftFoot, HumanBodyBones.LeftFoot, AvatarIKGoal.LeftFoot);
                    }
                    //print(animator.GetIKPositionWeight(AvatarIKGoal.RightFoot));
                    //print(animator.GetIKPositionWeight(AvatarIKGoal.LeftFoot));
                }

                //if (animator.GetCurrentAnimatorStateInfo(0).IsName("HangIdles"))
                //{
                //    hipIk = animator.bodyPosition + transform.up * Mathf.Max(0, input.DirectionInput.y * 0.5f) + transform.right * input.DirectionInput.x * 0.2f;
                //    var perc = (currenthipIk - animator.bodyPosition).magnitude / (hipIk - animator.bodyPosition).magnitude;
                //    if ((hipIk - animator.bodyPosition).magnitude < 0.01f) perc = 0;
                //    animator.bodyPosition = Vector3.MoveTowards(currenthipIk, hipIk, Time.deltaTime * 2f);
                //    currenthipIk = animator.bodyPosition;

                //    animator.SetLookAtPosition(rightHand.currentIkVecPoint + transform.up * Mathf.Max(0, input.DirectionInput.y) * perc + transform.right * input.DirectionInput.x * perc);
                //    animator.SetLookAtWeight(perc * 0.5f);

                //    if (input.DirectionInput.x > 0)
                //    {
                //        var rightFinal = rightHand.currentIkVecPoint + transform.up * Mathf.Max(-0.2f, input.DirectionInput.y) * perc + transform.right * input.DirectionInput.x * perc;
                //        animator.SetIKPosition(AvatarIKGoal.RightHand, rightFinal);
                //        animator.SetIKPositionWeight(AvatarIKGoal.RightHand, 0.7f);
                //    }
                //    else
                //    {
                //        var leftFinal = leftHand.currentIkVecPoint + transform.up * Mathf.Max(-0.2f, input.DirectionInput.y) * perc + transform.right * input.DirectionInput.x * perc;
                //        animator.SetIKPosition(AvatarIKGoal.LeftHand, leftFinal);
                //        animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, 0.7f);
                //    }
                //}
                //else
                //    currenthipIk = animator.bodyPosition;

            }

        }


        private Vector3 getFootIK(Vector3 rayStartPos, Vector3 rayDir)
        {
            //Vector3 rayStartPos = animator.GetIKPosition(ikGoal);
            rayStartPos -= rayDir * 0.2f;
            bool isWall = Physics.SphereCast(rayStartPos, 0.2f, rayDir, out RaycastHit hitInfo, footIkRayLength, envScanner.ObstacleLayer);
            //Debug.DrawRay(rayStartPos, rayDir * footIkRayLength, Color.green);

            var point = Vector3.zero;
            if (isWall)
            {
                point = hitInfo.point + hitInfo.normal * footPlacementOffset;
            }
            return point;
        }

        public override void HandleFixedUpdate()
        {
            if (enableClimbing)
                HandleClimbUpdate();
        }


        public void HandleClimbUpdate()
        {
            if (!parkourController.IsHanging && !parkourController.InAction)
            {
                previousPoint = null;
                currentPoint = null;

                if (!player.IsGrounded)
                {
                    if ((isFallingTimer += Time.deltaTime) > isFallingThreshold)
                        isFalling = true;
                }

                if (isFalling)
                {
                    isFallingTimer = 0;
                    if (player.IsGrounded)
                    {
                        isFalling = false;
                    }

                    if (inputManager.Jump)
                    {
                        var ledges = Physics.OverlapSphere(transform.position + transform.forward * 0.2f + Vector3.up * 1.5f, 0.4f, envScanner.LedgeLayer);
                        //GizmosExtend.drawSphere(transform.position + transform.forward * 0.2f + Vector3.up * 1.5f, 0.4f);
                        if (ledges.Length > 0)
                        {
                            var newPoint = GetNearestPoint(ledges[0].transform, transform.position + Vector3.up, checkDirection: transform.forward);

                            foreach (var item in ledges)
                            {
                                if (newPoint != null) break;
                                newPoint = GetNearestPoint(item.transform, transform.position + Vector3.up, checkDirection: transform.forward);
                            }
                            if (newPoint == null)//|| currentPoint.transform.position.y - newPoint.transform.position.y < 0.1f)
                                return;
                            previousPoint = currentPoint;
                            currentPoint = newPoint;

                            StartCoroutine(ClimbWhenFalling());
                        }
                    }

                }
                else if (inputManager.Drop && !parkourController.InAction)
                {
                    if (envScanner.DropLedgeCheck(transform.forward, out ClimbLedgeData ledgeData))
                    {
                        currentLedge = ledgeData.ledgeHit.transform;
                        if (DropToLedge(currentLedge, ledgeData.ledgeHit.point, false, obstacleCheck: false)) return;
                    }
                }
            }
            else if (!parkourController.InAction)
            {
                var moveInput = locomotionInputManager.DirectionInput;
                float h = moveInput.x;
                float v = moveInput.y;
                direction = new Vector3(h, v, 0f);
                animator.SetBool(AnimatorParameters.BackJumpMode, false);

                //if (animator.GetFloat(AnimatorParameters.freeHang) < 0.5f)
                //{
                //var cam = Camera.allCameras.First(c => c.enabled).transform;
                var cam = playerController.cameraGameObject.transform;
                var dir = cam.transform.forward;

                if (envScanner.alwaysUsePlayerForward)
                    dir = transform.forward;

                dir.y = 0f;
                var moveDir = ((locomotionInputManager.DirectionInput.x * cam.right) + (locomotionInputManager.DirectionInput.y * cam.forward)).normalized;

                var angleBWvec = Vector3.SignedAngle(transform.forward, dir, Vector3.down);

                var value = -angleBWvec / 180;
                //var value = -angleBWvec / 90;

                //animator.SetFloatMathf.Sign(value) * 0.1f + value(AnimatorParameters.jumpBackDirection, Mathf.Sign(value) * 0.1f + value);
                if (Mathf.Abs(value) < 0.2f) value = 0; //limit movement
                else
                    animator.SetBool(AnimatorParameters.BackJumpMode, true);

                animator.SetFloat(AnimatorParameters.BackJumpDir, Mathf.Clamp(value, -1, 1), 0.2f, Time.deltaTime * 2);
                //}
                if (inputManager.JumpFromHang || (inputManager.Jump && moveDir == Vector3.zero))
                {
                    if (moveDir != Vector3.zero || (inputManager.Jump && moveDir == Vector3.zero))
                    {
                        var checkDir = moveDir == Vector3.zero ? dir : moveDir;

                        checkDir.y = 0;

                        parkourController.HandlePredictiveBackJump(checkDir);
                    }
                }

                // Jump back
                else if (inputManager.Drop && !parkourController.InAction)
                {
                    if (moveInput == Vector2.zero)
                    {
                        DropFromPoint();
                        return;
                    }
                }

                else if (moveInput != Vector2.zero && !parkourController.InAction)
                {
                    // Climb up from mount point
                    if (moveInput.y > 0.6f && animator.GetCurrentAnimatorStateInfo(0).IsName("HangIdles"))
                    {
                        var hitData = envScanner.ObstacleCheck(forwardOriginOffset: (currentPoint.transform.position.y - transform.position.y) + 0.2f);
                        if ((currentPoint.useManualOptions) || (hitData.forwardHitFound && hitData.heightHitFound && hitData.hasSpace && (hitData.heightHit.point.y - currentPoint.transform.position.y) < 0.2f))
                        {
                            if (!currentPoint.useManualOptions || (currentPoint.useManualOptions && currentPoint.MountPoint))
                            {
                                MountPoint();
                                return;
                            }
                        }
                    }
                    if (moveInput.magnitude > 0.5f)
                    {
                        if (pressInputToClimb && inputManager.Jump && moveInput.y > .6f && moveInput.x == 0 && hangType == HangType.bracedHang)
                        {
                            StartCoroutine(DoClimbingAction("Bracedhang Try Jump Up",onComplete: () =>
                            {
                                player.OnEndSystem(this);
                            }));
                            parkourController.IsHanging = false;
                            isFalling = true;
                            animator.SetBool(AnimatorParameters.IsGrounded, false);
                            return;
                        }
                        // Shimmy
                        //currentPoint = neighbour.point;
                        //Neighbour neighbour = null;


                        ClimbPoint newPoint;

                        //newPoint = closestVirPoint(input.Jump ? moveInput : moveInput * 0.5f);
                        newPoint = closestPoint(moveInput);

                        if (!newPoint) return;

                        currentLedge = currentPoint.transform.parent;

                        var distance = Vector3.Distance(currentPoint.transform.position, newPoint.transform.position);
                        var distanceY = newPoint.transform.position.y - currentPoint.transform.position.y;
                        if (distance < 1f)
                        {
                            if (distanceY > 0.6f && hangType == HangType.freeHang)
                                return;
                        }
                        else
                        {
                            if (distanceY > 0f && hangType == HangType.freeHang)
                                return;
                        }
                        var angleDiff = Vector3.Angle(currentPoint.transform.forward, newPoint.transform.forward);

                        if (angleDiff < 30f && distance < 0.9f)//|| (neighbour != null && neighbour.connectionType == ConnectionType.Move))
                        {
                            DoShimmyAction(newPoint);

                        }
                        else if (inputManager.Jump)
                        {
                            DoClimbJumpAction(newPoint, angleDiff);
                        }
                    }
                }
            }
        }

        public IEnumerator ClimbWhenFalling()
        {
            parkourController.DisableRootMotion();
            parkourController.InAction = true;
            parkourController.IsHanging = true;

            player.OnStartSystem(this);
            if (playerController.WaitToStartSystem) yield return new WaitUntil(() => playerController.WaitToStartSystem == false);

            string animationName;
            if (CheckWall(currentPoint).Value.isWall)
            {
                animator.SetFloat(AnimatorParameters.freeHang, 0);
                animationName = "PredictiveToBracedhang";
            }
            else
            {
                animationName = "PredictiveToFreehangOneHanded";
                animator.SetFloat(AnimatorParameters.freeHang, 1);
            }
            animator.CrossFadeInFixedTime(animationName, 0.2f);
            animator.Update(0);

            var right = GetHandPos(currentPoint.transform, AvatarTarget.RightHand);

            var _animator = animatorHelper.sampleAnimation("HangIdles", animator);

            initializeBodyParts();

            animatorHelper.closeSampler(_animator);

            var animState = animator.GetNextAnimatorStateInfo(0);

            playerController.OnLand?.Invoke(0.01f, 1f);

            float timer = 0f;
            while (timer <= animState.length)
            {
                timer += Time.deltaTime;

                var handPos = rightHand.boneTransform;
                if (rightHand.boneTransform == Vector3.zero)
                    handPos = animator.GetBoneTransform(HumanBodyBones.RightHand).position;

                var offset = right - handPos;
                //transform.position += offset;
                transform.position = Vector3.MoveTowards(transform.position, transform.position + offset, 5f * Time.deltaTime);

                transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(-currentPoint.transform.forward),
                     parkourController.RotationSpeed * Time.deltaTime);

                yield return null;
            }
            parkourController.InAction = false;
            parkourController.EnableRootMotion();
        }

        public void DoClimbJumpAction(ClimbPoint newPoint, float angleDiff = 0)
        {
            previousPoint = currentPoint;
            currentPoint = newPoint;
            var angle = Vector3.SignedAngle(-transform.right, currentPoint.transform.position - previousPoint.transform.position, previousPoint.transform.forward);

            if (angleDiff < 30f && Mathf.Abs(angle) > 60 && Mathf.Abs(angle) < 120)
            {
                animator.SetFloat("x", 0f);
                if (angle > 0 && angle < 180)
                    animator.SetFloat("y", 1f);
                else
                    animator.SetFloat("y", -1f);
            }
            else
            {
                animator.SetFloat("y", 0f);
                if (Mathf.Abs(angle) > 90)
                    animator.SetFloat("x", 1f);
                else
                    animator.SetFloat("x", -1f);
            }

            var match = new Vector3();

            var isWall = CheckWall(currentPoint).Value.isWall;

            var climbTransition = 0;
            if (animator.GetFloat(AnimatorParameters.freeHang) > 0.7f && isWall)
                climbTransition = -1;
            else if (animator.GetFloat(AnimatorParameters.freeHang) < 0.3f && !isWall)
                climbTransition = 1;

            StartCoroutine(JumpToLedge(currentPoint.transform, "ClimbTree", match.x, match.y, matchHand: match.z == 0 ? AvatarTarget.RightHand : AvatarTarget.LeftHand, climbTransition: climbTransition));
        }

        public void DoShimmyAction(ClimbPoint newPoint)
        {
            previousPoint = currentPoint;
            currentPoint = newPoint;

            var match = new Vector3();

            var angle = Vector3.Angle(transform.right, currentPoint.transform.position - previousPoint.transform.position);
            var angleSigned = Vector3.SignedAngle(transform.right, currentPoint.transform.position - previousPoint.transform.position, previousPoint.transform.forward);

            var mirror = false;

            var isWall = CheckWall(currentPoint).Value.isWall;

            var climbTransition = 0;
            if (animator.GetFloat(AnimatorParameters.freeHang) > 0.7f && isWall)
                climbTransition = -1;
            else if (animator.GetFloat(AnimatorParameters.freeHang) < 0.3f && !isWall)
                climbTransition = 1;

            var x = 0.5f * Mathf.Cos(angleSigned * Mathf.Deg2Rad);
            var y = -0.5f * Mathf.Sin(angleSigned * Mathf.Deg2Rad);

            animator.SetFloat("x", Mathf.Abs(x));
            animator.SetFloat("y", y);

            if (angle > 85 && angle < 95) mirror = !animator.GetBool(AnimatorParameters.mirrorAction);
            //else if (angle < 90) mirror = true;
            else if (x < 0)
            {
                mirror = true;
                //match = new Vector3(0.40f, 0.6f, 0);
            }
            //else {
            //    mirror = false;
            //}

            StartCoroutine(JumpToLedge(currentPoint.transform, "ClimbTree", match.x, match.y, matchHand: match.z == 0 ? AvatarTarget.RightHand : AvatarTarget.LeftHand, climbTransition: climbTransition, ikAssist: true, mirror: mirror));
        }
        public void DropFromPoint()
        {
            if (hangType == HangType.bracedHang)
                StartCoroutine(DoClimbingAction("JumpFromHang",
               onComplete: () =>
               {
                   player.OnEndSystem(this);
               }));
            else
                StartCoroutine(DoClimbingAction("JumpFromFreeHang",
               onComplete: () =>
               {
                   player.OnEndSystem(this);
               }));
            parkourController.IsHanging = false;
            isFalling = true;
            animator.SetBool(AnimatorParameters.IsGrounded, false);
        }
        public bool DropToLedge(Transform currentLedge, Vector3 point, bool checkAngle = true, bool obstacleCheck = true, Vector3? checkDirection = null)
        {
            var newPoint = GetNearestPoint(currentLedge, point, checkAngle: checkAngle, obstacleCheck: obstacleCheck, checkDirection: checkDirection);
            if (newPoint == null || !CheckSpaceForClimb(newPoint))
                return false;
            DropToPoint(newPoint);
            return true;
        }
        public bool DropToPoint(ClimbPoint newPoint)
        {
            if (newPoint == null) return false;

            currentPoint = newPoint;

            player.OnStartSystem(this);

            if (CheckWall(currentPoint).Value.isWall)
            {
                animator.SetFloat(AnimatorParameters.freeHang, 0);
                StartCoroutine(JumpToLedge(currentPoint.transform, "DropToHang", 0.50f, 0.90f, rotateToLedge: true, matchStart: AvatarTarget.LeftFoot, onComplete: () =>
                {
                    animator.SetFloat(AnimatorParameters.freeHang, 0);
                }));
            }
            else
            {
                animator.SetFloat(AnimatorParameters.freeHang, 1);
                StartCoroutine(JumpToLedge(currentPoint.transform, "DropToFreeHang", 0.50f, 0.89f, rotateToLedge: true, matchStart: AvatarTarget.LeftFoot, onComplete: () =>
                {
                    animator.SetFloat(AnimatorParameters.freeHang, 1);
                }));
            }
            return true;
        }

        public void MountPoint()
        {
            if (hangType == HangType.bracedHang)
                StartCoroutine(DoClimbingAction("BracedHangClimb",
                    onComplete: () =>
                    {
                        player.OnEndSystem(this);
                        parkourController.IsHanging = false;
                        parkourController.ResetRootMotion();
                    }));
            else
                StartCoroutine(DoClimbingAction("FreeHangClimb",
                   onComplete: () =>
                   {
                       player.OnEndSystem(this);
                       parkourController.IsHanging = false;
                       parkourController.ResetRootMotion();
                   }));
        }
        public bool ClimbToLedge(Transform currentLedge, Vector3 point, string animName = null, bool combo = false, Vector3? checkDirection = null)
        {
            var newPoint = GetNearestPoint(currentLedge, point, checkDirection: checkDirection);

            if (newPoint == null)
                return false;

            ClimbToPoint(newPoint, point, animName, combo, checkDirection);
            return true;
        }
        public bool ClimbToPoint(ClimbPoint newPoint, Vector3 point, string animName = null, bool combo = false, Vector3? checkDirection = null)
        {
            if (newPoint == null)
                return false;

            previousPoint = currentPoint;
            currentPoint = newPoint;

            var distance = (new Vector3(currentPoint.transform.position.x - transform.position.x, 0, currentPoint.transform.position.z - transform.position.z)).magnitude;

            //if (distance > 1f)
            //{
            //    return;
            //}
            if (CheckWall(currentPoint).Value.isWall)
            {
                if (distance > 1.5f || combo)
                {
                    if (parkourController.IsHanging)
                        StartCoroutine(parkourController.DoPredictiveBackJump(null, currentPoint, 0, combo: combo));
                    else
                        StartCoroutine(parkourController.DoPredictiveClimb(currentPoint, 0));
                }
                else if (!parkourController.IsHanging)
                {
                    animator.SetFloat(AnimatorParameters.freeHang, 0);

                    player.OnStartSystem(this);
                    StartCoroutine(JumpToLedge(currentPoint.transform, "IdleToBracedHang", 0.44f, 0.68f, matchStart: AvatarTarget.RightFoot));
                }
                else currentPoint = previousPoint;
            }
            else
            {
                if (distance > 1.5f || combo)
                {
                    if (parkourController.IsHanging)
                        StartCoroutine(parkourController.DoPredictiveBackJump(null, currentPoint, 1, combo: combo));
                    else
                        StartCoroutine(parkourController.DoPredictiveClimb(currentPoint, 1));
                }
                else if (!parkourController.IsHanging)
                {
                    animator.SetFloat(AnimatorParameters.freeHang, 1);
                    player.OnStartSystem(this);
                    StartCoroutine(JumpToLedge(currentPoint.transform, "IdleToFreeHang", 0.5f, 0.8f, matchStart: AvatarTarget.RightFoot, onComplete: () =>
                    {
                        animator.SetFloat(AnimatorParameters.freeHang, 1);
                    }));
                }
                else currentPoint = previousPoint;
            }
            return true;
        }

        public void initializeBodyParts(TargetMatchParams matchParams = null, AnimatorHelper _animatorHelper = null, float climbTransition = 0, bool mirror = false)
        {
            initializeStartBodyParts(matchParams, _animatorHelper, climbTransition, mirror);
            initializeEndBodyParts(matchParams, _animatorHelper, climbTransition, mirror);
        }

        public float handSpacing;

        public void initializeStartBodyParts(TargetMatchParams matchParams = null, AnimatorHelper _animatorHelper = null, float climbTransition = 0, bool mirror = false)
        {
            Vector3 curHandOffsets = handOffsets;

            if (_animatorHelper == null) _animatorHelper = animatorHelper;
            if (previousPoint)
            {
                var startCenter = (_animatorHelper.getTransformPos(HumanBodyBones.LeftHand, 0) + _animatorHelper.getTransformPos(HumanBodyBones.RightHand, 0)) / 2;

                rightFoot.startBodyPartOffset = (_animatorHelper.getTransformPos(HumanBodyBones.RightFoot, 0) - startCenter);
                leftFoot.startBodyPartOffset = (_animatorHelper.getTransformPos(HumanBodyBones.LeftFoot, 0) - startCenter);
                rightHand.startBodyPartOffset = (_animatorHelper.getTransformPos(HumanBodyBones.RightHand, 0) - startCenter) + curHandOffsets;
                leftHand.startBodyPartOffset = (_animatorHelper.getTransformPos(HumanBodyBones.LeftHand, 0) - startCenter) + curHandOffsets;

                rightFoot.previousVecPoint = _animatorHelper.pointTransformWithVectorDown(previousPoint.transform, rightFoot.startBodyPartOffset);
                rightFoot.previousIkVecPoint = getFootIK(rightFoot.previousVecPoint, -previousPoint.transform.forward);
                if (rightFoot.previousIkVecPoint == Vector3.zero) rightFoot.previousIkVecPoint = rightFoot.previousVecPoint;

                leftFoot.previousVecPoint = _animatorHelper.pointTransformWithVectorDown(previousPoint.transform, leftFoot.startBodyPartOffset);
                leftFoot.previousIkVecPoint = getFootIK(leftFoot.previousVecPoint, -previousPoint.transform.forward);
                if (leftFoot.previousIkVecPoint == Vector3.zero) leftFoot.previousIkVecPoint = leftFoot.previousVecPoint;

                var _handSpacing = handSpacing;
                if (previousPoint.useManualOptions)
                {
                    _handSpacing = previousPoint.handSpacing;
                }
                rightHand.previousVecPoint = _animatorHelper.pointTransformWithVectorDown(previousPoint.transform, rightHand.startBodyPartOffset);
                rightHand.previousIkVecPoint = _animatorHelper.pointTransformWithVectorDown(previousPoint.transform, rightHand.startBodyPartOffset - _handSpacing * Vector3.right);
                //rightHand.previousIkVecPoint = rightHand.previousVecPoint;

                leftHand.previousVecPoint = _animatorHelper.pointTransformWithVectorDown(previousPoint.transform, leftHand.startBodyPartOffset);
                leftHand.previousIkVecPoint = _animatorHelper.pointTransformWithVectorDown(previousPoint.transform, leftHand.startBodyPartOffset + _handSpacing * Vector3.right);
                //leftHand.previousIkVecPoint = leftHand.previousVecPoint;
            }
        }

        public void initializeEndBodyParts(TargetMatchParams matchParams = null, AnimatorHelper _animatorHelper = null, float climbTransition = 0, bool mirror = false)
        {
            Vector3 curHandOffsets = handOffsets;

            if (_animatorHelper == null) _animatorHelper = animatorHelper;
            if (currentPoint)
            {
                var endCenter = (_animatorHelper.getTransformPos(HumanBodyBones.LeftHand, 1) + _animatorHelper.getTransformPos(HumanBodyBones.RightHand, 1)) / 2;

                rightFoot.endBodyPartOffset = (_animatorHelper.getTransformPos(HumanBodyBones.RightFoot, 1) - endCenter);
                leftFoot.endBodyPartOffset = (_animatorHelper.getTransformPos(HumanBodyBones.LeftFoot, 1) - endCenter);
                rightHand.endBodyPartOffset = (_animatorHelper.getTransformPos(HumanBodyBones.RightHand, 1) - endCenter) + curHandOffsets;
                leftHand.endBodyPartOffset = (_animatorHelper.getTransformPos(HumanBodyBones.LeftHand, 1) - endCenter) + curHandOffsets;

                rightFoot.currentVecPoint = _animatorHelper.pointTransformWithVectorDown(currentPoint.transform, rightFoot.endBodyPartOffset);
                rightFoot.currentIkVecPoint = getFootIK(rightFoot.currentVecPoint, -currentPoint.transform.forward);
                if (rightFoot.currentIkVecPoint == Vector3.zero) rightFoot.currentIkVecPoint = rightFoot.currentVecPoint;

                leftFoot.currentVecPoint = _animatorHelper.pointTransformWithVectorDown(currentPoint.transform, leftFoot.endBodyPartOffset);
                leftFoot.currentIkVecPoint = getFootIK(leftFoot.currentVecPoint, -currentPoint.transform.forward);
                if (leftFoot.currentIkVecPoint == Vector3.zero) leftFoot.currentIkVecPoint = leftFoot.currentVecPoint;

                var _handSpacing = handSpacing;
                if (currentPoint.useManualOptions)
                {
                    _handSpacing = currentPoint.handSpacing;
                }
                rightHand.currentVecPoint = _animatorHelper.pointTransformWithVectorDown(currentPoint.transform, rightHand.endBodyPartOffset);
                rightHand.currentIkVecPoint = _animatorHelper.pointTransformWithVectorDown(currentPoint.transform, rightHand.endBodyPartOffset - _handSpacing * Vector3.right);
                //rightHand.currentIkVecPoint = rightHand.currentVecPoint;

                leftHand.currentVecPoint = _animatorHelper.pointTransformWithVectorDown(currentPoint.transform, leftHand.endBodyPartOffset);
                leftHand.currentIkVecPoint = _animatorHelper.pointTransformWithVectorDown(currentPoint.transform, leftHand.endBodyPartOffset + _handSpacing * Vector3.right);
                //leftHand.currentIkVecPoint = leftHand.currentVecPoint;
            }

            if (matchParams == null) return;
            if (matchParams.target == AvatarTarget.RightHand)
            {
                matchParams.pos = rightHand.currentVecPoint;
            }
            else if (matchParams.target == AvatarTarget.LeftHand)
            {
                matchParams.pos = leftHand.currentVecPoint;
            }
        }

        public void DoShimmy(float normalTime, bool mirror = false, bool HangTypeChange = false)
        {
            float[] lerpValues = new float[4];


            //lerp value offsets , equation = (normalTime - startTime)/endTime
            lerpValues[0] = (normalTime - 0.1f) / 0.3f; //rightFoot
            lerpValues[1] = (normalTime - 0) / 0.3f; //rightHand
            lerpValues[2] = (normalTime - 0.5f) / 0.5f; //leftHand
            lerpValues[3] = (normalTime - 0.6f) / 0.4f; //leftFoot

            //if (animator.GetFloat("x") < 0f || mirror) Array.Reverse(lerpValues);
            if (mirror) Array.Reverse(lerpValues);

            rightFoot.lerpValue = lerpValues[0];
            rightHand.lerpValue = lerpValues[1];
            leftHand.lerpValue = lerpValues[2];
            leftFoot.lerpValue = lerpValues[3];

            rightHand.IKPoint = Vector3.Slerp(rightHand.previousIkVecPoint, rightHand.currentIkVecPoint, rightHand.lerpValue);
            leftHand.IKPoint = Vector3.Slerp(leftHand.previousIkVecPoint, leftHand.currentIkVecPoint, leftHand.lerpValue);

            rightHand.weight = rightFoot.weight = 1.3f;
            leftHand.weight = leftFoot.weight = 1.3f;

            rightHand.weight = Mathf.Abs(0.5f - rightHand.lerpValue) * 0.5f + 1f;
            rightFoot.weight = Mathf.Abs(0.5f - rightFoot.lerpValue) * 0.5f + 1f;
            leftHand.weight = Mathf.Abs(0.5f - leftHand.lerpValue) * 0.5f + 1f;
            leftFoot.weight = Mathf.Abs(0.5f - leftFoot.lerpValue) * 0.5f + 1f;

            var freeHanglerp = (0.5f - animator.GetFloat(AnimatorParameters.freeHang));
            rightFoot.IKPoint = Vector3.Slerp(rightFoot.previousIkVecPoint, rightFoot.currentIkVecPoint, rightFoot.lerpValue);
            leftFoot.IKPoint = Vector3.Slerp(leftFoot.previousIkVecPoint, leftFoot.currentIkVecPoint, leftFoot.lerpValue);
            rightFoot.weight *= freeHanglerp;
            leftFoot.weight *= freeHanglerp;

            //if (animator.GetFloat(AnimatorParameters.freeHang) < 0.5f) // to disable it in free hang
            //{
            //    rightFoot.IKPoint = Vector3.Slerp(rightFoot.previousIkVecPoint, rightFoot.currentIkVecPoint, rightFoot.lerpValue);
            //    leftFoot.IKPoint = Vector3.Slerp(leftFoot.previousIkVecPoint, leftFoot.currentIkVecPoint, leftFoot.lerpValue);
            //}
            //else
            //{
            //    rightFoot.weight = leftFoot.weight = 0f;
            //}

        }

        WallInfo? CheckWall(ClimbPoint point)
        {
            if (point == null) return null;

            WallInfo wallinfo = new WallInfo();

            var rightFootPoint = point.transform.position + wallRayOffset.x * point.transform.right + wallRayOffset.y * point.transform.up + wallRayOffset.z * point.transform.forward;
            var leftFootPoint = point.transform.position - wallRayOffset.x * point.transform.right + wallRayOffset.y * point.transform.up + wallRayOffset.z * point.transform.forward;

            wallinfo.isWall = Physics.SphereCast(rightFootPoint, 0.1f, -point.transform.forward, out wallinfo.rightFootInfo, hipRayLength, envScanner.ObstacleLayer);
            // Debug.DrawRay(rightFootPoint, -point.transform.forward * hipRay, isWall ? Color.green : Color.red);
            if (wallinfo.isWall)
            {
                wallinfo.isWall = Physics.SphereCast(leftFootPoint, 0.1f, -point.transform.forward, out wallinfo.leftFootInfo, hipRayLength, envScanner.ObstacleLayer);
            }

            // Debug.DrawRay(leftFootPoint, -point.transform.forward * hipRay, isWall ? Color.green : Color.red);

            return wallinfo;
        }
        public struct WallInfo
        {
            public bool isWall;
            public RaycastHit rightFootInfo;
            public RaycastHit leftFootInfo;
        }

        public bool RayCastCheck(ClimbPoint prevPoint, ClimbPoint currPoint)
        {
            var previousOffset = prevPoint.transform.position + obstacleRayOffset.y * prevPoint.transform.up + obstacleRayOffset.z * prevPoint.transform.forward;

            var currentOffset = currPoint.transform.position + obstacleRayOffset.y * currPoint.transform.up + obstacleRayOffset.z * currPoint.transform.forward;

            var direction = currPoint.transform.position - prevPoint.transform.position;


            var preDir = previousOffset + Vector3.ProjectOnPlane(direction, prevPoint.transform.forward);
            var curDir = currentOffset + Vector3.ProjectOnPlane(-direction, currPoint.transform.forward);

            var middlePoint = (preDir + curDir) / 2;

            var rayCastAmount = 2;

            for (int i = 0; i < rayCastAmount; i++)
            {
                var offset = (Vector3.down * 0.5f / rayCastAmount) * i;
                previousOffset += offset;
                currentOffset += offset;
                middlePoint += offset;
                if (!Physics.Linecast(previousOffset, middlePoint, (envScanner.ObstacleLayer | envScanner.LedgeLayer)))
                    if (!Physics.Linecast(middlePoint, currentOffset, (envScanner.ObstacleLayer | envScanner.LedgeLayer)))
                        return false;
            }

            return true;
        }
        public bool CheckSpaceForClimb(ClimbPoint point)
        {
            var halfExtends = new Vector3(.3f, .5f, .25f);
            var down = Vector3.down * .65f;
            if (!CheckWall(point).Value.isWall)
            {
                down = Vector3.down;
                halfExtends.y = 1;
            }
            var hasSpaceForClimb = !Physics.CheckBox(point.transform.position + point.transform.forward * .6f + down, halfExtends, Quaternion.LookRotation(Vector3.right), envScanner.ObstacleLayer);

            return hasSpaceForClimb && !point.hasOwner;
        }


        public Vector3 GetHandPos(Transform ledge, AvatarTarget hand, Vector3? handOffset = null)
        {
            //initializeBodyParts();
            rightHand.currentIkVecPoint = animatorHelper.pointTransformWithVectorDown(ledge.transform, rightHand.endBodyPartOffset);
            leftHand.currentIkVecPoint = animatorHelper.pointTransformWithVectorDown(ledge.transform, leftHand.endBodyPartOffset);
            var point = (hand == AvatarTarget.RightHand) ? rightHand.currentIkVecPoint : leftHand.currentIkVecPoint;

            return point;
        }

        public IEnumerator JumpToLedge(Transform ledge, string anim, float matchStartTime, float matchEndTime,
            Vector3? offset = null,
            AvatarTarget matchHand = AvatarTarget.RightHand,
            AvatarTarget matchStart = AvatarTarget.RightHand,
            bool rotateToLedge = true,
            Action onComplete = null, int climbTransition = 0, bool ikAssist = false, bool mirror = false, bool autoMatch = true)
        {
            if (parkourController.InAction) yield break;


            if (mirror) matchHand = matchHand == AvatarTarget.RightHand ? AvatarTarget.LeftHand : AvatarTarget.RightHand;
            if (mirror)
            {
                if (matchStart == AvatarTarget.RightHand)
                    matchStart = AvatarTarget.LeftHand;
                else if (matchStart == AvatarTarget.LeftHand)
                    matchStart = AvatarTarget.RightHand;
            }


            var grabPos = GetHandPos(ledge, matchHand, offset);

            var targetRot = Quaternion.LookRotation(-ledge.forward);
            //var targetRot = Quaternion.LookRotation(-(transform.position - ledge.position).normalized );

            var matchParams = new TargetMatchParams()
            {
                pos = grabPos,
                startTime = matchStartTime,
                endTime = matchEndTime,
                target = matchHand,
                startTarget = matchStart,
                posWeight = Vector3.one
            };

            parkourController.IsHanging = true;

            yield return DoClimbingAction(anim, rotateToLedge, targetRot, matchParams, onComplete: onComplete, climbTransition: climbTransition, ikAssist: ikAssist, mirror: mirror, autoMatch: autoMatch);
            ikEnabled = false;

        }

        AnimatorHelper animatorHelper = new AnimatorHelper();

        public IEnumerator DoClimbingAction(string anim, bool rotate = false,
              Quaternion targetRot = new Quaternion(), TargetMatchParams matchParams = null, bool mirror = false, Action onComplete = null, int climbTransition = 0, bool ikAssist = false, bool autoMatch = true)
        {
            parkourController.EnableRootMotion();
            parkourController.InAction = true;

            if (playerController.WaitToStartSystem)
            {
                yield return new WaitUntil(() => playerController.WaitToStartSystem == false);
            }

            animator.SetBool(AnimatorParameters.mirrorAction, mirror);

            AnimatorStateInfo animState;

            if (matchParams != null)
            {
                var _animator = animatorHelper.sampleAnimation(anim, animator);
                matchParams.startTime = animatorHelper.findStartTime(matchParams.startTarget);

                initializeStartBodyParts(matchParams, climbTransition: climbTransition, mirror: mirror);
                matchParams.startPos = transform.position;

                var maxDelta = 0.005f;

                if (climbTransition != 0)
                {
                    _animator.SetFloat(AnimatorParameters.freeHang, climbTransition == 1 ? 1 : -1);
                    maxDelta = 0.001f;
                }
                //if(animator.GetFloat(AnimatorParameters.freeHang) > 0.5f)
                //    maxDelta = 0.001f;
                if (ikAssist)
                    maxDelta = 0.001f;

                matchParams.endTime = animatorHelper.findEndTime(matchParams.target, maxDelta);
                initializeEndBodyParts(matchParams, climbTransition: climbTransition, mirror: mirror);



                if (matchParams.startTime > matchParams.endTime) (matchParams.startTime, matchParams.endTime) = (matchParams.endTime, matchParams.startTime);

                //matchParams.endPos = animatorHelper.getRotatedPos(AvatarTarget.Root, matchParams.endTime) - animatorHelper.getRotatedPos(matchParams.target, matchParams.endTime);

                if (matchParams.target == AvatarTarget.RightHand)
                {
                    matchParams.endPos = AnimatorHelper._animator.transform.position - animatorHelper.getTransformPos(HumanBodyBones.RightHand, matchParams.endTime);
                }
                else if (matchParams.target == AvatarTarget.LeftHand)
                {
                    matchParams.endPos = AnimatorHelper._animator.transform.position - animatorHelper.getTransformPos(HumanBodyBones.LeftHand, matchParams.endTime);
                }
                else
                    matchParams.endPos = animatorHelper.getRotatedPos(AvatarTarget.Root, matchParams.endTime) - animatorHelper.getRotatedPos(matchParams.target, matchParams.endTime);

                matchParams.endPos = animatorHelper.pointTransformWithVectorDown(currentPoint.transform, matchParams.endPos, matchParams.pos);
                var changedPos = animatorHelper.getRotatedPos(AvatarTarget.Root, matchParams.endTime) - animatorHelper.getRotatedPos(matchParams.target, matchParams.endTime);
                changedPos = animatorHelper.pointTransformWithVectorDown(currentPoint.transform, changedPos, matchParams.pos);

                matchParams.pos += (matchParams.endPos - changedPos);


                animatorHelper.closeSampler(_animator);
            }

            animator.CrossFadeInFixedTime(anim, 0.2f);
            yield return null;

            animState = animator.GetNextAnimatorStateInfo(0);

            float timer = 0f;

            var localPos = transform.position;

            while (timer <= animState.length)
            {
                timer += Time.deltaTime;
                float normalTime = timer / animState.length;

                if (matchParams != null)
                {
                    //autoMatcher(matchParams, normalTime);
                    MatchTarget(matchParams.pos, matchParams.rot, matchParams.target, new MatchTargetWeightMask(matchParams.posWeight, 0), matchParams.startTime, matchParams.endTime);

                    if (rotate && normalTime >= matchParams.startTime)
                        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRot,
                             parkourController.RotationSpeed * Time.deltaTime);

                }

                if (climbTransition != 0)
                {
                    var lerpMiddleTime = (matchParams.startTime + matchParams.endTime) / 2;
                    var lerpingTime = (matchParams.endTime - matchParams.startTime);
                    //var lerpValue = climbTransition < 0 ? (normalTime - lerpMiddleTime) / (lerpingTime / 2) : (normalTime - matchParams.startTime) / (lerpingTime / 2);
                    var lerpValue = ikAssist ? (normalTime - lerpMiddleTime) / (lerpingTime / 2) : (normalTime - matchParams.startTime) / (lerpingTime);
                    animator.Update(-Time.deltaTime);
                    animator.SetFloat(AnimatorParameters.freeHang, Mathf.Lerp((climbTransition - Math.Abs(climbTransition)) / -2, (climbTransition + Math.Abs(climbTransition)) / 2, lerpValue));
                    animator.Update(Time.deltaTime);
                }

                if (ikAssist)
                {
                    ikEnabled = true;
                    DoShimmy(normalTime, mirror, climbTransition != 0 ? true : false);
                }

                //else if (matchParams != null  && normalTime >= Mathf.Max(matchParams.endTime, 0.9f)) break; // for Smoothness

                yield return null;
            }

            previousPoint = currentPoint;
            parkourController.InAction = false;

            //if (matchParams != null)
            //{
            //    Vector3 offset = Vector3.zero;

            //    if (matchParams.target == AvatarTarget.RightHand)
            //        offset = rightHand.currentVecPoint - rightHand.boneTransform;
            //    else if (matchParams.target == AvatarTarget.LeftHand)
            //        offset = leftHand.currentVecPoint - leftHand.boneTransform;

            //    transform.position = Vector3.MoveTowards(transform.position, transform.position + offset, 5f * Time.deltaTime);
            //}
            //parkourController.DisableRootMotion();

            onComplete?.Invoke();
        }
        void autoMatcher(TargetMatchParams matchParams, float time)
        {
            if (time > matchParams.startTime && time <= matchParams.endTime)
            {
                parkourController.DisableRootMotion();
                transform.position = Vector3.Lerp(matchParams.startPos, matchParams.endPos, Mathf.Pow((time - matchParams.startTime) / (matchParams.endTime - matchParams.startTime), 0.6f));
                //transform.position = Vector3.Lerp(transform.position, matchParams.endPos, (time - matchParams.startTime) / (matchParams.endTime - matchParams.startTime));
                //Vector3.MoveTowards(transform.position, matchParams.endPos,animator.deltaPosition.magnitude);
            }
            else
            {
                matchParams.startPos = transform.position;
                parkourController.EnableRootMotion();

            }
        }
        void MatchTarget(Vector3 matchPos, Quaternion rotation, AvatarTarget target, MatchTargetWeightMask weightMask, float startTime, float endTime)
        {
            if (animator.isMatchingTarget || animator.IsInTransition(0)) return;

            animator.MatchTarget(matchPos, rotation, target, weightMask, startTime, endTime);
        }


        public ClimbPoint closestPoint(Vector2 input)
        {
            var converted = (input.x * transform.right + input.y * transform.up).normalized;
            if (converted.magnitude == 0)
                return null;
            Collider[] hitColliders;


            hitColliders = Physics.OverlapSphere(currentPoint.transform.position + converted, 1f, envScanner.LedgeLayer);


            List<ClimbPoint> points = new List<ClimbPoint>();
            foreach (var hitCollider in hitColliders)
            {
                points.AddRange(hitCollider.gameObject.GetComponentsInChildren<ClimbPoint>());

            }

            var origin = currentPoint.transform.position;

            //var distance = Mathf.Infinity;
            var distance = 2f;

            List<ClimbPoint> overlapPoint = new List<ClimbPoint>();

            foreach (var point in points)
            {
                Vector3 point2origin = point.transform.position - origin;


                var projected = Vector3.ProjectOnPlane(point2origin, currentPoint.transform.forward);

                var projected2 = Vector3.ProjectOnPlane(converted, currentPoint.transform.forward);

                var projected3 = Vector3.ProjectOnPlane(point2origin, currentPoint.transform.up);

                var projected4 = Vector3.ProjectOnPlane(converted, currentPoint.transform.up);

                var angleX = Vector3.Angle(projected, projected2);


                var angleZ = Vector3.SignedAngle(projected3, projected4, Vector3.up);

                if (Vector3.Angle(currentPoint.transform.forward, point.transform.forward) > 100f) continue;

                //if (angleZ > 100f) continue;

                if (Vector3.Angle(currentPoint.transform.forward, point.transform.forward) > 45f && point2origin.magnitude > 1f) continue;

                if (angleX < 45f && point2origin.magnitude < distance && point != currentPoint)
                {
                    if (angleX < 40f || overlapPoint.Count > 0)
                    {
                        if (CheckSpaceForClimb(point) && !RayCastCheck(currentPoint, point))
                        {
                            overlapPoint.Insert(0, point);
                            distance = point2origin.magnitude;
                        }

                    }
                }
            }
            if (overlapPoint.Count > 0)
                return overlapPoint[0];
            else
                return null;
        }

        Vector3 direction;
        public ClimbPoint ClosestVirPoint(Vector2 input)
        {
            var converted = (input.x * transform.right + input.y * transform.up).normalized;
            if (converted.magnitude == 0)
                return null;
            Collider[] hitColliders;


            hitColliders = Physics.OverlapSphere(currentPoint.transform.position + converted, 1f, envScanner.LedgeLayer);

            float distance = 2f;
            List<ClimbPoint> points = new List<ClimbPoint>();

            GameObject curPoint = new GameObject("test", typeof(ClimbPoint));

            ClimbPoint nextPoint = null;
            foreach (var hitCollider in hitColliders)
            {
                var point = hitCollider.ClosestPoint(currentPoint.transform.position + converted);
                point += hitCollider.transform.forward * 2f;
                point.y += 2f;
                point = hitCollider.ClosestPoint(point);

                var curDistance = ((currentPoint.transform.position + converted) - point).magnitude;
                if (curDistance < distance && curDistance > 0.2f)
                {
                    nextPoint = curPoint.GetComponent<ClimbPoint>();
                    nextPoint.transform.forward = hitCollider.transform.forward;
                    nextPoint.transform.position = point;
                    distance = ((currentPoint.transform.position + converted) - point).magnitude;
                }
            }
            if (nextPoint)
                return nextPoint;
            return null;
        }


        private void OnDrawGizmosSelected()
        {
            //Gizmos.color = Color.magenta;

            //Gizmos.DrawSphere(rightHand.currentVecPoint, 0.02f);
            //Gizmos.DrawSphere(leftHand.currentVecPoint, 0.02f);
            //Gizmos.DrawSphere(rightFoot.currentVecPoint, 0.02f);
            //Gizmos.DrawSphere(leftFoot.currentVecPoint, 0.02f);
            ////if (!turnOnGizmos) return;
            //Gizmos.color = Color.yellow;
            //Gizmos.DrawSphere(rightHand.previousVecPoint, 0.02f);
            //Gizmos.DrawSphere(leftHand.previousVecPoint, 0.02f);
            //Gizmos.DrawSphere(rightFoot.previousVecPoint, 0.02f);
            //Gizmos.DrawSphere(leftFoot.previousVecPoint, 0.02f);
            //if (rightHand.boneTransform != null)
            //{
            //    Gizmos.DrawSphere(rightHand.boneTransform, 0.02f);
            //    Gizmos.DrawSphere(leftHand.boneTransform, 0.02f);
            //    Gizmos.DrawSphere(rightFoot.boneTransform, 0.02f);
            //    Gizmos.DrawSphere(leftFoot.boneTransform, 0.02f);
            //}
            //Gizmos.color = Color.red;
            //if (animator)
            //{
            //    Gizmos.DrawSphere(animator.GetBoneTransform(HumanBodyBones.RightHand).position, 0.02f);
            //    Gizmos.DrawSphere(animator.GetBoneTransform(HumanBodyBones.LeftHand).position, 0.02f);
            //    Gizmos.DrawSphere(animator.GetBoneTransform(HumanBodyBones.RightFoot).position, 0.02f);
            //    Gizmos.DrawSphere(animator.GetBoneTransform(HumanBodyBones.LeftFoot).position, 0.02f);
            //}
            Gizmos.color = new Vector4(1, 1, 1, 0.1f);
            if (currentPoint)
                Gizmos.DrawSphere(GetHandPos(currentPoint.transform, AvatarTarget.RightHand), 0.02f);

            var converted = (direction.x * transform.right + direction.y * transform.up).normalized;
            if (!currentPoint)
                return;

            var hitColliders = Physics.OverlapSphere(currentPoint.transform.position + converted, 1f, envScanner.LedgeLayer);


            Gizmos.DrawSphere(currentPoint.transform.position + converted, 1f);



            List<ClimbPoint> points = new List<ClimbPoint>();
            foreach (var hitCollider in hitColliders)
            {
                //hitCollider.SendMessage("AddDamage");
                points.AddRange(hitCollider.gameObject.GetComponentsInChildren<ClimbPoint>());

            }

            var origin = currentPoint.transform.position;
            //points = FindObjectsByType<ClimbPoint>(sortMode: FindObjectsSortMode.None).ToList();
            //var distance = Mathf.Infinity;
            var distance = 2f;

            List<ClimbPoint> overlapPoint = new List<ClimbPoint>();

            foreach (var point in points)
            {
                Vector3 point2origin = point.transform.position - origin;


                var projected = Vector3.ProjectOnPlane(point2origin, currentPoint.transform.forward);

                var projected2 = Vector3.ProjectOnPlane(converted, currentPoint.transform.forward);

                var projected3 = Vector3.ProjectOnPlane(point2origin, currentPoint.transform.up);

                var projected4 = Vector3.ProjectOnPlane(converted, currentPoint.transform.up);

                var angleX = Vector3.Angle(projected, projected2);

                //var angleZ = Vector3.Angle(projected3, projected4);

                var angleZ = Vector3.SignedAngle(projected3, projected4, Vector3.up);

                point.gameObject.name = angleX.ToString() + " " + angleZ.ToString();
                //angleX = Vector3.Angle(converted, point2origin);

                //if (point2origin.magnitude > 2f)
                //    continue;

                //if (Vector3.Angle(converted, point2origin) > 22.5f)
                //continue;
                if (Vector3.Angle(currentPoint.transform.forward, point.transform.forward) > 100f) continue;
                //if (angleZ > 100f) continue;

                if (Vector3.Angle(currentPoint.transform.forward, point.transform.forward) > 45f && point2origin.magnitude > 1f) continue;

                if (angleX < 30f && point2origin.magnitude < distance && point != currentPoint)
                {
                    if (angleX < 22.5f || overlapPoint.Count > 0)
                    {
                        if (!RayCastCheck(currentPoint, point))
                        {
                            overlapPoint.Insert(0, point);
                            distance = point2origin.magnitude;
                        }

                    }
                }
            }

            if (overlapPoint.Count > 0)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawSphere(overlapPoint[0].transform.position, 0.1f);
            }

            return;
        }

        //distance to point
        public float GetDistPointToLine(Vector3 origin, Vector3 direction, Vector3 point)
        {
            direction.Normalize();
            Vector3 point2origin = point - origin;
            Vector3 point2closestPointOnLine = point2origin - Vector3.Dot(point2origin, direction) * direction;
            return point2closestPointOnLine.magnitude;
        }


        ClimbPoint GetNearestPoint(Transform ledge, Vector3 hitPoint, bool checkAngle = true, Vector3? checkDirection = null, bool obstacleCheck = true, Vector3? predictiveMaxHeight = null)
        {
            var points = ledge.GetComponentsInChildren<ClimbPoint>();
            //checkDirection = checkDirection == null ? player.MoveDir.normalized : checkDirection.Value;
            checkDirection = !checkDirection.HasValue ? player.MoveDir.normalized : checkDirection.Value;

            if (checkDirection.Value == Vector3.zero) checkDirection = transform.forward;
            //checkDirection = transform.forward;


            ClimbPoint nearestPoint = null;
            float minDistance = 2.0f;
            foreach (var point in points)
            {
                var distance = Vector3.Distance(point.transform.position, hitPoint);
                //if (point == currentPoint) continue;
                if (checkAngle)
                {
                    //var angle = Vector3.SignedAngle(player.MoveDir, point.transform.forward, Vector3.up);
                    //if (angle < 90 || angle < -90) continue;
                    var angle = Vector3.Dot(checkDirection.Value, point.transform.forward);
                    if (angle >= 0)
                        continue;
                }

                var pointForward = point.transform.position + point.transform.forward * 0.2f + Vector3.down * 0.4f;

                //GizmosExtend.AddByName("babu", () => GizmosExtend.drawSphere(pointForward, 0.05f, Color.red));
                //if (predictiveMaxHeight != null)
                //{
                //    var maxHeightPoint = transform.position + (currentPoint.transform.position - transform.position) / 2 +  predictiveMaxHeight;
                // if (Physics.Linecast(transform.position + Vector3.one, pointForward, envScanner.ObstacleLayer))
                //}
                if (obstacleCheck)
                {
                    if (!CheckSpaceForClimb(point) || Physics.Linecast(transform.position + Vector3.up, pointForward, envScanner.ObstacleLayer))
                        continue;
                }


                if (distance < minDistance && distance >= 0.03f)
                {
                    minDistance = distance;
                    nearestPoint = point;
                }
            }

            return nearestPoint;
        }

        private void OnGUI()
        {

        }
    }
}
