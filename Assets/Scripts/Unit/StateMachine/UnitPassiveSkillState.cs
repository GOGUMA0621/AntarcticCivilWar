using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitPassiveSkillState : StateMachineBehaviour
{
    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1f)
        {
            animator.Play("FollowState");
        }
    }
}
