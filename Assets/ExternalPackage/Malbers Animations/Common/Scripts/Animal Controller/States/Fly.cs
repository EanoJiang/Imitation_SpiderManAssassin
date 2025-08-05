using MalbersAnimations.Scriptables;
using MalbersAnimations.Utilities;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MalbersAnimations.Controller
{
    public class Fly : State
    {
        public enum FlyInput { Toggle, Press,}

        [Header("Fly Parameters")]
        [Range(0, 90)]
        public float Bank = 30;
        [Range(0, 90), Tooltip("Limit to go Up and Down")]
        public float Ylimit = 80;
        [Space]

        public FlyInput flyInput = FlyInput.Toggle;

        [Tooltip("When the Animal is close to the Ground it will automatically Land")]
        public BoolReference canLand = new BoolReference( true);
        [Tooltip("Ray Length multiplier to check for ground and automatically land (increases or decreases the MainPivot Lenght for the Fall Ray")]
        public FloatReference LandMultiplier = new FloatReference(1f);
        
        
        #region Check for Water
        [Tooltip("If there's water then Check landing on water")]
        public bool CheckforWater = false;
        protected Swim SwimState;
        #endregion


        //[Tooltip("Uses the Rotator on the Animal to Apply Rotations. If the Animations Rotates  the Animal. Disable this")]
        //public BoolReference UsePitchRotation = new BoolReference(true);

        public FloatReference InertiaTime = new FloatReference(1);

        [Header("Gliding")]
        public BoolReference GlideOnly = new BoolReference(false);
        public BoolReference AutoGlide = new BoolReference(true);
        [MinMaxRange(0, 10)]
        public RangedFloat GlideChance = new RangedFloat(0.8f, 4);
        [MinMaxRange(0, 10)]
        public RangedFloat FlapChange = new RangedFloat(0.5f, 4);

        protected bool isGliding = false;
        protected float FlyStyleTime = 1;


        protected float currentTime = 1;
        RaycastHit[] LandHit = new RaycastHit[1];

        [Header("Down Acceleration")]
        public FloatReference GravityDrag = new FloatReference(0);
        public FloatReference DownAcceleration = new FloatReference(1);
        private float acceleration = 0;
 
        protected Vector3 verticalInertia;
      
        public override void StatebyInput()
        {
            if (InputValue && !ActiveState)  //Enable fly if is not already active
            {
                if (flyInput == FlyInput.Toggle)
                {
                    InputValue = false; //Reset the Input to false if is set to toggle
                }

                Activate();
            }
            else if (InputValue && ActiveState && (flyInput == FlyInput.Toggle))
            {
                AllowExit();
                InputValue = false; 
            }
        }

        public override bool TryActivate()
        {
            return InputValue;
        }


        public override void StartState()
        {
            LandHit = new RaycastHit[1];
            currentTime = Time.time;
            FlyStyleTime = GlideChance.RandomValue;


            SwimState = (Swim)animal.GetState(4); //Get the Store the Swim State

            if (CheckforWater && (SwimState == null))
            {
                Debug.LogError("No Swim State Found.. please add the Swim State");
            }
        } 

        public override void AnimationStateEnter()
        {
            if (MainTagHash == CurrentAnimTag)
            {
                verticalInertia = MalbersTools.CleanUpVector(animal.DeltaPos, animal.Forward, animal.Right);
                acceleration = 0;
                animal.LastState = this; //IMPORTANT for Modes that changes the Last state enter

                animal.InertiaPositionSpeed = Vector3.ProjectOnPlane(animal.DeltaPos, animal.Up); //Keep the Speed from the take off
            }
        }

        public override void OnStateMove(float deltatime)
        {
            if (MainTagHash == CurrentAnimTag) //While is flying
            {
                if (InertiaTime > 0) animal.AddInertia(ref verticalInertia, deltatime * InertiaTime);
    

               if (General.FreeMovement)
                    animal.FreeMovementRotator(Ylimit, Bank, deltatime); 


                if (GlideOnly)
                {
                    if (animal.Smooth_UpDown > 0) animal.Smooth_UpDown = 0;
                    animal.currentSpeedModifier.Vertical = 2;
                    animal.UseSprintState = false;
                }
                else if (AutoGlide) AutoGliding();

                GravityAceleration(deltatime);
            }
        }


        void GravityAceleration(float deltaTime)
        {
            var Gravity = animal.GravityDirection;
            //Add more speed when going Down
            float downAcceleration = DownAcceleration * animal.ScaleFactor;

            if (animal.Smooth_UpDown < -0.001f)
            {
                animal.AdditivePosition += (Gravity * downAcceleration * deltaTime * acceleration);      //Add Gravity if is in use
                acceleration += downAcceleration * deltaTime;

                acceleration = Mathf.Lerp(acceleration, acceleration + downAcceleration, deltaTime);
            }
            else
            {
                acceleration = Mathf.MoveTowards(acceleration, 0, deltaTime * 2);                  //Deacelerate slowly all the acceleration you earned..
            }

            animal.AdditivePosition += animal.GravityDirection * acceleration * deltaTime;
            animal.AdditivePosition += transform.forward * acceleration * deltaTime;

            if (GravityDrag > 0)
            {
                animal.AdditivePosition += animal.GravityDirection * (GravityDrag * animal.ScaleFactor / 2) * deltaTime;
            }
        }

        void AutoGliding()
        {
            if (Time.time - FlyStyleTime >= currentTime)
            {
                currentTime = Time.time;
                isGliding = !isGliding;

                FlyStyleTime = isGliding ? GlideChance.RandomValue : FlapChange.RandomValue;

                animal.currentSpeedModifier.Vertical = isGliding ? 2 : Random.Range(1f, 1.5f);
            }
        }



        public override void ExitState()
        {
            base.ExitState();
            verticalInertia = Vector3.zero;
            animal.FreeMovement = false;
            acceleration = 0;
            isGliding = false;
        }


        public override void TryExitState(float DeltaTime)
        {
            if (CheckforWater)
            {
                SwimState.CheckWater();
                if (SwimState.IsInWater)
                {
                    AllowExit();  //Let the other states to awake (Water)
                    return;
                }
            }

            if (canLand)
            {
                var MainPivot = animal.Main_Pivot_Point + animal.AdditivePosition;

                if (debug) Debug.DrawRay(MainPivot, animal.GravityDirection * animal.Height * LandMultiplier, Color.yellow);

                int Hit = Physics.RaycastNonAlloc(MainPivot, animal.GravityDirection, LandHit, animal.Height * LandMultiplier, animal.GroundLayer, QueryTriggerInteraction.Ignore);

                if (Hit > 0)
                {
                    if (LandHit[0].distance < animal.Height) animal.Grounded = true; //Means the animal is touching the ground

                    AllowExit();  //Let the other states to awake (Ground)
                    return;
                }
            }

            switch (flyInput)
            {
                case FlyInput.Toggle:
                    if (ActiveState && InputValue)
                    {
                        AllowExit();
                        InputValue = false;
                    }
                    break;
                case FlyInput.Press:
                    if (ActiveState && !InputValue)
                    {
                        AllowExit();
                    }
                    break;
                default:
                    break;
            }
        }


#if UNITY_EDITOR
        public override void Reset()
        {
            ID = Resources.Load<StateID>("StatesID/Fly");
            Input = "Fly";

            General = new AnimalModifier()
            {
                RootMotion = true,
                Grounded = false,
                Sprint = true,
                OrientToGround = false,
                CustomRotation = false,
                IgnoreLowerStates = true,
                Gravity = false,
                modify = (modifier)(-1),
                AdditivePosition = true,
                Colliders = true,
                //KeepInertia = true
            };
        }
#endif
    }
}
