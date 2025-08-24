using UnityEngine;

public class ResetState : EnvironmentInteractionState
{
    // 持续时间计时器
    float _elapsedTimer = 0.0f;
    // 持续时间的阈值
    float _resetDuration = 1.0f;
    // 平滑过渡的持续时间
    float _lerpDuration = 10.0f;
    // 转向速度
    float _rotationSpeed = 500f;

    // 构造函数
    public ResetState(EnvironmentInteractionContext context, EnvironmentInteractionStateMachine.EEnvironmentInteractionState estate) : base(context, estate)
    {
        EnvironmentInteractionContext Context = context;
    }
    public override void EnterState(){
        // 重置 持续时间计时器
        _elapsedTimer = 0.0f;
        // 重置 最近碰撞点 和 当前碰撞体
        Context.ClosestPointOnColliderFromShoulder = Vector3.positiveInfinity;
        Context.CurrentIntersectingCollider = null;

        Debug.Log("进入 ResetState");
    }
    public override void ExitState() { 
        Debug.Log("退出 ResetState");
    }
    public override void UpdateState() {
        _elapsedTimer += Time.deltaTime;
        // 碰撞点的 Y 轴偏移，平滑过渡到角色碰撞体中心的高度
        Context.InteractionPoint_Y_Offset = Mathf.Lerp(
            Context.InteractionPoint_Y_Offset, 
            Context.CharacterColliderCenterY, 
            _elapsedTimer / _lerpDuration);

        // 更新权重：平滑重置当前的权重
        //IkConstraint：
        Context.CurrentIkConstraint.weight = Mathf.Lerp(
            Context.CurrentIkConstraint.weight,
            0,
            _elapsedTimer / _lerpDuration);
        //MultiRotationConstraint：
        Context.CurrentMultiRotationConstraint.weight = Mathf.Lerp(
            Context.CurrentMultiRotationConstraint.weight,
            0,
            _elapsedTimer / _lerpDuration);

        // ik目标控制器部件也回到原来的position和rotation
        Context.CurrentIkTargetTransform.localPosition = Vector3.Lerp(
            Context.CurrentIkTargetTransform.localPosition,
            Context.CurrentOriginalTargetPosition,
            _elapsedTimer / _lerpDuration
        );

        Context.CurrentIkTargetTransform.rotation = Quaternion.RotateTowards(
            Context.CurrentIkTargetTransform.rotation,
            Context.OriginalTargetRotation,
            _rotationSpeed * Time.deltaTime
        );
    }
    public override EnvironmentInteractionStateMachine.EEnvironmentInteractionState GetNextState() 
    {
        // 标志位：是否正在移动(是否有Movement输入)
        bool isMoving = GameInputManager.Instance.Movement != Vector2.zero;
        //只有当持续时间超过阈值，且角色正在移动时，才会切换到 SearchState
        if(_elapsedTimer > _resetDuration && isMoving)
        {
            // 下一个状态为 SearchState
            return EnvironmentInteractionStateMachine.EEnvironmentInteractionState.Search;
        }
        return StateKey; 
    }
    // ResetState 状态下不会调用任何触发器的Callback
    public override void OnTriggerEnter(Collider other) { }
    public override void OnTriggerStay(Collider other) { }
    public override void OnTriggerExit(Collider other) { }
}
