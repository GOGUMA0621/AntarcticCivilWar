using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

public class UnitIdleState : StateMachineBehaviour
{
    Unit _unit;

    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        _unit = animator.GetComponent<Unit>();

        //Debug.Log("Idle");
    }
    
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if(_unit.detectTarget.targetToAttack != null)
        {
            animator.SetBool("isFollow", true);
            animator.SetBool("isIdle", false);
            //Debug.Log(animator.GetBool("isFollow"));
        }
        animator.SetFloat("speed", _unit.agent.velocity.magnitude);

        if (animator.CompareTag("Unit"))
        {
            _unit.controller.MoveTo(_unit.player.playerPos);
        }
    }

    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {

    }
}
