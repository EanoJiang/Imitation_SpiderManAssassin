using UnityEngine;
using System.Collections;

public class CharacterSwitcher : MonoBehaviour
{
    [Header("角色设置")]
    public GameObject character1;
    public GameObject character2;

    [Header("切换按键")]
    public KeyCode switchKey = KeyCode.Tab;

    [Header("当前状态")]
    public bool isCharacter1Active = true;

    [Header("角色2专用相机")]
    public Camera camera2;

    [Header("切换延迟")]
    public float switchDelay = 0.5f;   // 等待时间

    private bool isSwitching = false;    // 正在等待切换

    private void Start()
    {
        if (character1 == null || character2 == null)
        {
            Debug.LogError("请在Inspector中指定两个角色的GameObject！");
            return;
        }

        character1.SetActive(isCharacter1Active);
        character2.SetActive(!isCharacter1Active);

        if (camera2 != null)
            camera2.gameObject.SetActive(!isCharacter1Active);
    }

    private void Update()
    {
        if (Input.GetKeyDown(switchKey) && !isSwitching)
            SwitchCharacter();
    }

    /* 供外部脚本调用的接口同样延迟 */
    public void SwitchCharacter()
    {
        if (character1 == null || character2 == null || isSwitching)
            return;

        isSwitching = true;

        /* 立即冻结当前角色，防止继续移动 */
        FreezeMovement(GetActiveCharacter());

        /* 延迟真正切换 */
        StartCoroutine(DelayedSwitch());
    }

    public void SwitchToSpecificCharacter(bool switchToCharacter1)
    {
        if (isCharacter1Active == switchToCharacter1 || isSwitching)
            return;

        isSwitching = true;
        FreezeMovement(GetActiveCharacter());
        StartCoroutine(DelayedSwitch(switchToCharacter1));
    }

    /* 0.5 秒后真正切换 */
    private IEnumerator DelayedSwitch(bool? targetState = null)
    {
        yield return new WaitForSeconds(switchDelay);

        bool nextState = targetState ?? !isCharacter1Active;

        isCharacter1Active = nextState;
        character1.SetActive(isCharacter1Active);
        character2.SetActive(!isCharacter1Active);

        if (camera2 != null)
            camera2.gameObject.SetActive(!isCharacter1Active);

        Debug.Log($"切换到: {(isCharacter1Active ? "角色1" : "角色2")}");

        isSwitching = false;
    }

    /* 简单冻结：把 Rigidbody 设为 Kinematic，关闭 CharacterController */
    private void FreezeMovement(GameObject go)
    {
        if (go.TryGetComponent(out Rigidbody rb))
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = true;
        }

        if (go.TryGetComponent(out CharacterController cc))
            cc.enabled = false;
    }

    public GameObject GetActiveCharacter()
    {
        return isCharacter1Active ? character1 : character2;
    }
}