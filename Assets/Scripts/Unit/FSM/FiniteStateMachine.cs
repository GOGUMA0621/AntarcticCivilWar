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
                int rangeCells = Mathf.RoundToInt(unitController.UnitStats.attackRange);
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
            // 타겟으로 이동 경로 설정 및 즉시 이동 시작
            unitController.SetTargetToMove(target.GetTransform(), unitController.OnPathCompleteToAttack);
            unitController.StartMovement();
            Debug.Log($"[FollowState] Enter: Moving to target {target.GetTransform().name}");
        }
        else
        {
            // 타겟이 없으면 Idle 상태로
            unitController.GoIdle();
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

        // 타겟 재정렬 후 새로운 타겟이 있는지 확인
        unitController.unit.detectTarget.SortClosestTarget();
        var target = unitController.unit.detectTarget.targetToAttack;
        if (target != null)
        {
            var tPos = target.GetTransform().position;

            int rangeCells = Mathf.RoundToInt(unitController.UnitStats.attackRange);
            int gridDist = GridRangeHelper.GetGridDistance(unitController, target.GetTransform());

            if (gridDist == int.MaxValue)
            {
                // 폴백: 월드 거리 기준
                float dist = Vector2.Distance(unitController.transform.position, tPos);
                if (dist <= unitController.UnitStats.attackRange)
                {
                    unitController.StopMovement();
                    unitController.LookAtTarget(); // 타겟을 바라보도록 설정
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
                    unitController.LookAtTarget(); // 타겟을 바라보도록 설정
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
        unitController.SetAnimation("AttackState");
        attackCooldown = 0f;
    }

    public void Update()
    {
        attackCooldown -= Time.deltaTime;

        // 목표가 없거나 죽었으면 새로운 타겟 찾기
        if (unitController.unit.detectTarget.targetToAttack == null || IsTargetDead(unitController.unit.detectTarget.targetToAttack.GetTransform()))
        {
            unitController.unit.detectTarget.SortClosestTarget();
            var target = unitController.unit.detectTarget.targetToAttack;

            if (target != null)
            {
                // 새로운 타겟이 있으면 바로 Follow 상태로 전환
                unitController.GoFollow();
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
            // 공격 사거리 계산 (근접 유닛은 좀 더 관대하게)
            float attackRange = unitController.UnitStats.attackRange;
            bool isMelee = unitController.unit.data.unitAttackType == UnitAttackType.Melee;
            
            int rangeCells = Mathf.RoundToInt(attackRange);
            int gridDist = GridRangeHelper.GetGridDistance(unitController, currentTarget.GetTransform());

            if (gridDist == int.MaxValue)
            {
                // 폴백: 월드 거리
                float dist = Vector2.Distance(unitController.transform.position, currentTarget.GetTransform().position);
                float maxAllowedDistance = isMelee ? attackRange + 0.5f : attackRange; // 근접은 여유 거리 추가
                
                if (dist > maxAllowedDistance)
                {
                    unitController.GoFollow();
                    return;
                }
            }
            else
            {
                int maxAllowedCells = isMelee ? rangeCells + 1 : rangeCells; // 근접은 1칸 여유
                if (gridDist > maxAllowedCells)
                {
                    unitController.GoFollow();
                    return;
                }
            }
        }
        // 공격 애니메이션 끝나면 Idle로
        if (isAttacking && animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1f
                && animator.GetCurrentAnimatorStateInfo(0).IsName("AttackState"))
        {
            animator.Play("IdleState");
            isAttacking = false;
        }

        // 공격 쿨타임이 끝나면 공격
        else if (attackCooldown <= 0f)
        {
            if (unitController.unit.detectTarget.targetToAttack != null && !IsTargetDead(unitController.unit.detectTarget.targetToAttack.GetTransform()))
            {
                // 공격 전에 타겟을 바라보도록 설정
                unitController.LookAtTarget();
                
                // 애니메이션 트리거 설정 (애니메이션에서만 공격 실행)
                animator.ResetTrigger("attack");
                animator.SetTrigger("attack");
                attackCooldown = 1f / unitController.unitAttackSpeed;
                isAttacking = true;
                
                Debug.Log($"{unitController.name}: 공격 애니메이션 시작 - 애니메이션 이벤트에서 공격 실행됨");
            }
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
    private Animator animator;
    private bool finished = false;

    public void Enter(UnitController unit)
    {
        this.unit = unit;
        animator = unit.unit?.animator;
        finished = false;

        // 스킬 시작 시 이동 중지 및 스킬 사용 플래그 처리
        unit.StopMovement();
        unit.canMana = false;

        // 스킬 애니메이션 트리거 (UnitController에 맞는 호출로 조정)
        unit.SetAnimation("ManaSkillState");
    }

    public void Update()
    {
        if (finished) return;
        if (animator == null) return;

        var state = animator.GetCurrentAnimatorStateInfo(0);
        // 스킬 애니메이션 플레이 중이고 끝났으면 정리
        if (state.IsName("ManaSkillState") && state.normalizedTime >= 1f)
        {
            finished = true;

            // 스킬 끝난 뒤 복구: 이동 재개, canMana 복구, 상태 복귀
            unit.canMana = true;

            // mover/이동 플래그 복구 (UnitController 내부 API에 맞게 조정)
            try
            {
                unit.unit.mover.SetCanMove(true);
            }
            catch { /* 안전하게 무시 */ }

            // 기본적으로 Idle로 전환. 원하면 이전 상태로 복귀하도록 수정 가능
            unit.GoIdle();
        }
    }

    public void Exit()
    {
        // 안전 보장: Exit 시에도 복구 처리
        if (unit != null)
        {
            unit.canMana = true;
            try { unit.unit.mover.SetCanMove(true); } catch { }
        }
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