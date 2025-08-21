using System;
using System.Collections;
using System.Collections.Generic;
using GGG.Tool;
using GGG.Tool.Singleton;
using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// ��ʱ�������������õ���ģʽ�����������м�ʱ�����к͹����м�ʱ���б�
/// ʵ�ּ�ʱ���ĳ�ʼ�������䡢���ռ������߼�
/// </summary>
public class TimerManager : Singleton<TimerManager>
{
    #region ˽���ֶ�
    // ��ʼ����ʱ���������� Inspector ������
    [SerializeField] private int _initMaxTimerCount;

    // ���м�ʱ�����У��洢���õ� GameTimer
    private Queue<GameTimer> _notWorkingTimer = new Queue<GameTimer>();
    // �����м�ʱ���б��洢���ڼ�ʱ�� GameTimer
    private List<GameTimer> _workingTimer = new List<GameTimer>();
    #endregion

    #region �����������ʼ��
    protected override void Awake()
    {
        base.Awake();
        InitTimerManager();
    }

    /// <summary>
    /// ��ʼ����ʱ����������������ʼ�����Ŀ��м�ʱ��
    /// </summary>
    private void InitTimerManager()
    {
        for (int i = 0; i < _initMaxTimerCount; i++)
        {
            CreateTimerInternal();
        }
    }

    /// <summary>
    /// �ڲ�������ʱ����������ж��еķ���
    /// </summary>
    private void CreateTimerInternal()
    {
        var timer = new GameTimer();
        _notWorkingTimer.Enqueue(timer);
    }
    #endregion

    #region ��ʱ�����������
    /// <summary>
    /// ���Ի�ȡһ����ʱ��������ִ�ж�ʱ����
    /// </summary>
    /// <param name="time">��ʱʱ��</param>
    /// <param name="task">��ʱ������ִ�е�����</param>
    public void TryGetOneTimer(float time, Action task)
    {
        // �����ж���Ϊ�գ����ⴴ��һ����ʱ��
        if (_notWorkingTimer.Count == 0)
        {
            CreateTimerInternal();
        }

        var timer = _notWorkingTimer.Dequeue();
        timer.StartTimer(time, task);
        _workingTimer.Add(timer);
    }

    /// <summary>
    /// ���ռ�ʱ�������� GameTimer �������ʱ���ã������߼��������ڸ����Ҳ����չ�ⲿ���ã�
    /// ע����ǰͨ�� UpdateWorkingTimer �Զ����գ��˷�����������չ
    /// </summary>
    /// <param name="timer">Ҫ���յļ�ʱ��</param>
    private void RecycleTimer(GameTimer timer)
    {
        timer.ResetTimer();
        _notWorkingTimer.Enqueue(timer);
        _workingTimer.Remove(timer);
    }
    #endregion

    #region ��ʱ�������߼�
    private void Update()
    {
        UpdateWorkingTimer();
    }

    /// <summary>
    /// ���¹����м�ʱ����״̬�������ʱ�ƽ�����ɺ�Ļ���
    /// </summary>
    private void UpdateWorkingTimer()
    {
        // ���������������б��޸�ʱ��������
        for (int i = _workingTimer.Count - 1; i >= 0; i--)
        {
            var timer = _workingTimer[i];
            timer.UpdateTimer();

            if (timer.GetTimerState() == TimerState.DONE)
            {
                RecycleTimer(timer);
            }
        }
    }
    #endregion
}