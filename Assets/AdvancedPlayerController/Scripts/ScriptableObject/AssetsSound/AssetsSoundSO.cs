using System.Collections.Generic;
using UnityEngine;

namespace Spiderman.Assets
{
    // 自定义创建Asset的菜单，方便在Unity编辑器右键创建该资源
    [CreateAssetMenu(fileName = "Sound", menuName = "CreateActions/Assets/Sound", order = 0)]
    public class AssetsSoundSO : ScriptableObject
    {
        // 序列化的内部类，用于配置声音类型和对应的音频片段数组
        [System.Serializable]
        private class SoundConfig
        {
            public SoundType SoundType;     // 声音类型，需有对应的枚举定义（代码里未展示，需确保存在）
            public AudioClip[] AudioClips;  // 该类型声音对应的音频片段数组
        }

        // 声音配置列表，可在Inspector中配置不同类型声音及其音频片段
        [SerializeField]
        private List<SoundConfig> _configSound = new List<SoundConfig>();

        /// <summary>
        /// 根据声音类型获取对应的音频片段
        /// </summary>
        /// <param name="_soundType"></param>
        /// <returns></returns>
        public AudioClip GetAudioClip(SoundType _soundType)
        {
            if(_configSound == null || _configSound.Count == 0)
                return null;

            switch (_soundType)
            {
                //随机返回对应类型的音频片段
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