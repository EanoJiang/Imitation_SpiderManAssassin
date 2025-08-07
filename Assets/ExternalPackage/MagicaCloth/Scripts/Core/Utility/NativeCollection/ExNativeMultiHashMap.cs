// Magica Cloth.
// Copyright (c) MagicaSoft, 2020-2021.
// https://magicasoft.jp
using System;
using System.Collections.Generic;
using Unity.Collections;

namespace MagicaCloth
{
    /// <summary>
    /// NativeMultiHashMapの機能拡張版
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    public class ExNativeMultiHashMap<TKey, TValue>
        where TKey : unmanaged, IEquatable<TKey>
        where TValue : unmanaged
    {
        /// <summary>
        /// ネイティブハッシュマップ
        /// </summary>
        NativeParallelHashMap<TKey, TValue> nativeMultiHashMap;

        /// <summary>
        /// ネイティブリストの配列数
        /// ※ジョブでエラーが出ないように事前に確保しておく
        /// </summary>
        int nativeLength;

        /// <summary>
        /// 使用キーの記録
        /// </summary>
        Dictionary<TKey, int> useKeyDict = new Dictionary<TKey, int>();

        //=========================================================================================
        public ExNativeMultiHashMap()
        {
            nativeMultiHashMap = new NativeParallelHashMap<TKey, TValue>(1, Allocator.Persistent);
            nativeLength = NativeCount;
        }

        public void Dispose()
        {
            if (nativeMultiHashMap.IsCreated)
            {
                nativeMultiHashMap.Dispose();
            }
            nativeLength = 0;
        }

        private int NativeCount
        {
            get
            {
#if UNITY_2019_3_OR_NEWER
                return nativeMultiHashMap.Count();
#else
                return nativeMultiHashMap.Length;
#endif
            }
        }

        //=========================================================================================
        public bool IsCreated
        {
            get
            {
                return nativeMultiHashMap.IsCreated;
            }
        }

        /// <summary>
        /// データ追加
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void Add(TKey key, TValue value)
        {
            nativeMultiHashMap.Add(key, value);

            if (useKeyDict.ContainsKey(key))
                useKeyDict[key] = useKeyDict[key] + 1;
            else
                useKeyDict[key] = 1;

            nativeLength = NativeCount;
        }

        /// <summary>
        /// データ削除
        /// データ削除にはコストがかかるので注意！
        /// そして何故かこの関数は削除するごとに重くなる性質があるらしい（何故？）
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void Remove(TKey key, TValue value)
        {
            if (nativeMultiHashMap.TryGetValue(key, out TValue data))
            {
                if (data.Equals(value))
                {
                    // 削除
                    nativeMultiHashMap.Remove(key);

                    var cnt = useKeyDict[key] - 1;
                    if (cnt == 0)
                        useKeyDict.Remove(key);
                }
            }
            nativeLength = NativeCount;
        }

        /// <summary>
        /// 条件判定削除
        /// </summary>
        /// <param name="func">trueを返せば削除</param>
        public void Remove(Func<TKey, TValue, bool> func)
        {
            List<TKey> removeKey = new List<TKey>();
            foreach (TKey key in useKeyDict.Keys)
            {
                TValue data;
                if (nativeMultiHashMap.TryGetValue(key, out data))
                {
                    // 削除判定
                    if (func(key, data))
                    {
                        // 削除
                        nativeMultiHashMap.Remove(key);
                        removeKey.Add(key);
                    }
                }
            }

            foreach (var key in removeKey)
                useKeyDict.Remove(key);
            nativeLength = NativeCount;
        }

        /// <summary>
        /// 条件判定置換
        /// </summary>
        /// <param name="func">trueを返せば置換</param>
        /// <param name="datafunc">新しいデータを返す</param>
        public void Replace(Func<TKey, TValue, bool> func, Func<TValue, TValue> datafunc)
        {
            foreach (TKey key in useKeyDict.Keys)
            {
                TValue data;
                if (nativeMultiHashMap.TryGetValue(key, out data))
                {
                    // 置換判定
                    if (func(key, data))
                    {
                        // 置換
                        nativeMultiHashMap.Remove(key);
                        nativeMultiHashMap.Add(key, datafunc(data));
                    }
                }
            }
        }

        /// <summary>
        /// 全データ処理
        /// </summary>
        /// <param name="act"></param>
        public void Process(Action<TKey, TValue> act)
        {
            foreach (TKey key in useKeyDict.Keys)
            {
                TValue data;
                if (nativeMultiHashMap.TryGetValue(key, out data))
                {
                    act(key, data);
                }
            }
        }

        /// <summary>
        /// 指定キーのデータ処理
        /// </summary>
        /// <param name="key"></param>
        /// <param name="act"></param>
        public void Process(TKey key, Action<TValue> act)
        {
            TValue data;
            if (nativeMultiHashMap.TryGetValue(key, out data))
            {
                act(data);
            }
        }

        /// <summary>
        /// データ存在判定
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool Contains(TKey key, TValue value)
        {
            TValue data;
            if (nativeMultiHashMap.TryGetValue(key, out data))
            {
                return data.Equals(value);
            }
            return false;
        }

        /// <summary>
        /// キー存在判定
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool Contains(TKey key)
        {
            return nativeMultiHashMap.ContainsKey(key);
        }

        /// <summary>
        /// キー削除
        /// </summary>
        /// <param name="key"></param>
        public void Remove(TKey key)
        {
            nativeMultiHashMap.Remove(key);
            useKeyDict.Remove(key);
            nativeLength = NativeCount;
        }

        /// <summary>
        /// データ数
        /// </summary>
        public int Count
        {
            get
            {
                return nativeLength;
            }
        }

        /// <summary>
        /// クリア
        /// </summary>
        public void Clear()
        {
            nativeMultiHashMap.Clear();
            useKeyDict.Clear();
            nativeLength = 0;
        }

        /// <summary>
        /// 内部のNativeMultiHashMapを取得する
        /// </summary>
        /// <returns></returns>
        public NativeParallelHashMap<TKey, TValue> Map
        {
            get
            {
                return nativeMultiHashMap;
            }
        }

        /// <summary>
        /// 使用キー辞書を取得する
        /// </summary>
        /// <returns></returns>
        public Dictionary<TKey, int> UseKeyDict
        {
            get
            {
                return useKeyDict;
            }
        }
    }
}
