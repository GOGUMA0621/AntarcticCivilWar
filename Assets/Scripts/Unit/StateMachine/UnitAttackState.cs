using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class UnitAttackState : StateMachineBehaviour
{
    private UnitDetectTarget _detect;
    private UnitController _unitController;

    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        _unitController = animator.GetComponent<UnitController>();
        _detect = animator.GetComponentInChildren<UnitDetectTarget>();
        animator.SetBool("isFollow", false);
    }


    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (_detect.targetToAttack == null)
        {
            animator.SetBool("isIdle", true);
        }
        if(animator.GetCurrentAnimatorStateInfo(0).length < animator.GetCurrentAnimatorStateInfo(0).normalizedTime)
        {
            animator.SetBool("isFollow", true);
        }
    }



    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        
    }
}
