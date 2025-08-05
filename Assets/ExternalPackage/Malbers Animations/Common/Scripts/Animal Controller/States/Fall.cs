using UnityEngine;
using MalbersAnimations.Utilities;
using MalbersAnimations.Scriptables;

namespace MalbersAnimations.Controller
{
    public class Fall : State
    {
        public enum FallBlending { None, Normalized, Distance, Direction }

        /// <summary>Air Resistance while falling</summary>
        [Header("Fall Parameters")]
        public BoolReference AirControl = new BoolReference(true);
        public FloatReference AirRotation = new FloatReference(10);
        //   public FloatReference AirRotationLerp = new FloatReference(5);
        [Space]
        //public float fallRayDownMultiplier;
        public FloatReference FallRayForwad = new FloatReference(0f);

        /// <summary>Multiplier to check if is falling in front of him</summary>
        [Tooltip("Multiplier for the Fall Ray Length")]
        public FloatReference fallRayMultiplier = new FloatReference(1f);


        /// <summary>Used to Set fallBlend to zero before reaching the ground</summary>
        [Space, Tooltip("Used to Set fallBlend to zero before reaching the ground")]
        public FloatReference LowerBlendDistance;


        //  public bool KeepInertia = false;


        protected Vector3 fall_Point;
        RaycastHit[] FallHits = new RaycastHit[1];
        //  protected Vector3 HorizontalInertia;
        private RaycastHit FallRayCast;

        /// <summary>this is to store the max Y heigth before falling</summary>
        private float MaxHeight;
        /// <summary>While Falling this is the distance to the ground</summary>
        private float DistanceToGround;

        /// <summary> Normalized Value of the Height </summary>
        float FallBlend;
        private Vector3 UpImpulse;
        private MSpeed FallSpeed = MSpeed.Default;

        public Vector3 FallPoint { get; private set; }

        private int fallHits;

        public override bool TryActivate()
        {
            float SprintMultiplier = (animal.CurrentSpeedModifier.Vertical);

            var fall_Pivot = animal.Main_Pivot_Point + (animal.Forward * SprintMultiplier * FallRayForwad * animal.ScaleFactor); //Calculate ahead the falling ray

            fall_Pivot += animal.DeltaPos; //Check for the Next Frame

            float Multiplier = animal.Pivot_Multiplier * fallRayMultiplier;

            return TryFallSphereCastNonAlloc(fall_Pivot, Multiplier);
            //  return  TryFallSphereCast(fall_Pivot, Multiplier);
        }


        private bool TryFallSphereCastNonAlloc(Vector3 fall_Pivot, float Multiplier)
        {
            fallHits = Physics.SphereCastNonAlloc(fall_Pivot, animal.RayCastRadius * animal.ScaleFactor, animal.GravityDirection, FallHits, Multiplier, animal.GroundLayer, QueryTriggerInteraction.Ignore);

            if (fallHits > 0)
            {
                FallRayCast = FallHits[0];

                DistanceToGround = FallRayCast.distance;

                if (debug)
                {
                    Debug.DrawRay(fall_Pivot, -transform.up * Multiplier, Color.magenta);
                    MalbersTools.DebugPlane(FallRayCast.point, animal.RayCastRadius * animal.ScaleFactor, Color.magenta, true);
                }

                if (!animal.Grounded)
                {
                    var TerrainSlope = Vector3.Angle(FallRayCast.normal, animal.UpVector);
                    var DeepSlope = TerrainSlope > animal.maxAngleSlope;

                    if (DeepSlope)
                    {
                        if (debug && animal.debugStates) Debug.Log("Try Fall State: The Animal is on Air but angle SLOPE of the ground found is too Deep");
                        return true;
                    }
                }
                else if (animal.Grounded && animal.DeepSlope)     //if wefound something but there's a deep slope
                {
                    if (debug && animal.debugStates) Debug.Log("Try Fall State: The Ground angle SLOPE is too Deep");
                    return true;
                }

                if (animal.Height >= DistanceToGround) //If the distance to ground is very small means that we are very close to the ground
                {
                    animal.Grounded = true;     //This Allow Locomotion and Idle to Try Activate themselves
                    return false;
                }
            }
            else
            {
                if (debug && animal.debugStates) Debug.Log("Try Fall State: There's no Ground beneath the Animal");
                return true;
            }

            return false;
        }


        public override void ExitState()
        {
            base.ExitState();
            MaxHeight = float.NegativeInfinity; //Resets MaxHeight
            DistanceToGround = float.PositiveInfinity;
            FallBlend = 1;
            animal.UpdateDirectionSpeed = true; //Reset the Rotate Direction
        }

        public override void Activate()
        {
            ResetValues();
            animal.SetFloatID(1f);

            FallBlend = 1;
            base.Activate();
        }

        public override void AnimationStateEnter()
        {
            if (CurrentAnimTag == AnimTag.FallEdge)
            {
                IgnoreLowerStates = true; 
            }
            else if (CurrentAnimTag == MainTagHash)
            {
                UpImpulse = MalbersTools.CleanUpVector(animal.DeltaPos, animal.Forward, animal.Right);   //Clean the Vector from Forward and Horizontal Influence    

                IgnoreLowerStates = false;

                FallSpeed = new MSpeed(animal.CurrentSpeedModifier)
                {
                    name = "FallSpeed",
                    position = animal.HorizontalSpeed,
                    animator = 1,
                    rotation = AirRotation.Value 
                };

                animal.UpdateDirectionSpeed = AirControl;
                animal.SetCustomSpeed(FallSpeed,true);
            }
        }

        public override void OnStateMove(float deltaTime)
        {
           if (CurrentAnimTag == MainTagHash)
            {
                animal.AdditivePosition += UpImpulse;
            }
        }

        //private void BlendFallDistance()
        //{
        //    if (BlendFall == FallBlending.Distance)
        //    {
        //        var CurrentUpImpulse = MalbersTools.CleanUpVector(animal.Inertia, animal.Forward, animal.Right);        //Remove Forward

        //        var UpSpeed = CurrentUpImpulse.magnitude * (Vector3.Dot(CurrentUpImpulse, animal.UpVector) < 0 ? 1f : -1f) / animal.GravityForce;

        //        FallBlend = UpSpeed;
        //        animal.SetFloatID(FallBlend); //Blend between High and Low Fall
        //    }
        //}

        public override void TryExitState(float DeltaTime)
        {
            FallPoint = animal.Main_Pivot_Point + animal.AdditivePosition;

            if (animal.debugGizmos && debug)
            {
                // MalbersTools.DebugTriangle(animal.Main_Pivot_Point, animal.RayCastRadius * animal.ScaleFactor, Color.magenta);
                MalbersTools.DebugTriangle(FallPoint, animal.RayCastRadius * animal.ScaleFactor, Color.magenta);
                Debug.DrawRay(FallPoint, animal.GravityDirection * 100f, Color.magenta);
            }

            //int hits = Physics.SphereCastNonAlloc(FallPoint, animal.RayCastRadius * animal.ScaleFactor,animal.GravityDirection, FallHits, 100f, animal.GroundLayer, QueryTriggerInteraction.Ignore);
            //if (hits > 0)
            if (Physics.SphereCast(FallPoint, animal.RayCastRadius, animal.GravityDirection, out FallRayCast, 100f, animal.GroundLayer, QueryTriggerInteraction.Ignore))
            {
                DistanceToGround = FallRayCast.distance - (/*animal.AdditivePosition.magnitude +*/ animal.DeltaPos.magnitude);

                if (animal.debugGizmos && debug)
                {
                    MalbersTools.DebugTriangle(FallRayCast.point, animal.RayCastRadius * animal.ScaleFactor, Color.magenta);
                }

                //  if (BlendFall == FallBlending.Normalized)
                {
                    if (MaxHeight < DistanceToGround) MaxHeight = DistanceToGround; //get the Highest Distance the first time you touch the ground

                    FallBlend = Mathf.Lerp(FallBlend, (DistanceToGround - LowerBlendDistance) / (MaxHeight - LowerBlendDistance), DeltaTime * 20); //Small blend in case there's a new ground found
                    animal.SetFloatID(FallBlend); //Blend between High and Low Fall
                }

                if (animal.Height >= DistanceToGround)
                {
                    var TerrainSlope = Vector3.Angle(FallRayCast.normal, animal.UpVector);
                    var DeepSlope = TerrainSlope > animal.maxAngleSlope;

                    if (!DeepSlope)
                    {
                        animal.AlingRayCasting(); //Check one time the Align Rays to calculate the Angle Slope used on the CanFallOnSlope
                        AllowExit();
                        animal.Grounded = true;             //This Allow Locomotion and Idle to Try Activate themselves
                        animal.UseGravity = false;
                    }
                }
            }
        }


        public override void StartState() { ResetValues(); }

        void ResetValues()
        {
            MaxHeight = float.NegativeInfinity; //Resets MaxHeight
            DistanceToGround = float.PositiveInfinity;
            FallSpeed = new MSpeed();
            FallBlend = 1;
            FallRayCast = new RaycastHit();
            UpImpulse = Vector3.zero;
        }

#if UNITY_EDITOR

        public override void DebugState()
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(FallPoint,animal.RayCastRadius);
            Gizmos.DrawWireSphere(animal.Main_Pivot_Point, animal.RayCastRadius);
        }

        /// <summary>This is Executed when the Asset is created for the first time </summary>
        public override void Reset()
        {
            ID = MalbersTools.GetInstance<StateID>("Fall");
            General = new AnimalModifier()
            {
                RootMotion = false,
                AdditivePosition = true,
                Grounded = false,
                Sprint = false,
                OrientToGround = false,
                Colliders = true,
                Gravity = true,
                CustomRotation = false,
                modify = (modifier)(-1),
            };

            LowerBlendDistance = 0.1f;
            FallRayForwad = 0.1f;
            fallRayMultiplier = 1f;

            FallSpeed.name = "FallSpeed";

            ExitFrame = false; //IMPORTANT
        }
#endif
    }
}