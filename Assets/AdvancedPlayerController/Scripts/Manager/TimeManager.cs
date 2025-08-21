using System;
using System.Collections;
using System.Collections.Generic;
using GGG.Tool;
using GGG.Tool.Singleton;
using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// 计时器管理器，采用单例模式，负责管理空闲计时器队列和工作中计时器列表，
/// 实现计时器的初始化、分配、回收及更新逻辑
/// </summary>
public class TimerManager : Singleton<TimerManager>
{
    #region 私有字段
    // 初始最大计时器数量，在 Inspector 中配置
    [SerializeField] private int _initMaxTimerCount;

    // 空闲计时器队列，存储可用的 GameTimer
    private Queue<GameTimer> _notWorkingTimer = new Queue<GameTimer>();
    // 工作中计时器列表，存储正在计时的 GameTimer
    private List<GameTimer> _workingTimer = new List<GameTimer>();
    #endregion

    #region 生命周期与初始化
    protected override void Awake()
    {
        base.Awake();
        InitTimerManager();
    }

    /// <summary>
    /// 初始化计时器管理器，创建初始数量的空闲计时器
    /// </summary>
    private void InitTimerManager()
    {
        for (int i = 0; i < _initMaxTimerCount; i++)
        {
            CreateTimerInternal();
        }
    }

    /// <summary>
    /// 内部创建计时器并加入空闲队列的方法
    /// </summary>
    private void CreateTimerInternal()
    {
        var timer = new GameTimer();
        _notWorkingTimer.Enqueue(timer);
    }
    #endregion

    #region 计时器分配与回收
    /// <summary>
    /// 尝试获取一个计时器，用于执行定时任务
    /// </summary>
    /// <param name="time">计时时长</param>
    /// <param name="task">计时结束后执行的任务</param>
    public void TryGetOneTimer(float time, Action task)
    {
        // 若空闲队列为空，额外创建一个计时器
        if (_notWorkingTimer.Count == 0)
        {
            CreateTimerInternal();
        }

        var timer = _notWorkingTimer.Dequeue();
        timer.StartTimer(time, task);
        _workingTimer.Add(timer);
    }

    /// <summary>
    /// 回收计时器（可在 GameTimer 完成任务时调用，这里逻辑已内联在更新里，也可扩展外部调用）
    /// 注：当前通过 UpdateWorkingTimer 自动回收，此方法可留作扩展
    /// </summary>
    /// <param name="timer">要回收的计时器</param>
    private void RecycleTimer(GameTimer timer)
    {
        timer.ResetTimer();
        _notWorkingTimer.Enqueue(timer);
        _workingTimer.Remove(timer);
    }
    #endregion

    #region 计时器更新逻辑
    private void Update()
    {
        UpdateWorkingTimer();
    }

    /// <summary>
    /// 更新工作中计时器的状态，处理计时推进和完成后的回收
    /// </summary>
    private void UpdateWorkingTimer()
    {
        // 遍历副本，避免列表修改时迭代出错
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