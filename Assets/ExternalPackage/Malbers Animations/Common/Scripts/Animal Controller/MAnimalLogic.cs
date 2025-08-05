using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MalbersAnimations.Utilities;
using UnityEngine.Events;
using MalbersAnimations.Events;
using System;
//using Physics = RotaryHeart.Lib.PhysicsExtension.Physics;

namespace MalbersAnimations.Controller
{
    //Animal Logic
    public partial class MAnimal
    {
        void Awake()
        {
            if (MainCamera == null)
            {
                MainCamera = MalbersTools.FindMainCamera().transform;
            }

            if (Anim ==null) Anim = GetComponentInParent<Animator>();            //Cache the Animator
            if (RB == null) RB = GetComponentInParent<Rigidbody>();             //Catche the Rigid Body  
            SpeedMultiplier = 1;
            GetHashIDs();
            OptionalAnimatorParameters();                                       //Enable Optional Animator Parameters on the Animator Controller;

            _transform = transform;         //Cache the Transform
            activeMode = null;

            foreach (var set in speedSets)
            {
                set.CurrentIndex = set.StartVerticalIndex;
            }

            if (RB)
            {
                RB.useGravity = false;
                RB.constraints = RigidbodyConstraints.FreezeRotation;
            }

            //DefaultAnimatorUpdate = Anim.updateMode;//Cache the Update Mode on the Animator to Physics or Normal

            statesD = new Dictionary<int, State>();
            modesD = new Dictionary<int, Mode>();

            for (int i = 0; i < states.Count; i++)
            {
                if (CloneStates)
                {
                    State instance = (State)ScriptableObject.CreateInstance(states[i].GetType());
                    instance = ScriptableObject.Instantiate(states[i]);                                 //Create a clone from the Original Scriptable Objects! IMPORTANT
                    instance.name = instance.name.Replace("(Clone)", "(C)");
                    states[i] = instance;
                }

                statesD.Add(states[i].ID.ID, states[i]);        //Convert it to a Dictionary

                states[i].SetAnimal(this);                      //Awake all States
                states[i].Priority = states.Count - i;
                states[i].AwakeState();
            }

            foreach (var mode in modes)
            {
                mode.AwakeMode(this);
                modesD.Add(mode.ID, mode);        //Convert it to a Dictionary
            }

            InputSource = GetComponentInChildren<IInputSource>();      //Cache the Input Source

            SetPivots();

            HitDirection = Vector3.zero; //Reset the Damage Direction;
        }

        private void SetPivots()
        {
            Pivot_Hip = pivots.Find(item => item.name.ToUpper() == "HIP");
            Pivot_Chest = pivots.Find(item => item.name.ToUpper() == "CHEST");

            Has_Pivot_Hip = Pivot_Hip != null;
            Has_Pivot_Chest = Pivot_Chest != null;
        }


        void Start()
        { SetStart(); }

        void OnEnable()
        {
            if (Animals == null) Animals = new List<MAnimal>();
            Animals.Add(this);          //Save the the Animal on the current List

            GetInputs(true);

            foreach (var state in states)  state.ResetState();

            if (isPlayer) SetMainPlayer();
        }

        void OnDisable()
        {
            if (Animals != null) Animals.Remove(this);       //Remove all this animal from the Overall AnimalList

            GetInputs(false);
            DisableMainPlayer();
        }  

        protected virtual void SetStart()
        {
            ScaleFactor = _transform.localScale.y;                                           //TOTALLY SCALABE animal
            MovementDetected = false;
            Grounded = true;
            UpdateDirectionSpeed = true;

            UseAdditivePos = true;
            Inertia = Vector3.zero;
            GravityStoredAceleration = 0;
            UseGravity = true;
            UseOrientToGround = true;
            UseCustomAlign = false;
            LastPos = _transform.position;
            AdditivePosition = Vector3.zero;
            InertiaPositionSpeed = Vector3.zero;
            activeMode = null;

            Attack_Triggers = GetComponentsInChildren<MAttackTrigger>(true).ToList();        //Save all Attack Triggers.

            foreach (var state in states) state.StartState();

            State StartState = OverrideStartState != null ? statesD[OverrideStartState] : states[states.Count - 1]; //Use override state or take the last one on the List

            activeState = StartState;                           //Set the var as Active state whitout calling the code               //Set the Last State as the Active State since is the first time
            StartState.Activate();
            AlingRayCasting();                                  //Make a first raycast

            Anim.speed = AnimatorSpeed;                         //Set the Global Animator Speed
        }

        #region Animator Stuff

        protected virtual void GetHashIDs()
        {
            hash_Vertical = Animator.StringToHash(m_Vertical);
            hash_Horizontal = Animator.StringToHash(m_Horizontal);
            hash_UpDown = Animator.StringToHash(m_UpDown);
            // hash_CameraSide = Animator.StringToHash(m_CameraSide);

            hash_Type = Animator.StringToHash(m_Type);
            hash_Slope = Animator.StringToHash(m_Slope);
            hash_SpeedMultiplier = Animator.StringToHash(m_SpeedMultiplier);

            hash_IDInt = Animator.StringToHash(m_IDInt);
            hash_IDFloat = Animator.StringToHash(m_IDFloat);

            hash_State = Animator.StringToHash(m_State);
            hash_LastState = Animator.StringToHash(m_LastState);
            hash_Mode = Animator.StringToHash(m_Mode);
            hash_Status = Animator.StringToHash(m_Status);
            hash_Stance = Animator.StringToHash(m_Stance);

            hash_StateTime = Animator.StringToHash(m_StateTime);
            hash_Movement = Animator.StringToHash(m_Movement);
            hash_DeltaAngle = Animator.StringToHash(m_DeltaAngle);
            hash_Grounded = Animator.StringToHash(m_Grounded);
        }

        /// <summary>Enable Optional Animator Parameters on the Animator Controller;</summary>
        protected virtual void OptionalAnimatorParameters()
        {
            hasUpDown = MalbersTools.FindAnimatorParameter(Anim, AnimatorControllerParameterType.Float, hash_UpDown);
            hasDeltaAngle = MalbersTools.FindAnimatorParameter(Anim, AnimatorControllerParameterType.Float, hash_DeltaAngle);
            hasSlope = MalbersTools.FindAnimatorParameter(Anim, AnimatorControllerParameterType.Float, hash_Slope);
            hasSpeedMultiplier = MalbersTools.FindAnimatorParameter(Anim, AnimatorControllerParameterType.Float, hash_SpeedMultiplier);
            hasStateTime = MalbersTools.FindAnimatorParameter(Anim, AnimatorControllerParameterType.Float, hash_StateTime);

            hasStance = MalbersTools.FindAnimatorParameter(Anim, AnimatorControllerParameterType.Int, hash_Stance);

            if (MalbersTools.FindAnimatorParameter(Anim, AnimatorControllerParameterType.Int, hash_Type)) //This is only done once!
            {
                Anim.SetInteger(hash_Type, animalType);
            }
        }

        protected virtual void CacheAnimatorState()
        {
            m_PreviousCurrentState = m_CurrentState;
            m_PreviousNextState = m_NextState;
            m_PreviousIsAnimatorTransitioning = m_IsAnimatorTransitioning;

            m_CurrentState = Anim.GetCurrentAnimatorStateInfo(0);
            m_NextState = Anim.GetNextAnimatorStateInfo(0);
            m_IsAnimatorTransitioning = Anim.IsInTransition(0);

            StateTime = Mathf.Repeat(m_CurrentState.normalizedTime, 1);

            if (m_IsAnimatorTransitioning)
            {
                if (m_NextState.fullPathHash != 0)
                {
                    AnimStateTag = m_NextState.tagHash;
                    AnimState = m_NextState;
                }
            }
            else
            {
                if (m_CurrentState.fullPathHash != AnimState.fullPathHash)
                {
                    AnimStateTag = m_CurrentState.tagHash;
                }

                AnimState = m_CurrentState;
            }
        }

        /// <summary>Link all Parameters to the animator</summary>
        protected virtual void UpdateAnimatorParameters()
        {
            Anim.SetFloat(hash_Vertical, MovementAxisSmoothed.z);
            Anim.SetFloat(hash_Horizontal, MovementAxisSmoothed.x);
            Anim.SetBool(hash_Movement, MovementDetected);


            if (hasUpDown) Anim.SetFloat(hash_UpDown, MovementAxisSmoothed.y);
            //  if (hasCameraSide) Anim.SetFloat(hash_CameraSide,CameraAngle);

            if (hasDeltaAngle) Anim.SetFloat(hash_DeltaAngle, DeltaAngle);
            if (hasSlope) Anim.SetFloat(hash_Slope, SlopeNormalized);
            if (hasSpeedMultiplier) Anim.SetFloat(hash_SpeedMultiplier, SpeedMultiplier);

            if (hasStateTime) Anim.SetFloat(hash_StateTime, StateTime);
        }

        #endregion


        #region Input Entering for Moving

        /// <summary>Get the Raw Input Axis from a source</summary>
        public virtual void SetInputAxis(Vector3 inputAxis)
        {
            if (UseCameraInput && MainCamera)
            {
                var Cam_Forward = Vector3.ProjectOnPlane(MainCamera.forward, UpVector).normalized; //Normalize the Camera Forward Depending the Up Vector IMPORTANT!
                var Cam_Right = Vector3.ProjectOnPlane(MainCamera.right, UpVector).normalized;
                // var Cam_Up = Vector3.ProjectOnPlane(mainCamera.up, Forward).normalized;

                var UpInput = (inputAxis.y * MainCamera.up);

                if (UseCameraUp)
                {
                    UpInput += Vector3.Project(MainCamera.forward, UpVector) * inputAxis.z;
                }
                if (Grounded)
                {
                    UpInput = Vector3.zero; //Reset the UP Input in case is on the Ground
                }

                var m_Move = (inputAxis.z * Cam_Forward) + (inputAxis.x * Cam_Right) + UpInput;
               
               MoveDirection(m_Move);
            }
            else
            {
                MoveWorld(inputAxis);
            }
        }

        public virtual void SetInputAxis(Vector2 move)
        {
            Vector3 move3 = new Vector3(move.x, 0, move.y);

            SetInputAxis(move3);
        }

        /// <summary>Gets the movement from the World Coordinates</summary>
        /// <param name="move">Direction Vector</param>
        public virtual void MoveWorld(Vector3 move)
        {
            if (LockMovement)
            {
                MovementAxis = Vector3.zero;
                return;
            }

            MoveWithDirection = false;
            MovementDetected = move.x != 0 || move.z != 0 || move.y != 0;

            if (!SmoothVertical && move.z > 0)                       //It will remove slowing Stick push when rotating and going Forward
            {
                move.z = 1;
            }

            MovementAxis = move;

            TargetMoveDirection = _transform.TransformDirection(move).normalized; //Convert from world to relative

            if (debugGizmos) Debug.DrawRay(_transform.position, TargetMoveDirection, Color.red);
        }


        /// <summary>Gets the movement from a Direction</summary>
        /// <param name="move">Direction Vector</param>
        public virtual void MoveDirection(Vector3 move)
        {
            if (LockMovement)
            {
                MovementAxis = Vector3.zero;
                return;
            }

            MoveWithDirection = true;

            MovementDetected = move.x != 0 || move.z != 0 || move.y != 0;


            if (move.magnitude > 1f) move.Normalize();

            if (Grounded)
            {
                move = Quaternion.FromToRotation(UpVector, SurfaceNormal) * move; //Rotate with the ground Surface Normal
            }

            TargetMoveDirection = move;

            DeltaAngle = 0;


            if (MovementDetected)
            {
                // Find the difference between the current rotation of the player and the desired rotation of the player in radians.
                float angleCurrent = Mathf.Atan2(Forward.x, Forward.z) * Mathf.Rad2Deg;
                float targetAngle = Mathf.Atan2(move.x, move.z) * Mathf.Rad2Deg;
                DeltaAngle = Mathf.DeltaAngle(angleCurrent, targetAngle);
            }

            move = _transform.InverseTransformDirection(move);               //Convert the move Input from world to Local

            float turnAmount = Mathf.Atan2(move.x, move.z);                 //Convert it to Radians
            float forwardAmount = Mathf.Abs(move.z);

            if (!SmoothVertical && forwardAmount > 0)                       //It will remove slowing Stick push when rotating and going Forward
            {
                forwardAmount = 1;             
            }
          

            MovementAxis = new Vector3(turnAmount, move.y, forwardAmount);

            if (debugGizmos) Debug.DrawRay(_transform.position, TargetMoveDirection, Color.red);
        }
        #endregion

        /// <summary>Add more Rotations to the current Turn Animations  </summary>
        protected virtual void AdditionalTurn(float time)
        {

            float SpeedRotation = CurrentSpeedModifier.rotation;

            if (MovementAxisSmoothed.z < 0.01 && !CustomSpeed)
            {
                SpeedRotation = CurrentSpeedSet.Speeds[0].rotation;
            }

            if (SpeedRotation < 0) return;          //Do nothing if the rotation is lower than 0

            if (MovementDetected)
            {
                float ModeRotation = 1;
                if (IsPlayingMode && !ActiveMode.AllowRotation) ModeRotation = 0; //If the mode does not allow rotation set the multiplier to zero

                if (MoveWithDirection)
                {
                    Quaternion targetRotation = Quaternion.Slerp(Quaternion.identity, Quaternion.Euler(0, DeltaAngle, 0),
                        time * (SpeedRotation + 1) / 4 *  ( (TurnMultiplier + 1) * ModeRotation ));

                    AdditiveRotation *= targetRotation;
                }
                else
                {
                    float Turn = SpeedRotation * 10;           //Add Extra Multiplier

                    float TurnInput = Mathf.Clamp(Smooth_Horizontal, -1, 1) * (MovementAxis.z >= 0 ? 1 : -1);  //Add +Rotation when going Forward and -Rotation when going backwards

                    AdditiveRotation *= Quaternion.Euler(0, Turn * TurnInput * time * ModeRotation, 0);

                    var TargetGlobal = Quaternion.Euler(0, TurnInput * (TurnMultiplier + 1), 0);
                    var AdditiveGlobal = Quaternion.Slerp(Quaternion.identity, TargetGlobal, time * (SpeedRotation + 1) * ModeRotation);
                    AdditiveRotation *= AdditiveGlobal;
                }
            }
        }

        /// <summary> Add more Speed to the current Move animations</summary> ???????????????????????????????????????
        protected virtual void AdditionalSpeed(float time)
        {
            //if (CurrentSpeedModifier.position <= 0)
            //{
            //    InertiaPositionSpeed = Vector3.zero;
            //    return;      //Means there's no additional speed
            //}


            InertiaPositionSpeed = (CurrentSpeedModifier.lerpPosition > 0) 
                ? Vector3.Lerp(InertiaPositionSpeed, TargetSpeed, time * CurrentSpeedModifier.lerpPosition) 
                : TargetSpeed;

            AdditivePosition += InertiaPositionSpeed;

           // Debug.DrawRay(transform.position, InertiaPositionSpeed.normalized*5, Color.yellow);
        }

        protected void PlatformMovement()
        {
            if (platform == null) return;

            if (!Grounded) return;         //Do not calculate if you are not on Locomotion or Idle

            var DeltaPlatformPos = platform.position - platform_Pos;
            DeltaPlatformPos.y = 0;                                                                                       //the Y is handled by the Fix Position

            AdditivePosition += DeltaPlatformPos;                          // Keep the same relative position.

            Quaternion Inverse_Rot = Quaternion.Inverse(platform_Rot);
            Quaternion Delta = Inverse_Rot * platform.rotation;

            if (Delta != Quaternion.identity)                                        // no rotation founded.. Skip the code below
            {
                var pos = _transform.DeltaPositionFromRotate(platform, Delta);
                AdditivePosition += pos;
            }

            AdditiveRotation *= Delta;

            platform_Pos = platform.position;
            platform_Rot = platform.rotation;
        }

        /// <summary>Raycasting stuff to align and calculate the ground from the animal ****IMPORTANT***</summary>
        internal virtual void AlingRayCasting()
        {
            Height = (height - RayCastRadius) * ScaleFactor;            //multiply the Height by the scale
            //MainRay = FrontRay = false;

            hit_Chest = new RaycastHit();                               //Clean the Raycasts every time 
            hit_Hip = new RaycastHit();                                 //Clean the Raycasts every time 

            hit_Chest.distance = hit_Hip.distance = Height;            //Reset the Distances to the Heigth of the animal

            if (Has_Pivot_Hip) //Ray From the Hip to the ground
            {
                var hipPoint = Pivot_Hip.World(_transform) + AdditivePosition;

                // if (Physics.Raycast(Pivot_Hip.World(transform), -_transform.up, out hit_Hip, ScaleFactor * Pivot_Hip.multiplier, GroundLayer, QueryTriggerInteraction.Ignore))
                if (Physics.SphereCast(hipPoint, RayCastRadius * ScaleFactor, -_transform.up, out hit_Hip, ScaleFactor * Pivot_Hip.multiplier, GroundLayer, QueryTriggerInteraction.Ignore))
                {
                    MainRay = true;

                    if (platform == null || platform != hit_Hip.transform)               //Platforming logic
                    {
                        platform = hit_Hip.transform;
                        platform_Pos = platform.position;
                        platform_Rot = platform.rotation;
                    }
                }
                else
                {
                    platform = null;
                    MainRay = false;
                }
            }

            if (Physics.SphereCast(Main_Pivot_Point, RayCastRadius * ScaleFactor, -_transform.up, out hit_Chest,  Pivot_Multiplier, GroundLayer, QueryTriggerInteraction.Ignore))
            //if (Physics.Raycast(Main_Pivot_Point, -_transform.up, out hit_Chest, Pivot_Multiplier, GroundLayer, QueryTriggerInteraction.Ignore))
            {
                FrontRay = true;

                if (platform == null || platform != hit_Chest.transform)               //Platforming logic
                {
                    platform = hit_Chest.transform;
                    platform_Pos = platform.position;
                    platform_Rot = platform.rotation;
                }
            }
            else
            {
                platform = null;
                FrontRay = false;
            }



            if (!Has_Pivot_Hip || !MainRay)
            {
                MainRay = FrontRay;    //In case there's no Hip Ray
                hit_Hip = hit_Chest;
            }


            if (ground_Changes_Gravity) GravityDirection  = -hit_Hip.normal;

            CalculateSurfaceNormal();


            // else if (!Has_Pivot_Chest) FrontRay = MainRay;    //In case there's no frontRay
        }

        internal virtual void CalculateSurfaceNormal()
        {
            if (Has_Pivot_Hip)
            {
                Vector3 TerrainNormal;

                if (Has_Pivot_Chest)
                {
                    Vector3 direction = (hit_Chest.point - hit_Hip.point).normalized;
                    Vector3 Side = Vector3.Cross(UpVector, direction).normalized;
                    SurfaceNormal = Vector3.Cross(direction, Side).normalized;


                    //TerrainNormal = (hit_Chest.normal + hit_Hip.normal).normalized;
                    TerrainNormal = SurfaceNormal;
                    //SurfaceNormal =  (hit_Chest.normal + hit_Hip.normal).normalized;
                }
                else
                {
                    SurfaceNormal =  TerrainNormal = hit_Hip.normal;
                }
                TerrainSlope = Vector3.Angle(TerrainNormal, UpVector);
                TerrainSlope *= Vector3.Dot(Forward_no_Y, TerrainNormal) > 0 ? -1 : 1;            //Calcualte the Fall Angle Positive or Negative
            }
            else
            {
                TerrainSlope = Vector3.Angle(hit_Hip.normal, UpVector);
                TerrainSlope *= Vector3.Dot(Forward_no_Y, hit_Hip.normal) > 0 ? -1 : 1;            //Calcualte the Fall Angle Positive or Negative
            }


         
        }

        /// <summary>Align the Animal to Terrain</summary>
        /// <param name="align">True: Aling to UP, False Align to Terrain</param>
        internal virtual void AlignRotation(bool align, float time, float smoothness)
        {
            if (align)
            {
                AlignRotation(SurfaceNormal, time, smoothness);
            }
            else
            {
                AlignRotation(UpVector, time, smoothness);
            }
        }

        /// <summary>Align the Animal to a Custom </summary>
        /// <param name="align">True: Aling to UP, False Align to Terrain</param>
        internal virtual void AlignRotation(Vector3 alignNormal, float time, float smoothness)
        {
            Quaternion AlignRot = Quaternion.FromToRotation(_transform.up, alignNormal) * _transform.rotation;  //Calculate the orientation to Terrain 
            Quaternion Inverse_Rot = Quaternion.Inverse(_transform.rotation);
            Quaternion Target = Inverse_Rot * AlignRot;
            Quaternion Delta = Quaternion.Lerp(Quaternion.identity, Target, time * smoothness ); //Calculate the Delta Align Rotation

            // _transform.rotation = Quaternion.Lerp(_transform.rotation, AlignRot, time * smoothness);

             _transform.rotation *= Delta;
            //AdditiveRotation *= Delta;
        }

        /// <summary>Snap to Ground with Smoothing</summary>
        internal void AlignPosition(float time)
        {
            AlignPosition(hit_Hip.distance, time,  AlignPosLerp * 2);
        }

        internal void AlignPosition(float distance, float time, float Smoothness)
        {
            float difference = Height - distance;

            if (!Mathf.Approximately(distance, Height))
            {
                Vector3 align = _transform.rotation * new Vector3(0, difference * time * Smoothness, 0); //Rotates with the Transform to better alignment
               // _transform.position += align;
                AdditivePosition += align;
            }
        }

        /// <summary>Snap to Ground with no Smoothing</summary>
        internal virtual void AlignPosition()
        {
            float difference = Height - hit_Hip.distance;

            AdditivePosition = _transform.rotation * new Vector3(0, difference, 0); //Rotates with the Transform to better alignment
        }


        /// <summary> Movement Trot Walk Run (Velocity changes)</summary>
        protected void MovementSystem(float DeltaTime)
        {
            float maxspeedV = CurrentSpeedModifier.Vertical;
            float maxspeedH = 1;
            var LerpVertical = DeltaTime * CurrentSpeedModifier.lerpPosition;
            var LerpTurn = DeltaTime * CurrentSpeedModifier.lerpRotation;
            var LerpAnimator = DeltaTime * CurrentSpeedModifier.lerpAnimator;

            if (Stance == 5) maxspeedH = maxspeedV; //if the animal is strafing

            VerticalSmooth = LerpVertical > 0 ? Mathf.MoveTowards(VerticalSmooth, MovementAxis.z * maxspeedV, LerpVertical) : MovementAxis.z * maxspeedV;  //smoothly transitions bettwen Speeds
            Smooth_Horizontal = LerpTurn > 0 ? Mathf.MoveTowards(Smooth_Horizontal, MovementAxis.x * maxspeedH, LerpTurn) : MovementAxis.x * maxspeedH;    //smoothly transitions bettwen Directions
            Smooth_UpDown = LerpVertical > 0 ? Mathf.MoveTowards(Smooth_UpDown, MovementAxis.y, LerpVertical) : MovementAxis.y;                            //smoothly transitions bettwen Directions

            if (Mathf.Abs(VerticalSmooth) < 0.001f) VerticalSmooth = 0;     //Clean Lower Values
            if (Mathf.Abs(Smooth_Horizontal) < 0.001f) Smooth_Horizontal = 0;
            if (Mathf.Abs(Smooth_UpDown) < 0.001f) Smooth_UpDown = 0;

            SpeedMultiplier = (LerpAnimator > 0) ? Mathf.MoveTowards(SpeedMultiplier, CurrentSpeedModifier.animator.Value, LerpAnimator) : CurrentSpeedModifier.animator.Value;               //Changue the velocity of the animator
        }

        protected void TryActivateState()
        {
         //  if (m_IsAnimatorTransitioning) return;
            if (ActiveState.IsPersistent) return; //If the State cannot be interrupted the ingored trying activating any other States


            foreach (var state in states)
            {
                if (state.ActiveState)
                {
                    if (state.IsPending) return;            //Do not try to activate any other state if  The Active State is Pending 

                    if (state.IgnoreLowerStates) return;    //If the Active State cannot exit ignore lower priority States

                    continue;                               //Ignore Try Activating yourSelf
                }


                if (StateQueued != null && state.ID == StateQueued.ID) continue;    //if the State on the list is on Queue Continue

                if (state.Active && !state.Sleep && state.TryActivate())
                {
                    // if (state.StateAnimationTags(AnimStateTag)) return;          //The Last State has not already exit the Animation State so do not Activate it

                    if (StateQueued != null && !StateQueued.QueueFrom.Contains(state.ID))
                    {
                        StateQueued.ActivateQueued();
                        StateQueued = null;
                        break;
                    }

                    state.Activate();
                    break;
                }
            }
        }

        /// <summary>Calculates the Pitch direction to Appy to the Rotator Transform</summary>
       
        private void CalculatePitchDirectionVector()
        {
            var UpDown = Mathf.Clamp(Smooth_UpDown, -1, 1); 
            var Vertical = Mathf.Clamp(VerticalSmooth, -1, 1);

            if (MoveWithDirection)                         //If the Animal is using Directional Movement use the Raw Direction Vector
            {
                PitchDirection = TargetMoveDirection;
                PitchDirection.Normalize();

                PitchDirection += (UpVector * UpDown);
                if (PitchDirection.magnitude > 1) PitchDirection.Normalize();                          //Remove extra Speed
            }
            else                                                                                         //If not is using Directional Movement Calculate New Direction Vector
            {
                if (MovementAxis.z < 0) UpDown = 0;                                                      //Remove UP DOWN MOVEMENT while going backwards
                PitchDirection = (transform.forward * Vertical) + (transform.up * UpDown);              //Calculate the Direction to Move
                if (PitchDirection.magnitude > 1) PitchDirection.Normalize();                          //Remove extra Speed
            }

           // if (debug) Debug.DrawRay(transform.position, PitchDirection, Color.yellow);
        }

        void OnAnimatorMove()
        {
            if (ActiveState == null) return;

            ScaleFactor = _transform.localScale.y;                      //Keep Updating the Scale Every Frame

            CacheAnimatorState();

            bool AnimatePhysics = Anim.updateMode == AnimatorUpdateMode.AnimatePhysics;

            DeltaTime = AnimatePhysics ? Time.fixedDeltaTime : Time.deltaTime;

            if (UpdateDirectionSpeed)
            {
                DirectionalSpeed = FreeMovement ? PitchDirection : _transform.forward; //Calculate the Direction Speed for the Additive Speed Position Direction
            }

            DeltaPos = _transform.position - LastPos;                    //DeltaPosition from the last frame

            ResetAdditiveValues();


            if (DeltaTime > 0 && DeltaPos != Vector3.zero)
            {
                Inertia = DeltaPos / DeltaTime;

                HorizontalSpeed = Vector3.ProjectOnPlane(Inertia, -GravityDirection).magnitude / ScaleFactor;
            }
            MainRay = FrontRay = false;

            ActiveState.OnStateMove(DeltaTime);                                                     //UPDATE THE STATE BEHAVIOUR

            MovementSystem(DeltaTime);

            CalculatePitchDirectionVector();

            AdditionalSpeed(DeltaTime);
            AdditionalTurn(DeltaTime);

            if (!m_IsAnimatorTransitioning)
            {
                if (ActiveState.MainTagHash == AnimStateTag) ActiveState.TryExitState(DeltaTime);     //if is not in transition and is in the Main Tag try to Exit to lower States

                TryActivateState();
            }


            if (JustActivateState)
            {
              if (LastState.ExitFrame)
                    LastState.OnStateMove(DeltaTime);           //Play One Last Time the Last State
                JustActivateState = false;
            }

            GravityLogic();
         
            if (InputMode != null)  InputMode.TryActivate(); //FOR MODES 
           
            if (Grounded)
            {
                AlingRayCasting();
                AlignPosition(DeltaTime);

                if (!UseCustomAlign)
                    AlignRotation(UseOrientToGround, DeltaTime, AlignRotLerp);

                PlatformMovement();
            }
            else
            {
                if (!UseCustomAlign)
                    AlignRotation(false, DeltaTime, AlignRotLerp); //Align to the Gravity Normal
                TerrainSlope = 0;
            }

            if (!FreeMovement && Rotator != null)
            {
                Rotator.localRotation = Quaternion.Lerp(Rotator.localRotation, Quaternion.identity, DeltaTime * (AlignPosLerp / 2)); //Improve this!!
                PitchAngle = Mathf.Lerp(PitchAngle, 0, DeltaTime * (AlignPosLerp / 2));
                Bank = Mathf.Lerp(Bank, 0, DeltaTime * (AlignPosLerp / 2));
            }

            LastPos = _transform.position;
           

            if (!DisablePositionRotation)
            {
                if (AnimatePhysics && RB)
                {
                    RB.velocity = Vector3.zero;
                    RB.angularVelocity = Vector3.zero;
                    if (DeltaTime > 0)
                        RB.velocity = AdditivePosition / DeltaTime;
                }
                else
                {
                    _transform.position += AdditivePosition;
                }

                _transform.rotation *= AdditiveRotation;
            }

            UpdateAnimatorParameters();              //Set all Animator Parameters

          // if (debugGizmos) Debug.DrawRay(transform.position, Inertia, Color.yellow);
        }

        private void GravityLogic()
        {
            if (UseGravity)
            {
                GravityStoredVelocity = GravityDirection * GravityForce * DeltaTime * GravityStoredAceleration;

                AdditivePosition += GravityStoredVelocity;                                  //Add Gravity if is in use
                GravityStoredAceleration += GravityForce * DeltaTime * GravityMultiplier;
            }
        }


        /// <summary> Smooth Value between Vertical and Forward Used for Flryn</summary>
        public float SmoothZY
        {
            get
            {
                if (!UpdateDirectionSpeed) return 1;

                var SmoothZY = Mathf.Clamp(  Mathf.Max(Mathf.Abs(Smooth_UpDown), Mathf.Abs(VerticalSmooth)),0,1);
               // if (VerticalSmooth < 0.01) SmoothZY *= -1;

                return SmoothZY;
            }
        }

        public virtual void FreeMovementRotator(float Ylimit, float bank, float deltatime)
        {
            Rotator.localEulerAngles = Vector3.zero;

            float NewAngle = 0;
            if (PitchDirection.magnitude > 0.001)                                                          //Rotation PITCH
            {
                NewAngle = 90 - Vector3.Angle(UpVector, PitchDirection);
                NewAngle = Mathf.Clamp(-NewAngle, -Ylimit, Ylimit);
            }

            PitchAngle = Mathf.Lerp(PitchAngle, NewAngle * SmoothZY, deltatime * UpDownLerp);

           

            Bank = Mathf.Lerp(Bank, -bank * Mathf.Clamp(Smooth_Horizontal, -1, 1) , deltatime * 5);


            var PitchRot = new Vector3(PitchAngle, 0, Bank);

            Rotator.localEulerAngles = PitchRot;
        }


        /// <summary> Resets Additive Rotation and Additive Position to their default</summary>
        void ResetAdditiveValues()
        {
            AdditivePosition = RootMotion ? Anim.deltaPosition : Vector3.zero;
            AdditiveRotation = RootMotion ? Anim.deltaRotation : Quaternion.identity;

            SurfaceNormal = UpVector;
            // TerrainSlope = 0;
        }
    }
}