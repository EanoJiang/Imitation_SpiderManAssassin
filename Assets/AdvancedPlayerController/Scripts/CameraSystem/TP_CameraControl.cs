using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GGG.Tool;

public class TP_CameraControl : MonoBehaviour
{

    [Header("相机参数配置")]
    [SerializeField] private Transform _lookTarget;             //相机跟随目标
    [SerializeField] private float _controlSpeed;               //相机移动速度
    [SerializeField] private Vector2 _cameraVerticalMaxAngle;   //相机上下旋转角度限制
    [SerializeField] private Vector2 _cameraHorizontalMaxAngle; //相机左右旋转角度限制
    [SerializeField] private float _smoothSpeed;                //平滑速度
    [SerializeField] private float _cameraDistance;             //相机到跟随目标的距离
    [SerializeField] private float _cameraHeight;               //相机高度
    [SerializeField] private float _distanceSmoothTime;         //位置跟随平滑时间

    private Vector3 smoothDampVelocity = Vector3.zero;          //旋转阻尼

    private Vector2 _input;                                     // 输入值
    private Vector3 _cameraRotation;                            // 相机旋转方向
    private bool _cameraInputEnabled = true;                    // 相机输入是否启用

    private void Awake()
    {
        //隐藏光标
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        // 检测到按下ESC键或鼠标左键点击窗口，则切换相机输入状态
        HandleCameraInputToggle();

        // 只有在相机输入启用时才处理输入
        if (_cameraInputEnabled)
        {
            // 实时处理相机输入
            CameraInput();
        }
    }


    private void LateUpdate()
    {
        // 更新相机旋转
        UpdateCameraRotation();
        // 更新相机位置
        UpdateCameraPosition();
    }

    /// <summary>
    /// 处理相机输入，获取并处理上下查看等输入，限制垂直角度范围
    /// </summary>
    private void CameraInput()
    {
        // 获取相机xy轴输入
        _input.y += GameInputManager.MainInstance.CameraLook.x * _controlSpeed;
        _input.x -= GameInputManager.MainInstance.CameraLook.y * _controlSpeed;

        // 限制相机垂直方向角度范围，垂直方向是绕 x 轴旋转，所以平滑的是x轴输入
        _input.x = Mathf.Clamp(
            _input.x,
            _cameraVerticalMaxAngle.x,
            _cameraVerticalMaxAngle.y
        );

        // 限制相机水平方向角度范围，水平方向是绕 y 轴旋转，所以限制的是y轴输入
        _input.y = Mathf.Clamp(
            _input.y,
            _cameraHorizontalMaxAngle.x,
            _cameraHorizontalMaxAngle.y
        );

    }

    /// <summary>
    /// 更新相机旋转
    /// </summary>
    private void UpdateCameraRotation()
    {
        var targetRotation = new Vector3(_input.x, _input.y, 0);
        _cameraRotation = Vector3.SmoothDamp(
            _cameraRotation,
            targetRotation,
            ref smoothDampVelocity,
            _smoothSpeed
        );

        //更新相机欧拉角
        transform.eulerAngles = _cameraRotation;

    }

    /// <summary>
    /// 更新相机位置
    /// </summary>
    private void UpdateCameraPosition()
    {
        var newPos = _lookTarget.position 
            + Vector3.back * _cameraDistance 
            + Vector3.up * _cameraHeight;
        // 平滑位置移动
        transform.position = Vector3.Lerp(
            transform.position,
            newPos,
            DevelopmentTools.UnTetheredLerp(_distanceSmoothTime)
        );
    }

    /// <summary>
    /// 处理相机输入状态切换
    /// </summary>
    private void HandleCameraInputToggle()
    {
        // 检测ESC键切换相机输入状态
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            _cameraInputEnabled = false;
            // 显示光标并解锁
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        // 检测鼠标左键点击窗口来恢复相机控制
        if (Input.GetMouseButtonDown(0) && !_cameraInputEnabled)
        {
            _cameraInputEnabled = true;
            // 隐藏光标并锁定
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

}
