using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IgnoreCollider : StateMachineBehaviour
{

    [SerializeField]
    private int _selfLayer;  // 自身所在的层

    [SerializeField]
    private int[] _targetLayers;  // 要处理碰撞忽略的目标层数组

    /// <summary>
    /// 当进入动画状态时调用，处理层碰撞忽略
    /// </summary>
    /// <param name="animator">动画组件</param>
    /// <param name="stateInfo">动画状态信息</param>
    /// <param name="layerIndex">层索引</param>
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // 遍历目标层数组，设置忽略自身层与目标层的碰撞
        foreach (int targetLayer in _targetLayers)
        {
            Physics.IgnoreLayerCollision(_selfLayer, targetLayer, true);
        }
    }

    /// <summary>
    /// 当退出动画状态时调用，恢复层碰撞（取消忽略）
    /// </summary>
    /// <param name="animator">动画组件</param>
    /// <param name="stateInfo">动画状态信息</param>
    /// <param name="layerIndex">层索引</param>
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // 遍历目标层数组，恢复自身层与目标层的碰撞（设置为不忽略）
        foreach (int targetLayer in _targetLayers)
        {
            Physics.IgnoreLayerCollision(_selfLayer, targetLayer, false);
        }
    }
}
