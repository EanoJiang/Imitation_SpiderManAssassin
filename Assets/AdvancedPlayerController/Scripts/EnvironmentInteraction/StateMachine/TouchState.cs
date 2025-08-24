using UnityEngine;

public class TouchState : EnvironmentInteractionState
{
    public float _elapsedTime = 0.0f;
    public float _resetThreshold = 0.5f;    // ������ֵ��������ʱ�����л��� Reset ״̬

    public TouchState(EnvironmentInteractionContext context,EnvironmentInteractionStateMachine.EEnvironmentInteractionState estate): base(context, estate)
    {
        EnvironmentInteractionContext Context = context; 
    }

    public override void EnterState()
    {
        // ���ü�ʱ��
        _elapsedTime = 0.0f;
        Debug.Log("���� Touch State");
    }

    public override void ExitState()
    {
        Debug.Log("�˳� Touch State");
    }

    public override void UpdateState()
    {
        _elapsedTime += Time.deltaTime;
    }

    public override EnvironmentInteractionStateMachine.EEnvironmentInteractionState GetNextState()
    {
        if (_elapsedTime > _resetThreshold || CheckShouldReset())
        {
            // �л��� ResetState
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