using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MalbersAnimations
{
    public class MGameSettings : MonoBehaviour
    {
        public bool HideCursor = false;
        public bool ForceFPS = false;
        public int GameFPS = 60;

        void Awake()
        {
            DontDestroyOnLoad(this);

            if (HideCursor)
            {
                Cursor.lockState = HideCursor ? CursorLockMode.Locked : CursorLockMode.None;  // Lock or unlock the cursor.
                Cursor.visible = !HideCursor;
            }

            if (ForceFPS)
            {
                QualitySettings.vSyncCount = 0;
                Application.targetFrameRate = GameFPS;
            }

        }
    }
}