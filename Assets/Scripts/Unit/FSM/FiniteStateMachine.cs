using System.Linq;
using AYellowpaper.SerializedCollections.Editor.Data;
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
    private UnitGridDragController dragController;

    public void Enter(UnitController unitController)
    {
        this.unitController = unitController;
        dragController = unitController.GetComponent<UnitGridDragController>();
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
        unitController.ReUnit();
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
           unitController.unit.detectTarget.SortClosetTarget();
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
            unitController.SetTargetToMove(target);
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
        
        if(unitController.RemainedDistance <= unitController.unitAttackDistance)
        {
            
            unitController.GoAttack();
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
        Debug.Log(unitController.RemainedDistance);
    }
    public void Update()
    {
        var target = unitController.unit.detectTarget.targetToAttack;

        attackCooldown -= Time.deltaTime;

        if (target == null || IsTargetDead(target))
        {
            unitController.unit.detectTarget.SortClosetTarget();
            var newTarget = unitController.unit.detectTarget.targetToAttack;

            if (newTarget != null)
            {
                unitController.SetTargetToMove(newTarget);
                unitController.GoIdle();
            }
            else
            {
                unitController.GoIdle();
            }
            return;
        }
        else if (unitController.RemainedDistance > unitController.unitAttackDistance)
        {
            unitController.GoIdle();
            return;
        }
        else if (attackCooldown <= 0f)
        {
            if (target != null && !IsTargetDead(target))
            {
                animator.ResetTrigger("attack");
                animator.SetTrigger("attack");
                attackCooldown = 1f / unitController.unitAttackSpeed;
                isAttacking = true;
            }
        }
        else if (isAttacking && animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1f && animator.GetCurrentAnimatorStateInfo(0).IsName("AttackState"))
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
    private void SetMultiflyAnimation(string animationName, int multiflyThreshold)
    {
        
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
        unit.unitSkill.DoActiveSkill();
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