using GGG.Tool;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Spiderman.Health
{
    public abstract class CharacterHealthBase : MonoBehaviour
    {
        // ��ͬ�߼�˵����
        // - �������ˡ����������񵲵Ⱥ���
        // - ��ά������ֵ��Ϣ����ǰ�����ߵ�״̬

        protected Animator _animator;

        // ��ǰ�Ĺ�����
        protected Transform _currentAttacker;

        private void Awake()
        {
            _animator = GetComponent<Animator>();
        }

        protected virtual void OnEnable()
        {
            GameEventManager.MainInstance.AddEventListening<float, string, string, Transform, Transform>
                ("�����˺�", OnCharacterHitEventHandler);

            GameEventManager.MainInstance.AddEventListening<string, Transform, Transform>
                ("��������", OnCharacterFinishAttackEventHandler);

            GameEventManager.MainInstance.AddEventListening<float,Transform>
                ("���������˺�", TriggerDamageEventHandler);
        }

        protected virtual void OnDisable()
        {
            GameEventManager.MainInstance.RemoveEvent<float, string, string, Transform, Transform>
                ("�����˺�", OnCharacterHitEventHandler);

            GameEventManager.MainInstance.RemoveEvent<string, Transform, Transform>
                ("��������", OnCharacterFinishAttackEventHandler);

            GameEventManager.MainInstance.RemoveEvent<float,Transform>
                ("���������˺�", TriggerDamageEventHandler);
        }

        protected virtual void Update()
        {
            OnHitLookAttacker(); // ��ɫ�ܻ�ʱ���򹥻���
        }

        #region �ܻ������Ϊ
        /// <summary>
        /// ��ɫ�ܻ���Ϊ����
        /// </summary>
        /// <param name="damage"> �ܵ����˺�ֵ </param>
        /// <param name="hitName">�������ƣ����������ֲ�ͬ�������ͱ��֣�</param>
        /// <param name="parryName">��������ƣ����ж�Ӧ���߼������ݴ˴���</param>
        protected virtual void CharacterHitAction(float damage, string hitName, string parryName)
        {
            
        }

        /// <summary>
        /// �����ɫ�ܵ��˺����߼�
        /// </summary>
        /// <param name="damage">�ܵ����˺�ֵ</param>
        protected virtual void TakeDamage(float damage)
        {
            // TODO: ȥ�۳�����ֵ 
            
        }
        #endregion

        #region �����������߼�
        /// <summary>
        /// ���õ�ǰ�Ĺ�����
        /// </summary>
        /// <param name="attacker">�����ߵ� Transform</param>
        private void SetAttacker(Transform attacker)
        {
            if (_currentAttacker == null || _currentAttacker != attacker)
            {
                // ��ǵ�ǰ������
                _currentAttacker = attacker;
            }
        }
        #endregion

        #region ��ɫ�ܻ�ʱ���򹥻���
        /// <summary>
        /// ��ɫ�ܻ�ʱ���򹥻���
        /// </summary>
        private void OnHitLookAttacker()
        {
            // û�е�ǰ�����ߣ�ֱ�ӷ���
            if (_currentAttacker == null)
                return;

            // ��ȡ��ǰ����״̬��Ϣ��Layer 0��
            AnimatorStateInfo currentState = _animator.GetCurrentAnimatorStateInfo(0);

            // �����������ܻ���Hit����񵲣�Parry�������׶Σ��Ҷ�����׼��ʱ��С�� 0.5
            bool isHitOrParryState = _animator.AnimationAtTag("Hit")
                                     || (_animator.AnimationAtTag("Parry")
                                         && currentState.normalizedTime < 0.5f);

            if (isHitOrParryState)
            {
                Debug.Log("���򹥻���");
                // �õ�ǰ�����򹥻���λ�ã�ƽ��ʱ����� 50f
                transform.Look(_currentAttacker.position, 50f);
            }
        }
        #endregion

        #region �¼������߼�

        #region Hit
        /// <summary>
        /// ��ɫ�ܻ��¼�
        /// </summary>
        /// <param name="damage">�ܵ����˺�ֵ</param>
        /// <param name="hitName">��������</param>
        /// <param name="parryName">���������</param>
        /// <param name="attack">�����ߵ� Transform</param>
        /// <param name="self">�����ɫ�� Transform������У���Ƿ��������ܻ���</param>
        private void OnCharacterHitEventHandler(float damage, string hitName, string parryName, Transform attacker, Transform self)
        {
            // ���������self���ǵ�ǰ����˵�������������ܻ�
            if (self != transform)
            {
                return;
            }

            // �����ľ����Լ�
            #region �����ܻ��߼�
            SetAttacker(attacker); // ��ǵ�ǰ������
            CharacterHitAction(damage, hitName, parryName); // �����ܻ���Ϊ����
            TakeDamage(damage); // �����˺��۳��߼�
            #endregion
        }
        #endregion

        #region FinishAttack
        /// <summary>
        /// ��ɫ�����¼�
        /// </summary>
        /// <param name="hitName"></param>
        /// <param name="attacker"></param>
        /// <param name="self"></param>
        private void OnCharacterFinishAttackEventHandler(string hitName, Transform attacker, Transform self)
        {
            // ���������self���ǵ�ǰ����˵�������������ܻ�
            if (self != transform)
            {
                return;
            }

            // �����ľ����Լ�
            #region �����ܻ��߼�
            SetAttacker(attacker); // ��ǵ�ǰ������
            _animator.Play(hitName); // �����ܻ�����
            // �����˺��۳��߼�
            #endregion
        }

        /// <summary>
        /// ���������Ĵ����˺��¼�
        /// </summary>
        /// <param name="damage"></param>
        private void TriggerDamageEventHandler(float damage, Transform self)
        {
            // ���������self���ǵ�ǰ����˵�������������ܻ�
            if(self != transform)
                return;
            // �����˺��۳��߼�
            TakeDamage(damage);
            // �����ܻ���Ч
            GamePoolManager.Instance.TryGetPoolItem("HitSound", transform.position, Quaternion.identity);
        }
        #endregion

        #endregion

    }
}
