using UnityEngine;
namespace FS_ThirdPerson
{
    public class FootIK : MonoBehaviour
    {
        public bool enableFootIK = true;
        public Transform root;
        [SerializeField] float hipIKSmooth = 5;

        [SerializeField] float footOffset = .1f;
        [SerializeField] float footRayHeight = .8f;
        [SerializeField] float footAngleLimit = 30;
        [SerializeField] LayerMask groundLayer = 1;

        public bool IkEnabled { get; set; }

        float ikSmooth;

        Animator animator;
        PlayerController playerController;

        float offs;
        Vector3 prevPos;

        private void Awake()
        {
            animator = GetComponent<Animator>();
            playerController = GetComponent<PlayerController>();

            if (!(groundLayer == (groundLayer | (1 << LayerMask.NameToLayer("Ledge")))))
                groundLayer += 1 << LayerMask.NameToLayer("Ledge");
        }
        private void OnAnimatorIK(int layerIndex)
        {
            if(enableFootIK)
                SetFootIK();
        }
        void SetFootIK()
        {
            if (playerController.FocusedSystemState != SystemState.Locomotion)
                return;
            if (IkEnabled)
            {
                var rightFoot = animator.GetBoneTransform(HumanBodyBones.RightFoot);
                var leftFoot = animator.GetBoneTransform(HumanBodyBones.LeftFoot);
                var hips = animator.GetBoneTransform(HumanBodyBones.Hips);
                //if (Vector3.Distance(prevPos, transform.position) > 0.001f)
                //{
                //    offs = Mathf.Lerp(offs, 0, Time.deltaTime * hipIKSmooth);
                //    root.localPosition = offs * hips.up;
                //    prevPos = transform.position;
                //    return;
                //}

                //if(animator.GetFloat(AnimatorParameters.moveAmount) > 0.05f)
                //{
                //    offs = Mathf.Lerp(offs, 0, Time.deltaTime * hipIKSmooth);
                //    root.localPosition = offs * hips.up;
                //    prevPos = transform.position;
                //    return;
                //}

                var leftFootHit = Physics.SphereCast(leftFoot.position + Vector3.up * footRayHeight / 2, 0.1f, Vector3.down, out RaycastHit leftHit, footRayHeight + footOffset, groundLayer);
                var rightFootHit = Physics.SphereCast(rightFoot.position + Vector3.up * footRayHeight / 2, 0.1f, Vector3.down, out RaycastHit rightHit, footRayHeight + footOffset, groundLayer);


                var leftFootIKWeight = (leftFootHit && leftHit.point.y > leftFoot.position.y - footOffset) ? 1 : animator.GetFloat("leftFootIK");
                var rightFootIKWeight = (rightFootHit && rightHit.point.y > rightFoot.position.y - footOffset) ? 1 : animator.GetFloat("rightFootIK");

                var leftFootPos = leftFootHit ? leftHit.point.y : leftFoot.position.y - footOffset;
                var rightFootPos = rightFootHit ? rightHit.point.y : rightFoot.position.y - footOffset;


                float leftOffset = leftFootPos - transform.position.y;
                float rightOffset = rightFootPos - transform.position.y;
                var idleVal = (animator.GetFloat(AnimatorParameters.idleType) < 0.5f ? 0.1f : 1);
                var offset = (leftOffset < rightOffset ? leftOffset : rightOffset) * idleVal;

                offs = Mathf.Lerp(offs, offset, Time.deltaTime * hipIKSmooth);
                Vector3 downDir = hips.up;
                hips.localPosition = offs * downDir;
                root.localPosition = offs * downDir;

                var isInCrouchIdle = animator.GetFloat(AnimatorParameters.crouchType) > 0.5f && idleVal > 0.5f;

                float _footOffset = isInCrouchIdle ? footOffset + 0.05f : footOffset;

                if (rightFootHit)
                {
                    var angle = Vector3.Angle(transform.up, rightHit.normal);
                    var normal = Vector3.Slerp(transform.up, rightHit.normal, footAngleLimit / angle);
                    var rot = Quaternion.FromToRotation(transform.up, normal) * transform.rotation;
                    var pos = rightHit.point + rightHit.normal * _footOffset - hips.localPosition;
                    animator.SetIKPosition(AvatarIKGoal.RightFoot, pos);
                    animator.SetIKRotation(AvatarIKGoal.RightFoot, rot);
                }
                if (leftFootHit)
                {
                    var angle = Vector3.Angle(transform.up, leftHit.normal);
                    var normal = Vector3.Slerp(transform.up, leftHit.normal, footAngleLimit / angle);
                    var rot = Quaternion.FromToRotation(transform.up, normal) * transform.rotation;
                    var pos = leftHit.point + leftHit.normal * _footOffset - hips.localPosition;
                    animator.SetIKPosition(AvatarIKGoal.LeftFoot, pos);
                    animator.SetIKRotation(AvatarIKGoal.LeftFoot, rot);
                }
                ikSmooth = Mathf.Clamp01(ikSmooth + 0.1f * Time.deltaTime);
                if (!isInCrouchIdle)
                {
                    //var ikRot = Vector3.Distance(prevPos, transform.position) > 0.001f ? 0.6f : 0;
                    animator.SetIKRotationWeight(AvatarIKGoal.RightFoot, leftFootIKWeight * 0.6f * (1 - animator.GetFloat(AnimatorParameters.moveAmount) * 2f));
                    animator.SetIKRotationWeight(AvatarIKGoal.LeftFoot, rightFootIKWeight * 0.6f * (1 - animator.GetFloat(AnimatorParameters.moveAmount) * 2f));
                }
                animator.SetIKPositionWeight(AvatarIKGoal.LeftFoot, leftFootIKWeight * (1 - animator.GetFloat(AnimatorParameters.moveAmount) * 2f));
                animator.SetIKPositionWeight(AvatarIKGoal.RightFoot, rightFootIKWeight * (1 - animator.GetFloat(AnimatorParameters.moveAmount) * 2f));

                prevPos = transform.position;
            }
            else 
            {
                ikSmooth = Mathf.Clamp01(ikSmooth - 0.2f * Time.deltaTime);
                animator.SetIKRotationWeight(AvatarIKGoal.RightFoot, ikSmooth * 0.6f);
                animator.SetIKRotationWeight(AvatarIKGoal.LeftFoot, ikSmooth * 0.6f);
                animator.SetIKPositionWeight(AvatarIKGoal.LeftFoot, ikSmooth);
                animator.SetIKPositionWeight(AvatarIKGoal.RightFoot, ikSmooth);
            }
        }

        private void Update()
        {
            if (playerController.FocusedSystemState != SystemState.Locomotion)
                return;
            if (IkEnabled)
                root.localPosition = Vector3.zero;
        }
    }
}
