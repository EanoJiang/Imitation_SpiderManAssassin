using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThirdPersonIKController : MonoBehaviour
{
    public Animator _animator;

    //IK控制点
    //四肢关节点
    public Transform ik_LHand;
    public Transform ik_RHand;
    public Transform ik_LFoot;
    public Transform ik_RFoot;
    //头部控制点,可以根据主相机的位置，让玩家能够从侧视角下看到头部偏转。
    public Transform Head_IKPoint;
    
    private void OnAnimatorIK(int layerIndex)
    {
        //四肢
        if (ik_LHand != null)
            IKControl(AvatarIKGoal.LeftHand, ik_LHand);
        if (ik_RHand != null)
            IKControl(AvatarIKGoal.RightHand, ik_RHand);
        if (ik_LFoot != null)
            IKControl(AvatarIKGoal.LeftFoot, ik_LFoot);
        if (ik_RFoot != null)
            IKControl(AvatarIKGoal.RightFoot, ik_RFoot);

        //头部
        if (Head_IKPoint != null)
            IKHeadControl(Head_IKPoint);
    }

    /// <summary>
    /// 头部IK控制。
    /// </summary>
    private void IKHeadControl(Transform target)
    {
        // 计算相机相对于角色的方向
        Vector3 directionToCamera = target.position - transform.position;
        
        // 计算相机是否在角色前方（使用点积判断）
        float dotProduct = Vector3.Dot(transform.forward, directionToCamera.normalized);
        bool isCameraInFront = dotProduct > 0;
        
        // 如果相机在角色前方，头部看向相机
        if (isCameraInFront)
        {
            _animator.SetLookAtWeight(1.0f); // 使用最大权重1
            _animator.SetLookAtPosition(target.position);
        }
        else
        {
            // 如果相机在角色后方，头部朝向相机视线向前发射的延长线
            
            // 获取相机的前方向（视线方向）
            Vector3 cameraForward = target.forward;
            
            // 计算相机视线向前延伸的点
            // 这里我们使用相机位置加上相机前方向乘以一个距离值
            Vector3 lookPoint = target.position + cameraForward * 10f;
            
            // 使用最大权重1
            _animator.SetLookAtWeight(1.0f);
            _animator.SetLookAtPosition(lookPoint);
            
            // 添加调试可视化，帮助你在Scene视图中看到头部朝向
            Debug.DrawLine(transform.position, lookPoint, Color.red);
            // 也可视化相机的视线方向
            Debug.DrawRay(target.position, cameraForward * 10f, Color.blue);
        }
    }

    /// <summary>
    /// 四肢IK控制
    /// </summary>
    /// <param name="ControlPosition"></param>
    /// <param name="target"></param>
    public void IKControl(AvatarIKGoal ControlPosition, Transform target)
    {
        _animator.SetIKPositionWeight(ControlPosition, 1);
        _animator.SetIKPosition(ControlPosition, target.position);
        _animator.SetIKRotationWeight(ControlPosition, 1);
        _animator.SetIKRotation(ControlPosition, target.rotation);
    }
}
