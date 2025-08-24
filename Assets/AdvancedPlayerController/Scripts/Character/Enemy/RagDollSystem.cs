using GGG.Tool;
using RootMotion.FinalIK;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerDeath : MonoBehaviour
{
    //  ragdoll �����������
    private RagdollUtility ragdollUtility;
    // ����״̬���
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
        // �л�����״̬
        if (_animator.AnimationAtTag("AssassinHit") && isDead == false)
        {
            isDead = true;
            StartCoroutine(WaitForAnimationThenEnableRagdoll());
        }
    }

    private IEnumerator WaitForAnimationThenEnableRagdoll()
    {
        // �ȴ���ǰ�����������
        // ��ȡ��ǰ״̬��Ϣ
        AnimatorStateInfo stateInfo = _animator.GetCurrentAnimatorStateInfo(0);

        // �ȴ������������
        yield return new WaitForSeconds(stateInfo.length - _animator.GetCurrentAnimatorStateInfo(0).normalizedTime * stateInfo.length);

        // ����������Ϻ����� ragdoll
        ragdollUtility.EnableRagdoll();
    }
    public bool AnimationAtTag(Animator animator, string tagName, int indexLayer = 0)
    {
        return animator.GetCurrentAnimatorStateInfo(indexLayer).IsTag(tagName);
    }
}