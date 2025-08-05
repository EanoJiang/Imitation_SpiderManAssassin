using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace MalbersAnimations.Utilities
{
    public class MInteract : MonoBehaviour, IInteractable
    {
        public bool m_HasInteracted = false;

        public UnityEvent OnInteract = new UnityEvent();
        public void Interact()
        {
            if (!m_HasInteracted)
            {
                OnInteract.Invoke();
                m_HasInteracted = true;
            }
        }

        public virtual void Reset()
        {
            m_HasInteracted = false;
        }

        public void InstantiateGO(GameObject GO)
        {
            Instantiate(GO,transform.position,transform.rotation);
        }

        public void DestroyMe()
        {
            Destroy(gameObject);
        }

        public void DestroyMe(float time)
        {
            Destroy(gameObject, time);
        }

        public void DestroyGO(GameObject GO)
        {
            Destroy(GO);
        }

        public void DebugLog(string log)
        {
            Debug.Log("<B>:"+name+"</B>: " +log);
        }
    }
}