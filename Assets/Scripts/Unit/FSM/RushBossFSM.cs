using System.Collections;
using UnityEngine;

public class RushBossIdleState : IUnitState
{
    private BossController boss;

    public void Enter(UnitController unitController)
    {
        boss = unitController as BossController;
        boss.SetMoveWork(true);
        boss.SetAnimation("IdleState");
        boss.SetTargetToMove(boss.unit.playerController.transform);
    }
    public void Update()
    {
        if (boss.unit.detectTarget.targetToAttack != null)
        {
            boss.ChangeState(new RushBossFollowState());
        }
        else if (boss.unit.detectTarget.targetToAttack == null)
        {
            boss.ChangeState(new RushBossIdleState());
        }
    }
    public void Exit()
    {
    }
}

public class RushBossFollowState : IUnitState
{
    private BossController boss;
    public void Enter(UnitController unitController)
    {
        boss = unitController as BossController;
        boss.SetTargetToMove(boss.unit.detectTarget.targetToAttack);
    }
    public void Update()
    {
        var target = boss.unit.detectTarget.targetToAttack;
        if (target == null || target.TryGetComponent<IDamageAble>(out var damageable) && damageable.IsDestroyed())
        {
            boss.ChangeState(new RushBossIdleState());
            return;
        }
        float distance = Vector3.Distance(boss.transform.position, target.position);
        if (distance < 1.0f)
        {
            boss.ChangeState(new RushBossAttackState());
        }
    }

    public void Exit()
    {
    }
}

public class RushBossAttackState : IUnitState
{
    private BossController boss;

    public void Enter(UnitController unitController)
    {
        this.boss = unitController as BossController;
    }

    public void Exit()
    {
        throw new System.NotImplementedException();
    }

    public void Update()
    {
        throw new System.NotImplementedException();
    }
}

public class RushBossDieState : IUnitState
{
    private BossController boss;
    public void Enter(UnitController unitController)
    {
        this.boss = unitController as BossController;
        boss.SetMoveWork(false);
        boss.SetAnimation("DieState");
        boss.SetTargetToMove(null);
    }
    public void Update()
    {
        throw new System.NotImplementedException();
    }

    public void Exit()
    {
        throw new System.NotImplementedException();
    }
}

public class RushBossBattlefieldCrusherState : IUnitState
{
    private BossController boss;
    public void Enter(UnitController unitController)
    {
        this.boss = unitController as BossController;
        boss.SetMoveWork(false);
        boss.SetAnimation("BattleFieldCrush");
        boss.SetTargetToMove(null);
    }
    public void Update()
    {
       if(boss.unit.animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1.0f)
        {
            boss.ChangeState(new RushBossIdleState());
        }
    }
    public void Exit()
    {

    }
}
