using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitIdleState : StateMachineBehaviour
{
    UnitAttackController _attackController;

    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        _attackController = animator.GetComponent<UnitAttackController>();
    }
    
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if(_attackController.targetToAttack != null)
        {
            animator.SetBool("isFollow", true);
            Debug.Log(animator.GetBool("isFollow"));
        }
    }

    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {

    }
}
