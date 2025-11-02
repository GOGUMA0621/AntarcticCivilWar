using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Royal_Shield_Skill : MonoBehaviour, IActiveSkill
{
    // 스킬 레벨에 따른 공격력 계수
    private readonly float[] damagePercent = { 1.1f, 1.2f, 1.6f };
    private readonly float stunSeconds = 1.5f;

    public int skillLevel = 1;
    public bool IsDurationSkill => false;
    public bool IsStandingSkill => false;
    public float Duration => 0f;

    public void ActivateSkill(UnitController unit)
    {
        UnitController target = FindClosestEnemy(unit);
        if (target == null) return;

        // 적에게 돌진
        StartCoroutine(ChargeAndAttack(unit, target));
    }

    private UnitController FindClosestEnemy(UnitController unit)
    {
        float searchRange = unit.GetFinalStat(StatType.AttackRange);
        Collider[] hitColliders = Physics.OverlapSphere(unit.transform.position, searchRange, LayerMask.GetMask("Enemy"));

        UnitController closestEnemy = null;
        float closestDistance = float.MaxValue;

        foreach (var hitCollider in hitColliders)
        {
            UnitController enemy = hitCollider.GetComponent<UnitController>();
            if (enemy != null && !enemy.isAllay)
            {
                float distance = Vector3.Distance(unit.transform.position, enemy.transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestEnemy = enemy;
                }
            }
        }
        return closestEnemy;
    }

    private IEnumerator ChargeAndAttack(UnitController unit, UnitController target)
    {
        Vector3 chargePosition = target.transform.position;

        unit.SetTargetToMove(target.transform);

        // 목표에 도달할 때까지 대기 (또는 일정 시간 동안)
        while (Vector3.Distance(unit.transform.position, chargePosition) > 1.5f) // 충돌 반경 고려
        {
            yield return null;
        }

        unit.StopMovement();

        // 피해량 계산
        float damage = unit.GetFinalStat(StatType.AttackDamage) * damagePercent[skillLevel - 1];
        DamageData dmg = new DamageData(damage, StatusEffectType.Physical, 0f);

        // 피해 및 기절 효과 적용
        target.ReceiveDamage(dmg);
        target.ApplyStun(stunSeconds);
    }
    public void DeactivateSkill(UnitController unit)
    {
    }
}
