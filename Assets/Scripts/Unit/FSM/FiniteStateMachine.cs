using UnityEngine;

public interface IUnitState
{
    void Enter(UnitController boss);
    void Update();
    void Exit();
}

public class UnitIdleState : IUnitState
{
    private UnitController unitController;
    public void Enter(UnitController unitController)
    {
        this.unitController = unitController;
        unitController.SetMoveWork(true);
        unitController.SetAnimation("IdleState");
        if(unitController.tag == "Unit")
        {
            unitController.SetTargetToMove(unitController.unit.playerController.transform);
        }
    }
    public void Update()
    {
        if (unitController.unit.detectTarget.targetToAttack != null)
        {
            unitController.ChangeState(new UnitFollowState());
        }
        else if (unitController.unit.detectTarget.targetToAttack == null)
        {
            unitController.ChangeState(new UnitIdleState());
        }
    }
    public void Exit()
    {

    }
}

public class UnitFollowState : IUnitState
{
    private UnitController unitController;
    public void Enter(UnitController unitController)
    {
        this.unitController = unitController;
        Debug.Log("FollowState");
        unitController.SetTargetToMove(unitController.unit.detectTarget.targetToAttack);
    }

    public void Update()
    {
        var target = unitController.unit.detectTarget.targetToAttack;

        if (target == null || target.TryGetComponent<IDamageAble>(out var damageable) && damageable.IsDestroyed())
        {
            unitController.ChangeState(new UnitIdleState());
            return;
        }

        float distance = Vector3.Distance(unitController.transform.position, target.position);
        if (distance <= unitController.unitAttackDistance)
        {
            unitController.SetMoveWork(false);
            unitController.ChangeState(new UnitAttackState());
        }
    }

    public void Exit()
    {

    }
}

public class UnitAttackState : IUnitState
{
    private UnitController unitController;
    public void Enter(UnitController unit)
    {
        Debug.Log("AttackState");
        this.unitController = unit;
        unit.SetMoveWork(false);
        unit.SetAnimation("AttackState");
    }
    public void Update()
    {
        AnimatorStateInfo state = unitController.unit.animator.GetCurrentAnimatorStateInfo(0);
        if (state.IsName("AttackState") && state.normalizedTime >= 1f)
        {
            unitController.ChangeState(new UnitIdleState());
        }

        var target = unitController.unit.detectTarget.targetToAttack;

        if (target == null || target.TryGetComponent<IDamageAble>(out var damageable) && damageable.IsDestroyed())
        {
            unitController.ChangeState(new UnitIdleState());
            unitController.unit.detectTarget.AttackClosestTarget();
            target = unitController.unit.detectTarget.targetToAttack;
            unitController.SetTargetToMove(target);
        }
        else
        {
            float distance = Vector3.Distance(unitController.transform.position, target.position);
            if (distance > unitController.unitAttackDistance)
            {
                unitController.ChangeState(new UnitFollowState());
            }
        }
    }

    public void Exit()
    {
    }
}

public class UnitDieState : IUnitState
{
    private UnitController unit;
    public void Enter(UnitController unit)
    {
        this.unit = unit;
        unit.SetMoveWork(false);
        unit.SetAnimation("DieState");
    }
    public void Update()
    {
        if (InputManager.instance.GetRevivePressed())
        {
            unit.Revive();
        }
    }
    public void Exit()
    {

    }
}