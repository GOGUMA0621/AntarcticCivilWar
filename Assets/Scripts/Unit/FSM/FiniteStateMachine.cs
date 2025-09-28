using System.Linq;
using UnityEngine;

public interface IUnitState
{
    void Enter(UnitController boss);
    void Update();
    void Exit();
}

public class UnitPlaceState : IUnitState
{
    private UnitController unitController;
    private UnitDragController dragController;

    public void Enter(UnitController unitController)
    {
        this.unitController = unitController;
        dragController = unitController.GetComponent<UnitDragController>();
        dragController.canDrag = true;
        unitController.SetAnimation("IdleState");
        unitController.StopMovement();
    }

    public void Update()
    {
        
    }
    public void Exit()
    {
        dragController.canDrag = false;
    }
}

public class UnitIdleState : IUnitState
{
    private UnitController unitController;

    public void Enter(UnitController unitController)
    {
        this.unitController = unitController;
        unitController.StartMovement();
        unitController.SetAnimation("IdleState");
    }

    public void Update()
    {
        if (unitController.unit.detectTarget.targets.Any())
        {
           unitController.unit.detectTarget.SortClosestTarget();
           var newTarget = unitController.unit.detectTarget.targetToAttack;
           if (newTarget != null)
           {
               unitController.GoFollow();  
           }
        }else
        {
            unitController.StopMovement();
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
        var target = unitController.unit.detectTarget.targetToAttack;

        if (target != null)
        {
            unitController.SetTargetToMove(target.GetTransform(), unitController.OnPathCompleteToAttack);
            unitController.StartMovement();
        }
    }

    public void Update()
    {
        var targets = unitController.unit.detectTarget.targets;

        if (!targets.Any())
        {
            unitController.GoIdle();
            return;
        }

    }

    public void Exit()
    {
    }

}

public class UnitAttackState : IUnitState
{
    private UnitController unitController;
    private Animator animator;
    private float attackCooldown = 0f;
    private bool isAttacking = false;

    public void Enter(UnitController unit)
    {
        this.unitController = unit;
        animator = unit.unit.animator;
        unitController.StopMovement();
        attackCooldown = 0f;
    }

    public void Update()
    {
        attackCooldown -= Time.deltaTime;

        // 목표가 없거나 죽었으면 추적 상태로
        if (unitController.unit.detectTarget.targetToAttack == null || IsTargetDead(unitController.unit.detectTarget.targetToAttack.GetTransform()))
        {
            unitController.unit.detectTarget.SortClosestTarget();
            var target = unitController.unit.detectTarget.targetToAttack;

            if (target != null)
            {
                unitController.SetTargetToMove(target.GetTransform(), unitController.OnPathCompleteToAttack);
                unitController.GoIdle();
            }
            else
            {
                unitController.GoIdle();
            }
            return;
        }
        // 목표가 사거리 밖으로 벗어나면 추적 상태로
        else if (unitController.RemainedDistance > unitController.UnitStats.attackRange)
        {
            unitController.SetTargetToMove(unitController.unit.detectTarget.targetToAttack.GetTransform(), unitController.OnPathCompleteToAttack);
            unitController.GoFollow();
            return;
        }
        // 공격 쿨타임이 끝나면 공격
        else if (attackCooldown <= 0f)
        {
            if (unitController.unit.detectTarget.targetToAttack != null && !IsTargetDead(unitController.unit.detectTarget.targetToAttack.GetTransform()))
            {
                animator.ResetTrigger("attack");
                animator.SetTrigger("attack");
                attackCooldown = 1f / unitController.unitAttackSpeed;
                isAttacking = true;
            }
        }
        // 공격 애니메이션 끝나면 Idle로
        else if (isAttacking && animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1f
                && animator.GetCurrentAnimatorStateInfo(0).IsName("AttackState"))
        {
            animator.Play("IdleState");
            isAttacking = false;
        }
    }

    public void Exit()
    {
        unitController.unit.mover.SetCanMove(true);
    }

    private bool IsTargetDead(Transform target)
    {
        return target.TryGetComponent<IDamageAble>(out var dmg) && dmg.IsDestroyed();
    }
}

public class UnitDieState : IUnitState
{
    private UnitController unit;
    private Animator animator;
    public void Enter(UnitController unit)
    {
        this.unit = unit;
        animator = unit.unit.animator;
        unit.unit.rb.simulated = false; // Rigidbody2D 비활성화
        unit.StopMovement();
        unit.SetAnimation("DieState");
    }
    public void Update()
    {
        if (animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1f)
        {
            unit.gameObject.SetActive(false); // 애니메이션이 끝나면 오브젝트 비활성화
        }
    }
    public void Exit()
    {
        unit.unit.rb.simulated = true; // Rigidbody2D 활성화
        unit.unit.detectTarget.ClearTarget(); // 타겟 초기화
    }
}

public class UnitManaSkillState : IUnitState
{
    private UnitController unit;
    public void Enter(UnitController unit)
    {
        this.unit = unit;
        unit.canMana = false;
        unit.StopMovement();
        unit.SetAnimation("ManaSkillState");
    }
    public void Update()
    {
        AnimatorStateInfo state = unit.unit.animator.GetCurrentAnimatorStateInfo(0);
        if (state.IsName("ManaSkillState") && state.normalizedTime >= 1f)
        {
            unit.GoIdle();
        }
    }
    public void Exit()
    {
        unit.SetCurrentMana(0);
        unit.canMana = true;
        unit.StartMovement();
    }
}

public class UnitCallState : IUnitState
{
    private UnitController unit;
    public void Enter(UnitController unit)
    {
        this.unit = unit;
        unit.SetAnimation("IdleState");
        unit.StopMovement();
        unit.StartMovement();
    }
    public void Update()
    {
        
    }
    public void Exit()
    {
        if (unit.unit.detectTarget.targetToAttack == null)
        {

        }
    }
}