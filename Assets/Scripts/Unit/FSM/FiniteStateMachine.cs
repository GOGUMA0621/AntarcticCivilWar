using UnityEngine;

public interface IUnitState
{
    void Enter(UnitController unit);
    void Update();
    void Exit();
}

public class UnitIdleState : IUnitState
{
    private UnitController unit;
    public void Enter(UnitController unit)
    {
        this.unit = unit;
        unit.SetMoveWork(true);
        unit.SetAnimation("IdleState");
        if(unit.tag == "Unit")
        {
            unit.SetTargetToMove(unit.playerController.transform);
        }
    }
    public void Update()
    {
        if (unit.unitDetectTarget.targetToAttack != null)
        {
            unit.ChangeState(new UnitFollowState());
        }
        else if (unit.unitDetectTarget.targetToAttack == null)
        {
            unit.ChangeState(new UnitIdleState());
        }
    }
    public void Exit()
    {

    }
}

public class UnitFollowState : IUnitState
{
    private UnitController unit;
    public void Enter(UnitController unit)
    {
        this.unit = unit;
        Debug.Log("FollowState");
        unit.SetTargetToMove(unit.unitDetectTarget.targetToAttack);
    }

    public void Update()
    {
        var target = unit.unitDetectTarget.targetToAttack;

        if (target == null || target.TryGetComponent<IDamageAble>(out var damageable) && damageable.IsDestroyed())
        {
            unit.ChangeState(new UnitIdleState());
            return;
        }

        float distance = Vector3.Distance(unit.transform.position, target.position);
        if (distance <= unit.unitAttackDistance)
        {
            unit.SetMoveWork(false);
            unit.ChangeState(new UnitAttackState());
        }
    }

    public void Exit()
    {

    }
}

public class UnitAttackState : IUnitState
{
    private UnitController unit;
    public void Enter(UnitController unit)
    {
        Debug.Log("AttackState");
        this.unit = unit;
        unit.SetMoveWork(false);
        unit.SetAnimation("AttackState");
    }
    public void Update()
    {
        AnimatorStateInfo state = unit.unitAnimator.GetCurrentAnimatorStateInfo(0);
        if (state.IsName("AttackState") && state.normalizedTime >= 1f)
        {
            unit.ChangeState(new UnitIdleState());
        }

        var target = unit.unitDetectTarget.targetToAttack;

        if (target == null || target.TryGetComponent<IDamageAble>(out var damageable) && damageable.IsDestroyed())
        {
            unit.ChangeState(new UnitIdleState());
            unit.unitDetectTarget.AttackClosestTarget();
            target = unit.unitDetectTarget.targetToAttack;
            unit.SetTargetToMove(target);
        }
        else
        {
            float distance = Vector3.Distance(unit.transform.position, target.position);
            if (distance > unit.unitAttackDistance)
            {
                unit.ChangeState(new UnitFollowState());
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
        
    }
    public void Exit()
    {

    }
}