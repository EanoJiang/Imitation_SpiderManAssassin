using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    //摄像机跟随的目标
    [SerializeField] Transform followTarget;
    //距离
    [SerializeField] float distance;

    // 保存原始跟随目标，用于切换回玩家
    private Transform originalFollowTarget;
    private bool isFollowingHorse = false;

    // 保存原始相机设置
    private Vector2 originalFrameOffset;
    private float originalDistance;
    private float originalMinVerticalAngle;
    private float originalMaxVerticalAngle;

    // 鼠标灵敏度设置
    [Header("鼠标灵敏度设置")]
    [SerializeField] float mouseSensitivity = 2.0f;
    [SerializeField] bool useRawInput = true; // 是否使用原始输入，避免系统鼠标加速影响

    //绕y轴的旋转角度——水平视角旋转
    float rotationY;
    //绕x轴的旋转角度——垂直视角旋转
    float rotationX;
    //限制rotationX幅度
    [SerializeField] float minVerticalAngle = -20;
    [SerializeField] float maxVerticalAngle = 45;
    //框架偏移向量——摄像机位置视差偏移
    [SerializeField] Vector2 frameOffset;

    // 骑马时的镜头偏移设置
    [Header("骑马镜头设置")]
    [SerializeField] Vector2 horseFrameOffset = new Vector2(0, 1.5f); // 骑马时镜头向上偏移
    [SerializeField] float horseDistance = 8f; // 骑马时的相机距离
    [SerializeField] float horseMinVerticalAngle = -15; // 骑马时的最小垂直角度
    [SerializeField] float horseMaxVerticalAngle = 60; // 骑马时的最大垂直角度

    //视角控制反转
    [Header("视角控制反转:invertX是否反转垂直视角,invertY是否反转水平视角")]
    [SerializeField] bool invertX;
    [SerializeField] bool invertY;

    float invertXValue;
    float invertYValue;

    // 帧同步相关参数
    [Header("帧同步设置")]
    [SerializeField] float smoothSpeed = 10f; // 平滑插值速度
    private Vector3 targetPosition;
    private Quaternion targetRotation;
    private bool isInitialized = false;

    private void Start()
    {
        //隐藏光标
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        
        // 保存原始跟随目标
        originalFollowTarget = followTarget;
        
        // 保存原始相机设置
        originalFrameOffset = frameOffset;
        originalDistance = distance;
        originalMinVerticalAngle = minVerticalAngle;
        originalMaxVerticalAngle = maxVerticalAngle;
        
        // 初始化目标位置和旋转
        if (followTarget != null)
        {
            InitializeCamera();
        }
    }

    private void InitializeCamera()
    {
        // 计算初始旋转
        var initialRotation = Quaternion.Euler(rotationX, rotationY, 0);
        var focusPosition = followTarget.position + new Vector3(frameOffset.x, frameOffset.y, 0);
        
        // 设置初始位置和旋转
        transform.position = focusPosition - initialRotation * new Vector3(0, 0, distance);
        transform.rotation = initialRotation;
        
        // 设置目标位置和旋转
        targetPosition = transform.position;
        targetRotation = transform.rotation;
        
        isInitialized = true;
    }

    private void LateUpdate()
    {
        if (followTarget == null || !isInitialized) return;

        //视角控制反转参数
        invertXValue = (invertX) ? -1 : 1;
        invertYValue = (invertY) ? -1 : 1;

        // 获取鼠标输入，使用原始输入避免系统鼠标加速影响
        float mouseX = useRawInput ? Input.GetAxisRaw("Mouse X") : Input.GetAxis("Camera X");
        float mouseY = useRawInput ? Input.GetAxisRaw("Mouse Y") : Input.GetAxis("Camera Y");

        //水平视角控制——鼠标(手柄)x轴控制rotationY
        rotationY += mouseX * mouseSensitivity * invertYValue;
        //垂直视角控制——鼠标(手柄)y轴控制rotationX
        rotationX += mouseY * mouseSensitivity * invertXValue;
        //限制rotationX幅度
        rotationX = Mathf.Clamp(rotationX, minVerticalAngle, maxVerticalAngle);

        //视角旋转参量
        //想要水平旋转视角，所以需要的参量为绕y轴旋转角度
        targetRotation = Quaternion.Euler(rotationX, rotationY, 0);

        //摄像机的焦点位置
        var focusPosition = followTarget.position + new Vector3(frameOffset.x, frameOffset.y, 0);
        
        //摄像机放在目标后面指定距离的位置
        targetPosition = focusPosition - targetRotation * new Vector3(0, 0, distance);

        // 使用平滑插值更新相机位置和旋转，使用Time.deltaTime确保帧率独立性
        float smoothFactor = smoothSpeed * Time.deltaTime;
        transform.position = Vector3.Lerp(transform.position, targetPosition, smoothFactor);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, smoothFactor);
    }



    //水平方向的旋转，返回摄像机的水平旋转四元数。
    public Quaternion PlanarRotation => Quaternion.Euler(0, rotationY, 0);

    /// <summary>
    /// 设置相机距离
    /// </summary>
    public void SetDistance(float newDistance)
    {
        distance = newDistance;
    }

    // 获取当前相机距离
    public float GetDistance => distance;

    /// <summary>
    /// 设置平滑速度
    /// </summary>
    public void SetSmoothSpeed(float speed)
    {
        smoothSpeed = speed;
    }

    /// <summary>
    /// 设置鼠标灵敏度
    /// </summary>
    public void SetMouseSensitivity(float sensitivity)
    {
        mouseSensitivity = sensitivity;
    }

    /// <summary>
    /// 获取当前鼠标灵敏度
    /// </summary>
    public float GetMouseSensitivity => mouseSensitivity;

    /// <summary>
    /// 切换到跟随马
    /// </summary>
    /// <param name="horseTransform">马的Transform组件</param>
    public void SwitchToHorse(Transform horseTransform)
    {
        if (horseTransform != null)
        {
            followTarget = horseTransform;
            isFollowingHorse = true;
            
            // 应用骑马时的相机设置
            frameOffset = horseFrameOffset;
            distance = horseDistance;
            minVerticalAngle = horseMinVerticalAngle;
            maxVerticalAngle = horseMaxVerticalAngle;
            
            // 重新初始化相机位置，确保平滑过渡
            if (isInitialized)
            {
                InitializeCamera();
            }
        }
    }

    /// <summary>
    /// 切换回跟随玩家
    /// </summary>
    public void SwitchBackToPlayer()
    {
        if (originalFollowTarget != null)
        {
            followTarget = originalFollowTarget;
            isFollowingHorse = false;
            
            // 恢复原始相机设置
            frameOffset = originalFrameOffset;
            distance = originalDistance;
            minVerticalAngle = originalMinVerticalAngle;
            maxVerticalAngle = originalMaxVerticalAngle;
            
            // 重新初始化相机位置，确保平滑过渡
            if (isInitialized)
            {
                InitializeCamera();
            }
        }
    }

    /// <summary>
    /// 检查当前是否正在跟随马
    /// </summary>
    public bool IsFollowingHorse => isFollowingHorse;

    /// <summary>
    /// 获取当前跟随目标
    /// </summary>
    public Transform GetCurrentFollowTarget => followTarget;

    /// <summary>
    /// 获取原始跟随目标（通常是玩家）
    /// </summary>
    public Transform GetOriginalFollowTarget => originalFollowTarget;

    /// <summary>
    /// 设置骑马时的镜头偏移
    /// </summary>
    public void SetHorseFrameOffset(Vector2 offset)
    {
        horseFrameOffset = offset;
        if (isFollowingHorse)
        {
            frameOffset = horseFrameOffset;
        }
    }

    /// <summary>
    /// 设置骑马时的相机距离
    /// </summary>
    public void SetHorseDistance(float newDistance)
    {
        horseDistance = newDistance;
        if (isFollowingHorse)
        {
            distance = horseDistance;
        }
    }

    /// <summary>
    /// 设置骑马时的垂直角度限制
    /// </summary>
    public void SetHorseVerticalAngles(float minAngle, float maxAngle)
    {
        horseMinVerticalAngle = minAngle;
        horseMaxVerticalAngle = maxAngle;
        if (isFollowingHorse)
        {
            minVerticalAngle = horseMinVerticalAngle;
            maxVerticalAngle = horseMaxVerticalAngle;
        }
    }
}
