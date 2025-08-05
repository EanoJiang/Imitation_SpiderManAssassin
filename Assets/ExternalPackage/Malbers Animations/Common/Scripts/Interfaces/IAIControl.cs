using UnityEngine;
namespace MalbersAnimations
{
    /// <summary>Interface used for Moving the Animal with a Nav Mesh Agent</summary>
    public interface IAIControl
    {
        /// <summary>Set the next Target</summary>  
        void SetTarget(Transform target);

        /// <summary>Set the next Destination Position without having a target</summary>   
        void SetDestination(Vector3 PositionTarget);

        /// <summary> Stop the Agent on the Animal... also remove the Transform target and the Target Position and Stops the Animal </summary>
        void Stop();
    }
}


