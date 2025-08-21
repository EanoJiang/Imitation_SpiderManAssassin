using FS_ThirdPerson;
using GGG.Tool;
using Spiderman.ComboData;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Spiderman.Combat
{
    /// <summary>
    /// ���ս�������࣬�������ɫ���������������߼����������Ƽ�������Ӧ
    /// </summary>
    public class PlayerCombatControl : MonoBehaviour
    {
        #region �������������л�
        private Animator _animator;

        // ��ɫ�������ã�Inspector �����ã�
        [Header("��ɫ�����������")]
        [SerializeField] private CharacterComboSO _lightCombo;      // �������
        [Header("��ɫ�ػ���������")]
        [SerializeField] private CharacterComboSO _heavyCombo;      // �ػ�����

        // ��ǰ������
        private CharacterComboSO _currentCombo;

        // ����ִ��״̬����
        private int _currentComboIndex;  // ��ǰ���ж�������
        private int _hitIndex;           // ��ǰ������������������չ�ã�
        private int _currentComboCount;  // ��ǰ���ж�������
        private float _maxColdTime;      // ���������ʱ��
        private bool _canAttackInput;    // �Ƿ��������빥���ź�
        
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


        #region ������������
        /// <summary>
        /// ��ɫ�����������봦��
        /// </summary>
        private void CharacterBaseAttackInput()
        {
            if (!CanBaseAttackInput())
                return;

            if (GameInputManager.MainInstance.LAttack)      // ������������������
            {
                // �л�/���û�������
                if (_currentCombo == null || _currentCombo != _lightCombo)
                {
                    ChangeComboState(_lightCombo);
                }
                // ִ�����ж���
                ExecuteComboAction();
            }
            else if (GameInputManager.MainInstance.RAttack) // ������й��������ػ�
            {
                // �л����ػ�����
                ChangeComboState(_heavyCombo);
                // ���ػ���Combo���б���ѡȡ��ͬ�Ķ��������ڱ���
                // case���������������
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
                // ִ�����ж���
                ExecuteComboAction();
                // �ػ���������������
                _currentComboCount = 0;
            }
        }

        /// <summary>
        /// �ж��Ƿ��������������
        /// </summary>
        /// <returns>���������� true������ false</returns>
        private bool CanBaseAttackInput()
        {
            // ����������ʱֱ�ӷ���
            if (!_canAttackInput)
                return false;
            // �����ܻ���Hit ��ǩ��������������
            if (_animator.AnimationAtTag("Hit"))
                return false;
            // ���ڸ񵲣�Parry ��ǩ��������������
            if (_animator.AnimationAtTag("Parry"))
                return false;

            return true;
        }
        #endregion

        #region ����ִ��
        /// <summary>
        /// ִ�����ж��������������л�����ʱ�����ü����������߼�
        /// </summary>
        private void ExecuteComboAction()
        {
            //ÿ�ν���ִ�ж�����ʱ������ǻ������� ������ǰ��������
            _currentComboCount += (_currentCombo == _lightCombo)? 1 : 0;

            // ������������
            _hitIndex = 0;

            // ��������ѭ������
            if (_currentComboIndex == _currentCombo.TryGetComboMaxCount())
            {
                _currentComboIndex = 0;
            }

            // ��ȡ�����õ�ǰ���������е������ȴʱ��
            _maxColdTime = _currentCombo.TryGetColdTime(_currentComboIndex);
            // �л���ϻ�����
            _animator.CrossFadeInFixedTime(
                _currentCombo.TryGetOneComboAction(_currentComboIndex),
                0.15f,
                0,
                0f
            );

            // ������ʱ������ȴ���������������Ϣ
            TimerManager.MainInstance.TryGetOneTimer(_maxColdTime, UpdateComboInfo);
            _canAttackInput = false;
        }
        #endregion

        #region ����������Ϣ

        /// <summary>
        /// ��������״̬��Ϣ��������������ȴ���á��ָ���������
        /// </summary>
        private void UpdateComboInfo()
        {
            _currentComboIndex++;
            _maxColdTime = 0f;
            _canAttackInput = true;
        }
        #endregion

        #region ��������״̬
        /// <summary>
        /// ��������״̬����������ȴʱ�䣩
        /// </summary>
        private void ResetComboInfo()
        {
            _currentComboIndex = 0;
            _maxColdTime = 0f;
            _hitIndex = 0;
        }

        /// <summary>
        /// �ƶ���ʱ������Combo����
        /// </summary>
        private void OnEndCombo()
        {
            if(_animator.AnimationAtTag("Motion") && _canAttackInput)
            {
                ResetComboInfo();
            }
        }
        #endregion

        #region �л�����״̬
        /// <summary>
        /// �л�����״̬�������||�ػ�
        /// </summary>
        /// <param name="newCombo"></param>
        private void ChangeComboState(CharacterComboSO newCombo)
        {
            if(newCombo != _currentCombo)
            {
                _currentCombo = newCombo;
                //ÿ���л���Ҫ����Combo����
                ResetComboInfo();
            }
        }
        #endregion
    }
}
