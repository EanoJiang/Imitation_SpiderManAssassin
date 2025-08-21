using System.Collections.Generic;
using UnityEngine;

namespace Spiderman.Assets
{
    // �Զ��崴��Asset�Ĳ˵���������Unity�༭���Ҽ���������Դ
    [CreateAssetMenu(fileName = "Sound", menuName = "CreateActions/Assets/Sound", order = 0)]
    public class AssetsSoundSO : ScriptableObject
    {
        // ���л����ڲ��࣬���������������ͺͶ�Ӧ����ƵƬ������
        [System.Serializable]
        private class SoundConfig
        {
            public SoundType SoundType;     // �������ͣ����ж�Ӧ��ö�ٶ��壨������δչʾ����ȷ�����ڣ�
            public AudioClip[] AudioClips;  // ������������Ӧ����ƵƬ������
        }

        // ���������б�����Inspector�����ò�ͬ��������������ƵƬ��
        [SerializeField]
        private List<SoundConfig> _configSound = new List<SoundConfig>();

        /// <summary>
        /// �����������ͻ�ȡ��Ӧ����ƵƬ��
        /// </summary>
        /// <param name="_soundType"></param>
        /// <returns></returns>
        public AudioClip GetAudioClip(SoundType _soundType)
        {
            if(_configSound == null || _configSound.Count == 0)
                return null;

            switch (_soundType)
            {
                //������ض�Ӧ���͵���ƵƬ��
                case SoundType.ATK:
                    return _configSound[0].AudioClips[Random.Range(0, _configSound[0].AudioClips.Length)];
                case SoundType.HIT:
                    return _configSound[1].AudioClips[Random.Range(0, _configSound[1].AudioClips.Length)];
                case SoundType.BLOCK:
                    return _configSound[2].AudioClips[Random.Range(0, _configSound[2].AudioClips.Length)];
                case SoundType.FOOT:
                    return _configSound[3].AudioClips[Random.Range(0, _configSound[3].AudioClips.Length)];
            }

            return null;
        }

    }
}