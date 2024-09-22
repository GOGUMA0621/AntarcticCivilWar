using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitDieState : StateMachineBehaviour
{
    private bool _canRevive = false;
    private UnitController _controller;
    
    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        _controller = animator.GetComponent<UnitController>();
        if (!animator.CompareTag("Unit"))
        {
            _canRevive = true;
            Debug.Log(_canRevive);
        }
    }

    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (_canRevive)
        {
            if (Input.GetMouseButtonDown(0))
            {
                _controller.Revive();
            }
        }
    }
}
