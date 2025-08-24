using UnityEngine;
using UnityEngine.UI;

public class CameraZoomController : MonoBehaviour
{
    // 相机焦距控制
    [Header("相机焦距控制")]
    [SerializeField] float minDistance = 1f; 
    [SerializeField] float maxDistance = 15f; 
    [SerializeField] float zoomSpeed = 2f;       
    private bool isZoomMode = false;
    
    // UI提示
    [Header("UI提示")]
    [SerializeField] GameObject zoomHintPanel;    // 缩放提示面板
    [SerializeField] Text zoomHintText;           // 提示文本组件

    // 相机控制器引用
    private CameraMainController cameraController;

    private void Start()
    {
        // 获取同一GameObject上的CameraMainController组件
        cameraController = GetComponent<CameraMainController>();
        
        // 初始化UI提示
        if (zoomHintPanel != null)
        {
            zoomHintPanel.SetActive(false);
        }
    }

    private void Update()
    {
        // 检测V键切换缩放模式
        if (Input.GetKeyDown(KeyCode.V))
        {
            ToggleZoomMode();
        }

        // 在缩放模式下处理鼠标滚轮缩放
        if (isZoomMode)
        {
            HandleZoom();
        }
    }

    /// <summary>
    /// 切换缩放模式
    /// </summary>
    private void ToggleZoomMode()
    {
        isZoomMode = !isZoomMode;
        
        if (isZoomMode)
        {
            // 进入缩放模式，显示提示
            ShowZoomHint();
        }
        else
        {
            // 退出缩放模式，隐藏提示
            HideZoomHint();
        }
    }
    
    /// <summary>
    /// 处理鼠标滚轮缩放
    /// </summary>
    private void HandleZoom()
    {
        float scrollInput = Input.GetAxis("Mouse ScrollWheel");
        if (scrollInput != 0 && cameraController != null)
        {
            // 获取当前距离
            float currentDistance = cameraController.GetDistance;
            // 根据滚轮方向调整距离
            currentDistance -= scrollInput * zoomSpeed;
            // 限制距离在有效范围内
            currentDistance = Mathf.Clamp(currentDistance, minDistance, maxDistance);
            // 设置新的距离值
            cameraController.SetDistance(currentDistance);
        }
    }
    
    /// <summary>
    /// 显示缩放提示
    /// </summary>
    private void ShowZoomHint()
    {
        if (zoomHintPanel != null)
        {
            zoomHintPanel.SetActive(true);
        }
        
        if (zoomHintText != null)
        {
            zoomHintText.text = "使用鼠标滚轮调整相机焦距\n按V键退出缩放模式";
        }
    }
    
    /// <summary>
    /// 隐藏缩放提示
    /// </summary>
    private void HideZoomHint()
    {
        if (zoomHintPanel != null)
        {
            zoomHintPanel.SetActive(false);
        }
    }

    /// <summary>
    /// 获取是否处于缩放模式
    /// </summary>
    public bool IsZoomMode()
    {
        return isZoomMode;
    }
} 