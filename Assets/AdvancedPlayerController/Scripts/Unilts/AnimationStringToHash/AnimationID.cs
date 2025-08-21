using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 动画参数哈希值管理类，用于统一存储Animator参数的哈希值，避免重复计算
/// </summary>
public class AnimationID
{
    // 角色移动相关动画参数哈希
    public static readonly int MovementID = Animator.StringToHash("Movement");
    public static readonly int LockID = Animator.StringToHash("Lock");
    public static readonly int HorizontalID = Animator.StringToHash("Horizontal");
    public static readonly int VerticalID = Animator.StringToHash("Vertical");
    public static readonly int HasInputID = Animator.StringToHash("HasInput");
    public static readonly int RunID = Animator.StringToHash("Run");
    public static readonly int DeltaAngleID = Animator.StringToHash("DeltaAngle");
}