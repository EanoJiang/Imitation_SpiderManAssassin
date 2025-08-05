// Copyright © 2017-2024 Vault Break Studios Pty Ltd

using UnityEngine;
using System.Collections.Generic;

namespace MxM
{
    //============================================================================================
    /**
    *  @brief A trajectory point used for movement prediction in the Motion Matching animaiton 
    *  system
    *         
    *********************************************************************************************/
    [System.Serializable]
    public struct TrajectoryPoint
    {
        public Vector3 Position; //The predicted position point
        public float FacingAngle; //The predicted angle of the entity at this point

        //============================================================================================
        /**
        *  @brief Constructor for Trajectory point struct
        *         
        *********************************************************************************************/
        public TrajectoryPoint(Vector3 _position, float _facingAngle)
        {
            Position = _position;
            FacingAngle = _facingAngle;
        }

        //============================================================================================
        /**
        *  @brief 
        *         
        *********************************************************************************************/
        public static void Lerp(ref TrajectoryPoint a_from, ref TrajectoryPoint a_to, float a_step, out TrajectoryPoint a_result)
        {
            a_result.Position = Vector3.Lerp(a_from.Position, a_to.Position, a_step);
            a_result.FacingAngle = Mathf.LerpAngle(a_from.FacingAngle, a_to.FacingAngle, a_step);
        }
    }//End of class: TrajectoryPoint
}//End of namespace: MxM
