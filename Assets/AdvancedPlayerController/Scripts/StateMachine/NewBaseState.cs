using UnityEngine;
using System;
using System.Collections.Generic;

/// <summary>
/// 状态管理器泛型抽象类
/// </summary>
/// <typeparam name="EState">状态枚举类型，需继承自Enum</typeparam>
public abstract class StateManager<EState> : MonoBehaviour where EState : Enum
{
    // 存储所有状态的字典，键为状态枚举，值为对应的状态实例
    protected Dictionary<EState, BaseState<EState>> States = new Dictionary<EState, BaseState<EState>>();
    // 当前激活的状态
    protected BaseState<EState> CurrentState;

    // 标志位：是否处于状态切换中
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
            // 如果当前状态和下一状态相同，则更新当前状态
            CurrentState.UpdateState();
        }
        else if(!IsTransitioningState)
        {
            // 不同，则切换到下一状态
            TransitionToState(nextStateKey);
        }
    }

    /// <summary>
    /// 状态切换方法，用于从当前状态切换到目标状态
    /// </summary>
    /// <param name="stateKey">目标状态的枚举标识</param>
    protected virtual void TransitionToState(EState stateKey)
    {
        IsTransitioningState = true;

        // 退出当前状态
        CurrentState.ExitState();
        // 进入目标状态
        CurrentState = States[stateKey];
        CurrentState.EnterState();

        IsTransitioningState = false;
    }

    /// <summary>
    /// 当碰撞体进入触发器时调用的方法
    /// </summary>
    /// <param name="other">进入触发器的碰撞体</param>
    void OnTriggerEnter(Collider other)
    {
        CurrentState.OnTriggerEnter(other);
    }

    /// <summary>
    /// 当碰撞体持续处于触发器中时调用的方法
    /// </summary>
    /// <param name="other">处于触发器中的碰撞体</param>
    void OnTriggerStay(Collider other)
    {
        CurrentState.OnTriggerStay(other);
    }

    /// <summary>
    /// 当碰撞体退出触发器时调用的方法
    /// </summary>
    /// <param name="other">退出触发器的碰撞体</param>
    void OnTriggerExit(Collider other)
    {
        CurrentState.OnTriggerExit(other);
    }
}