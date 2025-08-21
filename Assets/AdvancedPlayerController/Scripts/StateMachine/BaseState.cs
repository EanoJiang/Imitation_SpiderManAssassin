using UnityEngine;
using System;

/// <summary>
/// 状态基类，定义了状态机中所有状态的基本行为规范
/// 泛型参数EEState限制为枚举类型，用于表示具体的状态类型
/// </summary>
/// <typeparam name="EState">状态枚举类型，继承自Enum</typeparam>
public abstract class BaseState<EState> where EState : Enum
{
    //构造函数
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