using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class EnvironmentInteractionState : BaseState<EnvironmentInteractionStateMachine.EEnvironmentInteractionState>
{
    protected EnvironmentInteractionContext Context;

    // ���캯��
    public EnvironmentInteractionState(EnvironmentInteractionContext context, EnvironmentInteractionStateMachine.EEnvironmentInteractionState stateKey) : base(stateKey)
    {
        Context = context;
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
        float offsetDistance = 0.05f;

        // 4. ����Ҫ�����λ�ã��ڡ������ײ�㡱�����ϣ����� ��rayDirection���߷���ƫ�� offsetDistance ����
        Vector3 targettPosition = Context.ClosestPointOnColliderFromShoulder 
            + normalizedRayDirection * offsetDistance;

        // 5. ���� IK Ŀ��λ��
        Context.CurrentIkTargetTransform.position = targettPosition;
        #endregion
    }
}
