using Spiderman.Assets;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 声音类型枚举
/// </summary>
public enum SoundType
{
    ATK,    // 攻击
    HIT,    // 受击
    BLOCK,  // 格挡
    FOOT    // 脚步
}

/// <summary>
/// 声音对象池物品类
/// 用于管理音效播放对象的激活、回收，复用AudioSource
/// </summary>
public class PoolItemSound : PoolItemBase
{
    // 音频源
    private AudioSource _audioSource;
    [SerializeField] SoundType _soundType;
    [SerializeField] AssetsSoundSO _soundAssets;

    private void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
    }

    /// <summary>
    /// 音效对象从对象池取出
    /// </summary>
    public override void Spawn()
    {
        //被激活的时候播放音效
        PlaySound(); 
    }

    /// <summary>
    /// 播放音效
    /// </summary>
    private void PlaySound()
    {
        _audioSource.clip = _soundAssets.GetAudioClip(_soundType);
        _audioSource.Play();
        // 回收音效对象
        StartRecycle();
    }

    /// <summary>
    /// 音效对象回收
    /// </summary>
    private void StartRecycle()
    {
        // 延迟0.3秒后停止播放
        TimerManager.Instance.TryGetOneTimer(0.3f, DisableSelf);
    }

    /// <summary>
    /// 定时任务：停止播放
    /// </summary>
    private void DisableSelf()
    {
        _audioSource.Stop();
        gameObject.SetActive(false);
    }


}