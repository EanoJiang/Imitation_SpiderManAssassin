using UnityEngine;

public class ResetState : EnvironmentInteractionState
{
    // ���캯��
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
        // ��һ��״̬Ϊ SearchState
        return EnvironmentInteractionStateMachine.EEnvironmentInteractionState.Search;
        //return StateKey; 
    }
    // ResetState ״̬�²�������κδ�������Callback
    public override void OnTriggerEnter(Collider other) { }
    public override void OnTriggerStay(Collider other) { }
    public override void OnTriggerExit(Collider other) { }
}
