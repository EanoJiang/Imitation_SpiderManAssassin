using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Spiderman.Event
{
    public class AnimationEvent : MonoBehaviour
    {
        private void PlaySound(string _soundName)
        {
            //ѡȡ������е���Ч����
            GamePoolManager.Instance.TryGetPoolItem(_soundName,transform.position,Quaternion.identity);
        }
    }

}
