using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColliderInteraction : MonoBehaviour
{
    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if(hit.transform.TryGetComponent(out Rigidbody rigidbody))
        {
            // 只要接触到的碰撞体是rigidbody，就给它施加一个向前的力
            rigidbody.AddForce(transform.forward * 20f,ForceMode.Force);
        }
    }
}
