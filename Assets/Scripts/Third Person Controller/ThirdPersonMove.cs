using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class ThirdPersonMove : MonoBehaviour
{
    //锁定目标
    public Transform LockTarget;

    [Header("输入参数")]
    public float dashSpeed = 1.5f;
    public bool inputEnabled = true;
    
    // 混合树参数
    [Header("混合树参数")]
    public float blendTreeTransitionTime = 0.2f;          // 混合树切换时间
    
    // 状态机控制参数
    [Header("状态机控制参数")]
    public string moveStateParameter = "IsLockMode";      // 控制移动状态混合树的参数名
    
    // 锁定转向参数
    [Header("锁定转向参数")]
    public float lockRotationSpeed = 10f;      // 锁定时的旋转速度
    public float minLockDistance = 1f;         // 最小锁定距离
    public float maxLockDistance = 20f;        // 最大锁定距离

    // 自动锁定参数
    [Header("自动锁定参数")]
    public float lockDetectionRadius = 30f;    // 锁定检测半径
    public LayerMask enemyLayerMask = -1;      // 敌人层级遮罩

    GameObject mainCamera;
    Animator animator;
    CharacterController characterController;
    
    // 状态机相关变量
    private bool isLockMode = false;
    
    // 自动锁定相关变量
    private List<Transform> availableTargets = new List<Transform>();
    private int currentTargetIndex = -1;
    private bool isLocking = false;

    void Start()
    {
        //获取当前场景的主相机
        if (mainCamera == null)
        {
            mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
        }
    }


    void Awake()
    {
        animator = GetComponent<Animator>();
        characterController = GetComponent<CharacterController>();
        //速度控制
        animator.speed /= animator.humanScale;
    }


    // 目标旋转角度
    float targetRotation = 0.0f;
    // 平滑旋转时间
    public float RotationSmoothTime = 0.1f;
    // 平滑旋转速度
    float rotationVelocity;
    void Update()
    {
        if (!inputEnabled) return;

        #region 锁敌
        // 检测鼠标中键输入
        if (Input.GetMouseButtonDown(2)) // 鼠标中键
        {
            ToggleAutoLock();
        }

        // 验证当前锁定目标是否仍然有效
        if (isLocking)
        {
            ValidateCurrentTarget();
        }

        #endregion

        #region 疾跑
        if (Input.GetKey(KeyCode.LeftShift))
        {
            //animator.speed = moveSpeed * 1.5f / animator.humanScale;
            //只改变走路的速度，在混合树上乘这个SprintSpeed参数
            animator.SetFloat("SprintSpeed", dashSpeed / animator.humanScale);
        }
        else
        {
            animator.SetFloat("SprintSpeed", 1 / animator.humanScale);
        }
        #endregion

        // 检查是否进入或退出锁定模式
        bool shouldBeLockMode = LockTarget != null;
        if (shouldBeLockMode != isLockMode)
        {
            isLockMode = shouldBeLockMode;
        }
        
        // 更新混合树权重
        UpdateBlendTreeWeight();
        
        #region 水平方向
        if (LockTarget == null)
            FreeMove();
        else
            LockMove();
        #endregion

    }

    /// <summary>
    /// 更新混合树权重
    /// </summary>
    private void UpdateBlendTreeWeight()
    {
        // 状态机参数控制
        animator.SetBool(moveStateParameter, isLockMode);
    }

    /// <summary>
    /// 新版Input输入系统————移动
    /// </summary>
    Vector2 moveAmount;
    void OnMove(InputValue value)
    {
        moveAmount = value.Get<Vector2>();

    }

    /// <summary>
    /// 自由移动，方向
    /// </summary>
    private void FreeMove()
    {
        if (moveAmount != Vector2.zero)
        {
            // 计算水平方向输入方向，将其转换为世界空间坐标系——x和z是水平坐标轴
            Vector3 inputDir = new Vector3(moveAmount.x, 0.0f, moveAmount.y).normalized;
            // 计算目标朝向
            targetRotation = Mathf.Atan2(inputDir.x, inputDir.z) * Mathf.Rad2Deg +
                                  mainCamera.transform.eulerAngles.y;
            // 平稳地旋转玩家，使其面向他们移动的方向。
            float rotation = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetRotation, ref rotationVelocity, RotationSmoothTime);
            // 旋转玩家
            transform.rotation = Quaternion.Euler(0.0f, rotation, 0.0f);
        }
        //Y轴输入值——只影响前进的速度控制，朝向由上面的if中的语句决定
        var axisY = animator.GetFloat("AxisY");
        //平滑过渡到Y轴目标值
        axisY = Mathf.MoveTowards(axisY, moveAmount.magnitude, Time.deltaTime * 5f);
        animator.SetFloat("AxisY", axisY);

    }

    /// <summary>
    /// 锁定目标时的移动——八向移动
    /// </summary>
    private void LockMove()
    {
        if (LockTarget == null) return;
        
        //角色位置到目标的向量
        var dir = LockTarget.position - transform.position;
        float distance = dir.magnitude;
        
        // 检查距离是否在有效范围内
        if (distance < minLockDistance || distance > maxLockDistance)
        {
            // 距离太近或太远时，不进行锁定转向
            return;
        }
        
        //去除y轴的影响
        dir.y = 0;
        
        // 确保方向向量不为零
        if (dir.magnitude > 0.1f)
        {
            //目标朝向
            var targetRotation = Quaternion.LookRotation(dir);
            //从 当前朝向到目标朝向 平滑旋转
            // 使用lockRotationSpeed作为旋转速度
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, lockRotationSpeed * Time.deltaTime);
        }
        
        // 锁定移动混合树参数
        var AxisX = animator.GetFloat("AxisX");
        var AxisY = animator.GetFloat("AxisY");
        //平滑改变轴向
        AxisX = Mathf.MoveTowards(AxisX, moveAmount.x, Time.deltaTime * 5f);
        AxisY = Mathf.MoveTowards(AxisY, moveAmount.y, Time.deltaTime * 5f);
        //更新锁定移动动画参数
        animator.SetFloat("AxisX", AxisX);
        animator.SetFloat("AxisY", AxisY);
    }

    /// <summary>
    /// 切换自动锁定状态
    /// </summary>
    private void ToggleAutoLock()
    {
        if (!isLocking)
        {
            // 开始锁定模式
            FindAvailableTargets();
            if (availableTargets.Count > 0)
            {
                currentTargetIndex = 0;
                LockTarget = availableTargets[currentTargetIndex];
                isLocking = true;
                Debug.Log($"锁定目标: {LockTarget.name}");
            }
        }
        else
        {
            // 切换到下一个目标
            if (availableTargets.Count > 1)
            {
                currentTargetIndex = (currentTargetIndex + 1) % availableTargets.Count;
                LockTarget = availableTargets[currentTargetIndex];
                Debug.Log($"切换到目标: {LockTarget.name}");
            }
            else
            {
                // 只有一个目标或没有目标，退出锁定模式
                ExitLockMode();
            }
        }
    }

    /// <summary>
    /// 查找可用的锁定目标
    /// </summary>
    private void FindAvailableTargets()
    {
        availableTargets.Clear();
        
        // 获取检测半径内的所有碰撞体
        Collider[] colliders = Physics.OverlapSphere(transform.position, lockDetectionRadius, enemyLayerMask);
        
        foreach (Collider col in colliders)
        {
            // 检查对象名称是否包含"Enemy"
            if (col.name.ToLower().Contains("enemy"))
            {
                // 检查距离是否在有效范围内
                float distance = Vector3.Distance(transform.position, col.transform.position);
                if (distance >= minLockDistance && distance <= maxLockDistance)
                {
                    availableTargets.Add(col.transform);
                }
            }
        }
        
        // 按距离排序，最近的优先
        availableTargets.Sort((a, b) => 
            Vector3.Distance(transform.position, a.position).CompareTo(
                Vector3.Distance(transform.position, b.position)));
    }

    /// <summary>
    /// 退出锁定模式
    /// </summary>
    private void ExitLockMode()
    {
        LockTarget = null;
        isLocking = false;
        currentTargetIndex = -1;
        availableTargets.Clear();
        Debug.Log("退出锁定模式");
    }

    /// <summary>
    /// 检查当前锁定目标是否仍然有效
    /// </summary>
    private void ValidateCurrentTarget()
    {
        if (LockTarget == null)
        {
            ExitLockMode();
            return;
        }
        
        // 检查目标是否仍然存在
        if (LockTarget.gameObject == null)
        {
            ExitLockMode();
            return;
        }
        
        // 检查距离是否仍然有效
        float distance = Vector3.Distance(transform.position, LockTarget.position);
        if (distance < minLockDistance || distance > maxLockDistance)
        {
            ExitLockMode();
            return;
        }
    }

    /// <summary>
    /// 手动设置锁定目标
    /// </summary>
    /// <param name="target">要锁定的目标</param>
    public void SetLockTarget(Transform target)
    {
        if (target != null)
        {
            LockTarget = target;
            isLocking = true;
            currentTargetIndex = -1; // 手动设置时不使用索引
            Debug.Log($"手动锁定目标: {target.name}");
        }
    }

    /// <summary>
    /// 手动清除锁定目标
    /// </summary>
    public void ClearLockTarget()
    {
        ExitLockMode();
    }

    /// <summary>
    /// 获取当前是否处于锁定模式
    /// </summary>
    /// <returns>是否锁定中</returns>
    public bool IsLocking()
    {
        return isLocking;
    }

    /// <summary>
    /// 获取当前可用目标数量
    /// </summary>
    /// <returns>可用目标数量</returns>
    public int GetAvailableTargetCount()
    {
        return availableTargets.Count;
    }


}
