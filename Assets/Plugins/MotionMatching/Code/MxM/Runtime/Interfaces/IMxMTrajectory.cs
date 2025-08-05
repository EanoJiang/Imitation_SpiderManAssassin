// Copyright © 2017-2024 Vault Break Studios Pty Ltd

using UnityEngine;

namespace MxM
{

    //=============================================================================================
    /**
    *  @brief Interface for any gameplay object which is to have a motion matching animation entity
    *         
    *********************************************************************************************/
    public interface IMxMTrajectory
    {
        TrajectoryPoint[] GetCurrentGoal();
        Transform GetTransform();
        void SetGoalRequirements(float[] a_predictionTimes);
        void SetGoal(TrajectoryPoint[] a_goal);
        void CopyGoalFromPose(ref PoseData a_poseData);
        void Pause();
        void UnPause();
        void ResetMotion(float a_rotation=0f);
        bool HasMovementInput();
        bool IsEnabled();

    }//End of interface: IMxMTrajectory
}//End of namespace: MxM