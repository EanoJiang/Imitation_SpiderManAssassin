using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class EnvironmentInteractionState : BaseState<EnvironmentInteractionStateMachine.EEnvironmentInteractionState>
{
    protected EnvironmentInteractionContext Context;
    private float _movingAwayOffset = 0.005f;       // Զ��Ŀ���ƫ��ֵ

    bool _shouldReset;      // ��־λ���Ƿ��ܹ�����ResetState

    // ���캯��
    public EnvironmentInteractionState(EnvironmentInteractionContext context, EnvironmentInteractionStateMachine.EEnvironmentInteractionState stateKey) : base(stateKey)
    {
        Context = context;
    }


    /// <summary>
    /// �Ƿ��ܹ�����ResetState
    /// </summary>
    /// <returns>�ܹ�����ʱ���� true�����򷵻� false</returns>
    protected bool CheckShouldReset()
    {
        if (_shouldReset)
        {
            // ���á�������롹Ϊ�����
            Context.LowestDistance = Mathf.Infinity;
            // ���ñ�־λ
            _shouldReset = false;
            return true;
        }

        // ��־λ���Ƿ�ֹͣ�ƶ�
        bool isPlayerStopped = CheckIsStopped();
        // ��־λ���Ƿ�����Զ��Ŀ�꽻����
        bool isMovingAway = CheckIsMovingAway();
        // ��־λ���Ƿ��ǷǷ��Ƕ�(������Բ�д)
        bool isInvalidAngle = CheckIsInvalidAngle();
        // ��־λ���Ƿ�������Ծ
        bool isPlayerJumping = CheckIsJumping();

        if(isPlayerStopped || isMovingAway || isPlayerJumping)
        {
            // ���á�������롹Ϊ�����
            Context.LowestDistance = Mathf.Infinity;
            return true;
        }

        return false;
    }

    /// <summary>
    /// Reset�¼��Ĵ�������1�� �������� ����Ƿ�ֹͣ�ƶ�
    /// </summary>
    /// <returns></returns>
    protected bool CheckIsStopped()
    {
        bool isPlayerStopped = GameInputManager.Instance.Movement == Vector2.zero;
        return isPlayerStopped;
    }
    /// <summary>
    /// Reset�¼��Ĵ�������2�� �������� ����Ƿ�����Զ��Ŀ�꽻����
    /// </summary>
    /// <returns>���Զ��Ŀ��ʱ���� true�����򷵻� false</returns>
    protected bool CheckIsMovingAway()
    {
        // 1. ��ɫ���ڵ㵽Ŀ����ײ��ĵ�ǰ����
        float currentDistanceToTarget = Vector3.Distance(
            Context.RootTransform.position,
            Context.ClosestPointOnColliderFromShoulder
        );

        // ��־λ���Ƿ����������µĽ�����
        bool isSearchingForNewInteraction = Context.CurrentIntersectingCollider == null;
        if (isSearchingForNewInteraction)
        {
            return false;
        }

        // ��־λ���Ƿ��ڿ���Ŀ��
        bool isGettingCloserToTarget = currentDistanceToTarget <= Context.LowestDistance;
        if (isGettingCloserToTarget)
        {
            // �����������
            Context.LowestDistance = currentDistanceToTarget;
            // δԶ��
            return false;
        }

        // ��־λ���Ƿ���Զ��Ŀ�꣨��ǰ���볬����������� + ƫ��ֵ����
        bool isMovingAwayFromTarget = currentDistanceToTarget > Context.LowestDistance + _movingAwayOffset;
        if (isMovingAwayFromTarget)
        {
            // ���ΪԶ�룬���á�������롹���´����¿�ʼ���㣩
            Context.LowestDistance = Mathf.Infinity;
            // Զ��
            return true;
        }

        return false;
    }
    /// <summary>
    /// Reset�¼��Ĵ�������3�� �������� ��ǰ�����ĽǶ��Ƿ�Ϊ���Ƿ��Ƕȡ�
    /// </summary>
    /// <returns>����ǷǷ��Ƕȷ��� true�����򷵻� false</returns>
    protected bool CheckIsInvalidAngle()
    {
        // �����ǰ��������ײ��Ϊ�գ�ֱ���ж����ǲ����Ƕ�
        if (Context.CurrentIntersectingCollider == null)
        {
            return false;
        }

        // ����Ӽ粿ָ����ײ��ķ�������
        Vector3 targetDirection = Context.ClosestPointOnColliderFromShoulder
                                 - Context.CurrentShoulderTransform.position;

        // ������������/�ң�ȷ���粿�Ĳο�����
        Vector3 shoulderDirection = (Context.CurrentBodySide == EnvironmentInteractionContext.EBodySide.RIGHT) ?
            Context.RootTransform.right
            : -Context.RootTransform.right;

        // ����粿�ο�������Ŀ�귽��ĵ���������жϼнǷ���
        float dotProduct = Vector3.Dot(shoulderDirection, targetDirection.normalized);

        // �Ƿ��Ƕ� = ���С�� 0 (Ŀ�귽����粿�ο�����нǴ��� 90 ��)
        bool isInvalidAngle = dotProduct < 0;

        return isInvalidAngle;
    }
    /// <summary>
    /// Reset�¼��Ĵ�������4�� �������� ����Ƿ�������Ծ
    /// </summary>
    /// <returns></returns>
    protected bool CheckIsJumping()
    {
        bool isPlayerJumping = Mathf.Round(Context.CharacterController.velocity.y) >= 1;
        return isPlayerJumping;
    }

    /// <summary>
    /// ��ȡ�������ײ��
    /// </summary>
    /// <param name="intersectingCollider"></param> �ཻ����ײ��
    /// <param name="positionToCheck"></param> Ҫ���������λ��
    /// <returns></returns>
    private Vector3 GetClosestPointOnCollider(Collider intersectingCollider, Vector3 positionToCheck)
    {
        return intersectingCollider.ClosestPoint(positionToCheck);
    }

    /// <summary>
    /// ���� IK Ŀ��λ��׷��
    /// </summary>
    /// <param name="intersectingCollider">�ཻ����ײ�壬��Ϊ׷�ٹ�������</param>
    protected void StartIkTargetPositionTracking(Collider intersectingCollider)
    {
        //ֻ����ײ��Ĳ㼶ΪInteractable && ��ǰû�пɽ�������ײ�� ʱ�Ž���IKĿ��λ��׷��
        // ��ֹƵ������
        if (intersectingCollider.gameObject.layer == LayerMask.NameToLayer("Interactable") && Context.CurrentIntersectingCollider == null)
        {
            // ��¼��ǰ��ײ��
            Context.CurrentIntersectingCollider = intersectingCollider;
            // �������ײ��
            Vector3 closestPointFromRoot = GetClosestPointOnCollider(intersectingCollider, Context.RootTransform.position);
            // ���õ�ǰ�������Ĳ���(�����������ײ��)
            Context.SetCurrentSide(closestPointFromRoot);

            //����IKĿ��λ��
            SetIkTargetPosition();
        }
    }

    /// <summary>
    /// ���� IK Ŀ��λ��
    /// </summary>
    /// <param name="intersectingCollider">�ཻ����ײ�壬������״̬����Ŀ��λ��</param>
    protected void UpdateIkTargetPosition(Collider intersectingCollider)
    {
        // �ڽӴ������У�һֱ����IKĿ��λ��
        if (Context.CurrentIntersectingCollider == intersectingCollider)
        {
            SetIkTargetPosition();
        }
    }

    /// <summary>
    /// ���� IK Ŀ��λ��׷��
    /// </summary>
    /// <param name="intersectingCollider">�ཻ����ײ�壬�����ִ��׷������</param>
    protected void ResetIkTargetPositionTracking(Collider intersectingCollider)
    {
        if(intersectingCollider == Context.CurrentIntersectingCollider)
        {
            // ���õ�ǰ��ײ��Ϊ��
            Context.CurrentIntersectingCollider = null;
            // ����IKĿ��λ��Ϊ�����
            Context.ClosestPointOnColliderFromShoulder = Vector3.positiveInfinity;
            // �ܹ�����ResetState
            _shouldReset = true;
        }
    }

    /// <summary>
    /// ���� IK Ŀ��λ��
    /// </summary>
    /// <param name="targetPosition"></param>
    private void SetIkTargetPosition()
    {
        // �������ײ��
        Context.ClosestPointOnColliderFromShoulder = GetClosestPointOnCollider(Context.CurrentIntersectingCollider, 
            // Ŀ��λ�ã��ϰ����xzλ�� ��ɫ��ߵ�yλ��(�߶�λ��)
            new Vector3(Context.RootTransform.position.x, Context.CharacterShoulderHeight, Context.RootTransform.position.z));

        #region ���ֲ���IKĿ���ƶ�����������ײ��
        // 1. ���߷��򣺴ӡ������ײ�㡱ָ�򡰵�ǰ�粿λ�á�������
        Vector3 rayDirection = Context.CurrentShoulderTransform.position
                             - Context.ClosestPointOnColliderFromShoulder;
            // Unity �����������㣺Vector3 �յ� - Vector3 ���

        // 2. ��һ�����õ���λ����
        Vector3 normalizedRayDirection = rayDirection.normalized;

        // 3. ƫ�ƾ��룬��ֹ�ֲ���ģ
        float offsetDistance = 0.07f;

        // 4. ����Ҫ�����λ�ã��ڡ������ײ�㡱�����ϣ����� ��rayDirection���߷���ƫ�� offsetDistance ����
        Vector3 targetPosition = Context.ClosestPointOnColliderFromShoulder 
            + normalizedRayDirection * offsetDistance;

        // 5. ���� IK Ŀ��λ��
        Context.CurrentIkTargetTransform.position = 
            new Vector3(
                targetPosition.x,
                Context.InteractionPoint_Y_Offset,      //y�᷽�򻻳���ײ���y��ƫ��
                targetPosition.z);
        #endregion
    }
}
