#if gameCreator2
using GameCreator.Runtime.Characters;
#endif
using UnityEngine;

namespace FS_ParkourSystem
{
    public class AnimatorRootmotionController : MonoBehaviour
    {
#if gameCreator2
        Animator animator;
        public GC2_IntegrationHelper helper;

        public Character character;

        ParkourController parkourController;
        ClimbController climbController;

        private void Awake()
        {
            animator = GetComponent<Animator>();
            if (helper == null)
                helper = FindObjectOfType<GC2_IntegrationHelper>();
            parkourController = helper.GetComponent<ParkourController>();
            climbController = helper.GetComponent<ClimbController>();
            if (character == null)
                character = GetComponentInParent<Character>();
        }
        private void OnAnimatorMove()
        {
            if (helper.UseRootMotion && parkourController.ControlledByParkour)
            {
                if (animator.deltaPosition != Vector3.zero)
                {
                    helper.transform.position += animator.deltaPosition;
                }
                helper.transform.rotation *= animator.deltaRotation;
            }
        }
        private void OnAnimatorIK(int layerIndex)
        {
            if (parkourController.ControlledByParkour)
            {
                climbController.OnAnimatorIK(layerIndex);
            }
        }
#endif
    }
}
