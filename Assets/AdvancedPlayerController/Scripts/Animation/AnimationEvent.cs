using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Spiderman.Event
{
    public class AnimationEvent : MonoBehaviour
    {
        private void PlaySound(string _soundName)
        {
            //选取对象池中的音效对象
            GamePoolManager.Instance.TryGetPoolItem(_soundName,transform.position,Quaternion.identity);
        }
    }

}
