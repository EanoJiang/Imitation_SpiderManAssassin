using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColliderInteraction : MonoBehaviour
{
    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if(hit.transform.TryGetComponent(out Rigidbody rigidbody))
        {
            // ֻҪ�Ӵ�������ײ����rigidbody���͸���ʩ��һ����ǰ����
            rigidbody.AddForce(transform.forward * 20f,ForceMode.Force);
        }
    }
}
