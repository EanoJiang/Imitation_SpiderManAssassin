using HoaxGames;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThirdPersonJump : MonoBehaviour
{
    //flag
    //是否跳跃
    public bool jump;
    //public bool lockPlannar;
    
    // 跳跃相关参数
    [Header("跳跃参数")]
    public float jumpForce = 7f;           // 跳跃力度
    public float gravity = -9.8f;           // 重力
    public float fallMultiplier = 0.1f;    // 正常下落倍率
    public float lowJumpMultiplier = 2f;   // 短按起跳键时的下落倍率
   
    private Vector3 velocity;              // 竖直速度
    private bool isJumpAnimationPlaying = false; // 跳跃动画是否正在播放

    Animator animator;
    ThirdPersonMove thirdPersonMove;
    CharacterController characterController;



    private void Awake()
    {
        animator = GetComponent<Animator>();
        thirdPersonMove = GetComponent<ThirdPersonMove>();
        characterController = GetComponent<CharacterController>();
    }

    // Update is called once per frame
    void Update()
    {
        animator.SetBool("isGrounded", characterController.isGrounded);
        //Debug.Log(characterController.isGrounded);
        // 处理跳跃输入
        HandleJumpInput();

    }

    void OnAnimatorMove()
    {
        // 水平移动自动和物理系统同步更新
        Vector3 horizontalVelocity = animator.velocity;
        horizontalVelocity.y = 0; // 忽略动画的竖直速度
        characterController.SimpleMove(horizontalVelocity);

        // 应用重力
        ApplyGravity();
        // 竖直移动手动控制
        ApplyVerticalMovement();
    }



    /// <summary>
    /// 处理跳跃输入
    /// </summary>
    void HandleJumpInput()
    {
        jump = Input.GetButtonDown("Jump");

        // 检查跳跃动画是否正在播放
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        isJumpAnimationPlaying = stateInfo.IsName("Jump") && stateInfo.normalizedTime < 1.0f;

        if (jump && characterController.isGrounded && !isJumpAnimationPlaying)
        {
            velocity.y = jumpForce;
            animator.SetBool("jump", true);
        }
        else
        {
            animator.SetBool("jump", false);
        }
    }

    /// <summary>
    /// 应用重力
    /// </summary>
    void ApplyGravity()
    {
        if (velocity.y < 0)
        {
            // 下落时的重力
            velocity.y += gravity * fallMultiplier * Time.deltaTime;
        }
        else if (velocity.y > 0 && !Input.GetButton("Jump"))
        {
            // 上升时如果松开跳跃键，应用短跳重力
            velocity.y += gravity * lowJumpMultiplier * Time.deltaTime;
        }
        else
        {
            // 正常重力
            velocity.y += gravity * Time.deltaTime;
        }
    }

    /// <summary>
    /// 应用竖直移动
    /// </summary>
    void ApplyVerticalMovement()
    {
        Vector3 verticalMovement = new Vector3(0, velocity.y, 0);
        characterController.Move(verticalMovement * Time.deltaTime);
    }

    /// <summary>
    /// 起跳和落地角色控制权
    /// </summary>
    public void OnJumpEnter()
    {
        Debug.Log("起跳");
        //lockPlannar = true;
    }
    
    public void OnJumpExit()
    {
        Debug.Log("落地");
        //lockPlannar = false;
    }
    

    // 获取当前竖直速度
    public float GetVerticalVelocity => velocity.y;

}
