using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class EnvironmentInteractionState : BaseState<EnvironmentInteractionStateMachine.EEnvironmentInteractionState>
{
    protected EnvironmentInteractionContext Context;
    private float _movingAwayOffset = 0.005f;       // 远离目标的偏移值

    bool _shouldReset;      // 标志位：是否能够进入ResetState

    // 构造函数
    public EnvironmentInteractionState(EnvironmentInteractionContext context, EnvironmentInteractionStateMachine.EEnvironmentInteractionState stateKey) : base(stateKey)
    {
        Context = context;
    }


    /// <summary>
    /// 是否能够进入ResetState
    /// </summary>
    /// <returns>能够进入时返回 true，否则返回 false</returns>
    protected bool CheckShouldReset()
    {
        if (_shouldReset)
        {
            // 重置「最近距离」为无穷大
            Context.LowestDistance = Mathf.Infinity;
            // 重置标志位
            _shouldReset = false;
            return true;
        }

        // 标志位：是否停止移动
        bool isPlayerStopped = CheckIsStopped();
        // 标志位：是否正在远离目标交互点
        bool isMovingAway = CheckIsMovingAway();
        // 标志位：是否是非法角度(这个可以不写)
        bool isInvalidAngle = CheckIsInvalidAngle();
        // 标志位：是否正在跳跃
        bool isPlayerJumping = CheckIsJumping();

        if(isPlayerStopped || isMovingAway || isPlayerJumping)
        {
            // 重置「最近距离」为无穷大
            Context.LowestDistance = Mathf.Infinity;
            return true;
        }

        return false;
    }

    /// <summary>
    /// Reset事件的触发机制1： ———— 玩家是否停止移动
    /// </summary>
    /// <returns></returns>
    protected bool CheckIsStopped()
    {
        bool isPlayerStopped = GameInputManager.Instance.Movement == Vector2.zero;
        return isPlayerStopped;
    }
    /// <summary>
    /// Reset事件的触发机制2： ———— 玩家是否正在远离目标交互点
    /// </summary>
    /// <returns>玩家远离目标时返回 true，否则返回 false</returns>
    protected bool CheckIsMovingAway()
    {
        // 1. 角色根节点到目标碰撞点的当前距离
        float currentDistanceToTarget = Vector3.Distance(
            Context.RootTransform.position,
            Context.ClosestPointOnColliderFromShoulder
        );

        // 标志位：是否正在搜索新的交互点
        bool isSearchingForNewInteraction = Context.CurrentIntersectingCollider == null;
        if (isSearchingForNewInteraction)
        {
            return false;
        }

        // 标志位：是否在靠近目标
        bool isGettingCloserToTarget = currentDistanceToTarget <= Context.LowestDistance;
        if (isGettingCloserToTarget)
        {
            // 更新最近距离
            Context.LowestDistance = currentDistanceToTarget;
            // 未远离
            return false;
        }

        // 标志位：是否已远离目标（当前距离超过「最近距离 + 偏移值」）
        bool isMovingAwayFromTarget = currentDistanceToTarget > Context.LowestDistance + _movingAwayOffset;
        if (isMovingAwayFromTarget)
        {
            // 标记为远离，重置「最近距离」（下次重新开始计算）
            Context.LowestDistance = Mathf.Infinity;
            // 远离
            return true;
        }

        return false;
    }
    /// <summary>
    /// Reset事件的触发机制3： ———— 当前交互的角度是否为“非法角度”
    /// </summary>
    /// <returns>如果是非法角度返回 true，否则返回 false</returns>
    protected bool CheckIsInvalidAngle()
    {
        // 如果当前交互的碰撞体为空，直接判定不是不良角度
        if (Context.CurrentIntersectingCollider == null)
        {
            return false;
        }

        // 计算从肩部指向碰撞点的方向向量
        Vector3 targetDirection = Context.ClosestPointOnColliderFromShoulder
                                 - Context.CurrentShoulderTransform.position;

        // 根据身体侧别（左/右）确定肩部的参考方向
        Vector3 shoulderDirection = (Context.CurrentBodySide == EnvironmentInteractionContext.EBodySide.RIGHT) ?
            Context.RootTransform.right
            : -Context.RootTransform.right;

        // 计算肩部参考方向与目标方向的点积（用于判断夹角方向）
        float dotProduct = Vector3.Dot(shoulderDirection, targetDirection.normalized);

        // 非法角度 = 点积小于 0 (目标方向与肩部参考方向夹角大于 90 度)
        bool isInvalidAngle = dotProduct < 0;

        return isInvalidAngle;
    }
    /// <summary>
    /// Reset事件的触发机制4： ———— 玩家是否正在跳跃
    /// </summary>
    /// <returns></returns>
    protected bool CheckIsJumping()
    {
        bool isPlayerJumping = Mathf.Round(Context.CharacterController.velocity.y) >= 1;
        return isPlayerJumping;
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
            // 能够进入ResetState
            _shouldReset = true;
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
        float offsetDistance = 0.07f;

        // 4. 最终要到达的位置：在“最近碰撞点”基础上，加上 沿rayDirection射线方向偏移 offsetDistance 距离
        Vector3 targetPosition = Context.ClosestPointOnColliderFromShoulder 
            + normalizedRayDirection * offsetDistance;

        // 5. 更新 IK 目标位置
        Context.CurrentIkTargetTransform.position = 
            new Vector3(
                targetPosition.x,
                Context.InteractionPoint_Y_Offset,      //y轴方向换成碰撞点的y轴偏移
                targetPosition.z);
        #endregion
    }
}
