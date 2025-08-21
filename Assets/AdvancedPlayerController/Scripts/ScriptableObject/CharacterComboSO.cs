using System.Collections.Generic;
using UnityEngine;

namespace Spiderman.ComboData
{
    /// <summary>
    /// ��ɫ���м��������ʲ��࣬����һ�� CharacterComboDataSO ���ṩ�������ݻ�ȡ�ӿ�
    /// </summary>
    [CreateAssetMenu(fileName = "Combo", menuName = "CreateActions/Character/Combo", order = 0)]
    public class CharacterComboSO : ScriptableObject
    {
        #region ˽���ֶΣ��������ݼ��ϣ�
        [SerializeField]
        private List<CharacterComboDataSO> _allComboData = new List<CharacterComboDataSO>();
        #endregion

        #region �����������������ݻ�ȡ��
        /// <summary>
        /// ��ȡָ��������������������
        /// </summary>
        /// <param name="index">������ _allComboData �е�����</param>
        /// <returns>�������ƣ�������Чʱ���� null</returns>
        public string TryGetOneComboAction(int index)
        {
            // У�飺���������б�Ϊ�գ�ֱ�ӷ��� null
            if (_allComboData.Count == 0)
                return null;

            // У�飺����Խ�籣������������Խ���쳣��
            if (index < 0 || index >= _allComboData.Count)
                return null;

            return _allComboData[index].ComboName;
        }

        /// <summary>
        /// ��ȡָ�����С�ָ�����ж������������˶�������
        /// </summary>
        /// <param name="comboIndex">������ _allComboData �е�����</param>
        /// <param name="hitIndex">���ж����� ComboHitName �����е�����</param>
        /// <returns>���ж������ƣ�������Чʱ���� null</returns>
        public string TryGetOneHitName(int comboIndex, int hitIndex)
        {
            // ��У�����������б��Ƿ�Ϊ��
            if (_allComboData.Count == 0)
                return null;

            // У�����������Ƿ�Ϸ�
            if (comboIndex < 0 || comboIndex >= _allComboData.Count)
                return null;

            CharacterComboDataSO targetCombo = _allComboData[comboIndex];
            // У�����ж������鳤�ȣ���������á�����Խ��
            if (targetCombo.ComboHitName == null || targetCombo.ComboHitName.Length == 0)
                return null;
            if (hitIndex < 0 || hitIndex >= targetCombo.ComboHitName.Length)
                return null;

            return targetCombo.ComboHitName[hitIndex];
        }

        /// <summary>
        /// ��ȡָ�����С�ָ���мܶ����������мܶ�������
        /// </summary>
        /// <param name="comboIndex">������ _allComboData �е�����</param>
        /// <param name="hitIndex">�мܶ����� ComboParryName �����е�����</param>
        /// <returns>�мܶ������ƣ�������Чʱ���� null</returns>
        public string TryGetOneParryName(int comboIndex, int hitIndex)
        {
            // ��У�����������б��Ƿ�Ϊ��
            if (_allComboData.Count == 0)
                return null;

            // У�����������Ƿ�Ϸ�
            if (comboIndex < 0 || comboIndex >= _allComboData.Count)
                return null;

            CharacterComboDataSO targetCombo = _allComboData[comboIndex];
            // У���мܶ������鳤�ȣ���������á�����Խ��
            if (targetCombo.ComboParryName == null || targetCombo.ComboParryName.Length == 0)
                return null;
            if (hitIndex < 0 || hitIndex >= targetCombo.ComboParryName.Length)
                return null;

            return targetCombo.ComboParryName[hitIndex];
        }

        /// <summary>
        /// ��ȡ���е��˺�ֵ
        /// </summary>
        /// <param name="index">���������� _allComboData �е�����</param>
        /// <returns>���е��˺�ֵ����������Ч�򷵻� 0</returns>
        public float TryGetComboDamage(int index)
        {
            // У�飺���������б�Ϊ�գ�����Ĭ��ֵ 0
            if (_allComboData.Count == 0)
                return 0f;

            // У�飺����Խ�籣������������Խ���쳣��
            if (index < 0 || index >= _allComboData.Count)
                return 0f;

            return _allComboData[index].ComboDamage;
        }

        /// <summary>
        /// ��ȡ���е���ȴʱ��
        /// </summary>
        /// <param name="index">���������� _allComboData �е�����</param>
        /// <returns>���е���ȴʱ�䣬��������Ч�򷵻� 0</returns>
        public float TryGetColdTime(int index)
        {
            // У�飺���������б�Ϊ�գ�����Ĭ��ֵ 0
            if (_allComboData.Count == 0)
                return 0f;

            // У�飺����Խ�籣������������Խ���쳣��
            if (index < 0 || index >= _allComboData.Count)
                return 0f;

            return _allComboData[index].ColdTime;
        }

        /// <summary>
        /// ��ȡ���е�λ��ƫ��
        /// </summary>
        /// <param name="index">���������� _allComboData �е�����</param>
        /// <returns>���е�λ��ƫ�ƣ���������Ч�򷵻� 0</returns>
        public float TryGetComboPositionOffset(int index)
        {
            // У�飺���������б�Ϊ�գ�����Ĭ��ֵ 0
            if (_allComboData.Count == 0)
                return 0f;

            // У�飺����Խ�籣������������Խ���쳣��
            if (index < 0 || index >= _allComboData.Count)
                return 0f;

            return _allComboData[index].ComboPositionOffset;
        }

        /// <summary>
        /// ��ȡ���е��������/�мܶ�������
        /// </summary>
        /// <param name="index">���������� _allComboData �е�����</param>
        /// <returns>���е��������/�мܶ�����������������Ч�򷵻� 0</returns>
        public int TryGetHitOrParryMaxCount(int index)
        {
            // У�飺���������б�Ϊ�գ�����Ĭ��ֵ 0
            if (_allComboData.Count == 0)
                return 0;

            // У�飺����Խ�籣������������Խ���쳣��
            if (index < 0 || index >= _allComboData.Count)
                return 0;

            return _allComboData[index].GetHitAndParryNameMaxCount();
        }

        /// <summary>
        /// ��ȡ�������ݵ�����
        /// </summary>
        /// <returns>�������ݵ�����</returns>
        public int TryGetComboMaxCount()
        {
            return _allComboData.Count;
        }
        #endregion
    }
}