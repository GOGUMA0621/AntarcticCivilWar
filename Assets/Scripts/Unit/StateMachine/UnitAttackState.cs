using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public sealed class UnitAttackState : StateMachineBehaviour
{
    private Unit unit;

    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        unit = animator.GetComponent<Unit>();
        animator.SetBool("isFollow", false);
    }


    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (unit.unitController.isStunned && !unit.unitController.isUnitDie)
        {
            animator.CrossFade("IdleState", 1);
            return;
        }
        else if (unit.unitDetectTarget.targetToAttack == null)
        {
            animator.SetBool("isIdle", true);
        }
        else
        {
            float distanceFromTarget = Vector3.Distance(unit.unitDetectTarget.targetToAttack.transform.position, animator.transform.position);
            if (distanceFromTarget <= unit.data.UnitAttackDistance)
            {
                unit.unitAgent.velocity = Vector2.zero;
            }
            if (animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1f)
            {
                animator.SetBool("isFollow", true);
            }
        }


    }


    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        
    }
}
