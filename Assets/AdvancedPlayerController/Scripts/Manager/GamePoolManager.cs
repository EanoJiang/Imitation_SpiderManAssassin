using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GGG.Tool.Singleton;
using GGG.Tool;

public class GamePoolManager : Singleton<GamePoolManager>
{
    // 1. ������������
    [System.Serializable]
    private class PoolItem
    {
        public string ItemName;      // �������ƣ����ڱ�ʶ
        public GameObject Item;      // Ҫ�������Ϸ����
        public int InitMaxCount;     // ��ʼ��󻺴�����
    }

    // 2. ���������б�
    [SerializeField]
    private List<PoolItem> _configPoolItem = new List<PoolItem>();

    private Dictionary<string, Queue<GameObject>> _poolCenter = new Dictionary<string, Queue<GameObject>>();
    //����ظ�����
    private GameObject _poolItemParent;

    private void Start()
    {
        _poolItemParent = new GameObject("PoolItemParent");
        //�ŵ�GamePoolManager���Ӽ���ͳһ����
        _poolItemParent.transform.SetParent(this.transform);
        InitPool();
    }

    private void InitPool()
    {
        // 1. �����ж��ⲿ�����ǲ��ǿյġ�
        if (_configPoolItem.Count == 0)
            return;

        for (var i = 0; i < _configPoolItem.Count; i++)
        {
            for (int j = 0; j < _configPoolItem[i].InitMaxCount; j++)
            {
                var item = Instantiate(_configPoolItem[i].Item);
                // ����������Ϊ���ɼ�
                item.SetActive(false);
                // ����ΪPoolItemParent��������
                item.transform.SetParent(_poolItemParent.transform);
                // �жϳ�������û�д�����������
                if (!_poolCenter.ContainsKey(_configPoolItem[i].ItemName))
                {
                    // �����ǰ�������û�ж�Ӧ���Ƶĳ��ӣ���ô������Ҫ����һ��
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
    /// �Ӷ�����г��Ի�ȡָ�����ƵĶ��󣬲�������λ�ú���ת��Ϣ
    /// </summary>
    /// <param name="name">Ҫ��ȡ�Ķ�������ƣ����ڱ�ʶ�ض����͵Ķ���</param>
    /// <param name="position">���󼤻�����������λ��</param>
    /// <param name="rotation">���󼤻�������ռ���ת�Ƕ�</param>
    public void TryGetPoolItem(string name, Vector3 position, Quaternion rotation)
    {
        // ��������������Ƿ����ָ�����ƵĶ����
        if (_poolCenter.ContainsKey(name))
        {
            // �Ӷ�Ӧ���ƵĶ���ض�����ȡ�����׵Ķ��󣨳��Ӳ�����
            var item = _poolCenter[name].Dequeue();

            // ���ö����λ����Ϣ
            item.transform.position = position;

            // ���ö������ת��Ϣ
            item.transform.rotation = rotation;

            // �������
            item.SetActive(true);

            // ��ʹ�ú�Ķ������·Żض���β����ʵ�ֶ����ã�����Ƶ���������٣�
            _poolCenter[name].Enqueue(item);
        }
        else
        {
            // ������Ķ���ز�����ʱ
            Debug.Log(message: $"��ǰ����Ķ����{name}������");
        }
    }

    /// <summary>
    /// �Ӷ�����г��Ի�ȡָ�����ƵĶ������ط�������ָ��λ�ú���ת��
    /// </summary>
    /// <param name="name">Ҫ��ȡ�Ķ��������</param>
    /// <returns>��ȡ������Ϸ����������ز������򷵻�null</returns>
    public GameObject TryGetPoolItem(string name)
    {
        // ��������������Ƿ����ָ�����ƵĶ����
        if (_poolCenter.ContainsKey(name))
        {
            // �Ӷ�Ӧ���ƵĶ���ض�����ȡ�����׵Ķ���
            var item = _poolCenter[name].Dequeue();

            // �������
            item.SetActive(true);

            // ��ʹ�ú�Ķ������·Żض���β��
            _poolCenter[name].Enqueue(item);

            return item;
        }

        // ������Ķ���ز�����ʱ
        Debug.Log(message: $"��ǰ����Ķ����{name}������");
        return null;
    }
}
