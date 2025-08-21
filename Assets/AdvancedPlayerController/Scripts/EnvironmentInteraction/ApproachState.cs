using UnityEngine;

public class ApproachState : EnvironmentInteractionState
{
    // ¹¹Ôìº¯Êý
    public ApproachState(EnvironmentInteractionContext context, EnvironmentInteractionStateMachine.EEnvironmentInteractionState estate) : base(context, estate)
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