using UnityEngine;
using UnityEngine.Animations.Rigging;

public class EnvironmentInteractionContext
{
    // 身体两侧
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
    // 根对象
    private Transform _rootTransform;

    // 记录初始位置
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
        OriginalTargetRotation = _leftIkConstraint.data.target.rotation;            // 初始的目标旋转(左右侧一样)

        CharacterShoulderHeight = leftIkConstraint.data.root.transform.position.y;  // 角色的肩部高度(左右侧一样)

        // 默认设置当前身体的侧边为无穷大
        SetCurrentSide(Vector3.positiveInfinity);
    }

    // 外部可以访问的属性
    public TwoBoneIKConstraint LeftIkConstraint => _leftIkConstraint;
    public TwoBoneIKConstraint RightIkConstraint => _rightIkConstraint;
    public MultiRotationConstraint LeftMultiRotationConstraint => _leftMultiRotationConstraint;
    public MultiRotationConstraint RightMultiRotationConstraint => _rightMultiRotationConstraint;
    public CharacterController CharacterController => _characterController;
    public Transform RootTransform => _rootTransform;
    public Vector3 CurrentOriginalTargetPosition { get; private set; }
    public Quaternion OriginalTargetRotation { get; private set; }

    // 当前交互的碰撞体
    public Collider CurrentIntersectingCollider { get; set; }
    // 当前IK约束
    public TwoBoneIKConstraint CurrentIkConstraint { get; private set; }
    // 当前多旋转约束
    public MultiRotationConstraint CurrentMultiRotationConstraint { get; private set; }
    // 当前IK控制的目标位置
    public Transform CurrentIkTargetTransform { get; private set; }
    // 当前肩部骨骼
    public Transform CurrentShoulderTransform { get; private set; }
    // 当前身体的侧边（左或右）
    public EBodySide CurrentBodySide { get; private set; }
    // 相交碰撞体的最近点——默认值设为无穷大
    public Vector3 ClosestPointOnColliderFromShoulder { get; set; } = Vector3.positiveInfinity;
    // 角色的肩部高度,用来约束Ik的高度
    public float CharacterShoulderHeight { get; private set; }
    // 交互点的Y轴偏移量，用来细调每个具体状态的交互点的高度
    public float InteractionPoint_Y_Offset { get; set; } = 0.0f;
    // 角色碰撞体的中心点的高度
    public float CharacterColliderCenterY { get; set; }
    // 角色到other碰撞体的最短距离
    public float LowestDistance { get; set; } = Mathf.Infinity;

    /// <summary>
    /// 根据传入位置，判断目标更靠近左侧还是右侧肩部，设置当前身体的侧边
    /// </summary>
    /// <param name="positionToCheck">需要检测的目标位置</param>
    public void SetCurrentSide(Vector3 positionToCheck)
    {
        // 左肩部骨骼
        Vector3 leftShoulder = _leftIkConstraint.data.root.transform.position;
        // 右肩部骨骼
        Vector3 rightShoulder = _rightIkConstraint.data.root.transform.position;

        // 标志位：目标位置是否更靠近左侧
        bool isLeftCloser = Vector3.Distance(positionToCheck, leftShoulder) <
                            Vector3.Distance(positionToCheck, rightShoulder);
        //靠近哪边就赋值哪边的Rig相关参数到CurrentXXX参数
        if (isLeftCloser)
        {
            Debug.Log("目标更靠近角色的左侧");
            CurrentBodySide = EBodySide.LEFT;
            CurrentIkConstraint = _leftIkConstraint;
            CurrentMultiRotationConstraint = _leftMultiRotationConstraint;
            CurrentOriginalTargetPosition = _leftOriginalTargetPosition;
        }
        else
        {
            Debug.Log("目标更靠近角色的右侧");
            CurrentBodySide = EBodySide.RIGHT;
            CurrentIkConstraint = _rightIkConstraint;
            CurrentMultiRotationConstraint = _rightMultiRotationConstraint;
            CurrentOriginalTargetPosition = _rightOriginalTargetPosition;
        }
        // 记录当前肩部骨骼 和 IK控制的目标位置
        CurrentShoulderTransform = CurrentIkConstraint.data.root.transform;
        CurrentIkTargetTransform = CurrentIkConstraint.data.target.transform;
    }
}