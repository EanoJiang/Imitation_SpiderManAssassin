using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using GGG.Tool;
using GGG.Tool.Singleton;

public class GameEventManager : SingletonNonMono<GameEventManager>
{
    // �¼��ӿ�
    private interface IEventHelp
    {
    }

    // �¼��࣬ʵ�� IEventHelp �ӿڣ����ڹ����¼�ע�ᡢ���õ��߼�
    private class EventHelp : IEventHelp
    {
        // �洢�¼�ί��
        private event Action _action;

        // ���캯������ʼ��ʱ�󶨳�ʼ�¼��߼�
        public EventHelp(Action action)
        {
            // �״�ʵ����ʱ��ֵ����ִ����һ�γ�ʼ��
            _action = action;
        }

        // �����¼�ע��ķ��������µ��¼��߼�׷�ӵ�ί����
        public void AddCall(Action action)
        {
            _action += action;
        }

        // �����¼��ķ��������а��߼���ִ��
        public void Call()
        {
            _action?.Invoke();
        }

        // �Ƴ��¼��ķ�������ָ���¼��߼���ί�����Ƴ�
        public void Remove(Action action)
        {
            _action -= action;
        }
    }

    private class EventHelp<T> : IEventHelp
    {
        // �洢�¼�ί��
        private event Action<T> _action;

        // ���캯������ʼ��ʱ�󶨳�ʼ�¼��߼�
        public EventHelp(Action<T> action)
        {
            // �״�ʵ����ʱ��ֵ����ִ����һ�γ�ʼ��
            _action = action;
        }

        // �����¼�ע��ķ��������µ��¼��߼�׷�ӵ�ί����
        public void AddCall(Action<T> action)
        {
            _action += action;
        }

        // �����¼��ķ��������а��߼���ִ��
        public void Call(T value)
        {
            _action?.Invoke(value);
        }

        // �Ƴ��¼��ķ�������ָ���¼��߼���ί�����Ƴ�
        public void Remove(Action<T> action)
        {
            _action -= action;
        }
    }

    private class EventHelp<T1, T2> : IEventHelp
    {
        // �洢�¼�ί��
        private event Action<T1, T2> _action;

        // ���캯������ʼ��ʱ�󶨳�ʼ�¼��߼�
        public EventHelp(Action<T1, T2> action)
        {
            // �״�ʵ����ʱ��ֵ����ִ����һ�γ�ʼ��
            _action = action;
        }

        // �����¼�ע��ķ��������µ��¼��߼�׷�ӵ�ί����
        public void AddCall(Action<T1, T2> action)
        {
            _action += action;
        }

        // �����¼��ķ��������а��߼���ִ��
        public void Call(T1 value1, T2 value2)
        {
            _action?.Invoke(value1, value2);
        }

        // �Ƴ��¼��ķ�������ָ���¼��߼���ί�����Ƴ�
        public void Remove(Action<T1, T2> action)
        {
            _action -= action;
        }
    }

    /// <summary>
    /// �¼����ģ����ڹ����¼�ע�ᡢ����
    /// </summary>
    private Dictionary<string, IEventHelp> _eventCenter = new Dictionary<string, IEventHelp>();

    /// <summary>
    /// ����¼�����
    /// </summary>
    /// <param name="eventName">�¼�����</param>
    /// <param name="action">�ص�����</param>
    public void AddEventListening(string eventName, Action action)
    {
        if (_eventCenter.TryGetValue(eventName, out var eventHelp))
        {
            (eventHelp as EventHelp)?.AddCall(action);
        }
        else
        {
            // ����¼����Ĳ����ڽ�������ֵ��¼���newһ��Ȼ�����
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
            // ����¼����Ĳ����ڽ�������ֵ��¼���newһ��Ȼ�����
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
            // ����¼����Ĳ����ڽ�������ֵ��¼���newһ��Ȼ�����
            _eventCenter.Add(eventName, new EventHelp<T1, T2>(action));
        }
    }

    /// <summary>
    /// �����¼�
    /// </summary>
    /// <param name="eventName">�¼�����</param>
    public void CallEvent(string eventName)
    {
        if (_eventCenter.TryGetValue(eventName, out var eventHelp))
        {
            (eventHelp as EventHelp)?.Call();
        }
        else
        {
            LogEventNotFound(eventName, "����");
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
            LogEventNotFound(eventName, "����");
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
            LogEventNotFound(eventName, "����");
        }
    }


    /// <summary>
    /// �Ƴ��¼�����
    /// </summary>
    /// <param name="eventName">�¼�����</param>
    /// <param name="action">Ҫ�Ƴ����¼��ص�</param>
    public void RemoveEvent(string eventName, Action action)
    {
        if (_eventCenter.TryGetValue(eventName, out var eventHelp))
        {
            (eventHelp as EventHelp)?.Remove(action);
        }
        else
        {
            LogEventNotFound(eventName, "�Ƴ�");
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
            LogEventNotFound(eventName, "�Ƴ�");
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
            LogEventNotFound(eventName, "�Ƴ�");
        }
    }

    /// <summary>
    /// �¼�δ�ҵ�ʱ��ͳһ��־���
    /// </summary>
    /// <param name="eventName">�¼�����</param>
    /// <param name="operation">�������ͣ��Ƴ������ã�</param>
    private void LogEventNotFound(string eventName, string operation)
    {
        DevelopmentTools.WTF($"��ǰδ�ҵ�{eventName}���¼����޷�{operation}");
    }

}
