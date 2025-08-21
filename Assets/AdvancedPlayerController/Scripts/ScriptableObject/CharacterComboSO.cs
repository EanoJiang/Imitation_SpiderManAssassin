using System.Collections.Generic;
using UnityEngine;

namespace Spiderman.ComboData
{
    /// <summary>
    /// 角色连招集合数据资产类，管理一组 CharacterComboDataSO ，提供连招数据获取接口
    /// </summary>
    [CreateAssetMenu(fileName = "Combo", menuName = "CreateActions/Character/Combo", order = 0)]
    public class CharacterComboSO : ScriptableObject
    {
        #region 私有字段（连招数据集合）
        [SerializeField]
        private List<CharacterComboDataSO> _allComboData = new List<CharacterComboDataSO>();
        #endregion

        #region 公共方法（连招数据获取）
        /// <summary>
        /// 获取指定连招索引的连招名称
        /// </summary>
        /// <param name="index">连招在 _allComboData 中的索引</param>
        /// <returns>连招名称，索引无效时返回 null</returns>
        public string TryGetOneComboAction(int index)
        {
            // 校验：连招数据列表为空，直接返回 null
            if (_allComboData.Count == 0)
                return null;

            // 校验：索引越界保护（避免数组越界异常）
            if (index < 0 || index >= _allComboData.Count)
                return null;

            return _allComboData[index].ComboName;
        }

        /// <summary>
        /// 获取指定连招、指定命中动作索引的受伤动作名称
        /// </summary>
        /// <param name="comboIndex">连招在 _allComboData 中的索引</param>
        /// <param name="hitIndex">命中动作在 ComboHitName 数组中的索引</param>
        /// <returns>命中动作名称，索引无效时返回 null</returns>
        public string TryGetOneHitName(int comboIndex, int hitIndex)
        {
            // 先校验连招数据列表是否为空
            if (_allComboData.Count == 0)
                return null;

            // 校验连招索引是否合法
            if (comboIndex < 0 || comboIndex >= _allComboData.Count)
                return null;

            CharacterComboDataSO targetCombo = _allComboData[comboIndex];
            // 校验命中动作数组长度，避免空引用、索引越界
            if (targetCombo.ComboHitName == null || targetCombo.ComboHitName.Length == 0)
                return null;
            if (hitIndex < 0 || hitIndex >= targetCombo.ComboHitName.Length)
                return null;

            return targetCombo.ComboHitName[hitIndex];
        }

        /// <summary>
        /// 获取指定连招、指定招架动作索引的招架动作名称
        /// </summary>
        /// <param name="comboIndex">连招在 _allComboData 中的索引</param>
        /// <param name="hitIndex">招架动作在 ComboParryName 数组中的索引</param>
        /// <returns>招架动作名称，索引无效时返回 null</returns>
        public string TryGetOneParryName(int comboIndex, int hitIndex)
        {
            // 先校验连招数据列表是否为空
            if (_allComboData.Count == 0)
                return null;

            // 校验连招索引是否合法
            if (comboIndex < 0 || comboIndex >= _allComboData.Count)
                return null;

            CharacterComboDataSO targetCombo = _allComboData[comboIndex];
            // 校验招架动作数组长度，避免空引用、索引越界
            if (targetCombo.ComboParryName == null || targetCombo.ComboParryName.Length == 0)
                return null;
            if (hitIndex < 0 || hitIndex >= targetCombo.ComboParryName.Length)
                return null;

            return targetCombo.ComboParryName[hitIndex];
        }

        /// <summary>
        /// 获取连招的伤害值
        /// </summary>
        /// <param name="index">连招数据在 _allComboData 中的索引</param>
        /// <returns>连招的伤害值，若索引无效则返回 0</returns>
        public float TryGetComboDamage(int index)
        {
            // 校验：连招数据列表为空，返回默认值 0
            if (_allComboData.Count == 0)
                return 0f;

            // 校验：索引越界保护（避免数组越界异常）
            if (index < 0 || index >= _allComboData.Count)
                return 0f;

            return _allComboData[index].ComboDamage;
        }

        /// <summary>
        /// 获取连招的冷却时间
        /// </summary>
        /// <param name="index">连招数据在 _allComboData 中的索引</param>
        /// <returns>连招的冷却时间，若索引无效则返回 0</returns>
        public float TryGetColdTime(int index)
        {
            // 校验：连招数据列表为空，返回默认值 0
            if (_allComboData.Count == 0)
                return 0f;

            // 校验：索引越界保护（避免数组越界异常）
            if (index < 0 || index >= _allComboData.Count)
                return 0f;

            return _allComboData[index].ColdTime;
        }

        /// <summary>
        /// 获取连招的位置偏移
        /// </summary>
        /// <param name="index">连招数据在 _allComboData 中的索引</param>
        /// <returns>连招的位置偏移，若索引无效则返回 0</returns>
        public float TryGetComboPositionOffset(int index)
        {
            // 校验：连招数据列表为空，返回默认值 0
            if (_allComboData.Count == 0)
                return 0f;

            // 校验：索引越界保护（避免数组越界异常）
            if (index < 0 || index >= _allComboData.Count)
                return 0f;

            return _allComboData[index].ComboPositionOffset;
        }

        /// <summary>
        /// 获取连招的最大命中/招架动作数量
        /// </summary>
        /// <param name="index">连招数据在 _allComboData 中的索引</param>
        /// <returns>连招的最大命中/招架动作数量，若索引无效则返回 0</returns>
        public int TryGetHitOrParryMaxCount(int index)
        {
            // 校验：连招数据列表为空，返回默认值 0
            if (_allComboData.Count == 0)
                return 0;

            // 校验：索引越界保护（避免数组越界异常）
            if (index < 0 || index >= _allComboData.Count)
                return 0;

            return _allComboData[index].GetHitAndParryNameMaxCount();
        }

        /// <summary>
        /// 获取连招数据的总数
        /// </summary>
        /// <returns>连招数据的总数</returns>
        public int TryGetComboMaxCount()
        {
            return _allComboData.Count;
        }
        #endregion
    }
}