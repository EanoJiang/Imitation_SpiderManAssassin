using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace MalbersAnimations.Utilities
{
    [CreateAssetMenu(menuName = "Malbers Animations/Scriptables/Point Click Data")]
    public class PointClickData : ScriptableObject
    {
        [HideInInspector]
        public BaseEventDataEvent baseDataEvent = new BaseEventDataEvent();

        public void Invoke(BaseEventData data)
        {
            baseDataEvent.Invoke(data);
        }
    }

    [System.Serializable]
    public class BaseEventDataEvent : UnityEvent<BaseEventData> { }
    
}