using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerRoll : MonoBehaviour
{
    private Animator _animator;
    private bool _isRolling = false; // �������ж��Ƿ����ڷ���
    private float _rollDuration = 0.5f; // ������������ʱ�䣬����ʵ�ʶ�������
    private float _rollTimer = 0f; // ������ʱ��

    private void Awake()
    {
        _animator = GetComponent<Animator>();
    }

    private void Update()
    {
        // ������ڷ��������¼�ʱ����������������
        if (_isRolling)
        {
            _rollTimer += Time.deltaTime;
            if (_rollTimer >= _rollDuration)
            {
                // ��������������״̬
                _isRolling = false;
                _rollTimer = 0f;
            }
            return; // ���ڷ���ʱ�������ƶ�����
        }

        //// ֻ�в��ڷ���״̬ʱ�Ÿ����ƶ�����
        //var AxisX = GameInputManager.MainInstance.Movement.x;
        //var AxisY = GameInputManager.MainInstance.Movement.y;

        //// ƽ���ı�����
        //AxisX = Mathf.MoveTowards(AxisX, GameInputManager.MainInstance.Movement.x, Time.deltaTime * 5f);
        //AxisY = Mathf.MoveTowards(AxisY, GameInputManager.MainInstance.Movement.y, Time.deltaTime * 5f);

        //_animator.SetFloat("AxisX", AxisX);
        //_animator.SetFloat("AxisY", AxisY);

        // ��ⷭ�����룬ֻ�в��ڷ���״̬ʱ���ܴ����µķ���
        if (GameInputManager.Instance.Roll)
        {
            _isRolling = true;
            _animator.SetTrigger("Roll"); // ����ʹ��Trigger��������������
            GameEventManager.MainInstance.CallEvent("��ɫ����");
        }
    }
}