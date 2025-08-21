using UnityEngine;
using System.Collections;

public class CharacterSwitcher : MonoBehaviour
{
    [Header("��ɫ����")]
    public GameObject character1;
    public GameObject character2;

    [Header("�л�����")]
    public KeyCode switchKey = KeyCode.Tab;

    [Header("��ǰ״̬")]
    public bool isCharacter1Active = true;

    [Header("��ɫ2ר�����")]
    public Camera camera2;

    [Header("�л��ӳ�")]
    public float switchDelay = 0.5f;   // �ȴ�ʱ��

    private bool isSwitching = false;    // ���ڵȴ��л�

    private void Start()
    {
        if (character1 == null || character2 == null)
        {
            Debug.LogError("����Inspector��ָ��������ɫ��GameObject��");
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

    /* ���ⲿ�ű����õĽӿ�ͬ���ӳ� */
    public void SwitchCharacter()
    {
        if (character1 == null || character2 == null || isSwitching)
            return;

        isSwitching = true;

        /* �������ᵱǰ��ɫ����ֹ�����ƶ� */
        FreezeMovement(GetActiveCharacter());

        /* �ӳ������л� */
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

    /* 0.5 ��������л� */
    private IEnumerator DelayedSwitch(bool? targetState = null)
    {
        yield return new WaitForSeconds(switchDelay);

        bool nextState = targetState ?? !isCharacter1Active;

        isCharacter1Active = nextState;
        character1.SetActive(isCharacter1Active);
        character2.SetActive(!isCharacter1Active);

        if (camera2 != null)
            camera2.gameObject.SetActive(!isCharacter1Active);

        Debug.Log($"�л���: {(isCharacter1Active ? "��ɫ1" : "��ɫ2")}");

        isSwitching = false;
    }

    /* �򵥶��᣺�� Rigidbody ��Ϊ Kinematic���ر� CharacterController */
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