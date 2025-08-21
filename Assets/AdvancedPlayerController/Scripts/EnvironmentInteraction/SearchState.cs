using UnityEngine;

public class SearchState : EnvironmentInteractionState
{
    // ���캯��
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
        Debug.Log("Trigger��Enter");
        // ��������״̬����ʼ����Ŀ��λ��
        StartIkTargetPositionTracking(other);
    }
    public override void OnTriggerStay(Collider other) {
        // ����Ŀ��λ��
        UpdateIkTargetPosition(other);
    }
    public override void OnTriggerExit(Collider other) {
        Debug.Log("Trigger��Exit");
        // �˳�����״̬��ֹͣ����Ŀ��λ��
        ResetIkTargetPositionTracking(other);
    }
}