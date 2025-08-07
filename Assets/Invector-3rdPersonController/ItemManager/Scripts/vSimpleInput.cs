using Invector;
using Invector.vCharacterController;
using UnityEngine;
using UnityEngine.Events;

[vClassHeader("Simple Input", openClose = false)]
public class vSimpleInput : vMonoBehaviour
{
    [Tooltip("Input to press")]
    public GenericInput input = new GenericInput("Escape", "B", "B");
    [Tooltip("This Gameobject will turn off after the input is pressed")]
    public bool disableThisObjectAfterInput = false;
    public UnityEvent OnPressInput;

    void Update()
    {
        if (input.GetButtonDown() && gameObject.activeSelf)
        {
            if (disableThisObjectAfterInput)
            {
                this.gameObject.SetActive(false);
            }

            OnPressInput.Invoke();
        }
    }
}
