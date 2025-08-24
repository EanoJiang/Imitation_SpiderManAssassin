using UnityEngine;

public class ApproachState : EnvironmentInteractionState
{
    // 接近状态的计时器
    float _elapsedTimer = 0.0f;
    // 过渡时间
    float _lerpDuration = 5.0f;
    // 接近状态持续时间,超过就回到ResetState状态
    float _approachDuration = 2.0f;
    // 接近状态的IkConstraint目标权重
    float _approachWeight = 0.5f;
    // 接近状态的MultiRotationConstraint目标旋转权重
    float _approachRotationWeight = 0.75f;
    // 旋转速度
    float _rotationSpeed = 500f;
    // 是否能切换到上升状态的距离阈值
    float _riseDistanceThreshold = 0.5f;

    // 构造函数
    public ApproachState(EnvironmentInteractionContext context, EnvironmentInteractionStateMachine.EEnvironmentInteractionState estate) : base(context, estate)
    {
        EnvironmentInteractionContext Context = context;
    }
    public override void EnterState() {
        // 重置计时器
        _elapsedTimer = 0.0f;
        Debug.Log("进入 Approach State");
    }
    public override void ExitState()
    {
        Debug.Log("退出 Approach State");
    }
    public override void UpdateState() { 
        //目标朝向：让手掌朝向地面，forwad=向下，up=角色的朝向
        Quaternion targetGroundRotation = Quaternion.LookRotation(-Vector3.up, Context.RootTransform.forward);

        _elapsedTimer += Time.deltaTime;
        
        // 控制手腕旋转ik的控制器朝向 旋转到 目标朝向
        Context.CurrentIkTargetTransform.rotation = Quaternion.RotateTowards(
            Context.CurrentIkTargetTransform.rotation, 
            targetGroundRotation, 
            _rotationSpeed * Time.deltaTime);

        // 更新权重：从当前的权重过渡到接近状态的对应权重
        //MultiRotationConstraint：
        Context.CurrentMultiRotationConstraint.weight = Mathf.Lerp(
            Context.CurrentMultiRotationConstraint.weight, 
            _approachRotationWeight, 
            _elapsedTimer / _lerpDuration);
        //IkConstraint：
        Context.CurrentIkConstraint.weight = Mathf.Lerp(
            Context.CurrentIkConstraint.weight, 
            _approachWeight, 
            _elapsedTimer / _lerpDuration);
    }
    public override EnvironmentInteractionStateMachine.EEnvironmentInteractionState GetNextState()
    {
        // 是否超过Approach状态的持续时间
        bool isOverStateLifeTime = _elapsedTimer > _approachDuration;
        if (isOverStateLifeTime || CheckShouldReset())
        {
            // 切换到Reset状态
            return EnvironmentInteractionStateMachine.EEnvironmentInteractionState.Reset;
        }

        // 是否在手臂伸手范围内
        bool isWithArmsReach = Vector3.Distance(Context.ClosestPointOnColliderFromShoulder, Context.CurrentShoulderTransform.position) < _riseDistanceThreshold;
        bool isClosestPointOnColliderValid = Context.ClosestPointOnColliderFromShoulder != Vector3.positiveInfinity;
        if (isWithArmsReach && isClosestPointOnColliderValid)
        {
            // 切换到上升状态
            return EnvironmentInteractionStateMachine.EEnvironmentInteractionState.Rise;
        }
        return StateKey;
    }
    public override void OnTriggerEnter(Collider other) {
        StartIkTargetPositionTracking(other);
        }
    public override void OnTriggerStay(Collider other) {
        UpdateIkTargetPosition(other);
    }
    public override void OnTriggerExit(Collider other) { 
        ResetIkTargetPositionTracking(other);
    }
}