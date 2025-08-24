using UnityEngine;
using UnityEngine.Animations.Rigging;

public class EnvironmentInteractionContext
{
    // ��������
    public enum EBodySide
    {
        RIGHT,
        LEFT
    }

    private TwoBoneIKConstraint _leftIkConstraint;
    private TwoBoneIKConstraint _rightIkConstraint;
    private MultiRotationConstraint _leftMultiRotationConstraint;
    private MultiRotationConstraint _rightMultiRotationConstraint;
    private CharacterController _characterController;
    // ������
    private Transform _rootTransform;

    // ��¼��ʼλ��
    private Vector3 _leftOriginalTargetPosition;
    private Vector3 _rightOriginalTargetPosition;

    public EnvironmentInteractionContext(
        TwoBoneIKConstraint leftIkConstraint,
        TwoBoneIKConstraint rightIkConstraint,
        MultiRotationConstraint leftMultiRotationConstraint,
        MultiRotationConstraint rightMultiRotationConstraint,
        CharacterController characterController,
        Transform rootTransform)
    {
        _leftIkConstraint = leftIkConstraint;
        _rightIkConstraint = rightIkConstraint;
        _leftMultiRotationConstraint = leftMultiRotationConstraint;
        _rightMultiRotationConstraint = rightMultiRotationConstraint;
        _characterController = characterController;
        _rootTransform = rootTransform;

        _leftOriginalTargetPosition = _leftIkConstraint.data.target.transform.localPosition;
        _rightOriginalTargetPosition = _rightIkConstraint.data.target.transform.localPosition;
        OriginalTargetRotation = _leftIkConstraint.data.target.rotation;            // ��ʼ��Ŀ����ת(���Ҳ�һ��)

        CharacterShoulderHeight = leftIkConstraint.data.root.transform.position.y;  // ��ɫ�ļ粿�߶�(���Ҳ�һ��)

        // Ĭ�����õ�ǰ����Ĳ��Ϊ�����
        SetCurrentSide(Vector3.positiveInfinity);
    }

    // �ⲿ���Է��ʵ�����
    public TwoBoneIKConstraint LeftIkConstraint => _leftIkConstraint;
    public TwoBoneIKConstraint RightIkConstraint => _rightIkConstraint;
    public MultiRotationConstraint LeftMultiRotationConstraint => _leftMultiRotationConstraint;
    public MultiRotationConstraint RightMultiRotationConstraint => _rightMultiRotationConstraint;
    public CharacterController CharacterController => _characterController;
    public Transform RootTransform => _rootTransform;
    public Vector3 CurrentOriginalTargetPosition { get; private set; }
    public Quaternion OriginalTargetRotation { get; private set; }

    // ��ǰ��������ײ��
    public Collider CurrentIntersectingCollider { get; set; }
    // ��ǰIKԼ��
    public TwoBoneIKConstraint CurrentIkConstraint { get; private set; }
    // ��ǰ����תԼ��
    public MultiRotationConstraint CurrentMultiRotationConstraint { get; private set; }
    // ��ǰIK���Ƶ�Ŀ��λ��
    public Transform CurrentIkTargetTransform { get; private set; }
    // ��ǰ�粿����
    public Transform CurrentShoulderTransform { get; private set; }
    // ��ǰ����Ĳ�ߣ�����ң�
    public EBodySide CurrentBodySide { get; private set; }
    // �ཻ��ײ�������㡪��Ĭ��ֵ��Ϊ�����
    public Vector3 ClosestPointOnColliderFromShoulder { get; set; } = Vector3.positiveInfinity;
    // ��ɫ�ļ粿�߶�,����Լ��Ik�ĸ߶�
    public float CharacterShoulderHeight { get; private set; }
    // �������Y��ƫ����������ϸ��ÿ������״̬�Ľ�����ĸ߶�
    public float InteractionPoint_Y_Offset { get; set; } = 0.0f;
    // ��ɫ��ײ������ĵ�ĸ߶�
    public float CharacterColliderCenterY { get; set; }
    // ��ɫ��other��ײ�����̾���
    public float LowestDistance { get; set; } = Mathf.Infinity;

    /// <summary>
    /// ���ݴ���λ�ã��ж�Ŀ���������໹���Ҳ�粿�����õ�ǰ����Ĳ��
    /// </summary>
    /// <param name="positionToCheck">��Ҫ����Ŀ��λ��</param>
    public void SetCurrentSide(Vector3 positionToCheck)
    {
        // ��粿����
        Vector3 leftShoulder = _leftIkConstraint.data.root.transform.position;
        // �Ҽ粿����
        Vector3 rightShoulder = _rightIkConstraint.data.root.transform.position;

        // ��־λ��Ŀ��λ���Ƿ���������
        bool isLeftCloser = Vector3.Distance(positionToCheck, leftShoulder) <
                            Vector3.Distance(positionToCheck, rightShoulder);
        //�����ı߾͸�ֵ�ıߵ�Rig��ز�����CurrentXXX����
        if (isLeftCloser)
        {
            Debug.Log("Ŀ���������ɫ�����");
            CurrentBodySide = EBodySide.LEFT;
            CurrentIkConstraint = _leftIkConstraint;
            CurrentMultiRotationConstraint = _leftMultiRotationConstraint;
            CurrentOriginalTargetPosition = _leftOriginalTargetPosition;
        }
        else
        {
            Debug.Log("Ŀ���������ɫ���Ҳ�");
            CurrentBodySide = EBodySide.RIGHT;
            CurrentIkConstraint = _rightIkConstraint;
            CurrentMultiRotationConstraint = _rightMultiRotationConstraint;
            CurrentOriginalTargetPosition = _rightOriginalTargetPosition;
        }
        // ��¼��ǰ�粿���� �� IK���Ƶ�Ŀ��λ��
        CurrentShoulderTransform = CurrentIkConstraint.data.root.transform;
        CurrentIkTargetTransform = CurrentIkConstraint.data.target.transform;
    }
}