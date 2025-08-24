using UnityEngine;

public class RiseState : EnvironmentInteractionState
{
    float _elapsedTimer = 0.0f;         // 已消耗时间，用于控制插值进度
    float _lerpDuration = 5.0f;         // 插值总时长，决定状态过渡的“慢/快”
    float _riseWeight = 1.0f;           // 权重目标值，用于IK和旋转约束的过渡

    Quaternion _targetHandRotation;   // 手部的目标旋转角度，用于让手部贴合交互物体表面
    float _maxDistance = 0.5f;         // 射线检测的最大距离
    protected LayerMask _interactableLayerMask = LayerMask.GetMask("Interactable");
    float _rotationSpeed = 1000f;      // 旋转速度

    // 用于判断是否能够进入TouchState状态的阈值
    float _touchDistanceThreshold = 0.05f;  // TouchState的距离阈值
    float _touchTimeThreshold = 1f;         // TouchState的持续时间阈值

    public RiseState(EnvironmentInteractionContext context, EnvironmentInteractionStateMachine.EEnvironmentInteractionState estate): base(context, estate)
    {
        EnvironmentInteractionContext Context = context;
    }

    public override void EnterState()
    {
        // 重置计时器
        _elapsedTimer = 0.0f;
        Debug.Log("进入 Rise State");
    }
    public override void ExitState()
    {
        Debug.Log("退出 Rise State");
    }

    public override void UpdateState()
    {
        // 计算期望的手部旋转角度
        CalculateExpectedHandRotation();

        // 1. 碰撞点的y轴高度偏移 平滑更新到 最近碰撞点的Y坐标
        Context.InteractionPoint_Y_Offset = Mathf.Lerp(
            Context.InteractionPoint_Y_Offset,
            Context.ClosestPointOnColliderFromShoulder.y,
            _elapsedTimer / _lerpDuration
        );

        // 2. 更新IK约束CurrentIkConstraint的权重：从当前权重到目标权重_riseWeight
        Context.CurrentIkConstraint.weight = Mathf.Lerp(
            Context.CurrentIkConstraint.weight,
            _riseWeight,
            _elapsedTimer / _lerpDuration
        );

        // 3. 更新多旋转约束CurrentMultiRotationConstraint的权重：从当前权重到目标权重_riseWeight
        Context.CurrentMultiRotationConstraint.weight = Mathf.Lerp(
            Context.CurrentMultiRotationConstraint.weight,
            _riseWeight,
            _elapsedTimer / _lerpDuration
        );

        // 4. 让 IK目标控制器 朝着 预期的手部旋转角度 平滑旋转
        Context.CurrentIkTargetTransform.rotation = Quaternion.RotateTowards(
            Context.CurrentIkTargetTransform.rotation,
            _targetHandRotation,
            _rotationSpeed * Time.deltaTime
        );

        _elapsedTimer += Time.deltaTime;
    }

    /// <summary>
    /// 计算期望的手部旋转角度，用于让手部贴合交互物体表面
    /// </summary>
    private void CalculateExpectedHandRotation()
    {
        // 1. 获取起始点（肩部位置）和终点（最近碰撞点）
        Vector3 startPos = Context.CurrentShoulderTransform.position;
        Vector3 endPos = Context.ClosestPointOnColliderFromShoulder;

        // 2. 射线方向：肩部指向碰撞点的归一化方向向量
        Vector3 direction = (endPos - startPos).normalized;

        // 3. 发射射线
        if (Physics.Raycast(startPos, direction, out RaycastHit hit, _maxDistance, _interactableLayerMask))
        {
            // 碰撞点的表面法线
            Vector3 surfaceNormal = hit.normal;

            // 目标朝向：与表面法线相反（让手部朝向碰撞点的表面法线的反方向）
            Vector3 targetForward = -surfaceNormal;

            // 手部的目标旋转方向：与目标朝向相同，但绕着Y轴旋转90度
            _targetHandRotation = Quaternion.LookRotation(targetForward, Vector3.up);
        }
    }


    public override EnvironmentInteractionStateMachine.EEnvironmentInteractionState GetNextState()
    {
        if (CheckShouldReset())
        {
            // 切换到Reset状态
            return EnvironmentInteractionStateMachine.EEnvironmentInteractionState.Reset;
        }
        // 标志位： 是否达到能够Touch的距离阈值
        bool isCloseToTouch = Vector3.Distance(
                Context.CurrentIkTargetTransform.position,
                Context.ClosestPointOnColliderFromShoulder
            ) < _touchDistanceThreshold;
        // 标志位： 是否达到能够Touch的持续时间阈值
        bool isTouchTime = _elapsedTimer >= _touchTimeThreshold;

        if (isCloseToTouch && isTouchTime)
        {
            // 切换到Touch状态
            return EnvironmentInteractionStateMachine.EEnvironmentInteractionState.Touch;
        }

        return StateKey;
    }
    public override void OnTriggerEnter(Collider other)
    {
        StartIkTargetPositionTracking(other);
    }
    public override void OnTriggerStay(Collider other)
    {
        UpdateIkTargetPosition(other);
    }
    public override void OnTriggerExit(Collider other)
    {
        ResetIkTargetPositionTracking(other);
    }
}