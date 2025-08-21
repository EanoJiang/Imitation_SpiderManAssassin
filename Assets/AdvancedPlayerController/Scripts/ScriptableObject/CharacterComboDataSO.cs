using UnityEngine;

namespace Spiderman.ComboData
{
    /// <summary>
    /// 角色连招数据资产类，用于定义角色连招相关的配置信息，可作为 ScriptableObject 资产创建和编辑
    /// </summary>
    [CreateAssetMenu(fileName = "ComboData", menuName = "CreateActions/Character/ComboData", order = 0)]
    public class CharacterComboDataSO : ScriptableObject
    {
        #region 私有字段（连招配置数据）
        [SerializeField] private string _comboName; // 连招名称（对应动画片段名，用于关联动画）
        [SerializeField] private string[] _comboHitName; // 连招命中动作名称数组
        [SerializeField] private string[] _comboParryName; // 连招招架动作名称数组
        [SerializeField] private float _damage; // 连招伤害值
        [SerializeField] private float _coldTime; // 衔接下一段攻击的间隔时间
        [SerializeField] private float _comboPositionOffset; // 攻击与目标间保持的最佳距离偏移
        #endregion

        #region 公共属性（对外访问接口）
        /// <summary>
        /// 获取或设置连招名称
        /// </summary>
        public string ComboName => _comboName;

        /// <summary>
        /// 获取连招命中动作名称数组
        /// </summary>
        public string[] ComboHitName => _comboHitName;

        /// <summary>
        /// 获取连招招架动作名称数组
        /// </summary>
        public string[] ComboParryName => _comboParryName;

        /// <summary>
        /// 获取或设置连招伤害值
        /// </summary>
        public float ComboDamage => _damage;

        /// <summary>
        /// 获取或设置衔接下一段攻击的间隔时间
        /// </summary>
        public float ColdTime => _coldTime;

        /// <summary>
        /// 获取或设置攻击与目标间最佳距离偏移
        /// </summary>
        public float ComboPositionOffset => _comboPositionOffset;
        #endregion

        #region 公共方法（业务逻辑）
        /// <summary>
        /// 获取当前连招命中/招架动作名称数组的最大长度（用于遍历或判定动作数量）
        /// </summary>
        /// <returns>动作名称数组的长度</returns>
        public int GetHitAndParryNameMaxCount() => _comboHitName.Length;
        #endregion
    }
}