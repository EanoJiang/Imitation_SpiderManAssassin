using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace FS_ThirdPerson 
{
    public class MobileCameraInputController : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {

        public LocomotionInputManager locomotionInput;
        private int currentTouchId = -1;

        private void Update()
        {
#if UNITY_ANDROID || UNITY_IOS
            locomotionInput.CameraInput = Vector2.zero;
            if (currentTouchId >= 0 && Input.touchCount > 0)
            {
                bool touchFound = false;
                for (int i = 0; i < Input.touchCount; i++)
                {
                    Touch touch = Input.GetTouch(i);
                    if (touch.fingerId == currentTouchId)
                    {
                        touchFound = true;

                        if (touch.phase == TouchPhase.Moved)
                        {
                            locomotionInput.CameraInput = touch.deltaPosition;
                        }
                        else if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
                        {
                            ResetTouch();
                        }
                        break;
                    }
                }

                if (!touchFound)
                {
                    ResetTouch();
                }
            }
#endif
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (Input.touchCount > 0 && currentTouchId == -1)
            {
                for (int i = 0; i < Input.touchCount; i++)
                {
                    Touch touch = Input.GetTouch(i);
                    if (touch.phase == TouchPhase.Began)
                    {
                        currentTouchId = touch.fingerId;
                        break;
                    }
                }
            }
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            ResetTouch();
        }

        private void ResetTouch()
        {
            currentTouchId = -1;
        }
    }
}