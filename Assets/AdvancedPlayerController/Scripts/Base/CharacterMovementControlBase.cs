using UnityEngine;

namespace Spiderman.Movement
{
    [RequireComponent(typeof(CharacterController))]
    public abstract class CharacterMovementControlBase : MonoBehaviour
    {
        // 角色控制器组件，用于处理角色移动相关的物理交互
        protected CharacterController _controller;
        // 动画组件，用于控制角色动画播放
        public Animator _animator;

        public Vector3 _moveDirection; // 角色移动方向

        // 地面检测相关变量
        public bool _characterIsOnGround;
        [Header("地面检测相关变量")]
        [SerializeField]protected float _groundDetectionPositionOffset; // 地面检测位置偏移量
        [SerializeField]protected float _detectionRang;                 // 地面检测范围
        [SerializeField]protected LayerMask _whatIsGround;              // 地面层掩码

        // 重力相关变量
        protected readonly float CharacterGravity = -9.8f;
        protected float _characterVerticalVelocity;     // 角色垂直方向速度
        protected float _fallOutDeltaTime;              // 下落 delta 时间，用于计算重力作用的时间积累
        protected float _fallOutTime = 0.15f;           // 下落等待时间，控制跌落动画播放时机
        protected readonly float _characterVerticalMaxVelocity = 54f; // 角色最大垂直速度,低于这个值应用重力
        protected Vector3 _characterVerticalDirection;  // 角色Y轴移动方向，通过charactercontroller.move来实现y轴移动

        protected virtual void Awake()
        {
            _controller = GetComponent<CharacterController>();
            _animator = GetComponent<Animator>();
        }

        protected virtual void OnEnable()
        {
            GameEventManager.MainInstance.AddEventListening<float>("改变角色垂直方向的移动速度", ChangeCharacterVerticalVelocity);
            GameEventManager.MainInstance.AddEventListening("角色翻滚", CharacterRoll);
            GameEventManager.MainInstance.AddEventListening("角色跳跃", CharacterJump);
        }

        protected virtual void OnDisable()
        {
            GameEventManager.MainInstance.RemoveEvent<float>("改变角色垂直方向的移动速度", ChangeCharacterVerticalVelocity);
            GameEventManager.MainInstance.RemoveEvent("角色翻滚", CharacterRoll);
            GameEventManager.MainInstance.RemoveEvent("角色跳跃", CharacterJump);
        }

        protected virtual void Start()
        {
            _fallOutDeltaTime = _fallOutTime;
        }

        private void Update()
        {
            SetCharacterGravity();
            UpdateCharacterGravity();
        }

        /// <summary>
        /// 脚本控制animator的根运动
        /// </summary>
        protected virtual void OnAnimatorMove()
        {
            _animator.ApplyBuiltinRootMotion();
            UpdateCharacterMoveDirection(_animator.deltaPosition);
        }

        /// <summary>
        /// 地面检测方法
        /// </summary>
        /// <returns>返回角色是否在地面的布尔值</returns>
        private bool GroundDetection()
        {
            // 构建检测位置：基于角色当前位置，调整 Y 轴偏移（用于地面检测的位置修正）
            Vector3 detectionPosition = new Vector3(
                transform.position.x,
                transform.position.y - _groundDetectionPositionOffset,
                transform.position.z
            );

            // 球形检测：检查在指定位置、指定半径范围内，与 _whatIsGround 层的碰撞体是否存在相交
            // 参数分别为：检测中心、检测半径、地面层掩码、忽略触发器交互
            return Physics.CheckSphere(
                detectionPosition,
                _detectionRang,
                _whatIsGround,
                QueryTriggerInteraction.Ignore
            );
        }

        /// <summary>
        /// 根据是否在地面设置对应的角色重力逻辑
        /// </summary>
        private void SetCharacterGravity()
        {
            // 检测角色是否在地面
            _characterIsOnGround = GroundDetection();

            if (_characterIsOnGround)
            {
                //1.在地面
                // 1.1 重置下落等待时间
                _fallOutDeltaTime = _fallOutTime;

                // 1.2 重置垂直速度（防止落地后持续累积速度）
                if (_characterVerticalVelocity < 0)
                {
                    _characterVerticalVelocity = -2f;
                }
            }
            else
            {
                //2.不在地面
                if (_fallOutDeltaTime > 0)
                {
                    // 2.1 处理楼梯/小落差：等待 0.15 秒后再应用重力
                    _fallOutDeltaTime -= Time.deltaTime;
                }
                else
                {
                    // 2.2 倒计时结束还没有落地？那说明不是小落差，要开始应用重力
                }
                if (_characterVerticalVelocity < _characterVerticalMaxVelocity)
                {
                    _characterVerticalVelocity += CharacterGravity * Time.deltaTime;
                    // 重力公式累积垂直速度
                }
            }
        }

        /// <summary>
        /// 更新角色垂直方向移动（应用重力效果）
        /// </summary>
        private void UpdateCharacterGravity()
        {
            //这里只处理 y 轴重力
            // x/z 由其他移动逻辑控制
            Vector3 _characterVerticalDirection = new Vector3(0, _characterVerticalVelocity, 0);

            // 通过 CharacterController 应用y轴移动
            _controller.Move(_characterVerticalDirection * Time.deltaTime);
        }

        /// <summary>
        /// 斜坡方向重置：检测角色是否在坡上移动，防止下坡速度过快导致异常
        /// </summary>
        /// <param name="moveDirection">原始移动方向</param>
        /// <returns>适配斜坡后的移动方向</returns>
        private Vector3 SlopResetDirection(Vector3 moveDirection)
        {
            // 射线检测参数配置
            Vector3 rayOrigin = transform.position + transform.up * 0.5f;   // 射线起点
            Vector3 rayDirection = Vector3.down;                            // 射线方向
            float maxDistance = _controller.height * 0.85f;                 // 射线最大距离
            LayerMask targetLayer = _whatIsGround;                          // 检测的目标地面层
            QueryTriggerInteraction triggerInteraction = QueryTriggerInteraction.Ignore; // 忽略触发器

            // 执行向下的射线检测
            if (Physics.Raycast(rayOrigin, rayDirection, out RaycastHit hit, maxDistance, targetLayer, triggerInteraction))
            {
                // 点积判断：检测地面法线是否与角色上方向垂直（点积接近0表示垂直，非0则说明有坡度）
                if (Vector3.Dot(transform.up, hit.normal) != 0)
                {
                    // 将移动方向投影到斜坡平面
                    moveDirection = Vector3.ProjectOnPlane(moveDirection, hit.normal);
                }
            }
            return moveDirection;
        }

        /// <summary>
        /// 更新角色水平移动方向——绕y轴旋转
        /// </summary>
        protected void UpdateCharacterMoveDirection(Vector3 direction)
        {
            _moveDirection = SlopResetDirection(direction);
            _controller.Move(_moveDirection * Time.deltaTime);
        }

        private void OnDrawGizmos()
        {
            // 设置gizmos颜色为红色，使其更容易看到
            Gizmos.color = Color.red;
            
            Vector3 detectionPosition = new Vector3(
                transform.position.x,
                transform.position.y - _groundDetectionPositionOffset,
                transform.position.z
            );
            Gizmos.DrawWireSphere(detectionPosition, _detectionRang);
        }

        /// <summary>
        /// 事件注册：改变角色垂直方向的移动速度
        /// </summary>
        /// <param name="value"></param>
        private void ChangeCharacterVerticalVelocity(float value)
        {
            _characterVerticalVelocity = value;
        }

        /// <summary>
        /// 角色翻滚
        /// </summary>
        private void CharacterRoll()
        {
            _animator.SetTrigger("Roll");
        }

        private void CharacterJump()
        {
            if (_characterIsOnGround)
            {
                _animator.SetTrigger("Jump");
            }
        }


    }
}
