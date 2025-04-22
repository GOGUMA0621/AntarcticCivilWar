using System.Linq;
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
        unitController.unit.aiPath.canMove = true;
        unitController.SetAnimation("IdleState");
        //Debug.Log($"[IdleState] {unitController.name} is idle");

        if (unitController.tag == "Unit" && unitController.unit.detectTarget.targetToAttack == null)
        {
            unitController.SetTargetToMove(unitController.unit.playerController.transform);
        }
    }
    public void Update()
    {
        if (unitController.unit.detectTarget.targets.Any())
        {
            unitController.unit.detectTarget.SortClosetTarget();
            var newTarget = unitController.unit.detectTarget.targetToAttack;
            if (newTarget != null)
            {
                //Debug.Log($"New Target {newTarget.name}");
                unitController.GoFollow();  
            }
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
            unitController.ToggleAITrue(); // aiPath.canMove = true;
            unitController.SetMoveSpeed(unitController.unitSpeed); // 속도 재설정
            //Debug.Log($"[FollowState] Set target: {target.name}");
        }
        else
        {
            Debug.LogWarning("[FollowState] targetToAttack is null");
        }
    }

    public void Update()
    {
        var targets = unitController.unit.detectTarget.targets;
        var target = unitController.unit.detectTarget.targetToAttack;

        if (!targets.Any())
        {
            Debug.LogWarning("[FollowState] No targets available");
            unitController.GoIdle();
            return;
        }

        float distance = Vector3.Distance(unitController.transform.position, target.position);
        if (distance <= unitController.unitAttackDistance)
        {
            Debug.Log($"[FollowState] {unitController.name} distance too close");
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
    public void Enter(UnitController unit)
    {
        //Debug.Log("AttackState");
        this.unitController = unit;
        unit.SetAnimation("AttackState");
        //Debug.Log($"AttackState {unit.name}");
    }
    public void Update()
    {
        var target = unitController.unit.detectTarget.targetToAttack;
        var animator = unitController.unit.animator;

        if (animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1f)
        {
            Debug.Log($"[AttackState] {unitController.name} attack finished");
            unitController.GoIdle();
            return;
        }

        if (target == null || IsTargetDead(target))
        {
            unitController.unit.detectTarget.SortClosetTarget();
            var newTarget = unitController.unit.detectTarget.targetToAttack;

            if (newTarget != null)
            {
                Debug.Log($"[AttackState] New target: {newTarget.name}");
                unitController.SetTargetToMove(newTarget);
                unitController.GoFollow();
            }
            else
            {
                Debug.LogWarning("[AttackState] targetToAttack is null");
                unitController.GoIdle();
            }
            return;
        }
        else
        {
            float distance = Vector3.Distance(unitController.transform.position, target.position);
            if (distance > unitController.unitAttackDistance)
            {
                Debug.Log($"[AttackState] {unitController.name} distance too far");
                unitController.GoFollow();
                return;
            }
        }
        unitController.unit.aiPath.canMove = false;
    }

    public void Exit()
    {
    }
    private bool IsTargetDead(Transform target)
    {
        return target.TryGetComponent<IDamageAble>(out var dmg) && dmg.IsDestroyed();
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
        if (InputManager.instance.GetRevivePressed() && unit.tag != "Unit")
        {
            Debug.Log("ReviveState");
            unit.Revive();
        }
    }
    public void Exit()
    {

    }
}

public class UnitManaSkillState : IUnitState
{
    private UnitController unit;
    public void Enter(UnitController unit)
    {
        this.unit = unit;
        unit.canMana = false;
        unit.SetMoveWork(false);
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
        unit.currentMP = 0;
        unit.canMana = true;
        unit.SetMoveWork(true);
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
        unit.unit.settler.target = unit.unit.playerController.transform;
        unit.SetMoveWork(true);
    }
    public void Update()
    {
        
    }
    public void Exit()
    {
        if (unit.unit.detectTarget.targetToAttack == null)
        {
            unit.unit.settler.target = null;
        }
    }
}