using UnityEngine;

public class ApproachState : EnvironmentInteractionState
{
    // �ӽ�״̬�ļ�ʱ��
    float _elapsedTimer = 0.0f;
    // ����ʱ��
    float _lerpDuration = 5.0f;
    // �ӽ�״̬����ʱ��,�����ͻص�ResetState״̬
    float _approachDuration = 2.0f;
    // �ӽ�״̬��IkConstraintĿ��Ȩ��
    float _approachWeight = 0.5f;
    // �ӽ�״̬��MultiRotationConstraintĿ����תȨ��
    float _approachRotationWeight = 0.75f;
    // ��ת�ٶ�
    float _rotationSpeed = 500f;
    // �Ƿ����л�������״̬�ľ�����ֵ
    float _riseDistanceThreshold = 0.5f;

    // ���캯��
    public ApproachState(EnvironmentInteractionContext context, EnvironmentInteractionStateMachine.EEnvironmentInteractionState estate) : base(context, estate)
    {
        EnvironmentInteractionContext Context = context;
    }
    public override void EnterState() {
        // ���ü�ʱ��
        _elapsedTimer = 0.0f;
        Debug.Log("���� Approach State");
    }
    public override void ExitState()
    {
        Debug.Log("�˳� Approach State");
    }
    public override void UpdateState() { 
        //Ŀ�곯�������Ƴ�����棬forwad=���£�up=��ɫ�ĳ���
        Quaternion targetGroundRotation = Quaternion.LookRotation(-Vector3.up, Context.RootTransform.forward);

        _elapsedTimer += Time.deltaTime;
        
        // ����������תik�Ŀ��������� ��ת�� Ŀ�곯��
        Context.CurrentIkTargetTransform.rotation = Quaternion.RotateTowards(
            Context.CurrentIkTargetTransform.rotation, 
            targetGroundRotation, 
            _rotationSpeed * Time.deltaTime);

        // ����Ȩ�أ��ӵ�ǰ��Ȩ�ع��ɵ��ӽ�״̬�Ķ�ӦȨ��
        //MultiRotationConstraint��
        Context.CurrentMultiRotationConstraint.weight = Mathf.Lerp(
            Context.CurrentMultiRotationConstraint.weight, 
            _approachRotationWeight, 
            _elapsedTimer / _lerpDuration);
        //IkConstraint��
        Context.CurrentIkConstraint.weight = Mathf.Lerp(
            Context.CurrentIkConstraint.weight, 
            _approachWeight, 
            _elapsedTimer / _lerpDuration);
    }
    public override EnvironmentInteractionStateMachine.EEnvironmentInteractionState GetNextState()
    {
        // �Ƿ񳬹�Approach״̬�ĳ���ʱ��
        bool isOverStateLifeTime = _elapsedTimer > _approachDuration;
        if (isOverStateLifeTime || CheckShouldReset())
        {
            // �л���Reset״̬
            return EnvironmentInteractionStateMachine.EEnvironmentInteractionState.Reset;
        }

        // �Ƿ����ֱ����ַ�Χ��
        bool isWithArmsReach = Vector3.Distance(Context.ClosestPointOnColliderFromShoulder, Context.CurrentShoulderTransform.position) < _riseDistanceThreshold;
        bool isClosestPointOnColliderValid = Context.ClosestPointOnColliderFromShoulder != Vector3.positiveInfinity;
        if (isWithArmsReach && isClosestPointOnColliderValid)
        {
            // �л�������״̬
            return EnvironmentInteractionStateMachine.EEnvironmentInteractionState.Rise;
        }
        return StateKey;
    }
    public override void OnTriggerEnter(Collider other) {
        StartIkTargetPositionTracking(other);
        }
    public override void OnTriggerStay(Collider other) {
        UpdateIkTargetPosition(other);
    }
    public override void OnTriggerExit(Collider other) { 
        ResetIkTargetPositionTracking(other);
    }
}