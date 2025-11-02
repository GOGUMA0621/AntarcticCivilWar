using System.Collections;
using UnityEngine;

public class Low_Knight_Skill : MonoBehaviour, IActiveSkill
{
    // 레벨별 피해 계수
    private readonly float[] damageMultiplier = { 1.4f, 1.4f, 1.7f };
    private readonly float stunDuration = 2f;

    public bool IsDurationSkill => false;
    public bool IsStandingSkill => true;
    public float Duration => 0f;

    public void ActivateSkill(UnitController unit)
    {
        if (unit.isSkillActive) return;
        unit.isSkillActive = true;

        // 타겟 가져오기 (예시: 현재 공격 대상)
        var target = unit.unit.detectTarget.targetToAttack as UnitController;
        if (target == null || target.IsDestroyed())
        {
            DeactivateSkill(unit);
            return;
        }

        int levelIdx = Mathf.Clamp(unit.unitLevel - 1, 0, damageMultiplier.Length - 1);
        float baseDamage = unit.UnitStats.attackDamage;
        float skillDamage = baseDamage * damageMultiplier[levelIdx];

        // 피해 적용
        DamageData damageData = new DamageData(skillDamage, StatusEffectType.Physical, 0);
        target.ReceiveDamage(damageData);

        // 기절 효과 적용 (UnitController의 ApplyStun 만들어둠(임시))
        target.ApplyStun(stunDuration);

        DeactivateSkill(unit);
    }

    public void DeactivateSkill(UnitController unit)
    {
        unit.isSkillActive = false;
    }
}
