using System.Collections;
using UnityEngine;

public class TomahawkSkill : MonoBehaviour, IActiveSkill
{
    // 레벨별 수치
    private readonly float[] attackSpeedBonus = { 0.4f, 0.4f, 0.5f };
    private readonly float[] lifeStealBonus = { 0.3f, 0.4f, 0.5f };
    public bool IsDurationSkill => true;
    public bool IsStandingSkill => false;
    public float Duration => 4f;

    private string modifierSourceId = "TomahawkSkill_CombatStance";
    private Coroutine skillCoroutine;

    public void ActivateSkill(UnitController unit)
    {
        if (unit.isSkillActive) return;
        unit.isSkillActive = true;

        int levelIdx = Mathf.Clamp(unit.unitLevel - 1, 0, attackSpeedBonus.Length - 1);

        // 공격속도 버프
        var attackSpeedMod = new StatModifier(
            modifierSourceId,
            StatType.AttackSpeed,
            attackSpeedBonus[levelIdx],
            ModifierMethod.AdditivePercent
        );
        // 생명력 흡수 버프
        var lifeStealMod = new StatModifier(
            modifierSourceId,
            StatType.LifeSteal,
            lifeStealBonus[levelIdx],
            ModifierMethod.Additive
        );

        unit.AddModifierStat(attackSpeedMod);
        unit.AddModifierStat(lifeStealMod);

        unit.GoSkill(IsStandingSkill, Duration);

        skillCoroutine = unit.StartCoroutine(CombatStanceRoutine(unit));
    }

    private IEnumerator CombatStanceRoutine(UnitController unit)
    {
        yield return new WaitForSeconds(Duration);
        DeactivateSkill(unit);
    }

    public void DeactivateSkill(UnitController unit)
    {
        unit.RemoveModifierStats(modifierSourceId);
        unit.isSkillActive = false;
    }
}
