// Copyright © 2017-2024 Vault Break Studios Pty Ltd

using UnityEngine;
using System.Collections.Generic;

namespace MxM
{
    public interface IComplexAnimData
    {
        EComplexAnimType ComplexAnimType { get; }

        MotionCurveData GetMotionCurveData();

    }//End of interface: IComplexAnimData
}//End of namespace: MxM