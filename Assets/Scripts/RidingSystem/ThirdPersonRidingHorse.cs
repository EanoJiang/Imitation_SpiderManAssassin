using MalbersAnimations;
using MalbersAnimations.HAP;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThirdPersonRidingHorse : MonoBehaviour
{
    [Header("骑马参数")]
    public GameObject horse;
    public bool isOnHorse = false;

    [Header("相机设置")]
    [SerializeField] CameraController cameraController; // 相机控制器引用

    [Header("动画参数")]
    public int mountLayerIndex = 3;
    public int ridingLayerIndex = 2;
    public int dismountLayerIndex = 4; // 下马动画层

    CharacterController characterController;
    Animator animator;
    ThirdPersonMove thirdPersonMove;
    
    // 上马状态管理
    private bool isMounting = false;
    private bool isDismounting = false;
    private MountTriggers currentMountTrigger;
    private Coroutine mountCoroutine;
    private Coroutine dismountCoroutine;

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
        thirdPersonMove = GetComponent<ThirdPersonMove>();
        
        // 如果没有手动设置相机控制器，自动查找
        if (cameraController == null)
        {
            cameraController = FindObjectOfType<CameraController>();
        }
    }

    private void Update()
    {
        HandleInput();
        
        if (isOnHorse)
        {
            var axisX = Input.GetAxis("Horizontal");
            var axisY = Input.GetAxis("Vertical");
            animator.SetFloat("AxisX", axisX);
            animator.SetFloat("AxisY", axisY);
        }
        Ride();
    }

    void Ride()
    {
        //上马
        if (isOnHorse && !isDismounting)
        {
            Update_MountPlayerPosition();
        }

    }

    /// <summary>
    /// 更新上马时的位置朝向
    /// </summary>
    private void Update_MountPlayerPosition()
    {
        //将角色放到马上
        var playerPoint = horse.transform.Find("PlayerPoint");
        if (playerPoint != null)
        {
            transform.SetParent(playerPoint);
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;
        }
        //在马上禁用角色的characterController和move
        characterController.enabled = false;
        thirdPersonMove.enabled = false;
        //开启马的输入控制脚本
        horse.GetComponent<MalbersInput>().enabled = true;

        //切换马上动作状态,即权重从0到1
        animator.SetLayerWeight(ridingLayerIndex, 1f);
    }

    /// <summary>
    /// 上马触发器
    /// </summary>
    /// <param name="other"></param>
    void OnTriggerEnter(Collider other)
    {
        //在马下面进入触发器
        if(other.gameObject.tag == "MountTrigger" && !isOnHorse && !isMounting)
        {
            // 获取MountTriggers组件
            MountTriggers mountTrigger = other.GetComponent<MountTriggers>();
            if (mountTrigger != null)
            {
                currentMountTrigger = mountTrigger;
                Debug.Log("进入上马触发区域，按F键上马");
            }
        }
    }

    /// <summary>
    /// 离开上马触发器
    /// </summary>
    /// <param name="other"></param>
    void OnTriggerExit(Collider other)
    {
        if(other.gameObject.tag == "MountTrigger" && !isOnHorse && !isMounting)
        {
            currentMountTrigger = null;
            Debug.Log("离开上马触发区域");
        }
    }

    /// <summary>
    /// 开始上马流程
    /// </summary>
    private void StartMounting()
    {
        if (currentMountTrigger == null || isMounting || isOnHorse) return;

        isMounting = true;
        
        // 获取玩家的Animator组件
        Animator playerAnimator = GetComponent<Animator>();
        if (playerAnimator != null)
        {
            // 设置Mount Layer权重为1
            playerAnimator.SetLayerWeight(mountLayerIndex, 1f);
            
            // 播放上马动画
            playerAnimator.Play(currentMountTrigger.MountAnimation, mountLayerIndex);
            
            // 开始协程监听动画播放完成
            mountCoroutine = StartCoroutine(WaitForMountAnimationComplete());
        }
    }

    /// <summary>
    /// 等待上马动画播放完成
    /// </summary>
    private IEnumerator WaitForMountAnimationComplete()
    {
        Animator playerAnimator = GetComponent<Animator>();
        
        // 等待动画播放完成
        yield return new WaitForSeconds(0.1f); // 等待动画开始
        
        // 获取动画状态信息
        AnimatorStateInfo stateInfo = playerAnimator.GetCurrentAnimatorStateInfo(mountLayerIndex);
        
        // 等待动画播放到50%就完成上马
        while (stateInfo.normalizedTime < 0.5f)
        {
            yield return null;
            stateInfo = playerAnimator.GetCurrentAnimatorStateInfo(mountLayerIndex);
        }
        
        // 动画播放到50%，执行上马完成逻辑
        CompleteMounting();
    }

    /// <summary>
    /// 完成上马
    /// </summary>
    private void CompleteMounting()
    {
        // 立即停止Mount Layer动画并设置权重为0
        animator.SetLayerWeight(mountLayerIndex, 0f);
        animator.Play("Empty", mountLayerIndex); // 播放空动画来停止当前动画
        
        // 设置上马状态
        isOnHorse = true;
        isMounting = false;
        
        // 移动到PlayerPoint位置
        var playerPoint = horse.transform.Find("PlayerPoint");
        if (playerPoint != null)
        {
            transform.SetParent(playerPoint);
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;
        }
        
        // 禁用角色控制器和移动脚本
        characterController.enabled = false;
        thirdPersonMove.enabled = false;
        
        // 开启马的输入控制脚本
        horse.GetComponent<MalbersInput>().enabled = true;
        
        // 立即设置RidingHorse动画层权重为1，过渡到骑马状态
        animator.SetLayerWeight(ridingLayerIndex, 1f);
        
        // 切换相机跟随目标到马
        if (cameraController != null && horse != null)
        {
            cameraController.SwitchToHorse(horse.transform);
        }
        
        currentMountTrigger = null;
        mountCoroutine = null;
        
        Debug.Log("上马完成");
    }

    /// <summary>
    /// 开始下马流程
    /// </summary>
    private void StartDismounting()
    {
        if (!isOnHorse || isDismounting) return;

        isDismounting = true;
        
        // 获取玩家的Animator组件
        Animator playerAnimator = GetComponent<Animator>();
        if (playerAnimator != null)
        {
            // 设置Dismount Layer权重为1
            playerAnimator.SetLayerWeight(dismountLayerIndex, 1f);
            
            // 播放下马动画（可以根据需要设置不同的下马动画）
            playerAnimator.Play("Rider_Dismount_Right", dismountLayerIndex);
            
            // 开始协程监听动画播放完成
            dismountCoroutine = StartCoroutine(WaitForDismountAnimationComplete());
        }
    }

    /// <summary>
    /// 等待下马动画播放完成
    /// </summary>
    private IEnumerator WaitForDismountAnimationComplete()
    {
        Animator playerAnimator = GetComponent<Animator>();
        
        // 等待动画播放完成
        yield return new WaitForSeconds(0.1f); // 等待动画开始
        
        // 获取动画状态信息
        AnimatorStateInfo stateInfo = playerAnimator.GetCurrentAnimatorStateInfo(dismountLayerIndex);
        
        // 等待动画完全播放完毕
        while (stateInfo.normalizedTime < 1.0f)
        {
            yield return null;
            stateInfo = playerAnimator.GetCurrentAnimatorStateInfo(dismountLayerIndex);
        }
        
        // 动画播放完毕，执行下马完成逻辑
        CompleteDismounting();
    }

    /// <summary>
    /// 完成下马
    /// </summary>
    private void CompleteDismounting()
    {
        // 立即停止Dismount Layer动画并设置权重为0
        animator.SetLayerWeight(dismountLayerIndex, 0f);
        animator.Play("Empty", dismountLayerIndex); // 播放空动画来停止当前动画
        
        // 设置下马状态
        isOnHorse = false;
        isDismounting = false;
        
        if (horse != null)
        {
            // 设置角色位置（从马上下来）
            transform.SetParent(null);
            
            // 计算下马后的位置（从马的右边下马）
            Vector3 dismountPosition = horse.transform.position + horse.transform.right * 2f;
            transform.position = dismountPosition;
            transform.rotation = horse.transform.rotation;
            
            // 下马后恢复角色的characterController和move
            characterController.enabled = true;
            thirdPersonMove.enabled = true;
            
            // 关闭马的输入控制脚本
            horse.GetComponent<MalbersInput>().enabled = false;
            
            // 立即关闭马上动作层，过渡到正常状态
            animator.SetLayerWeight(ridingLayerIndex, 0f);
            
            // 切换相机跟随目标回玩家
            if (cameraController != null)
            {
                cameraController.SwitchBackToPlayer();
            }
        }
        
        dismountCoroutine = null;
        
        Debug.Log("下马完成");
    }

    /// <summary>
    /// 处理输入
    /// </summary>
    private void HandleInput()
    {
        // 在触发器内按F键开始上马
        if (currentMountTrigger != null && Input.GetKeyDown(KeyCode.F) && !isMounting && !isOnHorse)
        {
            StartMounting();
        }
        
        // 在马上按F键开始下马
        if (isOnHorse && Input.GetKeyDown(KeyCode.F) && !isDismounting)
        {
            StartDismounting();
        }
    }
}
