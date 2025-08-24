using UnityEngine;

[RequireComponent(typeof(Animator))]
public class RandomIdleAnimation : MonoBehaviour
{
    [Header("混合树中待机动画数量")]
    [SerializeField] private int IdleNum = 9;
    private Animator animator;
    private float idleTimeCounter = 0f;
    private bool isInIdleState = false;
    private const float idleThreshold = 5f;

    private const string idleStateName = "Idle";
    private const string blendTreeParameter = "IdleType";

    void Start()
    {
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        if(GameInputManager.Instance.Movement != null)
        {
            animator.SetBool("BackToBaseIdle", true);
        }
        else
        {
            animator.SetBool("BackToBaseIdle", false);
        }
        // 检查当前是否处于Idle状态
        bool isCurrentStateIdle = IsInIdleState();

        if (isCurrentStateIdle)
        {
            idleTimeCounter += Time.deltaTime;
            isInIdleState = true;

            if (idleTimeCounter >= idleThreshold)
            {
                RandomizeIdleAnimation();
                idleTimeCounter = 0f;
            }
        }
        else
        {
            // 离开Idle状态时重置
            idleTimeCounter = 0f;
            isInIdleState = false;
        }
    }

    // 检查是否处于Idle状态
    private bool IsInIdleState()
    {
        if (animator.layerCount == 0)
            return false;

        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        // 使用IsName方法检查状态名称，这比哈希值比较更可靠
        return stateInfo.IsName(idleStateName) && !animator.IsInTransition(0);
    }

    /// <summary>
    /// 随机选择待机动画
    /// </summary>
    private void RandomizeIdleAnimation()
    {
        int randomInt = Random.Range(0, IdleNum);
        animator.SetFloat(blendTreeParameter, randomInt);
    }
}
