using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

public class UnitIdleState : StateMachineBehaviour
{
    UnitDetectTarget _detect;
    UnitController _controller;
    NavMeshAgent _agent;

    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        _controller = animator.GetComponent<UnitController>();
        _agent = animator.GetComponent<NavMeshAgent>();
        _detect = _controller.detectTarget;

        //Debug.Log("Idle");
    }
    
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if(_detect.targetToAttack != null)
        {
            animator.SetBool("isFollow", true);
            animator.SetBool("isIdle", false);
            //Debug.Log(animator.GetBool("isFollow"));
        }
        animator.SetFloat("speed", _agent.velocity.magnitude);

        if (animator.CompareTag("Unit"))
        {
            _controller.MoveTo(_controller.PlayerController.playerPos);
        }
    }

    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {

    }
}
