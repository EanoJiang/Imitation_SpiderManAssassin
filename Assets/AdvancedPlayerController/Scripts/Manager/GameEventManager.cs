using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using GGG.Tool;
using GGG.Tool.Singleton;

public class GameEventManager : SingletonNonMono<GameEventManager>
{
    // 事件接口
    private interface IEventHelp
    {
    }

    // 事件类，实现 IEventHelp 接口，用于管理事件注册、调用等逻辑
    private class EventHelp : IEventHelp
    {
        // 存储事件委托
        private event Action _action;

        // 构造函数，初始化时绑定初始事件逻辑
        public EventHelp(Action action)
        {
            // 首次实例化时赋值，仅执行这一次初始绑定
            _action = action;
        }

        // 增加事件注册的方法，将新的事件逻辑追加到委托中
        public void AddCall(Action action)
        {
            _action += action;
        }

        // 调用事件的方法，若有绑定逻辑则执行
        public void Call()
        {
            _action?.Invoke();
        }

        // 移除事件的方法，将指定事件逻辑从委托中移除
        public void Remove(Action action)
        {
            _action -= action;
        }
    }

    private class EventHelp<T> : IEventHelp
    {
        // 存储事件委托
        private event Action<T> _action;

        // 构造函数，初始化时绑定初始事件逻辑
        public EventHelp(Action<T> action)
        {
            // 首次实例化时赋值，仅执行这一次初始绑定
            _action = action;
        }

        // 增加事件注册的方法，将新的事件逻辑追加到委托中
        public void AddCall(Action<T> action)
        {
            _action += action;
        }

        // 调用事件的方法，若有绑定逻辑则执行
        public void Call(T value)
        {
            _action?.Invoke(value);
        }

        // 移除事件的方法，将指定事件逻辑从委托中移除
        public void Remove(Action<T> action)
        {
            _action -= action;
        }
    }

    private class EventHelp<T1, T2> : IEventHelp
    {
        // 存储事件委托
        private event Action<T1, T2> _action;

        // 构造函数，初始化时绑定初始事件逻辑
        public EventHelp(Action<T1, T2> action)
        {
            // 首次实例化时赋值，仅执行这一次初始绑定
            _action = action;
        }

        // 增加事件注册的方法，将新的事件逻辑追加到委托中
        public void AddCall(Action<T1, T2> action)
        {
            _action += action;
        }

        // 调用事件的方法，若有绑定逻辑则执行
        public void Call(T1 value1, T2 value2)
        {
            _action?.Invoke(value1, value2);
        }

        // 移除事件的方法，将指定事件逻辑从委托中移除
        public void Remove(Action<T1, T2> action)
        {
            _action -= action;
        }
    }

    /// <summary>
    /// 事件中心，用于管理事件注册、调用
    /// </summary>
    private Dictionary<string, IEventHelp> _eventCenter = new Dictionary<string, IEventHelp>();

    /// <summary>
    /// 添加事件监听
    /// </summary>
    /// <param name="eventName">事件名称</param>
    /// <param name="action">回调函数</param>
    public void AddEventListening(string eventName, Action action)
    {
        if (_eventCenter.TryGetValue(eventName, out var eventHelp))
        {
            (eventHelp as EventHelp)?.AddCall(action);
        }
        else
        {
            // 如果事件中心不存在叫这个名字的事件，new一个然后添加
            _eventCenter.Add(eventName, new EventHelp(action));
        }
    }
    public void AddEventListening<T>(string eventName, Action<T> action)
    {
        if (_eventCenter.TryGetValue(eventName, out var eventHelp))
        {
            (eventHelp as EventHelp<T>)?.AddCall(action);
        }
        else
        {
            // 如果事件中心不存在叫这个名字的事件，new一个然后添加
            _eventCenter.Add(eventName, new EventHelp<T>(action));
        }
    }
    public void AddEventListening<T1, T2>(string eventName, Action<T1, T2> action)
    {
        if (_eventCenter.TryGetValue(eventName, out var eventHelp))
        {
            (eventHelp as EventHelp<T1, T2>)?.AddCall(action);
        }
        else
        {
            // 如果事件中心不存在叫这个名字的事件，new一个然后添加
            _eventCenter.Add(eventName, new EventHelp<T1, T2>(action));
        }
    }

    /// <summary>
    /// 调用事件
    /// </summary>
    /// <param name="eventName">事件名称</param>
    public void CallEvent(string eventName)
    {
        if (_eventCenter.TryGetValue(eventName, out var eventHelp))
        {
            (eventHelp as EventHelp)?.Call();
        }
        else
        {
            LogEventNotFound(eventName, "调用");
        }
    }

    public void CallEvent<T>(string eventName, T value)
    {
        if (_eventCenter.TryGetValue(eventName, out var eventHelp))
        {
            (eventHelp as EventHelp<T>)?.Call(value);
        }
        else
        {
            LogEventNotFound(eventName, "调用");
        }
    }

    public void CallEvent<T1, T2>(string eventName, T1 value, T2 value1)
    {
        if (_eventCenter.TryGetValue(eventName, out var eventHelp))
        {
            (eventHelp as EventHelp<T1, T2>)?.Call(value, value1);
        }
        else
        {
            LogEventNotFound(eventName, "调用");
        }
    }


    /// <summary>
    /// 移除事件监听
    /// </summary>
    /// <param name="eventName">事件名称</param>
    /// <param name="action">要移除的事件回调</param>
    public void RemoveEvent(string eventName, Action action)
    {
        if (_eventCenter.TryGetValue(eventName, out var eventHelp))
        {
            (eventHelp as EventHelp)?.Remove(action);
        }
        else
        {
            LogEventNotFound(eventName, "移除");
        }
    }

    public void RemoveEvent<T>(string eventName, Action<T> action)
    {
        if (_eventCenter.TryGetValue(eventName, out var eventHelp))
        {
            (eventHelp as EventHelp<T>)?.Remove(action);
        }
        else
        {
            LogEventNotFound(eventName, "移除");
        }
    }
    public void RemoveEvent<T1, T2>(string eventName, Action<T1, T2> action)
    {
        if (_eventCenter.TryGetValue(eventName, out var eventHelp))
        {
            (eventHelp as EventHelp<T1, T2>)?.Remove(action);
        }
        else
        {
            LogEventNotFound(eventName, "移除");
        }
    }

    /// <summary>
    /// 事件未找到时的统一日志输出
    /// </summary>
    /// <param name="eventName">事件名称</param>
    /// <param name="operation">操作类型（移除、调用）</param>
    private void LogEventNotFound(string eventName, string operation)
    {
        DevelopmentTools.WTF($"当前未找到{eventName}的事件，无法{operation}");
    }

}
