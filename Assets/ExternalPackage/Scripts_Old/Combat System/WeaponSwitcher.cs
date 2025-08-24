using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class WeaponSwitcher : MonoBehaviour
{
    [Header("武器设置")]
    [SerializeField] private GameObject weaponObject; // 场景中的武器对象
    [SerializeField] private Transform rightHandBone; // 右手骨骼
    
    [Header("收起武器设置")]
    [SerializeField] private Transform stowedParent; // 武器收起时的父对象
    [SerializeField] private Vector3 stowedPosition = new Vector3(0, -1, 0); // 武器收起时的位置
    [SerializeField] private Vector3 stowedRotation = Vector3.zero; // 武器收起时的旋转（欧拉角）
    
    [Header("动画设置")]
    [SerializeField] private string combatLayerName = "Combat"; // Combat层级名称
    [SerializeField] private string equipWeaponAnimation; // 装备武器动画名称
    [SerializeField] private string stowWeaponAnimation; // 收起武器动画名称
    [SerializeField] private float animationTransitionTime = 0.1f; // 动画过渡时间
    
    private GameObject currentWeapon; // 当前武器实例
    private int combatLayerIndex; // Combat层级索引
    private bool hasWeapon = true; // 默认状态
    private bool isAnimating = false; // 是否正在播放动画
    private Vector3 equippedPosition; // 武器装备时的位置（手部位置）
    private Quaternion equippedRotation; // 武器装备时的旋转（手部旋转）
    Animator animator; // 动画控制器

    void Awake()
    {
        animator = GetComponent<Animator>();
        // 如果没有指定Animator，尝试获取
        if (animator == null)
            animator = GetComponent<Animator>();
            
        // 获取Combat层级索引
        combatLayerIndex = animator.GetLayerIndex(combatLayerName);
        
        // 如果没有找到Combat层级，输出警告
        if (combatLayerIndex == -1)
        {
            Debug.LogWarning($"未找到名为 '{combatLayerName}' 的动画层级！");
        }
        
        // 设置武器对象
        if (weaponObject != null)
        {
            currentWeapon = weaponObject;
            
            // 检查武器是否已经在右手骨骼下
            if (currentWeapon.transform.parent == rightHandBone)
            {
                // 武器已经在手部，保存装备状态的位置和旋转
                equippedPosition = currentWeapon.transform.localPosition;
                equippedRotation = currentWeapon.transform.localRotation;
                
                // 如果没有设置收起父对象，默认使用角色本身
                if (stowedParent == null)
                {
                    stowedParent = transform;
                }
                
                // 武器已经在手部，启用Combat层级
                EnableCombatLayer(true);
                Debug.Log("武器已在手部，Combat层级已启用");
            }
            else
            {
                // 武器不在手部，需要设置装备位置
                equippedPosition = Vector3.zero;
                equippedRotation = Quaternion.identity;
                
                // 如果没有设置收起父对象，使用武器当前的父对象
                if (stowedParent == null)
                {
                    stowedParent = currentWeapon.transform.parent;
                    stowedPosition = currentWeapon.transform.localPosition;
                    stowedRotation = currentWeapon.transform.localEulerAngles;
                }
                
                // 装备武器到右手
                EquipWeapon();
            }
        }
    }
    
    /// <summary>
    /// 武器切换输入处理（新版输入系统）
    /// </summary>
    /// <param name="value">输入值</param>
    void OnWeaponSwitch(InputValue value)
    {
        if (value.isPressed && !isAnimating)
        {
            ToggleWeapon();
        }
        animator.SetBool("hasWeapon", !hasWeapon);
    }
    
    /// <summary>
    /// 切换武器状态
    /// </summary>
    void ToggleWeapon()
    {
        if (isAnimating) return; // 如果正在播放动画，忽略输入

        if (hasWeapon)
        {
            // 播放收起武器动画
            StartCoroutine(StowWeaponWithAnimation());
        }
        else
        {
            // 播放装备武器动画
            StartCoroutine(EquipWeaponWithAnimation());

        }
    }
    
    /// <summary>
    /// 播放装备武器动画并装备武器
    /// </summary>
    IEnumerator EquipWeaponWithAnimation()
    {
        isAnimating = true;
        
        // 播放装备武器动画
        if (!string.IsNullOrEmpty(equipWeaponAnimation))
        {
            animator.CrossFade(equipWeaponAnimation, animationTransitionTime);
            
            // 等待动画播放完成
            yield return StartCoroutine(WaitForAnimationComplete(equipWeaponAnimation));
        }
        
        // 动画播放完成后装备武器
        EquipWeapon();
        isAnimating = false;
    }
    
    /// <summary>
    /// 播放收起武器动画并收起武器
    /// </summary>
    IEnumerator StowWeaponWithAnimation()
    {
        isAnimating = true;
        
        // 播放收起武器动画
        if (!string.IsNullOrEmpty(stowWeaponAnimation))
        {
            animator.CrossFade(stowWeaponAnimation, animationTransitionTime);
            
            // 等待动画播放完成
            yield return StartCoroutine(WaitForAnimationComplete(stowWeaponAnimation));
        }
        
        // 动画播放完成后收起武器
        StowWeapon();
        isAnimating = false;
    }
    
    /// <summary>
    /// 等待动画播放完成
    /// </summary>
    /// <param name="animationName">动画名称</param>
    /// <returns></returns>
    IEnumerator WaitForAnimationComplete(string animationName)
    {
        // 获取动画片段
        AnimationClip[] clips = animator.runtimeAnimatorController.animationClips;
        AnimationClip targetClip = null;
        
        foreach (AnimationClip clip in clips)
        {
            if (clip.name == animationName)
            {
                targetClip = clip;
                break;
            }
        }
        
        if (targetClip != null)
        {
            // 等待动画播放完成
            yield return new WaitForSeconds(targetClip.length);
        }
        else
        {
            // 如果找不到动画，等待一个默认时间
            Debug.LogWarning($"未找到名为 '{animationName}' 的动画片段！");
            yield return new WaitForSeconds(1f);
        }
    }
    
    /// <summary>
    /// 装备武器到右手
    /// </summary>
    void EquipWeapon()
    {
        if (currentWeapon == null)
        {
            Debug.LogError("武器对象未设置！");
            return;
        }
        
        if (rightHandBone == null)
        {
            Debug.LogError("右手骨骼未设置！");
            return;
        }
        
        // 将武器设置为右手骨骼的子对象
        currentWeapon.transform.SetParent(rightHandBone);
        currentWeapon.transform.localPosition = equippedPosition;
        currentWeapon.transform.localRotation = equippedRotation;
        
        hasWeapon = true;

        // 启用Combat层级动画
        EnableCombatLayer(true);
        
        Debug.Log("武器已装备到右手");
    }
    
    /// <summary>
    /// 收起武器
    /// </summary>
    void StowWeapon()
    {
        if (currentWeapon != null)
        {
            // 将武器移动到收起位置
            currentWeapon.transform.SetParent(stowedParent);
            currentWeapon.transform.localPosition = stowedPosition;
            currentWeapon.transform.localEulerAngles = stowedRotation;
        }
        
        hasWeapon = false;

        // 禁用Combat层级动画
        EnableCombatLayer(false);
        
        Debug.Log("武器已收起");
    }
    
    /// <summary>
    /// 启用或禁用Combat层级动画
    /// </summary>
    /// <param name="enable">是否启用</param>
    void EnableCombatLayer(bool enable)
    {
        if (combatLayerIndex != -1)
        {
            animator.SetLayerWeight(combatLayerIndex, enable ? 1f : 0f);
        }
    }
    
    /// <summary>
    /// 检查武器是否在右手骨骼下
    /// </summary>
    /// <returns>是否持有武器</returns>
    public bool HasWeapon()
    {
        return hasWeapon && currentWeapon != null;
    }
    
    /// <summary>
    /// 获取当前武器实例
    /// </summary>
    /// <returns>当前武器GameObject</returns>
    public GameObject GetCurrentWeapon()
    {
        return currentWeapon;
    }
    
    /// <summary>
    /// 检查是否正在播放动画
    /// </summary>
    /// <returns>是否正在播放动画</returns>
    public bool IsAnimating()
    {
        return isAnimating;
    }
} 