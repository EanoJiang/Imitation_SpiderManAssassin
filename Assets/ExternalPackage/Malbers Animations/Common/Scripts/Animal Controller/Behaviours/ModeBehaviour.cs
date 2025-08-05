using MalbersAnimations.Scriptables;
using System.Collections;
using UnityEngine;

namespace MalbersAnimations.Controller
{
    public class ModeBehaviour : StateMachineBehaviour
    {
        public ModeID ModeID;
        private MAnimal animal;
        private Mode modeOwner;
        private int AbilityIndex;

        [Tooltip("Calls 'Animation Tag Enter' on the Modes")]
        public bool EnterMode = true;
        [Tooltip("Calls 'Animation Tag Exit' on the Modes")]
        public bool ExitMode = true;

        override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            animal = animator.GetComponent<MAnimal>();

            if (animal.IntID == Int_ID.Loop) return; //Means is Looping

            if (ModeID == null) Debug.LogError("Mode behaviour needs an ID");
               
            modeOwner = animal.Mode_Get(ModeID);

            AbilityIndex =  modeOwner.ActiveAbility.Index;

            if (modeOwner != null && EnterMode)
            {
                modeOwner.AnimationTagEnter(stateInfo.tagHash);
                //Debug.Log("Enterstate");
            }
        }

        // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
        override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            if (animal.IntID == Int_ID.Loop) return; //Means is Looping

            if (modeOwner != null && ExitMode)
            {
                modeOwner.AnimationTagExit(AbilityIndex);
             //   Debug.Log("Exit State");►◄
            }
        }

        public override void OnStateMove(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            if (modeOwner != null) modeOwner.OnModeStateMove(stateInfo,animator,layerIndex);
        }
    }
}