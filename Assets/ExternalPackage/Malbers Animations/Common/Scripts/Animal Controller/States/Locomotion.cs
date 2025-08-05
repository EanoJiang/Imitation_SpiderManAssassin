using MalbersAnimations.Utilities;
using UnityEngine;

namespace MalbersAnimations.Controller
{
    /// <summary>This will be in charge of the Movement While is on the Ground </summary>
    public class Locomotion : State
    {
        [Header("Locomotion Parameters"),Tooltip("Set this parameter to true if there's no Idle State")]
        public bool IsIdle = false;

        /// <summary>This try to enable the Locomotion Logic</summary>
        public override bool TryActivate()
        {
            if (animal.Grounded)
            {
                if (animal.TerrainSlope > animal.maxAngleSlope) return false;

                if (IsIdle) return true; //Return true if is grounded

                var move = animal.MovementAxisSmoothed;
                if ((Mathf.Abs(move.x) > 0.01f || Mathf.Abs(move.z) > 0.01f)) //If is moving? 
                //if (animal.MovementDetected)
                {
                    return true;
                }
            }
            return false;
        }

        public override void OnStateMove(float deltatime)
        {
            if (animal.TerrainSlope > animal.maxAngleSlope)  animal.MovementAxis.z = 0;
        }


#if UNITY_EDITOR
        public override void Reset()
        {
            ID = MalbersTools.GetInstance<StateID>("Locomotion");

            General = new AnimalModifier()
            {
                RootMotion = true,
                Grounded = true,
                Sprint = true,
                OrientToGround = true,
                CustomRotation = false,
                IgnoreLowerStates = false,
                Colliders = true,
                AdditivePosition = true,
                //AdditiveRotation = true,
                Gravity = false,
                modify = (modifier)(-1),
            };
        }
#endif
    }
}