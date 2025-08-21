using UnityEngine;

public class TouchState : EnvironmentInteractionState
{
    // ¹¹Ôìº¯Êý
    public TouchState(EnvironmentInteractionContext context, EnvironmentInteractionStateMachine.EEnvironmentInteractionState estate) : base(context, estate)
    {
        EnvironmentInteractionContext Context = context;
    }
    public override void EnterState() { }
    public override void ExitState() { }
    public override void UpdateState() { }
    public override EnvironmentInteractionStateMachine.EEnvironmentInteractionState GetNextState()
    {
        return StateKey;
    }
    public override void OnTriggerEnter(Collider other) { }
    public override void OnTriggerStay(Collider other) { }
    public override void OnTriggerExit(Collider other) { }
}