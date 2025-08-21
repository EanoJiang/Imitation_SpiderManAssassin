using UnityEngine;

public class ResetState : EnvironmentInteractionState
{
    // 构造函数
    public ResetState(EnvironmentInteractionContext context, EnvironmentInteractionStateMachine.EEnvironmentInteractionState estate) : base(context, estate)
    {
        EnvironmentInteractionContext Context = context;
    }
    public override void EnterState(){
        Debug.Log("ResetState EnterState");
    }
    public override void ExitState() { 
        Debug.Log("ResetState ExitState");
    }
    public override void UpdateState() { 
        Debug.Log("ResetState UpdateState");
    }
    public override EnvironmentInteractionStateMachine.EEnvironmentInteractionState GetNextState() 
    { 
        // 下一个状态为 SearchState
        return EnvironmentInteractionStateMachine.EEnvironmentInteractionState.Search;
        //return StateKey; 
    }
    // ResetState 状态下不会调用任何触发器的Callback
    public override void OnTriggerEnter(Collider other) { }
    public override void OnTriggerStay(Collider other) { }
    public override void OnTriggerExit(Collider other) { }
}
