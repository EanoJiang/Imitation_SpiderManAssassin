using HoaxGames;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerJump : MonoBehaviour
{
    private void Update()
    {
        if (GameInputManager.MainInstance.Jump)
        {
            GameEventManager.MainInstance.CallEvent("��ɫ��Ծ");
        }
    }

}
