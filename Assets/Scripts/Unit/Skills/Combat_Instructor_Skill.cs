using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Combat_Instructor_Skill : MonoBehaviour, IActiveSkill, IPasseiveSkillAttack
{
    // 스킬 설정
    private readonly float[] damagePercent = { 1.5f, 1.7f, 2.0f };
    private readonly float pengForcePercent = 0.5f;
    private readonly float[] stunSeconds = { 2f, 2f, 4f };
    private readonly float[] buffSeconds = { 3f, 4f, 5f };
    private readonly float attackSpeedBuff = 0.3f;
    private readonly float skillRange = 3f;

    // 패시브 설정
    private readonly float hpUp = 30f;
    private readonly float atkUp = 5f;
    private readonly float pengUp = 4f;
    private readonly float atkSpeedUp = 0.03f;
    private readonly float manaRegenUp = 1f;

    public int skillLevel = 1; // 1~3

    bool IActiveSkill.IsDurationSkill => false;
    bool IActiveSkill.IsStandingSkill => false;
    float IActiveSkill.Duration => 0f;

    void IActiveSkill.ActivateSkill(UnitController unit)
    {
        // 주변 적 찾기
        Collider[] hitColliders = Physics.OverlapSphere(unit.transform.position, skillRange, LayerMask.GetMask("Enemy"));
        foreach (var hit in hitColliders)
        {
            var enemy = hit.GetComponent<UnitController>();
            if (enemy != null && !enemy.isAllay)
            {
                float damage = unit.GetFinalStat(StatType.AttackDamage) * damagePercent[skillLevel - 1]
                             + unit.GetFinalStat(StatType.Pengforce) * pengForcePercent;
                DamageData dmg = new DamageData(damage, StatusEffectType.Physical, 0f);
                enemy.ReceiveDamage(dmg);
                enemy.ApplyStun(stunSeconds[skillLevel - 1]);
            }
        }
        // 아군 공격속도 버프
        unit.StartCoroutine(BuffAlliesAttackSpeed());
    }

    private IEnumerator BuffAlliesAttackSpeed()
    {
        var allies = FindObjectsOfType<UnitController>();
        string buffId = "CombatInstructorAtkSpeed";
        foreach (var ally in allies)
        {
            if (ally.isAllay)
            {
                var mod = new StatModifier(buffId, StatType.AttackSpeed, attackSpeedBuff, ModifierMethod.AdditivePercent);
                ally.AddModifierStat(mod);
            }
        }
        yield return new WaitForSeconds(buffSeconds[skillLevel - 1]);
        foreach (var ally in allies)
        {
            if (ally.isAllay)
            {
                ally.RemoveModifierStats(buffId);
            }
        }
    }

    // 패시브: 전투 종료 후 능력치 강화
    public void DoPassiveSkill()
    {
        var allies = FindObjectsOfType<UnitController>();
        int statType = Random.Range(0, 5);
        foreach (var ally in allies)
        {
            if (ally.isAllay)
            {
                switch (statType)
                {
                    case 0:
                        ally.AddModifierStat(new StatModifier("CombatInstructorPassive", StatType.MaxHealth, hpUp, ModifierMethod.Additive));
                        break;
                    case 1:
                        ally.AddModifierStat(new StatModifier("CombatInstructorPassive", StatType.AttackDamage, atkUp, ModifierMethod.Additive));
                        break;
                    case 2:
                        ally.AddModifierStat(new StatModifier("CombatInstructorPassive", StatType.Pengforce, pengUp, ModifierMethod.Additive));
                        break;
                    case 3:
                        ally.AddModifierStat(new StatModifier("CombatInstructorPassive", StatType.AttackSpeed, atkSpeedUp, ModifierMethod.AdditivePercent));
                        break;
                    case 4:
                        ally.AddModifierStat(new StatModifier("CombatInstructorPassive", StatType.ManaRegen, manaRegenUp, ModifierMethod.Additive));
                        break;
                }
            }
        }
    }

    public bool PassiveCondition()
    {
        return true;
    }

    void IActiveSkill.DeactivateSkill(UnitController unit)
    {
    }
}
