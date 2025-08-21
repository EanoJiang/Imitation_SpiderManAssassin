using UnityEngine;

namespace Spiderman.ComboData
{
    /// <summary>
    /// ��ɫ���������ʲ��࣬���ڶ����ɫ������ص�������Ϣ������Ϊ ScriptableObject �ʲ������ͱ༭
    /// </summary>
    [CreateAssetMenu(fileName = "ComboData", menuName = "CreateActions/Character/ComboData", order = 0)]
    public class CharacterComboDataSO : ScriptableObject
    {
        #region ˽���ֶΣ������������ݣ�
        [SerializeField] private string _comboName; // �������ƣ���Ӧ����Ƭ���������ڹ���������
        [SerializeField] private string[] _comboHitName; // �������ж�����������
        [SerializeField] private string[] _comboParryName; // �����мܶ�����������
        [SerializeField] private float _damage; // �����˺�ֵ
        [SerializeField] private float _coldTime; // �ν���һ�ι����ļ��ʱ��
        [SerializeField] private float _comboPositionOffset; // ������Ŀ��䱣�ֵ���Ѿ���ƫ��
        #endregion

        #region �������ԣ�������ʽӿڣ�
        /// <summary>
        /// ��ȡ��������������
        /// </summary>
        public string ComboName => _comboName;

        /// <summary>
        /// ��ȡ�������ж�����������
        /// </summary>
        public string[] ComboHitName => _comboHitName;

        /// <summary>
        /// ��ȡ�����мܶ�����������
        /// </summary>
        public string[] ComboParryName => _comboParryName;

        /// <summary>
        /// ��ȡ�����������˺�ֵ
        /// </summary>
        public float ComboDamage => _damage;

        /// <summary>
        /// ��ȡ�������ν���һ�ι����ļ��ʱ��
        /// </summary>
        public float ColdTime => _coldTime;

        /// <summary>
        /// ��ȡ�����ù�����Ŀ�����Ѿ���ƫ��
        /// </summary>
        public float ComboPositionOffset => _comboPositionOffset;
        #endregion

        #region ����������ҵ���߼���
        /// <summary>
        /// ��ȡ��ǰ��������/�мܶ��������������󳤶ȣ����ڱ������ж�����������
        /// </summary>
        /// <returns>������������ĳ���</returns>
        public int GetHitAndParryNameMaxCount() => _comboHitName.Length;
        #endregion
    }
}