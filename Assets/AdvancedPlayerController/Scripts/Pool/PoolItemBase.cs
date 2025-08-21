using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// 对象池物品接口
/// </summary>
public interface IPoolItem
{
    void Spawn();   // 当对象从对象池取出、激活时执行的逻辑，比如初始化状态、显示特效等
    void Recycle(); // 当对象回收到对象池时执行的逻辑，比如重置状态、隐藏对象等
}

/// <summary>
/// 对象池物品基类，继承自MonoBehaviour并实现IPoolItem接口
/// 作为具体对象池物品（如子弹、道具等）的抽象父类，封装通用逻辑
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
