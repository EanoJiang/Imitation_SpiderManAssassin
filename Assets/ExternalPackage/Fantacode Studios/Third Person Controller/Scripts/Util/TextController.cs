using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace FC_ParkourSystem
{
    public class TextController : MonoBehaviour
    {
        GameObject cam;
        void Start()
        {
            cam = Camera.main?.gameObject;
        }

        // Update is called once per frame
        void Update()
        {
            if (cam != null)
            {
                transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(cam.transform.forward),
                         500 * Time.deltaTime);
            }
        }
    }
}