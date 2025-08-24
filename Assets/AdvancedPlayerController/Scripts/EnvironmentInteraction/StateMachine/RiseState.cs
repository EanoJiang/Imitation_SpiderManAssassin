using UnityEngine;

public class RiseState : EnvironmentInteractionState
{
    float _elapsedTimer = 0.0f;         // ������ʱ�䣬���ڿ��Ʋ�ֵ����
    float _lerpDuration = 5.0f;         // ��ֵ��ʱ��������״̬���ɵġ���/�족
    float _riseWeight = 1.0f;           // Ȩ��Ŀ��ֵ������IK����תԼ���Ĺ���

    Quaternion _targetHandRotation;   // �ֲ���Ŀ����ת�Ƕȣ��������ֲ����Ͻ����������
    float _maxDistance = 0.5f;         // ���߼���������
    protected LayerMask _interactableLayerMask = LayerMask.GetMask("Interactable");
    float _rotationSpeed = 1000f;      // ��ת�ٶ�

    // �����ж��Ƿ��ܹ�����TouchState״̬����ֵ
    float _touchDistanceThreshold = 0.05f;  // TouchState�ľ�����ֵ
    float _touchTimeThreshold = 1f;         // TouchState�ĳ���ʱ����ֵ

    public RiseState(EnvironmentInteractionContext context, EnvironmentInteractionStateMachine.EEnvironmentInteractionState estate): base(context, estate)
    {
        EnvironmentInteractionContext Context = context;
    }

    public override void EnterState()
    {
        // ���ü�ʱ��
        _elapsedTimer = 0.0f;
        Debug.Log("���� Rise State");
    }
    public override void ExitState()
    {
        Debug.Log("�˳� Rise State");
    }

    public override void UpdateState()
    {
        // �����������ֲ���ת�Ƕ�
        CalculateExpectedHandRotation();

        // 1. ��ײ���y��߶�ƫ�� ƽ�����µ� �����ײ���Y����
        Context.InteractionPoint_Y_Offset = Mathf.Lerp(
            Context.InteractionPoint_Y_Offset,
            Context.ClosestPointOnColliderFromShoulder.y,
            _elapsedTimer / _lerpDuration
        );

        // 2. ����IKԼ��CurrentIkConstraint��Ȩ�أ��ӵ�ǰȨ�ص�Ŀ��Ȩ��_riseWeight
        Context.CurrentIkConstraint.weight = Mathf.Lerp(
            Context.CurrentIkConstraint.weight,
            _riseWeight,
            _elapsedTimer / _lerpDuration
        );

        // 3. ���¶���תԼ��CurrentMultiRotationConstraint��Ȩ�أ��ӵ�ǰȨ�ص�Ŀ��Ȩ��_riseWeight
        Context.CurrentMultiRotationConstraint.weight = Mathf.Lerp(
            Context.CurrentMultiRotationConstraint.weight,
            _riseWeight,
            _elapsedTimer / _lerpDuration
        );

        // 4. �� IKĿ������� ���� Ԥ�ڵ��ֲ���ת�Ƕ� ƽ����ת
        Context.CurrentIkTargetTransform.rotation = Quaternion.RotateTowards(
            Context.CurrentIkTargetTransform.rotation,
            _targetHandRotation,
            _rotationSpeed * Time.deltaTime
        );

        _elapsedTimer += Time.deltaTime;
    }

    /// <summary>
    /// �����������ֲ���ת�Ƕȣ��������ֲ����Ͻ����������
    /// </summary>
    private void CalculateExpectedHandRotation()
    {
        // 1. ��ȡ��ʼ�㣨�粿λ�ã����յ㣨�����ײ�㣩
        Vector3 startPos = Context.CurrentShoulderTransform.position;
        Vector3 endPos = Context.ClosestPointOnColliderFromShoulder;

        // 2. ���߷��򣺼粿ָ����ײ��Ĺ�һ����������
        Vector3 direction = (endPos - startPos).normalized;

        // 3. ��������
        if (Physics.Raycast(startPos, direction, out RaycastHit hit, _maxDistance, _interactableLayerMask))
        {
            // ��ײ��ı��淨��
            Vector3 surfaceNormal = hit.normal;

            // Ŀ�곯������淨���෴�����ֲ�������ײ��ı��淨�ߵķ�����
            Vector3 targetForward = -surfaceNormal;

            // �ֲ���Ŀ����ת������Ŀ�곯����ͬ��������Y����ת90��
            _targetHandRotation = Quaternion.LookRotation(targetForward, Vector3.up);
        }
    }


    public override EnvironmentInteractionStateMachine.EEnvironmentInteractionState GetNextState()
    {
        if (CheckShouldReset())
        {
            // �л���Reset״̬
            return EnvironmentInteractionStateMachine.EEnvironmentInteractionState.Reset;
        }
        // ��־λ�� �Ƿ�ﵽ�ܹ�Touch�ľ�����ֵ
        bool isCloseToTouch = Vector3.Distance(
                Context.CurrentIkTargetTransform.position,
                Context.ClosestPointOnColliderFromShoulder
            ) < _touchDistanceThreshold;
        // ��־λ�� �Ƿ�ﵽ�ܹ�Touch�ĳ���ʱ����ֵ
        bool isTouchTime = _elapsedTimer >= _touchTimeThreshold;

        if (isCloseToTouch && isTouchTime)
        {
            // �л���Touch״̬
            return EnvironmentInteractionStateMachine.EEnvironmentInteractionState.Touch;
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