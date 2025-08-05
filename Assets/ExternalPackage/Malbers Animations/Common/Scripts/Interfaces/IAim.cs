using UnityEngine;
namespace MalbersAnimations
{
    /// <summary>Interface used to Check if the Player/Animal is Aiming</summary>
    public interface IAim
    {
        /// <summary>Is the Aiming Logic Active?</summary>
        bool Active { get; set; }

        /// <summary>Direction Vector Stored of the Aiming Logic</summary>
        Vector3 AimDirection { get; }

        Camera MainCamera { get; }

        /// <summary>Limit the Aiming via Angle limit Which means the Aiming is Active but should not be used</summary>
        bool Limited { get; set; }

        /// <summary>Check if the Camera is in the (Right:true) or (Left: False) side of the Animal </summary>
        bool CameraSide { get; }

        /// <summary>Check if the Target is in the (Right:true) or (Left: False) side of the Animal </summary>
        bool TargetSide { get; }

        /// <summary>What to do with the Triggers ... Ignore them? Use them?</summary>
        QueryTriggerInteraction TriggerInteraction { get; }

        /// <summary>Layer to Aim and Hit</summary>
        LayerMask AimLayer { get; }

        /// <summary>RaycastHit Data of the Aim logic</summary>
        RaycastHit AimHit { get; }

        /// <summary>Forced Target on the Aiming Logic</summary>
        Transform ForcedTarget { set; get; }

        Transform AimOrigin { set; get; }

        /// <summary>Forced the Aim to Ignore a Transform</summary>
        Transform IgnoreTransform { set; get; }
    }

    public interface IAimTarget
    {
        bool AimAssist { get; }
        void AimEnter(bool enter);
    }
}