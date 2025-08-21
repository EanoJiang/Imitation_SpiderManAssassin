using System.Collections.Generic;
using UnityEngine;
namespace FS_ParkourSystem
{
    public class AnimatorHelper
    {

        public string animationName;
        public static Avatar currentAvatar;
        public static Dictionary<Avatar, Animator> _animators = new();
        public static Animator _animator;

        public Vector3 getRotatedPos(AvatarTarget target, float time)
        {
            var pos = getPos(target, time);
            var rot = getRot(AvatarTarget.Root, time);
            //rot = Quaternion.Inverse(rot);
            rot.eulerAngles *= -1;
            //pos = rot * pos;
            pos.x *= -1;
            pos.z *= -1;
            return rot * pos;
        }
        public Vector3 getPos(AvatarTarget target, float time)
        {
            _animator.SetTarget(target, time);
            _animator.Update(0);
            return _animator.targetPosition;
        }
        public Quaternion getRot(AvatarTarget target, float time)
        {
            _animator.SetTarget(target, time);
            _animator.Update(0);
            return _animator.targetRotation;
        }

        public Vector3 pointTransformWithVectorDown(Transform transform, Vector3 point, Vector3 pos = new Vector3())
        {
            if (pos == Vector3.zero)
                return transform.position + transform.right * point.x + Vector3.up * point.y + transform.forward * point.z;
            else
                return pos + transform.right * point.x + Vector3.up * point.y + transform.forward * point.z;
        }

        public Vector3 pointToTranform(Transform transform, Vector3 point, Vector3 pos = new Vector3())
        {
            if (pos == Vector3.zero)
                return transform.position + transform.right * point.x + transform.up * point.y + transform.forward * point.z;
            else
                return pos + transform.right * point.x + transform.up * point.y + transform.forward * point.z;
        }
        public Vector3 getTransformPos(HumanBodyBones target, float time, string animName = null)
        {
            if (animName == null) animName = animationName;
            _animator.Play(animName, 0, time);
            _animator.Update(0);
            var pos = _animator.GetBoneTransform(target).position;
            pos.x *= -1;
            pos.z *= -1;
            return pos;
        }
        public Vector3 getTransformRootPos(float time, string animName = null)
        {
            if (animName == null) animName = animationName;
            _animator.Play(animName, 0, time);
            _animator.Update(0);
            return _animator.transform.position;
        }
        public void initialize(Animator animator)
        {
            if (!_animators.ContainsKey(animator.avatar) || _animators[animator.avatar] == null)
            {
                currentAvatar = animator.avatar;
                var sampler = new GameObject("animation Sampler", animator.GetType());
                sampler.SetActive(false);
                _animator = sampler.GetComponent<Animator>();
                _animator.runtimeAnimatorController = animator.runtimeAnimatorController;
                _animator.avatar = animator.avatar;
                _animator.applyRootMotion = true;

                foreach (Transform child in animator.gameObject.transform)
                {
                    GameObject.Instantiate(child, _animator.gameObject.transform);
                }

                if (_animator.GetBoneTransform(HumanBodyBones.Hips) == null)
                {
                    _animator = null;
                    GameObject.Destroy(sampler);
                    foreach (Transform child in animator.gameObject.transform)
                    {
                        if (_animator = child.GetComponentInChildren<Animator>())
                            break;
                    }
                    if (_animator == null) _animator = animator.GetComponentInChildren<Animator>();

                    var controller = _animator.GetComponent<AnimatorRootmotionController>();
                    if (controller != null) controller.enabled = false;

                    sampler = GameObject.Instantiate(_animator.gameObject);

                    if (controller != null) controller.enabled = true;

                }
                sampler.SetActive(false);
                _animator = sampler.GetComponent<Animator>();
                _animator.runtimeAnimatorController = animator.runtimeAnimatorController;
                _animator.avatar = animator.avatar;
                _animator.applyRootMotion = true;

                //performance
                _animator.speed = 0;
                _animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
                sampler.SetActive(true);
                _animator.Update(0);

                foreach (var item in _animator.GetComponentsInChildren<SkinnedMeshRenderer>())
                {
                    GameObject.Destroy(item);
                }

                _animators[_animator.avatar] = _animator;
                //sampler.hideFlags = HideFlags.HideAndDontSave;
                sampler.hideFlags = HideFlags.HideInHierarchy;
            }
        }
        public Animator sampleAnimation(string animName, Animator animator)
        {
            initialize(animator);
            _animator = _animators[animator.avatar];
            _animator.gameObject.SetActive(true);
            //_animator.Update(0);

            foreach (var parameter in animator.parameters)
            {
                if (animator.IsParameterControlledByCurve(parameter.nameHash)) continue;

                if (parameter.type == AnimatorControllerParameterType.Float)
                    _animator.SetFloat(parameter.nameHash, animator.GetFloat(parameter.nameHash));
                else if (parameter.type == AnimatorControllerParameterType.Int)
                    _animator.SetInteger(parameter.nameHash, animator.GetInteger(parameter.nameHash));
                else if (parameter.type == AnimatorControllerParameterType.Bool)
                    _animator.SetBool(parameter.nameHash, animator.GetBool(parameter.nameHash));
            }
            animationName = animName;
            _animator.Play(animName, 0, 0);
            _animator.Update(0);

            return _animator;
        }

        public void closeSampler(Animator animator)
        {
            //GameObject.Destroy(animator.gameObject);
            animator.gameObject.SetActive(false);
        }

        public void setChildActive(Transform transform,bool active = false)
        {
            for (int i = 0; i < transform.childCount; i++)
                transform.GetChild(i).gameObject.SetActive(active);
        }

        public float findStartTime(AvatarTarget target, float maxDelta = 0.005f)
        {
            setChildActive(_animator.transform, false);

            maxDelta *= maxDelta;

            var startTime = 0f;
            var time = 0f;
            Vector3 start = new();
            while (time <= 1f)
            {
                _animator.SetTarget(target, 1f - time);
                _animator.Update(0);
                var diff = (start - _animator.targetPosition).sqrMagnitude;
                if (diff > maxDelta) startTime = 1f - time;
                start = _animator.targetPosition;
                time += 0.02f;
            }
            setChildActive(_animator.transform, true);

            return startTime;
        }

        public float findEndTime(AvatarTarget target, float maxDelta = 0.005f)
        {
            setChildActive(_animator.transform, false);

            maxDelta *= maxDelta;

            var endTime = 1f;
            var time = 0f;
            Vector3 end = new();
            while (time <= 1f)
            {
                _animator.SetTarget(target, time);
                _animator.Update(0);
                var diff = (end - _animator.targetPosition).sqrMagnitude;
                if (diff > maxDelta) endTime = time;
                end = _animator.targetPosition;
                time += 0.02f;
            }
            setChildActive(_animator.transform, true);

            return endTime;
        }
    }
}