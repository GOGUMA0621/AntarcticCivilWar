using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class UnitFollowState : StateMachineBehaviour
{
    private UnitAttackController _attackController;
    private UnitController _controller;
    private NavMeshAgent _agent;

    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        _attackController = animator.GetComponent<UnitAttackController>();
        _agent = animator.GetComponent<NavMeshAgent>();
        _controller = animator.GetComponent<UnitController>();
    }

    
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (_attackController.targetToAttack != null)
        {
            animator.SetBool("isFollow", true);
            Debug.Log(animator.GetBool("isFollow"));
        }

        _agent.SetDestination(_attackController.targetToAttack.transform.position);

        float distanceFromTarget = Vector3.Distance(_attackController.targetToAttack.transform.position, animator.transform.position);
        if (distanceFromTarget < _controller.unitData.UnitAttackDistance)
        {
            animator.SetBool("isAttack", true);
        }
    }

    
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        _agent.SetDestination(animator.transform.position);
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
