using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class UnitAttackState : StateMachineBehaviour
{
    private UnitDetectTarget _detect;
    private UnitController _unitController;

    private float _attackRate = 0.0f;
    private float _attackTime = 0.0f;

    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        _unitController = animator.GetComponent<UnitController>();
        _detect = animator.GetComponentInChildren<UnitDetectTarget>();
        animator.SetBool("isFollow", false);

        _attackTime = _unitController.unitAttackSpeed;
        _attackRate = _unitController.unitAttackSpeed;
        //Debug.Log("공격");


    }


    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if(_detect.targetToAttack != null)
        {      
            _attackTime += Time.deltaTime;
            if (_attackRate < _attackTime)
            {
                _attackTime = 0.0f;
                var unitInflictedDamage = _unitController.unitDamage;
                _detect.targetToAttack.GetComponent<UnitController>().ReceiveDamage(unitInflictedDamage, _unitController);
            }
        }
        if (_detect.targetToAttack == null)
        {
            animator.SetBool("isIdle", true);
        }
    }


    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        //Debug.Log("공격 끝남");
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
