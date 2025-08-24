using UnityEngine;

public class TouchState : EnvironmentInteractionState
{
    public float _elapsedTime = 0.0f;
    public float _resetThreshold = 0.5f;    // 重置阈值：超过该时长就切换到 Reset 状态

    public TouchState(EnvironmentInteractionContext context,EnvironmentInteractionStateMachine.EEnvironmentInteractionState estate): base(context, estate)
    {
        EnvironmentInteractionContext Context = context; 
    }

    public override void EnterState()
    {
        // 重置计时器
        _elapsedTime = 0.0f;
        Debug.Log("进入 Touch State");
    }

    public override void ExitState()
    {
        Debug.Log("退出 Touch State");
    }

    public override void UpdateState()
    {
        _elapsedTime += Time.deltaTime;
    }

    public override EnvironmentInteractionStateMachine.EEnvironmentInteractionState GetNextState()
    {
        if (_elapsedTime > _resetThreshold || CheckShouldReset())
        {
            // 切换到 ResetState
            return EnvironmentInteractionStateMachine.EEnvironmentInteractionState.Reset;
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