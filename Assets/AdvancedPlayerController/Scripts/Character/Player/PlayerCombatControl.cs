using GGG.Tool;
using Spiderman.ComboData;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Spiderman.Combat
{
    /// <summary>
    /// 玩家战斗控制类，负责处理角色基础攻击、连招逻辑、动画控制及输入响应
    /// </summary>
    public class PlayerCombatControl : MonoBehaviour
    {
        private Animator _animator;
        private Transform _mainCamera;

        // 角色连招配置（Inspector 可配置）
        [Header("角色轻击连招配置")]
        [SerializeField] private CharacterComboSO _lightCombo;          // 轻击连招
        [Header("角色重击连招配置")]
        [SerializeField] private CharacterComboSO _heavyCombo;          // 重击连招
        [Header("角色正面处决连招配置")]
        [SerializeField] private CharacterComboSO _finishCombo;         // 正面处决连招
        [Header("角色背后处决连招配置")]
        [SerializeField] private CharacterComboSO _assassinCombo;       // 背后处决连招


        // 当前的连招
        private CharacterComboSO _currentCombo;

        // 连招执行状态变量
        private int _currentComboIndex;  // 当前连招动作索引
        private int _hitIndex;           // 当前动作的命中索引（扩展用）
        private int _currentComboCount;  // 当前连招动作总数
        private float _maxColdTime;      // 攻击最大间隔时间
        private bool _canAttackInput;    // 是否允许输入攻击信号

        // 处决执行状态变量
        private int _finishComboIndex;  // 处决连招动作索引

        // 检测的方向
        private Vector3 _detectDirection;
        [Header("攻击检测")]
        [SerializeField] private float _detectionRange; // 攻击检测范围
        [SerializeField] private float _detectionDistance; // 攻击检测距离
        private Transform _currentEnemy;    // 当前检测到的敌人

        private void Awake()
        {
            _animator = GetComponent<Animator>();
            _mainCamera = Camera.main.transform;
        }

        private void Start()
        {
            _canAttackInput = true;
            _currentCombo = _lightCombo;
        }

        private void Update()
        {
            // 更新检测方向
            UpdateDetectDirection();

            // 基础攻击输入
            CharacterBaseAttackInput();

            // 移动的时候结束连招(重置Combo索引)
            OnEndCombo();

            // 角色朝向目标敌人
            LookTargetOnAttack();

            // 处决期间玩家位置同步
            MatchPosition();

            // 正面处决攻击输入
            CharacterFinishAttackInput();

            // 背后处决攻击输入
            CharacterAssassinInput();
        }

        private void FixedUpdate()
        {
            DetectionTarget();
        }

        #region 攻击事件
        /// <summary>
        /// 攻击事件
        /// </summary>
        private void ATKEvent()
        {
            // 伤害触发
            TriggerDamage();
            // 命中索引更新
            UpdateHitIndex();
            // 攻击音效触发（从对象池中取用）
            GamePoolManager.Instance.TryGetPoolItem(
                "ATKSound", 
                transform.position, 
                Quaternion.identity);
        }
        #endregion
       
        #region 攻击检测
        /// <summary>
        /// 检测目标敌人
        /// </summary>
        private void DetectionTarget()
        {
            if (Physics.SphereCast(
                GetDetectionOrigin(),
                _detectionRange,
                _detectDirection,
                out RaycastHit hit,
                _detectionDistance,
                layerMask: 1 << 13,
                QueryTriggerInteraction.Ignore))
            {
                _currentEnemy = hit.collider.transform;
            }
        }

        /// <summary>
        /// 调试用：绘制检测范围
        /// </summary>
        private void OnDrawGizmos()
        {
            // 检测中心在检测原点 + 检测方向 * 检测距离
            Vector3 sphereCenter = GetDetectionOrigin() + _detectDirection * _detectionDistance;
            // 半径为检测范围
            Gizmos.DrawWireSphere(sphereCenter, _detectionRange);
        }

        /// <summary>
        /// 获取检测原点
        /// </summary>
        private Vector3 GetDetectionOrigin()
        {
            // 检测原点：角色位置 + 向上偏移 0.7f
            return transform.position + transform.up * 0.7f;
        }

        /// <summary>
        /// 更新检测方向
        /// </summary>
        private void UpdateDetectDirection()
        {
            // 检测方向：相机水平方向的输入移动量
            _detectDirection =
                _mainCamera.forward * GameInputManager.Instance.Movement.y +
                _mainCamera.right * GameInputManager.Instance.Movement.x;

            // 检测方向不含竖直方向，清零
            // (如果后面要上击和下击，可以再加上竖直方向)
            _detectDirection.Set(_detectDirection.x, 0f, _detectDirection.z);

            // 归一化检测方向
            _detectDirection = _detectDirection.normalized;
        }
        #endregion

        #region 触发伤害（普通攻击和处决攻击）
        private void TriggerDamage()
        {
            // 1. 要确保有目标。
            // 2. 要确保敌人处于我们可触发伤害的距离和角度
            // 3. 去呼叫事件中心，帮我调用触发伤害这个函数

            #region 无法触发伤害的情况
            // 无目标敌人
            if (_currentEnemy == null)
                return;

            // 目标敌人不在有效角度内
            // 角色朝向 和 角色到当前敌人向量的点积cos值 ：是否在有效角度内（阈值 0.85，越接近 1 越好）
            if (Vector3.Dot(transform.forward, DirectionForTarget(transform, _currentEnemy)) < 0.85f)
                return;

            // 距离超过阈值
            if (DistanceForTarget(_currentEnemy, transform) > 1.3f)
                return;
            #endregion

            //条件都满足，才可以触发伤害
            #region 可以触发伤害的情况
            if (_animator.AnimationAtTag("Attack"))
            {                // 这里传的受伤动画是单个动画片段
                //基础攻击
                // 从连击数据中获取伤害相关参数
                float damageValue = _currentCombo.TryGetComboDamage(_currentComboIndex);
                string hitName = _currentCombo.TryGetOneHitName(_currentComboIndex, _hitIndex);
                string parryName = _currentCombo.TryGetOneParryName(_currentComboIndex, _hitIndex);

                // 调用事件中心触发伤害事件
                GameEventManager.MainInstance.CallEvent(
                    "触发伤害",
                    damageValue,          // value: 伤害值
                    hitName,              // value1: 受伤动画名
                    parryName,            // value2: 格挡动画名
                    transform,            // value3: 攻击者（自己）
                    _currentEnemy         // value4: 当前被攻击者（敌人）
                );

            }
            else
            {               // 同一处决动画期间会触发多次伤害
                //处决攻击
                // 从处决数据中获取连招伤害相关参数
                float damageValue = _finishCombo.TryGetComboDamage(_finishComboIndex);
                // 调用触发处决伤害事件
                GameEventManager.MainInstance.CallEvent("触发处决伤害", damageValue,_currentEnemy);
                Debug.Log("触发处决伤害");
            }
            #endregion
        }

        /// <summary>
        /// 自身到目标的单位向量
        /// </summary>
        /// <param name="target">目标</param>
        /// <param name="self">自身</param>
        /// <returns></returns>
        public Vector3 DirectionForTarget(Transform target, Transform self)
        {
            return (self.position - target.position).normalized;
        }

        /// <summary>
        /// 自身到目标之间的距离
        /// </summary>
        /// <param name="target"></param>
        /// <param name="self"></param>
        /// <returns></returns>
        public float DistanceForTarget(Transform target, Transform self)
        {
            return Vector3.Distance(self.position, target.position);
        }
        #endregion

        #region 基础攻击
        #region 基础攻击输入
        /// <summary>
        /// 角色基础攻击输入处理
        /// </summary>
        private void CharacterBaseAttackInput()
        {
            if (!CanBaseAttackInput())
                return;

            if (GameInputManager.Instance.LAttack)      // 发起基础攻击——轻击
            {
                // 切换/重置基础连招
                if (_currentCombo == null || _currentCombo != _lightCombo)
                {
                    ChangeComboState(_lightCombo);
                }
                // 执行连招动作
                ExecuteComboAction();
            }
            else if (GameInputManager.Instance.RAttack) // 发起变招攻击——重击
            {
                // 切换到重击连招
                ChangeComboState(_heavyCombo);
                // 在重击的Combo连招表中选取不同的动作，用于变招
                // case几就是轻击多少下
                switch (_currentComboCount)
                {
                    case 0:
                        // R
                    case 1:
                        // LR
                        _currentComboIndex = 0;
                        break;
                    case 2:
                        // LLR
                        _currentComboIndex = 1;
                        break;
                    case 3:
                        // LLLR
                        _currentComboIndex = 2;
                        break;
                    case 4:
                        // LLLLR
                        _currentComboIndex = 3;
                        break;
                }
                // 执行连招动作
                ExecuteComboAction();
                // 重击会重置连招总数
                _currentComboCount = 0;
            }
        }

        /// <summary>
        /// 判断是否允许发起基础攻击
        /// </summary>
        /// <returns>允许攻击返回 true，否则 false</returns>
        private bool CanBaseAttackInput()
        {
            // 不能攻击输入时不允许攻击
            if (!_canAttackInput)
                return false;
            // 正在受击（Hit 标签动画）不允许攻击
            if (_animator.AnimationAtTag("Hit"))
                return false;
            // 正在格挡（Parry 标签动画）不允许攻击
            if (_animator.AnimationAtTag("Parry"))
                return false;
            // 正在处决（Finish 标签动画）不允许攻击
            if (_animator.AnimationAtTag("Finish"))
                return false;

            return true;
        }
        #endregion

        #region 连招执行
        /// <summary>
        /// 执行连招动作，包含动画切换、计时器设置及输入锁定逻辑
        /// </summary>
        private void ExecuteComboAction()
        {
            //每次进入执行动作的时候，如果是基础攻击 自增当前连招总数
            _currentComboCount += (_currentCombo == _lightCombo)? 1 : 0;

            // 重置命中索引
            _hitIndex = 0;

            // 连招索引循环处理
            if (_currentComboIndex == _currentCombo.TryGetComboMaxCount())
            {
                _currentComboIndex = 0;
            }

            // 获取并设置当前索引处连招的最大冷却时间
            _maxColdTime = _currentCombo.TryGetColdTime(_currentComboIndex);
            // 切换组合机动画
            _animator.CrossFadeInFixedTime(
                _currentCombo.TryGetOneComboAction(_currentComboIndex),
                0.15f,
                0,
                0f
            );

            // 启动计时器，冷却结束后更新连招信息
            TimerManager.Instance.TryGetOneTimer(_maxColdTime, UpdateComboInfo);
            _canAttackInput = false;
        }
        #endregion

        #region 更新连招信息

        /// <summary>
        /// 更新连招状态信息：索引递增、冷却重置、恢复攻击输入
        /// </summary>
        private void UpdateComboInfo()
        {
            _currentComboIndex++;
            _maxColdTime = 0f;
            _canAttackInput = true;
        }
        #endregion

        #region 更新命中索引
        /// <summary>
        /// 更新命中索引
        /// </summary>
        private void UpdateHitIndex()
        {
            _hitIndex++;

            int maxCount = _currentCombo.TryGetHitOrParryMaxCount(_currentComboIndex);
            if (_hitIndex == maxCount)
            {
                _hitIndex = 0;
            }
        }
        #endregion

        #region 重置连招状态
        /// <summary>
        /// 重置连招状态（索引、冷却时间）
        /// </summary>
        private void ResetComboInfo()
        {
            _currentComboIndex = 0;
            _maxColdTime = 0f;
            _hitIndex = 0;
            _finishComboIndex = 0;
        }

        /// <summary>
        /// 移动的时候重置Combo索引
        /// </summary>
        private void OnEndCombo()
        {
            if(_animator.AnimationAtTag("Motion") && _canAttackInput)
            {
                ResetComboInfo();
            }
        }
        #endregion

        #region 切换连招状态
        /// <summary>
        /// 切换连招状态——轻击||重击
        /// </summary>
        /// <param name="newCombo"></param>
        private void ChangeComboState(CharacterComboSO newCombo)
        {
            if(newCombo != _currentCombo)
            {
                _currentCombo = newCombo;
                //每次切换都要重置Combo索引
                ResetComboInfo();
            }
        }
        #endregion

        #endregion

        #region 让玩家朝向目标敌人的位置
        /// <summary>
        /// 让玩家朝向目标敌人
        /// </summary>
        private void LookTargetOnAttack()
        {
            if(_currentEnemy == null)
                return;

            // 和敌人距离超过阈值,不进行朝向
            if (DistanceForTarget(_currentEnemy, transform) > 5.0f)
                return;

            // 获取Layer 0层的当前动画状态信息
            AnimatorStateInfo currentState = _animator.GetCurrentAnimatorStateInfo(0);

            // 当前动画状态是攻击动画，且动画未播放到后半段
            if (_animator.AnimationAtTag("Attack") && currentState.normalizedTime < 0.5f)
            {
                // 让当前对象平滑朝向目标敌人的位置
                transform.Look(_currentEnemy.position, 50f);
            }
        }

        #region 工具函数
        /// <summary>
        /// 看向目标
        /// </summary>
        /// <param name="transform"></param>
        /// <param name="target"></param>
        /// <param name="timer">平滑时间(如果是单击某个按键触发那么值最好设置100以上。)</param>
        public void Look(Transform transform, Vector3 target, float timer)
        {
            var direction = (target - transform.position).normalized;
            direction.y = 0f;
            Quaternion lookRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, UnTetheredLerp(timer));
        }

        /// <summary>
        /// 不受帧数影响的Lerp
        /// </summary>
        /// <param name="time">平滑时间(尽量设置为大于10的值)</param>
        public float UnTetheredLerp(float time = 10f)
        {
            return 1 - Mathf.Exp(-time * Time.deltaTime);
        }
        #endregion

        #endregion

        #region 处决
        /// <summary>
        /// 是否允许执行处决攻击
        /// </summary>
        private bool CanSpecialAttack()
        {
            // 处于 "Finish" 标签动画时，不允许
            if (_animator.AnimationAtTag("Finish"))
                return false;

            // 没有当前敌人时，不允许
            if (_currentEnemy == null)
                return false;
            // 当前连招总数小于2时，不允许
            if(_currentComboCount < 2)
                return false;
            // 在敌人后方或侧面时，不允许(和敌人同向的时候，是很小的锐角，慢慢放大这个角就是侧面，所以小于某个超过90度的角即可，这里取120度)
            float angle = Vector3.Angle(transform.forward, _currentEnemy.forward);
            if (angle < 120f)
                return false;

            return true;
        }

        /// <summary>
        /// 处理角色处决攻击的输入响应逻辑
        /// </summary>
        private void CharacterFinishAttackInput()
        {
            // 不满足处决攻击条件时，直接返回
            if (!CanSpecialAttack())
                return;

            // 检测到处决输入时，执行处决流程
            if (GameInputManager.Instance.FinishAttack)
            {
                // 1. 随机选取处决连招索引
                _finishComboIndex = Random.Range(0, _finishCombo.TryGetComboMaxCount());

                // 2. 播放对应的处决动画
                string finishAnim = _finishCombo.TryGetOneComboAction(_finishComboIndex);
                _animator.Play(finishAnim);

                // 3. 调用事件中心，触发敌人的处决事件
                string hitName = _finishCombo.TryGetOneHitName(_finishComboIndex, 0);
                GameEventManager.MainInstance.CallEvent(
                    "触发处决",
                    hitName,
                    transform,
                    _currentEnemy
                );
                // 4. 调用定时器事件：更新连招状态信息，防止索引越界
                TimerManager.Instance.TryGetOneTimer(
                    _finishCombo.TryGetColdTime(_finishComboIndex),    //这里原先写的是固定的0.5f
                    UpdateComboInfo);
                // 5. 重置连招状态，防止索引越界
                ResetComboInfo();

            }
        }
        #endregion

        #region 位置同步
        /// <summary>
        /// 处决期间玩家位置同步
        /// </summary>
        private void MatchPosition()
        {
            if (_currentEnemy == null)
                return;
            if (!_animator)
                return;

            if (_animator.AnimationAtTag("Finish"))//当前在处决动画
            {
                transform.rotation = Quaternion.LookRotation(-_currentEnemy.forward);   // 面对敌人
                RunningMatch(_finishCombo,_finishComboIndex);
            }
            else if (_animator.AnimationAtTag("Assassin"))//当前在普通攻击动画
            {
                transform.rotation = Quaternion.LookRotation(_currentEnemy.forward);    // 背对敌人
                RunningMatch(_assassinCombo, _finishComboIndex);
            }

        }

        private void RunningMatch(CharacterComboSO combo,int comboIndex, float startTime = 0f, float endTime = 0.01f)
        {
            if (!_animator.isMatchingTarget && !_animator.IsInTransition(0))//当前不在匹配,同时不处于过渡状态
            {
                _animator.MatchTarget(
                    _currentEnemy.position + (-transform.forward * combo.TryGetComboPositionOffset(comboIndex)),
                    Quaternion.identity,
                    AvatarTarget.Body,
                    new MatchTargetWeightMask(Vector3.one, 0f),
                    startTime,
                    endTime
                );
            }
        }
        #endregion

        #region 暗杀
        /// <summary>
        /// 是否允许执行暗杀逻辑的条件判断
        /// </summary>
        private bool CanAssassin()
        {
            // 1. 无目标时不允许
            if (_currentEnemy == null)
                return false;

            // 2. 距离超过 2f 时不允许
            float distance = DevelopmentTools.DistanceForTarget(_currentEnemy, transform);
            if (distance > 2f)
                return false;

            // 3. 在敌人前方/在敌人背后但角度差超过 60° 时不允许(和敌人同向的时候，是很小的锐角)
            float angle = Vector3.Angle(transform.forward, _currentEnemy.forward);
            if (angle > 60f)
                return false;

            // 4. 正在播放暗杀动画时不允许（避免重复触发）
            if (_animator.AnimationAtTag("Assassin"))
                return false;

            return true;
        }

        /// <summary>
        /// 处理角色暗杀输入的响应逻辑
        /// </summary>
        private void CharacterAssassinInput()
        {
            // 不满足暗杀条件时直接返回
            if (!CanAssassin())
                return;

            // 检测到 "取出武器/触发暗杀" 输入时执行逻辑
            if (GameInputManager.Instance.FinishAttack)
            {
                // 1. 随机选取暗杀连招索引
                _finishComboIndex = Random.Range(
                    0,
                    _assassinCombo.TryGetComboMaxCount()
                );

                // 2. 播放对应的暗杀动画
                string animationState = _assassinCombo.TryGetOneComboAction(_finishComboIndex);
                _animator.Play(animationState, 0, 0f);

                // 3. 获取暗杀动画的命中名称，用于事件传递
                string hitName = _assassinCombo.TryGetOneHitName(_finishComboIndex, 0);

                // 4. 调用事件中心，触发敌人的处决/暗杀事件
                GameEventManager.MainInstance.CallEvent(
                    "触发处决",
                    hitName,
                    transform,
                    _currentEnemy
                );
                // 5. 重置连招状态，防止索引越界
                ResetComboInfo();
            }
        }
        #endregion



    }
}
