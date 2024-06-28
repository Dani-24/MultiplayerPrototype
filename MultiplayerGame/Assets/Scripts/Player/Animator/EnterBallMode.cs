using UnityEngine;

public class EnterBallMode : StateMachineBehaviour
{
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (stateInfo.normalizedTime >= 1.0f)
        {
            animator.SetBool("ballMode", true);
        }
    }
}
