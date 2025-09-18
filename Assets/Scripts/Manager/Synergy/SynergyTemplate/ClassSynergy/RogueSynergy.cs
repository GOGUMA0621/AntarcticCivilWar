using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[SynergyTag("Rogue", SynergyType.ClassType)]
public class RogueSynergy : MonoBehaviour, ISynergy, ISynergyGlobal
{
    public string Tag => "Rogue";
    public string Name => "도적";
    public bool allowDuplicate => true;
    public string synergyDescription => "";
    public Sprite synergyIcon => Resources.Load<Sprite>($"Synergy/{Name}");
    public int currentTier => lastTier;
    public int[] tierThresholds => new int[] { 3, 5, 7 };

    private UnitController unit;
    private int lastTier = -1;
    private RogueBonusDamageEffect bonusEffect;

    private static readonly SynergyTierEffect[] RogueTierEffects = new SynergyTierEffect[]
    {
        new SynergyTierEffect { RequiredCount = 3, Description = "공격력 10증가, 10% 추가 피해", StatModifiers = new() { { StatType.AttackDamage, 10f }, { StatType.AdditionalDamage, 0.10f } } },
        new SynergyTierEffect { RequiredCount = 5, Description = "공격력 15증가, 15% 추가 피해", StatModifiers = new() { { StatType.AttackDamage, 15f }, { StatType.AdditionalDamage, 0.15f } } },
        new SynergyTierEffect { RequiredCount = 7, Description = "공격력 20증가, 20% 추가 피해", StatModifiers = new() { { StatType.AttackDamage, 20f }, { StatType.AdditionalDamage, 0.20f } } },
    };

    private static readonly float[] globalAttackDamage = { 10f, 15f, 20f }; // 모든 유닛

    public void Initialize(UnitController unit)
    {
        this.unit = unit;
    }

    private int GetTier(int count)
    {
        int tier = -1;
        for (int i = 0; i < tierThresholds.Length; i++)
        {
            if (count >= tierThresholds[i])
                tier = i;
        }
        return tier;
    }

    public void OnCountUpdate(int count)
    {
        int tier = GetTier(count);

        // 기존 효과 제거
        if (bonusEffect != null)
        {
            unit.UnregisterOnHitEffect(bonusEffect);
            bonusEffect = null;
        }

        unit.RemoveModifierStats(Tag);

        if (tier < 0)
        {
            lastTier = -1;
            return;
        }

        lastTier = tier;

        // 공격력 증가 적용
        if (RogueTierEffects[tier].StatModifiers.TryGetValue(StatType.AttackDamage, out float attackBonus))
        {
            unit.AddModifierStat(new StatModifier(Tag, StatType.AttackDamage, attackBonus, ModifierMethod.Additive));
        }

        // 매 공격시 추가 피해 효과 등록
        float addPercent = RogueTierEffects[tier].StatModifiers.TryGetValue(StatType.AdditionalDamage, out float add) ? add : 0.1f;
        bonusEffect = new RogueBonusDamageEffect(unit, addPercent);
        unit.RegisterOnHitEffect(bonusEffect);
    }

    // 매 공격시 추가 피해 효과 클래스
    private class RogueBonusDamageEffect : OnHitItem
    {
        private readonly UnitController owner;
        private readonly float percent;

        public RogueBonusDamageEffect(UnitController owner, float percent)
        {
            this.owner = owner;
            this.percent = percent;
        }

        public override void OnHit(UnitController attacker, IDamageAble target)
        {
            if (attacker != owner) return;

            // target이 UnitController라면 추가피해 적용
            if (target is UnitController targetUnit && tag == "Enemy")
            {
                float bonus = 0f;

                bonus = attacker.unitDamage * percent;

                // 추가피해 적용
                targetUnit.ReceiveDamage(new DamageData(bonus, StatusEffectType.None, 0));
            }
        }

        public override void ApplyEffect(UnitController unit){}
        public override void UpdateEffect(UnitController unit){}
    }

    public void ApplyToGlobal(int count)
    {
        int tier = GetTier(count);

        foreach (var u in GetAllUnits())
        {
            u.RemoveModifierStats(Tag + "_Global");

            if (tier >= 0)
            {
                float attackBonus = globalAttackDamage[tier];
                u.AddModifierStat(new StatModifier(Tag + "_Global", StatType.AttackDamage, attackBonus, ModifierMethod.Additive));
            }
        }
    }

    private IEnumerable<UnitController> GetAllUnits()
    {
        return FindObjectsOfType<UnitController>();
    }
}
