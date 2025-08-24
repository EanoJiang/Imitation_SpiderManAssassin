using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerRoll : MonoBehaviour
{
    private Animator _animator;
    private bool _isRolling = false; // 新增：判断是否正在翻滚
    private float _rollDuration = 0.5f; // 翻滚动画持续时间，根据实际动画调整
    private float _rollTimer = 0f; // 翻滚计时器

    private void Awake()
    {
        _animator = GetComponent<Animator>();
    }

    private void Update()
    {
        // 如果正在翻滚，更新计时器并跳过参数更新
        if (_isRolling)
        {
            _rollTimer += Time.deltaTime;
            if (_rollTimer >= _rollDuration)
            {
                // 翻滚结束，重置状态
                _isRolling = false;
                _rollTimer = 0f;
            }
            return; // 正在翻滚时不更新移动参数
        }

        //// 只有不在翻滚状态时才更新移动参数
        //var AxisX = GameInputManager.MainInstance.Movement.x;
        //var AxisY = GameInputManager.MainInstance.Movement.y;

        //// 平滑改变轴向
        //AxisX = Mathf.MoveTowards(AxisX, GameInputManager.MainInstance.Movement.x, Time.deltaTime * 5f);
        //AxisY = Mathf.MoveTowards(AxisY, GameInputManager.MainInstance.Movement.y, Time.deltaTime * 5f);

        //_animator.SetFloat("AxisX", AxisX);
        //_animator.SetFloat("AxisY", AxisY);

        // 检测翻滚输入，只有不在翻滚状态时才能触发新的翻滚
        if (GameInputManager.Instance.Roll)
        {
            _isRolling = true;
            _animator.SetTrigger("Roll"); // 建议使用Trigger来触发翻滚动画
            GameEventManager.MainInstance.CallEvent("角色翻滚");
        }
    }
}