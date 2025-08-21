using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// �������Ʒ�ӿ�
/// </summary>
public interface IPoolItem
{
    void Spawn();   // ������Ӷ����ȡ��������ʱִ�е��߼��������ʼ��״̬����ʾ��Ч��
    void Recycle(); // ��������յ������ʱִ�е��߼�����������״̬�����ض����
}

/// <summary>
/// �������Ʒ���࣬�̳���MonoBehaviour��ʵ��IPoolItem�ӿ�
/// ��Ϊ����������Ʒ�����ӵ������ߵȣ��ĳ����࣬��װͨ���߼�
/// </summary>
public abstract class PoolItemBase : MonoBehaviour, IPoolItem
{

    private void OnEnable()
    {
        Spawn();
    }

    private void OnDisable()
    {
        Recycle();
    }

    public virtual void Spawn()
    {

    }

    public virtual void Recycle()
    {

    }
}
