using System.Collections;
using UnityEngine;

public class RushBossIdleState : IUnitState
{
    private BossController boss;

    public void Enter(UnitController unitController)
    {
        boss = unitController as BossController;
        boss.StartMovement();
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
    private Transform targetTransform;
    public void Enter(UnitController unitController)
    {
        boss = unitController as BossController;
        if (boss.unit.detectTarget.targetToAttack is Component comp)
            targetTransform = comp.transform;
        boss.SetTargetToMove(targetTransform);
    }
    public void Update()
    {
        var target = boss.unit.detectTarget.targetToAttack;
        if (target == null || target.IsDestroyed())
        {
            boss.GoIdle();
            return;
        }
        float distance = Vector3.Distance(boss.transform.position, targetTransform.position);
        if (distance <= boss.UnitStats.attackRange)
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
    private Transform targetTransform;

    public void Enter(UnitController unitController)
    {
        this.boss = unitController as BossController;
        if (boss.unit.detectTarget.targetToAttack is Component comp)
            targetTransform = comp.transform;
        boss.StopMovement();

        boss.SetAnimation("AttackState");
    }

    public void Update()
    {
        var animator = boss.unit.animator;
        var target = boss.unit.detectTarget.targetToAttack;

        // �ִϸ��̼� 1 ���� ����Ǹ� ���� ���·�
        if (animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1f)
        {
            boss.GoIdle();
            return;
        }

        // Ÿ���� null�̰ų� �׾����� ���� �ʱ�ȭ
        if (target == null || target.IsDestroyed())
        {
            boss.unit.detectTarget.SortClosestTarget();
            target = boss.unit.detectTarget.targetToAttack;

            if (target != null)
                boss.SetTargetToMove(targetTransform);

            boss.GoIdle();
            return;
        }

        // Ÿ�ٰ��� �Ÿ� üũ
        float distance = Vector3.Distance(boss.transform.position, targetTransform.position);

        // �Ÿ��� ���� �������� �ָ� ���� ���·� ��ȯ
        if (boss.RemainedDistance > boss.UnitStats.attackRange)
        {
            boss.GoFollow();
            return;
        }

        // ������ �̵� ���� ���� ����
        boss.StopMovement();
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
        boss.StopMovement();
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
        boss.StopMovement();
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
        boss.StopMovement();
        boss.SetAnimation("ManaSkill");
        boss.SetTargetToMove(null);
        boss.canMana = false;
    }
    public void Update()
    {
        //Debug.Log(boss.unit.animator.GetCurrentAnimatorStateInfo(0).normalizedTime.ToString());
        if (boss.unit.animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1.0f)
        {
            Debug.Log("���� ��ų ���");
            boss.GoIdle();
        }
    }
    public void Exit()
    {
        boss.StartMovement();
        boss.canMana = true;
        boss.SetCurrentMana(0);
    }
} 

public class RushBossDogFightState : IUnitState
{
    private BossController boss;
    public void Enter(UnitController unitController)
    {
        this.boss = unitController as BossController;
        boss.StopMovement();
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
        boss.StartMovement();
    }
}
