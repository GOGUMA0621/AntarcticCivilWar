using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class UnitFollowState : StateMachineBehaviour
{
    private UnitDetectTarget detect;
    private UnitController controller;
    private NavMeshAgent agent;
    float distanceFromTarget = 0.0f;
    float distanceToTarget = 0.0f;

    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        detect = animator.GetComponentInChildren<UnitDetectTarget>();
        agent = animator.GetComponent<NavMeshAgent>();
        controller = animator.GetComponent<UnitController>();

        //Debug.Log("추적");
    }

    
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (detect.targetToAttack != null)
        {
            animator.SetBool("isFollow", true);
            animator.SetBool("isIdle", false);
            agent.SetDestination(detect.targetToAttack.transform.position);
            //Debug.Log(animator.GetBool("isFollow"));
            distanceFromTarget = Vector3.Distance(detect.targetToAttack.transform.position, animator.transform.position);
            distanceToTarget = Mathf.Abs(detect.targetToAttack.position.y - animator.transform.position.y);

            if (IsTargetInRange(animator, stateInfo, layerIndex))
            {
                animator.SetTrigger("attack");
            }
        }
        else
        {
            animator.SetBool("isFollow", false);
            animator.SetBool("isIdle", true);
        }

        
    }

    bool IsTargetInRange(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        Collider2D[] targetObjects = Physics2D.OverlapCircleAll(animator.transform.position, controller.unitAttackDistance - 0.5f);
        foreach (Collider2D targetObject in targetObjects)
        {
            if (targetObject.gameObject == detect.targetToAttack.gameObject)
            {
                return true;
            }
        }

        return false;
    }

    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        animator.ResetTrigger("attack");
        //Debug.Log("추적 끝남");
    }
}
