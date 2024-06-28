using UnityEngine;

public class ExitBallMode : StateMachineBehaviour
{
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        animator.SetBool("ballMode", false);
    }
}
