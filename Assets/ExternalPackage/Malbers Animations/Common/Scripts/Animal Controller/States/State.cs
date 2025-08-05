using MalbersAnimations.Events;
using MalbersAnimations.Scriptables;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace MalbersAnimations.Controller
{
    public abstract class State : ScriptableObject 
    {
        /// <summary>Index on the Array of States</summary>
        [HideInInspector] public int Index;
        /// <summary>You can enable/disable temporarly  the State</summary>
        [HideInInspector] public bool Active = true;


        /// <summary>True if this state is the Active State</summary>
        public bool ActiveState { get; internal set; }
        /// <summary>Priority of the State.  Higher value more priority</summary>
        public int Priority { get; internal set; }

        /// <summary>Reference for the Animal that Holds this State</summary>
        protected MAnimal animal;
        /// <summary>ID Asset Reference</summary>
        public StateID ID;
        ///// <summary> Reset a Given State when entering this state </summary>
        //public StateID resetState;
        //[Header("General")]
        /// <summary>Input to Activate this State</summary>
        public string Input;

        ///// <summary>If a Mode is playing then it will stop it</summary>
        //public bool StopMode = true;

        public AnimalModifier General;
        protected Transform transform;

        protected Animator Anim { get { return animal.Anim; } }

        protected OnEnterExitState EnterExitEvent;

        /// <summary>Main Tag of the Animation State which it will rule the State the ID name Converted to Hash</summary>
        public int MainTagHash { get; private set; }

        [Tooltip("if True, the state will execute another frame of logic while entering the other state ")]
        public bool ExitFrame = true;
          

        /// <summary> If the Active State is one of one on the List ...This State cannot be enable</summary>
        [SerializeField] public  List<StateID> SleepFrom = new List<StateID>();
      
        /// <summary> If the Active State is one of one on the List ...This State cannot be enable</summary>
        [SerializeField] public List<StateID> QueueFrom = new List<StateID>();
       
        [SerializeField] public List<TagModifier> TagModifiers = new List<TagModifier>();

        public bool debug = false;
      
        #region Common Properties
        /// <summary>Returns the Active Animation State tag Hash on the Base Layer</summary>
        protected int CurrentAnimTag { get { return animal.AnimStateTag; } }

        /// <summary>Check if the State was Activated by an Input</summary>
        public bool InputValue { get; set; }

        /// <summary>Put a state to sleep it works with the Avoid States list</summary>
        public bool Sleep { get; set; }

        /// <summary>is this state being on queue?</summary>
        public bool OnQueue { get; set; }

        /// <summary>If True this state cannot be interrupted by other States</summary>
        public bool IsPersistent { get; set; }
        /// <summary>If true the states below it will not try to Activate themselves</summary>
        public bool IgnoreLowerStates { get; set; }
       
       
        /// <summary>Means that is already active but is Still exiting the Last State</summary>
        public bool IsPending { get; set; }

        ///// <summary>Set a new Speed index on the Animal. if is -1 then Speeds Modifiers will be ingored if 0 it will set to Default speeds</summary>
        //public int SpeedIndex { get; set; }


        /// <summary>Speed Set this State has... if Null the state will not change speeds</summary>
        public MSpeedSet SpeedSet { get; set; }
        #endregion

        #region Methods
        /// <summary> Return if this state have a current Tag used on the animal</summary>
        protected bool StateAnimationTags(int MainTag)
        {
            if (MainTagHash == MainTag) return true;

            var Foundit = TagModifiers.Find(tag => tag.TagHash == MainTag);

            return Foundit != null;
        }

        /// <summary>Set all the values for all the States on Awake</summary>
        public void SetAnimal(MAnimal mAnimal)
        {
            animal = mAnimal;
            transform = animal.transform;
        }

        /// <summary>Called on Awake</summary>
        public virtual void AwakeState()
        {
            MainTagHash = Animator.StringToHash(ID.name);                       //Store the Main Tag at Awake

            foreach (var mod in TagModifiers)
            {
                mod.TagHash = Animator.StringToHash(mod.AnimationTag);          //Convert all the Tags to HashTags
            }

            SpeedSet = null;
            foreach (var set in animal.speedSets) //Find if this state has a Speed Set
            {
                if (set.states.Contains(ID))
                {
                    SpeedSet = set;
                    break;
                }
            }

            EnterExitEvent = animal.OnEnterExitStates.Find(st => st.ID == ID);

            ExitState(); //Reset all the values to their default IMPORTANT
        }



        /// <summary>Activate the State </summary>
        public virtual void Activate()
        {
            SetSpeeds();

            animal.LastState = animal.ActiveState;                          //Set a new Last State
            var LastState = animal.LastState;

            if (LastState != null && QueueFrom.Contains(LastState.ID) && !OnQueue)
            {
                if (animal.debugStates) Debug.Log(ID.name + " State has being Queue");
                OnQueue = true;
                animal.QueueState(this);
            }

            if (OnQueue) return;

            if (animal.debugStates) Debug.Log("Enter State: " + ID.name);

            LastStateExit();

            animal.JustActivateState = true;

            animal.ActiveState = this;                                      //Update to the Current State
            ActiveState = true;

            if (EnterExitEvent != null) EnterExitEvent.OnEnter.Invoke();
        }

        /// <summary>Invoke the Exit State for the Laset State and Execute the Exit State method</summary>
        private void LastStateExit()
        {
            if (animal.LastState.EnterExitEvent != null)
            {
                animal.LastState.EnterExitEvent.OnExit.Invoke();
            }
            

            //HasEnterState = false;
            animal.LastState.ExitState();
        }


        /// <summary> Wake all the Sleep States when entering a new State</summary>
        public virtual void WakeUpStates()
        {
            foreach (var st in animal.sleepStates)
            {
                st.Sleep = false;
                st.JustWakeUp();        //Call Just Wake Up State in case any state needs a frame of calculation :)
            }
        }

        public virtual void ActivateQueued()
        {
            OnQueue = false;

            animal.LastState = animal.ActiveState;                          //Set a new Last State

            LastStateExit();

          //  animal.JustActivateState = true;
            animal.ActiveState = this;                                      //Update to the Current State
            ActiveState = true;
            //animal.CurrentSpeedIndex = SpeedIndex;

            if (EnterExitEvent != null) EnterExitEvent.OnEnter.Invoke();
        }

     // bool HasEnterState;

        /// <summary>When a Tag Changes apply this modifications</summary>
        public bool AnimationTagEnter(int animatorTagHash)
        {
            IsPending = true;

            if (MainTagHash == animatorTagHash)
            {
                General.Modify(animal);
                //animal.SetIntID(0);
                IsPending = false;
            }

            TagModifier ActiveTag = TagModifiers.Find(tag => tag.TagHash == animatorTagHash);

            if (ActiveTag != null)
            {
                ActiveTag.modifier.Modify(animal);
                IsPending = false;
            }

            if (ActiveState/* && !HasEnterState*/)
            {
                //HasEnterState = true;
                AnimationStateEnter();
            }

            return IsPending;
        }

        /// <summary>This try to enable the State Logic</summary>
        public virtual bool TryActivate() { return false; }

        public void ReceiveMessages(string message, object value)
        {
            this.Invoke(message, value);
        }

        /// <summary>Enable the State using an Input. Example :Fly, Jump </summary>
        public void ActivatebyInput(bool InputValue)
        {
            // Debug.Log(InputValue);
            this.InputValue = InputValue;

            State activeState = animal.ActiveState;
            if (activeState == null) return;
           
            if (!Active || Sleep || animal.LockInput || activeState.IsPersistent) return;    //Do no Activate if is sleep or disable or lock input is Enable;
            if (activeState.IgnoreLowerStates && activeState.Priority > Priority) return;  //Do not Activate if the Active state is greater and has ignore lower states

            StatebyInput();
        }

        public virtual void ExitState()
        {
            InputValue = false;
            IgnoreLowerStates = false;
            IsPersistent = false;
            IsPending = false;
            OnQueue = false;
            ActiveState = false; //This will not be any longer the Active State
          //  HasEnterState = false;
        }

        /// <summary> Reset a State values to its first Awakening </summary>
        public virtual void ResetState() { ExitState(); }


        /// <summary> Reset a State from the animal is using it with a StateID to its first Awakening </summary>
        public virtual void ResetState(StateID id)
        {
            if (animal != null) animal.State_Reset(id);
        } 

        /// <summary>Allow the State to be Replaced by lower States</summary>
        public void AllowExit()
        {
            IgnoreLowerStates = false;
            IsPersistent = false;
        }
        #endregion

        /// <summary>Used to set the speeds </summary>
        protected void SetSpeeds()
        {
            if (SpeedSet != null)
            {
                animal.CurrentSpeedSet = SpeedSet;    //Set a new Speed Set
                animal.CustomSpeed = false;
                animal.CurrentSpeedIndex = SpeedSet.CurrentIndex;
            }
        }

        #region Empty Methods
        /// <summary>Reset the State When is added to the List of states</summary>
        public virtual void Reset() { }

        /// <summary>Set all the values for all the States on Start</summary>
        public virtual void StartState() { }

        /// <summary>When Entering a new animation State do this</summary>
        public virtual void AnimationStateEnter() { }

        /// <summary>Logic to Try exiting to Lower Priority States</summary>
        public virtual void TryExitState(float DeltaTime) { }


        /// <summary>Called when Sleep is false</summary>
        public virtual void JustWakeUp() { }

        public virtual void StatebyInput() { }

        public virtual void OnStateMove(float deltatime) { }
        public virtual void DebugState() { }

        #endregion
    }


    /// <summary>When an new Animation State Enters and it have a tag = "AnimationTag"</summary>
    [System.Serializable]
    public class TagModifier
    { 
        /// <summary>Animation State with the Tag  to apply the modifiers</summary>
        public string AnimationTag;
        public AnimalModifier modifier;
        /// <summary>"Animation Tag" Converted to TagHash</summary>
        public int TagHash { get; set; }
    }

    /// <summary>Modifier for the Animals</summary>
    [System.Serializable]
    public struct AnimalModifier
    {
        ///// <summary>Animation State with the Tag  to apply the modifiers</summary>
        //public string AnimationTag;

        [Utilities.Flag]
        public modifier modify;

        /// <summary>Enable/Disable the Root Motion on the Animator</summary>
        public bool RootMotion;
        /// <summary>Enable/Disable the Sprint on the Animal </summary>
        public bool Sprint;
        /// <summary>Enable/Disable the Gravity on the Animal, only used when the animal is on the air, falling, jumping ..etc</summary>
        public bool Gravity;
        /// <summary>Enable/Disable the if the Animal is Grounded (Align|Snap to ground position) </summary>
        public bool Grounded;
        /// <summary>Enable/Disable the Rotation Alignment while grounded </summary>
        public bool OrientToGround;
        /// <summary>Enable/Disable the  Custom Rotations (Used in Fly, Climb, UnderWater Swimming, etc)</summary>
        public bool CustomRotation;
        /// <summary>Enable/Disable the Free Movement... This allow to Use the Pitch direction vector</summary>
        public bool FreeMovement;
        /// <summary>Enable/Disable Additive Position use on the Speed Modifiers</summary>
        public bool AdditivePosition;
        ///// <summary>Enable/Disable Additive Rotation use on the Speed Modifiers</summary>
        //public bool AdditiveRotation = true;


        /// <summary>Enable/Disable is Persistent on the Active State ... meaning it cannot activate any other states whatsoever</summary>
        public bool Persistent;

        /// <summary>Enable/Disable is Persistent on the Active State ... meaning it cannot activate any other states whatsoever</summary>
        public bool IgnoreLowerStates;

        /// <summary>Enable/Disable the movement on the Animal</summary>
        public bool LockMovement;

        /// <summary>Enable/Disable is AllInputs on the Animal</summary>
        public bool LockInput;

        /// <summary>Enable/Disable is AllInputs on the Animal</summary>
        public bool Colliders;



        public void Modify(MAnimal animal)
        {
            if ((int)modify == 0) return; //Means that the animal have no modification

            if (Modify(modifier.IgnoreLowerStates)) animal.ActiveState.IgnoreLowerStates = IgnoreLowerStates;
            if (Modify(modifier.AdditivePositionSpeed))  animal.UseAdditivePos = AdditivePosition;
    
                //  if (Modify(modifier.AdditiveRotationSpeed)) animal.UseAdditiveRot = AdditiveRotation;
            if (Modify(modifier.RootMotion)) animal.RootMotion = RootMotion;
            if (Modify(modifier.Gravity)) animal.UseGravity = Gravity;
            if (Modify(modifier.Sprint)) animal.UseSprintState = Sprint;
           
            if (Modify(modifier.Grounded)) animal.Grounded = Grounded;
            if (Modify(modifier.OrientToGround)) animal.UseOrientToGround = OrientToGround;
            if (Modify(modifier.CustomRotation)) animal.UseCustomAlign = CustomRotation;
            if (Modify(modifier.Persistent)) animal.ActiveState.IsPersistent = Persistent;
            if (Modify(modifier.LockInput)) animal.LockInput = LockInput;
            if (Modify(modifier.LockMovement)) animal.LockMovement = LockMovement;
            if (Modify(modifier.Colliders)) animal.EnableColliders(Colliders);
            if (Modify(modifier.FreeMovement)) animal.FreeMovement = FreeMovement;
        }

        private bool Modify(modifier modifier)
        {
            return ((modify & modifier) == modifier);
        }


       
    }
    public enum modifier
    {
        RootMotion = 1,
        Sprint = 2,
        Gravity = 4,
        Grounded = 8,
        OrientToGround = 16,
        CustomRotation = 32,
        IgnoreLowerStates = 64,
        Persistent = 128,
        LockMovement = 256,
        LockInput = 512,
        Colliders = 1024,
        AdditivePositionSpeed = 2048,
        FreeMovement = 4096,
    }
}
