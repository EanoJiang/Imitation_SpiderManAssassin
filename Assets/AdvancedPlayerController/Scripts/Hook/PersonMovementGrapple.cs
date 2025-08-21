using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//钩锁移动脚本
public class PersonMovementGrapple : MonoBehaviour
{
    public Rigidbody rigidbody;
    public Animator animator;

    public bool isGrapple;

    //移动速度和方向
    public Vector3 grappleVelocity;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

 //   public void JumpToPosition(Vector3 targetPosition,float height)
	//{
 //       isGrapple = true;
 //       grappleVelocity = CalculateVelocity(transform.position, targetPosition, height);

 //   }

 //   Vector3 CalculateVelocity(Vector3 startPosition,Vector3 targetPosition,float height)
	//{
 //       float gravity = Physics.gravity.y;
 //       float displacementY = targetPosition.y - startPosition.y;
 //       Vector3 displacementXZ = new Vector3(targetPosition.x - startPosition.x, 0f, targetPosition.z - startPosition.z);

 //       Vector3 velocityY = Vector3.up * Mathf.Sqrt(-2 * gravity * height);
 //       Vector3 velocityXZ = displacementXZ / (Mathf.Sqrt(-2 * height / gravity)
 //           + Mathf.Sqrt(2 * (displacementY - height) / gravity));

 //       return velocityXZ + velocityY;
 //   }
}
