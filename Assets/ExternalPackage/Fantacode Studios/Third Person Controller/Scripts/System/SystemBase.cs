using System;
using UnityEngine;
namespace FS_ThirdPerson
{
    public class SystemBase : MonoBehaviour
    {
        public virtual void HandleAwake() { }
        public virtual void HandleStart() { }
        public virtual void HandleFixedUpdate() { }
        public virtual void HandleUpdate() { }
        public virtual void HandleOnAnimatorMove(Animator animator)
        {
            if (animator.deltaPosition != Vector3.zero)
                transform.position += animator.deltaPosition;
            transform.rotation *= animator.deltaRotation;
        }
        /// <summary>
        /// priority based on higher number
        /// </summary>
        public virtual float Priority { get; set; } = 0;
        public bool IsInFocus { get; set; }

        [field: SerializeField]
        public virtual SystemState State { get; } = SystemState.Other;
        public virtual SubSystemState SubState { get; } = SubSystemState.None;
        /// <summary>
        /// only this script will run. all other updates are discarded.
        /// </summary>
        public void FocusScript() => IsInFocus = true;

        public void UnFocusScript() => IsInFocus = false;

        Type SystemBaseType = typeof(SystemBase);
        public bool HasOverrided(string methonName) => SystemBaseType.GetMethod(methonName).DeclaringType != SystemBaseType;


        public virtual void EnterSystem() { }
        public virtual void ExitSystem() { }

        public Action OnStateEntered { get; set; }
        public Action OnStateExited { get; set; }

    }
}