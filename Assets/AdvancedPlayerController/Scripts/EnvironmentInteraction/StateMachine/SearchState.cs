using UnityEngine;

public class SearchState : EnvironmentInteractionState
{
    // 接近碰撞点的距离阈值
    public float _approachDistanceThreshold = 2.0f;

    // 构造函数
    public SearchState(EnvironmentInteractionContext context, EnvironmentInteractionStateMachine.EEnvironmentInteractionState estate) : base(context, estate)
    {
        EnvironmentInteractionContext Context = context;
    }
    public override void EnterState() {
        Debug.Log("进入 Search State");}
    public override void ExitState() { 
        Debug.Log("退出 Search State");}
    public override void UpdateState() { 
        }
    public override EnvironmentInteractionStateMachine.EEnvironmentInteractionState GetNextState()
    {
        if (CheckShouldReset())
        {
            // 切换到Reset状态
            return EnvironmentInteractionStateMachine.EEnvironmentInteractionState.Reset;
        }

        // 标志位：是否接近目标
        bool isCloseToTarget = Vector3.Distance(Context.ClosestPointOnColliderFromShoulder, Context.RootTransform.position) < _approachDistanceThreshold;
        // 标志位：是否是最近碰撞点(只要不是无穷大，就是最近碰撞点)
        bool isClosestPointOnColliderValid = Context.ClosestPointOnColliderFromShoulder != Vector3.positiveInfinity;
        if (isCloseToTarget && isClosestPointOnColliderValid)
        {
            // 切换到Approach状态
            return EnvironmentInteractionStateMachine.EEnvironmentInteractionState.Approach;
        }
        return StateKey;
    }
    public override void OnTriggerEnter(Collider other) {
        // 进入搜索状态，开始跟踪目标位置
        StartIkTargetPositionTracking(other);
    }
    public override void OnTriggerStay(Collider other) {
        // 跟踪目标位置
        UpdateIkTargetPosition(other);
    }
    public override void OnTriggerExit(Collider other) {
        // 退出搜索状态，停止跟踪目标位置
        ResetIkTargetPositionTracking(other);
    }
}