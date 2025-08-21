using FS_ThirdPerson;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace FC_ParkourSystem {

    public class SetLayersForDemoscene : MonoBehaviour
    {
        public List<Transform> ledgeParent;
        public FootStepEffects footStepEffects;
        //public GameObject mobileUI;
        //bool uiEnabled = false;

        void Start()
        {
            foreach (var ledges in ledgeParent)
            {
                foreach (Transform ledge in ledges)
                    ledge.gameObject.layer = LayerMask.NameToLayer("Ledge");
            }

            if (!(footStepEffects.groundLayer == (footStepEffects.groundLayer | (1 << LayerMask.NameToLayer("Ledge")))))
                footStepEffects.groundLayer += 1 << LayerMask.NameToLayer("Ledge");
        }

        //private void Update()
        //{
        //    if (Input.GetKeyDown(KeyCode.M))
        //    {
                

        //        uiEnabled = !uiEnabled;
        //        mobileUI.SetActive(uiEnabled);

        //        Cursor.visible = uiEnabled;
        //        Cursor.lockState = uiEnabled ? CursorLockMode.None : CursorLockMode.Locked;
        //    }
        //}
    }
}