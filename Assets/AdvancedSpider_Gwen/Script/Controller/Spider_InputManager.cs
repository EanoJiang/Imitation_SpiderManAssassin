using UnityEngine;

public class Spider_InputManager : Controller
{
    void Update()
    {
        // �ƶ�
        stickL = GameInputManager.Instance.Movement;
        // ת��
        stickR = GameInputManager.Instance.CameraLook;

    }
}
