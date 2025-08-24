using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IgnoreCollider : StateMachineBehaviour
{

    [SerializeField]
    private int _selfLayer;  // �������ڵĲ�

    [SerializeField]
    private int[] _targetLayers;  // Ҫ������ײ���Ե�Ŀ�������

    /// <summary>
    /// �����붯��״̬ʱ���ã��������ײ����
    /// </summary>
    /// <param name="animator">�������</param>
    /// <param name="stateInfo">����״̬��Ϣ</param>
    /// <param name="layerIndex">������</param>
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // ����Ŀ������飬���ú����������Ŀ������ײ
        foreach (int targetLayer in _targetLayers)
        {
            Physics.IgnoreLayerCollision(_selfLayer, targetLayer, true);
        }
    }

    /// <summary>
    /// ���˳�����״̬ʱ���ã��ָ�����ײ��ȡ�����ԣ�
    /// </summary>
    /// <param name="animator">�������</param>
    /// <param name="stateInfo">����״̬��Ϣ</param>
    /// <param name="layerIndex">������</param>
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // ����Ŀ������飬�ָ��������Ŀ������ײ������Ϊ�����ԣ�
        foreach (int targetLayer in _targetLayers)
        {
            Physics.IgnoreLayerCollision(_selfLayer, targetLayer, false);
        }
    }
}
