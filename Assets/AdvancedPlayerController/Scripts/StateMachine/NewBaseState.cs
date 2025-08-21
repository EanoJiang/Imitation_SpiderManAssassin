using UnityEngine;
using System;
using System.Collections.Generic;

/// <summary>
/// ״̬���������ͳ�����
/// </summary>
/// <typeparam name="EState">״̬ö�����ͣ���̳���Enum</typeparam>
public abstract class StateManager<EState> : MonoBehaviour where EState : Enum
{
    // �洢����״̬���ֵ䣬��Ϊ״̬ö�٣�ֵΪ��Ӧ��״̬ʵ��
    protected Dictionary<EState, BaseState<EState>> States = new Dictionary<EState, BaseState<EState>>();
    // ��ǰ�����״̬
    protected BaseState<EState> CurrentState;

    // ��־λ���Ƿ���״̬�л���
    protected bool IsTransitioningState = false;

    void Start()
    {
        CurrentState.EnterState();
    }

    void Update()
    {
        EState nextStateKey = CurrentState.GetNextState();

        if (!IsTransitioningState && nextStateKey.Equals(CurrentState.StateKey))
        {
            // �����ǰ״̬����һ״̬��ͬ������µ�ǰ״̬
            CurrentState.UpdateState();
        }
        else if(!IsTransitioningState)
        {
            // ��ͬ�����л�����һ״̬
            TransitionToState(nextStateKey);
        }
    }

    /// <summary>
    /// ״̬�л����������ڴӵ�ǰ״̬�л���Ŀ��״̬
    /// </summary>
    /// <param name="stateKey">Ŀ��״̬��ö�ٱ�ʶ</param>
    protected virtual void TransitionToState(EState stateKey)
    {
        IsTransitioningState = true;

        // �˳���ǰ״̬
        CurrentState.ExitState();
        // ����Ŀ��״̬
        CurrentState = States[stateKey];
        CurrentState.EnterState();

        IsTransitioningState = false;
    }

    /// <summary>
    /// ����ײ����봥����ʱ���õķ���
    /// </summary>
    /// <param name="other">���봥��������ײ��</param>
    void OnTriggerEnter(Collider other)
    {
        CurrentState.OnTriggerEnter(other);
    }

    /// <summary>
    /// ����ײ��������ڴ�������ʱ���õķ���
    /// </summary>
    /// <param name="other">���ڴ������е���ײ��</param>
    void OnTriggerStay(Collider other)
    {
        CurrentState.OnTriggerStay(other);
    }

    /// <summary>
    /// ����ײ���˳�������ʱ���õķ���
    /// </summary>
    /// <param name="other">�˳�����������ײ��</param>
    void OnTriggerExit(Collider other)
    {
        CurrentState.OnTriggerExit(other);
    }
}