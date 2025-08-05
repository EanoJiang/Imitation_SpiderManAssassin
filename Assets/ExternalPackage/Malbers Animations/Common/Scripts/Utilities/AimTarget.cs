using MalbersAnimations.Events;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace MalbersAnimations.Utilities
{
    /// <summary>
    /// For when someone with LookAt enters it will set this transform as the target
    /// </summary>
    public class AimTarget : MonoBehaviour, IAimTarget
    {
        /// <summary>This will set AutoAiming for the Aim Logic</summary>
        [SerializeField] private bool aimAssist;
        public BoolEvent OnAimEnter = new BoolEvent();

        public bool AimAssist
        {
            get { return aimAssist; }
            set { aimAssist = value; }
        }

        public void AimEnter(bool enter)
        {
            OnAimEnter.Invoke(enter);
        }

        void OnTriggerEnter(Collider other)
        {
            if (other.isTrigger) return; //Ignore if the Collider entering is a Trigger

            IAim Aimer = other.GetComponentInParent<IAim>();

            if (Aimer != null)
            {
                //Aimer.Active = true;
                Aimer.ForcedTarget = transform;
                OnAimEnter.Invoke(true);
            }
        }

        void OnTriggerExit(Collider other)
        {
            if (other.isTrigger) return;//Ignore if the Collider exiting is a Trigger

            IAim Aimer = other.GetComponentInParent<IAim>();

            if (Aimer != null)
            {
                Aimer.ForcedTarget = null;
                OnAimEnter.Invoke(false);
            }
        }
    }
}