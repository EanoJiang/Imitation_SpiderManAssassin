using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class vUseRootMotion : StateMachineBehaviour
{
    // OnStateMove is called before OnStateMove is called on any state inside this state machine
    override public void OnStateMove(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        animator.ApplyBuiltinRootMotion();
    }
}
