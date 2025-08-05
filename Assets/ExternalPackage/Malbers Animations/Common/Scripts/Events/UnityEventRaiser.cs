using UnityEngine;
namespace MalbersAnimations.Events
{
    /// <summary>Simple Event Raiser On Enable</summary>
    public class UnityEventRaiser : MonoBehaviour
    {
        public float Delayed = 0;
        public UnityEngine.Events.UnityEvent OnEnableEvent;

        public void OnEnable()
        {
            if (Delayed > 0)
            {
                Invoke("StartEvent", Delayed);
            }
            else
            {
                OnEnableEvent.Invoke();
            }
        }

        private void StartEvent()
        {
            OnEnableEvent.Invoke();
        }


        public void DestroyMe(float time)
        {
            Destroy(gameObject, time);
        }
    }
}