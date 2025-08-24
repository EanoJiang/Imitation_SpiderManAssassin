using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GGG.Tool.Singleton;

public class GameInputManager : Singleton<GameInputManager>
{
    private GameInputAction _gameInputAction;

    public Vector2 Movement => _gameInputAction.Player.Movement.ReadValue<Vector2>();
    public Vector2 CameraLook => _gameInputAction.Player.CameraLook.ReadValue<Vector2>();

    public bool Run => _gameInputAction.Player.Run.triggered;
    public bool Roll => _gameInputAction.Player.Roll.triggered;
    public bool Jump => _gameInputAction.Player.Jump.triggered;
    public bool LAttack => _gameInputAction.Player.LAttack.triggered;
    public bool RAttack => _gameInputAction.Player.RAttack.triggered;
    public bool FinishAttack => _gameInputAction.Player.FinishAttack.triggered;

    public bool SwitchCharacter => _gameInputAction.Player.SwitchCharacter.triggered;

    protected override void Awake()
    {
        base.Awake();
        _gameInputAction ??= new GameInputAction(); //是空的，则创建新的实例
    }

    private void OnEnable()
    {
        _gameInputAction.Enable();
    }
    private void OnDisable()
    {
        _gameInputAction.Disable();
    }
}
