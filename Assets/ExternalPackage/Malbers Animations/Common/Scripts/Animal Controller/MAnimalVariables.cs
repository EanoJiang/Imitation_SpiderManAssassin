using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Events;
using MalbersAnimations.Events;
using MalbersAnimations.Utilities;
using MalbersAnimations.Scriptables;

namespace MalbersAnimations.Controller 
{
    /// Variables
    public partial class MAnimal
    {
        #region Static Properties
        /// <summary>List of all the animals on the scene</summary>
        public static List<MAnimal> Animals;
        /// <summary>Main Animal use as the player</summary>
        public static MAnimal MainAnimal;
        #endregion

        [SerializeField] private LayerMask hitLayer = ~0;
        [SerializeField] private QueryTriggerInteraction triggerInteraction = QueryTriggerInteraction.Collide;
        public LayerMask HitLayer
        {
            get { return hitLayer; }
            set { hitLayer = value; }
        }

        public QueryTriggerInteraction TriggerInteraction
        {
            get { return triggerInteraction; }
            set { triggerInteraction = value; }
        }

        #region States


        /// <summary>NECESARY WHEN YOU ARE USING MULTIPLE ANIMALS </summary>
        public bool CloneStates = true;

        ///<summary> List of States for this animal  </summary>
        public List<State> states = new List<State>();

        ///<summary> List of States for this animal  converted to a Dictionary</summary>
        public Dictionary<int, State> statesD = new Dictionary<int, State>();

        ///<summary> List of  All Sleep States</summary>
        internal List<State> sleepStates = new List<State>();

        ///<summary>List of Events to Use on the States</summary>
        public List<OnEnterExitState> OnEnterExitStates;

        public StateID OverrideStartState;

        ///// <summary>Used on the Editor can't remember why??</summary>
        //public int SelectedState;

       // public StateID LastStateID { get; private set; }
        internal State activeState;
        internal State lastState;
        /// <summary> Store the Last State </summary> 
        public State LastState
        {
            get { return lastState; }
            internal set
            {
                lastState = value;
                SetAnimParameter(hash_LastState, (int)/*LastStateID =*/ lastState.ID);          //Sent to the Animator the previews Active State 
            }
        }
      
        ///<summary> Store Possible State (PreState) that can be Activate it by Input</summary>
        protected State PreInputState;

        /// <summary> Store on the Animal a Queued State</summary>
        public State StateQueued { get; private set; }
        internal void QueueState(State state)
        {
            StateQueued = state;
            LastState = state;
        }


        /// <summary>Used to call the Last State one more time before it changes to the new state </summary>
        public bool JustActivateState { get; internal set; }

        public StateID ActiveStateID { get; private set; }


        /// <summary>Set/Get the Active State</summary>
        public State ActiveState
        {
            get { return activeState; }
            internal set
            { 

                activeState = value;

                if (value == null) return;

                SetAnimParameter(hash_State, (int)(ActiveStateID = activeState.ID));           //Sent to the Animator the value to Apply  
                OnStateChange.Invoke(ActiveStateID);

                sleepStates = new List<State>();                                        //Reset all the Sleep States

                SendToSleepStates();

                if (IsPlayingMode)
                {
                    ActiveMode.StateChanged(ActiveStateID);        //If a mode is playing check a State Change
                }
            }
        }

        /// <summary>When a new State is Activated, Make sure the other States are sent to sleep</summary>
        private void SendToSleepStates()
        {
            foreach (var st in states)
            {
                st.Sleep = st.SleepFrom.Contains(ActiveStateID);        //Sent to sleep states that has some Avoid State

                if (st.Sleep) sleepStates.Add(st);                      //Fill the list of sleep States
            }
        }


        #endregion

        #region General

        /// <summary> Layers the Animal considers ground</summary>
        public LayerMask GroundLayer = 1;
        /// <summary>Distance from the Pivots to the ground (USED ON THE EDITOR) </summary>
        public  float height = 1f;
        /// <summary>Height from the ground to the hip multiplied for the Scale Factor</summary>
        public float Height { get; protected set; }

        /// <summary>The Scale Factor of the Animal.. if the animal has being scaled this is the multiplier for the raycasting things </summary>
        public float ScaleFactor { get; protected set; }

        /// <summary>Does this Animal have an InputSource </summary>
        public IInputSource InputSource;

        private int stance;
        /// <summary>Stance Integer Value sent to the animator</summary>
        public int Stance
        {
            get { return stance; }
            set
            {
                stance = value;
                OnStanceChange.Invoke(value);

                if (hasStance) SetAnimParameter(hash_Stance, stance);
                   
            }
        }

        public BoolReference isPlayer;


        #endregion

        #region Movement

        public FloatReference AnimatorSpeed = new FloatReference(1);


        ///// <summary>(Z), horizontal (X) and Vertical (Y) Raw Input Axis getit from a source</summary>
        //public Vector3 RawInputAxis;
        /// <summary>(Z), horizontal (X) and Vertical (Y) Raw Movement Input</summary>
        public Vector3 MovementAxis;
        /// <summary>Forward (Z), horizontal (X) and Vertical (Y) Smoothed Movement Input after aplied Speeds Multipliers</summary>
        public Vector3 MovementAxisSmoothed;


        /// <summary>Direction Speed Applied to the Additional Speed </summary>
        public Vector3 DirectionalSpeed { get; private set; }

        /// <summary>if False then the Directional Speed wont be Updated, used to Rotate the Animal but still moving on the Last Direction </summary>
        public bool UpdateDirectionSpeed { get; set; }

        /// <summary>Inertia Speed to smoothly change the Speed Modifiers </summary>
        public Vector3 InertiaPositionSpeed { get; internal set; }

        /// <summary> Direction the Character is Heading when the Additional Speed is appplied</summary>
        public Vector3 TargetMoveDirection { get; internal set; }
        /// <summary>Checking if the movement input was activated</summary>
        public bool MovementDetected { get; private set;  }


        /// <summary>The Animal uses the Camera Forward Diretion to Move</summary>
        public BoolReference useCameraInput =  new BoolReference();

        /// <summary>Use the Camera Up Vector to Move while flying or Swiming UnderWater</summary>
        public BoolReference useCameraUp = new BoolReference();

        /// <summary>The Animal uses the Camera Forward Diretion to Move</summary>
        public bool UseCameraInput
        {
            get { return useCameraInput; }
            set { useCameraInput.Value = value; }
        }


        /// <summary>Use the Camera Up Vector to Move while flying or Swiming UnderWater</summary>
        public bool UseCameraUp
        {
            get { return useCameraUp; }
            set { useCameraUp.Value = value; }
        }

        /// <summary> Is the animal using a Direction Vector for moving?</summary>
        public bool MoveWithDirection { private set; get; }
      

        /// <summary>Main Camera on the Game</summary>
        public static Transform MainCamera;

        /// <summary> Additive Position Modifications for the  animal (Terrian Snapping, Speed Modifiers Positions, etc)</summary>
        public Vector3 AdditivePosition;
        /// <summary> Additive Rotation Modifications for the  animal (Terrian Aligment, Speed Modifiers Rotations, etc)</summary>
        public Quaternion AdditiveRotation;
        /// <summary> If true it will keep the Conrtoller smooth push of the movement stick</summary>
        public BoolReference SmoothVertical = new BoolReference(true);
        /// <summary>Global turn multiplier</summary>
        public FloatReference TurnMultiplier = new FloatReference(0f);
        /// <summary>Up Down Axis Smooth Factor</summary>
        public FloatReference UpDownLerp = new FloatReference(10f);

      
        /// <summary>Difference from the Last Frame and the Current Frame</summary>
        public Vector3 DeltaPos { get; private set; }
        /// <summary>World Position on the last Frame</summary>
        public Vector3 LastPos { get; internal set; }


        /// <summary>Velocity acumulated from the last Frame</summary>
        public Vector3 Inertia { get; private set; }

        /// <summary>Difference between the Current Rotation and the desire Input Rotation </summary>
        public float DeltaAngle { get; internal set; }


        /// <summary>Pitch direction used when Free Movement is Enable (Direction of the Move Input) </summary>
        public Vector3 PitchDirection { get; private set; }
        private float PitchAngle;
        private float Bank;

        /// <summary>Speed from the Vertical input multiplied by the speeds inputs(Walk Trot Run) this is the value thats goes to the Animator, is not the actual Speed of the animals</summary>
        public float VerticalSmooth
        {
            internal set { MovementAxisSmoothed.z = value; }
            get { return MovementAxisSmoothed.z; }
        }

        /// <summary> If true it will keep the Conrtoller smooth push of the movement stick</summary>
        public bool Smooth_Vertical
        {
            set { SmoothVertical.Value = value; }
            get { return SmoothVertical; }
        }

        /// <summary>Direction from the Horizontal input multiplied by the speeds inputs this is the value thats goes to the Animator, is not the actual Speed of the animals</summary>
        public float Smooth_Horizontal
        {
            internal set { MovementAxisSmoothed.x = value; }
            get { return MovementAxisSmoothed.x; }
        }

        /// <summary>Direction from the Up Down input multiplied by the speeds inputs this is the value thats goes to the Animator, is not the actual Speed of the animals</summary>
        public float Smooth_UpDown
        {
            internal set { MovementAxisSmoothed.y = value; }
            get { return MovementAxisSmoothed.y; }
        }

        private bool sprint;
        /// <summary>Sprint Input</summary>
        public bool Sprint
        {
            get { return sprint; }
            set
            {
                var NewSprint = UseSprintState && value && UseSprint;// && CurrentSpeedModifier.sprint;


                if (MovementAxis.z <= 0 && Grounded  && !CustomSpeed) NewSprint = false; //Check Sprint only when is moving forward
                if (!MovementDetected) NewSprint = false; //IF there's no movement then Sprint False


                if (sprint != NewSprint) // Only invoke when the values are different
                {
                    sprint = NewSprint;
                    OnSprintEnabled.Invoke(sprint);
                    OnSpeedChange.Invoke(SprintSpeed); //Invoke the Speed again
                }
            }
        }

        /// <summary> The current value of the Delta time the animal is using (Fixed or not)</summary>
        public float DeltaTime { get; private set; }

        #endregion

        #region Alignment
        /// <summary>Smoothness value to Snap to ground </summary>
        public FloatReference AlignPosLerp = new FloatReference(15f);
        /// <summary>Smoothness value to Snap to ground  </summary>
        public FloatReference AlignRotLerp = new FloatReference(15f);

        /// <summary>Maximun angle on the terrain the animal can walk </summary>
        [Range(0f, 90f), Tooltip("Maximun angle on the terrain the animal can walk")]
        public float maxAngleSlope = 45f;

        /// <summary>Used to add extra Rotations to the Animal</summary>
        public Transform Rotator;


        /// <summary>Check if can Fall on slope while on the ground </summary>
        public bool DeepSlope
        {
            get { return Mathf.Abs(TerrainSlope) > maxAngleSlope; }
        }


        /// <summary>Speed of the Animal used on the RIgid Body</summary>
        public float HorizontalSpeed { get; private set; }

        /// <summary>Calculation of the Average Surface Normal</summary>
        public Vector3 SurfaceNormal { get; set; }

        /// <summary>Calculate slope and normalize it</summary>
        public float SlopeNormalized
        {
            get
            {
                if (maxAngleSlope > 0)
                {
                    return TerrainSlope / maxAngleSlope;    //Normalize the AngleSlop by the MAX Angle Slope and make it positive(HighHill) or negative(DownHill)
                }
                return 0;
            }
        }

        /// <summary>Slope Calculate from the Surface Normal. Positive = Higher Slope, Negative = Lower Slope </summary>
        public float TerrainSlope { get; protected set; }

        #endregion

        #region References
        /// <summary>Returns the Animator Component of the Animal </summary>
        public Animator Anim;
        public Rigidbody RB;                   //Reference for the RigidBody

        /// <summary>Catched Transform</summary>
        private Transform _transform;

        /// <summary>Transform.UP (Stored)</summary>
        public Vector3 Up { get { return _transform.up; } }
        /// <summary>Transform.Right (Stored)</summary>
        public Vector3 Right { get { return _transform.right; } }
        /// <summary>Transform.Forward (Stored) </summary>
        public Vector3 Forward { get { return _transform.forward; } }

       

        /// <summary>Transform.Forward with no Y Value</summary>
        public Vector3 Forward_no_Y
        {
            get { return Vector3.ProjectOnPlane(Forward, UpVector); }
        }

        #endregion

        #region Modes
        private int modeID;
        private int modeStatus;
        private Mode activeMode;

        ///<summary> List of States for this animal  </summary>
        public List<Mode> modes = new List<Mode>();
        ///<summary> List of Modes for this animal  converted to a Dictionary</summary>
        public Dictionary<int, Mode> modesD = new Dictionary<int, Mode>();


        /// <summary>Is Playing a mode or is still exiting a Mode</summary>
        public bool IsPlayingMode { get; internal set; }

        /// <summary>Is the Animal on any Zone</summary>
        public bool IsOnZone { get; internal set; }


        /// <summary>Checks if there's any Input with the Input Active</summary>
        public Mode InputMode { get; internal set; }

        /// <summary>Set/Get the Active Mode</summary>
        public Mode ActiveMode
        {
            get { return activeMode; }
            set
            {
                //if (value != null && value != activeMode)            //Means is a new Mode entering
                //{
                //    if (activeMode != null)
                //        activeMode.ExitMode();      //if the Last Mode was still Active then Exit 
                //} 

                activeMode = value;

                if (value != null)
                {
                    ModeID = (activeMode.ID * 1000) +
                        (activeMode.ActiveAbility != null ? (int)activeMode.ActiveAbility.Index : 0);

                  SetIntID(Int_ID.Available); //IMPORTANT WHEN IS MAKING SOME RANDOM STUFF
                }
                else
                {
                    SetIntID(ModeID = Int_ID.Available);
                }
            }
        }

        /// <summary>Modes are Abilities the animal can do on top of the States</summary>
        public int ModeID
        {
            get { return modeID; }
            private set
            {
                modeID = value;
                SetAnimParameter(hash_Mode, modeID);
                OnModeChange.Invoke(modeID); //On Mode Change
            }
        }

        /// <summary>Status of a mode while playing</summary>
        public int ModeStatus
        {
            get { return modeStatus; }
            private set
            {
                modeStatus = value;
                SetAnimParameter(hash_Status, modeStatus);
               // OnModeChange.Invoke(modeStatus); //On Mode Change
            }
        }


        public Mode Pin_Mode { get; private set; }

 
        #endregion


        #region Pivots

        protected RaycastHit hit_Hip;            //Hip and Chest Ray Cast Information
        protected RaycastHit hit_Chest;            //Hip and Chest Ray Cast Information


        public List<MPivots> pivots = new List<MPivots>
        { new MPivots("Hip", new Vector3(0,0.7f,-0.7f), 1), new MPivots("Chest", new Vector3(0,0.7f,0.7f), 1), new MPivots("Water", new Vector3(0,1,0), 0.05f) };

        public MPivots Pivot_Hip;
        public MPivots Pivot_Chest;
        
        /// <summary>Does it have a Hip Pivot?</summary>
        private bool Has_Pivot_Hip;
        /// <summary>Does it have a Hip Pivot?</summary>
        private bool Has_Pivot_Chest;

        /// <summary> Do the Main (Hip Ray) found ground </summary>
        public bool MainRay { get; private set; }
        /// <summary> Do the Fron (Chest Ray) found ground </summary>
        public bool FrontRay { get; private set; }

        /// <summary>Main pivot Point is the Pivot Chest Position, if not the Pivot Hip Position one</summary>
        public Vector3 Main_Pivot_Point
        {
            get
            {
                Vector3 mainPivot;

                if (Has_Pivot_Chest) mainPivot = Pivot_Chest.World(_transform);

                else if (Has_Pivot_Hip) mainPivot = Pivot_Hip.World(_transform);

                else mainPivot = _transform.TransformPoint(new Vector3(0, Height, 0));

                return mainPivot; 
            }
        }

        /// <summary>The full Speed we want to without lerping, for the Additional Speed</summary>
        public Vector3 TargetSpeed
        {
            get
            {
                Vector3 forward = DirectionalSpeed;
                var SpeedModPos = CurrentSpeedModifier.position;

                forward = forward * SmoothZY * (UseAdditivePos ? 1 : 0);

                if (VerticalSmooth < 0)
                {
                    forward *= 0.5f;  //Decrease half when going backwards
                    SpeedModPos = CurrentSpeedSet.Speeds[0].position;
                }

                if (forward.magnitude > 1) forward.Normalize();

                return forward * SpeedModPos * ScaleFactor * DeltaTime;
            }
        }

        /// <summary>Check if there's no Pivot Active </summary>
        public bool NoPivot { get { return !Has_Pivot_Chest && !Has_Pivot_Hip; } }

        /// <summary> Gets the the Main Pivot Multiplier * Scale factor (Main Pivot is the Chest, if not then theHip Pivot) </summary>
        public float Pivot_Multiplier
        {
            get
            {
                float multiplier = Has_Pivot_Chest ? Pivot_Chest.multiplier : (Has_Pivot_Hip ? Pivot_Hip.multiplier : 1f);
                return multiplier * ScaleFactor * (NoPivot ? 1.5f : 1f);
            }
        }


        #endregion


        #region Speed Modifiers
        /// <summary>Speed Set for Stances</summary>
        public List<MSpeedSet> speedSets;
        /// <summary>Active Speed Set</summary>
        private MSpeedSet currentSpeedSet;
        /// <summary>True if the State is modifing the current Speed Modifier</summary>
        public bool CustomSpeed;

        public MSpeed currentSpeedModifier = MSpeed.Default;
        protected MSpeed SprintSpeed = MSpeed.Default;
        //public List<MSpeed> speedModifiers = new List<MSpeed>();

        protected int speedIndex;

        public MSpeed CurrentSpeedModifier
        {
            get
            {
                if (Sprint && !CustomSpeed)
                    return SprintSpeed;

                return currentSpeedModifier;
            }
            set
            {
                currentSpeedModifier = value;
                OnSpeedChange.Invoke(currentSpeedModifier);
            }
        }


        /// <summary>Current Speed Multiplier of the State</summary>
        public int CurrentSpeedIndex
        {
            internal set
            {
                speedIndex = value;

                if (CurrentSpeedSet == null) return;

                var SP = CurrentSpeedSet.Speeds;

                speedIndex = Mathf.Clamp(value, 1, SP.Count); //Clamp the Speed Index
                var sprintSpeed = Mathf.Clamp(value + 1, 1, SP.Count);

                CurrentSpeedModifier = SP[speedIndex - 1];

                SprintSpeed = SP[sprintSpeed - 1];

                if (CurrentSpeedSet != null) CurrentSpeedSet.CurrentIndex = speedIndex; //Keep the Speed saved on the state too in case the active speed was changed
            }
            get { return speedIndex; }
        }

        #endregion


        #region Gravity
        [SerializeField] private Vector3Reference gravityDirection = new Vector3Reference(-Vector3.up);
        /// <summary> How Fast the Animal will fall to the ground </summary>
        public FloatReference GravityForce = new FloatReference(3f);
        /// <summary> Gravity acceleration multiplier </summary>
        public FloatReference GravityMultiplier = new FloatReference(1f);

        public float GravityStoredAceleration { get; protected set; }
        public Vector3 GravityStoredVelocity { get; protected set; }

        /// <summary> Direction of the Gravity </summary>
        public Vector3 GravityDirection
        {
            get { return gravityDirection; }
            set { gravityDirection.Value = value; }
        }

        public Vector3 UpVector
        {
            get { return -gravityDirection.Value; }
        }

        private bool ground_Changes_Gravity;

        #endregion

        #region Advanced Parameters
        ///// <summary>Update all Parameters in the Animator Controller </summary>
        //public BoolReference DisableRootMotion = new BoolReference(false);

        public BoolReference rootMotion;
        /// <summary> Raudius for the Sphere Cast</summary>
        public FloatReference RayCastRadius = new FloatReference(0.05f);
        /// <summary>This parameter exist to Add Additive pose to correct the animal</summary>
        public IntReference animalType = new IntReference(0);
        #endregion


        #region Use Stuff Properties

        private bool grounded;
        /// <summary> Is the Animal on a surface, when True the Raycasting for the Ground is Applied</summary>
        public bool Grounded
        {
            internal set
            {
                if (grounded != value)
                {
                    grounded = value;
                    if (!value) platform = null; //If groundes is false remove the stored Platform 
                    SetAnimParameter(hash_Grounded, Grounded);
                    OnGrounded.Invoke(grounded);
                }
            }
            get { return grounded; }
        }

        /// <summary>Does the Active State uses Additive position?</summary>
        public bool UseAdditivePos { get; internal set; }
        //  public bool UseAdditiveRot { get;internal set; }
        /// <summary>Does the Active State uses Sprint?</summary>
        public bool UseSprintState { get; internal set; }
        public bool UseCustomAlign{ get; set; }
        /// <summary>The Animal is on Free Movement... which means is flying or swiming underwater</summary>
        public bool FreeMovement { get; set; }
        /// <summary>Enable Disable the Global Sprint</summary>
        public bool UseSprint
        {
            get { return useSprintGlobal; }
            set
            {
                useSprintGlobal.Value = value;
            }
        }
        
        /// <summary>Locks Input on the Animal, Ingore inputs like Jumps, Attacks , Actions etc</summary>
        public bool LockInput
        {
            get { return lockInput; }
            set
            {
                if (lockInput != value)
                {
                    lockInput.Value = value;
                    OnInputLocked.Invoke(lockInput);
                }
            }
        }

      
        /// <summary>Enable/Disable RootMotion on the Animator</summary>
        public bool RootMotion
        {
            set { Anim.applyRootMotion = rootMotion.Value = value; }
            get { return rootMotion; }
        }

        /// <summary>Does it use Gravity or not? </summary>
        public bool UseGravity
        {
            get { return useGravity; }
            set
            {
                useGravity = value;
                GravityStoredAceleration = 0;
                GravityStoredVelocity = Vector3.zero;
            }
        }

        /// <summary>Locks the Movement on the Animal</summary>
        public bool LockMovement
        {
            get { return lockMovement; }
            set
            {
                if (lockMovement != value)
                {
                    lockMovement.Value = value;
                    OnMovementLocked.Invoke(lockMovement);
                }
            }
        }
        /// <summary>if True It will Aling it to the ground rotation depending the Front and Back Pivots</summary>
        public bool UseOrientToGround { get; set; }
        private bool useGravity;
        public BoolReference lockInput;
        public BoolReference lockMovement;
        public BoolReference useSprintGlobal = new  BoolReference( true);
        #endregion


        #region Animator States Info
        protected AnimatorStateInfo m_CurrentState;             // Information about the base layer of the animator cached.
        protected AnimatorStateInfo m_NextState;
        protected AnimatorStateInfo m_PreviousCurrentState;    // Information about the base layer of the animator from last frame.
        protected AnimatorStateInfo m_PreviousNextState;
        internal bool m_IsAnimatorTransitioning;
        protected bool m_PreviousIsAnimatorTransitioning;


        /// <summary>Returns the Current Animation State Tag of animal, if is in transition it will return the NextState Tag</summary>
        public AnimatorStateInfo AnimState { get; set; }

        public int currentAnimTag;
        /// <summary>Current Active Animation Hash Tag </summary>
        public int AnimStateTag
        {
            get { return currentAnimTag; }
            private set
            {
                if (value != currentAnimTag)
                {
                    currentAnimTag = value;
                    activeState.AnimationTagEnter(value);

                    if (ActiveState.IsPending)                      //If the new Animation Tag is not on the New Active State try to activate it on the last State
                    {
                        LastState.AnimationTagEnter(value);
                    }
                }
            }
        }
        #endregion


        #region Animator Parameters Variables

       
        #endregion


        #region Platform
        protected Transform platform;
        protected Vector3 platform_Pos;
        protected Quaternion platform_Rot;
        protected float platform_formAngle;
        #endregion

        /// <summary>Used for Disabling Additive Position and Additive Rotation on the ANimal (The Pulling  take care of it)</summary>
        internal bool DisablePositionRotation = false;


        #region Extras
        /// <summary> Value for the Speed Multiplier Parameter on the Animator</summary>
        internal float SpeedMultiplier { get; set; }

        protected List<MAttackTrigger> Attack_Triggers;      //List of all the Damage Triggers on this Animal.
       

        /// <summary>Colliders to disable with animator </summary>
        List<Collider> colliders = new List<Collider>(); 
            
        /// <summary>Animator Normalized State Time for the Base Layer  </summary>
        public float StateTime { get; private set; }

        /// <summary>Store from where the damage came from</summary>
        public Vector3 HitDirection { set; get; }

        #endregion


        #region Events
        public IntEvent OnAnimationChange;

        //public UnityEvent OnSyncAnimator = new UnityEvent();       //Used for Sync Animators
        public BoolEvent OnInputLocked = new BoolEvent();          //Used for Sync Animators
        public BoolEvent OnMovementLocked = new BoolEvent();        //Used for Sync Animators
        public BoolEvent OnSprintEnabled = new BoolEvent();       //Used for Sync Animators
        public BoolEvent OnGrounded = new BoolEvent();       //Used for Sync Animators

        public IntEvent OnStateChange = new IntEvent();         //Invoked when is Changed to a new State
        public IntEvent OnModeChange = new IntEvent();          //Invoked when is Changed to a new Mode
        public IntEvent OnStanceChange = new IntEvent();        //Invoked when is Changed to a new Stance
        public SpeedModifierEvent OnSpeedChange = new SpeedModifierEvent();        //Invoked when a new Speed is changed
        public BoolEvent OnMainPlayer = new BoolEvent();          //Used for Sync Animators
        #endregion


        #region ID Int Float

        public int IntID { get; private set; }
        

        public float IDFloat { get; private set; }

        public MSpeedSet CurrentSpeedSet
        {
            get { return currentSpeedSet; }
           
            set
            {
                currentSpeedSet = value;
                CurrentSpeedIndex = currentSpeedSet.CurrentIndex;

               // speedModifiers = currentSpeedSet.Speeds;
            }
        }


        #endregion

        #region Animator Parameters

        [SerializeField] private string m_Vertical = "Vertical";
        [SerializeField] private string m_Horizontal = "Horizontal";
        [SerializeField] private string m_UpDown = "UpDown";

        [SerializeField] private string m_IDFloat = "IDFloat";
        [SerializeField] private string m_IDInt = "IDInt";
        [SerializeField] private string m_Grounded = "Grounded";
        [SerializeField] private string m_Movement = "Movement";


        [SerializeField] private string m_State = "State";
        [SerializeField] private string m_LastState = "LastState";
        [SerializeField] private string m_Mode = "Mode";
        [SerializeField] private string m_Status = "Status";

        [SerializeField] private string m_Stance = "Stance";
        [SerializeField] private string m_Slope = "Slope";
        [SerializeField] private string m_Type = "Type";
        [SerializeField] private string m_SpeedMultiplier = "SpeedMultiplier";
        [SerializeField] private string m_StateTime = "StateTime";

        //[SerializeField] private string m_Randomizer = "Randomizer";
        [SerializeField] private string m_DeltaAngle = "DeltaAngle";

        internal int hash_Vertical;
        internal int hash_Horizontal;
        internal int hash_UpDown;

       // internal int hash_ActionID;
       // internal int hash_Action;

        internal int hash_IDInt;
        internal int hash_IDFloat;

       // internal int hash_Randomizer;

        internal int hash_State;
        internal int hash_LastState;
        internal int hash_Slope;
        //internal int hash_Sprint;

        internal int hash_Mode;
        internal int hash_Status;
        internal int hash_Type;
        internal int hash_SpeedMultiplier;
        internal int hash_StateTime;
        internal int hash_Stance;
        internal int hash_Movement;
        internal int hash_DeltaAngle;
        internal int hash_Grounded;


        #region Optional Animator Parameters Activation
        private bool hasUpDown;
        //private bool hasCameraSide;
        private bool hasDeltaAngle;
        private bool hasSlope;
        private bool hasSpeedMultiplier;
        private bool hasStateTime;
        private bool hasStance;

        #endregion




        #endregion


        public bool debugStates = true;
        public bool debugModes = true;
        public bool debugGizmos = true;

    }

    [System.Serializable]
    public class OnEnterExitState
    {
        public StateID ID;
        public UnityEvent OnEnter;
        public UnityEvent OnExit;
    }

    [System.Serializable]
    public class SpeedModifierEvent : UnityEvent<MSpeed> { }
}
