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
    }
    public void Update()
    {
        //Debug.Log("IdleState");
        if (boss.unit.detectTarget.targetToAttack != null)
        {
            boss.GoFollow();
        }
        else if (boss.unit.detectTarget.targetToAttack == null)
        {
            boss.GoIdle();
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
            boss.GoIdle();
            return;
        }
        float distance = Vector3.Distance(boss.transform.position, target.position);
        if (distance <= boss.unitAttackDistance)
        {
            boss.GoAttack();
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
        boss.StopMovement();

        boss.SetAnimation("AttackState");
    }

    public void Update()
    {
        var animator = boss.unit.animator;
        var target = boss.unit.detectTarget.targetToAttack;

        // 애니메이션 1 루프 종료되면 다음 상태로
        if (animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1f)
        {
            boss.GoIdle();
            return;
        }

        // 타겟이 null이거나 죽었으면 상태 초기화
        if (target == null || target.TryGetComponent<IDamageAble>(out var damageable) && damageable.IsDestroyed())
        {
            boss.unit.detectTarget.SortClosetTarget();
            target = boss.unit.detectTarget.targetToAttack;

            if (target != null)
                boss.SetTargetToMove(target);

            boss.GoIdle();
            return;
        }

        // 타겟과의 거리 체크
        float distance = Vector3.Distance(boss.transform.position, target.position);

        // 거리가 공격 범위보다 멀면 추적 상태로 전환
        if (distance > boss.unitAttackDistance)
        {
            boss.GoFollow();
            return;
        }

        // 가까우면 이동 멈춤 상태 유지
        boss.unit.aiPath.canMove = false;
    }

    public void Exit()
    {
        
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
    }

    public void Exit()
    {
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
            boss.GoIdle();
        }


    }
    public void Exit()
    {

    }

    //public void DoMethod()
    //{
    //    switch
    //}
}

public class RushBossManaSkillState : IUnitState
{
    private BossController boss;
    public void Enter(UnitController unitController)
    {
        this.boss = unitController as BossController;
        boss.SetMoveWork(false);
        boss.SetAnimation("ManaSkill");
        boss.SetTargetToMove(null);
        boss.canMana = false;
    }
    public void Update()
    {
        //Debug.Log(boss.unit.animator.GetCurrentAnimatorStateInfo(0).normalizedTime.ToString());
        if (boss.unit.animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1.0f)
        {
            Debug.Log("보스 스킬 사용");
            boss.GoIdle();
        }
    }
    public void Exit()
    {
        boss.SetMoveWork(true);
        boss.canMana = true;
        boss.currentMP = 0;
    }
} 

public class RushBossDogFightState : IUnitState
{
    private BossController boss;
    public void Enter(UnitController unitController)
    {
        this.boss = unitController as BossController;
        boss.SetMoveWork(false);
        boss.SetAnimation("DogFight");
        boss.SetTargetToMove(null);
    }
    public void Update()
    {
        boss.SetTargetToMove(null);
        if (boss.unit.animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1.0f)
        {
            boss.GoIdle();
        }
    }
    public void Exit()
    {
        boss.SetMoveWork(true);
    }
}
