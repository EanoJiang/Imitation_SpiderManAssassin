using UnityEngine;
using UnityEngine.Animations.Rigging;
using UnityEngine.Assertions;   //������


public class EnvironmentInteractionStateMachine : StateManager<EnvironmentInteractionStateMachine.EEnvironmentInteractionState>
{
    // ��������״̬
    public enum EEnvironmentInteractionState
    {
        Search,   // ����״̬
        Approach, // �ӽ�״̬
        Rise,     // ����״̬
        Touch,    // ����״̬
        Reset     // ����״̬
    }

    private EnvironmentInteractionContext _context;

    // Լ�������������
    [SerializeField] private TwoBoneIKConstraint _leftIkConstraint;
    [SerializeField] private TwoBoneIKConstraint _rightIkConstraint;
    [SerializeField] private MultiRotationConstraint _leftMultiRotationConstraint;
    [SerializeField] private MultiRotationConstraint _rightMultiRotationConstraint;
    [SerializeField] private CharacterController characterController;

    /// <summary>
    /// �����屻ѡ��ʱ����Gizmos����
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;

        // �������ײ�㴦����һ����ɫ����
        if (_context != null && _context.ClosestPointOnColliderFromShoulder != null)
        {
            Gizmos.DrawSphere(_context.ClosestPointOnColliderFromShoulder, 0.03f);
        }
    }

    void Awake()
    {
        ValidateConstraints();

        _context = new EnvironmentInteractionContext(_leftIkConstraint, _rightIkConstraint, _leftMultiRotationConstraint, _rightMultiRotationConstraint, characterController,transform.root);

        InitalizeStates();

        ConstructEnvironmentDetectionCollider();
    }

    // У�����Լ��������Ƿ���ȷ��ֵ
    private void ValidateConstraints()
    {
        Assert.IsNotNull(_leftIkConstraint, "Left IK constraint û�и�ֵ");
        Assert.IsNotNull(_rightIkConstraint, "Right IK constraint û�и�ֵ");
        Assert.IsNotNull(_leftMultiRotationConstraint, "Left multi-rotation constraint û�и�ֵ");
        Assert.IsNotNull(_rightMultiRotationConstraint, "Right multi-rotation constraint û�и�ֵ");
        Assert.IsNotNull(characterController, "characterController used to control character û�и�ֵ");
    }

    /// <summary>
    /// ��ʼ��״̬��
    /// </summary>
    private void InitalizeStates()
    {
        //���״̬
        States.Add(EEnvironmentInteractionState.Reset, new ResetState(_context, EEnvironmentInteractionState.Reset));
        States.Add(EEnvironmentInteractionState.Search, new SearchState(_context, EEnvironmentInteractionState.Search));
        States.Add(EEnvironmentInteractionState.Approach, new ApproachState(_context, EEnvironmentInteractionState.Approach));
        States.Add(EEnvironmentInteractionState.Rise, new RiseState(_context, EEnvironmentInteractionState.Rise));
        States.Add(EEnvironmentInteractionState.Touch, new TouchState(_context, EEnvironmentInteractionState.Touch));

        //���ó�ʼ״̬ΪReset
        CurrentState = States[EEnvironmentInteractionState.Reset];

    }

    /// <summary>
    /// ����һ����������õ���ײ��
    /// </summary>
    private void ConstructEnvironmentDetectionCollider()
    {
        // ��ײ���С�Ļ�׼ֵ
        float wingspan = characterController.height;

        // ����ǰ��Ϸ������Ӻ�����ײ�����
        BoxCollider boxCollider = gameObject.AddComponent<BoxCollider>();

        // ������ײ���СΪ�����壬���߳��ȵ�����չ
        boxCollider.size = new Vector3(wingspan, wingspan, wingspan);

        // ������ײ������λ��
        // ���ڽ�ɫ������������λ�ý���ƫ�ƣ�
        // Y�᷽��������չ��25%��Z�᷽��ǰ����չ��50%
        boxCollider.center = new Vector3(
            characterController.center.x,
            characterController.center.y + (.25f * wingspan),
            characterController.center.z + (.5f * wingspan)
        );

        // ����ײ������Ϊ������ģʽ�����ڼ����ײ����������ײ��Ӧ��
        boxCollider.isTrigger = true;

        _context.CharacterColliderCenterY = characterController.center.y;
    }
}