using GGG.Tool;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Spiderman.Health
{
    public abstract class CharacterHealthBase : MonoBehaviour
    {
        // 共同逻辑说明：
        // - 都有受伤、被处决、格挡等函数
        // - 都维护生命值信息及当前攻击者等状态

        protected Animator _animator;

        // 当前的攻击者
        protected Transform _currentAttacker;

        private void Awake()
        {
            _animator = GetComponent<Animator>();
        }

        protected virtual void OnEnable()
        {
            GameEventManager.MainInstance.AddEventListening<float, string, string, Transform, Transform>
                ("触发伤害", OnCharacterHitEventHandler);

            GameEventManager.MainInstance.AddEventListening<string, Transform, Transform>
                ("触发处决", OnCharacterFinishAttackEventHandler);

            GameEventManager.MainInstance.AddEventListening<float,Transform>
                ("触发处决伤害", TriggerDamageEventHandler);
        }

        protected virtual void OnDisable()
        {
            GameEventManager.MainInstance.RemoveEvent<float, string, string, Transform, Transform>
                ("触发伤害", OnCharacterHitEventHandler);

            GameEventManager.MainInstance.RemoveEvent<string, Transform, Transform>
                ("触发处决", OnCharacterFinishAttackEventHandler);

            GameEventManager.MainInstance.RemoveEvent<float,Transform>
                ("触发处决伤害", TriggerDamageEventHandler);
        }

        protected virtual void Update()
        {
            OnHitLookAttacker(); // 角色受击时面向攻击者
        }

        #region 受击相关行为
        /// <summary>
        /// 角色受击行为处理
        /// </summary>
        /// <param name="damage"> 受到的伤害值 </param>
        /// <param name="hitName">攻击名称（可用于区分不同攻击类型表现）</param>
        /// <param name="parryName">格挡相关名称（若有对应格挡逻辑可依据此处理）</param>
        protected virtual void CharacterHitAction(float damage, string hitName, string parryName)
        {
            
        }

        /// <summary>
        /// 处理角色受到伤害的逻辑
        /// </summary>
        /// <param name="damage">受到的伤害值</param>
        protected virtual void TakeDamage(float damage)
        {
            // TODO: 去扣除生命值 
            
        }
        #endregion

        #region 攻击者设置逻辑
        /// <summary>
        /// 设置当前的攻击者
        /// </summary>
        /// <param name="attacker">攻击者的 Transform</param>
        private void SetAttacker(Transform attacker)
        {
            if (_currentAttacker == null || _currentAttacker != attacker)
            {
                // 标记当前攻击者
                _currentAttacker = attacker;
            }
        }
        #endregion

        #region 角色受击时面向攻击者
        /// <summary>
        /// 角色受击时面向攻击者
        /// </summary>
        private void OnHitLookAttacker()
        {
            // 没有当前攻击者，直接返回
            if (_currentAttacker == null)
                return;

            // 获取当前动画状态信息（Layer 0）
            AnimatorStateInfo currentState = _animator.GetCurrentAnimatorStateInfo(0);

            // 条件：处于受击（Hit）或格挡（Parry）动画阶段，且动画标准化时间小于 0.5
            bool isHitOrParryState = _animator.AnimationAtTag("Hit")
                                     || (_animator.AnimationAtTag("Parry")
                                         && currentState.normalizedTime < 0.5f);

            if (isHitOrParryState)
            {
                Debug.Log("面向攻击者");
                // 让当前对象朝向攻击者位置，平滑时间参数 50f
                transform.Look(_currentAttacker.position, 50f);
            }
        }
        #endregion

        #region 事件处理逻辑

        #region Hit
        /// <summary>
        /// 角色受击事件
        /// </summary>
        /// <param name="damage">受到的伤害值</param>
        /// <param name="hitName">攻击名称</param>
        /// <param name="parryName">格挡相关名称</param>
        /// <param name="attack">攻击者的 Transform</param>
        /// <param name="self">自身角色的 Transform（用于校验是否是自身受击）</param>
        private void OnCharacterHitEventHandler(float damage, string hitName, string parryName, Transform attacker, Transform self)
        {
            // 如果传来的self不是当前对象，说明不是自身在受击
            if (self != transform)
            {
                return;
            }

            // 否则打的就是自己
            #region 处理受击逻辑
            SetAttacker(attacker); // 标记当前攻击者
            CharacterHitAction(damage, hitName, parryName); // 处理受击行为表现
            TakeDamage(damage); // 处理伤害扣除逻辑
            #endregion
        }
        #endregion

        #region FinishAttack
        /// <summary>
        /// 角色处决事件
        /// </summary>
        /// <param name="hitName"></param>
        /// <param name="attacker"></param>
        /// <param name="self"></param>
        private void OnCharacterFinishAttackEventHandler(string hitName, Transform attacker, Transform self)
        {
            // 如果传来的self不是当前对象，说明不是自身在受击
            if (self != transform)
            {
                return;
            }

            // 否则打的就是自己
            #region 处理受击逻辑
            SetAttacker(attacker); // 标记当前攻击者
            _animator.Play(hitName); // 播放受击动画
            // 处理伤害扣除逻辑
            #endregion
        }

        /// <summary>
        /// 处决动画的触发伤害事件
        /// </summary>
        /// <param name="damage"></param>
        private void TriggerDamageEventHandler(float damage, Transform self)
        {
            // 如果传来的self不是当前对象，说明不是自身在受击
            if(self != transform)
                return;
            // 处理伤害扣除逻辑
            TakeDamage(damage);
            // 播放受击音效
            GamePoolManager.Instance.TryGetPoolItem("HitSound", transform.position, Quaternion.identity);
        }
        #endregion

        #endregion

    }
}
