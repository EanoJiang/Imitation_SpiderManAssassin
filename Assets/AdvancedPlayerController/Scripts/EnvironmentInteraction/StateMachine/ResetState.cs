using UnityEngine;

public class ResetState : EnvironmentInteractionState
{
    // ����ʱ���ʱ��
    float _elapsedTimer = 0.0f;
    // ����ʱ�����ֵ
    float _resetDuration = 1.0f;
    // ƽ�����ɵĳ���ʱ��
    float _lerpDuration = 10.0f;
    // ת���ٶ�
    float _rotationSpeed = 500f;

    // ���캯��
    public ResetState(EnvironmentInteractionContext context, EnvironmentInteractionStateMachine.EEnvironmentInteractionState estate) : base(context, estate)
    {
        EnvironmentInteractionContext Context = context;
    }
    public override void EnterState(){
        // ���� ����ʱ���ʱ��
        _elapsedTimer = 0.0f;
        // ���� �����ײ�� �� ��ǰ��ײ��
        Context.ClosestPointOnColliderFromShoulder = Vector3.positiveInfinity;
        Context.CurrentIntersectingCollider = null;

        Debug.Log("���� ResetState");
    }
    public override void ExitState() { 
        Debug.Log("�˳� ResetState");
    }
    public override void UpdateState() {
        _elapsedTimer += Time.deltaTime;
        // ��ײ��� Y ��ƫ�ƣ�ƽ�����ɵ���ɫ��ײ�����ĵĸ߶�
        Context.InteractionPoint_Y_Offset = Mathf.Lerp(
            Context.InteractionPoint_Y_Offset, 
            Context.CharacterColliderCenterY, 
            _elapsedTimer / _lerpDuration);

        // ����Ȩ�أ�ƽ�����õ�ǰ��Ȩ��
        //IkConstraint��
        Context.CurrentIkConstraint.weight = Mathf.Lerp(
            Context.CurrentIkConstraint.weight,
            0,
            _elapsedTimer / _lerpDuration);
        //MultiRotationConstraint��
        Context.CurrentMultiRotationConstraint.weight = Mathf.Lerp(
            Context.CurrentMultiRotationConstraint.weight,
            0,
            _elapsedTimer / _lerpDuration);

        // ikĿ�����������Ҳ�ص�ԭ����position��rotation
        Context.CurrentIkTargetTransform.localPosition = Vector3.Lerp(
            Context.CurrentIkTargetTransform.localPosition,
            Context.CurrentOriginalTargetPosition,
            _elapsedTimer / _lerpDuration
        );

        Context.CurrentIkTargetTransform.rotation = Quaternion.RotateTowards(
            Context.CurrentIkTargetTransform.rotation,
            Context.OriginalTargetRotation,
            _rotationSpeed * Time.deltaTime
        );
    }
    public override EnvironmentInteractionStateMachine.EEnvironmentInteractionState GetNextState() 
    {
        // ��־λ���Ƿ������ƶ�(�Ƿ���Movement����)
        bool isMoving = GameInputManager.Instance.Movement != Vector2.zero;
        //ֻ�е�����ʱ�䳬����ֵ���ҽ�ɫ�����ƶ�ʱ���Ż��л��� SearchState
        if(_elapsedTimer > _resetDuration && isMoving)
        {
            // ��һ��״̬Ϊ SearchState
            return EnvironmentInteractionStateMachine.EEnvironmentInteractionState.Search;
        }
        return StateKey; 
    }
    // ResetState ״̬�²�������κδ�������Callback
    public override void OnTriggerEnter(Collider other) { }
    public override void OnTriggerStay(Collider other) { }
    public override void OnTriggerExit(Collider other) { }
}
