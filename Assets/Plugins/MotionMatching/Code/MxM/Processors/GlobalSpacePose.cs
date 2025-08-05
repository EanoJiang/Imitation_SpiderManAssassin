// Copyright © 2017-2024 Vault Break Studios Pty Ltd

using System.Collections.Generic;
using UnityEngine;

namespace MxMEditor
{
    //============================================================================================
    /**
    *  @brief 
    *         
    *********************************************************************************************/
    public class GlobalSpacePose
    {
        public Vector3 Position;
        public Quaternion Rotation;
        public Vector3 Forward;
        public List<Vector3> JointPositions = new List<Vector3>();

        public int ClipId;
        public float Time;

        public bool TrajectoryOnly;

    }//End of class: GlobalSpacePose
}//End of namespace: MxMEditor