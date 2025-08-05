using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using MalbersAnimations.Events;
using UnityEngine.AI;

namespace MalbersAnimations.Utilities
{
    public class PointClick : MonoBehaviour
    {
        public PointClickData pointClickData;
        public GameObject PointUI;
        public float radius = 0.2f;
        private const float navMeshSampleDistance = 4f;

        [Header("Events")]
        public Vector3Event OnPointClick = new Vector3Event();
        public TransformEvent OnInteractableClick = new TransformEvent();

        protected Collider[] interactables;

        void OnEnable()
        {
            if (pointClickData)
            {
                pointClickData.baseDataEvent.AddListener(OnGroundClick);
            }
        }


        void OnDisable()
        {
            if (pointClickData)
            {
                pointClickData.baseDataEvent.RemoveListener(OnGroundClick);
            }
        }

        Vector3 destinationPosition;

        public void OnGroundClick(BaseEventData data)   
        {
            PointerEventData pData = (PointerEventData)data;
            NavMeshHit hit;
            if (NavMesh.SamplePosition(pData.pointerCurrentRaycast.worldPosition, out hit, navMeshSampleDistance, NavMesh.AllAreas))
                destinationPosition = hit.position;
            else
                destinationPosition = pData.pointerCurrentRaycast.worldPosition;

            if (PointUI)
            {
                Instantiate(PointUI, destinationPosition, Quaternion.FromToRotation(PointUI.transform.up, pData.pointerCurrentRaycast.worldNormal));
            }

            interactables = Physics.OverlapSphere(destinationPosition, radius);

            foreach (var inter in interactables)
            {
                if (inter.GetComponent<IDestination>() != null)
                {
                    OnInteractableClick.Invoke(inter.transform.root); //Invoke only the first interactable found
                    return;
                }
            }

            OnPointClick.Invoke(destinationPosition);
        }

        void OnDrawGizmos()
        {
            if (Application.isPlaying)
            {
                Gizmos.color = Color.green;

                Gizmos.DrawWireSphere(destinationPosition, 0.1f);
                Gizmos.DrawSphere(destinationPosition, 0.1f);
            }
        }
    }
}