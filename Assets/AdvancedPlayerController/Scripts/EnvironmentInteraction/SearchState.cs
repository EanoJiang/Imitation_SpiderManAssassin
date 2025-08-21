using UnityEngine;

public class SearchState : EnvironmentInteractionState
{
    // 构造函数
    public SearchState(EnvironmentInteractionContext context, EnvironmentInteractionStateMachine.EEnvironmentInteractionState estate) : base(context, estate)
    {
        EnvironmentInteractionContext Context = context;
    }
    public override void EnterState() {
        Debug.Log("Enter Search State");}
    public override void ExitState() { 
        Debug.Log("Exit Search State");}
    public override void UpdateState() { 
        }
    public override EnvironmentInteractionStateMachine.EEnvironmentInteractionState GetNextState()
    {
        return StateKey;
    }
    public override void OnTriggerEnter(Collider other) {
        Debug.Log("Trigger：Enter");
        // 进入搜索状态，开始跟踪目标位置
        StartIkTargetPositionTracking(other);
    }
    public override void OnTriggerStay(Collider other) {
        // 跟踪目标位置
        UpdateIkTargetPosition(other);
    }
    public override void OnTriggerExit(Collider other) {
        Debug.Log("Trigger：Exit");
        // 退出搜索状态，停止跟踪目标位置
        ResetIkTargetPositionTracking(other);
    }
}