using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IKController : MonoBehaviour
{
    public Animator _animator;

    //IK控制点
    //四肢关节点
    public Transform ik_LHand;
    public Transform ik_RHand;
    public Transform ik_LFoot;
    public Transform ik_RFoot;
    //头部控制点,可以根据主相机的位置，让玩家能够从侧视角下看到头部偏转。
    public Transform Head_IKPoint;

    [Header("IK 权重控制")]
    [SerializeField] private float ikBlendSpeed = 5f; // IK权重变化速度
    [SerializeField] private float headTurnSpeed = 5f; // 头部转向速度  
    [SerializeField] private float maxHeadAngle = 60f; // 头部最大转向角度

    // IK相关私有变量
    private float _currentHeadIKWeight = 0f; // 当前头部IK权重
    private Vector3 _currentLookTarget; // 缓存"当前正在看的点"
    private bool _hasInitializedLookTarget = false; // 是否已初始化看向目标

    private void OnAnimatorIK(int layerIndex)
    {
        // 四肢IK控制
        if (ik_LHand != null)
            IKControl(AvatarIKGoal.LeftHand, ik_LHand);
        if (ik_RHand != null)
            IKControl(AvatarIKGoal.RightHand, ik_RHand);
        if (ik_LFoot != null)
            IKControl(AvatarIKGoal.LeftFoot, ik_LFoot);
        if (ik_RFoot != null)
            IKControl(AvatarIKGoal.RightFoot, ik_RFoot);

        // 头部IK控制 - 使用平滑权重过渡
        HandleHeadIK();
    }

    /// <summary>
    /// 处理头部IK控制 - 解决生硬切换问题
    /// </summary>
    private void HandleHeadIK()
    {
        if (Head_IKPoint == null)
            return;

        // 判断摄像机是否在角色前面
        Vector3 directionToCamera = Head_IKPoint.position - transform.position;
        bool isCameraInFront = Vector3.Dot(transform.forward, directionToCamera.normalized) > 0;

        // 判断是否应该启用头部IK
        // 当摄像机在角色前面时，禁用头部IK；当摄像机在角色后面时，启用头部IK
        bool shouldUseHeadIK = !isCameraInFront && _animator.GetFloat(AnimationID.MovementID) < 0.1f;

        // 计算目标权重
        float targetWeight = shouldUseHeadIK ? 1f : 0f;

        // 平滑过渡权重 - 这是解决生硬切换的关键
        _currentHeadIKWeight = Mathf.Lerp(_currentHeadIKWeight, targetWeight, ikBlendSpeed * Time.deltaTime);

        // 如果权重大于0，执行头部IK控制
        if (_currentHeadIKWeight > 0.01f)
        {
            IKHeadControl(Head_IKPoint, headTurnSpeed, maxHeadAngle);
        }

        // 使用平滑权重而不是固定的1f
        _animator.SetLookAtWeight(_currentHeadIKWeight);

        // 如果已初始化目标位置，设置看向位置
        if (_hasInitializedLookTarget)
        {
            _animator.SetLookAtPosition(_currentLookTarget);
        }
    }


    /// <summary>
    /// 头部 IK 控制（平滑转向 + 角度限制）
    /// </summary>
    /// <param name="target">要看的对象</param>
    /// <param name="turnSpeed">插值速度</param>
    /// <param name="maxAngle">最大允许夹角（度数）</param>
    private void IKHeadControl(Transform target,
                               float turnSpeed = 5f,
                               float maxAngle = 60f)
    {
        // 初始化看向目标 - 防止第一次启用时的突然跳转
        if (!_hasInitializedLookTarget)
        {
            _currentLookTarget = transform.position + transform.forward * 5f;
            _hasInitializedLookTarget = true;
        }

        // 1. 计算最终想要看的点
        Vector3 rawTargetPos;

        Vector3 directionToCamera = target.position - transform.position;
        bool isCameraInFront = Vector3.Dot(transform.forward, directionToCamera.normalized) > 0;

        if (isCameraInFront)
        {
            // 相机在前面，看向相机
            rawTargetPos = target.position;
        }
        else
        {
            // 相机在背后，看向相机视线向前延伸的点
            rawTargetPos = target.position + target.forward * 10f;
        }

        // 2. 计算与正前方向的夹角
        Vector3 dirToRawTarget = (rawTargetPos - transform.position).normalized;
        float angle = Vector3.Angle(transform.forward, dirToRawTarget);

        // 3. 如果角度在范围内，才允许平滑转向
        if (angle <= maxAngle)
        {
            _currentLookTarget = Vector3.Lerp(_currentLookTarget, rawTargetPos,
                                              turnSpeed * Time.deltaTime);
        }
        // 否则保持上一帧的 _currentLookTarget 不变（即不更新）

        // 4. Debug绘制
        Debug.DrawLine(transform.position, _currentLookTarget, Color.red);
        Debug.DrawRay(target.position, target.forward * 10f, Color.blue);

        // 注意：移除了这里的SetLookAtWeight和SetLookAtPosition调用
        // 因为现在在HandleHeadIK()中统一处理
    }
    /// <summary>
    /// 四肢IK控制
    /// </summary>
    /// <param name="ControlPosition"></param>
    /// <param name="target"></param>
    public void IKControl(AvatarIKGoal ControlPosition, Transform target)
    {
        _animator.SetIKPositionWeight(ControlPosition, 1);
        _animator.SetIKPosition(ControlPosition, target.position);
        _animator.SetIKRotationWeight(ControlPosition, 1);
        _animator.SetIKRotation(ControlPosition, target.rotation);
    }
}
