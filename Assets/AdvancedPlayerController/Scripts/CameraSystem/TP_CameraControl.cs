using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GGG.Tool;

public class TP_CameraControl : MonoBehaviour
{

    [Header("�����������")]
    [SerializeField] private Transform _lookTarget;             //�������Ŀ��
    [SerializeField] private float _controlSpeed;               //����ƶ��ٶ�
    [SerializeField] private Vector2 _cameraVerticalMaxAngle;   //���������ת�Ƕ�����
    [SerializeField] private Vector2 _cameraHorizontalMaxAngle; //���������ת�Ƕ�����
    [SerializeField] private float _smoothSpeed;                //ƽ���ٶ�
    [SerializeField] private float _cameraDistance;             //���������Ŀ��ľ���
    [SerializeField] private float _cameraHeight;               //����߶�
    [SerializeField] private float _distanceSmoothTime;         //λ�ø���ƽ��ʱ��

    private Vector3 smoothDampVelocity = Vector3.zero;          //��ת����

    private Vector2 _input;                                     // ����ֵ
    private Vector3 _cameraRotation;                            // �����ת����
    private bool _cameraInputEnabled = true;                    // ��������Ƿ�����

    private void Awake()
    {
        //���ع��
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        // ��⵽����ESC����������������ڣ����л��������״̬
        HandleCameraInputToggle();

        // ֻ���������������ʱ�Ŵ�������
        if (_cameraInputEnabled)
        {
            // ʵʱ�����������
            CameraInput();
        }
    }


    private void LateUpdate()
    {
        // ���������ת
        UpdateCameraRotation();
        // �������λ��
        UpdateCameraPosition();
    }

    /// <summary>
    /// ����������룬��ȡ���������²鿴�����룬���ƴ�ֱ�Ƕȷ�Χ
    /// </summary>
    private void CameraInput()
    {
        // ��ȡ���xy������
        _input.y += GameInputManager.MainInstance.CameraLook.x * _controlSpeed;
        _input.x -= GameInputManager.MainInstance.CameraLook.y * _controlSpeed;

        // ���������ֱ����Ƕȷ�Χ����ֱ�������� x ����ת������ƽ������x������
        _input.x = Mathf.Clamp(
            _input.x,
            _cameraVerticalMaxAngle.x,
            _cameraVerticalMaxAngle.y
        );

        // �������ˮƽ����Ƕȷ�Χ��ˮƽ�������� y ����ת���������Ƶ���y������
        _input.y = Mathf.Clamp(
            _input.y,
            _cameraHorizontalMaxAngle.x,
            _cameraHorizontalMaxAngle.y
        );

    }

    /// <summary>
    /// ���������ת
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

        //�������ŷ����
        transform.eulerAngles = _cameraRotation;

    }

    /// <summary>
    /// �������λ��
    /// </summary>
    private void UpdateCameraPosition()
    {
        var newPos = _lookTarget.position 
            + Vector3.back * _cameraDistance 
            + Vector3.up * _cameraHeight;
        // ƽ��λ���ƶ�
        transform.position = Vector3.Lerp(
            transform.position,
            newPos,
            DevelopmentTools.UnTetheredLerp(_distanceSmoothTime)
        );
    }

    /// <summary>
    /// �����������״̬�л�
    /// </summary>
    private void HandleCameraInputToggle()
    {
        // ���ESC���л��������״̬
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            _cameraInputEnabled = false;
            // ��ʾ��겢����
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        // �������������������ָ��������
        if (Input.GetMouseButtonDown(0) && !_cameraInputEnabled)
        {
            _cameraInputEnabled = true;
            // ���ع�겢����
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

}
