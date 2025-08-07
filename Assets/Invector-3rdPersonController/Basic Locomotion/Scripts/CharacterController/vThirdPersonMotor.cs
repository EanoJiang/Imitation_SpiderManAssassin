using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using System;

namespace Invector.vCharacterController
{
    using UnityEngine.Serialization;
    using vEventSystems;

    public class vThirdPersonMotor : vCharacter, vIAnimatorStateInfoController
    {
        #region Variables               

        #region Stamina       

        [vEditorToolbar("Stamina", order = 2)]
        [SerializeField] protected float _maxStamina = 200f;
        public virtual float maxStamina { get { return _maxStamina; } set { _maxStamina = value; } }
        [SerializeField] protected float _staminaRecovery = 1.2f;
        public virtual float staminaRecovery { get { return _staminaRecovery; } set { _staminaRecovery = value; } }
        internal float currentStamina;
        internal float currentStaminaRecoveryDelay;
        [SerializeField] protected float _sprintStamina = 30f;
        public virtual float sprintStamina { get { return _sprintStamina; } set { _sprintStamina = value; } }
        [SerializeField] protected float _jumpStamina = 30f;
        public virtual float jumpStamina { get { return _jumpStamina; } set { _jumpStamina = value; } }
        [SerializeField] protected float _rollStamina = 25f;
        public virtual float rollStamina { get { return _rollStamina; } set { _rollStamina = value; } }

        [vEditorToolbar("Events", order = 7)]
        public UnityEvent OnExitGround;
        public UnityEvent OnGrounded;
        public UnityEvent OnRoll;
        public UnityEvent OnJump;
        public UnityEvent OnStartSprinting;
        public UnityEvent OnFinishSprinting;
        public UnityEvent OnFinishSprintingByStamina;
        public UnityEvent OnStaminaEnd;

        #endregion

        #region Crouch
        [vEditorToolbar("Crouch", order = 3)]
        [Tooltip("Capsule radius reduction while in crouch state")]
        [Range(0, 1f)]
        public float capsuleHeightReductionCrouched = 0.5f;
        [Tooltip("Capsule height reduction while in crouch state")]
        [Range(0, 1f)]
        public float capsuleRadiusReductionCrouched = 0f;

        [Tooltip("What objects can make the character auto crouch")]
        public LayerMask autoCrouchLayer = 1 << 0;
        [Tooltip("[SPHERECAST] ADJUST IN PLAY MODE - White Spherecast put just above the head, this will make the character Auto-Crouch if something hit the sphere.")]
        public float crouchHeadDetect = 0.95f;
        #endregion

        #region Character Variables   
        [vEditorToolbar("Physics Material")]
        public PhysicMaterial idleMaterial;
        public PhysicMaterial movingMaterial;
        public PhysicMaterial airborneMaterial;
        [vEditorToolbar("Locomotion", order = 0)]

        [vSeparator("Movement Settings")]
        [Tooltip("Multiply the current speed of the controller rigidbody velocity")]
        [SerializeField] protected float _speedMultiplier = 1;
        public virtual float speedMultiplier { get { return _speedMultiplier; } set { _speedMultiplier = value; } }
        [Tooltip("Use this to rotate the character using the World axis, or false to use the camera axis - CHECK for Isometric Camera")]
        public bool rotateByWorld = false;
        [Tooltip("The Character always  move to forward direction and rotate to Input direction when in free locomotion.\nUse this to move and rotate the character to Input direction when in free locomotion")]
        public bool moveToDirectionInFree;
        public enum LocomotionType
        {
            FreeWithStrafe,
            OnlyStrafe,
            OnlyFree,
        }

        [vHelpBox("FreeLocomotion: Rotate on any direction regardless of the camera \nStrafeLocomotion: Move always facing forward (extra directional animations)")]

        [SerializeField, FormerlySerializedAs("locomotionType")] protected LocomotionType _locomotionType = LocomotionType.FreeWithStrafe;
        public virtual LocomotionType locomotionType { get { return _locomotionType; } set { _locomotionType = value; } }

        public vMovementSpeed freeSpeed, strafeSpeed;

        [vSeparator("Extra Animation Settings")]

        [Tooltip("Use it for debug purposes")]
        public bool disableAnimations;
        [Tooltip("Turn off if you have 'in place' animations and use this values above to move the character, or use with root motion as extra speed")]
        [vHelpBox("When 'Use RootMotion' is checked, make sure to reset all speeds to zero to use the original root motion velocity.")]
        public bool useRootMotion = false;
        [Tooltip("While in Free Locomotion the character will lean to left/right when steering")]
        public bool useLeanMovementAnim = true;
        [Tooltip("Smooth value for the Lean Movement animation")]
        [Range(0.01f, 0.1f)]
        public float leanSmooth = 0.05f;
        [Tooltip("Check this to use the TurnOnSpot animations while the character is stading still and rotating in place")]
        public bool useTurnOnSpotAnim = true;
        public float turnOnSpotSmooth = 0.01f;
        [Tooltip("Put your Random Idle animations at the AnimatorController and select a value to randomize, 0 is disable.")]
        public float randomIdleTime = 0f;


        /// <summary>
        /// ignore animation root motion when input is zero
        /// </summary>
        internal bool ignoreAnimatorMovement;

        [vSeparator("Extra Movement Settings")]
        [Tooltip("Check This to use sprint on press button to your Character run until the stamina finish or movement stops\nIf uncheck your Character will sprint as long as the SprintInput is pressed or the stamina finishes")]
        public bool useContinuousSprint = true;
        [Tooltip("Check this to sprint always in free movement")]
        public bool sprintOnlyFree = true;

        public enum CustomFixedTimeStep { Default, FPS30, FPS60, FPS75, FPS90, FPS120, FPS144 };

        [vHelpBox("Set the FixedTimeStep to match the FPS of your Game, \nEx: If your game aims to run at 30fps, select FPS30 to match the FixedUpdate Physics")]
        public CustomFixedTimeStep customFixedTimeStep = CustomFixedTimeStep.FPS60;

        [vEditorToolbar("Jump / Airborne", order = 3)]

        [vHelpBox("Jump only works via Rigidbody Physics, if you want Jump that use only RootMotion make sure to use the AnimatorTag 'CustomAction' ")]

        [vSeparator("Jump")]
        [Tooltip("Use the currently Rigidbody Velocity to influence on the Jump Distance")]
        public bool jumpWithRigidbodyForce = false;
        [Tooltip("Rotate or not while airborne")]
        public bool jumpAndRotate = true;
        [Tooltip("How much time the character will be jumping")]
        public float jumpTimer = 0.3f;
        [Tooltip("Delay to match the animation anticipation")]
        public float jumpStandingDelay = 0.25f;
        internal float jumpCounter;
        internal bool inJumpStarted;
        [Tooltip("Add Extra jump height, if you want to jump only with Root Motion leave the value with 0.")]
        [SerializeField] protected float _jumpHeight = 4f;
        public virtual float jumpHeight { get { return _jumpHeight; } set { _jumpHeight = value; } }

        [vSeparator("Falling")]
        [Tooltip("Speed that the character will move while airborne")]
        [SerializeField] protected float _airSpeed = 5f;
        public virtual float airSpeed { get { return _airSpeed; } set { _airSpeed = value; } }

        [Tooltip("Smoothness of the direction while airborne")]
        [SerializeField] protected float _airSmooth = 6f;
        public virtual float airSmooth { get { return _airSmooth; } set { _airSmooth = value; } }
        [Tooltip("Apply extra gravity when the character is not grounded")]
        [SerializeField] protected float _extraGravity = -10f;
        public virtual float extraGravity { get { return _extraGravity; } set { _extraGravity = value; } }
        [Tooltip("Limit of the vertical velocity when Falling")]
        [SerializeField] protected float _limitFallVelocity = -15f;
        public virtual float limitFallVelocity { get { return _limitFallVelocity; } set { _limitFallVelocity = value; } }
        [Tooltip("Turn the Ragdoll On when falling at high speed (check VerticalVelocity) - leave the value with 0 if you don't want this feature")]
        [SerializeField] protected float _ragdollVelocity = -15f;
        public virtual float ragdollVelocity { get { return _ragdollVelocity; } set { _ragdollVelocity = value; } }

        [vSeparator("Fall Damage")]
        [SerializeField] protected float _fallMinHeight = 6f;
        public virtual float fallMinHeight { get { return _fallMinHeight; } set { _fallMinHeight = value; } }
        [SerializeField] protected float _fallMinVerticalVelocity = -10f;
        public virtual float fallMinVerticalVelocity { get { return _fallMinVerticalVelocity; } set { _fallMinVerticalVelocity = value; } }
        [SerializeField] protected float _fallDamage = 10f;
        public virtual float fallDamage { get { return _fallDamage; } set { _fallDamage = value; } }

        [vEditorToolbar("Roll", order = 4)]
        public bool useRollRootMotion = true;
        [Tooltip("Animation Transition from current animation to Roll")]
        public float rollTransition = .25f;
        [Tooltip("Capsule height reduction while in roll state")]
        [Range(0, 1)]
        public float capsuleHeightReductionRolling = 0.5f;
        [Tooltip("Capsule radius reduction while in roll state")]
        [Range(0, 1)]
        public float capsuleRadiusReductionRolling = 0f;
        [Tooltip("Can control the Roll Direction")]
        public bool rollControl = true;
        [Tooltip("Speed of the Roll Movement")]
        [SerializeField] protected float _rollSpeed = 0f;
        public virtual float rollSpeed { get { return _rollSpeed; } set { _rollSpeed = value; } }

        [Tooltip("Speed of the Roll Rotation")]
        [SerializeField] protected float _rollRotationSpeed = 20f;
        public virtual float rollRotationSpeed { get { return _rollRotationSpeed; } set { _rollRotationSpeed = value; } }
        [Tooltip("Extra gravity when rolling, this value can be negative to fall faster OR positive to delay the falling")]
        [SerializeField] protected float _rollUseGravityTime = -10f;
        public virtual float rollExtraGravity { get { return _rollUseGravityTime; } set { _rollUseGravityTime = value; } }
        [Tooltip("Use the normalized time of the animation to know when you can roll again")]
        [Range(0, 1)]
        [SerializeField] protected float _timeToRollAgain = 0.75f;
        public virtual float timeToRollAgain { get { return _timeToRollAgain; } set { _timeToRollAgain = value; } }
        [Tooltip("Ignore all damage while is rolling, include Damage that ignore defense")]
        [SerializeField] protected bool _noDamageWhileRolling = true;
        public virtual bool noDamageWhileRolling { get { return _noDamageWhileRolling; } set { _noDamageWhileRolling = value; } }
        [Tooltip("Ignore damage that needs to activate ragdoll")]
        [SerializeField] protected bool _noActiveRagdollWhileRolling = true;
        public virtual bool noActiveRagdollWhileRolling { get { return _noActiveRagdollWhileRolling; } set { _noActiveRagdollWhileRolling = value; } }

        public enum StopMoveCheckMethod
        {
            RayCast, SphereCast, CapsuleCast
        }

        [vEditorToolbar("Grounded", order = 3)]

        [vSeparator("Ground Detection")]
        [SerializeField]
        [vReadOnly()] protected bool _isGrounded;

        [Tooltip("Layers that the character can walk on")]
        public LayerMask groundLayer = 1 << 0;
        public float groundMinDistance = 0.5f;
        public float groundMaxDistance = 0.1f;
        public float sphereCastRadius = 0.3f;
        public float castLengthAirborne = 1.5f;
        public float castLengthGrounded = 3f;

        public bool useSphereCast = true;
        public float planeSize = 0.25f;
        public float stepHeight = 0.3f;
        public float groundAngleSmooth = 4f;

        [vSeparator("Collider Options")]
        public bool useCeilingDetection = true;
        public bool lerpCapsuleAdjust = true;
        public float lerpCapsuleSmooth = 10;
        public Vector3 capsuleOffset = new Vector3(0, 0.5f, 0);
        public float capsuleThickness = .75f;
        public float capsuleHeight = 1.8f;

        [vSeparator("Snap to Ground")]
        public float snapFactor = 15f;
        public float maxSnapVelocityUp = 5;
        public float maxSnapVelocityDown = 15;
        public float snapFactorSpeed = 2f;
        public float slideSpeed = 1;

        protected vGroundPlaneCast groundPlane;
        protected RaycastHit groundAngleHit;
        protected RaycastHit stepAheadHit;
        protected float stepHeightRatio;
        protected float targetHeightRatio;
        protected float lastTrueSlope;
        protected float groundPlaneProjectionWeight;
        protected float slopeSlideFactor;
        protected float heightReduction;
        protected float radiusReduction;

        private bool _isOnSlope;
        public bool isOnSlope
        {
            get { return _isOnSlope; }
            private set
            {
                var _value = value;
                float stepHeight = isInAirborne ? 0 : this.stepHeight;

                if (value && CheckStepAhead())
                {
                    stepHeight = 0f;
                }

                if (_value != _isOnSlope || stepHeight != stepHeightRatio)
                {
                    if (_value == true || lastTrueSlope < Time.time)
                    {
                        _isOnSlope = _value;

                        if (_value == true)
                        {
                            lastTrueSlope = Time.time + 0.5f;
                        }
                    }
                }
                targetHeightRatio = stepHeight;
            }
        }


        [vSeparator("StopMove")]

        public LayerMask stopMoveLayer;
        [vHelpBox("Character will stop moving, ex: walls - set the layer to nothing to not use")]
        public float stopMoveRayDistance = 1f;
        public float stopMoveMaxHeight = 1.6f;
        public StopMoveCheckMethod stopMoveCheckMethod = StopMoveCheckMethod.RayCast;


        [vSeparator("Slope")]

        [Range(30, 80)]
        [SerializeField] protected float _slopeLimit = 75f;
        public virtual float slopeLimit { get { return _slopeLimit; } set { _slopeLimit = value; } }

        [vHelpBox("Slide on Slopes is not available yet, coming soon")]
        [SerializeField] protected bool _useSlide = true;
        public virtual bool useSlide { get { return _useSlide; } set { _useSlide = value; } }
        [Tooltip("Velocity to slide down when on a slope limit ramp")]
        [Range(0, 30)]
        [SerializeField] protected float _slideDownVelocity = 10f;
        public virtual float slideDownVelocity { get { return _slideDownVelocity; } set { _slideDownVelocity = value; } }
        [Tooltip("Smooth to slide down the controller")]
        [SerializeField] protected float _slideDownSmooth = 2f;
        public virtual float slideDownSmooth { get { return _slideDownSmooth; } set { _slideDownSmooth = value; } }
        [Tooltip("Velocity to slide sideways when on a slope limit ramp")]
        [Range(0, 1)]
        [SerializeField] protected float _slideSidewaysVelocity = 0.5f;
        public virtual float slideSidewaysVelocity { get { return _slideSidewaysVelocity; } set { _slideSidewaysVelocity = value; } }
        [Range(0f, 1f)]
        [Tooltip("Delay to start sliding once the character is standing on a slope")]
        [SerializeField] protected float _SlidingEnterTime = 0.2f;
        public virtual float slidingEnterTime { get { return _SlidingEnterTime; } set { _SlidingEnterTime = value; } }

        internal float _slidingEnterTime;
        [Range(0f, 1f)]
        [Tooltip("Delay to rotate once the character started sliding")]
        [SerializeField] protected float _RotateSlopeEnterTime = 0.1f;
        public virtual float rotateSlopeEnterTime { get { return _RotateSlopeEnterTime; } set { _RotateSlopeEnterTime = value; } }
        [Tooltip("Smooth to rotate the controller")]
        [SerializeField] protected float _rotateDownSlopeSmooth = 8f;
        public virtual float rotateDownSlopeSmooth { get { return _rotateDownSlopeSmooth; } set { _rotateDownSlopeSmooth = value; } }

        internal float _rotateSlopeEnterTime;
        internal float _stopMoveWeight;
        internal virtual float stopMoveWeight { get { return _stopMoveWeight; } set { _stopMoveWeight = value; } }
        internal float _sprintWeight;
        internal virtual float sprintWeight { get { return _sprintWeight; } set { _sprintWeight = value; } }
        internal float groundDistance;
        public RaycastHit groundHit;

        [vEditorToolbar("Debug", order = 9)]
        [Header("--- Debug Info ---")]
        public bool debugWindow;
        [vReadOnly]
        public float currentSnapFactor;
        [vReadOnly]
        public bool rayHasDetectedHit;
        [vReadOnly]
        public bool sphereHasDetectedHit;
        public vAnimatorStateInfos _animatorStateInfos;
        public vAnimatorStateInfos animatorStateInfos { get => _animatorStateInfos; protected set => _animatorStateInfos = value; }


        #endregion

        #region Actions

        public virtual bool isStrafing
        {
            get
            {
                return sprintOnlyFree && isSprinting ? false : _isStrafing;
            }
            set
            {
                _isStrafing = value;
            }
        }

        // movement bools

        public bool isGrounded
        {
            get
            {
                return _isGrounded;
            }
            set
            {
                if (_isGrounded != value)
                {
                    _isGrounded = value;
                    if (_isGrounded) OnGrounded.Invoke();
                    else OnExitGround.Invoke();
                }
            }
        }
        /// <summary>
        /// use to stop update the Check Ground method and return true for IsGrounded
        /// </summary>
        public bool disableCheckGround { get; set; }
        public bool inCrouchArea { get; protected set; }
        protected bool _isSprinting = false;
        public virtual bool isSprinting { get { return _isSprinting; } set { _isSprinting = value; } }
        public bool isSliding { get; protected set; }
        public bool autoCrouch { get; protected set; }

        // action bools
        internal bool
            isRolling,
            isJumping,
            isInAirborne,
            isTurningOnSpot;

        internal bool customAction;


        protected void RemoveComponents()
        {
            if (!removeComponentsAfterDie)
            {
                return;
            }

            if (_capsuleCollider != null)
            {
                Destroy(_capsuleCollider);
            }

            if (_rigidbody != null)
            {
                Destroy(_rigidbody);
            }

            if (animator != null)
            {
                Destroy(animator);
            }

            var comps = GetComponents<MonoBehaviour>();
            for (int i = 0; i < comps.Length; i++)
            {
                Destroy(comps[i]);
            }
        }

        #endregion      

        #region Components

        internal Rigidbody _rigidbody;                                                      // access the Rigidbody component
        internal float changeMaterialPhysics = 0;
        internal virtual PhysicMaterial MovingPhysicsMaterial => movingMaterial;
        internal virtual PhysicMaterial IdlePhysicsMaterial => idleMaterial;
        internal virtual PhysicMaterial AirbornePhysicsMaterial => airborneMaterial;         // create PhysicMaterial for the Rigidbody
        internal CapsuleCollider _capsuleCollider;                                          // access CapsuleCollider information
        public PhysicMaterial currentMaterialPhysics { get; protected set; }

        #endregion

        #region Hide Variables

        public virtual float defaultSpeedMultiplier { get; set; }       // storage the default speed multiplier
        public virtual float inputMagnitude { get; set; }               // sets the inputMagnitude to update the animations in the animator controller
        public virtual float rotationMagnitude { get; set; }            // sets the rotationMagnitude to update the animations in the animator controller
        public virtual float verticalSpeed { get; set; }                // set the verticalSpeed based on the verticalInput        
        public virtual float horizontalSpeed { get; set; }              // set the horizontalSpeed based on the horizontalInput
        public virtual bool invertVerticalSpeed { get; set; }           // used to invert vertical speed
        public virtual bool invertHorizontalSpeed { get; set; }         // used to invert horizontal speed
        public virtual float moveSpeed { get; set; }                    /// set the current moveSpeed for the<seealso cref="MoveCharacter(Vector3)"/> method        
        public virtual float verticalVelocity { get; set; }             // set the vertical velocity of the rigidbody
        public virtual float colliderRadius { get; set; }               // storage the capsule collider radius
        public virtual float colliderHeight { get; set; }               // storage capsule collider extra information                       
        public virtual float jumpMultiplier { get; set; }               // internally used to set the jumpMultiplier
        public virtual float timeToResetJumpMultiplier { get; set; }    // internally used to reset the jump multiplier
        public virtual float heightReached { get; set; }                // max height that character reached in air;        
        public virtual float lastCeilingContact { get; set; }           // used internally for ceiling detection
        public virtual bool hasCeilingContact { get; set; }             // used internally for ceiling detection
        public virtual float groundAngle { get; set; }                  // used to know the ground angle
        public virtual bool lockMovement { get; set; }                  // lock the movement of the controller (not the animation)
        public virtual bool lockRotation { get; set; }                  // lock the rotation of the controller (not the animation)
        public virtual bool lockSetMoveSpeed { get; set; }              // locks the method to update the moveset based on the locomotion type, so you can modify externally
        protected bool _isStrafing { get; set; }                        // internally used to set the strafe movement
        public virtual bool lockInStrafe { get; set; }                  // locks the controller to only used the strafe locomotion type        
        public virtual bool forceRootMotion { get; set; }               // force the controller to use root motion
        public virtual bool keepDirection { get; set; }                 // keeps the character direction even if the camera direction changes
        public virtual bool finishStaminaOnSprint { get; set; }         // used to trigger the OnFinishStamina event
        public virtual bool applyingStepOffset { get; set; }            // internally used to apply the StepOffset       
        public virtual bool lockAnimMovement { get; set; }              // internally used with the vAnimatorTag("LockMovement"), use on the animator to lock the movement of a specific animation clip        
        public virtual bool lockAnimRotation { get; set; }              // internally used with the vAnimatorTag("LockRotation"), use on the animator to lock a rotation of a specific animation clip       
        public virtual bool disableSnapToGround { get; set; }           // used to disable snap ground system. This will make the capsule full.
        public virtual Vector3 lastCharacterAngle { get; set; }         // last angle of the character used to calculate rotationMagnitude;
        public virtual Transform rotateTarget { get; set; }             // target reference to rotate
        public virtual Vector3 input { get; set; }                      // generate raw input for the controller
        public virtual Vector3 oldInput { get; set; }                   // used internally to identify oldinput from the current input
        public virtual Vector3 colliderCenter { get; set; }             // storage the center of the capsule collider info                
        public virtual Vector3 inputSmooth { get; set; }                // generate smooth input based on the inputSmooth value
        public virtual Vector3 moveDirection { get; set; }              // used to idenfify the direction the controller will move 
        public virtual Vector3 targetVelocity { get; set; }
        public virtual float snapDistance { get; set; }
        public virtual bool validSnapPoint { get; set; }
        public virtual Vector3 rayOrigin => transform.position + transform.up * ((capsuleHeight * stepHeight) + _capsuleCollider.radius);
        public RaycastHit stepOffsetHit;
        public RaycastHit slopeHitInfo;

        internal AnimatorStateInfo baseLayerInfo, underBodyInfo, rightArmInfo, leftArmInfo, fullBodyInfo, upperBodyInfo;

        public virtual int baseLayer { get { return animator.GetLayerIndex("Base Layer"); } }
        public virtual int underBodyLayer { get { return animator.GetLayerIndex("UnderBody"); } }
        public virtual int rightArmLayer { get { return animator.GetLayerIndex("RightArm"); } }
        public virtual int leftArmLayer { get { return animator.GetLayerIndex("LeftArm"); } }
        public virtual int upperBodyLayer { get { return animator.GetLayerIndex("UpperBody"); } }
        public virtual int fullbodyLayer { get { return animator.GetLayerIndex("FullBody"); } }

        /// <summary>
        /// Default radius of the Character Capsule
        /// </summary>
        public virtual float colliderRadiusDefault
        {
            get; protected set;
        }
        /// <summary>
        /// Default Height of the Character Capsule
        /// </summary>
        public virtual float colliderHeightDefault
        {
            get; protected set;
        }
        /// <summary>
        /// Default Center of the Character Capsule
        /// </summary>
        public virtual Vector3 colliderCenterDefault
        {
            get; protected set;
        }

        /// <summary>
        ///Check if Can Apply Fall Damage and/or Enable Ragdoll when landing. <see cref="jumpMultiplier"/>  automatically return false if > 1 or if <seealso cref="customAction"/> is true
        /// </summary>
        protected virtual bool _canApplyFallDamage { get { return !blockApplyFallDamage && jumpMultiplier <= 1 && !customAction; } }
        /// <summary>
        /// For movement to walk by default. 
        /// </summary>
        public virtual bool alwaysWalkByDefault { get; set; }
        /// <summary>
        /// Can Apply Fall Damage and/or Enable Ragdoll when landing;
        /// </summary>
        public virtual bool blockApplyFallDamage { get; set; }

        #endregion

        #endregion

        #region Initilize Motor

        protected override void OnValidate()
        {
            base.OnValidate();

            if (!Application.isPlaying)
            {
                targetHeightRatio = stepHeight;
                stepHeightRatio = stepHeight;
                ControlCapsuleHeight();
            }
        }

        protected virtual void Awake()
        {
            defaultSpeedMultiplier = 1;
            jumpMultiplier = 1;
            heightReached = transform.position.y;
            SetCustomFixedTimeStep();
        }

        protected override void Start()
        {
            base.Start();
        }

        public override void Init()
        {
            base.Init();

            animator.updateMode = AnimatorUpdateMode.AnimatePhysics;
            // rigidbody info
            _rigidbody = GetComponent<Rigidbody>();

            // capsule collider info
            _capsuleCollider = GetComponent<CapsuleCollider>();

            // save your collider preferences 
            colliderCenter = colliderCenterDefault = _capsuleCollider.center;
            colliderRadius = colliderRadiusDefault = _capsuleCollider.radius;
            colliderHeight = colliderHeightDefault = _capsuleCollider.height;

            // avoid collision detection with inside colliders 
            Collider[] AllColliders = this.GetComponentsInChildren<Collider>();
            for (int i = 0; i < AllColliders.Length; i++)
            {
                Physics.IgnoreCollision(_capsuleCollider, AllColliders[i]);
            }

            // health info
            if (fillHealthOnStart)
            {
                currentHealth = maxHealth;
            }

            currentHealthRecoveryDelay = healthRecoveryDelay;
            currentStamina = maxStamina;
            ResetJumpMultiplier();
            isGrounded = true;
            ResetControllerSpeedMultiplier();
            freeSpeed.Init();
            strafeSpeed.Init();
            ControlCapsuleHeight();
        }

        public virtual void SetCustomFixedTimeStep()
        {
            switch (customFixedTimeStep)
            {
                case CustomFixedTimeStep.Default:
                    break;
                case CustomFixedTimeStep.FPS30:
                    Time.fixedDeltaTime = 0.03333334f;
                    break;
                case CustomFixedTimeStep.FPS60:
                    Time.fixedDeltaTime = 0.01666667f;
                    break;
                case CustomFixedTimeStep.FPS75:
                    Time.fixedDeltaTime = 0.01333333f;
                    break;
                case CustomFixedTimeStep.FPS90:
                    Time.fixedDeltaTime = 0.01111111f;
                    break;
                case CustomFixedTimeStep.FPS120:
                    Time.fixedDeltaTime = 0.008333334f;
                    break;
                case CustomFixedTimeStep.FPS144:
                    Time.fixedDeltaTime = 0.006944444f;
                    break;
            }
        }

        #endregion

        protected virtual void OnEnable()
        {

        }

        /// <summary>
        /// When Disabling the Controller Component we change the Capsule Collider to Fullsize to avoid sinking in the ground
        /// </summary>
        protected virtual void OnDisable()
        {
            SetFullCapsuleHeight();
        }

        public virtual void UpdateMotor()
        {
            CheckStamina();
            CheckGround();
            CheckRagdoll();

            SlideMovementBehavior();

            ControlCapsuleHeight();
            ControlJumpBehaviour();
            ApplyAirMovement();
            ApplyMovement(targetVelocity);
            ApplyExtraGravity();
            StaminaRecovery();
            HealthRecovery();
            CalculateRotationMagnitude();
        }

        #region Health & Stamina

        public override void TakeDamage(vDamage damage)
        {
            // don't apply damage if the character is rolling, you can add more conditions here
            if (currentHealth <= 0 || IgnoreDamageRolling())
            {
                if (damage.activeRagdoll && !IgnoreDamageActiveRagdollRolling())
                {
                    onActiveRagdoll.Invoke(damage);
                }

                return;
            }

            if (damage.activeRagdoll && IgnoreDamageActiveRagdollRolling())
            {
                damage.activeRagdoll = false;
            }

            base.TakeDamage(damage);
        }

        protected virtual bool IgnoreDamageRolling()
        {
            return noDamageWhileRolling == true && isRolling == true;
        }

        protected virtual bool IgnoreDamageActiveRagdollRolling()
        {
            return noActiveRagdollWhileRolling == true && isRolling == true;
        }

        protected override void TriggerDamageReaction(vDamage damage)
        {
            if (!customAction)
            {
                base.TriggerDamageReaction(damage);
            }
            else if (damage.activeRagdoll)
            {
                onActiveRagdoll.Invoke(damage);
            }
        }

        public virtual void ReduceStamina(float value, bool accumulative)
        {
            if (customAction)
            {
                return;
            }

            if (accumulative)
            {
                currentStamina -= value * Time.fixedDeltaTime;
            }
            else
            {
                currentStamina -= value;
            }

            if (currentStamina < 0)
            {
                currentStamina = 0;
                OnStaminaEnd.Invoke();
            }
        }

        /// <summary>
        /// Change the currentStamina of Character
        /// </summary>
        /// <param name="value"></param>
        public virtual void ChangeStamina(int value)
        {
            currentStamina += value;
            currentStamina = Mathf.Clamp(currentStamina, 0, maxStamina);
        }

        /// <summary>
        /// Change the MaxStamina of Character
        /// </summary>
        /// <param name="value"></param>
        public virtual void ChangeMaxStamina(int value)
        {
            maxStamina += value;
            if (maxStamina < 0)
            {
                maxStamina = 0;
            }
        }

        public override bool isDead
        {
            get => base.isDead;
            set
            {
                base.isDead = value;
                if (value)
                {
                    if (isGrounded)
                    {
                        if (_rigidbody) _rigidbody.isKinematic = true;
                        if (_capsuleCollider) _capsuleCollider.enabled = false;
                    }
                }
                else if (!ragdolled)
                {
                    if (_rigidbody) _rigidbody.isKinematic = false;
                    if (_capsuleCollider) _capsuleCollider.enabled = true;
                }
            }
        }

        protected virtual void CheckStamina()
        {
            // check how much stamina this action will consume
            if (isSprinting)
            {
                currentStaminaRecoveryDelay = 0.25f;
                ReduceStamina(sprintStamina, true);
            }
        }

        public virtual void StaminaRecovery()
        {
            if (currentStaminaRecoveryDelay > 0)
            {
                currentStaminaRecoveryDelay -= Time.fixedDeltaTime;
            }
            else
            {
                if (currentStamina > maxStamina)
                {
                    currentStamina = maxStamina;
                }

                if (currentStamina < maxStamina)
                {
                    currentStamina += staminaRecovery;
                }
            }
        }

        #endregion

        #region Locomotion

        /// <summary>
        /// Check if <see cref="input"/> and <see cref="inputSmooth"/> has some value greater than 0.1f
        /// </summary>
        public virtual bool hasMovementInput
        {
            get => (inputSmooth.sqrMagnitude + input.sqrMagnitude) > 0.1f || (input - inputSmooth).sqrMagnitude > 0.1f;
        }

        /// <summary>
        /// Calculates the rotation magnitude based on the difference between the current rotation of an object and the last recorded rotation.        
        /// </summary>
        protected virtual void CalculateRotationMagnitude()
        {
            var eulerDifference = this.transform.eulerAngles - lastCharacterAngle;
            if (eulerDifference.sqrMagnitude < 0.01)
            {
                lastCharacterAngle = transform.eulerAngles;
                rotationMagnitude = 0f;
                return;
            }

            var magnitude = eulerDifference.NormalizeAngle().y / (isStrafing ? strafeSpeed.rotationSpeed : freeSpeed.rotationSpeed);
            rotationMagnitude = (float)System.Math.Round(magnitude, 2);
            lastCharacterAngle = transform.eulerAngles;
        }

        /// <summary>
        /// Sets the controller speed multiplier to the provided speed value.
        /// </summary>
        /// <param name="speed">The value to set the speedMultiplier variable to.</param>
        public virtual void SetControllerSpeedMultiplier(float speed)
        {
            this.speedMultiplier = speed;
        }

        /// <summary>
        /// Resets the controller speed multiplier to its default value.
        /// </summary>
        public virtual void ResetControllerSpeedMultiplier()
        {
            this.speedMultiplier = defaultSpeedMultiplier;
        }

        /// <summary>
        /// Sets the move speed of a controller based on the provided vMovementSpeed object.
        /// </summary>
        /// <param name="speed">The speed object that contains different movement speeds for the controller.</param>
        public virtual void SetControllerMoveSpeed(vMovementSpeed speed)
        {
            if (isCrouching)
            {
                moveSpeed = Mathf.Lerp(moveSpeed, speed.crouchSpeed, speed.movementSmooth * Time.fixedDeltaTime);
                return;
            }

            if (speed.walkByDefault || alwaysWalkByDefault)
            {
                moveSpeed = Mathf.Lerp(moveSpeed, isSprinting ? speed.runningSpeed : speed.walkSpeed, speed.movementSmooth * Time.fixedDeltaTime);
            }
            else
            {
                moveSpeed = Mathf.Lerp(moveSpeed, isSprinting ? speed.sprintSpeed : speed.runningSpeed, speed.movementSmooth * Time.fixedDeltaTime);
            }
        }

        /// <summary>
        /// Moves the character in a specified direction.
        /// </summary>
        /// <param name="direction">The direction in which the character should move.</param>
        public virtual void MoveCharacter(Vector3 direction)
        {
            inputSmooth = Vector3.Lerp(inputSmooth, input, (isStrafing ? strafeSpeed.movementSmooth : freeSpeed.movementSmooth) * (useRootMotion ? vTime.deltaTime : vTime.fixedDeltaTime));
            isOnSlope = CheckSlope();

            if (MoveCharacterConditions())
            {
                var _direction = isStrafing || moveToDirectionInFree ? direction : transform.forward;
                _direction.y = 0;
                _direction = _direction.normalized * Mathf.Clamp(direction.magnitude, 0, 1f);

                Vector3 targetPosition = (useRootMotion ? animator.rootPosition : _rigidbody.position) + _direction * (useRootMotion ? vTime.deltaTime : vTime.fixedDeltaTime);
                Vector3 targetVelocity = (targetPosition - transform.position) / (useRootMotion ? vTime.deltaTime : vTime.fixedDeltaTime);

                // OPTIONA A Full RootMotion
                Vector3 targetBodyVelocity = useRootMotion ? animator.deltaPosition / vTime.fixedDeltaTime : targetVelocity * (moveSpeed * speedMultiplier);

                // OPTION B Smooth Rootmotion
                // float _moveSpeed = useRootMotion ? 1f + moveSpeed : moveSpeed;
                // Vector3 targetBodyVelocity = targetVelocity * (_moveSpeed * speedMultiplier);

                targetBodyVelocity.y = 0;
                this.targetVelocity = targetBodyVelocity;
            }
        }

        /// <summary>
        /// Checks the conditions to move the character.
        /// </summary>
        /// <returns>A boolean value indicating whether the character can move or not.</returns>
        public virtual bool MoveCharacterConditions()
        {
            return isGrounded && !isSliding && !ragdolled && !isJumping && !isRolling && !_rigidbody.isKinematic;
        }

        /// <summary>
        /// Applies movement to the controller.
        /// </summary>
        /// <param name="targetVelocity">The desired velocity to apply to the character.</param>
        public virtual void ApplyMovement(Vector3 targetVelocity)
        {
            CheckStopMove(ref targetVelocity);

            if (ApplyMovementConditions())
            {
                if (isJumping || isInAirborne || disableSnapToGround)
                {
                    targetVelocity.y = _rigidbody.velocity.y;
                }
                else
                {
                    targetVelocity.y = 0;
                }

                if (!isJumping && !isInAirborne && validSnapPoint && !disableSnapToGround)
                {
                    targetVelocity = ProjectOnGround(targetVelocity);
                    ApplyGroundMargin(ref targetVelocity);
                }

                _rigidbody.velocity = targetVelocity;
            }
        }

        /// <summary>
        /// Checks various movement conditions of the character and returns a boolean value indicating whether the conditions are met.
        /// </summary>
        /// <returns>A boolean value indicating whether the movement conditions are met.</returns>
        public virtual bool ApplyMovementConditions()
        {
            return isGrounded && !isSliding && !ragdolled && !isJumping && !_rigidbody.isKinematic;
        }

        /// <summary>
        /// Projects the targetBodyVelocity on the ground plane.
        /// </summary>
        /// <param name="targetBodyVelocity">The velocity of the character's body.</param>
        /// <returns>The modified targetBodyVelocity after being projected on the ground plane.</returns>
        private Vector3 ProjectOnGround(Vector3 targetBodyVelocity)
        {
            groundAngle = Mathf.Lerp(groundAngle, GroundAngleFromDirection(), groundAngleSmooth * Time.deltaTime);

            // project the velocity relative to the ground normal
            if (groundAngle < slopeLimit)
            {
                float _velMag = targetBodyVelocity.magnitude;
                var right = Quaternion.AngleAxis(90, Vector3.up) * targetBodyVelocity;
                right.y = 0f;
                var fwd = Quaternion.AngleAxis(-groundAngle, right) * targetBodyVelocity;
                groundPlaneProjectionWeight = Mathf.Clamp01(groundPlaneProjectionWeight + groundAngleSmooth * Time.deltaTime);
                targetBodyVelocity = Vector3.Lerp(targetBodyVelocity, fwd.normalized * _velMag, groundPlaneProjectionWeight);
            }
            else
            {
                groundPlaneProjectionWeight = 0f;
            }

            return targetBodyVelocity;
        }

        /// <summary>
        /// Checks if the character is on a slope by performing a sphere cast downwards and comparing the angle of the hit surface with the slope limit.
        /// </summary>
        /// <returns>A boolean value indicating whether the character is on a slope or not.</returns>
        protected virtual bool CheckSlope()
        {
            Vector3 _origin = _capsuleCollider.bounds.center;

            float radius = _capsuleCollider.radius * 0.99f;
            float castLength = castLengthGrounded - radius;
            /// Check floor slope
            if (Physics.SphereCast(_origin, radius, Vector3.down, out groundAngleHit, castLength, groundLayer, QueryTriggerInteraction.Ignore))
            {
                int angle = (int)Vector3.Angle(groundAngleHit.normal, transform.up);
                if (angle > slopeLimit && angle < 85)
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Checks if there is a step ahead of the character's current position.
        /// </summary>
        /// <returns>A boolean value indicating whether there is a step ahead of the character's current position.</returns>
        protected virtual bool CheckStepAhead()
        {
            var height = capsuleHeight * stepHeight;
            Vector3 _origin = transform.position + Vector3.up * height;
            Vector3 _direction = _capsuleCollider.transform.forward;
            float raycastDistance = 3f;
            Ray ray = new Ray(_origin, _direction);
            var slopeDirection = groundAngleHit.normal;
            slopeDirection.y = 0;
            slopeDirection.Normalize();
            var _newDirection = slopeDirection;
            ray.direction = -_newDirection;

            if (Physics.Raycast(ray, out stepAheadHit, raycastDistance, groundLayer, QueryTriggerInteraction.Ignore))
            {
                //Debug.DrawLine(ray.origin, stepAheadHit.point, Color.green);
                int angle = (int)Vector3.Angle(stepAheadHit.normal, transform.up);
                if (angle < 85)
                {
                    return true;
                }
                else
                {
                    if (Physics.Raycast(groundAngleHit.point - _direction.normalized * 0.1f, Vector3.down, out stepAheadHit, raycastDistance, groundLayer, QueryTriggerInteraction.Ignore))
                    {
                        angle = (int)Vector3.Angle(stepAheadHit.normal, transform.up);
                        if (angle > slopeLimit)
                        {
                            return true;
                        }

                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Checks if the character should stop moving based on certain conditions and adjusts the target velocity accordingly.
        /// </summary>
        /// <param name="targetVelocity">The current target velocity of the character.</param>
        protected virtual void CheckStopMove(ref Vector3 targetVelocity)
        {
            RaycastHit hit;
            Vector3 origin = transform.position + transform.up * colliderRadiusDefault;
            Vector3 direction = moveDirection.normalized;
            direction = Vector3.ProjectOnPlane(direction, groundHit.normal);

            float targetStopWeight = 0;
            float smooth = isStrafing ? strafeSpeed.movementSmooth : freeSpeed.movementSmooth;

            if (StopMoveConditions() && CheckStopMove(direction, out hit))
            {
                var angle = Vector3.Angle(direction, -hit.normal);
                if (angle < slopeLimit)
                {
                    float dst = hit.distance - colliderRadiusDefault;
                    targetStopWeight = 1.0f - dst;
                }
                else
                {
                    targetStopWeight = -0.01f;
                }

                if (debugWindow)
                {
                    Debug.DrawLine(origin, hit.point, Color.cyan);
                }
            }

            stopMoveWeight = Mathf.Lerp(stopMoveWeight, targetStopWeight, smooth * Time.deltaTime);
            stopMoveWeight = Mathf.Clamp(stopMoveWeight, 0f, 1f);

            targetVelocity = Vector3.LerpUnclamped(targetVelocity, Vector3.zero, stopMoveWeight);
        }

        /// <summary>
        /// Determines whether the character can stop moving based on various conditions.
        /// </summary>
        /// <returns>A boolean value indicating whether the character can stop moving.</returns>
        public virtual bool StopMoveConditions()
        {
            return isGrounded && !isJumping && !isInAirborne && !applyingStepOffset && !customAction;
        }

        /// <summary>
        /// Checks if the character should stop moving in a given direction by performing a raycast or capsule cast.
        /// </summary>
        /// <param name="direction">The direction in which the character is moving.</param>
        /// <param name="hit">An output parameter that stores information about the hit if there is one.</param>
        /// <returns>A boolean value indicating whether the character should stop moving.</returns>
        protected virtual bool CheckStopMove(Vector3 direction, out RaycastHit hit)
        {
            Vector3 origin = transform.position + transform.up * colliderRadiusDefault;
            float distance = colliderRadiusDefault + stopMoveRayDistance;
            switch (stopMoveCheckMethod)
            {
                case StopMoveCheckMethod.SphereCast:

                case StopMoveCheckMethod.CapsuleCast:
                    Vector3 p1 = origin + transform.up * (capsuleHeight * stepHeight);
                    Vector3 p2 = origin + transform.up * (stopMoveMaxHeight - _capsuleCollider.radius);
                    return Physics.CapsuleCast(p1, p2, _capsuleCollider.radius, direction, out hit, distance, stopMoveLayer);
                default:
                    return Physics.Raycast(origin, direction, out hit, distance, stopMoveLayer);
            }
        }

        /// <summary>
        /// Stops the character's movement by gradually reducing the input, velocity, and animation parameters to zero using linear interpolation.
        /// </summary>
        public virtual void StopCharacterWithLerp()
        {
            isSprinting = false;
            sprintWeight = 0f;
            horizontalSpeed = 0f;
            verticalSpeed = 0f;
            moveDirection = Vector3.zero;
            targetVelocity = Vector3.zero;
            input = Vector3.Lerp(input, Vector3.zero, 2f * Time.fixedDeltaTime);
            inputSmooth = Vector3.Lerp(inputSmooth, Vector3.zero, 2f * Time.fixedDeltaTime);
            if (!_rigidbody.isKinematic)
                _rigidbody.velocity = Vector3.Lerp(_rigidbody.velocity, Vector3.zero, 4f * Time.fixedDeltaTime);
            inputMagnitude = Mathf.Lerp(inputMagnitude, 0f, 2f * Time.fixedDeltaTime);
            moveSpeed = Mathf.Lerp(moveSpeed, 0f, 2f * Time.fixedDeltaTime);
            animator.SetFloat(vAnimatorParameters.InputMagnitude, 0f, 0.2f, Time.fixedDeltaTime);
            animator.SetFloat(vAnimatorParameters.InputVertical, 0f, 0.2f, Time.fixedDeltaTime);
            animator.SetFloat(vAnimatorParameters.InputHorizontal, 0f, 0.2f, Time.fixedDeltaTime);
            animator.SetFloat(vAnimatorParameters.RotationMagnitude, 0f, 0.2f, Time.fixedDeltaTime);
        }

        /// <summary>
        /// Stops the character's movement instantly by resetting various variables and setting the animator parameters to zero.
        /// </summary>
        public virtual void StopCharacter()
        {
            isSprinting = false;
            sprintWeight = 0f;
            horizontalSpeed = 0f;
            verticalSpeed = 0f;
            moveDirection = Vector3.zero;
            targetVelocity = Vector3.zero;
            input = Vector3.zero;
            inputSmooth = Vector3.zero;
            if (!_rigidbody.isKinematic)
                _rigidbody.velocity = Vector3.zero;
            inputMagnitude = 0f;
            moveSpeed = 0f;
            animator.SetFloat(vAnimatorParameters.InputMagnitude, 0f, 0.25f, Time.fixedDeltaTime);
            animator.SetFloat(vAnimatorParameters.InputVertical, 0f, 0.25f, Time.fixedDeltaTime);
            animator.SetFloat(vAnimatorParameters.InputHorizontal, 0f, 0.25f, Time.fixedDeltaTime);
            animator.SetFloat(vAnimatorParameters.RotationMagnitude, 0f, 0.25f, Time.fixedDeltaTime);
        }


        /// <summary>
        /// Rotates the controller to face a specified position.
        /// </summary>
        /// <param name="position">The target position to rotate towards.</param>
        public virtual void RotateToPosition(Vector3 position)
        {
            Vector3 desiredDirection = position - transform.position;
            RotateToDirection(desiredDirection.normalized);
        }


        /// <summary>
        /// Rotates the controller to face the specified direction.
        /// </summary>
        /// <param name="direction">The direction to rotate the character towards.</param>
        public virtual void RotateToDirection(Vector3 direction)
        {
            RotateToDirection(direction, isStrafing ? strafeSpeed.rotationSpeed : freeSpeed.rotationSpeed);
        }

        /// <summary>
        /// Rotates the controller to face a given direction at a specified rotation speed.
        /// </summary>
        /// <param name="direction">The direction to rotate the object towards.</param>
        /// <param name="rotationSpeed">The speed at which the rotation should occur.</param>
        public virtual void RotateToDirection(Vector3 direction, float rotationSpeed)
        {
            if (RotateToDirectionConditions())
            {
                direction.y = 0f;
                if (direction.normalized.magnitude == 0)
                {
                    direction = transform.forward;
                }

                var euler = transform.rotation.eulerAngles.NormalizeAngle();
                var targetEuler = Quaternion.LookRotation(direction.normalized).eulerAngles.NormalizeAngle();
                euler.y = Mathf.LerpAngle(euler.y, targetEuler.y, rotationSpeed * Time.fixedDeltaTime);
                Quaternion _newRotation = Quaternion.Euler(euler);
                transform.rotation = _newRotation;
            }
        }

        /// <summary>
        /// Conditions to Rotate the controller or not.
        /// </summary>
        /// <returns>A boolean value indicating whether the character should rotate to a certain direction.</returns>
        public virtual bool RotateToDirectionConditions()
        {
            return !lockAnimRotation && !customAction && (jumpAndRotate || isGrounded) && !ragdolled && !isSliding;
        }

        /// <summary>
        /// Adjusts the character's height by creating a ground margin
        /// </summary>
        /// <param name="velocity">The velocity vector of the character.</param>
        protected virtual void ApplyGroundMargin(ref Vector3 velocity)
        {
            float _middle = transform.InverseTransformPoint(rayOrigin).y;
            float _distanceToGo = _middle - snapDistance;

            currentSnapFactor = snapFactor;
            slopeSlideFactor = Mathf.Clamp(slopeSlideFactor, 0, slideSpeed);

            // creates a extra margin for ramps to avoid sinking the character into the ramp
            //if (Mathf.Abs(groundAngle) > slopeLimit / 4f)
            //{
            //    _distanceToGo += _capsuleCollider.radius * 0.5f;
            //}
            var currentGroundAdjustmentVelocity = transform.up * (_distanceToGo / Time.fixedDeltaTime);

            // ceiling detection to avoid the character sinking when the capsule hits the top on another collider
            CeilingDetection(ref velocity);

            var margin = hasCeilingContact ? currentGroundAdjustmentVelocity : currentGroundAdjustmentVelocity * currentSnapFactor * Time.fixedDeltaTime;
            var slopeDirection = groundAngleHit.normal;

            slopeDirection.y = 0;
            slopeDirection.Normalize();
            slopeDirection = Vector3.ProjectOnPlane(slopeDirection, groundAngleHit.normal);

            if (isOnSlope && targetHeightRatio <= 0 && groundPlane.planeAngle > slopeLimit)
            {
                slopeSlideFactor += slideSpeed * Time.deltaTime;
                margin += slopeDirection * (slopeSlideFactor / Time.fixedDeltaTime);
            }
            else
            {
                slopeSlideFactor = 0;
            }
            //Debug.DrawRay(transform.position, velocity, Color.blue,10);
            //Debug.DrawRay(transform.position, margin, Color.cyan,10);
            var localMargim = Vector3.zero + Vector3.up * transform.InverseTransformDirection(margin).y;
            margin = transform.TransformDirection(localMargim);
            if (margin.magnitude > maxSnapVelocityUp && margin.y > 0) margin = margin.normalized * maxSnapVelocityUp;
            else if (margin.magnitude > Math.Abs(maxSnapVelocityDown)) { margin = margin.normalized * (Math.Abs(maxSnapVelocityDown)); }
            velocity += margin;

        }

        /// <summary>
        /// Performs a ceiling detection check for the controller.
        /// </summary>
        /// <param name="velocity">The velocity vector of the character.</param>
        protected virtual void CeilingDetection(ref Vector3 velocity)
        {
            if (!useCeilingDetection) return;

            Ray ray = new()
            {
                origin = _capsuleCollider.bounds.center + transform.up * ((_capsuleCollider.height * 0.5f) - _capsuleCollider.radius),
                direction = new Vector3(velocity.x, velocity.y, velocity.z).normalized
            };

            if (Physics.SphereCast(ray, _capsuleCollider.radius * 0.98f, out RaycastHit hit, 0.25f, groundLayer))
            {
                if (hit.point.y > ray.origin.y)
                {
                    var hitDirection = hit.point - ray.origin - hit.normal;
                    hitDirection.y = 0f;
                    var velocityH = velocity;
                    velocityH.y = 0f;
                    var angle = velocityH.normalized.AngleFormOtherDirection(hitDirection);
                    Vector3 newDirection;
                    if (angle.y > 1f)
                    {
                        newDirection = Quaternion.AngleAxis(-90, Vector3.up) * -hit.normal;
                        newDirection.y = 0f;
                    }
                    else if (angle.y < -1f)
                    {
                        newDirection = Quaternion.AngleAxis(90, Vector3.up) * -hit.normal;
                        newDirection.y = 0f;
                    }
                    else
                    {
                        newDirection = Vector3.zero;
                    }

                    velocity = newDirection.normalized * velocity.magnitude * Mathf.Abs(angle.y) / 90f;
                    hasCeilingContact = true;
                    lastCeilingContact = Time.time + 0.25f;
                }
            }

            if (lastCeilingContact < Time.time)
            {
                hasCeilingContact = false;
            }
        }

        /// <summary>
        /// Applies extra gravity to the character when they are in the air and falling.
        /// </summary>
        protected virtual void ApplyExtraGravity()
        {
            if (isDead || customAction || disableCheckGround || isSliding)
            {
                applyingStepOffset = false;
                if (!isDead) heightReached = transform.position.y;
            }

            if (isInAirborne)
            {
                // check vertical velocity
                verticalVelocity = _rigidbody.velocity.y;
                // apply extra gravity when falling
                if (!applyingStepOffset && !isJumping && extraGravity != 0)
                {
                    _rigidbody.AddForce(transform.up * extraGravity * Time.fixedDeltaTime, ForceMode.VelocityChange);
                }
            }
        }

        #endregion

        #region Jump Methods

        /// <summary>
        /// Controls the behavior of jumping.
        /// </summary>
        protected virtual void ControlJumpBehaviour()
        {
            if (!isJumping)
            {
                return;
            }

            jumpCounter -= Time.fixedDeltaTime;
            if (jumpCounter <= 0)
            {
                jumpCounter = 0;
                isJumping = false;
            }
            // apply extra force to the jump height   
            var vel = _rigidbody.velocity;
            vel.y = jumpHeight * jumpMultiplier;
            _rigidbody.velocity = vel;
        }

        /// <summary>
        /// Sets the jump multiplier to the provided value.
        /// </summary>
        /// <param name="jumpMultiplier">The value to set the jump multiplier to.</param>
        public virtual void SetJumpMultiplier(float jumpMultiplier)
        {
            this.jumpMultiplier = jumpMultiplier;
        }

        /// <summary>
        /// Sets the jump multiplier value and starts a coroutine to reset it after a specified time.
        /// </summary>
        /// <param name="jumpMultiplier">The value to set the jump multiplier to.</param>
        /// <param name="timeToReset">The time in seconds to reset the jump multiplier. Default is 1 second.</param>
        public virtual void SetJumpMultiplier(float jumpMultiplier, float timeToReset = 1f)
        {
            this.jumpMultiplier = jumpMultiplier;

            if (timeToResetJumpMultiplier <= 0)
            {
                timeToResetJumpMultiplier = timeToReset;
                StartCoroutine(ResetJumpMultiplierRoutine());
            }
            else
            {
                timeToResetJumpMultiplier = timeToReset;
            }
        }

        /// <summary>
        /// Resets the jump multiplier value to its default state.
        /// </summary>
        public virtual void ResetJumpMultiplier()
        {
            StopCoroutine("ResetJumpMultiplierRoutine");
            timeToResetJumpMultiplier = 0;
            jumpMultiplier = 1;
        }

        /// <summary>
        /// Resets the jump multiplier after a certain amount of time has passed.
        /// </summary>        
        protected virtual IEnumerator ResetJumpMultiplierRoutine()
        {
            while (timeToResetJumpMultiplier > 0 && jumpMultiplier != 1 && (isJumping || !isGrounded))
            {
                timeToResetJumpMultiplier -= Time.fixedDeltaTime;
                yield return null;
            }
            timeToResetJumpMultiplier = 0;
            jumpMultiplier = 1;
        }

        /// <summary>
        /// Applies air movement to the character when it is in the air.
        /// </summary>
        public virtual void ApplyAirMovement()
        {
            if ((isGrounded && !isJumping) || isSliding || ragdolled || _rigidbody.isKinematic || customAction)
            {
                return;
            }
            if (transform.position.y > heightReached)
            {
                heightReached = transform.position.y;
            }

            inputSmooth = Vector3.Lerp(inputSmooth, input, airSmooth * Time.fixedDeltaTime);

            if (jumpWithRigidbodyForce && !isGrounded)
            {
                _rigidbody.AddForce(moveDirection * airSpeed * Time.fixedDeltaTime, ForceMode.VelocityChange);
                return;
            }
            var _moveDirection = moveDirection;
            _moveDirection.y = 0;
            _moveDirection.x = Mathf.Clamp(moveDirection.x, -1f, 1f);
            _moveDirection.z = Mathf.Clamp(moveDirection.z, -1f, 1f);
            moveDirection = _moveDirection;
            Vector3 targetPosition = _rigidbody.position + moveDirection * airSpeed * Time.fixedDeltaTime;
            Vector3 targetVelocity = (targetPosition - transform.position) / Time.fixedDeltaTime;

            targetVelocity.y = _rigidbody.velocity.y;
            _rigidbody.velocity = Vector3.Lerp(_rigidbody.velocity, targetVelocity, airSmooth * Time.fixedDeltaTime);
        }

        /// <summary>
        /// Property that checks if there are any collisions in front of the character when jumping.
        /// </summary>
        /// <returns>A boolean value indicating whether it is safe to jump forward.</returns>
        protected virtual bool jumpFwdCondition
        {
            get
            {
                Vector3 p1 = transform.position + _capsuleCollider.center + Vector3.up * -_capsuleCollider.height * 0.5F;
                Vector3 p2 = p1 + Vector3.up * _capsuleCollider.height;
                return Physics.CapsuleCastAll(p1, p2, _capsuleCollider.radius * 0.5f, transform.forward, 0.6f, groundLayer).Length == 0;
            }
        }

        #endregion

        #region Crouch Methods

        public virtual void UseAutoCrouch(bool value)
        {
            autoCrouch = value;
        }

        public virtual void AutoCrouch()
        {
            if (autoCrouch)
            {
                isCrouching = true;
            }

            if (autoCrouch && !inCrouchArea && CanExitCrouch())
            {
                autoCrouch = false;
                isCrouching = false;
            }
        }

        public virtual bool CanExitCrouch()
        {
            if (isCrouching)
            {
                // radius of SphereCast
                float radius = _capsuleCollider.radius * 0.9f;
                // Position of SphereCast origin stating in base of capsule
                Vector3 pos = transform.position + Vector3.up * ((colliderHeight * 0.5f) - colliderRadius);
                // ray for SphereCast
                Ray ray2 = new Ray(pos, Vector3.up);
                // sphere cast around the base of capsule for check ground distance
                if (Physics.SphereCast(ray2, radius, out groundHit, crouchHeadDetect - (colliderRadius * 0.1f), autoCrouchLayer))
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
            return true;
        }

        protected virtual void AutoCrouchExit(Collider other)
        {
            if (other.CompareTag("AutoCrouch"))
            {
                inCrouchArea = false;
            }
        }

        protected virtual void CheckForAutoCrouch(Collider other)
        {
            if (other.gameObject.CompareTag("AutoCrouch"))
            {
                autoCrouch = true;
                inCrouchArea = true;
            }
        }

        #endregion

        #region Roll Methods

        public virtual bool canRollAgain
        {
            get
            {

                return isRolling && animatorStateInfos.GetCurrentNormalizedTime(0) >= timeToRollAgain;
            }
        }

        protected virtual void RollBehavior()
        {
            if (!isRolling)
            {
                return;
            }

            if (rollControl)
            {
                // calculate input smooth
                inputSmooth = Vector3.Lerp(inputSmooth, input, (isStrafing ? strafeSpeed.movementSmooth : freeSpeed.movementSmooth) * Time.deltaTime);
            }

            // rotation
            RotateToDirection(moveDirection, rollRotationSpeed);

            // movement
            Vector3 deltaPosition = useRollRootMotion ? new Vector3(animator.deltaPosition.x, animator.deltaPosition.y, animator.deltaPosition.z) : transform.forward * Time.deltaTime;
            Vector3 v = deltaPosition * (rollSpeed > 0 ? rollSpeed : 1f) / Time.deltaTime * (1f - stopMoveWeight);

            if (isGrounded)
            {
                v = ProjectOnGround(v);
                ApplyGroundMargin(ref v);

                if (hasCeilingContact)
                {
                    autoCrouch = true;
                }
            }
            else
            {
                v.y = _rigidbody.velocity.y;

                // add roll extra gravity
                if (rollExtraGravity != 0)
                {
                    // rollExtraGravity can be negative to increase fall speed or positive to delay falling
                    _rigidbody.AddForce(transform.up * rollExtraGravity * Time.fixedDeltaTime, ForceMode.VelocityChange);
                }
            }

            targetVelocity = v;
        }

        #endregion

        #region Ground Check                       

        /// <summary>
        /// This method is responsible for checking if the character is on the ground or in the air.
        /// </summary>
        protected virtual void CheckGround()
        {
            // TO-DO SLIDE 
            //SlideOnSteepSlope();

            ControlMaterialPhysics();

            Vector3 direction = -transform.up;
            Vector3 origin = rayOrigin;

            float castDistance = isJumping || !isGrounded ? castLengthAirborne : castLengthGrounded;
            float groundDistance = castDistance;
            bool floorDetected = isGrounded;

            groundPlane.CalculatePlane(transform, origin, Vector3.down, castLengthGrounded, groundLayer, planeSize);

            rayHasDetectedHit = Physics.Raycast(origin, direction, out groundHit, castDistance, groundLayer, QueryTriggerInteraction.Ignore);

            if (rayHasDetectedHit)
            {
                validSnapPoint = true;
                float hitDistance = (origin - groundHit.point).DotVector(direction).magnitude;
                snapDistance = hitDistance;
                Vector3 relativeGroundPosition = transform.InverseTransformPoint(groundHit.point);
                groundDistance = Mathf.Max(0, relativeGroundPosition.y * -1);
            }

            if (useSphereCast)
            {
                sphereHasDetectedHit = Physics.SphereCast(origin, sphereCastRadius, direction, out groundHit, castDistance - sphereCastRadius, groundLayer, QueryTriggerInteraction.Ignore);

                if (sphereHasDetectedHit)
                {
                    validSnapPoint = true;
                    // Calculate validSnapPoint based on angle between velocity and hit direction
                    Vector3 velocity = targetVelocity;
                    velocity.y = 0f;
                    Vector3 hitDirection = groundHit.point - origin;
                    hitDirection.y = 0f;

                    // Calculate snapDistance and groundDistance based on spherecast hit
                    Vector3 normal = groundHit.normal;
                    normal.y = 0;
                    normal.Normalize();

                    Vector3 hitPosition = groundHit.point;

                    float hitDistance = (origin - hitPosition).DotVector(direction).magnitude /*+hitDirection.magnitude*/ ;
                    var _snapDistance = hitDistance;

                    Vector3 relativeGroundPosition = transform.InverseTransformPoint(hitPosition);
                    var _groundDistance = Mathf.Max(0, relativeGroundPosition.y * -1);
                    if (_groundDistance < groundDistance && !_isOnSlope)
                    {
                        snapDistance = _snapDistance;
                        groundDistance = _groundDistance;
                        //Debug.DrawLine(origin, groundHit.point, Color.green);
                    }
                    //else Debug.DrawLine(origin, groundHit.point, Color.yellow);
                }
                // else
                // {
                //     Debug.DrawLine(origin, groundHit.point, Color.red);
                // }
            }

            this.groundDistance = groundDistance;

            CheckFloorDetected(floorDetected);
            UpdateHeightReached();
        }

        /// <summary>
        /// Checks if the character is on the ground based on the distance between the character and the ground.
        /// If the character is not on the ground, it checks if the character is dead and calls a method to check for fall damage.
        /// </summary>
        /// <param name="floorDetected">A boolean variable indicating if the character is currently on the ground.</param>
        protected virtual void CheckFloorDetected(bool floorDetected)
        {
            if (customAction || disableCheckGround)
            {
                floorDetected = true;
            }
            else
            {
                if (groundDistance <= groundMinDistance)
                    floorDetected = true;
                else if (groundDistance >= groundMaxDistance)
                    floorDetected = false;
            }

            if (floorDetected != isGrounded)
            {
                if (floorDetected && !isDead) CheckFallDamage();

                isGrounded = floorDetected;
            }
        }

        /// <summary>
        /// Updates the heightReached variable based on the current position of the player.
        /// </summary>
        protected virtual void UpdateHeightReached()
        {
            if (isGrounded)
            {
                heightReached = transform.position.y;
            }
            else if (transform.position.y > heightReached)
            {
                heightReached = transform.position.y;
            }
        }

        /// <summary>
        /// Method to check and apply fall damage to a character if FallDamageConditions are true
        /// </summary>
        protected virtual void CheckFallDamage()
        {
            if (FallDamageConditions())
            {
                float fallHeight = heightReached - transform.position.y;

                fallHeight -= fallMinHeight;
                if (fallHeight > 0)
                {
                    int damage = (int)(fallDamage * fallHeight);
                    TakeDamage(new vDamage(damage, true));
                }
            }
        }

        /// <summary>
        /// Checks if the conditions for fall damage are met.
        /// </summary>
        /// <returns>True if the conditions for fall damage are met, otherwise false.</returns>
        public virtual bool FallDamageConditions()
        {
            return !isGrounded && verticalVelocity > fallMinVerticalVelocity && _canApplyFallDamage && fallMinHeight != 0 || fallDamage != 0;
        }

        /// <summary>
        /// Controls the physics material of the character's capsule collider based on its current state.
        /// </summary>
        protected virtual void ControlMaterialPhysics()
        {
            if (changeMaterialPhysics < Time.time)
            {
                if (isGrounded && input.magnitude < 0.1f)
                {
                    if (IdlePhysicsMaterial && _capsuleCollider.material != IdlePhysicsMaterial)
                        _capsuleCollider.material = IdlePhysicsMaterial;
                }
                else if (isGrounded && input.magnitude > 0.1f)
                {
                    if (MovingPhysicsMaterial && _capsuleCollider.material != MovingPhysicsMaterial)
                        _capsuleCollider.material = MovingPhysicsMaterial;
                }
                else if (!isGrounded)
                {
                    if (AirbornePhysicsMaterial && _capsuleCollider.material != AirbornePhysicsMaterial)
                        _capsuleCollider.material = AirbornePhysicsMaterial;
                }
                changeMaterialPhysics = Time.time + 1;
            }
        }

        /// <summary>
        /// Return the current ground angle
        /// </summary>
        /// <returns></returns>
        public virtual float GroundAngle()
        {
            var groundAngle = Vector3.Angle(groundHit.normal, Vector3.up);
            return groundAngle;
        }

        /// <summary>
        /// Return the angle of ground based on movement direction
        /// </summary>
        /// <returns></returns>
        public virtual float GroundAngleFromDirection()
        {
            var dir = isStrafing && input.magnitude > 0 ? (transform.right * input.x + transform.forward * input.z).normalized : transform.forward;
            var movementAngle = Vector3.Angle(dir, groundHit.normal) - 90;
            return movementAngle;
        }

        /// <summary>
        /// Prototype to align capsule collider with surface normal
        /// </summary>
        protected virtual void AlignWithSurface()
        {
            Ray ray = new Ray(transform.position, -transform.up);
            RaycastHit hit;
            var surfaceRot = transform.rotation;

            if (Physics.Raycast(ray, out hit, 1.5f, groundLayer))
            {
                surfaceRot = Quaternion.FromToRotation(transform.up, hit.normal) * transform.localRotation;
            }
            transform.rotation = Quaternion.Lerp(transform.rotation, surfaceRot, 10f * Time.fixedDeltaTime);
        }

        //TO-DO - Needs to be updated to work with the new Ground Detection System
        protected virtual void SlideOnSteepSlope()
        {
            if (useSlide && isGrounded && GroundAngle() > slopeLimit && !disableCheckGround)
            {
                if (_slidingEnterTime <= 0f || isSliding)
                {
                    var normal = groundHit.normal;
                    normal.y = 0f;
                    var dir = Vector3.ProjectOnPlane(normal.normalized, groundHit.normal).normalized;

                    if (!Physics.Raycast(transform.position + Vector3.up * 0.1f, dir, 0.5f, groundLayer))
                    {
                        isSliding = true;
                    }
                    //else
                    //{
                    //    isSliding = true;
                    //}
                }
                else
                {
                    _slidingEnterTime -= Time.fixedDeltaTime;
                }
            }
            else
            {
                _rotateSlopeEnterTime = rotateSlopeEnterTime;
                _slidingEnterTime = isGrounded ? slidingEnterTime : 0f;
                isSliding = false;
            }
        }

        //TO-DO - Needs to be updated to work with the new Ground Detection System
        protected virtual void SlideMovementBehavior()
        {
            if (!isSliding)
            {
                return;
            }

            var normal = groundHit.normal;
            normal.y = 0f;

            var dir = Vector3.ProjectOnPlane(normal.normalized, groundHit.normal).normalized;

            if (debugWindow)
            {
                Debug.DrawRay(transform.position, dir * slideDownVelocity);
            }

            _rigidbody.velocity = Vector3.Lerp(_rigidbody.velocity, dir * slideDownVelocity, slideDownSmooth * Time.fixedDeltaTime);
            dir.y = 0f;

            if (_rotateSlopeEnterTime <= 0f)
            {
                Vector3 desiredForward = Vector3.RotateTowards(transform.forward, dir, rotateDownSlopeSmooth * Time.fixedDeltaTime, 0f);
                Quaternion _newRotation = Quaternion.LookRotation(desiredForward);
                _rigidbody.MoveRotation(_newRotation);

                var rightMovement = transform.InverseTransformDirection(moveDirection);
                rightMovement.y = 0f;
                rightMovement.z = 0f;
                rightMovement = transform.TransformDirection(rightMovement);
                if (debugWindow)
                {
                    Debug.DrawRay(transform.position, rightMovement * slideSidewaysVelocity, Color.blue);
                }

                _rigidbody.AddForce(rightMovement * slideSidewaysVelocity, ForceMode.VelocityChange);

                if (debugWindow)
                {
                    Debug.DrawRay(transform.position, Vector3.ProjectOnPlane(normal.normalized, groundHit.normal).normalized, Color.blue);
                    Debug.DrawRay(transform.position, Quaternion.AngleAxis(90, groundHit.normal) * Vector3.ProjectOnPlane(normal.normalized, groundHit.normal).normalized, Color.red);
                    Debug.DrawRay(transform.position, transform.TransformDirection(rightMovement.normalized * 2f), Color.green);
                }
            }
            else
            {
                _rotateSlopeEnterTime -= Time.fixedDeltaTime;
            }
        }

        #endregion

        #region Colliders Check        

        /// <summary>
        /// Conditions to make the CapsuleCollider Height turn to full size
        /// </summary>
        /// <returns></returns>
        public virtual bool FullCapsuleHeightConditions()
        {
            return disableSnapToGround || customAction || isDead || !(!isRolling || isGrounded);
        }
        public virtual void SetFullCapsuleHeight()
        {
            _capsuleCollider.height = capsuleHeight;
            _capsuleCollider.center = capsuleOffset * capsuleHeight;
            _capsuleCollider.radius = capsuleThickness / 2f;
        }
        public virtual void ControlCapsuleHeight()
        {
            if (FullCapsuleHeightConditions())
                stepHeightRatio = 0f;
            //stepHeightRatio = Mathf.LerpUnclamped(stepHeightRatio, 0f, lerpCapsuleAdjust ? lerpCapsuleSmooth * Time.fixedDeltaTime : 1f);
            else
                stepHeightRatio = Mathf.LerpUnclamped(stepHeightRatio, targetHeightRatio, lerpCapsuleAdjust ? lerpCapsuleSmooth * Time.fixedDeltaTime : 1f);

            if (_capsuleCollider == null)
            {
                _capsuleCollider = GetComponent<CapsuleCollider>();
            }

            if (isCrouching && !isRolling)
            {
                heightReduction = Mathf.Lerp(heightReduction, capsuleHeightReductionCrouched, lerpCapsuleSmooth * Time.fixedDeltaTime);
                radiusReduction = Mathf.Lerp(radiusReduction, capsuleRadiusReductionCrouched, lerpCapsuleSmooth * Time.fixedDeltaTime);
            }
            else if (isRolling)
            {
                heightReduction = Mathf.Lerp(heightReduction, capsuleHeightReductionRolling, lerpCapsuleSmooth * Time.fixedDeltaTime);
                radiusReduction = Mathf.Lerp(radiusReduction, capsuleRadiusReductionRolling, lerpCapsuleSmooth * Time.fixedDeltaTime);
            }
            else
            {
                heightReduction = Mathf.Lerp(heightReduction, 0f, lerpCapsuleSmooth * Time.fixedDeltaTime);
                radiusReduction = Mathf.Lerp(radiusReduction, 0f, lerpCapsuleSmooth * Time.fixedDeltaTime);
            }
            var targetColliderHeight = capsuleHeight;
            var targetColliderCenter = capsuleOffset * capsuleHeight;
            var targetColliderRadius = capsuleThickness / 2f;

            targetColliderHeight /= 1 + heightReduction;
            targetColliderCenter /= 1 + heightReduction;
            targetColliderRadius /= 1 + radiusReduction;
            targetColliderCenter = targetColliderCenter + new Vector3(0f, stepHeightRatio * capsuleHeight / 2f, 0f);
            targetColliderHeight -= capsuleHeight * stepHeightRatio;
            if (targetColliderHeight / 2f < targetColliderRadius) targetColliderRadius = targetColliderHeight / 2f;

            if (targetColliderHeight != _capsuleCollider.height) _capsuleCollider.height = targetColliderHeight;
            if (targetColliderCenter != _capsuleCollider.center) _capsuleCollider.center = targetColliderCenter;
            if (targetColliderRadius != _capsuleCollider.radius) _capsuleCollider.radius = targetColliderRadius;
        }

        /// <summary>
        /// Reset Capsule Height, Radius and Center to default values
        /// </summary>
        public virtual void ResetCapsule()
        {
            colliderCenter = colliderCenterDefault;
            colliderRadius = colliderRadiusDefault;
            colliderHeight = colliderHeightDefault;
        }

        /// <summary>
        /// Disables rigibody gravity, turn the capsule collider trigger and reset all input from the animator.
        /// </summary>
        public virtual void DisableGravityAndCollision()
        {
            animator.SetFloat("InputHorizontal", 0f);
            animator.SetFloat("InputVertical", 0f);
            animator.SetFloat("VerticalVelocity", 0f);
            //Disable gravity and collision
            _rigidbody.useGravity = false;
            _rigidbody.isKinematic = true;
            _capsuleCollider.isTrigger = true;
        }

        /// <summary>
        /// Turn rigidbody gravity on the uncheck the capsulle collider as Trigger
        /// </summary>      
        public virtual void EnableGravityAndCollision()
        {
            // Enable collision and gravity
            _capsuleCollider.isTrigger = false;
            _rigidbody.useGravity = true;
            _rigidbody.isKinematic = false;
        }

        #endregion

        #region Ragdoll 

        protected virtual void CheckRagdoll()
        {
            if (ragdollVelocity == 0)
            {
                return;
            }

            // check your verticalVelocity and assign a value on the variable RagdollVel at the Player Inspector
            if (verticalVelocity <= ragdollVelocity && groundDistance <= 0.1f && _canApplyFallDamage && !ragdolled)
            {
                onActiveRagdoll.Invoke(null);
            }
        }

        /// <summary>
        /// Reset the necessary variables after exit Ragdoll
        /// </summary>
        public override void ResetRagdoll()
        {
            onDisableRagdoll.Invoke();
            verticalVelocity = 0f;
            ragdolled = false;
            _rigidbody.WakeUp();
            _rigidbody.useGravity = true;
            _rigidbody.isKinematic = false;
            _capsuleCollider.isTrigger = false;
            _capsuleCollider.enabled = true;
        }

        /// <summary>
        /// Set the necessary variables to enable Ragdoll Physics 
        /// </summary>
        public override void EnableRagdoll()
        {
            StopCharacter();
            animator.SetFloat("InputHorizontal", 0f);
            animator.SetFloat("InputVertical", 0f);
            animator.SetFloat("InputMagnitude", 0f);
            animator.SetFloat("VerticalVelocity", 0f);
            ragdolled = true;
            _capsuleCollider.isTrigger = true;
            _rigidbody.useGravity = false;
            _rigidbody.isKinematic = true;
            lockAnimMovement = true;
        }

        #endregion

        #region Debug

        public delegate void GetDebugDelegate(ref System.Text.StringBuilder stringBuilder);

        public event GetDebugDelegate OnDebug;
        public virtual string DebugInfo(string additionalText = "")
        {
            string debugInfo = string.Empty;
            if (debugWindow)
            {
                float delta = Time.smoothDeltaTime;
                float fps = 1 / delta;

                debugInfo =
                    " \n" +
                    "FPS " + fps.ToString("#,##0 fps") + "\n" +
                    "Health = " + currentHealth.ToString() + "\n" +
                    "Input Vertical = " + inputSmooth.z.ToString("0.0") + "\n" +
                    "Input Horizontal = " + inputSmooth.x.ToString("0.0") + "\n" +
                    "Input Magnitude = " + inputMagnitude.ToString("0.0") + "\n" +
                    "Rotation Magnitude = " + rotationMagnitude.ToString("0.0") + "\n" +
                    "Vertical Velocity = " + verticalVelocity.ToString("0.00") + "\n" +
                    "Current MoveSpeed = " + moveSpeed.ToString("0.00") + "\n" +
                    "Ground Distance = " + groundDistance.ToString("0.00") + "\n" +
                    "Ground Angle = " + GroundAngleFromDirection().ToString("0.00") + "\n" +
                    "Is Grounded = " + BoolToRichText(isGrounded) + "\n" +
                    "Ceiling Contact = " + BoolToRichText(hasCeilingContact) + "\n" +
                    "Is Strafing = " + BoolToRichText(isStrafing) + "\n" +
                    "Is Trigger = " + BoolToRichText(_capsuleCollider.isTrigger) + "\n" +
                    "Use Gravity = " + BoolToRichText(_rigidbody.useGravity) + "\n" +
                    "Is Kinematic = " + BoolToRichText(_rigidbody.isKinematic) + "\n" +
                    "Lock Movement = " + BoolToRichText(lockMovement) + "\n" +
                    "Lock AnimMov = " + BoolToRichText(lockAnimMovement) + "\n" +
                    "Lock Rotation = " + BoolToRichText(lockRotation) + "\n" +
                    "Lock AnimRot = " + BoolToRichText(lockAnimRotation) + "\n" +
                    "--- Actions Bools ---" + "\n" +
                    "Is Sliding = " + BoolToRichText(isSliding) + "\n" +
                    "Is Sprinting = " + BoolToRichText(isSprinting) + "\n" +
                    "Is Crouching = " + BoolToRichText(isCrouching) + "\n" +
                    "Is Rolling = " + BoolToRichText(isRolling) + "\n" +
                    "Is Jumping = " + BoolToRichText(isJumping) + "\n" +
                    "Is Airborne = " + BoolToRichText(isInAirborne) + "\n" +
                    "Is Ragdolled = " + BoolToRichText(ragdolled) + "\n" +
                    "CustomAction = " + BoolToRichText(customAction) + "\n" + additionalText;
            }
            if (OnDebug != null)
            {
                System.Text.StringBuilder stringBuilder = new System.Text.StringBuilder();
                OnDebug.Invoke(ref stringBuilder);
                debugInfo += stringBuilder.ToString();
            }
            return debugInfo;
        }

        protected virtual string BoolToRichText(bool value)
        {
            return value ? "<color=yellow> True </color>" : "<color=red> False </color>";
        }

        protected virtual void OnDrawGizmos()
        {
            if (Application.isPlaying && debugWindow)
            {
                // debug auto crouch
                Vector3 posHead = transform.position + Vector3.up * ((colliderHeight * 0.5f) - colliderRadius);
                Ray ray1 = new Ray(posHead, Vector3.up);
                Gizmos.DrawWireSphere(ray1.GetPoint(crouchHeadDetect - (colliderRadius * 0.1f)), colliderRadius * 0.9f);
            }

        }

        protected virtual void OnDrawGizmosSelected()
        {
            if (!_capsuleCollider) _capsuleCollider = GetComponent<CapsuleCollider>();
            if (_capsuleCollider)
            {
                var matrix = Gizmos.matrix;
                var surfaceRot = Quaternion.FromToRotation(Vector3.up, groundPlane.planeNormal);
                var newRot = surfaceRot * Quaternion.AngleAxis(transform.rotation.eulerAngles.y, Vector3.up);
                Gizmos.color = Color.red * 0.8f;
                if (isGrounded)
                {
                    Gizmos.matrix = Matrix4x4.TRS(groundPlane.planeCenter, newRot, new Vector3(planeSize * 2, 0f, planeSize * 2));
                    Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
                    Gizmos.matrix = matrix;
                    Gizmos.DrawSphere(groundPlane.planeCenter, 0.1f);
                }
                Gizmos.matrix = matrix;

                var p1 = transform.position + transform.up * _capsuleCollider.radius;
                var p2 = transform.position + transform.up * ((capsuleHeight * stepHeightRatio) + _capsuleCollider.radius);
                Gizmos.matrix = Matrix4x4.TRS(p1, transform.rotation, Vector3.one);
                Gizmos.DrawWireSphere(Vector3.zero, _capsuleCollider.radius);
                Gizmos.matrix = matrix;
                Gizmos.DrawLine(p1 + transform.right * _capsuleCollider.radius, p2 + transform.right * _capsuleCollider.radius);
                Gizmos.DrawLine(p1 - transform.right * _capsuleCollider.radius, p2 - transform.right * _capsuleCollider.radius);
                Gizmos.DrawLine(p1 + transform.forward * _capsuleCollider.radius, p2 + transform.forward * _capsuleCollider.radius);
                Gizmos.DrawLine(p1 - transform.forward * _capsuleCollider.radius, p2 - transform.forward * _capsuleCollider.radius);
            }
        }

        #endregion

        #region MovementSpeed Class

        /// <summary>
        /// This class defines the movement speed and animation settings for a character in a third-person motor controller.
        /// It includes variables for movement smoothness, animation smoothness, rotation speed, default movement mode, camera rotation,
        /// and different speeds for walking, running, sprinting, and crouching.
        /// </summary>
        [System.Serializable]
        public class vMovementSpeed
        {
            /// <summary>
            /// Higher means faster/responsive movement, lower means smooth movement
            /// </summary>
            [vHelpBox("Higher means faster/responsive movement, lower means smooth movement")]
            [Range(1f, 20f)]
            [FormerlySerializedAs("movementSmooth")]
            [SerializeField] public float movementSmooth = 6f;

            /// <summary>
            /// Lower means faster transitions between animations, higher means slower
            /// </summary>
            [vHelpBox("Lower means faster transitions between animations, higher means slower")]
            [Range(0f, 1f)]
            [FormerlySerializedAs("animationSmooth")]
            [SerializeField] public float animationSmooth = 0.2f;

            /// <summary>
            /// Rotation speed of the character
            /// </summary>
            [Tooltip("Rotation speed of the character")]
            [FormerlySerializedAs("rotationSpeed")]
            [SerializeField] public float rotationSpeed = 20f;

            /// <summary>
            /// Character will limit the movement to walk instead of running
            /// </summary>
            [Tooltip("Character will limit the movement to walk instead of running")]
            [FormerlySerializedAs("walkByDefault")]
            [SerializeField] public bool walkByDefault = false;

            /// <summary>
            /// Rotate with the Camera forward when standing idle
            /// </summary>
            [Tooltip("Rotate with the Camera forward when standing idle")]
            [FormerlySerializedAs("rotateWithCamera")]
            [SerializeField] public bool rotateWithCamera = false;

            /// <summary>
            /// Speed to Walk using rigidbody or extra speed if you're using RootMotion
            /// </summary>
            [Tooltip("Speed to Walk using rigidbody or extra speed if you're using RootMotion")]
            [FormerlySerializedAs("walkSpeed")]
            [SerializeField] public float walkSpeed = 2f;

            /// <summary>
            /// Speed to Run using rigidbody or extra speed if you're using RootMotion
            /// </summary>
            [Tooltip("Speed to Run using rigidbody or extra speed if you're using RootMotion")]
            [FormerlySerializedAs("runningSpeed")]
            [SerializeField] public float runningSpeed = 4f;

            /// <summary>
            /// Speed to Sprint using rigidbody or extra speed if you're using RootMotion
            /// </summary>
            [Tooltip("Speed to Sprint using rigidbody or extra speed if you're using RootMotion")]
            [FormerlySerializedAs("sprintSpeed")]
            [SerializeField] public float sprintSpeed = 6f;

            /// <summary>
            /// Speed to Crouch using rigidbody or extra speed if you're using RootMotion
            /// </summary>
            [Tooltip("Speed to Crouch using rigidbody or extra speed if you're using RootMotion")]
            [FormerlySerializedAs("crouchSpeed")]
            [SerializeField] public float crouchSpeed = 2f;

            /// <summary>
            /// Gets or sets the default movement smoothness.
            /// </summary>
            public float defaultMovementSmooth { get; set; }

            /// <summary>
            /// Gets or sets the default animation smoothness.
            /// </summary>
            public float defaultAnimationSmooth { get; set; }

            /// <summary>
            /// Gets or sets the default rotation speed.
            /// </summary>
            public float defaultRotationSpeed { get; set; }

            /// <summary>
            /// Gets or sets the default walk by default value.
            /// </summary>
            public bool defaultWalkByDefault { get; set; }

            /// <summary>
            /// Gets or sets the default rotate with camera value.
            /// </summary>
            public bool defaultRotateWithCamera { get; set; }

            /// <summary>
            /// Gets or sets the default walk speed.
            /// </summary>
            public float defaultWalkSpeed { get; set; }

            /// <summary>
            /// Gets or sets the default running speed.
            /// </summary>
            public float defaultRunningSpeed { get; set; }

            /// <summary>
            /// Gets or sets the default sprint speed.
            /// </summary>
            public float defaultSprintSpeed { get; set; }

            /// <summary>
            /// Gets or sets the default crouch speed.
            /// </summary>
            public float defaultCrouchSpeed { get; set; }

            /// <summary>
            /// Initializes the default values of the variables.
            /// </summary>
            public void Init()
            {
                defaultMovementSmooth = movementSmooth;
                defaultAnimationSmooth = animationSmooth;
                defaultRotationSpeed = rotationSpeed;
                defaultWalkByDefault = walkByDefault;
                defaultRotateWithCamera = rotateWithCamera;
                defaultWalkSpeed = walkSpeed;
                defaultRunningSpeed = runningSpeed;
                defaultSprintSpeed = sprintSpeed;
                defaultCrouchSpeed = crouchSpeed;
            }

            /// <summary>
            /// Resets the values to their default settings.
            /// </summary>
            public void ResetToDefault()
            {
                movementSmooth = defaultMovementSmooth;
                animationSmooth = defaultAnimationSmooth;
                rotationSpeed = defaultRotationSpeed;
                walkByDefault = defaultWalkByDefault;
                rotateWithCamera = defaultRotateWithCamera;
                walkSpeed = defaultWalkSpeed;
                runningSpeed = defaultRunningSpeed;
                sprintSpeed = defaultSprintSpeed;
                crouchSpeed = defaultCrouchSpeed;
            }
        }

        #endregion
    }
}