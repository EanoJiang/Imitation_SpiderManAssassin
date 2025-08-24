using UnityEngine;

[RequireComponent(typeof(Animator))]
public class RandomIdleAnimation : MonoBehaviour
{
    [Header("������д�����������")]
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
        // ��鵱ǰ�Ƿ���Idle״̬
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
            // �뿪Idle״̬ʱ����
            idleTimeCounter = 0f;
            isInIdleState = false;
        }
    }

    // ����Ƿ���Idle״̬
    private bool IsInIdleState()
    {
        if (animator.layerCount == 0)
            return false;

        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        // ʹ��IsName�������״̬���ƣ���ȹ�ϣֵ�Ƚϸ��ɿ�
        return stateInfo.IsName(idleStateName) && !animator.IsInTransition(0);
    }

    /// <summary>
    /// ���ѡ���������
    /// </summary>
    private void RandomizeIdleAnimation()
    {
        int randomInt = Random.Range(0, IdleNum);
        animator.SetFloat(blendTreeParameter, randomInt);
    }
}
