using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class UnitFollowState : StateMachineBehaviour
{
    private UnitDetectTarget _detect;
    private UnitController _controller;
    private NavMeshAgent _agent;
    float distanceFromTarget = 0.0f;
    float distanceToTarget = 0.0f;

    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        _detect = animator.GetComponentInChildren<UnitDetectTarget>();
        _agent = animator.GetComponent<NavMeshAgent>();
        _controller = animator.GetComponent<UnitController>();

        //Debug.Log("추적");
    }

    
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (_detect.targetToAttack != null)
        {
            animator.SetBool("isFollow", true);
            animator.SetBool("isIdle", false);
            _agent.SetDestination(_detect.targetToAttack.transform.position);
            //Debug.Log(animator.GetBool("isFollow"));
            distanceFromTarget = Vector3.Distance(_detect.targetToAttack.transform.position, animator.transform.position);
            distanceToTarget = Mathf.Abs(_detect.targetToAttack.position.y - animator.transform.position.y);

        }
        else
        {
            animator.SetBool("isFollow", false);
            animator.SetBool("isIdle", true);
        }

        
        
        if (distanceFromTarget <= _controller.unitAttackDistance)
        {
            animator.SetTrigger("attack");
            //Debug.Log("어택");
        }
    }


    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        animator.ResetTrigger("attack");
        _agent.SetDestination(animator.transform.position);
        //Debug.Log("추적 끝남");
    }

    // OnStateMove is called right after Animator.OnAnimatorMove()
    //override public void OnStateMove(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    // Implement code that processes and affects root motion
    //}

    // OnStateIK is called right after Animator.OnAnimatorIK()
    //override public void OnStateIK(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    // Implement code that sets up animation IK (inverse kinematics)
    //}
}
