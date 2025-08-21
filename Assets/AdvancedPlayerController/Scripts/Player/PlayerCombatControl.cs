using FS_ThirdPerson;
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
        #region 变量定义与序列化
        private Animator _animator;

        // 角色连招配置（Inspector 可配置）
        [Header("角色轻击连招配置")]
        [SerializeField] private CharacterComboSO _lightCombo;      // 轻击连招
        [Header("角色重击连招配置")]
        [SerializeField] private CharacterComboSO _heavyCombo;      // 重击连招

        // 当前的连招
        private CharacterComboSO _currentCombo;

        // 连招执行状态变量
        private int _currentComboIndex;  // 当前连招动作索引
        private int _hitIndex;           // 当前动作的命中索引（扩展用）
        private int _currentComboCount;  // 当前连招动作总数
        private float _maxColdTime;      // 攻击最大间隔时间
        private bool _canAttackInput;    // 是否允许输入攻击信号
        
        #endregion

        private void Awake()
        {
            _animator = GetComponent<Animator>();
        }

        private void Start()
        {
            _canAttackInput = true;
            _currentCombo = _lightCombo;
        }

        private void Update()
        {
            CharacterBaseAttackInput();
            OnEndCombo();
        }


        #region 基础攻击输入
        /// <summary>
        /// 角色基础攻击输入处理
        /// </summary>
        private void CharacterBaseAttackInput()
        {
            if (!CanBaseAttackInput())
                return;

            if (GameInputManager.MainInstance.LAttack)      // 发起基础攻击——轻击
            {
                // 切换/重置基础连招
                if (_currentCombo == null || _currentCombo != _lightCombo)
                {
                    ChangeComboState(_lightCombo);
                }
                // 执行连招动作
                ExecuteComboAction();
            }
            else if (GameInputManager.MainInstance.RAttack) // 发起变招攻击——重击
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
            // 不允许输入时直接返回
            if (!_canAttackInput)
                return false;
            // 正在受击（Hit 标签动画）不允许攻击
            if (_animator.AnimationAtTag("Hit"))
                return false;
            // 正在格挡（Parry 标签动画）不允许攻击
            if (_animator.AnimationAtTag("Parry"))
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
            TimerManager.MainInstance.TryGetOneTimer(_maxColdTime, UpdateComboInfo);
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

        #region 重置连招状态
        /// <summary>
        /// 重置连招状态（索引、冷却时间）
        /// </summary>
        private void ResetComboInfo()
        {
            _currentComboIndex = 0;
            _maxColdTime = 0f;
            _hitIndex = 0;
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
    }
}
