using GGG.Tool;
using RootMotion.FinalIK;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerDeath : MonoBehaviour
{
    //  ragdoll 工具组件引用
    private RagdollUtility ragdollUtility;
    // 死亡状态标记
    private bool isDead;
    private Animator _animator;

    private void Start()
    {
        ragdollUtility = GetComponent<RagdollUtility>();
        _animator = GetComponent<Animator>();
        isDead = false;
    }

    private void Update()
    {
        // 切换死亡状态
        if (_animator.AnimationAtTag("AssassinHit") && isDead == false)
        {
            isDead = true;
            StartCoroutine(WaitForAnimationThenEnableRagdoll());
        }
    }

    private IEnumerator WaitForAnimationThenEnableRagdoll()
    {
        // 等待当前动画播放完毕
        // 获取当前状态信息
        AnimatorStateInfo stateInfo = _animator.GetCurrentAnimatorStateInfo(0);

        // 等待动画播放完成
        yield return new WaitForSeconds(stateInfo.length - _animator.GetCurrentAnimatorStateInfo(0).normalizedTime * stateInfo.length);

        // 动画播放完毕后启用 ragdoll
        ragdollUtility.EnableRagdoll();
    }
    public bool AnimationAtTag(Animator animator, string tagName, int indexLayer = 0)
    {
        return animator.GetCurrentAnimatorStateInfo(indexLayer).IsTag(tagName);
    }
}