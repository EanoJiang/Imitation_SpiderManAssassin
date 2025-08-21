using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class EnvironmentInteractionState : BaseState<EnvironmentInteractionStateMachine.EEnvironmentInteractionState>
{
    protected EnvironmentInteractionContext Context;

    // 构造函数
    public EnvironmentInteractionState(EnvironmentInteractionContext context, EnvironmentInteractionStateMachine.EEnvironmentInteractionState stateKey) : base(stateKey)
    {
        Context = context;
    }

    /// <summary>
    /// 获取最近的碰撞点
    /// </summary>
    /// <param name="intersectingCollider"></param> 相交的碰撞体
    /// <param name="positionToCheck"></param> 要检测对象本身的位置
    /// <returns></returns>
    private Vector3 GetClosestPointOnCollider(Collider intersectingCollider, Vector3 positionToCheck)
    {
        return intersectingCollider.ClosestPoint(positionToCheck);
    }

    /// <summary>
    /// 启动 IK 目标位置追踪
    /// </summary>
    /// <param name="intersectingCollider">相交的碰撞体，作为追踪关联对象</param>
    protected void StartIkTargetPositionTracking(Collider intersectingCollider)
    {
        //只有碰撞体的层级为Interactable && 当前没有可交互的碰撞体 时才进行IK目标位置追踪
        // 防止频繁触发
        if (intersectingCollider.gameObject.layer == LayerMask.NameToLayer("Interactable") && Context.CurrentIntersectingCollider == null)
        {
            // 记录当前碰撞体
            Context.CurrentIntersectingCollider = intersectingCollider;
            // 最近的碰撞点
            Vector3 closestPointFromRoot = GetClosestPointOnCollider(intersectingCollider, Context.RootTransform.position);
            // 设置当前更靠近的侧面(根据最近的碰撞点)
            Context.SetCurrentSide(closestPointFromRoot);

            //设置IK目标位置
            SetIkTargetPosition();
        }
    }

    /// <summary>
    /// 更新 IK 目标位置
    /// </summary>
    /// <param name="intersectingCollider">相交的碰撞体，依据其状态更新目标位置</param>
    protected void UpdateIkTargetPosition(Collider intersectingCollider)
    {
        // 在接触过程中，一直更新IK目标位置
        if (Context.CurrentIntersectingCollider == intersectingCollider)
        {
            SetIkTargetPosition();
        }
    }

    /// <summary>
    /// 重置 IK 目标位置追踪
    /// </summary>
    /// <param name="intersectingCollider">相交的碰撞体，针对其执行追踪重置</param>
    protected void ResetIkTargetPositionTracking(Collider intersectingCollider)
    {
        if(intersectingCollider == Context.CurrentIntersectingCollider)
        {
            // 重置当前碰撞体为空
            Context.CurrentIntersectingCollider = null;
            // 重置IK目标位置为无穷大
            Context.ClosestPointOnColliderFromShoulder = Vector3.positiveInfinity;
        }
    }

    /// <summary>
    /// 设置 IK 目标位置
    /// </summary>
    /// <param name="targetPosition"></param>
    private void SetIkTargetPosition()
    {
        // 最近的碰撞点
        Context.ClosestPointOnColliderFromShoulder = GetClosestPointOnCollider(Context.CurrentIntersectingCollider, 
            // 目标位置：上半身的xz位置 角色肩高的y位置(高度位置)
            new Vector3(Context.RootTransform.position.x, Context.CharacterShoulderHeight, Context.RootTransform.position.z));

        #region 让手部的IK目标移动到这个最近碰撞点
        // 1. 射线方向：从“最近碰撞点”指向“当前肩部位置”的向量
        Vector3 rayDirection = Context.CurrentShoulderTransform.position
                             - Context.ClosestPointOnColliderFromShoulder;
            // Unity 中向量的运算：Vector3 终点 - Vector3 起点

        // 2. 归一化，得到单位向量
        Vector3 normalizedRayDirection = rayDirection.normalized;

        // 3. 偏移距离，防止手部穿模
        float offsetDistance = 0.05f;

        // 4. 最终要到达的位置：在“最近碰撞点”基础上，加上 沿rayDirection射线方向偏移 offsetDistance 距离
        Vector3 targettPosition = Context.ClosestPointOnColliderFromShoulder 
            + normalizedRayDirection * offsetDistance;

        // 5. 更新 IK 目标位置
        Context.CurrentIkTargetTransform.position = targettPosition;
        #endregion
    }
}
