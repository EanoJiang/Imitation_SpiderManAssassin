using UnityEngine;
using System;

/// <summary>
/// ״̬���࣬������״̬��������״̬�Ļ�����Ϊ�淶
/// ���Ͳ���EEState����Ϊö�����ͣ����ڱ�ʾ�����״̬����
/// </summary>
/// <typeparam name="EState">״̬ö�����ͣ��̳���Enum</typeparam>
public abstract class BaseState<EState> where EState : Enum
{
    //���캯��
    public BaseState(EState key)
    {
        StateKey = key;
    }
    public EState StateKey { get; private set; }

    public abstract void EnterState();
    public abstract void ExitState();
    public abstract void UpdateState();
    public abstract EState GetNextState();
    public abstract void OnTriggerEnter(Collider other);
    public abstract void OnTriggerStay(Collider other);
    public abstract void OnTriggerExit(Collider other);
}