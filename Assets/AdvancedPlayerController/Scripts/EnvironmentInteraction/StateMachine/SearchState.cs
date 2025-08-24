using UnityEngine;

public class SearchState : EnvironmentInteractionState
{
    // �ӽ���ײ��ľ�����ֵ
    public float _approachDistanceThreshold = 2.0f;

    // ���캯��
    public SearchState(EnvironmentInteractionContext context, EnvironmentInteractionStateMachine.EEnvironmentInteractionState estate) : base(context, estate)
    {
        EnvironmentInteractionContext Context = context;
    }
    public override void EnterState() {
        Debug.Log("���� Search State");}
    public override void ExitState() { 
        Debug.Log("�˳� Search State");}
    public override void UpdateState() { 
        }
    public override EnvironmentInteractionStateMachine.EEnvironmentInteractionState GetNextState()
    {
        if (CheckShouldReset())
        {
            // �л���Reset״̬
            return EnvironmentInteractionStateMachine.EEnvironmentInteractionState.Reset;
        }

        // ��־λ���Ƿ�ӽ�Ŀ��
        bool isCloseToTarget = Vector3.Distance(Context.ClosestPointOnColliderFromShoulder, Context.RootTransform.position) < _approachDistanceThreshold;
        // ��־λ���Ƿ��������ײ��(ֻҪ��������󣬾��������ײ��)
        bool isClosestPointOnColliderValid = Context.ClosestPointOnColliderFromShoulder != Vector3.positiveInfinity;
        if (isCloseToTarget && isClosestPointOnColliderValid)
        {
            // �л���Approach״̬
            return EnvironmentInteractionStateMachine.EEnvironmentInteractionState.Approach;
        }
        return StateKey;
    }
    public override void OnTriggerEnter(Collider other) {
        // ��������״̬����ʼ����Ŀ��λ��
        StartIkTargetPositionTracking(other);
    }
    public override void OnTriggerStay(Collider other) {
        // ����Ŀ��λ��
        UpdateIkTargetPosition(other);
    }
    public override void OnTriggerExit(Collider other) {
        // �˳�����״̬��ֹͣ����Ŀ��λ��
        ResetIkTargetPositionTracking(other);
    }
}