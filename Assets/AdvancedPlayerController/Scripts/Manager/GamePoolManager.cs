using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GGG.Tool.Singleton;
using GGG.Tool;

public class GamePoolManager : Singleton<GamePoolManager>
{
    // 1. 缓存配置项类
    [System.Serializable]
    private class PoolItem
    {
        public string ItemName;      // 对象名称，用于标识
        public GameObject Item;      // 要缓存的游戏对象
        public int InitMaxCount;     // 初始最大缓存数量
    }

    // 2. 缓存配置列表
    [SerializeField]
    private List<PoolItem> _configPoolItem = new List<PoolItem>();

    private Dictionary<string, Queue<GameObject>> _poolCenter = new Dictionary<string, Queue<GameObject>>();
    //对象池父对象
    private GameObject _poolItemParent;

    private void Start()
    {
        _poolItemParent = new GameObject("PoolItemParent");
        //放到GamePoolManager的子级，统一管理
        _poolItemParent.transform.SetParent(this.transform);
        InitPool();
    }

    private void InitPool()
    {
        // 1. 我们判断外部配置是不是空的。
        if (_configPoolItem.Count == 0)
            return;

        for (var i = 0; i < _configPoolItem.Count; i++)
        {
            for (int j = 0; j < _configPoolItem[i].InitMaxCount; j++)
            {
                var item = Instantiate(_configPoolItem[i].Item);
                // 将对象设置为不可见
                item.SetActive(false);
                // 设置为PoolItemParent的子物体
                item.transform.SetParent(_poolItemParent.transform);
                // 判断池子中有没有存在这个对象的
                if (!_poolCenter.ContainsKey(_configPoolItem[i].ItemName))
                {
                    // 如果当前对象池中没有对应名称的池子，那么我们需要创建一个
                    _poolCenter.Add(
                        _configPoolItem[i].ItemName,
                        new Queue<GameObject>()
                    );
                    _poolCenter[_configPoolItem[i].ItemName].Enqueue(item);
                }
                else
                {
                    _poolCenter[_configPoolItem[i].ItemName].Enqueue(item);
                }
            }
        }
        //Debug.Log(_poolCenter.Count);
        //Debug.Log(_poolCenter["ATKSound"].Count);
    }

    /// <summary>
    /// 从对象池中尝试获取指定名称的对象，并设置其位置和旋转信息
    /// </summary>
    /// <param name="name">要获取的对象池名称（用于标识特定类型的对象）</param>
    /// <param name="position">对象激活后的世界坐标位置</param>
    /// <param name="rotation">对象激活后的世界空间旋转角度</param>
    public void TryGetPoolItem(string name, Vector3 position, Quaternion rotation)
    {
        // 检查对象池容器中是否存在指定名称的对象池
        if (_poolCenter.ContainsKey(name))
        {
            // 从对应名称的对象池队列中取出队首的对象（出队操作）
            var item = _poolCenter[name].Dequeue();

            // 设置对象的位置信息
            item.transform.position = position;

            // 设置对象的旋转信息
            item.transform.rotation = rotation;

            // 激活对象
            item.SetActive(true);

            // 将使用后的对象重新放回队列尾部（实现对象复用，避免频繁创建销毁）
            _poolCenter[name].Enqueue(item);
        }
        else
        {
            // 当请求的对象池不存在时
            Debug.Log(message: $"当前请求的对象池{name}不存在");
        }
    }

    /// <summary>
    /// 从对象池中尝试获取指定名称的对象（重载方法，不指定位置和旋转）
    /// </summary>
    /// <param name="name">要获取的对象池名称</param>
    /// <returns>获取到的游戏对象，若对象池不存在则返回null</returns>
    public GameObject TryGetPoolItem(string name)
    {
        // 检查对象池容器中是否存在指定名称的对象池
        if (_poolCenter.ContainsKey(name))
        {
            // 从对应名称的对象池队列中取出队首的对象
            var item = _poolCenter[name].Dequeue();

            // 激活对象
            item.SetActive(true);

            // 将使用后的对象重新放回队列尾部
            _poolCenter[name].Enqueue(item);

            return item;
        }

        // 当请求的对象池不存在时
        Debug.Log(message: $"当前请求的对象池{name}不存在");
        return null;
    }
}
