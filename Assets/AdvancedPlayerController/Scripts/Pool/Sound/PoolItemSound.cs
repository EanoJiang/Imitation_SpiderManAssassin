using Spiderman.Assets;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ��������ö��
/// </summary>
public enum SoundType
{
    ATK,    // ����
    HIT,    // �ܻ�
    BLOCK,  // ��
    FOOT    // �Ų�
}

/// <summary>
/// �����������Ʒ��
/// ���ڹ�����Ч���Ŷ���ļ�����գ�����AudioSource
/// </summary>
public class PoolItemSound : PoolItemBase
{
    // ��ƵԴ
    private AudioSource _audioSource;
    [SerializeField] SoundType _soundType;
    [SerializeField] AssetsSoundSO _soundAssets;

    private void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
    }

    /// <summary>
    /// ��Ч����Ӷ����ȡ��
    /// </summary>
    public override void Spawn()
    {
        //�������ʱ�򲥷���Ч
        PlaySound(); 
    }

    /// <summary>
    /// ������Ч
    /// </summary>
    private void PlaySound()
    {
        _audioSource.clip = _soundAssets.GetAudioClip(_soundType);
        _audioSource.Play();
        // ������Ч����
        StartRecycle();
    }

    /// <summary>
    /// ��Ч�������
    /// </summary>
    private void StartRecycle()
    {
        // �ӳ�0.3���ֹͣ����
        TimerManager.Instance.TryGetOneTimer(0.3f, DisableSelf);
    }

    /// <summary>
    /// ��ʱ����ֹͣ����
    /// </summary>
    private void DisableSelf()
    {
        _audioSource.Stop();
        gameObject.SetActive(false);
    }


}