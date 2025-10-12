using System.Linq;
using UnityEngine;

// 그리드 기반 거리 계산 헬퍼 (UnitController의 astar를 사용)
internal static class GridRangeHelper
{
    public static int GetGridDistance(UnitController unitController, Transform other)
    {
        if (unitController == null || other == null) return int.MaxValue;
        var astar = unitController.GetAstarGrid() ?? Object.FindObjectOfType<AstarPathfinder>();
        if (astar == null) return int.MaxValue;
        // GridRangeUtility는 이전에 추가한 static 유틸 사용
        return GridRangeUtility.GridDistance(astar, unitController.transform.position, other.position, GridDistanceMetric.Chebyshev);
    }
}

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
        Debug.Log($"UnitPlaceState Enter - {unitController.name}");
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
                int rangeCells = Mathf.CeilToInt(unitController.UnitStats.attackRange);
                int gridDist = GridRangeHelper.GetGridDistance(unitController, newTarget.GetTransform());

                if (gridDist == int.MaxValue)
                {
                    // 폴백: 월드 거리
                    float dist = Vector2.Distance(unitController.transform.position, newTarget.GetTransform().position);
                    if (dist <= unitController.UnitStats.attackRange) unitController.GoAttack();
                    else unitController.GoFollow();
                }
                else
                {
                    if (gridDist <= rangeCells) unitController.GoAttack();
                    else unitController.GoFollow();
                }
            }
        }
        else
        {
            unitController.StopMovement();
        }
    }
    public void Exit() { }
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

        // 목표가 존재하면 실시간으로 거리 체크하여 사거리 이내이면 공격 또는 스킬로 전환
        var target = unitController.unit.detectTarget.targetToAttack;
        if (target != null)
        {
            var tPos = target.GetTransform().position;

            int rangeCells = Mathf.CeilToInt(unitController.UnitStats.attackRange);
            int gridDist = GridRangeHelper.GetGridDistance(unitController, target.GetTransform());

            if (gridDist == int.MaxValue)
            {
                // 폴백: 월드 거리 기준
                float dist = Vector2.Distance(unitController.transform.position, tPos);
                if (dist <= unitController.UnitStats.attackRange)
                {
                    unitController.StopMovement();
                    Debug.Log($"[FollowState] (world) Reached attack range ({dist:F2}) -> Try skill or attack");
                    // 스킬 가능하면 스킬로, 아니면 공격으로
                    if (unitController.unit != null && unitController.isSkillActive)
                    {
                        unitController.GoSkill();
                    }
                    else
                    {
                        unitController.GoAttack();
                    }
                }
            }
            else
            {
                if (gridDist <= rangeCells)
                {
                    unitController.StopMovement();
                    Debug.Log($"[FollowState] (grid) Reached attack range (cells: {gridDist}) -> Try skill or attack");
                    if (unitController.unit != null && unitController.isSkillActive)
                    {
                        unitController.GoSkill();
                    }
                    else
                    {
                        unitController.GoAttack();
                    }
                }
            }
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

        // 목표가 사거리 밖으로 벗어나면 추적 상태로 (월드 거리 기반)
        var currentTarget = unitController.unit.detectTarget.targetToAttack;
        if (currentTarget != null)
        {
            int rangeCells = Mathf.CeilToInt(unitController.UnitStats.attackRange);
            int gridDist = GridRangeHelper.GetGridDistance(unitController, currentTarget.GetTransform());

            if (gridDist == int.MaxValue)
            {
                // 폴백: 월드 거리
                float dist = Vector2.Distance(unitController.transform.position, currentTarget.GetTransform().position);
                if (dist > unitController.UnitStats.attackRange)
                {
                    unitController.GoFollow();
                    return;
                }
            }
            else
            {
                if (gridDist > rangeCells)
                {
                    unitController.GoFollow();
                    return;
                }
            }
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
        if (isAttacking && animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1f
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
        unit.unit.detectTarget.ClearTarget(); // 타겟 초기화
    }
}

public class UnitManaSkillState : IUnitState
{
    private UnitController unit;
    private bool skillStarted = false;

    public void Enter(UnitController unit)
    {
        this.unit = unit;
        skillStarted = false;

        // 이동/정지 등 필요시 추가
        unit.StopMovement();
        unit.canMana = false;
        skillStarted = true;
    }

    public void Update()
    {
        if (unit.unit.animator.GetCurrentAnimatorStateInfo(0).IsName("ManaSkillState") && !skillStarted)
        {
            unit.SetAnimation("ManaSkillState");
            skillStarted = true;
        }
    }

    public void Exit()
    {

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