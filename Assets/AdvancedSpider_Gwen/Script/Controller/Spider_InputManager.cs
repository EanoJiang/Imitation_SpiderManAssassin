using UnityEngine;

public class Spider_InputManager : Controller
{
    void Update()
    {
        // ÒÆ¶¯
        stickL = GameInputManager.Instance.Movement;
        // ×ªÏò
        stickR = GameInputManager.Instance.CameraLook;

    }
}
