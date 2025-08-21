using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ����������ϣֵ�����࣬����ͳһ�洢Animator�����Ĺ�ϣֵ�������ظ�����
/// </summary>
public class AnimationID
{
    // ��ɫ�ƶ���ض���������ϣ
    public static readonly int MovementID = Animator.StringToHash("Movement");
    public static readonly int LockID = Animator.StringToHash("Lock");
    public static readonly int HorizontalID = Animator.StringToHash("Horizontal");
    public static readonly int VerticalID = Animator.StringToHash("Vertical");
    public static readonly int HasInputID = Animator.StringToHash("HasInput");
    public static readonly int RunID = Animator.StringToHash("Run");
    public static readonly int DeltaAngleID = Animator.StringToHash("DeltaAngle");
}