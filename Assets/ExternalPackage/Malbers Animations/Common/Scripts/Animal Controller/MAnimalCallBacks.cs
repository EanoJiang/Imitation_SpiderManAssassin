using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MalbersAnimations.Scriptables;

namespace MalbersAnimations.Controller
{
    /// All Callbacks/Public Methods are Here
    public partial class MAnimal
    {
        /// <summary>Set an Animal as the Main Player and remove the otherOne</summary>
        public virtual void SetMainPlayer()
        {
            if (MAnimal.MainAnimal)
            {
                MAnimal.MainAnimal.isPlayer.Value = false;
                MAnimal.MainAnimal.OnMainPlayer.Invoke(false);
            }

            this.isPlayer.Value = true;
            MAnimal.MainAnimal = this;
            OnMainPlayer.Invoke(true);
        }

        void DisableMainPlayer()
        {
            if (MainAnimal == this)
            {
                MainAnimal = null;
                OnMainPlayer.Invoke(isPlayer.Value = false);
            }
        }


        #region Teleport    
        public virtual void Teleport(Transform newPos)
        {
            if (newPos)
            {
                Teleport(newPos.position);
            }
            else
            {
                Debug.LogWarning("You are using Teleport but the Transform you are entering on the parameters is null");
            }
        }

        public virtual void TeleportRot(Transform newPos)
        {
            if (newPos)
            {
                Teleport(newPos.position);
                _transform.rotation = newPos.rotation;
            }
            else
            {
                Debug.LogWarning("You are using TeleportRot but the Transform you are entering on the parameters is null");
            }
        }


        public virtual void Teleport(Vector3 newPos)
        {
            transform.position = newPos;
            LastPos = transform.position;
            platform = null;
        }
        #endregion

        #region Gravity
        /// <summary>Resets the gravity to the default Vector.Down value</summary>
        public void ResetGravityDirection() { GravityDirection = Vector3.down; }
        /// <summary>Resets the gravity to the default Vector.Down value</summary>
        public void ResetGravity() { ResetGravityDirection(); }

        /// <summary>The Ground</summary>
        public void GroundChangesGravity(bool value) { ground_Changes_Gravity = value; }

        /// <summary>Aling with no lerp to the Gravity Direction</summary>
        public void AlignGravity()
        {
            Quaternion AlignRot = Quaternion.FromToRotation(_transform.up, UpVector) * _transform.rotation;  //Calculate the orientation to Terrain 
            transform.rotation = AlignRot;
        }
        #endregion

        #region Stances
        /// <summary>Toogle the New Stance with the Default Stance▼▲ </summary>
        public void StanceToggle(int NewStance)
        {
            Stance = Stance == NewStance ? 0 : NewStance;
        }

        /// <summary>Toogle the New Stance with the Default Stance▼▲ </summary>
        public void StanceToggle(StanceID NewStance) { StanceToggle(NewStance.ID); }

        public void StanceSet(StanceID id) { Stance = id; }

        public void StanceReset() { Stance = 0; }

        #endregion

        #region Animator Methods
        /// <summary>
        /// Method required for the Interface IAnimator Listener to send messages From the Animator to any class who uses this Interface
        /// </summary>
        public virtual void OnAnimatorBehaviourMessage(string message, object value)
        {
            this.InvokeWithParams(message, value);


            foreach (var state in states)
            {
                state.ReceiveMessages(message, value);
            }
        }

        /// <summary>Set a Int on the Animator</summary>
        internal void SetAnimParameter(int hash, int value) { Anim.SetInteger(hash, value); }

        /// <summary>Set a float on the Animator</summary>
        internal void SetAnimParameter(int hash, float value) { Anim.SetFloat(hash, value); }

        /// <summary>Set a Bool on the Animator</summary>
        internal void SetAnimParameter(int hash, bool value) { Anim.SetBool(hash, value); }

        /// <summary>Set a Trigger to the Animator</summary>
        internal void SetAnimParameter(int hash) { Anim.SetTrigger(hash); }

        /// <summary> Set the Parameter Int ID to a value and pass it also to the Animator </summary>
        public void SetIntID(int value)
        {
            SetAnimParameter(hash_IDInt, IntID = value);
        }

        /// <summary> Set the Parameter Float ID to a value and pass it also to the Animator </summary>
        public void SetFloatID(float value) { SetAnimParameter(hash_IDFloat, IDFloat = value); }

        ///// <summary>Set a Random number to ID Int , that work great for randomly Play More animations</summary>
        //protected void SetIntIDRandom(int range) { SetIntID(IntID = Random.Range(1, range + 1)); }

        /// <summary>Used by Animator Events </summary>
        public virtual void EnterTag(string tag) { AnimStateTag = Animator.StringToHash(tag); }
        #endregion

        #region States

        /// <summary>Force the Activation of an state regarding if is enable or not</summary>
        public virtual void State_Force(StateID ID) { State_Force(ID.ID); }

        /// <summary>Force the Activation of an state regarding if is enable or not</summary>
        public virtual void State_Force(int ID) { statesD[ID].Activate(); }

        /// <summary>Try to Activate a State direclty from the Animal Script </summary>
        public virtual void State_Activate(StateID id) { State_TryActivate(id.ID); }

        public virtual bool State_TryActivate(int ID)
        {
            State NewState;

            if (statesD.TryGetValue(ID, out NewState))
            {
                if (ActiveState.Priority > NewState.Priority && ActiveState.IgnoreLowerStates)
                {
                    return false;  //if the Active state is set to ignore the lower ones then SKip
                }

                if (ActiveState == NewState) return false;                      //We are already on this state
                if (ActiveState.IsPersistent) return false;                     //if the Active state is persitent then ignore the Activation
                if (!NewState.Active || NewState.Sleep) return false;           //if the New state is disabled or os sleep ignore Activation

                NewState.Activate();
            }
            return true;
        }

        /// <summary>Try to Activate a State direclty from the Animal Script </summary>
        public virtual void State_Activate(int ID)
        {
            State NewState;

            if (statesD.TryGetValue(ID, out NewState))
            {
                if (ActiveState.Priority > NewState.Priority && ActiveState.IgnoreLowerStates)
                {
                    return;  //if the Active state is set to ignore the lower ones then SKip
                }

                if (ActiveState == NewState) return;                        //We are already on this state
                if (ActiveState.IsPersistent) return;                       //if the Active state is persitent then ignore the Activation
                if (!NewState.Active || NewState.Sleep) return;             //if the New state is disabled or os sleep ignore Activation

                //  Debug.Log("Zone State: "+ ID);
                NewState.Activate();
            }
        }



        /// <summary> Return a State by its ID </summary>
        public virtual State GetState(int ID)
        {
            State state;

            if (statesD.TryGetValue(ID, out state))
            {
                return state;
            }
            return null;
        }

        /// <summary> Call the Reset State on the given State </summary>
        public virtual void State_Reset(int ID)
        {
            State toReset;
            if (statesD.TryGetValue(ID, out toReset))
            {
                toReset.ResetState();
            }
        }

        /// <summary> Call the Reset State on the given State </summary>
        public virtual void State_Reset(StateID ID) { State_Reset(ID.ID); }


        ///<summary> Find to the Possible State and store it to the (PreState) using an StateID value</summary>
        public virtual void State_Pin(StateID stateID)
        {
            State_Pin(stateID.ID);
        }

        ///<summary> Find to the Possible State and store it to the (PreState) using an int value</summary>
        public virtual void State_Pin(int stateID)
        {
            if (PreInputState == null || PreInputState.ID != stateID)
            {
                statesD.TryGetValue(stateID, out PreInputState);
            }
        }

        ///<summary>Use the (PreState) the and Try to activate it using an Input</summary>
        public virtual void State_Pin_ByInput(bool input)
        {
            if (PreInputState != null && !ActiveState.IsPersistent)   //if we have stored a PreInput state and if the Active State is not persistent then Try to activate it
            {
                if (ActiveState.IgnoreLowerStates && ActiveState.Priority > PreInputState.Priority) return; //if the Active State is not Ignoring Lower States then Activae it
                PreInputState.ActivatebyInput(input);
            }
        }

        ///<summary> Send to the Possible State (PreState) the value of the Input</summary>
        public virtual void State_ByInput(StateID stateID, bool input)
        {
            State_Pin(stateID.ID);
            State_Pin_ByInput(input);
        }

        ///<summary> Send to the Possible State (PreState) the value of the Input</summary>
        public virtual void State_ByInput(int stateID, bool input)
        {
            State_Pin(stateID);
            State_Pin_ByInput(input);
        }
        #endregion

        #region Modes
        /// <summary> Returns a Mode by its ID</summary>
        public virtual Mode Mode_Get(ModeID ModeID)
        { return Mode_Get(ModeID.ID); }

        /// <summary> Returns a Mode by its ID</summary>
        public virtual Mode Mode_Get(int ModeID)
        {
            Mode result;

            if (modesD.TryGetValue(ModeID, out result))
            {
                return result;
            }

            return null;
        }

        /// <summary>Enable a mode on the Animal</summary>
        public virtual void Mode_Activate(ModeID ModeID)
        {
            Mode_Activate(ModeID.ID, -1);
        }

        /// <summary>Enable a mode on the Animal</summary>
        /// <param name="ModeID">ID of the Mode</param>
        /// <param name="AbilityIndex">Ability Index. If this value is -1 then the Mode will activate a random Ability</param>
        public virtual void Mode_Activate(ModeID ModeID, int AbilityIndex)
        {
            Mode_Activate(ModeID.ID, AbilityIndex);
        }


        /// <summary>Enable a mode on the Animal</summary>
        /// <param name="ModeID">ID of the Mode</param>
        /// <param name="AbilityIndex">Ability Index. If this value is -1 then the Mode will activate a random Ability</param>
        public virtual void Mode_Activate(int ModeID, int AbilityIndex = -1)
        {
            var mode = Mode_Get(ModeID);

            if (mode != null && mode.active)
            {
                Pin_Mode = mode;
                Pin_Mode.AbilityIndex = AbilityIndex;
                Pin_Mode.TryActivate();
            }
            else
            {
                Debug.LogWarning("You are trying to Activate a Mode but here's no Mode with the ID or is Disabled: " + ModeID);
            }
        }

        public void UseCameraBasedInput()
        {
            UseCameraInput = true;
        }

        public virtual void Mode_Activate_Endless(ModeID ModeID)
        {
            Mode_Activate_Endless(ModeID.ID);
        }

        public virtual void Mode_Activate_Endless(int ModeID)
        {
            var mode = Mode_Get(ModeID);

            if (mode != null && mode.active)
            {
                Pin_Mode = mode;
                InputMode = Pin_Mode;
            }
        }

        /// <summary>Stop all modes </summary>
        public virtual void Mode_Stop() { ActiveMode = null; }

        public virtual void Mode_Stop_Endless()
        {
            InputMode = null;
            ActiveMode = null;
        }

        /// <summary>Set IntID to -2 to exit the Mode Animation</summary>
        public virtual void Mode_Interrupt() { SetIntID(Int_ID.Interrupted); }         //Means the Mode is interrupted
        

        /// <summary>Disable a Mode by his ID</summary>
        public virtual void Mode_Disable(ModeID id) { Mode_Disable((int)id); }

        /// <summary>Disable a Mode by his ID</summary>
        public virtual void Mode_Disable(int id)
        {
            var mod = Mode_Get(id);
            if (mod != null) mod.Disable();  
        }


        /// <summary>Enable a Mode by his ID</summary>
        public virtual void Mode_Enable(ModeID id)
        { Mode_Enable( id.ID); }

        /// <summary>Enable a Mode by his ID</summary>
        public virtual void Mode_Enable(int id)
        {
            var newMode = Mode_Get(id);
            if (newMode != null)
                newMode.active = true;
        }


        /// <summary>Pin a mode to Activate later</summary>
        public virtual void Mode_Pin(ModeID ModeID)
        {
            if (Pin_Mode != null && Pin_Mode.ID == ModeID) return;  //the mode is already pinned

            Pin_Mode = Mode_Get(ModeID);

            if (Pin_Mode == null) Debug.LogWarning("There's no " + ModeID.name + "Mode");
        }
   

        /// <summary>Pin an Ability on the Pin Mode to Activate later</summary>
        public virtual void Mode_Pin_Ability(int AbilityIndex)
        {
            if (AbilityIndex == 0) return;

            if (Pin_Mode != null)
                Pin_Mode.AbilityIndex = AbilityIndex;
        }


        /// <summary>Changes the Pinned Mode Status</summary>
        public virtual void Mode_Pin_Status(int aMode)
        {
            if (Pin_Mode != null)
                Pin_Mode.GlobalProperties.Status  = (AbilityStatus) aMode ;
        }

        /// <summary>Changes the Pinned Mode time when using Hold by time Status</summary>
        public virtual void Mode_Pin_Time(float time)
        {
            if (Pin_Mode != null)
                Pin_Mode.GlobalProperties.HoldByTime = time;
        }

        public virtual void Mode_Pin_Input(bool value)
        {
            if (Pin_Mode != null)
            {
                Pin_Mode.ActivatebyInput(value);
            }
        }
        /// <summary>Tries to Activate the Pin Mode</summary>
        public virtual void Mode_Pin_Activate()
        {
            if (Pin_Mode != null)
                Pin_Mode.TryActivate();
        }

        /// <summary>Tries to Activate the Pin Mode with an Ability</summary>
        public virtual void Mode_Pin_AbilityActivate(int AbilityIndex)
        {
            if (AbilityIndex == 0) return;

            if (Pin_Mode != null)
            {
                Pin_Mode.AbilityIndex = AbilityIndex;
                Pin_Mode.TryActivate();
            }
        }

        public virtual void SetAction(Action actionID)
        {
            Mode_Activate(ModeEnum.Action, actionID);          //4 is the ID for the Actions
        }

        #region Attacks Commands
        /// <summary> Tries to Activate the Primary Attack Mode with an Attack ID Animation</summary>
        public virtual void SetAttack(int attackID) { Mode_Activate(ModeEnum.Attack1, attackID); }

        /// <summary> Tries to Activate the Primary Attack Mode with an Random Attack Animation</summary>
        public virtual void SetAttack() { Mode_Activate(ModeEnum.Attack1, -1); }

        /// <summary> Tries to Activate the Primary Attack Forever... until StopAttack Is Called... usefull for AI</summary>
        public virtual void SetAttack_Endless(int attackID) { Mode_Activate_Endless(ModeEnum.Attack1); }

        /// <summary> Stop Primary Attack Animations... usefull for AI</summary>
        public virtual void StopAttack() { Mode_Stop_Endless(); }

        /// <summary> Tries to Activate the Secondary Attack Mode with an Attack ID Animation</summary>
        public virtual void SetAttack2(int attackID) { Mode_Activate(ModeEnum.Attack2, attackID); }

        /// <summary> Try to Activate the Secondary Attack Mode with an Random Attack Animation</summary>
        public virtual void SetAttack2() { Mode_Activate(ModeEnum.Attack2, -1); }

        /// <summary> Try to Activate the Secondary Attack Forever... until StopAttack Is Called... usefull for AI</summary>
        public virtual void SetAttack2_Endless(int attackID) { Mode_Activate_Endless(ModeEnum.Attack2); }

        /// <summary> Stop Secondary Attack Animations... usefull for AI</summary>
        public virtual void StopAttack2() { Mode_Stop_Endless(); }
        #endregion

        #endregion

        #region Movement

        /// <summary> Get the Inputs for the Source to add it to the States </summary>
        internal virtual void GetInputs(bool add)
        {
            InputSource = GetComponentInParent<IInputSource>();

            if (InputSource != null)
            {
                //Enable Disable the Inputs for the States
                foreach (var state in states)
                {
                    if (state.Input != string.Empty)
                    {
                        var input = InputSource.GetInput(state.Input);
                        if (input != null)
                        {
                            if (add)
                                input.OnInputChanged.AddListener(state.ActivatebyInput);
                            else
                                input.OnInputChanged.RemoveListener(state.ActivatebyInput);
                        }
                    }
                }


                //Enable Disable the Inputs for the States
                foreach (var mode in modes)
                {
                    if (mode.Input != string.Empty)
                    {
                        var input = InputSource.GetInput(mode.Input);

                        if (input != null)
                        {
                            if (add)
                                input.OnInputChanged.AddListener(mode.ActivatebyInput);
                            else
                                input.OnInputChanged.RemoveListener(mode.ActivatebyInput);
                        }
                    }
                }
            }
        }
        /// <summary>Gets the movement from the Input Script or AI</summary>
        public virtual void Move(Vector3 move)
        {
            MoveDirection(move);
        }

        /// <summary>Gets the movement from the Input using a 2 Vector  (ex UI Axis Joystick)</summary>
        public virtual void Move(Vector2 move)
        {
            Vector3 move3 = new Vector3(move.x, 0, move.y);
            MoveDirection(move3);
        }

        /// <summary>Gets the movement from the Input ignoring the Direction Vector, using a 2 Vector  (ex UI Axis Joystick)</summary>
        public virtual void MoveWorld(Vector2 move)
        {
            Vector3 move3 = new Vector3(move.x, 0, move.y);
            MoveWorld(move3);
        }

        /// <summary>Stop the animal from moving</summary>
        public virtual void StopMoving() { Move(Vector3.zero); }

        public virtual void ResetMovement() { Move(Vector3.zero); }
    


        /// <summary>Add Inertia to the Movement</summary>
        public virtual void AddInertia(ref Vector3 Inertia, float deltatime)
        {
            AdditivePosition += Inertia;
            Inertia = Vector3.Lerp(Inertia, Vector3.zero, deltatime);
        }


        /// <summary>Change the Speed Up</summary>
        public virtual void SpeedUp() { AddSpeed(+1); }

        /// <summary> Changes the Speed Down </summary>
        public virtual void SpeedDown() { AddSpeed(-1); }

        public virtual void SetCustomSpeed(MSpeed customSpeed, bool keepInertiaSpeed = false)
        {
            CustomSpeed = true;
            CurrentSpeedModifier = customSpeed;

            if (keepInertiaSpeed)
            {
                InertiaPositionSpeed = TargetSpeed; //Set the Target speed to the Fall Speed so there's no Lerping when the speed changes
            }
        }

        private void AddSpeed(int change)
        {
            if (CurrentSpeedSet == null) return;
            var SP = CurrentSpeedSet.Speeds;


            if (CustomSpeed) return;

            var value = speedIndex + change;

            value = Mathf.Clamp(value, 1, SP.Count);        //Clamp the Speed Index
            var sprintSpeed = Mathf.Clamp(value + 1, 1, SP.Count);

            if (value > CurrentSpeedSet.TopIndex)
            {
               // Debug.Log("REACH TOP");
                return;
            }

            speedIndex = value;
            CurrentSpeedModifier = SP[speedIndex - 1];

            SprintSpeed = SP[sprintSpeed - 1];
            if (CurrentSpeedSet != null) CurrentSpeedSet.CurrentIndex = speedIndex; //Keep the Speed saved on the state too in case the active speed was changed
        }

        /// <summary> Set an specific Speed for a State </summary>
        public virtual void SetSpeed(int speedIndex) { CurrentSpeedIndex = speedIndex; }

        /// <summary> Set an specific Speed for a State using IntVars </summary>
        public virtual void SetSpeed(IntVar speedIndex) { CurrentSpeedIndex = speedIndex; }
        #endregion

        #region Extras

        /// <summary>Method for the IDamagable Interface</summary>
        public virtual void Damage()
        {
            Mode_Activate(ModeEnum.Damage);
        }

        /// <summary>Activate Attack triggers  </summary>
        internal virtual void AttackTrigger(int triggerIndex)
        {
            if (triggerIndex == -1)                         //Enable all Attack Triggers
            {
                foreach (var trigger in Attack_Triggers)
                {
                    trigger.enabled = true;
                }
                return;
            }

            if (triggerIndex == 0)                          //Disable all Attack Triggers
            {
                foreach (var trigger in Attack_Triggers)
                {
                    trigger.enabled = false;
                }

                return;
            }


            List<MAttackTrigger> Att_T =
                Attack_Triggers.FindAll(item => item.index == triggerIndex);        //Enable just a trigger with an index

            if (Att_T != null)
            {
                foreach (var trigger in Att_T)
                {
                    trigger.enabled = true;
                }
            }
        }


        /// <summary>Enable/Disable All Colliders on the animal. Avoid the Triggers </summary>
        public virtual void EnableColliders(bool active)
        {
            if (!active)
            {
                colliders = GetComponentsInChildren<Collider>(false).ToList();      //Get all the Active colliders

                List<Collider> coll = new List<Collider>();

                foreach (var item in colliders)
                {
                    if (!item.isTrigger && item.enabled) coll.Add(item);        //Remove all disabled colliders and all triggers
                }
                colliders = coll;

                SendMessage("NoTarget", SendMessageOptions.DontRequireReceiver);  //Remove all Targets if it has by any change LookAt
            }


            foreach (Collider item in colliders)
            {
                item.enabled = active;
            }

            if (active) colliders = new List<Collider>();
        }

        /// <summary>Disable this Script and MalbersInput Script if it has it.</summary>
        public virtual void DisableAnimal()
        {
            enabled = false;
            MalbersInput MI = GetComponent<MalbersInput>();
            if (MI) MI.enabled = false;
        }
        #endregion
    }
}