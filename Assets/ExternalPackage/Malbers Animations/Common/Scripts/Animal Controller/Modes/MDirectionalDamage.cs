using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MalbersAnimations.Controller
{
    [CreateAssetMenu(menuName = "Malbers Animations/Scriptables/Mode Modifier/Directional Damage")]
    public class MDirectionalDamage : ModeModifier
    {
        public bool TwoSided = false;
        public override void OnModeEnter(Mode mode)
        {
            MAnimal animal = mode.Animal;

            Vector3 HitDirection = animal.HitDirection;

            if (HitDirection == Vector3.zero) //Set it to random if there's no hit direction
            {
                mode.AbilityIndex = -1;
                return;
            }

            HitDirection = Vector3.ProjectOnPlane(HitDirection, animal.UpVector);     //Remove the Y on the Direction

            float angle = Vector3.Angle(animal.Forward, HitDirection);                           //Get The angle


            bool left = Vector3.Dot(animal.Right, HitDirection) < 0;            //Calculate which directions comes the hit Left or right
                                                                         //Debug.Log(angle  * (left ? 1:-1));

            int Side = -1;

            if (TwoSided)
            {
                mode.AbilityIndex = left ? 1 : 2;
                return;
            }

            if (left)
            {
                if (angle > 0 && angle <= 60) Side = 1;
                else if (angle > 60 && angle <= 120) Side = 2;
                else if (angle > 120 && angle <= 180) Side = 3;
            }
            else
            {
                if (angle > 0 && angle <= 60) Side = 4;
                else if (angle > 60 && angle <= 120) Side = 5;
                else if (angle > 120 && angle <= 180) Side = 6;
            }

            mode.AbilityIndex = Side;
        }
    }
}