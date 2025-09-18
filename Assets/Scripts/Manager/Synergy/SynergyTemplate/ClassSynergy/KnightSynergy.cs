using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[SynergyTag("Knight", SynergyType.ClassType)]
public class KnightSynergy : MonoBehaviour, ISynergy, ISynergyGlobal
{
    public string Tag => "Knight";
    public string Name => "기사";
    public bool allowDuplicate => true;
    public string synergyDescription => "";
    public Sprite synergyIcon => Resources.Load<Sprite>($"Synergy/{Name}");
    public int currentTier => lastTier;
    public int[] tierThresholds => new int[] { 3, 5, 8, 10 };

    private UnitController unit;

    private int lastTier = -1;

    private KnightFirstAttackEffect firstAttackEffect;

    private static readonly SynergyTierEffect[] KnightTierEffects = new SynergyTierEffect[]
    {
        new SynergyTierEffect{ RequiredCount = 3, Description = "체력 100 증가, 첫 공격 추가피해 30", StatModifiers = new() { { StatType.MaxHealth, 100f }, { StatType.AdditionalDamage, 30f } } },
        new SynergyTierEffect{ RequiredCount = 5, Description = "체력 150 증가, 첫 공격 추가피해 45+(추가 공격력 10%)", StatModifiers = new() { { StatType.MaxHealth, 150f }, { StatType.AdditionalDamage, 45f } } },
        new SynergyTierEffect{ RequiredCount = 8, Description = "체력 200 증가, 첫 공격 추가피해 65+(추가 공격력 10%)", StatModifiers = new() { { StatType.MaxHealth, 200f }, { StatType.AdditionalDamage, 65f } } },
        new SynergyTierEffect{ RequiredCount = 10, Description = "체력 300 증가, 첫 공격 추가피해 80+(추가 공격력 15%)", StatModifiers = new() { { StatType.MaxHealth, 300f }, { StatType.AdditionalDamage, 80f } } },
    };

    private static readonly float[] globalMaxHealth = { 100f, 150f, 200f, 300f }; // 모든 유닛체력
    private static readonly int[] firstAttackBaseDamage = { 30, 45, 65, 80 };
    private static readonly float[] firstAttackBonusPercent = { 0.0f, 0.10f, 0.10f, 0.15f };

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

        unit.RemoveModifierStats(Tag);

        // 기존 첫 공격 효과 제거
        if (firstAttackEffect != null)
        {
            unit.UnregisterOnHitEffect(firstAttackEffect);
            firstAttackEffect = null;
        }

        if (tier < 0)
        {
            lastTier = -1;
            return;
        }

        lastTier = tier;

        // 체력 증가 적용
        foreach (var kvp in KnightTierEffects[tier].StatModifiers)
        {
            unit.AddModifierStat(new StatModifier(Tag, kvp.Key, kvp.Value, ModifierMethod.Additive));
        }

        // 첫 공격 추가피해 효과 등록
        firstAttackEffect = new KnightFirstAttackEffect(tier, unit);
        unit.RegisterOnHitEffect(firstAttackEffect);
    }

    // 매 라운드 시작 시 첫 공격 효과 초기화
    public void OnRoundStart()
    {
        if (firstAttackEffect != null)
            firstAttackEffect.ResetUsed();
    }

    // 첫 공격 추가피해 효과 클래스
    private class KnightFirstAttackEffect : OnHitItem
    {
        private readonly int tier;
        private readonly UnitController owner;
        private bool used = false;

        public KnightFirstAttackEffect(int tier, UnitController owner)
        {
            this.tier = tier;
            this.owner = owner;
        }

        public override void OnHit(UnitController attacker, IDamageAble target)
        {
            if (used) return;
            if (attacker != owner) return;

            // target이 UnitController라면 추가피해 적용
            if (target is UnitController targetUnit && tag == "Enemy")
            {
                float bonus = firstAttackBaseDamage[tier];
                if (firstAttackBonusPercent[tier] > 0)
                    bonus += attacker.unitDamage * firstAttackBonusPercent[tier];

                // targetUnit.ReceiveDamage를 통해 추가피해 적용
                targetUnit.ReceiveDamage(new DamageData(bonus, StatusEffectType.None, 0));
                used = true;
            }
        }

        public void ResetUsed()
        {
            used = false;
        }

        public override void ApplyEffect(UnitController unit) { }
        public override void UpdateEffect(UnitController unit) { }
    }

    public void ApplyToGlobal(int count)
    {
        int tier = GetTier(count);

        foreach (var u in GetAllUnits())
        {
            u.RemoveModifierStats(Tag);

            if (tier >= 0)
            {
                u.AddModifierStat(new StatModifier(Tag + "_Global", StatType.MaxHealth, globalMaxHealth[tier], ModifierMethod.Additive));
            }
        }
    }
    private IEnumerable<UnitController> GetAllUnits()
    {
        return FindObjectsOfType<UnitController>();
        //return UnitManager.instance.GetAllUnits();
    }
}
