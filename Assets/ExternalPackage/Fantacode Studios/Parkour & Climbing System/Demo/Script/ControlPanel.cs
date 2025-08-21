using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace FC_ParkourSystem
{
    public class ControlPanel : MonoBehaviour
    {
        public GameObject image;
        public Text text;

        string controlPanelKey = "controlPanelKey";
        int isActive;

        private void Awake()
        {
            isActive = PlayerPrefs.GetInt(controlPanelKey);
            ControlPanelController();
        }
        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Return))
            {
                isActive = image.activeSelf ? 0 : 1;
                PlayerPrefs.SetInt(controlPanelKey, isActive);
                ControlPanelController();
            }
        }

        void ControlPanelController()
        {
            image.SetActive(isActive == 0 ? false : true);
            var t = image.activeSelf ? "disable" : "enable";
            text.text = "Click Enter to " + t + " control panel";
        }
    }
}
