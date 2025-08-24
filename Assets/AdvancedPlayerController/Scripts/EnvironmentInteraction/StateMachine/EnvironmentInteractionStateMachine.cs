using UnityEngine;
using UnityEngine.Animations.Rigging;
using UnityEngine.Assertions;   //调试用


public class EnvironmentInteractionStateMachine : StateManager<EnvironmentInteractionStateMachine.EEnvironmentInteractionState>
{
    // 环境交互状态
    public enum EEnvironmentInteractionState
    {
        Search,   // 搜索状态
        Approach, // 接近状态
        Rise,     // 起身状态
        Touch,    // 触碰状态
        Reset     // 重置状态
    }

    private EnvironmentInteractionContext _context;

    // 约束、组件等引用
    [SerializeField] private TwoBoneIKConstraint _leftIkConstraint;
    [SerializeField] private TwoBoneIKConstraint _rightIkConstraint;
    [SerializeField] private MultiRotationConstraint _leftMultiRotationConstraint;
    [SerializeField] private MultiRotationConstraint _rightMultiRotationConstraint;
    [SerializeField] private CharacterController characterController;

    /// <summary>
    /// 当物体被选中时调用Gizmos绘制
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;

        // 在最近碰撞点处绘制一个红色的球
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

    // 校验各类约束、组件是否正确赋值
    private void ValidateConstraints()
    {
        Assert.IsNotNull(_leftIkConstraint, "Left IK constraint 没有赋值");
        Assert.IsNotNull(_rightIkConstraint, "Right IK constraint 没有赋值");
        Assert.IsNotNull(_leftMultiRotationConstraint, "Left multi-rotation constraint 没有赋值");
        Assert.IsNotNull(_rightMultiRotationConstraint, "Right multi-rotation constraint 没有赋值");
        Assert.IsNotNull(characterController, "characterController used to control character 没有赋值");
    }

    /// <summary>
    /// 初始化状态机
    /// </summary>
    private void InitalizeStates()
    {
        //添加状态
        States.Add(EEnvironmentInteractionState.Reset, new ResetState(_context, EEnvironmentInteractionState.Reset));
        States.Add(EEnvironmentInteractionState.Search, new SearchState(_context, EEnvironmentInteractionState.Search));
        States.Add(EEnvironmentInteractionState.Approach, new ApproachState(_context, EEnvironmentInteractionState.Approach));
        States.Add(EEnvironmentInteractionState.Rise, new RiseState(_context, EEnvironmentInteractionState.Rise));
        States.Add(EEnvironmentInteractionState.Touch, new TouchState(_context, EEnvironmentInteractionState.Touch));

        //设置初始状态为Reset
        CurrentState = States[EEnvironmentInteractionState.Reset];

    }

    /// <summary>
    /// 创建一个环境检测用的碰撞体
    /// </summary>
    private void ConstructEnvironmentDetectionCollider()
    {
        // 碰撞体大小的基准值
        float wingspan = characterController.height;

        // 给当前游戏对象添加盒型碰撞体组件
        BoxCollider boxCollider = gameObject.AddComponent<BoxCollider>();

        // 设置碰撞体大小为立方体，各边长度等于翼展
        boxCollider.size = new Vector3(wingspan, wingspan, wingspan);

        // 设置碰撞体中心位置
        // 基于角色控制器的中心位置进行偏移：
        // Y轴方向上移翼展的25%，Z轴方向前移翼展的50%
        boxCollider.center = new Vector3(
            characterController.center.x,
            characterController.center.y + (.25f * wingspan),
            characterController.center.z + (.5f * wingspan)
        );

        // 将碰撞体设置为触发器模式（用于检测碰撞而非物理碰撞响应）
        boxCollider.isTrigger = true;

        _context.CharacterColliderCenterY = characterController.center.y;
    }
}